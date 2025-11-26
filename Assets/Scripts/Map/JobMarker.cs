using DispatchQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DispatchQuest.Map
{
    public class JobMarker : MonoBehaviour, IPointerClickHandler
    {
        public TMP_Text LabelText;
        public Image Icon;
        public Color UnassignedColor = new(0.6f, 0.6f, 0.2f, 0.9f);
        public Color AssignedColor = new(0.2f, 0.7f, 0.25f, 0.9f);

        [HideInInspector] public JobTicket Job;
        [SerializeField] private JobDetailPanelUI jobDetailPanel;

        public void SetDetailPanel(JobDetailPanelUI panel)
        {
            jobDetailPanel = panel;
        }

        public void Bind(JobTicket job)
        {
            Job = job;
            Refresh();
        }

        public void Refresh()
        {
            if (Job == null) return;
            if (LabelText != null)
            {
                LabelText.text = $"{Job.Title}\n{Job.Priority}";
            }

            if (Icon != null)
            {
                Icon.color = Job.AssignedTechnician == null ? UnassignedColor : AssignedColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (jobDetailPanel != null && Job != null)
            {
                jobDetailPanel.ShowJob(Job);
            }
        }
    }
}
