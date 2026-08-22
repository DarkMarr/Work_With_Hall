using System;
using QuizGame.Item.Interfaces;
using UnityEngine;

namespace QuizGame.Item
{
    [Serializable]
    public class ItemWithQuantityPair : IQuantifiableItem
    {
        private IItem Item;
        private int Quantity;

        public ItemWithQuantityPair(IItem item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public string GetID() => Item != null ? Item.GetID() : "";

        public ItemTier GetItemTier() => Item != null ? Item.GetItemTier() : ItemTier.NoTier;

        public ItemType GetItemType() => Item != null ? Item.GetItemType() : ItemType.Consumable;

        public string GetName() => Item != null ? Item.GetName() : "";

        public int GetQuantity() => Quantity;

        public override string ToString() => $"{GetName()}({GetID()}) x{GetQuantity()}";

        public Sprite GetSprite() => Item != null ? Item.GetSprite() : null;

        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }
    }
}
