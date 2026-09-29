using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class RecoverAccountUI : BaseUI
    {
        public delegate void OnRecoverAccount(string email);

        [SerializeField]
        private TMP_InputField emailInput;

        [SerializeField]
        private Button submitButton;

        private TMP_Text busyLabel;
        private string idleLabel;

        public void SetBusy(bool busy)
        {
            submitButton.interactable = !busy;
            emailInput.interactable = !busy;
            if (busy && busyLabel == null)
            {
                busyLabel = submitButton.GetComponentInChildren<TMP_Text>();
                if (busyLabel != null) idleLabel = busyLabel.text;
            }
            if (busyLabel != null)
            {
                busyLabel.text = busy
                    ? LocalizationSettings.StringDatabase.GetLocalizedString("Common", "authentication.sending")
                    : idleLabel;
            }
        }

        public void Init(OnRecoverAccount onRecoverAccount, string prefillEmail = null)
        {
            if (!string.IsNullOrEmpty(prefillEmail))
            {
                emailInput.text = prefillEmail;
            }
            submitButton.onClick.AddListener(() => onRecoverAccount?.Invoke(emailInput.text));
        }
    }
}
