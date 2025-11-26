using DispatchQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class AssignedJobEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text jobTitleText;
        [SerializeField] private TMP_Text jobStatusText;
        [SerializeField] private Button openJobButton;

        private JobTicket _job;
        private JobDetailPanelUI _jobDetailPanel;

        private void Awake()
        {
            if (openJobButton != null)
            {
                openJobButton.onClick.AddListener(OpenJobDetail);
            }
        }

        public void SetData(JobTicket job, JobDetailPanelUI jobDetailPanel)
        {
            _job = job;
            _jobDetailPanel = jobDetailPanel;

            if (jobTitleText != null)
            {
                jobTitleText.text = job != null ? job.Title : "Job";
            }

            if (jobStatusText != null)
            {
                jobStatusText.text = job != null ? job.Status.ToString() : string.Empty;
            }
        }

        private void OpenJobDetail()
        {
            if (_job == null || _jobDetailPanel == null) return;
            _jobDetailPanel.ShowJob(_job);
        }
    }
}
