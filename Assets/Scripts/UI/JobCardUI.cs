using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DispatchQuest.Services;

namespace DispatchQuest.UI
{
    public class JobCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI References")]
        public TMP_Text TitleText;
        public TMP_Text ClientText;
        public TMP_Text PriorityText;
        public TMP_Text DurationText;
        public TMP_Text SkillsText;
        public Image Background;
        public Button RecommendButton;
        public Button NotesButton;

        [Header("Settings")]
        public Color UnassignedColor = new(0.25f, 0.35f, 0.6f, 0.9f);
        public Color AssignedColor = new(0.2f, 0.6f, 0.2f, 0.9f);

        [HideInInspector] public JobTicket Job;
        [HideInInspector] public DispatchDataManager DataManager;
        [HideInInspector] public Canvas RootCanvas;
        [HideInInspector] public JobRecommendationService RecommendationService;
        [HideInInspector] public TechnicianHighlightController HighlightController;
        [HideInInspector] public CommunicationPanelUI CommunicationPanel;

        private CanvasGroup _canvasGroup;
        private Transform _originalParent;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            if (RecommendButton != null)
            {
                RecommendButton.onClick.AddListener(OnRecommendClicked);
            }

            if (NotesButton != null)
            {
                NotesButton.onClick.AddListener(OpenNotes);
            }
        }

        public void Bind(JobTicket job, DispatchDataManager dataManager)
        {
            Job = job;
            DataManager = dataManager;
            Refresh();
        }

        public void Refresh()
        {
            if (Job == null) return;

            TitleText.text = Job.Title;
            ClientText.text = Job.ClientName;
            PriorityText.text = Job.Priority.ToString();
            DurationText.text = $"{Job.EstimatedDurationHours:0.#}h";
            SkillsText.text = string.Join(", ", Job.RequiredSkills);
            if (Background != null)
            {
                Background.color = Job.AssignedTechnician == null ? UnassignedColor : AssignedColor;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Job == null || Job.Status != JobStatus.Unassigned) return;
            _originalParent = transform.parent;
            transform.SetParent(RootCanvas.transform);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.85f;
            UpdateDragPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Job == null || Job.Status != JobStatus.Unassigned) return;
            UpdateDragPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Job == null || Job.Status != JobStatus.Unassigned) return;
            transform.SetParent(_originalParent);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
            _rectTransform.anchoredPosition = Vector2.zero;
        }

        private void UpdateDragPosition(PointerEventData eventData)
        {
            if (RootCanvas == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                RootCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);
            _rectTransform.localPosition = localPoint;
        }

        private void OnRecommendClicked()
        {
            if (RecommendationService == null || HighlightController == null || Job == null) return;
            var recommendations = RecommendationService.GetRecommendedTechnicians(Job);
            HighlightController.HighlightTechnicians(recommendations);
        }

        private void OpenNotes()
        {
            if (CommunicationPanel == null || Job == null) return;
            CommunicationPanel.ShowForJob(Job);
        }
    }
}
