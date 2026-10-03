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
        public static string GetDataInSlotTempDataJson() => @"[
            {
                ""slot_id"": ""ShelfTrophy_01"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""ShelfTrophy_02"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""ShelfTrophy_03"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""ShelfTrophy_04"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""WallTrophy_01"",
                ""item_id"": """"
            },
            {
                ""slot_id"": ""FloorTrophy_01"",
                ""item_id"": ""33101""
            },
            {
                ""slot_id"": ""FloorTrophy_02"",
                ""item_id"": ""33102""
            },
            {
                ""slot_id"": ""FloorTrophy_03"",
                ""item_id"": ""33103""
            }
        ]";
    }
}
