using UnityEngine;

namespace DispatchQuest.Data
{
    public class CafeDatabaseLoader : MonoBehaviour
    {
        private const string DefaultResourcePath = "Data/cafes";
        public static CafeDatabase Database { get; private set; }

        [SerializeField]
        [Tooltip("Resources path (without .json) used to load cafes.")]
        private string resourcePath = DefaultResourcePath;

        private void Awake()
        {
            if (Database == null)
            {
                LoadFromResources(resourcePath);
            }
            else if (Database != null && Database.cafes.Count == 0)
            {
                LoadFromResources(resourcePath);
            }

            // Keep this alive if spawned in a bootstrap scene.
            DontDestroyOnLoad(gameObject);
        }

        public static CafeDatabase LoadFromResources(string path = DefaultResourcePath)
        {
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null)
            {
                Debug.LogError($"Cafe data not found at Resources/{path}.json");
                return null;
            }

            Database = CafeDatabase.FromJson(asset.text);
            return Database;
        }
    }
}
