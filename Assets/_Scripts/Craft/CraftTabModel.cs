using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizGame.Craft
{
    /// <summary>
    /// One tab of the craft bench: a name, and the categories of thing it can make.
    ///
    /// A category used to be a <see cref="DecorationType"/>, which is why garments could not be put
    /// on the bench even though the item sheet prices every one of them. A category is now just a
    /// label and a way to fetch its items, so a tab can be built from anything craftable.
    /// </summary>
    public class CraftTabModel
    {
        /// <summary>A row of tabs within a tab: "Small", "Big", or "Head", "Body".</summary>
        public class Category
        {
            public string Label { get; private set; }

            private readonly Func<List<ICraftableItem>> fetch;
            private List<ICraftableItem> cached;

            public Category(string label, Func<List<ICraftableItem>> fetch)
            {
                Label = label;
                this.fetch = fetch;
            }

            /// <summary>
            /// Read when the tab is opened rather than when this list is first built, because the
            /// resource managers are singletons that may not exist yet at static initialisation.
            /// </summary>
            public List<ICraftableItem> GetItems()
            {
                if (cached == null) cached = fetch() ?? new List<ICraftableItem>();
                return cached;
            }
        }

        private readonly string tabName;
        private readonly List<Category> categories;

        public CraftTabModel(string tabName, List<Category> categories)
        {
            this.tabName = tabName;
            this.categories = categories;
        }

        public string GetName() { return tabName; }

        public IList<Category> GetCategories() { return categories; }

        /// <summary>Every decoration of one type, as something the bench can offer.</summary>
        private static Category Decorations(string label, DecorationType type)
        {
            return new Category(label, () =>
            {
                var decorations = DecorationItemResourceManager.Instance.GetDecorationByType(type);
                if (decorations == null)
                {
                    Debug.LogWarning("[CraftTabModel] No decoration type of " + type);
                    return new List<ICraftableItem>();
                }
                return decorations.Cast<ICraftableItem>().ToList();
            });
        }

        /// <summary>
        /// Every garment for one body slot. The slot is spelled as the outfit assets spell it, which
        /// is a CharacterPartType name, while the label is what the sheet calls it.
        /// </summary>
        private static Category Garments(string label, string slotName)
        {
            return new Category(label, () => OutfitItemResourceManager.Instance.GetAllResources()
                .Where(garment => string.Equals(garment.GetSlotName(), slotName, StringComparison.OrdinalIgnoreCase))
                .Cast<ICraftableItem>()
                .ToList());
        }

        public static List<CraftTabModel> CraftTabList = new List<CraftTabModel>()
        {
            new CraftTabModel("Room", new List<Category>
            {
                Decorations("Room", DecorationType.Room),
            }),

            // The item sheet names four kinds of decoration, by the space they take up rather than
            // the furniture they sit on.
            new CraftTabModel("Decor", new List<Category>
            {
                Decorations("Small", DecorationType.Small),
                Decorations("Big", DecorationType.Big),
                Decorations("Window", DecorationType.Window),
                Decorations("Suitcase", DecorationType.Suitcase),
            }),

            // The sheet's craft list prices all four garment slots, so the bench offers them.
            // Hand is spelled Prop in the assets, which is the CharacterPartType the art uses.
            new CraftTabModel("Outfit", new List<Category>
            {
                Garments("Head", "HeadDecoration"),
                Garments("Body", "BodyDecoration"),
                Garments("Hand", "Prop"),
                Garments("Back", "BackDecoration"),
            }),
        };
    }
}
