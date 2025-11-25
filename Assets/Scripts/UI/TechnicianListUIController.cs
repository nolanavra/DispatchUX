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

        private readonly List<TechnicianCardUI> _spawnedCards = new();

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
                card.Bind(tech, DataManager);
                _spawnedCards.Add(card);
            }
        }
    }
}
