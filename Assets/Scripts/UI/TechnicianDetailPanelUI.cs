using System.Linq;
using DispatchQuest.Data;
using UnityEngine;
using TMPro;

namespace DispatchQuest.UI
{
    public class TechnicianDetailPanelUI : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Fields")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text addressText;
        [SerializeField] private TMP_Text phoneText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text plannedHoursText;
        [SerializeField] private TMP_Text expectedHoursText;

        [Header("Skills")]
        [SerializeField] private Transform skillsContainer;
        [SerializeField] private SkillEntryUI skillEntryPrefab;

        [Header("Assigned Jobs")]
        [SerializeField] private Transform assignedJobsContainer;
        [SerializeField] private AssignedJobEntryUI assignedJobEntryPrefab;

        [Header("Notes")]
        [SerializeField] private Transform notesContainer;
        [SerializeField] private NoteEntryUI noteEntryPrefab;

        [Header("External Panels")]
        [SerializeField] private CommunicationPanelUI communicationPanel;
        [SerializeField] private JobDetailPanelUI jobDetailPanel;

        private Technician _currentTechnician;

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

        public bool IsPointerOver(Vector2 screenPosition)
        {
            if (panelRoot == null) return false;
            var rect = panelRoot.GetComponent<RectTransform>();
            return rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition);
        }

        public void ShowTechnician(Technician tech)
        {
            _currentTechnician = tech;
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (tech == null) return;

            tech.EnsureSkillProficiencyEntries();

            if (nameText != null) nameText.text = tech.Name;
            if (addressText != null) addressText.text = string.IsNullOrWhiteSpace(tech.Address) ? "No address" : tech.Address;
            if (phoneText != null) phoneText.text = string.IsNullOrWhiteSpace(tech.Phone) ? "No phone" : tech.Phone;
            if (statusText != null) statusText.text = tech.Status.ToString();
            if (plannedHoursText != null) plannedHoursText.text = $"Planned: {tech.PlannedHoursToday:0.#}h";
            if (expectedHoursText != null) expectedHoursText.text = $"Expected: {tech.ExpectedHoursToday:0.#}h";

            RefreshSkills(tech);
            RefreshAssignedJobs(tech);
            RefreshNotes(tech);
        }

        public void Hide()
        {
            _currentTechnician = null;
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void OnOpenCommunicationForTechnician()
        {
            if (communicationPanel == null || _currentTechnician == null) return;
            communicationPanel.ShowForTechnician(_currentTechnician);
        }

        private void RefreshSkills(Technician tech)
        {
            ClearChildren(skillsContainer);
            if (skillsContainer == null || skillEntryPrefab == null || tech?.SkillProficiencyEntries == null) return;

            foreach (var entry in tech.SkillProficiencyEntries)
            {
                var ui = Instantiate(skillEntryPrefab, skillsContainer);
                ui.SetData(entry.skillName, entry.proficiency);
            }
        }

        private void RefreshAssignedJobs(Technician tech)
        {
            ClearChildren(assignedJobsContainer);
            if (assignedJobsContainer == null || assignedJobEntryPrefab == null || tech?.AssignedJobs == null) return;

            foreach (var job in tech.AssignedJobs)
            {
                if (job == null) continue;
                var entry = Instantiate(assignedJobEntryPrefab, assignedJobsContainer);
                entry.SetData(job, jobDetailPanel);
            }
        }

        private void RefreshNotes(Technician tech)
        {
            ClearChildren(notesContainer);
            if (notesContainer == null || noteEntryPrefab == null || tech?.Notes == null) return;

            foreach (var note in tech.Notes.OrderBy(n => n?.timestamp))
            {
                var entry = Instantiate(noteEntryPrefab, notesContainer);
                entry.SetData(note);
            }
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
