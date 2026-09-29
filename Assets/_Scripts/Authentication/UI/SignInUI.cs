using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class SignInUI : BaseUI
    {
        public delegate void OnSignIn(string email, string password);
        
        [SerializeField]
        private TMP_InputField emailInput;

        [SerializeField]
        private TMP_InputField passwordInput;

        [SerializeField]
        private Button signInButton;

        [SerializeField]
        private Button forgotPasswordButton;

        private TMP_Text busyLabel;
        private string idleLabel;

        public void SetBusy(bool busy)
        {
            signInButton.interactable = !busy;
            emailInput.interactable = !busy;
            passwordInput.interactable = !busy;
            forgotPasswordButton.interactable = !busy;
            if (busy && busyLabel == null)
            {
                busyLabel = signInButton.GetComponentInChildren<TMP_Text>();
                if (busyLabel != null) idleLabel = busyLabel.text;
            }
            if (busyLabel != null)
            {
                busyLabel.text = busy
                    ? LocalizationSettings.StringDatabase.GetLocalizedString("Common", "authentication.signing_in")
                    : idleLabel;
            }
        }

        public void Init(OnSignIn signIn, Action<string> onForgotPassword)
        {
            signInButton.onClick.AddListener(() => signIn?.Invoke(emailInput.text, passwordInput.text));
            forgotPasswordButton.onClick.AddListener(() => onForgotPassword?.Invoke(emailInput.text));
        }
    }
}
