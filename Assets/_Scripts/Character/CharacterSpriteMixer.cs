using System.Collections.Generic;
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
            spriteMixerByPartType = new Dictionary<CharacterPartType, CharacterSpriteMixerCategory>(categories.Length);
            foreach (var category in categories)
            {
                if (category != null && category.Resolver != null)
                {
                    spriteMixerByPartType[category.PartType] = category;
                }
            }
        }

        [ContextMenu("Auto Find ResolveSprite In Children")]
        void AutoFindResolveSpriteInChildren()
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
                    Resolver = resolver
                };
            }
        }

        public void SetPartLabel(CharacterPartType partType, string label)
        {
            if (spriteMixerByPartType.TryGetValue(partType, out var category) && category.Resolver != null)
            {
                var categoryName = CharacterSpriteUtilities.GetCategoryNameByPartType(partType);
                category.Resolver.SetCategoryAndLabel(categoryName, label);
            }
        }
    }
}

