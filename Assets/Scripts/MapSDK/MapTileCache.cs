using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class MapTileCache : MonoBehaviour
    {
        [SerializeField]
        private MapTileProvider tileProvider;

        private readonly Dictionary<string, Texture2D> memoryCache = new();

        public async Task<Texture2D> GetTileAsync(int zoom, int x, int y)
        {
            string key = BuildKey(zoom, x, y);
            if (memoryCache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            if (tileProvider == null)
            {
                Debug.LogWarning("MapTileCache has no tile provider assigned.");
                return null;
            }

            Texture2D tile = await tileProvider.LoadTileAsync(zoom, x, y);
            if (tile != null)
            {
                memoryCache[key] = tile;
            }

            return tile;
        }

        public void Clear()
        {
            memoryCache.Clear();
        }

        private static string BuildKey(int zoom, int x, int y)
        {
            return $"{zoom}/{x}/{y}";
        }
    }
}
