using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay.Quiz.FourChoicesMode
{
    [RequireComponent(typeof(Button))]
    public class AnswerButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TextMeshProUGUI buttonText;

        private void OnValidate()
        {
            button ??= GetComponent<Button>();
            buttonText ??= GetComponent<TextMeshProUGUI>();
        }

        public void Init(string answerText, Action onButtonClicked)
        {
            buttonText.text = answerText;
            button.onClick.AddListener(() => onButtonClicked?.Invoke());
        }

        public Button GetButton() => button;

        public void SetInteractable(bool isInteractable)
        {
            button.interactable = isInteractable;
        }

        public void SetSpriteOnButton(Sprite sprite)
        {
            button.image.sprite = sprite;
        }
    }
}
