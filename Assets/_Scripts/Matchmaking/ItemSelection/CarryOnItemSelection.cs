using System;
using System.Collections.Generic;
using QuizGame.Item.Interfaces;
using UnityEngine;

namespace QuizGame.Matchmaking.UI
{
    public class CarryOnItemSelection : MonoBehaviour
    {
        public event Action<int, IQuantifiableItem> OnCarryOnItemSelected;
        public event Action<int, IQuantifiableItem> OnCarryOnItemUnselected;

        public int MaxSelectableItems => selectedItemVisuals.Length;

        public List<PrepTimeSelectableItem> PrepTimeSelectableItems { get; private set; } = new List<PrepTimeSelectableItem>();

        [SerializeField]
        private PrepTimeSelectableItem prepTimeSelectableItemPrefab;

        [SerializeField]
        private Transform contentContainer;

        [SerializeField]
        private SelectedItemVisual[] selectedItemVisuals;

        public void Init(IQuantifiableItem[] carryOnItems)
        {
            foreach (Transform child in contentContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < carryOnItems.Length; i++)
            {
                var index = i;
                var carryOnItem = carryOnItems[i];
                var prepTimeSelectableItem = Instantiate(prepTimeSelectableItemPrefab, contentContainer);
                prepTimeSelectableItem.SetItemImage(carryOnItem.GetSprite());
                prepTimeSelectableItem.SetItemQuantityText(carryOnItem.GetQuantity().ToString());
                prepTimeSelectableItem.SetIsSelected(false);
                prepTimeSelectableItem.OnItemButtonClicked += () =>
                {
                    if (!prepTimeSelectableItem.IsSelected && CanSelectItem())
                    {
                        OnCarryOnItemSelected?.Invoke(index, carryOnItem);
                        prepTimeSelectableItem.SetIsSelected(true);

                        var firstAvailableSelectedItemVisual = GetFirstAvailableSelectedItemVisual();
                        firstAvailableSelectedItemVisual.SetItem(carryOnItem);
                    }
                    else if (prepTimeSelectableItem.IsSelected)
                    {
                        OnCarryOnItemUnselected?.Invoke(index, carryOnItem);
                        prepTimeSelectableItem.SetIsSelected(false);

                        foreach (var selectedItemVisual in selectedItemVisuals)
                        {
                            if (selectedItemVisual.SelectedItem == carryOnItem)
                            {
                                selectedItemVisual.ClearItem();
                                break;
                            }
                        }
                    }
                };
                PrepTimeSelectableItems.Add(prepTimeSelectableItem);
            }

            foreach (var selectedItemVisual in selectedItemVisuals)
            {
                selectedItemVisual.ClearItem();
            }
        }

        public bool CanSelectItem()
        {
            foreach (var selectedItemVisual in selectedItemVisuals)
            {
                if (!selectedItemVisual.HasItem)
                    return true;
            }
            return false;
        }

        public SelectedItemVisual GetFirstAvailableSelectedItemVisual()
        {
            foreach (var selectedItemVisual in selectedItemVisuals)
            {
                if (!selectedItemVisual.HasItem)
                    return selectedItemVisual;
            }
            return null;
        }

        public IQuantifiableItem[] GetSelectedCarryOnItems()
        {
            var selectedItems = new IQuantifiableItem[selectedItemVisuals.Length];
            for (int i = 0; i < selectedItemVisuals.Length; i++)
            {
                selectedItems[i] = selectedItemVisuals[i].SelectedItem as IQuantifiableItem;
            }
            return selectedItems;
        }
    }
}
