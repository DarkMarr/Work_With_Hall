using System;
using UnityEngine.U2D.Animation;

namespace QuizGame.Character
{
    [Serializable]
    public class CharacterSpriteMixerCategory
    {
        public CharacterPartType PartType;

        public string DisplayTitle;

        /// <summary>
        /// The actual category name as reported by the sprite library at runtime.
        /// Different character libraries use different spellings ("Arm L", "Arm_Left", "Arm_Dec", ...)
        /// so we keep the real name here instead of relying on the canonical mapping.
        /// </summary>
        public string ActualCategoryName;

        public SpriteResolver Resolver;
    }
}
