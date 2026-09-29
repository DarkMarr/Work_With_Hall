using TMPro;
using UnityEngine;
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
