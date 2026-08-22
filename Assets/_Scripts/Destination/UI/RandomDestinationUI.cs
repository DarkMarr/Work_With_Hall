using QuizGame.UI;
using UnityEngine;

namespace QuizGame.Destination.UI
{
    public class RandomDestinationUI : BaseUI
    {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private PlayerSelectedDestinationVisualize selectedDestinationVisualizePrefab;

        public void Init(string[] playerNames, IDestinationInfo[] destinationInfo, int matchSelectDestinationIndex)
        {
            for (int i = 0; i < destinationInfo.Length; i++)
            {
                var visualize = Instantiate(selectedDestinationVisualizePrefab, container);
                var destination = destinationInfo[i];
                var playerName = playerNames[i];
                visualize.Init(
                    playerName: playerName,
                    destinationInfo: destination
                );
                visualize.SetActiveHighlight(i == matchSelectDestinationIndex);
            }
        }
    }
}
