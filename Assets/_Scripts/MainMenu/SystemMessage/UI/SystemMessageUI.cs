using System;
using System.Linq;
using Newtonsoft.Json;
using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.Store;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class SystemMessageUI : BaseUI
    {
        public event Action OnBackButtonClicked;
        public event Action<IQuantifiableItem[]> OnUserAcceptGivingItems;

        [SerializeField]
        private SystemMessageView systemMessageView;

        [SerializeField]
        private Button backButton;

        private SystemMessageData[] systemMessageDatas;

        private void Start()
        {
            backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
        }

        public void Init(SystemMessageData[] systemMessageDatas)
        {
            this.systemMessageDatas = systemMessageDatas;
            systemMessageView.Init(systemMessageDatas.Length, onElementDataUpdate);
        }

        private void onElementDataUpdate(int dataIndex, SystemMessageElement element)
        {
            var messageData = systemMessageDatas[dataIndex];
            var hasDetails = messageData.messageDetails != null;

            element.Setup(messageData.MessageHeader, messageData.isNewMessage, hasDetails);

            if (!hasDetails)
                return;

            element.SetOnDetailsButtonClickedEvent(() => ShowMessageDetailsUI(messageData));
        }

        private void ShowMessageDetailsUI(SystemMessageData messageData)
        {
            var details = messageData.messageDetails;

            var givingItemData = details.givingItems;
            var hasGivingItems = givingItemData != null && givingItemData.Length > 0;

            var givingItemInfos = hasGivingItems
                ? ItemHelper.GetItemsWithQuantityFromData(givingItemData).ToArray()
                : null;

            var messageDetailsUI = UIManager.Instance.Create<SystemMessageDetailsUI>(this);
            messageDetailsUI.Init(
                givingItems: givingItemInfos,
                headerMessage: messageData.MessageHeader,
                bodyMessage: details.BodyMessage,
                canAcceptItem: hasGivingItems && !details.isAlreadyAccepted
            );
            messageDetailsUI.SetOnAcceptItemButtonClickedEvent(() => OnAcceptGivingItems(details, givingItemInfos));
        }

        private void OnAcceptGivingItems(SystemMessageDetailsData details, ItemWithQuantityPair[] givingItemInfos)
        {
            if (details.isAlreadyAccepted)
                return;

            OnUserAcceptGivingItems?.Invoke(givingItemInfos);
            details.isAlreadyAccepted = true;
        }

    }
}
