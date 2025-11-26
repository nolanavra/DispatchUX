using DispatchQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class TimelineJobBlockUI : MonoBehaviour
    {
        public TMP_Text TitleText;
        public TMP_Text DurationText;
        public Image Background;

        public Color NormalColor = new(0.3f, 0.45f, 0.75f, 0.9f);
        public Color CriticalColor = new(0.7f, 0.2f, 0.2f, 0.9f);

        [SerializeField] private JobDetailPanelUI jobDetailPanel;

        private JobTicket _job;

        public void Bind(JobTicket job, JobDetailPanelUI detailPanel = null)
        {
            _job = job;
            if (detailPanel != null)
            {
                jobDetailPanel = detailPanel;
            }

            TitleText.text = job.Title;
            DurationText.text = $"{job.EstimatedDurationHours:0.#}h";
            if (Background != null)
            {
                Background.color = job.Priority == JobPriority.Critical ? CriticalColor : NormalColor;
            }
        }

        public void OnClick()
        {
            if (jobDetailPanel != null && _job != null)
            {
                jobDetailPanel.ShowJob(_job);
            }
        }
    }
}
