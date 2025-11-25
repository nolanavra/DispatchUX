using DispatchQuest.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class TimelineJobBlockUI : MonoBehaviour
    {
        public Text TitleText;
        public Text DurationText;
        public Image Background;

        public Color NormalColor = new(0.3f, 0.45f, 0.75f, 0.9f);
        public Color CriticalColor = new(0.7f, 0.2f, 0.2f, 0.9f);

        public void Bind(JobTicket job)
        {
            TitleText.text = job.Title;
            DurationText.text = $"{job.EstimatedDurationHours:0.#}h";
            if (Background != null)
            {
                Background.color = job.Priority == JobPriority.Critical ? CriticalColor : NormalColor;
            }
        }
    }
}
