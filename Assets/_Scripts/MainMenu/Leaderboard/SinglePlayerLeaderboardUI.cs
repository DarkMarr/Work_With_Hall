using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QuizGame.MainMenu.Leaderboard
{
    public class SinglePlayerLeaderboardUI : MultiplayerLeaderboardUI
    {
        public event Action<int> OnCategoryChanged;

        // [SerializeField]
        // private TMP_Dropdown categoryDropdown;

        public void SetupDropdownOptions(string[] options)
        {
            // categoryDropdown.ClearOptions();
            // categoryDropdown.AddOptions(new List<string>(options));
            // categoryDropdown.onValueChanged.AddListener((selectIndex) => OnCategoryChanged?.Invoke(selectIndex));
        }
    }
}
