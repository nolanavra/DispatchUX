using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Geo;
using UnityEngine;

namespace DispatchQuest.Map
{
    public class CafeMapSpawner : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CafeDatabaseLoader loader;

        [Header("Map Projection")]
        [SerializeField] private double originLatitude = 42.8864; // Buffalo City Hall-ish
        [SerializeField] private double originLongitude = -78.8784;
        [SerializeField] private float mapScale = 0.001f; // Meters to Unity units scaling factor

        [Header("Markers")]
        [SerializeField] private GameObject cafeMarkerPrefab;
        [SerializeField] private Transform markerParent;

        private void Start()
        {
            if (loader == null)
            {
                loader = FindObjectOfType<CafeDatabaseLoader>();
            }

            if (loader == null)
            {
                Debug.LogError("CafeDatabaseLoader was not found in the scene.");
                return;
            }

            var db = CafeDatabaseLoader.Database ?? CafeDatabaseLoader.LoadFromResources();
            if (db == null || db.cafes == null || db.cafes.Count == 0)
            {
                Debug.LogWarning("No cafe data loaded to spawn markers.");
                return;
            }

            foreach (Transform child in markerParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var cafe in db.cafes.OrderBy(c => c.name))
            {
                var localPos = LatLonMapper.ToLocalXY(originLatitude, originLongitude, cafe.lat, cafe.lon) * mapScale;
                var marker = Instantiate(cafeMarkerPrefab, markerParent);
                marker.name = $"CafeMarker_{cafe.name}";
                var rect = marker.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = localPos;
                }
                else
                {
                    marker.transform.localPosition = new Vector3(localPos.x, 0f, localPos.y);
                }

                var label = marker.GetComponentInChildren<UnityEngine.UI.Text>();
                if (label != null)
                {
                    label.text = string.IsNullOrWhiteSpace(cafe.name) ? "Cafe" : cafe.name;
                }
            }
        }
    }
}
