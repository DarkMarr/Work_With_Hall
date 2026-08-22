using QuizGame.Destination;
using QuizGame.UI;
using System;
using UnityEngine;

namespace QuizGame.MyDiary.UI
{
    public class TravelDataTabUI : BaseUI
    {
        [SerializeField]
        private DestinationCategorySelectionButtons destinationSelection;

        public DestinationRewardInfoUI RewardInfoUI;

        public void Setup(IDestinationInfo[] avaliableDestinations, Action<IDestinationInfo> OnDestinationChanged)
        {
            destinationSelection.Init(avaliableDestinations, (visualize, information) =>
            {
                visualize.Init(information);
            });

            destinationSelection.OnIDChange += (index) => OnDestinationChanged.Invoke(avaliableDestinations[index]);
        }
    }
}