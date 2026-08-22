using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace QuizGame.Gameplay.UI
{
    public class PlayerGameResultInfoVisualization : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI pointText;

        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private Image rankImage;

        [SerializeField]
        private Button addFriendButton;

        [SerializeField]
        private TextMeshProUGUI addFriendButtonText;

        //TODO: Render player characters

        public void Init(Sprite rankSprite, string rankName, string name, int point)
        {
            nameText.text = name;
            pointText.text = $"{point} Points";
            rankText.text = rankName;
            rankImage.sprite = rankSprite;

            var isFriendOrSentRequested = IsFriendOrSentRequested();
            addFriendButton.enabled = !isFriendOrSentRequested;
            addFriendButtonText.text = isFriendOrSentRequested ? "Requested" : "Add friend"; //TODO: Replace with localization

            addFriendButton.onClick.AddListener(() =>
            {
                if (!isFriendOrSentRequested)
                {
                    addFriendButton.enabled = false;
                    addFriendButtonText.text = "Requested";
                    Debug.Log($"Send friend request to: {name}");
                    //TODO: [Network] Do the friend request
                }
            });
        }

        public bool IsFriendOrSentRequested() => Random.Range(0, 2) == 0; //TODO: [Network] check is this your friend?

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
