using System.Collections.Generic;
using System.Linq;
using QuizGame.Utilities;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace QuizGame.Character
{
    public class CharacterSpriteMixer : MonoBehaviour
    {
        [SerializeField]
        private SpriteLibrary spriteLibrary;

        [SerializeField]
        private CharacterSpriteMixerCategory[] categories;

        private Dictionary<CharacterPartType, CharacterSpriteMixerCategory> spriteMixerByPartType;

        public SpriteLibraryAsset GetLibraryAsset() => spriteLibrary.spriteLibraryAsset;
        public CharacterSpriteMixerCategory[] GetCategories() => categories;

        void Awake()
        {
            BuildPartTypeDictionary();
        }

        private void BuildPartTypeDictionary()
        {
            // If categories were not set up in the inspector, auto-populate from resolvers at runtime.
            if (categories == null || categories.Length == 0)
            {
                AutoFindResolveSpriteInChildren();
            }

            spriteMixerByPartType = new Dictionary<CharacterPartType, CharacterSpriteMixerCategory>(categories.Length);
            foreach (var category in categories)
            {
                if (category != null && category.Resolver != null)
                {
                    // Capture the actual category name from the resolver if not already set.
                    if (string.IsNullOrEmpty(category.ActualCategoryName))
                    {
                        category.ActualCategoryName = category.Resolver.GetCategory();
                    }
                    spriteMixerByPartType[category.PartType] = category;
                }
            }
        }

        [ContextMenu("Auto Find ResolveSprite In Children")]
        public void AutoFindResolveSpriteInChildren()
        {
            var spriteResolvers = transform.GetComponentsInChildren<SpriteResolver>();
            categories = new CharacterSpriteMixerCategory[spriteResolvers.Length];
            for (int i = 0; i < spriteResolvers.Length; i++)
            {
                var resolver = spriteResolvers[i];
                var categoryName = resolver.GetCategory();
                var partType = CharacterSpriteUtilities.GetPartTypeByCategoryName(categoryName);
                categories[i] = new CharacterSpriteMixerCategory
                {
                    PartType = partType,
                    DisplayTitle = StringUtilities.SplitCamelCase(partType.ToString()),
                    ActualCategoryName = categoryName,
                    Resolver = resolver
                };
            }
        }

        public void SetPartLabel(CharacterPartType partType, string label)
        {
            if (spriteMixerByPartType.TryGetValue(partType, out var category) && category.Resolver != null)
            {
                // Use the actual category name from the library, falling back to canonical if needed.
                var categoryName = !string.IsNullOrEmpty(category.ActualCategoryName)
                    ? category.ActualCategoryName
                    : CharacterSpriteUtilities.GetCategoryNameByPartType(partType);
                category.Resolver.SetCategoryAndLabel(categoryName, label);
            }
        }

        /// <summary>
        /// Checks whether the sprite library has a label for the given part type.
        /// </summary>
        public bool HasLabel(CharacterPartType partType, string label)
        {
            if (string.IsNullOrEmpty(label)) return false;
            var labels = GetAvailableLabels(partType);
            return labels != null && labels.Contains(label);
        }

        /// <summary>
        /// Returns all available labels for a part type from the sprite library.
        /// </summary>
        public string[] GetAvailableLabels(CharacterPartType partType)
        {
            if (!spriteMixerByPartType.TryGetValue(partType, out var category))
            {
                return null;
            }

            var categoryName = !string.IsNullOrEmpty(category.ActualCategoryName)
                ? category.ActualCategoryName
                : CharacterSpriteUtilities.GetCategoryNameByPartType(partType);

            var libraryAsset = GetLibraryAsset();
            if (libraryAsset == null) return null;

            return libraryAsset.GetCategoryLabelNames(categoryName).ToArray();
        }
    }
}
