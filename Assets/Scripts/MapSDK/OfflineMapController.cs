using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class OfflineMapController : MonoBehaviour
    {
        [Header("Tile system")]
        [SerializeField]
        private LocalMapTileCache tileCache;

        [SerializeField]
        private GameObject tilePrefab;

        [SerializeField]
        private float worldUnitsPerTile = 10f;

        [SerializeField]
        private int visibleRadiusInTiles = 3;

        [Header("Geo center")]
        [SerializeField]
        private double centerLat = 42.8864f;

        [SerializeField]
        private double centerLon = -78.8784f;

        [SerializeField]
        private int zoom = 14;

        private readonly Dictionary<string, GameObject> activeTiles = new Dictionary<string, GameObject>();

        private async void Start()
        {
            await RefreshTilesAsync();
        }

        public async Task RefreshTilesAsync()
        {
            if (tileCache == null || tilePrefab == null)
            {
                Debug.LogWarning("OfflineMapController missing cache or tile prefab reference.");
                return;
            }

            WebMercatorUtils.LatLonToTileXY(centerLat, centerLon, zoom, out int centerTileX, out int centerTileY);
            int radius = Mathf.Max(0, visibleRadiusInTiles);

            var desiredKeys = new HashSet<string>();
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int tileX = centerTileX + dx;
                    int tileY = centerTileY + dy;
                    string key = $"{zoom}/{tileX}/{tileY}";
                    desiredKeys.Add(key);

                    Texture2D texture = await tileCache.GetTileAsync(zoom, tileX, tileY);
                    if (texture == null)
                    {
                        continue;
                    }

                    if (!activeTiles.TryGetValue(key, out GameObject tileObj))
                    {
                        tileObj = Instantiate(tilePrefab, transform);
                        tileObj.name = $"tile_{zoom}_{tileX}_{tileY}";
                        tileObj.transform.localScale = new Vector3(worldUnitsPerTile, worldUnitsPerTile, 1f);
                        activeTiles[key] = tileObj;
                    }

                    ApplyTexture(tileObj, texture);
                    Vector3 position = PositionForTile(tileX, tileY, centerTileX, centerTileY);
                    tileObj.transform.localPosition = position;
                }
            }

            CleanupMissingTiles(desiredKeys);
        }

        public async void SetCenter(double lat, double lon)
        {
            centerLat = lat;
            centerLon = lon;
            await RefreshTilesAsync();
        }

        public async void SetZoom(int newZoom)
        {
            zoom = Mathf.Clamp(newZoom, 1, 20);
            await RefreshTilesAsync();
        }

        private Vector3 PositionForTile(int tileX, int tileY, int centerX, int centerY)
        {
            float dx = (tileX - centerX) * worldUnitsPerTile;
            float dy = (centerY - tileY) * worldUnitsPerTile;
            return new Vector3(dx, dy, 0f);
        }

        private void ApplyTexture(GameObject tileObj, Texture2D texture)
        {
            if (tileObj.TryGetComponent(out Renderer renderer))
            {
                if (renderer.material.mainTexture != texture)
                {
                    renderer.material.mainTexture = texture;
                }
            }
            else if (tileObj.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                if (spriteRenderer.sprite == null || spriteRenderer.sprite.texture != texture)
                {
                    var rect = new Rect(0, 0, texture.width, texture.height);
                    spriteRenderer.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), texture.width / worldUnitsPerTile);
                }
            }
            else
            {
                Debug.LogWarning($"Tile prefab {tileObj.name} has no Renderer or SpriteRenderer.");
            }
        }

        private void CleanupMissingTiles(HashSet<string> desiredKeys)
        {
            var toRemove = new List<string>();
            foreach (var kvp in activeTiles)
            {
                if (!desiredKeys.Contains(kvp.Key))
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (string key in toRemove)
            {
                activeTiles.Remove(key);
            }
        }
    }
}
