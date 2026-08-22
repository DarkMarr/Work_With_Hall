using System;
using Newtonsoft.Json;
using QuizGame.Item;

namespace QuizGame.MainMenu.UI
{
    [Serializable]
    public class SystemMessageDetailsData
    {
        [JsonProperty("body_message")]
        public string BodyMessage;

        [JsonProperty("is_already_accepted")]
        public bool isAlreadyAccepted;

        [JsonProperty("giving_items")]
        public ItemWithQuantityPairData[] givingItems;
    }
}
