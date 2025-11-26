using System.Collections.Generic;
using DispatchQuest.Managers;
using DispatchQuest.Data;
using DispatchQuest.Services;
using UnityEngine;

namespace DispatchQuest.UI
{
    public class JobListUIController : MonoBehaviour
    {
        public DispatchDataManager DataManager;
        public GameObject JobCardPrefab;
        public RectTransform ContentRoot;
        public Canvas RootCanvas;
        public JobRecommendationService RecommendationService;
        public TechnicianHighlightController HighlightController;
        public CommunicationPanelUI CommunicationPanel;
        public JobDetailPanelUI JobDetailPanel;

        private readonly List<JobCardUI> _spawnedCards = new();

        private void Start()
        {
            RefreshList();
            if (DataManager != null)
            {
                DataManager.OnJobAssigned += HandleJobAssigned;
                DataManager.OnDataChanged += RefreshList;
            }
        }

        private void OnDestroy()
        {
            if (DataManager != null)
            {
                DataManager.OnJobAssigned -= HandleJobAssigned;
                DataManager.OnDataChanged -= RefreshList;
            }
        }

        private void HandleJobAssigned(JobTicket job, Technician technician)
        {
            RefreshList();
        }

        public void RefreshList()
        {
            if (DataManager == null || JobCardPrefab == null || ContentRoot == null) return;

            foreach (var card in _spawnedCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _spawnedCards.Clear();

            var jobs = DataManager.GetUnassignedJobs();
            foreach (var job in jobs)
            {
                GameObject go = Instantiate(JobCardPrefab, ContentRoot);
                JobCardUI card = go.GetComponent<JobCardUI>();
                card.RootCanvas = RootCanvas;
                card.RecommendationService = RecommendationService;
                card.HighlightController = HighlightController;
                card.CommunicationPanel = CommunicationPanel;
                card.JobDetailPanel = JobDetailPanel;
                card.Bind(job, DataManager);
                _spawnedCards.Add(card);
            }
        }
    }
}
