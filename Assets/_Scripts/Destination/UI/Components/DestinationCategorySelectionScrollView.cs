using UnityEngine;

namespace QuizGame.Destination.UI
{
    public class DestinationCategorySelectionScrollView : MonoBehaviour
    {
        [SerializeField]
        private DestinationVisualization destinationVisualizationPrefab;

        [SerializeField]
        private Transform contentContainer;

        public void Init(IDestinationInfo[] destinationInfos)
        {
            foreach (Transform child in contentContainer)
                Destroy(child.gameObject);

            foreach (var destinationInfo in destinationInfos)
            {
                var destinationVisualization = Instantiate(destinationVisualizationPrefab, contentContainer);
                destinationVisualization.Init(destinationInfo);
            }
        }
    }
}
