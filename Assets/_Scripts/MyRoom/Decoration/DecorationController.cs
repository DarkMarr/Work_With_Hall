using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.UI;
using Item.UI;
using Newtonsoft.Json;

namespace QuizGame.MyRoom.Decoration
{
    [Serializable]
    public class DecorationController
    {
        public bool IsOpeningASlot { get; private set; }

        [SerializeField]
        private DecorationView view;

        private DecorationModel model;
        private Dictionary<string, IDecorationItem> itemMapByID = new();

        public void Init(DecorationModel model)
        {
            this.model = model;

            itemMapByID = model.GetAvailableItemsByType()
                .SelectMany(pair => pair.Value)
                .Distinct()
                .ToDictionary(item => item.GetID());

            view.InitSlots(OnSlotClicked);

            foreach (var slot in view.GetSlots())
            {
                var slotID = slot.GetID();
                var installedItem = model.GetItemInSlot(slotID);
                view.SetSlotSprite(slotID, installedItem?.GetSprite());
            }
        }

        private void OnSlotClicked(DecorationSlot slot)
        {
            if (IsOpeningASlot) return;

            IsOpeningASlot = true;
            SetAsDecorateMode(false);
            slot.SetSelectingVisual(true);
            var slotID = slot.GetID();
            var type = slot.GetDecorationType();
            if (!model.GetAvailableItemsByType().TryGetValue(type, out var itemList))
            {
                Debug.Log($"[DecorationController] No item type: {type}");
                return;
            }
            var currentItem = model.GetItemInSlot(slotID);
            var currentIndex = currentItem == null ? -1 : Array.FindIndex(itemList, i => i == currentItem);
            var itemSelectionUI = UIManager.Instance.Create<EquipItemSelectionUI>();
            var itemContainer = itemSelectionUI.GetContainer();
            itemSelectionUI.Init(
                defaultSelectingItemIndex: currentIndex,
                selectionTitle: slot.GetTypeName(),
                itemSprites: itemList,
                onSelectButtonClicked: () =>
                {
                    var selectedIndex = itemSelectionUI.SelectingItemIndex;
                    var selectedItem = itemList[selectedIndex];
                    if (!model.AnySlotOwnItem(selectedItem))
                    {
                        EquipItem(slot, selectedItem);
                    }
                    else
                    {
                        var slotToClear = model.GetSlotOwnerOfItem(selectedItem);
                        UnequipItem(slotToClear.GetSlotID());
                    }
                    RefreshEquipText(itemSelectionUI, selectedItem);
                    RefreshInstallingVisual(itemContainer, itemList);
                }
            );

            itemSelectionUI.OnCloseButtonClicked += () =>
            {
                IsOpeningASlot = false;
                SetAsDecorateMode(true);
                slot.SetSelectingVisual(false);
            };
            itemSelectionUI.OnSelectItem += (button, itemInfo) => RefreshEquipText(itemSelectionUI, (IDecorationItem)itemInfo);
            itemContainer.OnPageChange += () => RefreshInstallingVisual(itemContainer, itemList);

            if (currentIndex >= 0)
            {
                RefreshEquipText(itemSelectionUI, itemList[currentIndex]);
            }
            RefreshInstallingVisual(itemContainer, itemList);
        }

        private void EquipItem(DecorationSlot slot, IDecorationItem item)
        {
            var slotID = slot.GetID();
            model.SetItemInSlot(slotID, item);
            view.SetSlotSprite(slotID, item.GetSprite());
        }

        private void UnequipItem(string slotID)
        {
            model.SetItemInSlot(slotID, null);
            view.SetSlotSprite(slotID, null);
        }

        private void RefreshEquipText(EquipItemSelectionUI selectionUI, IDecorationItem item)
        {
            var isEquipped = model.AnySlotOwnItem(item);
            if (isEquipped)
            {
                selectionUI.VisualizeUnequipText();
            }
            else
            {
                selectionUI.VisualizeEquipText();
            }
        }

        private void RefreshInstallingVisual(SelectItemButtonPagesContainer container, IItem[] itemList)
        {
            var equippedIndices = model.GetIndicesOfItemEquippedInSlots(itemList);
            foreach (var button in container.GetSpawnedObjects())
            {
                if (button is EquipItemButton equipButton)
                    equipButton.SetEquippedVisualizeActive(false);
            }

            foreach (var index in equippedIndices)
            {
                if (container.IsDataBelongToCurrentPage(index))
                {
                    var inPageIndex = container.GetDataIndexInPage(index);
                    if (container.TryGetSpawnedObject(inPageIndex, out var btn) && btn is EquipItemButton equipBtn)
                    {
                        equipBtn.SetEquippedVisualizeActive(true);
                    }
                }
            }
        }

        public string GetDecorationDatasAsJson() => JsonConvert.SerializeObject(model.GetAllSlotDatas(), Formatting.Indented);
        public void SetAsDecorateMode(bool isActive) => view.SetChangeDecorateButtonsActive(isActive);
    }
}
