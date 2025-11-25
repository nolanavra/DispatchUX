using System.Collections.Generic;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;

namespace DispatchQuest.Map
{
    public class MapViewController : MonoBehaviour
    {
        public DispatchDataManager DataManager;
        public RectTransform MapArea;
        public GameObject TechnicianMarkerPrefab;
        public GameObject JobMarkerPrefab;

        private readonly List<TechnicianMarker> _techMarkers = new();
        private readonly List<JobMarker> _jobMarkers = new();

        private void Start()
        {
            BuildMarkers();
            if (DataManager != null)
            {
                DataManager.OnJobAssigned += HandleJobAssigned;
                DataManager.OnDataChanged += RefreshMarkers;
            }
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

        private void BuildMarkers()
        {
            if (DataManager == null || MapArea == null || TechnicianMarkerPrefab == null || JobMarkerPrefab == null) return;
            ClearMarkers();

            foreach (var tech in DataManager.Technicians)
            {
                var go = Instantiate(TechnicianMarkerPrefab, MapArea);
                var marker = go.GetComponent<TechnicianMarker>();
                marker.Bind(tech, DataManager);
                _techMarkers.Add(marker);
                SetMarkerPosition(go.GetComponent<RectTransform>(), tech.MapPosition);
            }

            foreach (var job in DataManager.Jobs)
            {
                var go = Instantiate(JobMarkerPrefab, MapArea);
                var marker = go.GetComponent<JobMarker>();
                marker.Bind(job);
                _jobMarkers.Add(marker);
                SetMarkerPosition(go.GetComponent<RectTransform>(), job.MapPosition);
            }
        }

        private void RefreshMarkers()
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
        }

        private void ClearMarkers()
        {
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
