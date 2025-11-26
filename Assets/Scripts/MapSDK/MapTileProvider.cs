using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DispatchQuest.MapSDK
{
    public class MapTileProvider : MonoBehaviour
    {
        [SerializeField]
        private string tileUrlTemplate = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        public async Task<Texture2D> LoadTileAsync(int zoom, int tileX, int tileY)
        {
            string url = BuildTileUrl(zoom, tileX, tileY);
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to load tile {zoom}/{tileX}/{tileY}: {request.error}");
                return null;
            }

            try
            {
                return DownloadHandlerTexture.GetContent(request);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error decoding tile {zoom}/{tileX}/{tileY}: {ex.Message}");
                return null;
            }
        }

        private string BuildTileUrl(int zoom, int tileX, int tileY)
        {
            return tileUrlTemplate
                .Replace("{z}", zoom.ToString())
                .Replace("{x}", tileX.ToString())
                .Replace("{y}", tileY.ToString());
        }
    }
}
