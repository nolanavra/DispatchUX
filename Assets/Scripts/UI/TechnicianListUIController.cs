using System.Collections.Generic;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;

namespace DispatchQuest.UI
{
    public class TechnicianListUIController : MonoBehaviour
    {
        public DispatchDataManager DataManager;
        public GameObject TechnicianCardPrefab;
        public RectTransform ContentRoot;
        public CommunicationPanelUI CommunicationPanel;
        public TechnicianDetailPanelUI TechnicianDetailPanel;

        private readonly List<TechnicianCardUI> _spawnedCards = new();

        public IReadOnlyList<TechnicianCardUI> SpawnedCards => _spawnedCards;

        private void Start()
        {
            RefreshList();
            if (DataManager != null)
            {
                DataManager.OnJobAssigned += HandleJobAssigned;
                DataManager.OnDataChanged += RefreshList;
                DataManager.OnWorkloadChanged += RefreshCardsOnly;
                DataManager.OnRoutesGenerated += RefreshCardsOnly;
            }
        }

        private void OnDestroy()
        {
            if (DataManager != null)
            {
                DataManager.OnJobAssigned -= HandleJobAssigned;
                DataManager.OnDataChanged -= RefreshList;
                DataManager.OnWorkloadChanged -= RefreshCardsOnly;
                DataManager.OnRoutesGenerated -= RefreshCardsOnly;
            }
        }

        private void HandleJobAssigned(JobTicket job, Technician technician)
        {
            RefreshList();
        }

        private void RefreshCardsOnly()
        {
            foreach (var card in _spawnedCards)
            {
                card?.Refresh();
            }
        }

        public void RefreshList()
        {
            if (DataManager == null || TechnicianCardPrefab == null || ContentRoot == null) return;

            foreach (var card in _spawnedCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _spawnedCards.Clear();

            foreach (var tech in DataManager.Technicians)
            {
                GameObject go = Instantiate(TechnicianCardPrefab, ContentRoot);
                TechnicianCardUI card = go.GetComponent<TechnicianCardUI>();
                card.CommunicationPanel = CommunicationPanel;
                card.TechnicianDetailPanel = TechnicianDetailPanel;
                card.Bind(tech, DataManager);
                _spawnedCards.Add(card);
            }
        }
    }
}
