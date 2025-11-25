# Cafe Data Pipeline for Erie County, NY

This document captures the JSON schema, Unity-side data model, loading flow, queries, and a minimal preprocessing recipe for OpenStreetMap → Unity.

## JSON schema (final)
```json
{
  "cafes": [
    {
      "id": "node/123456789",
      "name": "Example Cafe",
      "brand": "Starbucks",
      "branch": "Elmwood Avenue",
      "address": "123 Elmwood Ave, Buffalo, NY 14222",
      "city": "Buffalo",
      "postcode": "14222",
      "lat": 42.912345,
      "lon": -78.876543,
      "tags": {
        "addr:housenumber": "123",
        "addr:street": "Elmwood Ave",
        "addr:city": "Buffalo",
        "brand": "Starbucks",
        "amenity": "cafe"
      }
    }
  ]
}
```
- `id`: full OSM id with element type (node/way/relation prefix).
- `lat`/`lon`: required; everything else is optional but recommended.
- `tags`: free-form original OSM tag set preserved as key/value pairs.

## Recommended Unity folder layout
```
Assets/Resources/Data/cafes.json          // runtime data file
Assets/Resources/Data/cafes.schema.json   // optional validation schema
Assets/Scripts/Data/CafeModels.cs         // Cafe + CafeDatabase + SerializableTag
Assets/Scripts/Data/CafeDatabaseLoader.cs // Resources loader
Assets/Scripts/Data/CafeQueries.cs        // query helpers
Assets/Scripts/Data/MiniJSON.cs           // lightweight JSON parser for tags
Assets/Scripts/Geo/LatLonMapper.cs        // lat/lon → XY projection
Assets/Scripts/Map/CafeMapSpawner.cs      // marker spawning example
```

## Unity usage notes
- Place `cafes.json` under `Resources/Data` so `CafeDatabaseLoader` can load it via `Resources.Load("Data/cafes")`.
- Add `CafeDatabaseLoader` to a bootstrap object (or the map scene) so `CafeDatabaseLoader.Database` is populated early.
- The loader uses a local MiniJSON parser to handle the `tags` object without an external dependency.

## Example: plotting cafés on a UI/world map
`CafeMapSpawner` expects a marker prefab and parent container. It projects lat/lon to a flat XY plane relative to an origin (defaults to downtown Buffalo) and instantiates markers either as UI (`RectTransform`) or world-space objects.

## Query capabilities
`CafeQueries` exposes:
- `GetCafesNearLatLon(lat, lon, maxKm)`
- `GetByBrand(brand)`
- `GetByName(namePart)`
- `GetByAddress(text)`
- `GetByCity(city)`
- `GetByPostcode(zip)`
- `SortByDistance(lat, lon)`

## Preprocessing raw Overpass JSON → cafes.json
Example Python snippet (expects Overpass JSON features in `elements`):
```python
import json

with open("overpass_cafes.json", "r", encoding="utf-8") as f:
    raw = json.load(f)

cafes = []
for element in raw.get("elements", []):
    tags = element.get("tags", {})
    if tags.get("amenity") != "cafe":
        continue
    cafes.append({
        "id": f"{element.get('type')}/{element.get('id')}",
        "name": tags.get("name"),
        "brand": tags.get("brand"),
        "branch": tags.get("branch"),
        "address": ", ".join(filter(None, [
            f"{tags.get('addr:housenumber', '')} {tags.get('addr:street', '')}".strip(),
            tags.get("addr:city"),
            tags.get("addr:postcode")
        ])),
        "city": tags.get("addr:city"),
        "postcode": tags.get("addr:postcode"),
        "lat": element.get("lat"),
        "lon": element.get("lon"),
        "tags": tags
    })

with open("cafes.json", "w", encoding="utf-8") as f:
    json.dump({"cafes": cafes}, f, ensure_ascii=False, indent=2)
```
- Pass the resulting `cafes.json` into Unity under `Assets/Resources/Data/`.
- Keep the full `tags` object so future filters (Wi-Fi, power outlets, etc.) remain available without re-scraping.
