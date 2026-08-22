using Newtonsoft.Json;

namespace QuizGame.MyRoom.Trade
{
    public struct TradeSlotData
    {
        [JsonProperty("offer_material_id")]
        public string OfferMaterialID;

        [JsonProperty("request_material_id")]
        public string RequestMaterialID;

        [JsonProperty("fulfilled_player_name")]
        public string FulfilledPlayerName;

        public TradeSlotData(string offerMaterialID, string requestMaterialID, string fulfilledPlayerName)
        {
            OfferMaterialID = offerMaterialID;
            RequestMaterialID = requestMaterialID;
            FulfilledPlayerName = fulfilledPlayerName;
        }
    }
}
