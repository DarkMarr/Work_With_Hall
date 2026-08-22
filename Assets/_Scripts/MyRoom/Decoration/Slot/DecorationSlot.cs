using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace QuizGame.MyRoom.Decoration
{
    public class DecorationSlot : MonoBehaviour
    {
        [SerializeField]
        private string SlotID;

        [SerializeField]
        private DecorationType decorationType;

        [SerializeField]
        private LocalizedString itemTypeLocalized;

        [SerializeField]
        private SpriteRenderer decorationSpriteRenderer;

        [SerializeField]
        private Button changeSpriteButton;

        [SerializeField]
        private GameObject selectingVisual;

        public void Init(Action onChangeDecorationButtonClicked)
        {
            changeSpriteButton.onClick.AddListener(() => onChangeDecorationButtonClicked?.Invoke());
            SetSelectingVisual(false);
        }

        public DecorationType GetDecorationType() => decorationType;
        public string GetTypeName() => itemTypeLocalized.GetLocalizedString();
        public string GetID() => SlotID;

        public void SetChangeSpriteButtonActive(bool isActive)
        {
            changeSpriteButton.gameObject.SetActive(isActive);
        }

        public void SetSprite(Sprite sprite)
        {
            decorationSpriteRenderer.sprite = sprite;
        }

        public void SetSelectingVisual(bool isActive)
        {
            selectingVisual.SetActive(isActive);
        }
    }
}
