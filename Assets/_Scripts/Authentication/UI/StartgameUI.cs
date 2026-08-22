using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

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
        }
    }
}
