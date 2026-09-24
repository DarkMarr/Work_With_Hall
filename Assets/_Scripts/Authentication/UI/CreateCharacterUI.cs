using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.Character;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    /// <summary>
    /// Profile creation step: pick a starter avatar from a looping carousel.
    /// </summary>
    public class CreateCharacterUI : BaseUI
    {
        [SerializeField]
        private LoopCharacterCategorySelectionButtons characterSelection;

        [SerializeField]
        private TMP_Text characterNameText;

        [SerializeField]
        private TMP_Text characterInfoText;

        [SerializeField]
        private TMP_Text characterDescriptionText;

        [SerializeField]
        private Button nextButton;

        private CharacterInfoSO[] characters;
        private bool isBusy;

        public void Init(CharacterInfoSO[] characterList, Action<CharacterInfoSO> onCharacterSelected)
        {
            characters = characterList;
            if (characters == null || characters.Length == 0)
            {
                SetBusy(true);
                Debug.LogError("[CreateCharacterUI] No characters to select.");
                return;
            }

            characterSelection.Init(characters, (preview, character) => preview.Show(character.GetCharacterPrefab()));
            characterSelection.OnIDChange += RefreshDetails;
            RefreshDetails(characterSelection.SelectingIndex);

            nextButton.onClick.AddListener(() =>
            {
                if (isBusy) return;
                onCharacterSelected?.Invoke(characters[characterSelection.SelectingIndex]);
            });
            SetBusy(false);
        }

        public void SetBusy(bool busy)
        {
            isBusy = busy;
            foreach (var button in GetComponentsInChildren<Button>(true))
                button.interactable = !busy && characters != null && characters.Length > 0;
        }

        private void RefreshDetails(int index)
        {
            var character = characters[index];
            if (characterNameText != null) characterNameText.text = character.GetName();
            if (characterInfoText != null) characterInfoText.text = $"{character.GetRarity()} · {character.GetSpecies()}";
            if (characterDescriptionText != null) characterDescriptionText.text = character.GetDescription();
        }
    }
}
