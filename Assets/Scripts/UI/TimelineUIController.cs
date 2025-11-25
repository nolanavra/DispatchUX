using System.Collections.Generic;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;

namespace DispatchQuest.UI
{
    public class TimelineUIController : MonoBehaviour
    {
        [Header("References")]
        public DispatchDataManager DataManager;
        public GameObject TechnicianRowPrefab;
        public GameObject JobBlockPrefab;
        public RectTransform ContentRoot;

        private readonly List<GameObject> _rows = new();

        private void Start()
        {
            BuildTimeline();
            if (DataManager != null)
            {
                DataManager.OnJobAssigned += HandleJobAssigned;
                DataManager.OnDataChanged += BuildTimeline;
            }
        }

        private void OnDestroy()
        {
            if (DataManager != null)
            {
                DataManager.OnJobAssigned -= HandleJobAssigned;
                DataManager.OnDataChanged -= BuildTimeline;
            }
        }

        private void HandleJobAssigned(JobTicket job, Technician technician)
        {
            BuildTimeline();
        }

        public void BuildTimeline()
        {
            if (ContentRoot == null || TechnicianRowPrefab == null || JobBlockPrefab == null || DataManager == null) return;

            foreach (var row in _rows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }
            _rows.Clear();

            foreach (var tech in DataManager.Technicians)
            {
                GameObject row = Instantiate(TechnicianRowPrefab, ContentRoot);
                _rows.Add(row);

                var label = row.transform.Find("TechLabel");
                if (label != null)
                {
                    var text = label.GetComponent<UnityEngine.UI.Text>();
                    if (text != null) text.text = tech.Name;
                }

                var jobContainer = row.transform.Find("Jobs");
                RectTransform jobRoot = jobContainer != null
                    ? jobContainer.GetComponent<RectTransform>()
                    : row.transform as RectTransform;

                if (jobRoot == null) continue;
                foreach (var job in tech.AssignedJobs)
                {
                    GameObject block = Instantiate(JobBlockPrefab, jobRoot);
                    var blockUI = block.GetComponent<TimelineJobBlockUI>();
                    blockUI.Bind(job);
                }
            }
        }
    }
}
