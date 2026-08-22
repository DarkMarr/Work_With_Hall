using System;
using TMPro;
using UnityEngine;

namespace QuizGame.UI
{
    public class CategoryTextSelectionButtons : BaseCategorySelectionButtons
    {
        public event Action<string> OnTextChange;

        public string CurrentText => textContent[SelectingIndex];

        public override int ContentCount => textContent.Length;

        [SerializeField]
        private TextMeshProUGUI visualizeText;

        private string[] textContent;

        public void Init(string[] textContents)
        {
            this.textContent = textContents;
            visualizeText.text = textContents[0];
        }

        public override void RefreshUIAndRaiseEvent()
        {
            base.RefreshUIAndRaiseEvent();
            visualizeText.text = CurrentText;
            OnTextChange?.Invoke(CurrentText);
        }
    }
}
