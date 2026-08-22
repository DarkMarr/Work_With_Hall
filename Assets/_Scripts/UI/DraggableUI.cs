using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace QuizGame.UI
{
    public class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action OnStartDrag;
        public event Action OnEndDrag;

        public bool IsDragging { get; private set; }

        [SerializeField]
        private bool enableDrag = true;

        private Vector2 dragOffset;

        public Vector2 GetTouchPosition() => Touch.activeTouches.Count > 0 ? Touch.activeTouches[0].screenPosition : Vector2.zero;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsDragging && enableDrag) return;

            IsDragging = true;
            dragOffset = (Vector2)transform.position - GetTouchPosition();
            OnStartDrag?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsDragging && enableDrag) return;

            IsDragging = false;
            OnEndDrag?.Invoke();
        }

        public void SetDragEnable(bool isEnable)
        {
            enableDrag = isEnable;
        }

        private void Update()
        {
            if (IsDragging && enableDrag)
            {
                transform.position = GetTouchPosition() + dragOffset;
            }
        }
    }
}
