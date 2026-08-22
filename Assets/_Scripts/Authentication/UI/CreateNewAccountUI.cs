using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class CreateNewAccountUI : BaseUI
    {
        public delegate void OnCreateNewAccount(string email, string password, string reEnterPassword);

        [SerializeField]
        private TMP_InputField enterYourEmailInput;

        [SerializeField]
        private TMP_InputField passwordInput;

        [SerializeField]
        private TMP_InputField reEnterPasswordInput;

        [SerializeField]
        private Button submitButton;

        public void Init(OnCreateNewAccount createNewAccount)
        {
            submitButton.onClick.AddListener(() =>
                createNewAccount.Invoke(enterYourEmailInput.text, passwordInput.text, reEnterPasswordInput.text));
        }
    }
}
