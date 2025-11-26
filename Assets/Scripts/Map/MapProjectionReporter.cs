using DispatchQuest.Managers;
using TMPro;
using UnityEngine;

namespace DispatchQuest.Map
{
    /// <summary>
    /// Reports map corner lat/lon bounds and provides helpers to convert between
    /// lat/lon and anchored UI positions for the current map panel.
    /// </summary>
    public class MapProjectionReporter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DispatchDataManager dataManager;
        [SerializeField] private RectTransform mapArea;
        [SerializeField] private TMP_Text cornerReadout;

        public (double lat, double lon) SouthWest { get; private set; }
        public (double lat, double lon) NorthWest { get; private set; }
        public (double lat, double lon) NorthEast { get; private set; }
        public (double lat, double lon) SouthEast { get; private set; }

        private void OnEnable()
        {
            if (dataManager != null)
            {
                dataManager.OnDataChanged += ReportCorners;
            }
            ReportCorners();
        }

        private void OnDisable()
        {
            if (dataManager != null)
            {
                dataManager.OnDataChanged -= ReportCorners;
            }
        }

        public void ReportCorners()
        {
            if (dataManager == null || mapArea == null)
            {
                return;
            }

            var min = dataManager.MapMin;
            var max = dataManager.MapMax;

            SouthWest = dataManager.MapToLatLon(new Vector2(min.x, min.y));
            NorthWest = dataManager.MapToLatLon(new Vector2(min.x, max.y));
            NorthEast = dataManager.MapToLatLon(new Vector2(max.x, max.y));
            SouthEast = dataManager.MapToLatLon(new Vector2(max.x, min.y));

            if (cornerReadout != null)
            {
                cornerReadout.text =
                    $"NW: {FormatLatLon(NorthWest)}\nNE: {FormatLatLon(NorthEast)}\nSW: {FormatLatLon(SouthWest)}\nSE: {FormatLatLon(SouthEast)}";
            }
        }

        public Vector2 LatLonToAnchoredPosition(double latitude, double longitude)
        {
            if (dataManager == null || mapArea == null)
            {
                return Vector2.zero;
            }

            var mapPos = dataManager.LatLonToMapPosition(latitude, longitude);
            Vector2 normalized = new(
                Mathf.InverseLerp(dataManager.MapMin.x, dataManager.MapMax.x, mapPos.x),
                Mathf.InverseLerp(dataManager.MapMin.y, dataManager.MapMax.y, mapPos.y));

            Vector2 size = mapArea.rect.size;
            return new Vector2(
                (normalized.x - 0.5f) * size.x,
                (normalized.y - 0.5f) * size.y);
        }

        private string FormatLatLon((double lat, double lon) pair)
        {
            return $"{pair.lat:F5}, {pair.lon:F5}";
        }
    }
}
