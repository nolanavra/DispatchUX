using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class LocalMapTileProvider : MonoBehaviour
    {
        [Header("Local tile root folder (relative to StreamingAssets)")]
        [SerializeField]
        private string tilesRoot = "MapTiles";

        public async Task<Texture2D> LoadTileAsync(int zoom, int tileX, int tileY)
        {
            string path = Path.Combine(Application.streamingAssetsPath, tilesRoot, zoom.ToString(), tileX.ToString(), $"{tileY}.png");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Local tile not found at {path}");
                return null;
            }

            byte[] data;
            try
            {
                data = await Task.Run(() => File.ReadAllBytes(path));
            }
            catch (IOException ioEx)
            {
                Debug.LogWarning($"Failed reading tile {zoom}/{tileX}/{tileY}: {ioEx.Message}");
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(data))
            {
                Debug.LogWarning($"Failed to decode tile {zoom}/{tileX}/{tileY}");
                Destroy(texture);
                return null;
            }

            texture.name = $"tile_{zoom}_{tileX}_{tileY}";
            return texture;
        }
    }
}
