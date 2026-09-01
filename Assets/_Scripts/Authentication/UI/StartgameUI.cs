using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class StartGameUI : BaseUI
    {
        public event Action OnStartGameButtonClicked;

        [SerializeField]
        private Button startGameButton;

        private void Start()
        {
            startGameButton.onClick.AddListener(() => OnStartGameButtonClicked?.Invoke());
            CreateContactUsButton();
        }

        private void CreateContactUsButton()
        {
            if (transform.Find("Contact-Us-Button") != null) return;

            var buttonObject = new GameObject("Contact-Us-Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var buttonTransform = buttonObject.GetComponent<RectTransform>();
            buttonTransform.SetParent(transform, false);
            buttonTransform.anchorMin = new Vector2(0.5f, 0f);
            buttonTransform.anchorMax = new Vector2(0.5f, 0f);
            buttonTransform.pivot = new Vector2(0.5f, 0f);
            buttonTransform.anchoredPosition = new Vector2(0f, 125f);
            buttonTransform.sizeDelta = new Vector2(744f, 96f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(OpenContactUs);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var labelTransform = labelObject.GetComponent<RectTransform>();
            labelTransform.SetParent(buttonTransform, false);
            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = Vector2.zero;
            labelTransform.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "Contact Us";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 36f;
            label.raycastTarget = false;
        }

        private void OpenContactUs()
        {
            ContactUsPopupUI.Open(transform.root);
        }
    }
}
