using System;
using TMPro;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.UI
{
    public class MyRoomUI : BaseUI
    {
        public enum Stage
        {
            Normal,
            Decoration
        }

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private Button doneButton;

        [SerializeField]
        private Button decorateButton;

        [Header("Stage")]
        [SerializeField]
        private GameObject normalStageGroup;

        [SerializeField]
        private GameObject decorationStageGroup;

        [Header("Buttons")]
        [SerializeField]
        private Button menuButton;

        [SerializeField]
        private Button itemButton;

        [SerializeField]
        private Button equipButton;

        [SerializeField]
        private Button tradeButton;

        [SerializeField]
        private Button friendsButton;

        public void Init(
                        Action onDecorateButtonClicked,
                        Action onDoneButtonClicked,
                        Action onMenuButtonClicked,
                        Action onItemButtonClicked,
                        Action onEquipButtonClicked,
                        Action onTradeButtonClicked,
                        Action onFriendButtonClicked)
        {

            var buttonActionPairs = new (Button button, Action action)[]
            {
                (decorateButton, onDecorateButtonClicked),
                (doneButton, onDoneButtonClicked),
                (menuButton, onMenuButtonClicked),
                (itemButton, onItemButtonClicked),
                (equipButton, onEquipButtonClicked),
                (tradeButton, onTradeButtonClicked),
                (friendsButton, onFriendButtonClicked),
            };

            foreach (var (button, action) in buttonActionPairs)
            {
                if (button != null && action != null)
                    button.onClick.AddListener(() => action.Invoke());
            }
        }

        public void SwitchUIStage(Stage stage)
        {
            normalStageGroup.gameObject.SetActive(stage == Stage.Normal);
            decorationStageGroup.gameObject.SetActive(stage == Stage.Decoration);
        }
    }
}
