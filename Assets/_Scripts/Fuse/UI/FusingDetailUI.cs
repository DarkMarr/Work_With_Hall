using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Fuse.UI
{
    public class FusingDetailUI : BaseUI
    {
        public event Action<IDecorationItem> OnPreviewButtonClicked;

        public event Action<IDecorationItem> OnFuseButtonClicked;

        [SerializeField]
        private Button previewButton;

        [SerializeField]
        private Button fuseButton;

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
            fuseButton.onClick.AddListener(() => OnFuseButtonClicked?.Invoke(currentDecorationItem));
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
            var fuseRequirements = decorationItem.GetFuseRequirementItems().Cast<IQuantifiableItem>();

            foreach (var requirementItem in fuseRequirements)
            {
                var materialSlot = Instantiate(materialSlotPrefab, requirementContainer).GetComponent<RequirementSlot>();
                materialSlot.Setup(itemData: requirementItem);
            }
        }
    }
}