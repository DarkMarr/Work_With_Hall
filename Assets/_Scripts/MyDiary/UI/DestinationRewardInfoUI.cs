using QuizGame.Destination;
using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyDiary.UI
{
    public class DestinationRewardInfoUI : BaseItemSelectionUI<IItem>
    {
        [SerializeField]
        private ToggleGroup itemSelectToggleGroup;

        private List<IItem> trophyItems;
        private List<IItem> equipmentItems;
        private List<IItem> materialItems;

        public void Setup(DestinationItemReward destinationReward)
        {
            UpdateItems(destinationReward);
            OnSelectItem += ShowItemDetails;
        }

        public void UpdateItems(DestinationItemReward model)
        {
            trophyItems = model.GetTrophyRewardItems().ToList();
            equipmentItems = model.GetEquipmentRewardItems().ToList();
            materialItems = model.GetMaterialRewardItems().ToList();

            RefreshUI();
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
        }

        public void RefreshUI()
        {
            var currentActiveToggele = itemSelectToggleGroup.ActiveToggles().FirstOrDefault();
            OnToggleChanged(currentActiveToggele);
        }

        private void ShowItemDetails(SelectItemButton button, IItem item)
        {
            var detailsUI = UIManager.Instance.Create<ItemPopupInfoUI>();
            detailsUI.Setup(item);
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

                case "Material":
                    ShowItems(materialItems);
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