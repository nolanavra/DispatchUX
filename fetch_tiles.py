"""
Offline tile fetcher for XYZ tile servers.
Downloads tiles for a bounding box and zoom range into z/x/y.png folders
and emits a manifest describing coverage.
"""

import argparse
import json
import math
from pathlib import Path
from typing import Iterable, List, Tuple
from concurrent.futures import ThreadPoolExecutor, as_completed
from urllib import request, error

MIN_LAT = -85.05112878
MAX_LAT = 85.05112878
MIN_LON = -180.0
MAX_LON = 180.0


def clamp(value: float, min_value: float, max_value: float) -> float:
    return max(min_value, min(max_value, value))


def latlon_to_tile_xy(lat_deg: float, lon_deg: float, zoom: int) -> Tuple[int, int]:
    lat_deg = clamp(lat_deg, MIN_LAT, MAX_LAT)
    lon_deg = clamp(lon_deg, MIN_LON, MAX_LON)

    lat_rad = math.radians(lat_deg)
    n = 2.0 ** zoom
    tile_x = int(math.floor((lon_deg + 180.0) / 360.0 * n))
    mercator_y = math.log(math.tan(math.pi / 4.0 + lat_rad / 2.0))
    tile_y = int(math.floor((1.0 - mercator_y / math.pi) / 2.0 * n))
    return tile_x, tile_y


def tiles_for_bbox(min_lat: float, min_lon: float, max_lat: float, max_lon: float, zoom: int) -> Iterable[Tuple[int, int]]:
    corners = [
        latlon_to_tile_xy(min_lat, min_lon, zoom),
        latlon_to_tile_xy(min_lat, max_lon, zoom),
        latlon_to_tile_xy(max_lat, min_lon, zoom),
        latlon_to_tile_xy(max_lat, max_lon, zoom),
    ]
    xs = [c[0] for c in corners]
    ys = [c[1] for c in corners]
    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys), max(ys)

    for x in range(min_x, max_x + 1):
        for y in range(min_y, max_y + 1):
            yield x, y


def build_url(url_template: str, z: int, x: int, y: int) -> str:
    return (
        url_template.replace("{z}", str(z))
        .replace("{x}", str(x))
        .replace("{y}", str(y))
    )


def download_tile(url: str, destination: Path, retry_count: int) -> bool:
    if destination.exists():
        return True

    destination.parent.mkdir(parents=True, exist_ok=True)

    for attempt in range(1, retry_count + 1):
        try:
            with request.urlopen(url) as response:
                data = response.read()
                destination.write_bytes(data)
                return True
        except error.HTTPError as http_error:
            print(f"HTTP error for {url} (attempt {attempt}/{retry_count}): {http_error}")
        except Exception as ex:  # pragma: no cover - broad to keep retries simple
            print(f"Failed to fetch {url} (attempt {attempt}/{retry_count}): {ex}")
    return False


def download_all_tiles(
    min_lat: float,
    min_lon: float,
    max_lat: float,
    max_lon: float,
    min_zoom: int,
    max_zoom: int,
    url_template: str,
    out_dir: Path,
    concurrency: int,
    retry_count: int,
) -> List[Tuple[int, int, int]]:
    planned: List[Tuple[int, int, int]] = []
    for zoom in range(min_zoom, max_zoom + 1):
        for x, y in tiles_for_bbox(min_lat, min_lon, max_lat, max_lon, zoom):
            planned.append((zoom, x, y))

    if not planned:
        print("No tiles to download; check your bounds or zoom levels.")
        return []

    completed: List[Tuple[int, int, int]] = []
    total = len(planned)
    print(f"Downloading {total} tiles with concurrency={concurrency}...")

    def worker(task: Tuple[int, int, int]) -> Tuple[int, int, int, bool]:
        z, x, y = task
        dest = out_dir / str(z) / str(x) / f"{y}.png"
        url = build_url(url_template, z, x, y)
        success = download_tile(url, dest, retry_count)
        return z, x, y, success

    with ThreadPoolExecutor(max_workers=concurrency) as executor:
        futures = {executor.submit(worker, t): t for t in planned}
        for idx, future in enumerate(as_completed(futures), start=1):
            z, x, y, success = future.result()
            if success:
                completed.append((z, x, y))
            else:
                print(f"Warning: tile {z}/{x}/{y} failed after retries.")
            if idx % 50 == 0 or idx == total:
                print(f"Progress: {idx}/{total} ({idx / total * 100:.1f}%)")

    return completed


def write_manifest(
    min_lat: float,
    min_lon: float,
    max_lat: float,
    max_lon: float,
    min_zoom: int,
    max_zoom: int,
    tiles: List[Tuple[int, int, int]],
    out_dir: Path,
) -> None:
    manifest = {
        "minLat": min_lat,
        "minLon": min_lon,
        "maxLat": max_lat,
        "maxLon": max_lon,
        "minZoom": min_zoom,
        "maxZoom": max_zoom,
        "tiles": [
            {"z": z, "x": x, "y": y}
            for z, x, y in sorted(tiles)
        ],
    }
    manifest_path = out_dir / "tiles_manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2))
    print(f"Wrote manifest to {manifest_path}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Download XYZ tiles to disk for offline use.")
    parser.add_argument("--min-lat", type=float, required=True, help="Minimum latitude (south)")
    parser.add_argument("--min-lon", type=float, required=True, help="Minimum longitude (west)")
    parser.add_argument("--max-lat", type=float, required=True, help="Maximum latitude (north)")
    parser.add_argument("--max-lon", type=float, required=True, help="Maximum longitude (east)")
    parser.add_argument("--min-zoom", type=int, required=True, help="Minimum zoom level (inclusive)")
    parser.add_argument("--max-zoom", type=int, required=True, help="Maximum zoom level (inclusive)")
    parser.add_argument("--url-template", type=str, required=True, help="Tile URL template with {z}/{x}/{y}")
    parser.add_argument("--out-dir", type=str, required=True, help="Output directory root")
    parser.add_argument("--concurrency", type=int, default=8, help="Number of concurrent downloads")
    parser.add_argument("--retry-count", type=int, default=3, help="Retries per tile on failure")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    out_dir = Path(args.out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)

    tiles = download_all_tiles(
        min_lat=args.min_lat,
        min_lon=args.min_lon,
        max_lat=args.max_lat,
        max_lon=args.max_lon,
        min_zoom=args.min_zoom,
        max_zoom=args.max_zoom,
        url_template=args.url_template,
        out_dir=out_dir,
        concurrency=max(1, args.concurrency),
        retry_count=max(1, args.retry_count),
    )

    write_manifest(
        min_lat=args.min_lat,
        min_lon=args.min_lon,
        max_lat=args.max_lat,
        max_lon=args.max_lon,
        min_zoom=args.min_zoom,
        max_zoom=args.max_zoom,
        tiles=tiles,
        out_dir=out_dir,
    )


if __name__ == "__main__":
    main()
