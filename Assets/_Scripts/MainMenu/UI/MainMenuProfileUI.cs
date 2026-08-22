using QuizGame.Localization;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

namespace QuizGame.MainMenu.UI
{
    public class MainMenuProfileUI : BaseUI
    {
        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private TextMeshProUGUI profileNameText;

        [SerializeField]
        private TextMeshProUGUI energyRechargeText;

        [SerializeField]
        private TextMeshProUGUI energyText;

        private LocalizedStringProperty energyRechargeLocalized;

        protected override void Awake()
        {
            base.Awake();
            energyRechargeLocalized = energyRechargeText.GetTrackedLocalizedText();
        }

        public void SetTimer(int millisecondLeft)
        {
            var min = millisecondLeft / 60;
            var sec = millisecondLeft % 60;

            energyRechargeLocalized = energyRechargeText.GetTrackedLocalizedText();
            energyRechargeLocalized?.SetLocalizedArguments(min, sec);
        }

        public void SetRank(string rank)
        {
            rankText.SetLocalizedArguments(rank);
        }

        public void SetProfileName(string profileName)
        {
            profileNameText.text = profileName;
        }

        public void SetEnergy(int current, int max)
        {
            energyText.text = $"{current} / {max}";
        }
    }
}
