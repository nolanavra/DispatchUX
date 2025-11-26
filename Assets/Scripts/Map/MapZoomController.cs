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
        [SerializeField] private float minZoom = 0.6f;
        [SerializeField] private float maxZoom = 2.5f;
        [SerializeField] private float zoomStep = 0.2f;
        [SerializeField] private MapProjectionReporter projectionReporter;

        private float _currentZoom = 1f;
        private bool _isPointerOver;

        private void Awake()
        {
            if (mapArea != null)
            {
                _currentZoom = mapArea.localScale.x;
            }
        }

        private void Update()
        {
            if (!_isPointerOver || mapArea == null)
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
            mapArea.localScale = Vector3.one * _currentZoom;
            projectionReporter?.ReportCorners();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
        }
    }
}
