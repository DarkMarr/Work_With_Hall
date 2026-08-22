using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Fuse.UI
{
    public class FusingTabUI : BaseUI
    {
        public event Action OnCloseButtonClicked;

        public event Action<IDecorationItem> OnSelectedFuseItem;

        [SerializeField]
        private TextMeshProUGUI fusingTitle;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddListener(() => OnCloseButtonClicked.Invoke());
        }

        public void Init(FuseTabModel tabModel)
        {
            fusingTitle.text = tabModel.GetName() + " Fusing";

            var fuseItemSelection = UIManager.Instance.Create<FusingItemSelectionUI>();
            this.AddChild(fuseItemSelection);
            fuseItemSelection.OnSelectItem += (button, item) => OnSelectedFuseItem.Invoke((IDecorationItem)item);
            fuseItemSelection.OnToggleTabLoaded += decorationList => OnSelectedFuseItem.Invoke(decorationList);
            fuseItemSelection.Setup(tabModel);
        }
    }
}