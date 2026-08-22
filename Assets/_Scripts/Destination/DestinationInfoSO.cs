using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Destination
{
    [CreateAssetMenu(fileName = "NewDestination", menuName = "QuizGame/Destination", order = 1)]
    public class DestinationInfoSO : ScriptableObject, IDestinationInfo
    {
        [SerializeField, ReadOnly]
        private string destinationID;

        [SerializeField]
        private DestinationType destinationType;

        [SerializeField]
        private GameObject npcPrefab;

        [SerializeField, ShowAssetPreview]
        private Sprite sprite;

        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private LocalizedString localizedDescription;

        [SerializeField]
        private DestinationItemReward destinationItemRewards;

        private void OnValidate()
        {
            destinationID = destinationType.ToString();
        }

        public string GetID() => destinationID;
        public Sprite GetSprite() => sprite;
        public string GetName() => localizedName.GetLocalizedString();
        public string GetDescription() => localizedDescription.GetLocalizedString();
        public DestinationItemReward GetItemRewardInDestination() => destinationItemRewards;
        public DestinationType GetDestinationType() => destinationType;
        public GameObject GetNPCPrefab() => npcPrefab;
    }
}
