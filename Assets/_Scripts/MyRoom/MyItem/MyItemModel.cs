using System.Collections.Generic;
using QuizGame.Item.Interfaces;

namespace QuizGame.MyRoom.MyItem
{
    public class MyItemModel
    {
        public List<IItem> TrophyItems { get; private set; }
        public List<IItem> EquipmentItems { get; private set; }
        public List<IItem> ConsumableItems { get; private set; }

        public void Init(List<IItem> tropyItems, List<IItem> equipmentItems, List<IItem> consumableItems)
        {
            TrophyItems = tropyItems;
            EquipmentItems = equipmentItems;
            ConsumableItems = consumableItems;
        }

        public void RemoveItem(IItem item)
        {
            switch (item.GetItemType())
            {
                case ItemType.Consumable:
                    ConsumableItems.Remove(item);
                    break;
                    
                case ItemType.Decoration:
                    TrophyItems.Remove(item);
                    break;

                case ItemType.Equipment:
                    EquipmentItems.Remove(item);
                    break;
            }
        }
    }
}
