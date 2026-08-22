using System;
using QuizGame.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.FriendList
{
    [RequireComponent(typeof(RectTransform))]
    public class FriendListElement : MonoBehaviour, IHasRectTransform
    {
        public enum State
        {
            Home,
            Delete
        }

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private Button homeButton;

        [SerializeField]
        private Button deleteButton;

        private event Action homeButtonClickedEvent;
        private event Action deleteButtonClickedEvent;

        private void OnValidate()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }

        private void Awake()
        {
            homeButton.onClick.AddListener(() => homeButtonClickedEvent?.Invoke());
            deleteButton.onClick.AddListener(() => deleteButtonClickedEvent?.Invoke());
        }

        public void Setup(string name, string rank, Action onHomeButtonClicked, Action onDeleteButtonClicked)
        {
            nameText.text = name;
            rankText.text = rank;
            homeButtonClickedEvent = onHomeButtonClicked;
            deleteButtonClickedEvent = onDeleteButtonClicked;
        }

        public RectTransform GetRectTransform() => rectTransform;

        public void ChangeState(State state)
        {
            homeButton.gameObject.SetActive(state == State.Home);
            deleteButton.gameObject.SetActive(state == State.Delete);
        }
    }
}
