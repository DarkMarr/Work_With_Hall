using NaughtyAttributes;
using QuizGame.Item.Interfaces;
using UnityEngine;

namespace QuizGame.Item
{
    public abstract class BaseItemSO : ScriptableObject, IItem
    {
        protected abstract ItemType ItemType { get; }
        protected abstract ItemTier ItemTier { get; }

        [SerializeField]
        private string itemID;

        [SerializeField, ShowAssetPreview]
        private Sprite itemSprite;

        private void OnValidate()
        {
            if (itemID == string.Empty)
            {
                itemID = name;
            }
        }

        public ItemType GetItemType() => ItemType;

        public string GetID() => itemID;

        public virtual string GetName() => itemID;

        public override string ToString() => $"{GetName()}({GetID()})";

        public Sprite GetSprite() => itemSprite;

        public ItemTier GetItemTier() => ItemTier;
    }
}
