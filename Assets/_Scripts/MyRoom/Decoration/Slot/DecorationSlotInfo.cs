using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuizGame.MyRoom.Decoration
{
    public class DecorationSlotInfo
    {
        private string slotID;
        private IDecorationItem decorationItem;

        public DecorationSlotInfo(string slotID, IDecorationItem decorationItem)
        {
            this.slotID = slotID;
            SetItem(decorationItem);
        }

        public static List<DecorationSlotInfo> FromJson(string json)
        {
            var decorationSlotDatas = JsonConvert.DeserializeObject<List<DecorationSlotData>>(json);
            var playerDecorationSlots = new List<DecorationSlotInfo>();
            foreach (var decorationData in decorationSlotDatas)
            {
                var decorationInfo = decorationData.ItemID != string.Empty ? DecorationItemResourceManager.Instance.GetResource(decorationData.ItemID) : null;
                playerDecorationSlots.Add(new DecorationSlotInfo(decorationData.SlotID, decorationInfo));
            }
            return playerDecorationSlots;
        }

        public string GetSlotID() => slotID;
        public IDecorationItem GetDecorationItem() => decorationItem;

        public void SetItem(IDecorationItem decorationItem)
        {
            this.decorationItem = decorationItem;
        }
    }
}
