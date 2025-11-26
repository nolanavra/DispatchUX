using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public class MapExampleController : MonoBehaviour
    {
        [SerializeField] private MapController mapController;
        [SerializeField] private MapMarkerManager markerManager;
        [SerializeField] private double startLat = 42.8864;
        [SerializeField] private double startLon = -78.8784;
        [SerializeField] private int startZoom = 13;
        [SerializeField] private float panSpeed = 5f;
        [SerializeField] private float zoomStep = 1f;

        private void Start()
        {
            if (mapController != null)
            {
                mapController.SetCenter(startLat, startLon);
                mapController.SetZoom(startZoom);
            }

            if (markerManager != null)
            {
                markerManager.AddMarker("example_location", startLat, startLon);
            }
        }

        private void Update()
        {
            if (mapController == null || markerManager == null)
            {
                return;
            }

            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");
            if (Mathf.Approximately(inputX, 0f) && Mathf.Approximately(inputY, 0f))
            {
                return;
            }

            Vector2 worldDelta = new(inputX, inputY);
            worldDelta *= panSpeed * Time.deltaTime * mapController.WorldUnitsPerTile;
            Vector2 newLatLon = mapController.WorldToLatLon(worldDelta);
            mapController.SetCenter(newLatLon.x, newLatLon.y);
            markerManager.RefreshMarkersPositions();
        }

        private void HandleZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Approximately(scroll, 0f))
            {
                return;
            }

            int targetZoom = Mathf.RoundToInt(mapController.Zoom + scroll * zoomStep);
            mapController.SetZoom(targetZoom);
            markerManager.RefreshMarkersPositions();
        }
    }
}
