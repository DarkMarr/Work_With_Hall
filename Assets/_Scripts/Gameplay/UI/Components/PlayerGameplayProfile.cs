using TMPro;
using UnityEngine;

namespace QuizGame.Gameplay.UI
{
    public class PlayerGameplayProfile : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerNameText;

        [SerializeField]
        private TextMeshProUGUI playerPointText;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetPlayerName(string name)
        {
            playerNameText.text = name;
        }

        public void SetPlayerPoint(int point)
        {
            playerPointText.text = $"{point} pt";
        }
    }
}
