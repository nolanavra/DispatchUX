using DispatchQuest.Data;
using DispatchQuest.Managers;
using DispatchQuest.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DispatchQuest.Map
{
    public class TechnicianMarker : MonoBehaviour, IDropHandler
    {
        public TMP_Text LabelText;
        public Image Icon;
        public Color AvailableColor = new(0.2f, 0.5f, 0.2f, 0.9f);
        public Color BusyColor = new(0.6f, 0.35f, 0.2f, 0.9f);
        public Color OffShiftColor = new(0.4f, 0.4f, 0.4f, 0.9f);

        [HideInInspector] public Technician Technician;
        [HideInInspector] public DispatchDataManager DataManager;

        public void Bind(Technician technician, DispatchDataManager dataManager)
        {
            Technician = technician;
            DataManager = dataManager;
            Refresh();
        }

        public void Refresh()
        {
            if (Technician == null) return;
            if (LabelText != null)
            {
                LabelText.text = $"{Technician.Name}\n{Technician.Status}";
            }

            if (Icon != null)
            {
                Icon.color = Technician.Status switch
                {
                    TechnicianStatus.Available => AvailableColor,
                    TechnicianStatus.Busy => BusyColor,
                    TechnicianStatus.OffShift => OffShiftColor,
                    _ => Icon.color
                };
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DataManager == null || Technician == null) return;
            var card = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<JobCardUI>() : null;
            if (card == null || card.Job == null) return;
            DataManager.AssignJobToTechnician(card.Job, Technician);
        }
    }
}
