using System;
using UnityEngine;

namespace QuizGame.Gameplay.Quiz.FourChoicesMode
{
    public class FourChoicesModeUI : BaseQuizModeUI
    {
        [SerializeField]
        private AnswerButton[] answerButtons;

        public class Info
        {
            public string[] ButtonMessage;
            public Action<int> OnSubmitAnswer;
        }

        public void Init(Info information)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                var buttonText = information.ButtonMessage[i];
                var answerIndex = i;
                var answerButton = answerButtons[i];
                answerButton.Init(buttonText, () =>
                {
                    information?.OnSubmitAnswer(answerIndex);

                    var pressedSprite = answerButton.GetButton().spriteState.pressedSprite;
                    answerButton.SetSpriteOnButton(pressedSprite); //Freeze sprite
                });
            }
        }

        public override void SetInteractable(bool isInteractable)
        {
            foreach (var answerButton in answerButtons)
            {
                answerButton.SetInteractable(isInteractable);
            }
        }
    }
}
