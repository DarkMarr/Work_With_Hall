using QuizGame.MyRoom.Decoration;
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

        public event Action<IDecorationItem> OnSelectedCraftItem;

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
            craftItemSelection.OnSelectItem += (button, item) => OnSelectedCraftItem.Invoke((IDecorationItem)item);
            craftItemSelection.OnToggleTabLoaded += decorationList => OnSelectedCraftItem.Invoke(decorationList);
            craftItemSelection.Setup(tabModel);
        }
    }
}