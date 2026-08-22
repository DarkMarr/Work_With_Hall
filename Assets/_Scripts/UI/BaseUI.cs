using System;
using System.Collections.Generic;
using Crystal;
using UnityEngine;

namespace QuizGame.UI
{
    [RequireComponent(typeof(RectTransform), typeof(SafeArea))]
    public abstract class BaseUI : MonoBehaviour
    {
        public event Action OnClosed;
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnCreated;

        private List<BaseUI> childrenUI = new List<BaseUI>();

        private bool isClosed;

        protected virtual void Awake()
        {
            OnCreated?.Invoke();
        }

        protected virtual void OnValidate()
        {
            SetRectTransformToDefault();
        }

        public void SetRectTransformToDefault()
        {
            var targetRectTransform = GetComponent<RectTransform>();
            if (targetRectTransform == null)
            {
                Debug.LogError("Target RectTransform is not assigned!", this);
                return;
            }
            targetRectTransform.anchorMin = new Vector2(0f, 0f);
            targetRectTransform.anchorMax = new Vector2(1f, 1f);
            targetRectTransform.pivot = new Vector2(0.5f, 0.5f);
            targetRectTransform.offsetMin = Vector2.zero;
            targetRectTransform.offsetMax = Vector2.zero;
            targetRectTransform.localScale = Vector3.one;
        }

        public virtual void Close()
        {
            if (isClosed) return;

            isClosed = true;
            UIManager.Instance.CloseUI(this);

            if (childrenUI.Count > 0)
            {
                CloseAllChildren();
            }
            OnClosed?.Invoke();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow?.Invoke();
        }

        public void CloseAllChildren()
        {
            foreach (var ui in childrenUI)
            {
                ui?.Close();
            }
            childrenUI.Clear();
        }

        public void CloseChild(BaseUI childUI)
        {
            if (childrenUI.Contains(childUI))
            {
                childUI.Close();
                childrenUI.Remove(childUI);
            }
            else
            {
                Debug.LogError($"[BaseUI] {childUI} is not a child of {this}");
            }
        }

        public void AddChild(BaseUI childUI)
        {
            if (!childrenUI.Contains(childUI))
            {
                childrenUI.Add(childUI);
            }
            else
            {
                Debug.LogError($"[BaseUI] {childUI} is already be a child of {this}");
            }
        }
    }
}
