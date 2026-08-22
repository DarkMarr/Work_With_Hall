using System;
using UnityEngine.U2D.Animation;

namespace QuizGame.Character
{
    [Serializable]
    public class CharacterSpriteMixerCategory
    {
        public CharacterPartType PartType;

        public string DisplayTitle;

        public SpriteResolver Resolver;
    }
}
