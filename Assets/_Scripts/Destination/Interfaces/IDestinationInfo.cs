using QuizGame.Interfaces;
using QuizGame.Resources;
using UnityEngine;

namespace QuizGame.Destination
{
    public interface IDestinationInfo : IHasSprite, IHasID
    {
        public string GetName();
        public string GetDescription();
        public DestinationItemReward GetItemRewardInDestination();
        public GameObject GetNPCPrefab();
    }
}
