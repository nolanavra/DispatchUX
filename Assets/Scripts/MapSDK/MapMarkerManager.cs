using System.Collections.Generic;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class MapMarkerManager : MonoBehaviour
    {
        [SerializeField] private MapController mapController;
        [SerializeField] private GameObject markerPrefab;

        [System.Serializable]
        public class MapMarker
        {
            public string id;
            public double lat;
            public double lon;
            public GameObject instance;
        }

        private readonly Dictionary<string, MapMarker> markers = new();

        public MapMarker AddMarker(string id, double lat, double lon)
        {
            if (markerPrefab == null || mapController == null)
            {
                Debug.LogWarning("MapMarkerManager missing dependencies.");
                return null;
            }

            if (markers.ContainsKey(id))
            {
                RemoveMarker(id);
            }

            GameObject instance = Instantiate(markerPrefab, transform);
            instance.name = $"marker_{id}";
            instance.transform.localPosition = mapController.LatLonToWorld(lat, lon);

            var marker = new MapMarker
            {
                id = id,
                lat = lat,
                lon = lon,
                instance = instance
            };

            markers[id] = marker;
            return marker;
        }

        public void RemoveMarker(string id)
        {
            if (!markers.TryGetValue(id, out MapMarker marker))
            {
                return;
            }

            if (marker.instance != null)
            {
                Destroy(marker.instance);
            }

            markers.Remove(id);
        }

        public void RefreshMarkersPositions()
        {
            if (mapController == null)
            {
                return;
            }

            foreach (var marker in markers.Values)
            {
                if (marker.instance == null)
                {
                    continue;
                }

                marker.instance.transform.localPosition = mapController.LatLonToWorld(marker.lat, marker.lon);
            }
        }
    }
}
