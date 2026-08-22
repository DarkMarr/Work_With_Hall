using QuizGame.UI.Interfaces;
using TMPro;
using UnityEngine;

namespace QuizGame.UI
{
    public class PageCategorySelectionButtons : BaseCategorySelectionButtons
    {
        public override int ContentCount => pageContainer.PageCount();

        [SerializeField]
        private TextMeshProUGUI pageCountText;

        IPageContainable pageContainer;

        public void Setup(IPageContainable pageContainable, int startingIndex = 0)
        {
            pageContainer = pageContainable;
            SelectingIndex = startingIndex;
            RefreshUIAndRaiseEvent();
        }

        public override void RefreshUIAndRaiseEvent()
        {
            base.RefreshUIAndRaiseEvent();
            pageCountText.text = $"{SelectingIndex + 1}/{pageContainer.PageCount()}";
        }

        public override void OnRightButtonClicked()
        {
            base.OnRightButtonClicked();
            pageContainer.OpenPage(SelectingIndex);
        }

        public override void OnLeftButtonClicked()
        {
            base.OnLeftButtonClicked();
            pageContainer.OpenPage(SelectingIndex);
        }
    }
}
