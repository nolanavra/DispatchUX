using System.Collections.Generic;
using DispatchQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class CommunicationPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject PanelRoot;
        public TMP_Text HeaderText;
        public RectTransform NotesContentRoot;
        public GameObject NoteEntryPrefab;
        public TMP_InputField InputField;
        public Button AddNoteButton;
        public Button CloseButton;

        private JobTicket _currentJob;
        private Technician _currentTechnician;
        private readonly List<GameObject> _spawnedNotes = new();

        private void Awake()
        {
            if (AddNoteButton != null)
            {
                AddNoteButton.onClick.AddListener(AddNote);
            }

            if (CloseButton != null)
            {
                CloseButton.onClick.AddListener(Hide);
            }
        }

        public void ShowForJob(JobTicket job)
        {
            _currentTechnician = null;
            _currentJob = job;
            if (HeaderText != null && job != null)
            {
                HeaderText.text = $"Job Notes: {job.Title}";
            }
            ShowPanel();
            RefreshNotes();
        }

        public void ShowForTechnician(Technician tech)
        {
            _currentJob = null;
            _currentTechnician = tech;
            if (HeaderText != null && tech != null)
            {
                HeaderText.text = $"Tech Notes: {tech.Name}";
            }
            ShowPanel();
            RefreshNotes();
        }

        public void Hide()
        {
            _currentJob = null;
            _currentTechnician = null;
            if (PanelRoot != null)
            {
                PanelRoot.SetActive(false);
            }
        }

        private void ShowPanel()
        {
            if (PanelRoot != null && !PanelRoot.activeSelf)
            {
                PanelRoot.SetActive(true);
            }
        }

        private void RefreshNotes()
        {
            foreach (var go in _spawnedNotes)
            {
                if (go != null) Destroy(go);
            }
            _spawnedNotes.Clear();

            List<string> notes = _currentJob != null ? _currentJob.Notes : _currentTechnician?.Notes;
            if (notes == null || NoteEntryPrefab == null || NotesContentRoot == null) return;

            foreach (var note in notes)
            {
                var go = Instantiate(NoteEntryPrefab, NotesContentRoot);
                var text = go.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = note;
                }
                _spawnedNotes.Add(go);
            }
        }

        private void AddNote()
        {
            string noteText = InputField != null ? InputField.text : string.Empty;
            if (string.IsNullOrWhiteSpace(noteText)) return;

            if (_currentJob != null)
            {
                _currentJob.Notes.Add(noteText.Trim());
            }
            else if (_currentTechnician != null)
            {
                _currentTechnician.Notes.Add(noteText.Trim());
            }
            if (InputField != null)
            {
                InputField.text = string.Empty;
            }
            RefreshNotes();
        }
    }
}
