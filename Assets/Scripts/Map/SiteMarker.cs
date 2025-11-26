using DispatchQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DispatchQuest.Map
{
    public class SiteMarker : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image icon;
        [SerializeField] private Color defaultColor = new(0.2f, 0.6f, 0.8f, 0.85f);

        public Cafe Cafe { get; private set; }

        public void Bind(Cafe cafe)
        {
            Cafe = cafe;
            if (labelText != null)
            {
                var name = !string.IsNullOrWhiteSpace(cafe?.name) ? cafe.name : cafe?.brand ?? "Site";
                labelText.text = name;
            }

            if (icon != null)
            {
                icon.color = defaultColor;
            }
        }
    }
}
