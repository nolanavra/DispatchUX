using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapTileCache tileCache;
        [SerializeField] private int zoom = 13;
        [SerializeField] private double centerLat = 42.8864;
        [SerializeField] private double centerLon = -78.8784;
        [SerializeField] private int visibleRadiusInTiles = 3;
        [SerializeField] private float worldUnitsPerTile = 10f;
        [SerializeField] private GameObject tilePrefab;

        public System.Action ViewChanged;

        private readonly Dictionary<string, GameObject> activeTiles = new();
        private bool isRefreshing;

        public int Zoom => zoom;
        public double CenterLat => centerLat;
        public double CenterLon => centerLon;
        public float WorldUnitsPerTile => worldUnitsPerTile;

        private async void Start()
        {
            await RefreshTilesAsync();
        }

        public async void SetCenter(double lat, double lon)
        {
            centerLat = Mathf.Clamp((float)lat, -85.05112878f, 85.05112878f);
            centerLon = Mathf.Clamp((float)lon, -180f, 180f);
            await RefreshTilesAsync();
        }

        public async void SetZoom(int newZoom)
        {
            newZoom = Mathf.Clamp(newZoom, 3, 18);
            if (newZoom == zoom)
            {
                return;
            }

            zoom = newZoom;
            await RefreshTilesAsync();
        }

        public Vector2 LatLonToWorld(double lat, double lon)
        {
            return WebMercatorUtils.LatLonToWorldPosition(lat, lon, centerLat, centerLon, zoom, worldUnitsPerTile);
        }

        public Vector2 WorldToLatLon(Vector2 worldPos)
        {
            return WebMercatorUtils.WorldPositionToLatLon(worldPos, centerLat, centerLon, zoom, worldUnitsPerTile);
        }

        private async Task RefreshTilesAsync()
        {
            if (isRefreshing || tilePrefab == null || tileCache == null)
            {
                return;
            }

            isRefreshing = true;

            WebMercatorUtils.LatLonToTileXY(centerLat, centerLon, zoom, out int centerTileX, out int centerTileY);
            var neededKeys = new HashSet<string>();

            int n = 1 << zoom;
            for (int dx = -visibleRadiusInTiles; dx <= visibleRadiusInTiles; dx++)
            {
                for (int dy = -visibleRadiusInTiles; dy <= visibleRadiusInTiles; dy++)
                {
                    int tileX = centerTileX + dx;
                    int tileY = centerTileY + dy;
                    if (tileX < 0 || tileY < 0 || tileX >= n || tileY >= n)
                    {
                        continue;
                    }

                    string key = BuildKey(tileX, tileY);
                    neededKeys.Add(key);
                    await UpdateTileAsync(tileX, tileY, dx, dy, key);
                }
            }

            ReapObsoleteTiles(neededKeys);
            isRefreshing = false;
            ViewChanged?.Invoke();
        }

        private async Task UpdateTileAsync(int tileX, int tileY, int dx, int dy, string key)
        {
            if (!activeTiles.TryGetValue(key, out GameObject tileObj))
            {
                tileObj = Instantiate(tilePrefab, transform);
                tileObj.name = $"tile_{zoom}_{tileX}_{tileY}";
                activeTiles[key] = tileObj;
            }

            tileObj.transform.localScale = new Vector3(worldUnitsPerTile, worldUnitsPerTile, 1f);
            Vector3 position = new(dx * worldUnitsPerTile, dy * worldUnitsPerTile, 0f);
            tileObj.transform.localPosition = position;

            Texture2D texture = await tileCache.GetTileAsync(zoom, tileX, tileY);
            if (texture != null)
            {
                ApplyTexture(tileObj, texture);
            }
        }

        private void ApplyTexture(GameObject tileObj, Texture2D texture)
        {
            var spriteRenderer = tileObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
                return;
            }

            var renderer = tileObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetTexture("_MainTex", texture);
                renderer.SetPropertyBlock(block);
            }
        }

        private void ReapObsoleteTiles(HashSet<string> needed)
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in activeTiles)
            {
                if (!needed.Contains(kvp.Key))
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }

                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                activeTiles.Remove(key);
            }
        }

        private string BuildKey(int tileX, int tileY)
        {
            return $"{tileX}/{tileY}";
        }
    }
}
