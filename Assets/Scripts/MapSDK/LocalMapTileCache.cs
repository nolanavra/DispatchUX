using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class LocalMapTileCache : MonoBehaviour
    {
        [SerializeField]
        private LocalMapTileProvider tileProvider;

        private readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public async Task<Texture2D> GetTileAsync(int zoom, int x, int y)
        {
            string key = $"{zoom}/{x}/{y}";
            if (cache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture = await tileProvider.LoadTileAsync(zoom, x, y);
            if (texture != null)
            {
                cache[key] = texture;
            }
            return texture;
        }

        public void Clear()
        {
            foreach (var kvp in cache)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            cache.Clear();
        }
    }
}
