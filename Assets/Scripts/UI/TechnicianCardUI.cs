using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class TechnicianCardUI : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [Header("UI References")]
        public TMP_Text NameText;
        public TMP_Text SkillsText;
        public TMP_Text StatusText;
        public Image Background;
        public Image HighlightFrame;
        public Button NotesButton;
        public TechnicianWorkloadMeterUI WorkloadMeter;
        public TechnicianDetailPanelUI TechnicianDetailPanel;

        public Color AvailableColor = new(0.2f, 0.4f, 0.2f, 0.85f);
        public Color BusyColor = new(0.5f, 0.3f, 0.1f, 0.85f);
        public Color OffShiftColor = new(0.3f, 0.3f, 0.3f, 0.85f);
        public Color HighlightColor = new(0.8f, 0.85f, 0.25f, 0.85f);

        [HideInInspector] public Technician Technician;
        [HideInInspector] public DispatchDataManager DataManager;
        [HideInInspector] public CommunicationPanelUI CommunicationPanel;

        private Color _baseHighlightColor;

        private void Awake()
        {
            _baseHighlightColor = HighlightFrame != null ? HighlightFrame.color : Color.white;
            if (NotesButton != null)
            {
                NotesButton.onClick.AddListener(OpenNotes);
            }
        }

        public void Bind(Technician tech, DispatchDataManager dataManager)
        {
            Technician = tech;
            DataManager = dataManager;
            if (WorkloadMeter != null)
            {
                WorkloadMeter.Bind(tech, dataManager);
            }
            Refresh();
        }

        public void Refresh()
        {
            if (Technician == null) return;
            NameText.text = Technician.Name;
            SkillsText.text = string.Join(", ", Technician.Skills);
            StatusText.text = Technician.Status.ToString();
            if (Background != null)
            {
                Background.color = Technician.Status switch
                {
                    TechnicianStatus.Available => AvailableColor,
                    TechnicianStatus.Busy => BusyColor,
                    TechnicianStatus.OffShift => OffShiftColor,
                    _ => Background.color
                };
            }
        }

        public void SetHighlight(bool isHighlighted)
        {
            if (HighlightFrame != null)
            {
                HighlightFrame.enabled = isHighlighted;
                HighlightFrame.color = isHighlighted ? HighlightColor : _baseHighlightColor;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Technician == null || DataManager == null) return;
            var card = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<JobCardUI>() : null;
            if (card == null || card.Job == null) return;

            DataManager.AssignJobToTechnician(card.Job, Technician);
        }

        private void OpenNotes()
        {
            if (CommunicationPanel == null || Technician == null) return;
            CommunicationPanel.ShowForTechnician(Technician);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (TechnicianDetailPanel != null && Technician != null)
            {
                TechnicianDetailPanel.ShowTechnician(Technician);
            }
        }
    }
}
