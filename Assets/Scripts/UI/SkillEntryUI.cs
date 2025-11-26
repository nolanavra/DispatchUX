using TMPro;
using UnityEngine;

namespace DispatchQuest.UI
{
    public class SkillEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text proficiencyText;

        public void SetData(string skillName, float proficiency)
        {
            if (skillNameText != null)
            {
                skillNameText.text = string.IsNullOrWhiteSpace(skillName) ? "Unknown Skill" : skillName;
            }

            if (proficiencyText != null)
            {
                proficiencyText.text = proficiency.ToString("0.##");
            }
        }
    }
}
