using UnityEngine;
using UnityEngine.UI;
using QuizGame.UI;
using System;

namespace QuizGame.Authentication.UI
{
    public class LoginAndRegisterUI : BaseUI
    {
        [SerializeField]
        private Button googleSignIn;

        [SerializeField]
        private Button appleSignIn;

        [SerializeField]
        private Button signInWithEmail;

        [SerializeField]
        private Button createNewAccount;

        [SerializeField]
        private Button recoverAccount;

        public void Init(
            Action onGoogleSignInButtonClicked,
            Action onAppleSignInButtonClicked,
            Action onSignInWithEmailButtonClicked,
            Action onCreateAccountButtonClicked,
            Action onRecoverAccountButtonClicked)
        {
            googleSignIn.onClick.AddListener(() => onGoogleSignInButtonClicked?.Invoke());
            appleSignIn.onClick.AddListener(() => onAppleSignInButtonClicked?.Invoke());
            signInWithEmail.onClick.AddListener(() => onSignInWithEmailButtonClicked?.Invoke());
            createNewAccount.onClick.AddListener(() => onCreateAccountButtonClicked?.Invoke());
            recoverAccount.onClick.AddListener(() => onRecoverAccountButtonClicked?.Invoke());
        }
    }
}
