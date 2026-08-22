using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class CreateProfileNameUI : BaseUI
    {
        public delegate void OnCreateProfileName(string profileName);

        [SerializeField]
        private TMP_InputField profileNameInputField;

        [SerializeField]
        private Button nextButton;

        public void Init(OnCreateProfileName createProfileName)
        {
            nextButton.onClick.AddListener(() => createProfileName.Invoke(profileNameInputField.text));
        }
    }
}
