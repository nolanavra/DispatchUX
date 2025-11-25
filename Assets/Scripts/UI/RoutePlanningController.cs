using DispatchQuest.Services;
using DispatchQuest.UI;
using UnityEngine;

namespace DispatchQuest.Controllers
{
    public class RoutePlanningController : MonoBehaviour
    {
        [SerializeField] private RoutePlannerService routePlannerService;
        [SerializeField] private TechnicianHighlightController highlightController;

        public void GenerateRoutes()
        {
            if (routePlannerService == null) return;
            highlightController?.ClearHighlights();
            routePlannerService.GenerateDailyRoutesForAllTechnicians();
        }
    }
}
