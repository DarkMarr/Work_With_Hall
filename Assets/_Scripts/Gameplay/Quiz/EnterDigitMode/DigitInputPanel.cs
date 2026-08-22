using System;
using System.Text;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay.Quiz.DigitMode
{
    public class DigitInputPanel : MonoBehaviour
    {
        public string CurrentDigit { get; private set; }

        [SerializeField]
        private bool isInteractable = true;

        [SerializeField]
        private int maxDigit = 4;

        [SerializeField]
        private TextMeshProUGUI resultText;

        [SerializeField]
        private Button submitButton;

        [SerializeField]
        private Button resetDigitButton;

        [SerializeField]
        private TextReturnButton[] digitButtons;

        private void Start()
        {
            ResetDigit();
            InitDigitAndResetButtons();
        }

        private void InitDigitAndResetButtons()
        {
            resetDigitButton.onClick.AddListener(ResetDigit);
            foreach (var digitButton in digitButtons)
            {
                digitButton.OnClicked += textInButton =>
                {
                    if (!isInteractable || resultText.text.Length >= maxDigit) return;

                    if (textInButton.Length > 1)
                    {
                        Debug.LogError($"[QuizEnterDigitMode] Text in button must has 1 character.");
                        return;
                    }

                    if (int.TryParse(textInButton, out int result))
                    {
                        AddDigit(textInButton[0]);
                    }
                    else
                    {
                        Debug.LogError($"[QuizEnterDigitMode] This digit doesn't a number, it might cause am issue later.");
                    }
                };
            }
        }

        public void Init(Action<string> onSubmitButtonClicked)
        {
            submitButton.onClick.AddListener(() =>
            {
                onSubmitButtonClicked?.Invoke(CurrentDigit);
                submitButton.image.sprite = submitButton.spriteState.pressedSprite;
            });
        }

        public void SetInteractable(bool isEnable)
        {
            isInteractable = isEnable;
        }

        private void AddDigit(char digit)
        {
            CurrentDigit += digit;
            RefreshResultText();
        }

        public void ResetDigit()
        {
            CurrentDigit = string.Empty;
            RefreshResultText();
        }

        public string GetZeroDigitByMaxDigit()
        {
            var zero = new StringBuilder();
            for (int i = 0; i < maxDigit; i++)
            {
                zero.Append('0');

            }
            return zero.ToString();
        }

        private void RefreshResultText()
        {
            resultText.text = CurrentDigit;
        }
    }
}
