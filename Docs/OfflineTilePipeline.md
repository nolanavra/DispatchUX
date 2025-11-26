# Offline Tile Pipeline

A small pipeline for prefetching raster tiles and consuming them locally in Unity. No runtime networking required, no reliance on public OSM endpoints.

## Python downloader (fetch_tiles.py)
- Fetch tiles from any XYZ server with CLI bounds and zoom options.
- Saves to `outDir/z/x/y.png` and writes `tiles_manifest.json` alongside.
- Skips already-downloaded tiles, retries transient failures, and logs progress.

### Example
```
python fetch_tiles.py \
  --min-lat 42.3 --min-lon -79.2 \
  --max-lat 43.1 --max-lon -78.3 \
  --min-zoom 11 --max-zoom 15 \
  --url-template "https://example.com/tiles/{z}/{x}/{y}.png" \
  --out-dir ./tiles
```

## Unity integration
- `LocalMapTileProvider` loads tiles from `Application.streamingAssetsPath/<tilesRoot>/z/x/y.png`.
- `LocalMapTileCache` keeps textures in memory and can be cleared explicitly.
- `OfflineMapController` instantiates a grid of tiles around a center lat/lon and zoom.

### Scene wiring
1. Create `Assets/StreamingAssets/MapTiles/` and copy the downloaded folders (z/x/y.png) into it.
2. Make a tile prefab: a Quad or Sprite with a simple unlit material.
3. Add an empty GameObject (e.g., `OfflineMapRoot`) and attach `LocalMapTileProvider`, `LocalMapTileCache`, and `OfflineMapController`.
4. Assign references: cache → provider, controller → cache + tile prefab. Tune `worldUnitsPerTile`, `visibleRadiusInTiles`, and the starting center/zoom.

## Licensing reminder
Use tiles from a properly licensed or self-hosted server (e.g., OpenMapTiles, MapTiler). Bulk pulls from the public OpenStreetMap tile servers are not allowed.
