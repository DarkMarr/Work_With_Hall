using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public abstract class BaseCategorySelectionButtons : MonoBehaviour
    {
        public event Action<int> OnIDChange;
        public abstract int ContentCount { get; }
        public int SelectingIndex { get; protected set; }

        [SerializeField]
        protected bool IsCircleIndex;

        [SerializeField]
        private Button rightButton;

        [SerializeField]
        private Button leftButton;

        protected virtual void Awake()
        {
            rightButton?.onClick.AddListener(OnRightButtonClicked);
            leftButton?.onClick.AddListener(OnLeftButtonClicked);
        }

        public virtual void OnRightButtonClicked()
        {
            var nextSelectingIndex = SelectingIndex + 1;
            if (nextSelectingIndex >= ContentCount)
            {
                nextSelectingIndex = IsCircleIndex ? 0 : ContentCount - 1;
            }

            if (nextSelectingIndex != SelectingIndex)
            {
                SelectingIndex = nextSelectingIndex;
                RefreshUIAndRaiseEvent();
            }
        }

        public virtual void OnLeftButtonClicked()
        {
            var previousSelectingIndex = SelectingIndex - 1;
            if (previousSelectingIndex < 0)
            {
                previousSelectingIndex = IsCircleIndex ? ContentCount - 1 : 0;
            }

            if (previousSelectingIndex != SelectingIndex)
            {
                SelectingIndex = previousSelectingIndex;
                RefreshUIAndRaiseEvent();
            }
        }

        public virtual void RefreshUIAndRaiseEvent()
        {
            OnIDChange?.Invoke(SelectingIndex);
        }
    }
}
