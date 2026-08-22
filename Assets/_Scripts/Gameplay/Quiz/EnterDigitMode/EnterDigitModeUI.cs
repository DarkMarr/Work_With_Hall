using System;
using UnityEngine;

namespace QuizGame.Gameplay.Quiz.DigitMode
{
    public class QuizEnterDigitModeUI : BaseQuizModeUI
    {
        public enum Panel
        {
            DigitInputPanel,
            WaitingPanel,
            ResultPanel
        }

        public class Info
        {
            public Action<string> OnSubmitButtonClicked;
        }

        [SerializeField]
        private DigitInputPanel digitInputPanel;

        [SerializeField]
        private WaitingPanel waitingPanel;

        [SerializeField]
        private ResultPanel resultPanel;

        public void Init(Info information)
        {
            digitInputPanel.Init((resultDigit) => information?.OnSubmitButtonClicked(resultDigit));
        }

        public void ShowWaitingPanel(string scoreInput)
        {
            waitingPanel.SetScoreText(scoreInput);
            ShowPanel(Panel.WaitingPanel);
        }

        public void ShowResultPanel()
        {
            ShowPanel(Panel.ResultPanel);
        }

        public void ShowPanel(Panel panel)
        {
            digitInputPanel.gameObject.SetActive(panel == Panel.DigitInputPanel);
            waitingPanel.gameObject.SetActive(panel == Panel.WaitingPanel);
            resultPanel.gameObject.SetActive(panel == Panel.ResultPanel);
        }

        public override void SetInteractable(bool isEnable)
        {
            digitInputPanel.SetInteractable(isEnable);
        }
    }
}
