using QuizGame.Item.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.Trade
{
    public class TradeSlot : MonoBehaviour
    {
        private static readonly Color tradePanelReadyTradeColor = Color.yellow;

        [SerializeField]
        private SelectItemButton requestButton;

        [SerializeField]
        private SelectItemButton offerButton;

        [SerializeField]
        private TextMeshProUGUI tradeNumberText;

        [SerializeField]
        private GameObject tutorialText;

        [SerializeField]
        private Image tradePanelBG;

        private Color tradePanelOGColor;

        private void Awake()
        {
            tradePanelOGColor = tradePanelBG.color;
        }

        public void Init(
                    int tradePanelNumber,
                    SelectItemButton.InitData requestButtonData,
                    SelectItemButton.InitData offerButtonData)
        {
            SetTradeNumberText(tradePanelNumber);
            requestButton.Init(requestButtonData);
            offerButton.Init(offerButtonData);
            RefreshTradeUI();
        }

        public void SetTradeNumberText(int number)
        {
            tradeNumberText.text = number.ToString();
        }

        public void RefreshTradeUI()
        {
            var isTradeReady = IsTradeReady();
            tutorialText.gameObject.SetActive(!isTradeReady);
            tradePanelBG.color = isTradeReady ? tradePanelReadyTradeColor : tradePanelOGColor;
        }

        public bool IsTradeReady() => GetRequestButton().HasItem() && GetOfferButton().HasItem();

        public SelectItemButton GetRequestButton() => requestButton;
        public SelectItemButton GetOfferButton() => offerButton;
    }
}
