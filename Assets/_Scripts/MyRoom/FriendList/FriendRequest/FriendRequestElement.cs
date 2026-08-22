using System;
using QuizGame.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.FriendList
{
    [RequireComponent(typeof(RectTransform))]
    public class FriendRequestElement : MonoBehaviour, IHasRectTransform
    {
        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private Button acceptButton;

        [SerializeField]
        private Button rejectButton;

        private event Action acceptButtonClickedEvent;
        private event Action rejectButtonClickedEvent;

        private void OnValidate()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }

        private void Awake()
        {
            acceptButton.onClick.AddListener(() => acceptButtonClickedEvent?.Invoke());
            rejectButton.onClick.AddListener(() => rejectButtonClickedEvent?.Invoke());
        }

        public void Setup(string name, string rank, Action onAcceptButtonClicked, Action onRejectButtonClicked)
        {
            nameText.text = name;
            rankText.text = rank;
            acceptButtonClickedEvent = onAcceptButtonClicked;
            rejectButtonClickedEvent = onRejectButtonClicked;
        }

        public RectTransform GetRectTransform() => rectTransform;
    }
}
