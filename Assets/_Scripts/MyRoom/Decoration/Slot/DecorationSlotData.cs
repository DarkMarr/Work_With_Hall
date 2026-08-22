using System;
using Newtonsoft.Json;

namespace QuizGame.MyRoom.Decoration
{
    [Serializable]
    public struct DecorationSlotData
    {
        [JsonProperty("slot_id")]
        public string SlotID { get; private set; }

        [JsonProperty("item_id")]
        public string ItemID { get; private set; }

        public DecorationSlotData(string slotID, string itemID)
        {
            SlotID = slotID;
            ItemID = itemID;
        }
    }
}
