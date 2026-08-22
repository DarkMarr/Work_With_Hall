using System;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay.UI
{
    public class GameplayUI : BaseUI
    {
        [SerializeField]
        private TextMeshProUGUI narratorText;

        [SerializeField]
        private TextMeshProUGUI currentAnswerText;

        [SerializeField]
        private TextMeshProUGUI correctAnswerCountText;

        [SerializeField]
        private Image timerFilledImage;

        [SerializeField]
        private Transform quizContainer;

        [SerializeField]
        private PlayerGameplayProfile[] playerProfiles;

        public void SetNarratorText(string message)
        {
            narratorText.text = message;
        }

        public void SetTimerPercentage(float timerPercentage)
        {
            timerFilledImage.fillAmount = timerPercentage;
        }

        public void SetCurrentAnswerCountText(int answeredCount, int totalCount)
        {
            currentAnswerText.text = $"{answeredCount}";
        }

        public void SetCorrectAnswerCountText(int correctCount)
        {
            correctAnswerCountText.text = $"{correctCount}";
        }

        public void SetEnablePlayerUIs(params int[] enableIndex)
        {
            for (int i = 0; i < playerProfiles.Length; i++)
            {
                var enabled = Array.IndexOf(enableIndex, i) >= 0;
                playerProfiles[i].gameObject.SetActive(enabled);
            }
        }

        public void SetPlayerName(int playerIndex, string name)
        {
            if (playerIndex < 0 || playerIndex >= playerProfiles.Length)
            {
                Debug.LogError($"SetPlayerName: playerIndex {playerIndex} is out of range.");
                return;
            }
            playerProfiles[playerIndex].SetPlayerName(name);
        }

        public void SetPlayerPoint(int playerIndex, int point)
        {
            if (playerIndex < 0 || playerIndex >= playerProfiles.Length)
            {
                Debug.LogError($"SetPlayerPoint: playerIndex {playerIndex} is out of range.");
                return;
            }
            playerProfiles[playerIndex].SetPlayerPoint(point);
        }

        public void ClearAllPlayerPoint()
        {
            foreach (var profile in playerProfiles)
            {
                profile.SetPlayerPoint(0);
            }
        }

        public Transform GetQuizContainer() => quizContainer;
    }
}
