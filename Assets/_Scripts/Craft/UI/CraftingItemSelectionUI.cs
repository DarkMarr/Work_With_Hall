using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.MyRoom.Decoration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Craft.UI
{
    public class CraftingItemSelectionUI : BaseItemSelectionUI<IItem>
    {
        public event Action<IDecorationItem> OnToggleTabLoaded;

        [SerializeField]
        private ToggleTab togglePrefab;

        [SerializeField]
        private ToggleGroup itemSelectToggleGroup;

        private CraftTabModel currentTabModel;
        private Dictionary<ToggleTab, DecorationType> decorationTypeByToggleTab;

        public void Setup(CraftTabModel tabModel)
        {
            currentTabModel = tabModel;
            decorationTypeByToggleTab = new Dictionary<ToggleTab, DecorationType>();

            var toggleTabs = CreateToggles(
                togglePrefab: togglePrefab,
                model: tabModel,
                toggleGroup: itemSelectToggleGroup,
                container: itemSelectToggleGroup.transform);
            itemSelectToggleGroup.GetFirstActiveToggle();

            toggleTabs.First().Toggle.isOn = true;
            UpdateItems();
        }

        private List<ToggleTab> CreateToggles(ToggleTab togglePrefab, CraftTabModel model, ToggleGroup toggleGroup, Transform container)
        {
            var toggleTabs = new List<ToggleTab>();
            var types = model.GetDecorationTypes();

            foreach (var type in types)
            {
                var toggleTab = Instantiate(togglePrefab, container).GetComponent<ToggleTab>();
                decorationTypeByToggleTab.Add(toggleTab, type);
                toggleTabs.Add(toggleTab);

                toggleTab.Init(
                    name: model.GetDecorationLabelName(type),
                    group: toggleGroup);
            }

            return toggleTabs;
        }

        private void UpdateItems()
        {
            RefreshUI();
            foreach (var toggleTab in decorationTypeByToggleTab)
            {
                toggleTab.Key.Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        HandleToggleChanged(toggleTab.Key);
                    }
                });
            }
        }

        public void RefreshUI()
        {
            var currentActiveToggleTab = itemSelectToggleGroup.ActiveToggles().FirstOrDefault().GetComponent<ToggleTab>();
            HandleToggleChanged(currentActiveToggleTab);
        }

        private void HandleToggleChanged(ToggleTab toggleTab)
        {
            var decorationItem = currentTabModel.GetDecorationByType(decorationTypeByToggleTab[toggleTab]);
            var craftableItems = decorationItem.Where(item => item.GetCraftRequirementItems().Count() > 0).ToList();
            ShowItems(craftableItems);
            OnToggleTabLoaded.Invoke(craftableItems.First());
        }

        private void ShowItems(List<IDecorationItem> items)
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