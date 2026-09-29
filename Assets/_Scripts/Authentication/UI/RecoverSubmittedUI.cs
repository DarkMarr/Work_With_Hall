using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class RecoverSubmittedUI : BaseUI
    {
        [SerializeField]
        private Button backButton;

        private TMP_Text backLabel;

        public void Init(Action onBack)
        {
            backButton.onClick.AddListener(() => onBack?.Invoke());

            backLabel = backButton.GetComponentInChildren<TMP_Text>(true);
            ApplyBackLabel();
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
        }

        private void HandleLocaleChanged(Locale locale) => ApplyBackLabel();

        private void ApplyBackLabel()
        {
            if (backLabel == null) return;
            backLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("Common", "authentication.back");
        }
    }
}
