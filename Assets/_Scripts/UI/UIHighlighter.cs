using UnityEngine;


namespace QuizGame.UI
{
    /// <summary>
    /// This highlighter take width and hight from rect and locate behind the target.
    /// </summary>
    public class UIHighlighter : MonoBehaviour
    {
        [SerializeField]
        private RectTransform highlighter;

        [SerializeField]
        private bool isKeepUpdatePosition;

        [SerializeField]
        private float widthPadding;

        [SerializeField]
        private float heightPadding;

        private RectTransform targetRect;

        private void Update()
        {
            if (targetRect == null || !isKeepUpdatePosition) return;
            UpdatePosition();
        }

        public void SetTarget(RectTransform targetRect)
        {
            if (targetRect == null)
            {
                Debug.LogWarning("Target RectTransform is null. Cannot set highlighter position/size.", this);
                return;
            }
            this.targetRect = targetRect;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            var targetSize = new Vector2(
                targetRect.rect.width + widthPadding,
                targetRect.rect.height + heightPadding
            );
            highlighter.sizeDelta = targetSize;
            highlighter.position = targetRect.position;
        }

        public void SetActive(bool isActive)
        {
            highlighter.gameObject.SetActive(isActive);
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }
    }
}
