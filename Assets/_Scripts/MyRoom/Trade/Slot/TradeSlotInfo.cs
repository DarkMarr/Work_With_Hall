using System.Collections.Generic;
using Newtonsoft.Json;
using QuizGame.Material;

namespace QuizGame.MyRoom.Trade
{
    public class TradeSlotInfo
    {
        public IMaterial OfferMaterial { get; private set; }
        public IMaterial RequestMaterial { get; private set; }
        public string FulfilledPlayerName { get; private set; }

        public TradeSlotInfo(IMaterial offerMaterial, IMaterial requestMaterial, string fulfilledPlayerName)
        {
            OfferMaterial = offerMaterial;
            RequestMaterial = requestMaterial;
            FulfilledPlayerName = fulfilledPlayerName;
        }

        public bool IsTradeCompleted() => FulfilledPlayerName != string.Empty;

        public void SetOfferMaterial(IMaterial offerMaterial)
        {
            OfferMaterial = offerMaterial;
        }

        public void SetRequestMaterial(IMaterial requestMaterial)
        {
            RequestMaterial = requestMaterial;
        }

        public static List<TradeSlotInfo> FromJson(string json)
        {
            var tradeDatas = JsonConvert.DeserializeObject<List<TradeSlotData>>(json);
            var tradeSlotInfos = new List<TradeSlotInfo>();
            foreach (var tradeData in tradeDatas)
            {
                var offerMaterial = string.IsNullOrEmpty(tradeData.OfferMaterialID)
                    ? null : MaterialResourceManager.Instance.GetResource(tradeData.OfferMaterialID);
                var requestMaterial = string.IsNullOrEmpty(tradeData.RequestMaterialID)
                    ? null : MaterialResourceManager.Instance.GetResource(tradeData.RequestMaterialID);
                tradeSlotInfos.Add(new TradeSlotInfo(offerMaterial, requestMaterial, tradeData.FulfilledPlayerName));
            }
            return tradeSlotInfos;
        }
    }
}
