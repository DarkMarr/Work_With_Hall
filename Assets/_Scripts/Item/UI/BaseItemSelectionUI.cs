using System;
using QuizGame.Interfaces;
using QuizGame.UI;
using UnityEngine;

namespace QuizGame.Item.UI
{
    public class BaseItemSelectionUI<TData> : BaseUI where TData : class, IHasSprite
    {
        public delegate void OnSelectItemHandler(SelectItemButton button, TData item);
        public event OnSelectItemHandler OnSelectItem;

        public event Action<int, SelectItemButton> OnButtonCreated;

        public int SelectingItemIndex { get; private set; }
        public int SelectingItemInPageIndex { get; private set; }

        [SerializeField]
        protected SelectItemButtonPagesContainer itemSelectionContainer;

        [SerializeField]
        protected PageCategorySelectionButtons pageCategorySelectionButtons;

        public virtual void Setup(int defaultSelectingItemIndex, TData[] items)
        {
            SetupItemSelection(items);
            SetupDefaultSelection(defaultSelectingItemIndex, items);
        }

        private void SetupItemSelection(TData[] items)
        {
            itemSelectionContainer.Setup(
                data: items,
                onObjectCreate: (buttonIndex, button) =>
                {
                    button.Init(
                        itemSprite: null,
                        onItemButtonClicked: () => OnItemSelected(buttonIndex, button, items)
                    );
                    OnButtonCreated?.Invoke(buttonIndex, button);
                });
        }

        private void OnItemSelected(int buttonIndex, SelectItemButton button, TData[] items)
        {
            var itemIndex = itemSelectionContainer.GetDataIndexFromObjectOnCurrentPage(buttonIndex);
            if (itemIndex < 0 || itemIndex >= items.Length) return;

            SelectingItemIndex = itemIndex;
            SelectingItemInPageIndex = itemSelectionContainer.CurrentPageIndex;

            var item = items[itemIndex];
            SetSelectingItemVisualization(button, item);
            OnSelectItem?.Invoke(button, item);
        }

        private void SetupDefaultSelection(int defaultIndex, TData[] items)
        {
            var pageIndex = 0;

            if (defaultIndex >= 0 && defaultIndex < items.Length)
            {
                pageIndex = itemSelectionContainer.GetPageIndexFromData(defaultIndex);
                var button = itemSelectionContainer.GetObjectFromDataIndex(defaultIndex);
                SetSelectingItemVisualization(button, items[defaultIndex]);
            }
            else
            {
                SetNullCurrentItemVisualization();
            }

            SelectingItemIndex = defaultIndex;
            SelectingItemInPageIndex = pageIndex;

            itemSelectionContainer.OpenPage(pageIndex);
            pageCategorySelectionButtons.Setup(itemSelectionContainer, pageIndex);
        }

        public virtual void RefreshSelection()
        {
            SelectingItemIndex = -1;
            SetNullCurrentItemVisualization();
        }

        public virtual void SetNullCurrentItemVisualization() { }

        public virtual void SetSelectingItemVisualization(SelectItemButton button, TData itemInfo) { }

        public SelectItemButtonPagesContainer GetContainer() => itemSelectionContainer;
    }
}
