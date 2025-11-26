using System.Collections.Generic;
using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using DispatchQuest.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.Map
{
    public class MapViewController : MonoBehaviour
    {
        public DispatchDataManager DataManager;
        public RectTransform MapArea;
        public GameObject TechnicianMarkerPrefab;
        public GameObject JobMarkerPrefab;
        public GameObject SiteMarkerPrefab;
        public TechnicianDetailPanelUI TechnicianDetailPanel;
        public JobDetailPanelUI JobDetailPanel;
        public MapProjectionReporter ProjectionReporter;
        [Header("Map Tile Wiring")]
        [SerializeField] private RectTransform tileContainer;

        private readonly List<SiteMarker> _siteMarkers = new();
        private readonly List<TechnicianMarker> _techMarkers = new();
        private readonly List<JobMarker> _jobMarkers = new();

        public IReadOnlyList<TechnicianMarker> TechnicianMarkers => _techMarkers;

        private void Start()
        {
            AttachTileContainer();
            BuildMarkers();
            if (DataManager != null)
            {
                DataManager.OnJobAssigned += HandleJobAssigned;
                DataManager.OnDataChanged += RefreshMarkers;
            }

            ProjectionReporter?.ReportCorners();
        }

        private void OnDestroy()
        {
            if (DataManager != null)
            {
                DataManager.OnJobAssigned -= HandleJobAssigned;
                DataManager.OnDataChanged -= RefreshMarkers;
            }
        }

        private void HandleJobAssigned(JobTicket job, Technician technician)
        {
            RefreshMarkers();
        }

        public void BuildMarkers()
        {
            if (DataManager == null || MapArea == null || TechnicianMarkerPrefab == null || JobMarkerPrefab == null) return;
            ClearMarkers();

            BuildSiteMarkers();

            foreach (var tech in DataManager.Technicians)
            {
                var go = Instantiate(TechnicianMarkerPrefab, MapArea);
                var marker = go.GetComponent<TechnicianMarker>();
                marker.SetDetailPanel(TechnicianDetailPanel);
                marker.Bind(tech, DataManager);
                _techMarkers.Add(marker);
                SetMarkerPosition(go.GetComponent<RectTransform>(), tech.MapPosition);
            }

            foreach (var job in DataManager.Jobs)
            {
                var go = Instantiate(JobMarkerPrefab, MapArea);
                var marker = go.GetComponent<JobMarker>();
                marker.SetDetailPanel(JobDetailPanel);
                marker.Bind(job);
                _jobMarkers.Add(marker);
                SetMarkerPosition(go.GetComponent<RectTransform>(), job.MapPosition);
            }
        }

        private void BuildSiteMarkers()
        {
            if (SiteMarkerPrefab == null || MapArea == null || DataManager?.CafePositions == null || DataManager.CafePositions.Count == 0)
            {
                return;
            }

            foreach (var kvp in DataManager.CafePositions)
            {
                var go = Instantiate(SiteMarkerPrefab, MapArea);
                var marker = go.GetComponent<SiteMarker>();
                if (marker != null)
                {
                    marker.Bind(kvp.Key);
                    _siteMarkers.Add(marker);
                }
                SetMarkerPosition(go.GetComponent<RectTransform>(), kvp.Value);
            }
        }

        public void RefreshMarkers()
        {
            if (_techMarkers.Count == 0 && _jobMarkers.Count == 0)
            {
                BuildMarkers();
                return;
            }

            foreach (var marker in _techMarkers)
            {
                marker.Refresh();
                SetMarkerPosition(marker.GetComponent<RectTransform>(), marker.Technician.MapPosition);
            }

            foreach (var marker in _jobMarkers)
            {
                marker.Refresh();
                SetMarkerPosition(marker.GetComponent<RectTransform>(), marker.Job.MapPosition);
            }

            foreach (var marker in _siteMarkers)
            {
                if (marker == null) continue;
                if (DataManager.CafePositions.TryGetValue(marker.Cafe, out var pos))
                {
                    SetMarkerPosition(marker.GetComponent<RectTransform>(), pos);
                }
            }

            ProjectionReporter?.ReportCorners();
        }

        public void HighlightTechnicians(IEnumerable<Technician> technicians)
        {
            var set = technicians != null ? new HashSet<Technician>(technicians) : new HashSet<Technician>();
            foreach (var marker in _techMarkers)
            {
                bool highlight = marker != null && set.Contains(marker.Technician);
                marker?.SetHighlighted(highlight);
            }
        }

        private void ClearMarkers()
        {
            foreach (var marker in _siteMarkers)
            {
                if (marker != null) Destroy(marker.gameObject);
            }
            _siteMarkers.Clear();

            foreach (var marker in _techMarkers)
            {
                if (marker != null) Destroy(marker.gameObject);
            }
            _techMarkers.Clear();

            foreach (var marker in _jobMarkers)
            {
                if (marker != null) Destroy(marker.gameObject);
            }
            _jobMarkers.Clear();
        }

        private void AttachTileContainer()
        {
            if (MapArea == null)
            {
                return;
            }

            // Keep the tile layer clipped to the map panel without relying on scene-time wiring.
            if (MapArea.GetComponent<RectMask2D>() == null)
            {
                MapArea.gameObject.AddComponent<RectMask2D>();
            }

            if (tileContainer == null)
            {
                return;
            }

            tileContainer.SetParent(MapArea, false);
            tileContainer.anchorMin = Vector2.zero;
            tileContainer.anchorMax = Vector2.one;
            tileContainer.offsetMin = Vector2.zero;
            tileContainer.offsetMax = Vector2.zero;
            tileContainer.SetSiblingIndex(0);
        }

        private void SetMarkerPosition(RectTransform rect, Vector2 mapPosition)
        {
            if (MapArea == null || rect == null) return;
            Vector2 normalized = new(
                Mathf.InverseLerp(DataManager.MapMin.x, DataManager.MapMax.x, mapPosition.x),
                Mathf.InverseLerp(DataManager.MapMin.y, DataManager.MapMax.y, mapPosition.y));

            Vector2 size = MapArea.rect.size;
            Vector2 localPos = new(
                (normalized.x - 0.5f) * size.x,
                (normalized.y - 0.5f) * size.y);
            rect.anchoredPosition = localPos;
        }
    }
}
