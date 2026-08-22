using System;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Item.UI;
using QuizGame.Material;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.Trade
{
    public class TradeUI : BaseUI
    {
        public event Action<int, TradeSlotInfo> OnCollectTradeMaterial;

        [SerializeField]
        private ImageWithTextVisualization heldMaterialVisualizationPrefab;

        [SerializeField]
        private Transform materialVisualizationContainer;

        [SerializeField]
        private TradeSlot tradePanelPrefab;

        [SerializeField]
        private CompletedTradeSlot completedTradePanelPrefab;

        [SerializeField]
        private Transform tradePanelContainer;

        [SerializeField]
        private TextMeshProUGUI offerTradeQuotaText;

        [SerializeField]
        private Button backButton;

        List<ImageWithTextVisualization> heldMaterialVisualizations = new List<ImageWithTextVisualization>();
        List<IQuantifiableMaterial> materials;

        private ItemSelectionUI currentItemSelectionUI;

        public void Init(
            List<TradeSlotInfo> tradeSlotInfos,
            List<IQuantifiableMaterial> materials,
            Action<TradeSlotInfo, int> onRequestItemSelect,
            Action<TradeSlotInfo, int> onOfferItemSelect,
            Action onBackButtonClick)
        {
            this.materials = materials;
            backButton.onClick.AddListener(() => onBackButtonClick?.Invoke());
            InitTradePanels(tradeSlotInfos, onRequestItemSelect, onOfferItemSelect);
            InitMaterialVisualizations();
        }

        public void RefreshMaterialsVisualization()
        {
            for (int i = 0; i < heldMaterialVisualizations.Count; i++)
            {
                var material = materials[i];
                heldMaterialVisualizations[i].Setup(material.GetSprite(), $"x{material.GetQuantity()}");
            }
        }

        private void InitTradePanels(
            List<TradeSlotInfo> tradeSlotInfos,
            Action<TradeSlotInfo, int> onRequestItemSelect,
            Action<TradeSlotInfo, int> onOfferItemSelect)
        {
            for (int i = 0; i < tradeSlotInfos.Count; i++)
            {
                var tradeSlotInfo = tradeSlotInfos[i];
                var tradeSlotNumber = i + 1;
                if (tradeSlotInfo.IsTradeCompleted())
                {
                    CreateCompletedTradeSlot(tradeSlotNumber, tradeSlotInfo);
                }
                else
                {
                    CreateTradeSlot(tradeSlotNumber, tradeSlotInfo, onRequestItemSelect, onOfferItemSelect);
                }
            }
        }

        private void CreateCompletedTradeSlot(
            int tradeSlotNumber,
            TradeSlotInfo tradeSlotInfo)
        {
            var tradePanel = Instantiate(completedTradePanelPrefab, tradePanelContainer);
            tradePanel.Init(tradeSlotNumber, tradeSlotInfo.FulfilledPlayerName, tradeSlotInfo.RequestMaterial);
            tradePanel.OnCollectButtonClicked += () =>
            {
                OnCollectTradeMaterial?.Invoke(tradeSlotNumber, tradeSlotInfo);
                tradePanel.Close();
            };
        }

        private void CreateTradeSlot(
            int tradeSlotNumber,
            TradeSlotInfo tradeSlotInfo,
            Action<TradeSlotInfo, int> onRequestItemSelect,
            Action<TradeSlotInfo, int> onOfferItemSelect)
        {
            var tradePanel = Instantiate(tradePanelPrefab, tradePanelContainer);
            var requestButtonData = new SelectItemButton.InitData(
                            itemSprite: tradeSlotInfo.RequestMaterial?.GetSprite(),
                            onItemButtonClicked: () =>
                            {
                                if (currentItemSelectionUI != null) return;

                                var requestMaterialIndex = GetMaterialIndex(tradeSlotInfo.RequestMaterial);
                                currentItemSelectionUI = CreateMaterialSelectionUI(
                                    clickedButton: tradePanel.GetRequestButton(),
                                    selectingIndex: requestMaterialIndex,
                                    selectionTitle: $"Request {tradeSlotNumber}",
                                    isForOfferSlot: false,
                                    onItemSelect: selectItemIndex =>
                                    {
                                        onRequestItemSelect?.Invoke(tradeSlotInfo, selectItemIndex);
                                        tradePanel.RefreshTradeUI();
                                    });
                            });
            var offerButtonData = new SelectItemButton.InitData(
                            itemSprite: tradeSlotInfo.OfferMaterial?.GetSprite(),
                            onItemButtonClicked: () =>
                            {
                                if (currentItemSelectionUI != null) return;

                                var offerMaterialIndex = GetMaterialIndex(tradeSlotInfo.OfferMaterial);
                                currentItemSelectionUI = CreateMaterialSelectionUI(
                                    clickedButton: tradePanel.GetOfferButton(),
                                    selectingIndex: offerMaterialIndex,
                                    selectionTitle: $"Offer {tradeSlotNumber}",
                                    isForOfferSlot: true,
                                    onItemSelect: selectItemIndex =>
                                    {
                                        onOfferItemSelect?.Invoke(tradeSlotInfo, selectItemIndex);
                                        tradePanel.RefreshTradeUI();
                                    });
                            });
            tradePanel.Init(
                tradePanelNumber: tradeSlotNumber,
                requestButtonData: requestButtonData,
                offerButtonData: offerButtonData
            );
        }

        private void InitMaterialVisualizations()
        {
            foreach (var material in materials)
            {
                var visualization = Instantiate(heldMaterialVisualizationPrefab, materialVisualizationContainer);
                visualization.Setup(material.GetSprite(), $"x{material.GetQuantity()}");
                heldMaterialVisualizations.Add(visualization);
            }
        }

        private int GetMaterialIndex(IMaterial material)
        {
            if (material == null) return -1;
            return materials.FindIndex(m => m.GetID() == material.GetID());
        }

        private ItemSelectionUI CreateMaterialSelectionUI(
            SelectItemButton clickedButton,
            int selectingIndex,
            string selectionTitle,
            bool isForOfferSlot,
            Action<int> onItemSelect)
        {
            var itemSelectionUI = UIManager.Instance.Create<ItemSelectionUI>();

            itemSelectionUI.Init(
                defaultSelectingItemIndex: selectingIndex,
                selectionTitle: selectionTitle,
                itemSprites: materials.ToArray(),
                onSelectButtonClicked: () =>
                {
                    var itemIndex = itemSelectionUI.SelectingItemIndex;
                    var material = materials[itemIndex];

                    var canSelect = !isForOfferSlot || (isForOfferSlot && material.GetQuantity() > 0);
                    if (canSelect)
                    {
                        clickedButton.SetSprite(material.GetSprite());
                        onItemSelect.Invoke(itemIndex);
                        itemSelectionUI.Close();
                    }
                    else
                    {
                        var popUI = UIManager.Instance.Create<MessagePopupUI>();
                        popUI.Setup(
                            "Insufficient Material!",
                            "This order requires 1 material.\nPlease play the game to get more materials!",
                            "Okay",
                            popUI.Close
                        );
                    }
                }
            );

            return itemSelectionUI;
        }
    }
}
