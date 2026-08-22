using System;
using QuizGame.Material;
using QuizGame.UI;
using UnityEngine;

namespace QuizGame.MyRoom.Trade
{
    [Serializable]
    public class TradeController
    {
        public event Action OnTradeClose;

        private TradeModel model;
        private TradeUI tradeUI;

        public void Init(TradeModel tradeModel)
        {
            model = tradeModel;
            ShowTradeUI();
        }

        private void ShowTradeUI()
        {
            tradeUI = UIManager.Instance.Create<TradeUI>();
            tradeUI.Init(
                tradeSlotInfos: model.GetTradeSlotInfos(), //TODO: [Network] May be add date validation which define which trade slot available at the moment
                materials: model.GetMaterials(),
                onOfferItemSelect: (slot, selectItemIndex) =>
                {
                    if (slot.OfferMaterial != null)
                    {
                        model.AddMaterial(slot.OfferMaterial.GetID(), 1); //Give back the material in offer slot
                    }

                    var material = model.GetMaterials()[selectItemIndex];
                    if (model.IsMaterialEnough(material.GetID(), 1)) //We don't show material with 0 quantity in UI anyway
                    {
                        model.RemoveMaterial(material.GetID(), 1); //Take the material that used in offer slot
                        slot.SetOfferMaterial(material);
                        tradeUI.RefreshMaterialsVisualization();
                    }
                },
                onRequestItemSelect: (slot, selectItemIndex) =>
                {
                    var material = model.GetMaterials()[selectItemIndex];
                    slot.SetRequestMaterial(material);
                },
                onBackButtonClick: CloseTrade
            );
            tradeUI.OnCollectTradeMaterial += CollectTradeMaterial;
        }

        private void CollectTradeMaterial(int tradeSlotNumber, TradeSlotInfo tradeSlotInfo)
        {
            //TODO: [Network] collect a request material to database
            Debug.Log($"[Trade] At trade slot number {tradeSlotNumber}, User has collect request materialID: {tradeSlotInfo.RequestMaterial.GetID()}, which offer with materialID: {tradeSlotInfo.OfferMaterial.GetID()}");
            model.AddMaterial(tradeSlotInfo.RequestMaterial.GetID(), 1);
            model.RemoveTradeSlotInfo(tradeSlotInfo); //TODO: [Network] Remove that trade slot
            tradeUI.RefreshMaterialsVisualization();
        }

        private void CloseTrade()
        {
            tradeUI.Close();
            OnTradeClose?.Invoke();

            var slotDatasToJson = model.GetAllSlotDatasAsJson();
            Debug.Log("[Trade] Save trade to json: " + slotDatasToJson); //TODO: [Network] Save data to database

            var materialDatasToJson = model.GetAllSlotDatasAsJson();
            Debug.Log("[Trade] Save materials to json: " + materialDatasToJson); //TODO: [Network] Save data to database
        }
    }
}
