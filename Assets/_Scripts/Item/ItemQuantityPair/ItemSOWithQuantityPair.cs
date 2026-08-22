using System;
using QuizGame.Item.Interfaces;
using UnityEngine;

namespace QuizGame.Item
{
    [Serializable]
    public class ItemSOWithQuantityPair : IQuantifiableItem
    {
        public BaseItemSO Item;
        public int Quantity;

        public string GetID() => Item.GetID();

        public ItemTier GetItemTier() => Item.GetItemTier();

        public ItemType GetItemType() => Item.GetItemType();

        public string GetName() => Item.GetName();

        public int GetQuantity() => Quantity;

        public override string ToString() => $"{GetName()}({GetID()}) x{GetQuantity()}";

        public Sprite GetSprite() => Item.GetSprite();

        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }
    }
}
