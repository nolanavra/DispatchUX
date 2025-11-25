using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class TechnicianCardUI : MonoBehaviour, IDropHandler
    {
        [Header("UI References")]
        public Text NameText;
        public Text SkillsText;
        public Text StatusText;
        public Image Background;

        public Color AvailableColor = new(0.2f, 0.4f, 0.2f, 0.85f);
        public Color BusyColor = new(0.5f, 0.3f, 0.1f, 0.85f);
        public Color OffShiftColor = new(0.3f, 0.3f, 0.3f, 0.85f);

        [HideInInspector] public Technician Technician;
        [HideInInspector] public DispatchDataManager DataManager;

        public void Bind(Technician tech, DispatchDataManager dataManager)
        {
            Technician = tech;
            DataManager = dataManager;
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

        public void OnDrop(PointerEventData eventData)
        {
            if (Technician == null || DataManager == null) return;
            var card = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<JobCardUI>() : null;
            if (card == null || card.Job == null) return;

            DataManager.AssignJobToTechnician(card.Job, Technician);
        }
    }
}
