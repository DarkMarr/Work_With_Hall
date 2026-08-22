using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Character
{
    public class SpriteMixerSceneController : MonoBehaviour
    {
        [SerializeField]
        private CharacterSpriteMixer[] characterSpriteMixers;

        [SerializeField]
        private Transform dropdownContainer;

        [SerializeField]
        private SpriteMixerDropdownSelector dropdownSelectorTemplate;

        [SerializeField]
        private TextMeshProUGUI characterIndexText;

        [SerializeField]
        private Button nextCharacterButton;

        [SerializeField]
        private Button previousCharacterButton;

        private int currentCharacterIndex = 0;

        void Start()
        {
            UpdateCharacterVisibility(characterSpriteMixers[currentCharacterIndex]);
            nextCharacterButton.onClick.AddListener(OnNextCharacterButtonClicked);
            previousCharacterButton.onClick.AddListener(OnPreviousCharacterButtonClicked);
        }

        public void OnNextCharacterButtonClicked()
        {
            if (characterSpriteMixers.Length > 0)
            {
                currentCharacterIndex = (currentCharacterIndex + 1) % characterSpriteMixers.Length;
                UpdateCharacterVisibility(characterSpriteMixers[currentCharacterIndex]);
            }
        }

        public void OnPreviousCharacterButtonClicked()
        {
            if (characterSpriteMixers.Length > 0)
            {
                currentCharacterIndex = (currentCharacterIndex - 1 + characterSpriteMixers.Length) % characterSpriteMixers.Length;
                UpdateCharacterVisibility(characterSpriteMixers[currentCharacterIndex]);
            }
        }

        private void UpdateCharacterVisibility(CharacterSpriteMixer characterSpriteMixer)
        {
            foreach (Transform child in dropdownContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var category in characterSpriteMixer.GetCategories())
            {
                AddDropdownSelector(characterSpriteMixer, category);
            }

            foreach (var character in characterSpriteMixers)
            {
                character.gameObject.SetActive(character == characterSpriteMixer);
            }
            characterIndexText.text = $"Character: {characterSpriteMixer.name}";
        }

        private void AddDropdownSelector(CharacterSpriteMixer characterSpriteMixer, CharacterSpriteMixerCategory category)
        {
            if (dropdownSelectorTemplate != null)
            {
                var libraryAsset = characterSpriteMixer.GetLibraryAsset();
                var categoryName = CharacterSpriteUtilities.GetCategoryNameByPartType(category.PartType);
                var labels = libraryAsset.GetCategoryLabelNames(categoryName).ToArray();
                if (labels.Length > 0)
                {
                    var selector = Instantiate(dropdownSelectorTemplate, dropdownContainer);
                    selector.Init(characterSpriteMixer, category, labels);
                    selector.gameObject.SetActive(true);
                }
            }
        }
    }
}
