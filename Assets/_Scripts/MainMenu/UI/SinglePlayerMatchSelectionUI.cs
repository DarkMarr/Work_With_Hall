using System;
using QuizGame.Localization;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class SinglePlayerMatchSelectionUI : BaseUI
    {
        public event Action OnLibraryButtonClicked;
        public event Action OnPlaygroundButtonClicked;
        public event Action OnLeaderboardButtonClicked;
        public event Action OnBackButtonClicked;

        [SerializeField]
        private Button libraryButton;

        [SerializeField]
        private Button playgroundButton;

        [SerializeField]
        private Button leaderboardButton;

        [SerializeField]
        private Button backButton;

        void Start()
        {
            libraryButton.onClick.AddListener(() => OnLibraryButtonClicked?.Invoke());
            playgroundButton.onClick.AddListener(() => OnPlaygroundButtonClicked?.Invoke());
            leaderboardButton.onClick.AddListener(() => OnLeaderboardButtonClicked?.Invoke());
            backButton.onClick.AddListener(() =>
            {
                OnBackButtonClicked?.Invoke();
                Close();
            });
        }
    }
}