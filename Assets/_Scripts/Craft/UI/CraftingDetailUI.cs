using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Craft.UI
{
    public class CraftingDetailUI : BaseUI
    {
        public event Action<IDecorationItem> OnPreviewButtonClicked;

        public event Action<IDecorationItem> OnCraftButtonClicked;

        [SerializeField]
        private Button previewButton;

        [SerializeField]
        private Button craftButton;

        [SerializeField]
        private Image itemIcon;

        [SerializeField]
        private TextMeshProUGUI nameLabel;

        [SerializeField]
        private RequirementSlot materialSlotPrefab;

        [SerializeField]
        private Transform requirementContainer;

        private IDecorationItem currentDecorationItem;

        private void Start()
        {
            previewButton.onClick.AddListener(() => OnPreviewButtonClicked?.Invoke(currentDecorationItem));
            craftButton.onClick.AddListener(() => OnCraftButtonClicked?.Invoke(currentDecorationItem));
        }

        public void Setup(IDecorationItem decorationItem)
        {
            Clear();
            currentDecorationItem = decorationItem;
            itemIcon.sprite = decorationItem.GetSprite();
            nameLabel.text = decorationItem.GetName();
            SetupRequirement(decorationItem);
        }

        public void Clear()
        {
            itemIcon.sprite = default;
            nameLabel.text = default;

            foreach (Transform item in requirementContainer)
            {
                Destroy(item.gameObject);
            }
        }

        private void SetupRequirement(IDecorationItem decorationItem)
        {
            var craftRequirements = decorationItem.GetCraftRequirementItems().Cast<IQuantifiableItem>();

            foreach (var requirementItem in craftRequirements)
            {
                var materialSlot = Instantiate(materialSlotPrefab, requirementContainer).GetComponent<RequirementSlot>();
                materialSlot.Setup(itemData: requirementItem);
            }
        }
    }
}