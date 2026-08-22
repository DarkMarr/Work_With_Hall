using System;
using Newtonsoft.Json;

namespace QuizGame.Item
{
    [Serializable]
    public class ItemWithQuantityPairData
    {
        [JsonProperty("item_id")]
        public string ItemID;

        [JsonProperty("item_type")]
        public int ItemType;

        [JsonProperty("quantity")]
        public int Quantity;
    }
}
