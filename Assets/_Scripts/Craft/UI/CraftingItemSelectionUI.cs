using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Craft.UI
{
    public class CraftingItemSelectionUI : BaseItemSelectionUI<IItem>
    {
        public event Action<ICraftableItem> OnToggleTabLoaded;

        [SerializeField]
        private ToggleTab togglePrefab;

        [SerializeField]
        private ToggleGroup itemSelectToggleGroup;

        private CraftTabModel currentTabModel;
        private Dictionary<ToggleTab, CraftTabModel.Category> categoryByToggleTab;

        public void Setup(CraftTabModel tabModel)
        {
            currentTabModel = tabModel;
            categoryByToggleTab = new Dictionary<ToggleTab, CraftTabModel.Category>();

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

            foreach (var category in model.GetCategories())
            {
                var toggleTab = Instantiate(togglePrefab, container).GetComponent<ToggleTab>();
                categoryByToggleTab.Add(toggleTab, category);
                toggleTabs.Add(toggleTab);

                toggleTab.Init(name: category.Label, group: toggleGroup);
            }

            return toggleTabs;
        }

        private void UpdateItems()
        {
            RefreshUI();
            foreach (var toggleTab in categoryByToggleTab)
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
            var craftableItems = categoryByToggleTab[toggleTab].GetItems()
                .Where(item => item.GetCraftRequirementItems().Length > 0)
                .ToList();

            // A category with no recipes yet is a real state, not a fault: the item sheet prices
            // things ahead of the assets existing. Showing it empty beats throwing on First().
            if (craftableItems.Count == 0)
            {
                Debug.Log($"[Craft] Nothing with a recipe under \"{categoryByToggleTab[toggleTab].Label}\" yet.");
                base.Setup(0, new IItem[0]);
                return;
            }

            base.Setup(0, craftableItems.ToArray());
            OnToggleTabLoaded.Invoke(craftableItems.First());
        }
    }
}