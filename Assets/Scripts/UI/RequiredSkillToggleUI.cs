using System;
using DispatchQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.UI
{
    public class RequiredSkillToggleUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Toggle toggle;

        private RequiredSkillToggle _toggleData;
        private Action<RequiredSkillToggle> _onChanged;

        private void Awake()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        public void Bind(RequiredSkillToggle toggleData, Action<RequiredSkillToggle> onChanged)
        {
            _toggleData = toggleData;
            _onChanged = onChanged;

            if (labelText != null)
            {
                labelText.text = toggleData != null ? toggleData.skillName : "Skill";
            }

            if (toggle != null && toggleData != null)
            {
                toggle.isOn = toggleData.isRequired;
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            if (_toggleData != null)
            {
                _toggleData.isRequired = value;
            }
            _onChanged?.Invoke(_toggleData);
        }
    }
}
