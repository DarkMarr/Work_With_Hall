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
        private bool isBusy;

        public void Init(OnCreateProfileName createProfileName)
        {
            nextButton.onClick.AddListener(() =>
            {
                if (!isBusy && !string.IsNullOrWhiteSpace(profileNameInputField.text))
                    createProfileName.Invoke(profileNameInputField.text.Trim());
            });
            profileNameInputField.onValueChanged.AddListener(_ => SetBusy(isBusy));
            SetBusy(false);
        }

        public void SetBusy(bool busy)
        {
            isBusy = busy;
            profileNameInputField.interactable = !busy;
            nextButton.interactable = !busy && !string.IsNullOrWhiteSpace(profileNameInputField.text);
        }
    }
}
