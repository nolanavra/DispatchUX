using UnityEngine;
using UnityEngine.InputSystem;

namespace DispatchQuest.UI
{
    /// <summary>
    /// Listens for mouse input to close floating panels when clicking outside them.
    /// Right click always hides; left click hides if not over any tracked panel.
    /// </summary>
    public class PanelDismissInputHandler : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private CommunicationPanelUI communicationPanel;
        [SerializeField] private TechnicianDetailPanelUI technicianDetailPanel;
        [SerializeField] private JobDetailPanelUI jobDetailPanel;

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                HideAllPanels();
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 clickPos = mouse.position.ReadValue();
                if (!IsPointerOverAnyPanel(clickPos))
                {
                    HideAllPanels();
                }
            }
        }

        private bool IsPointerOverAnyPanel(Vector2 screenPosition)
        {
            bool overCommunication = communicationPanel != null && communicationPanel.IsVisible &&
                                     communicationPanel.IsPointerOver(screenPosition);
            bool overTechnician = technicianDetailPanel != null && technicianDetailPanel.IsVisible &&
                                   technicianDetailPanel.IsPointerOver(screenPosition);
            bool overJob = jobDetailPanel != null && jobDetailPanel.IsVisible &&
                           jobDetailPanel.IsPointerOver(screenPosition);

            return overCommunication || overTechnician || overJob;
        }

        private void HideAllPanels()
        {
            if (communicationPanel != null && communicationPanel.IsVisible)
            {
                communicationPanel.Hide();
            }

            if (technicianDetailPanel != null && technicianDetailPanel.IsVisible)
            {
                technicianDetailPanel.Hide();
            }

            if (jobDetailPanel != null && jobDetailPanel.IsVisible)
            {
                jobDetailPanel.Hide();
            }
        }
    }
}
