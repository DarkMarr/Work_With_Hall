using NaughtyAttributes;
using QuizGame.Interfaces;
using QuizGame.Resources;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Character
{
    /// <summary>
    /// Defines a playable character (e.g. Rabbit, Cat, Dog).
    /// References the base prefab that contains a CharacterSpriteMixer for outfit mixing.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "QuizGame/Character/Character", order = 0)]
    public class CharacterInfoSO : ScriptableObject, IHasID, IHasSprite, IHasName
    {
        [SerializeField, ReadOnly]
        private string characterID;

        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private GameObject characterPrefab;

        [SerializeField, ShowAssetPreview]
        private Sprite previewSprite;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(characterID))
            {
                characterID = name;
            }
        }

        public string GetID() => characterID;
        public string GetName() => localizedName.IsEmpty ? characterID : localizedName.GetLocalizedString();
        public Sprite GetSprite() => previewSprite;
        public GameObject GetCharacterPrefab() => characterPrefab;
    }
}
