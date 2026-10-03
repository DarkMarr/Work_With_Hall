using QuizGame.Item.Interfaces;
using QuizGame.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Craft.UI
{
    public class CraftingTabUI : BaseUI
    {
        public event Action OnCloseButtonClicked;

        public event Action<ICraftableItem> OnSelectedCraftItem;

        [SerializeField]
        private TextMeshProUGUI craftingTitle;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddListener(() => OnCloseButtonClicked.Invoke());
        }

        public void Init(CraftTabModel tabModel)
        {
            craftingTitle.text = tabModel.GetName() + " Crafting";

            var craftItemSelection = UIManager.Instance.Create<CraftingItemSelectionUI>();
            this.AddChild(craftItemSelection);
            craftItemSelection.OnSelectItem += (button, item) => OnSelectedCraftItem.Invoke((ICraftableItem)item);
            craftItemSelection.OnToggleTabLoaded += craftable => OnSelectedCraftItem.Invoke(craftable);
            craftItemSelection.Setup(tabModel);
        }
    }
}