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
        public event Action<ICraftableItem> OnPreviewButtonClicked;

        public event Action<ICraftableItem> OnCraftButtonClicked;

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

        private ICraftableItem currentCraftItem;

        private void Start()
        {
            previewButton.onClick.AddListener(() => OnPreviewButtonClicked?.Invoke(currentCraftItem));
            craftButton.onClick.AddListener(() => OnCraftButtonClicked?.Invoke(currentCraftItem));
        }

        public void Setup(ICraftableItem craftItem)
        {
            Clear();
            currentCraftItem = craftItem;
            itemIcon.sprite = craftItem.GetSprite();
            nameLabel.text = craftItem.GetName();
            SetupRequirement(craftItem);
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

        private void SetupRequirement(ICraftableItem craftItem)
        {
            var craftRequirements = craftItem.GetCraftRequirementItems().Cast<IQuantifiableItem>();

            foreach (var requirementItem in craftRequirements)
            {
                var materialSlot = Instantiate(materialSlotPrefab, requirementContainer).GetComponent<RequirementSlot>();
                materialSlot.Setup(itemData: requirementItem);
            }
        }
    }
}