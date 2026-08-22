using System;
using QuizGame.Interfaces;
using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.MyItem
{
    public class MyItemInfoUI : BaseUI
    {
        public Action<IItem> OnRemoveItem;

        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Button recycleButton;

        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private TextMeshProUGUI itemTypeText;

        [SerializeField]
        private TextMeshProUGUI quantityText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
        }

        public void Setup(IItem item)
        {
            SetupQuantifiableItem(item);
            SetupRecycleableItem(item);
            SetupDescriptionItem(item);

            itemNameText.text = item.GetName();
            if (item is IDecorationItem decorationItem)
            {
                itemTypeText.text = decorationItem.GetSubDescription();
            }
            else
            {
                itemTypeText.text = item.GetItemType().ToString(); //TODO: Replace with localization
            }
            itemImage.sprite = item.GetSprite();
        }

        private void SetupQuantifiableItem(IItem item)
        {
            if (item is IQuantifiableItem quantifiableItem)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = $"Quantity : {quantifiableItem.GetQuantity()}";
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        private void SetupDescriptionItem(IItem item)
        {
            if (item is IHasDescription descriptionItem)
            {
                descriptionText.text = descriptionItem.GetDescription();
            }
            else
            {
                descriptionText.text = "";
            }
        }

        private void SetupRecycleableItem(IItem item)
        {
            if (item is not IRecyclable recycleableItem)
            {
                recycleButton.gameObject.SetActive(false);
                return;
            }

            recycleButton.gameObject.SetActive(true);
            recycleButton.onClick.RemoveAllListeners();
            recycleButton.onClick.AddListener(() => OnRecycleButtonClicked(item, recycleableItem));
        }

        private void OnRecycleButtonClicked(IItem item, IRecyclable recycleableItem)
        {
            Hide();
            var confirmPopupUI = UIManager.Instance.Create<ConfirmPopupUI>();
            confirmPopupUI.Setup(
                title: "Recycle ?",
                description: item.GetName(),
                onCancelButtonClicked: () => OnRecycleCancel(confirmPopupUI),
                onConfirmButtonClicked: () => OnRecycleConfirm(item, recycleableItem, confirmPopupUI)
            );
        }

        private void OnRecycleCancel(ConfirmPopupUI confirmPopupUI)
        {
            Show();
            confirmPopupUI.Close();
        }

        private void OnRecycleConfirm(IItem item, IRecyclable recycleableItem, ConfirmPopupUI confirmPopupUI)
        {
            confirmPopupUI.Close();
            Close();

            ShowAcquiredItems(recycleableItem);
            OnRemoveItem?.Invoke(item);
        }

        private void ShowAcquiredItems(IRecyclable recycleableItem)
        {
            var itemAcquiredUI = UIManager.Instance.Create<ItemAcquiredUI>();
            itemAcquiredUI.Init(recycleableItem.GetRecycledItems());
        }
    }
}
