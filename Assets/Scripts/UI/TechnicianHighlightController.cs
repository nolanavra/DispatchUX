using System.Collections.Generic;
using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Map;
using UnityEngine;

namespace DispatchQuest.UI
{
    /// <summary>
    /// Central highlight broker for technician UI cards and map markers.
    /// </summary>
    public class TechnicianHighlightController : MonoBehaviour
    {
        [SerializeField] private TechnicianListUIController listController;
        [SerializeField] private MapViewController mapViewController;

        private readonly HashSet<Technician> _highlighted = new();

        public void HighlightTechnicians(List<Technician> technicians)
        {
            _highlighted.Clear();
            if (technicians != null)
            {
                foreach (var tech in technicians.Where(t => t != null))
                {
                    _highlighted.Add(tech);
                }
            }

            UpdateCardHighlights();
            UpdateMapHighlights();
        }

        public void ClearHighlights()
        {
            _highlighted.Clear();
            UpdateCardHighlights();
            UpdateMapHighlights();
        }

        private void UpdateCardHighlights()
        {
            if (listController == null) return;
            foreach (var card in listController.SpawnedCards)
            {
                bool highlight = card != null && _highlighted.Contains(card.Technician);
                card?.SetHighlight(highlight);
            }
        }

        private void UpdateMapHighlights()
        {
            if (mapViewController == null) return;
            mapViewController.HighlightTechnicians(_highlighted);
        }
    }
}
