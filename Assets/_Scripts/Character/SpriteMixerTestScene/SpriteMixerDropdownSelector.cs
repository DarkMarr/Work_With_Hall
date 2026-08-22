using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Character
{
    public class SpriteMixerDropdownSelector : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabelText;

        [SerializeField]
        private TMP_Dropdown dropdown;

        public void Init(CharacterSpriteMixer characterSpriteMixer, CharacterSpriteMixerCategory category, string[] labels)
        {
            if (titleLabelText != null)
            {
                titleLabelText.text = category.DisplayTitle;
            }

            if (dropdown != null)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(new List<string>(labels));
                dropdown.value = Array.IndexOf(labels, category.Resolver.GetLabel());
                dropdown.onValueChanged.AddListener((index) =>
                {
                    var categoryName = CharacterSpriteUtilities.GetCategoryNameByPartType(category.PartType);
                    characterSpriteMixer.SetPartLabel(category.PartType, labels[index]);
                });
            }
        }
    }
}
