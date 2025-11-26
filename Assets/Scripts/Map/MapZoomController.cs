using DispatchQuest.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DispatchQuest.Map
{
    /// <summary>
    /// Zooms the map panel when the pointer is over it using the mouse scroll wheel.
    /// </summary>
    public class MapZoomController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform mapArea;
        [SerializeField] private DispatchDataManager dataManager;
        [SerializeField] private MapViewController mapViewController;
        [SerializeField] private float minZoom = 0.6f;
        [SerializeField] private float maxZoom = 2.5f;
        [SerializeField] private float zoomStep = 0.2f;
        [SerializeField] private MapProjectionReporter projectionReporter;

        private float _currentZoom = 1f;
        private Vector2 _baseMin;
        private Vector2 _baseMax;
        private bool _hasBaseBounds;
        private bool _isPointerOver;

        private void Awake()
        {
            if (mapArea != null)
            {
                mapArea.localScale = Vector3.one;
            }

            CacheBaseBounds();
            ApplyZoom();
        }

        private void Update()
        {
            if (!_hasBaseBounds)
            {
                CacheBaseBounds();
            }

            if (!_isPointerOver || mapArea == null || !_hasBaseBounds)
            {
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            float scrollDelta = mouse.scroll.ReadValue().y;
            if (Mathf.Approximately(scrollDelta, 0f))
            {
                return;
            }

            _currentZoom = Mathf.Clamp(_currentZoom + (scrollDelta / 120f) * zoomStep, minZoom, maxZoom);
            ApplyZoom();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
        }

        private void CacheBaseBounds()
        {
            if (dataManager == null)
            {
                return;
            }

            _baseMin = dataManager.MapMin;
            _baseMax = dataManager.MapMax;
            _hasBaseBounds = true;
        }

        private void ApplyZoom()
        {
            if (dataManager == null || !_hasBaseBounds)
            {
                return;
            }

            Vector2 baseCenter = (_baseMin + _baseMax) * 0.5f;
            Vector2 baseSize = _baseMax - _baseMin;
            Vector2 zoomedSize = baseSize / Mathf.Max(_currentZoom, 0.001f);

            dataManager.MapMin = baseCenter - (zoomedSize * 0.5f);
            dataManager.MapMax = baseCenter + (zoomedSize * 0.5f);

            mapViewController?.RefreshMarkers();
            projectionReporter?.ReportCorners();
        }
    }
}
