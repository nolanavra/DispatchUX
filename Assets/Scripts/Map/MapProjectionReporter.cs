using DispatchQuest.Managers;
using TMPro;
using UnityEngine;
using DispatchQuest.MapSDK;

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
        [Header("Tile Sync")]
        [SerializeField] private MapController tileController;
        [SerializeField] private RectTransform tileContainer;

        public (double lat, double lon) SouthWest { get; private set; }
        public (double lat, double lon) NorthWest { get; private set; }
        public (double lat, double lon) NorthEast { get; private set; }
        public (double lat, double lon) SouthEast { get; private set; }
        public (double lat, double lon) Center { get; private set; }

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
            Center = dataManager.MapToLatLon((min + max) * 0.5f);

            SyncTileContainer();
            if (tileController != null)
            {
                tileController.SetCenter(Center.lat, Center.lon);
            }

            if (cornerReadout != null)
            {
                cornerReadout.text =
                    $"NW: {FormatLatLon(NorthWest)}\nNE: {FormatLatLon(NorthEast)}\nSW: {FormatLatLon(SouthWest)}\nSE: {FormatLatLon(SouthEast)}";
            }
        }

        private void SyncTileContainer()
        {
            if (tileContainer == null || mapArea == null)
            {
                return;
            }

            tileContainer.anchorMin = Vector2.zero;
            tileContainer.anchorMax = Vector2.one;
            tileContainer.offsetMin = Vector2.zero;
            tileContainer.offsetMax = Vector2.zero;
            tileContainer.SetParent(mapArea, false);
            tileContainer.SetSiblingIndex(0);
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
