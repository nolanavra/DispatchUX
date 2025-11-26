# Minimal Tile-Based Map SDK (Unity 2022+)

This package adds a lightweight Web Mercator tile renderer and marker system that targets orthographic 2D scenes. The stack is provider-agnostic: point it at any XYZ tile server URL template and wire in a quad or sprite prefab for tiles and a prefab for markers.

## Folder layout
```
Assets/Scripts/MapSDK/WebMercatorUtils.cs
Assets/Scripts/MapSDK/MapTileProvider.cs
Assets/Scripts/MapSDK/MapTileCache.cs
Assets/Scripts/MapSDK/MapController.cs
Assets/Scripts/MapSDK/MapMarkerManager.cs
Assets/Scripts/MapSDK/MapExampleController.cs
```

## Scene wiring
1. **Camera**: Set the main camera to Orthographic. Size can be whatever fits your gameplay; tiles are spaced with `worldUnitsPerTile` on `MapController`.
2. **Tile prefab**: Create a simple Quad (3D) or Sprite (2D) prefab. Assign a material that supports a `_MainTex` property (Standard, URP Unlit, etc.). Drop it into `MapController.tilePrefab`.
3. **Map GameObject**: Add `MapTileProvider`, `MapTileCache`, and `MapController` to an empty GameObject. Assign the cache’s provider reference and the controller’s cache reference.
4. **Marker manager**: Add `MapMarkerManager` to another GameObject (or the same). Assign the `mapController` reference and a simple marker prefab (e.g., a sprite or quad with a distinctive material).
5. **Example driver**: Add `MapExampleController` and set `mapController` and `markerManager`. The example seeds the map with a single marker and handles WASD/arrow panning plus scroll-wheel zoom.
6. **Input expectations**: Panning reads `Horizontal`/`Vertical` axes; zoom reads `Input.mouseScrollDelta.y`. Ensure your project’s input settings expose those axes.

## Tile providers
- Default template: `https://tile.openstreetmap.org/{z}/{x}/{y}.png`.
- Swap the template on `MapTileProvider` to use your own server or a cached/local file source (e.g., `http://localhost:8080/tiles/{z}/{x}/{y}.png`).
- Respect provider usage limits and attribution requirements; OSM’s public tiles are for light testing only.

## Workflow notes
- `MapController` treats the center tile as `(0,0)` in world space and arranges a square grid sized by `visibleRadiusInTiles`.
- `MapMarkerManager` converts lat/lon to world space via `MapController.LatLonToWorld`, so markers stay aligned when you pan or zoom. Call `RefreshMarkersPositions` after changing center or zoom.
- You can swap in your own input logic; `MapExampleController` is intentionally bare-bones.
