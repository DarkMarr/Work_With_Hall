using System;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.MyItem
{
    public class MyItemUI : BaseItemSelectionUI<IItem>
    {
        public event Action<IItem> OnRemoveItem;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private ToggleGroup itemSelectToggleGroup;

        private List<IItem> trophyItems;
        private List<IItem> equipmentItems;
        private List<IItem> consumableItems;

        private void Start()
        {
            closeButton.onClick.AddListener(() => Close());
        }

        public void Init(List<IItem> trophyItems, List<IItem> equipmentItems, List<IItem> consumableItems)
        {
            this.trophyItems = trophyItems;
            this.equipmentItems = equipmentItems;
            this.consumableItems = consumableItems;

            Refresh();
            foreach (var toggle in itemSelectToggleGroup.GetComponentsInChildren<Toggle>())
            {
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        OnToggleChanged(toggle);
                    }
                });
            }
            OnSelectItem += ShowItemDetails;
        }

        public void Refresh()
        {
            var currentActiveToggele = itemSelectToggleGroup.ActiveToggles().FirstOrDefault();
            OnToggleChanged(currentActiveToggele);
        }

        private void ShowItemDetails(SelectItemButton button, IItem item)
        {
            var detailsUI = UIManager.Instance.Create<MyItemInfoUI>();
            detailsUI.Setup(item);
            detailsUI.OnRemoveItem += OnRemoveItem;
        }

        private void OnToggleChanged(Toggle newlyOn)
        {
            switch (newlyOn.name)
            {
                case "Trophy":
                    ShowItems(trophyItems);
                    break;
                case "Equipment":
                    ShowItems(equipmentItems);
                    break;
                case "Consumable":
                    ShowItems(consumableItems);
                    break;
                default:
                    Debug.LogWarning($"Unknown toggle: {newlyOn.name}");
                    break;
            }
        }

        private void ShowItems(List<IItem> items)
        {
            if (items == null || items.Count() == 0)
            {
                Debug.LogWarning("No items to show.");
                return;
            }
            base.Setup(0, items.ToArray());
        }
    }
}
