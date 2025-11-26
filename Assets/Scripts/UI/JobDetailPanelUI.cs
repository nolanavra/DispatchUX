using System.Linq;
using DispatchQuest.Data;
using TMPro;
using UnityEngine;

namespace DispatchQuest.UI
{
    public class JobDetailPanelUI : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Fields")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text clientNameText;
        [SerializeField] private TMP_Text addressText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text priorityText;
        [SerializeField] private TMP_Text estimatedDurationText;
        [SerializeField] private TMP_Text assignedTechnicianText;

        [Header("Required Skills")]
        [SerializeField] private Transform requiredSkillsContainer;
        [SerializeField] private RequiredSkillToggleUI requiredSkillTogglePrefab;

        [Header("Notes")]
        [SerializeField] private Transform notesContainer;
        [SerializeField] private NoteEntryUI noteEntryPrefab;

        [Header("External Panels")]
        [SerializeField] private CommunicationPanelUI communicationPanel;
        [SerializeField] private TechnicianDetailPanelUI technicianDetailPanel;

        private JobTicket _currentJob;

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

        public bool IsPointerOver(Vector2 screenPosition)
        {
            if (panelRoot == null) return false;
            var rect = panelRoot.GetComponent<RectTransform>();
            return rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition);
        }

        public void ShowJob(JobTicket job)
        {
            _currentJob = job;
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (job == null) return;

            job.EnsureRequiredSkillToggles();

            if (titleText != null) titleText.text = job.Title;
            if (clientNameText != null) clientNameText.text = job.ClientName;
            if (addressText != null) addressText.text = string.IsNullOrWhiteSpace(job.Address) ? "No address" : job.Address;
            if (descriptionText != null) descriptionText.text = string.IsNullOrWhiteSpace(job.DetailedDescription) ? "No description" : job.DetailedDescription;
            if (priorityText != null) priorityText.text = job.Priority.ToString();
            if (estimatedDurationText != null) estimatedDurationText.text = $"{job.EstimatedDurationHours:0.#}h";

            if (assignedTechnicianText != null)
            {
                assignedTechnicianText.text = job.AssignedTechnician != null ? job.AssignedTechnician.Name : "Unassigned";
            }

            RefreshRequiredSkills(job);
            RefreshNotes(job);
        }

        public void Hide()
        {
            _currentJob = null;
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void OnAssignedTechButtonClicked()
        {
            if (_currentJob?.AssignedTechnician == null || technicianDetailPanel == null) return;
            technicianDetailPanel.ShowTechnician(_currentJob.AssignedTechnician);
        }

        public void OnOpenCommunicationForJob()
        {
            if (communicationPanel == null || _currentJob == null) return;
            communicationPanel.ShowForJob(_currentJob);
        }

        public void OnOpenCommunicationForTechnician()
        {
            if (_currentJob?.AssignedTechnician == null || communicationPanel == null) return;
            communicationPanel.ShowForTechnician(_currentJob.AssignedTechnician);
        }

        private void RefreshRequiredSkills(JobTicket job)
        {
            ClearChildren(requiredSkillsContainer);
            if (requiredSkillsContainer == null || requiredSkillTogglePrefab == null || job?.RequiredSkillToggles == null) return;

            foreach (var toggleData in job.RequiredSkillToggles)
            {
                var toggleUI = Instantiate(requiredSkillTogglePrefab, requiredSkillsContainer);
                toggleUI.Bind(toggleData, OnRequiredSkillChanged);
            }
        }

        private void RefreshNotes(JobTicket job)
        {
            ClearChildren(notesContainer);
            if (notesContainer == null || noteEntryPrefab == null || job?.Notes == null) return;

            var relevantNotes = job.Notes
                .Where(n => n != null && (string.IsNullOrEmpty(n.technicianId) ||
                                          (job.AssignedTechnician != null && n.technicianId == job.AssignedTechnician.Id)))
                .OrderBy(n => n.timestamp);

            foreach (var note in relevantNotes)
            {
                var entry = Instantiate(noteEntryPrefab, notesContainer);
                entry.SetData(note);
            }
        }

        private void OnRequiredSkillChanged(RequiredSkillToggle toggle)
        {
            if (_currentJob == null) return;
            _currentJob.SyncRequiredSkillsFromToggles();
        }

        private void ClearChildren(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}
