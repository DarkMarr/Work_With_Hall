using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Destination
{
    /// <summary>
    /// Defines an NPC that appears in the gameplay scene as a quiz narrator/opponent.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNpc", menuName = "QuizGame/Destination/NPC", order = 2)]
    public class NpcInfoSO : ScriptableObject, Resources.IHasID
    {
        [SerializeField, ReadOnly]
        private string npcID;

        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private GameObject npcPrefab;

        [SerializeField, ShowAssetPreview]
        private Sprite previewSprite;

        [SerializeField]
        private LocalizedString localizedDescription;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(npcID))
            {
                npcID = name;
            }
        }

        public string GetID() => npcID;
        public string GetName() => localizedName.IsEmpty ? npcID : localizedName.GetLocalizedString();
        public string GetDescription() => localizedDescription.IsEmpty ? string.Empty : localizedDescription.GetLocalizedString();
        public Sprite GetSprite() => previewSprite;
        public GameObject GetNpcPrefab() => npcPrefab;
    }
}
