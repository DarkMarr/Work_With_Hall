using System;
using QuizGame.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class SystemMessageElement : MonoBehaviour, IHasRectTransform
    {
        private event Action OnDetailsButtonClicked;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private Button detailsButton;

        [SerializeField]
        private GameObject newTextObj;

        private void Start()
        {
            detailsButton.onClick.AddListener(() => OnDetailsButtonClicked?.Invoke());
        }

        public void Setup(string message, bool isNewMessage, bool hasDetails)
        {
            messageText.text = message;
            detailsButton.gameObject.SetActive(hasDetails);
            newTextObj.SetActive(isNewMessage);
        }

        public void SetOnDetailsButtonClickedEvent(Action action)
        {
            OnDetailsButtonClicked = action;
        }

        public RectTransform GetRectTransform() => rectTransform;
    }
}
