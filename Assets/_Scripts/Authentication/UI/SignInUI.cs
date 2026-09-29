using System;
using TMPro;
using UnityEngine;
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

        public void Init(OnSignIn signIn, Action<string> onForgotPassword)
        {
            signInButton.onClick.AddListener(() => signIn?.Invoke(emailInput.text, passwordInput.text));
            forgotPasswordButton.onClick.AddListener(() => onForgotPassword?.Invoke(emailInput.text));
        }
    }
}
