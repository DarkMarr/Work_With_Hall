using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay.Quiz.TrueFalseMode
{
    public class TrueFalseModeController : BaseQuizModeUI
    {
        public class Info
        {
            public Action<bool> OnAnswerButtonClicked;
        }

        [SerializeField]
        private Button trueButton;

        [SerializeField]
        private Button falseButton;

        public void Init(Info information)
        {
            trueButton.onClick.AddListener(() =>
            {
                information.OnAnswerButtonClicked?.Invoke(true);
                trueButton.image.sprite = trueButton.spriteState.pressedSprite;
            });

            falseButton.onClick.AddListener(() =>
            {
                information.OnAnswerButtonClicked?.Invoke(false);
                falseButton.image.sprite = falseButton.spriteState.pressedSprite;
            });
        }

        public override void SetInteractable(bool isEnable)
        {
            trueButton.interactable = isEnable;
            falseButton.interactable = isEnable;
        }
    }
}
