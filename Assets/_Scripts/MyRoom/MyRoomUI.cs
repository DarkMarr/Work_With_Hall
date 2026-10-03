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

        [Header("Decoration stage")]
        [SerializeField]
        [Tooltip("Leaves decoration mode without keeping the changes. The tick keeps them.")]
        private Button decorationBackButton;

        [SerializeField]
        [Tooltip("Opens the list of wallpapers for the room itself, rather than for one slot.")]
        private Button roomStyleButton;

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

        /// <summary>
        /// Fills the profile strip above the room. Both fields were wired to the prefab but nothing
        /// ever set them, so the room showed the placeholder 'Name' and 'Rank {0}' from design time.
        /// A blank value is ignored rather than written, so a slow profile fetch leaves the strip
        /// as it is instead of blanking it.
        /// </summary>
        public void SetProfile(string profileName, string rankName)
        {
            if (nameText != null && !string.IsNullOrWhiteSpace(profileName))
            {
                nameText.text = profileName;
            }
            if (rankText != null && !string.IsNullOrWhiteSpace(rankName))
            {
                rankText.text = rankName;
            }
        }

        public void Init(
                        Action onDecorateButtonClicked,
                        Action onDoneButtonClicked,
                        Action onMenuButtonClicked,
                        Action onItemButtonClicked,
                        Action onEquipButtonClicked,
                        Action onTradeButtonClicked,
                        Action onFriendButtonClicked,
                        Action onDecorationBackButtonClicked = null,
                        Action onRoomStyleButtonClicked = null)
        {

            var buttonActionPairs = new (Button button, Action action)[]
            {
                (decorateButton, onDecorateButtonClicked),
                (doneButton, onDoneButtonClicked),
                (decorationBackButton, onDecorationBackButtonClicked),
                (roomStyleButton, onRoomStyleButtonClicked),
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
