using DispatchQuest.Data;
using DispatchQuest.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class TechnicianWorkloadMeterUI : MonoBehaviour
    {
        [Header("UI References")]
        public Image FillImage;
        public TMP_Text WorkloadLabel;
        public Color LowColor = new(0.35f, 0.75f, 0.35f, 0.9f);
        public Color NormalColor = new(0.35f, 0.55f, 0.85f, 0.9f);
        public Color HighColor = new(0.85f, 0.35f, 0.35f, 0.9f);

        [SerializeField] private float maxHoursScale = 10f;

        private Technician _technician;
        private DispatchDataManager _dataManager;

        private void OnEnable()
        {
            if (_dataManager != null)
            {
                _dataManager.OnWorkloadChanged += Refresh;
                _dataManager.OnDataChanged += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (_dataManager != null)
            {
                _dataManager.OnWorkloadChanged -= Refresh;
                _dataManager.OnDataChanged -= Refresh;
            }
        }

        public void Bind(Technician technician, DispatchDataManager dataManager)
        {
            _technician = technician;
            _dataManager = dataManager;
            Refresh();
        }

        public void Refresh()
        {
            if (_technician == null || _dataManager == null) return;
            float hours = _dataManager.GetTechnicianWorkloadHours(_technician);
            float maxHours = Mathf.Max(maxHoursScale, _dataManager.WorkloadHighThreshold);
            float fill = Mathf.Clamp01(hours / maxHours);

            if (FillImage != null)
            {
                Color target = hours < _dataManager.WorkloadLowThreshold
                    ? LowColor
                    : hours > _dataManager.WorkloadHighThreshold
                        ? HighColor
                        : NormalColor;
                FillImage.fillAmount = fill;
                FillImage.color = target;
            }

            if (WorkloadLabel != null)
            {
                WorkloadLabel.text = $"{hours:0.#}h";
            }
        }
    }
}
