using System;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Item.Interfaces;

namespace QuizGame.MyRoom.Decoration
{
    public class DecorationModel
    {
        private List<DecorationSlotInfo> decorationSlotInfos;
        private Dictionary<string, IDecorationItem> installedSlotByID = new();
        private Dictionary<DecorationType, IDecorationItem[]> decorationsByType = new();

        public DecorationModel(Dictionary<DecorationType, IDecorationItem[]> byType, List<DecorationSlotInfo> decorationSlotInfos)
        {
            decorationsByType = byType;
            foreach (var slot in decorationSlotInfos)
            {
                installedSlotByID.Add(slot.GetSlotID(), slot.GetDecorationItem());
            }
            this.decorationSlotInfos = decorationSlotInfos;
        }

        public Dictionary<DecorationType, IDecorationItem[]> GetAvailableItemsByType() => decorationsByType;
        public DecorationSlotInfo GetSlotOwnerOfItem(IDecorationItem item) => GetAllSlots().First(d => d.GetDecorationItem() == item);
        public bool AnySlotOwnItem(IDecorationItem item) => GetAllSlots().Any(d => d.GetDecorationItem() == item);
        public List<DecorationSlotInfo> GetAllSlots() => decorationSlotInfos;

        public List<DecorationSlotData> GetAllSlotDatas() =>
            installedSlotByID.Select(p => new DecorationSlotData(p.Key, p.Value != null ? p.Value.GetID() : string.Empty)).ToList();

        public string GetItemIDInSlot(string slotID)
        {
            if (installedSlotByID.TryGetValue(slotID, out var itemInSlot))
            {
                return itemInSlot.GetID();
            }
            return null;
        }

        public IDecorationItem GetItemInSlot(string slotID)
        {
            if (installedSlotByID.TryGetValue(slotID, out var itemInSlot))
            {
                return itemInSlot;
            }
            return null;
        }

        public void SetItemInSlot(string slotID, IDecorationItem item)
        {
            if (installedSlotByID.ContainsKey(slotID))
            {
                installedSlotByID[slotID] = item;
                var targetSlot = decorationSlotInfos.First(x => x.GetSlotID() == slotID);
                var slotIndex = decorationSlotInfos.IndexOf(targetSlot);
                decorationSlotInfos[slotIndex].SetItem(item);
            }
        }

        public int[] GetIndicesOfItemEquippedInSlots(IItem[] itemList) => GetAllSlots()
                .Where(d => d.GetDecorationItem() != null)
                .Select(d => Array.FindIndex(itemList, i => i == d.GetDecorationItem()))
                .Where(idx => idx >= 0).ToArray();

        /// <summary>
        /// Every slot standing in the room has to appear here. <see cref="SetItemInSlot"/> only
        /// writes to ids it already knows, so a slot missing from this list can be tapped and
        /// decorated and nothing will happen.
        ///
        /// The eight ids match the slots Art named in pv_position_name.png: four on the shelf and
        /// table, one in the window, three on the floor.
        /// </summary>
        /// <summary>
        /// Every slot standing in the room has to appear here. <see cref="SetItemInSlot"/> only
        /// writes to ids it already knows, so a slot missing from this list can be tapped and
        /// decorated and nothing will happen.
        ///
        /// The eight ids match the slots Art named in pv_position_name.png. What starts in them is
        /// what the item sheet marks STARTER, which is one window view and one suitcase; the rest
        /// of the room is for the player to fill.
        /// </summary>
        public static string GetDataInSlotTempDataJson() => @"[
            {
                ""slot_id"": ""Small_01"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""Small_02"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""Small_03"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""Small_04"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""Window_01"",
                ""item_id"": ""311001""
            },
            {
                ""slot_id"": ""Big_01"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""Big_02"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""Suitcase_01"",
                ""item_id"": ""341001""
            }
        ]";
    }
}
