using System;
using DispatchQuest.Data;
using TMPro;
using UnityEngine;

namespace DispatchQuest.UI
{
    public class NoteEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text bodyText;

        public void SetData(JobNote note)
        {
            if (note == null)
            {
                if (headerText != null) headerText.text = "Unknown note";
                if (bodyText != null) bodyText.text = string.Empty;
                return;
            }

            if (headerText != null)
            {
                string author = string.IsNullOrWhiteSpace(note.author) ? "Unknown" : note.author;
                string timestamp = note.timestamp != default ? note.timestamp.ToString("g") : DateTime.Now.ToString("g");
                headerText.text = $"{timestamp} · {author}";
            }

            if (bodyText != null)
            {
                bodyText.text = note.text ?? string.Empty;
            }
        }
    }
}
