using TMPro;
using UnityEngine;

namespace QuizGame.Destination.UI
{
    public class PlayerSelectedDestinationVisualize : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerNameText;

        [SerializeField]
        private DestinationVisualization destinationVisualization;

        [SerializeField]
        private GameObject highlightGameObject;

        public void Init(string playerName, IDestinationInfo destinationInfo)
        {
            playerNameText.text = playerName;
            destinationVisualization.Init(destinationInfo);
        }

        public void SetActiveHighlight(bool isActive)
        {
            highlightGameObject.SetActive(isActive);
        }
    }
}
