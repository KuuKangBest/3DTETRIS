using UnityEngine;
using UnityEngine.UI;

namespace TDTTetris.UI
{
    /// <summary>
    /// 能量条 — 跟随玩家技能能量平滑变化
    /// </summary>
    public class EnergyBar : MonoBehaviour
    {
        [SerializeField] private Skills.FlightAbility flightAbility;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color fullColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] private Color lowColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private float smoothSpeed = 5f;

        private float displayFill;

        private void Start()
        {
            if (flightAbility == null)
                flightAbility = FindObjectOfType<Skills.FlightAbility>();
            displayFill = 1f;
        }

        private void Update()
        {
            if (flightAbility == null) return;

            float target = flightAbility.EnergyRatio;
            displayFill = Mathf.Lerp(displayFill, target, smoothSpeed * Time.deltaTime);

            if (fillImage != null)
            {
                fillImage.fillAmount = displayFill;
                fillImage.color = Color.Lerp(lowColor, fullColor, displayFill);
            }
        }
    }
}
