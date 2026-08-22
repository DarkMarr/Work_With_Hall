using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public class ImageCategorySelectionButtons : BaseCategorySelectionButtons
    {
        public override int ContentCount => spriteContent.Length;

        [SerializeField]
        private Image imagePrefab;

        [SerializeField]
        private Transform collection;

        [SerializeField]
        private float animationSpeed;

        [SerializeField]
        private float gapX = 70f;

        private Sprite[] spriteContent;
        private List<Vector3> positions = new List<Vector3>();
        private List<Image> images = new List<Image>();

        private RectTransform collectionRect;

        protected override void Awake()
        {
            base.Awake();
            collectionRect = collection.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (positions == null || positions.Count <= 0) return;

            var currentPos = collectionRect.anchoredPosition;
            var targetPos = GetScrollPosition();

            collectionRect.anchoredPosition = Vector2.Lerp(currentPos, targetPos, animationSpeed * Time.deltaTime);
        }

        public void Init(Sprite[] sprites)
        {
            spriteContent = sprites;
            images = new List<Image>(sprites.Length);
            positions = new List<Vector3>(sprites.Length);

            for (int i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];

                var imageHolder = Instantiate(imagePrefab, collection);
                images.Add(imageHolder);

                if (sprite != null)
                    imageHolder.sprite = sprite;

                var rectTransform = imageHolder.GetComponent<RectTransform>();
                var width = rectTransform.rect.width;
                rectTransform.anchoredPosition = new Vector2(i * (width + gapX), 0);
                positions.Add(rectTransform.anchoredPosition);
            }
            collectionRect.anchoredPosition = GetScrollPosition();
        }

        public Vector3 GetScrollPosition()
        {
            var currentPos = collectionRect.anchoredPosition;
            var targetPosX = -positions[SelectingIndex].x;
            return new Vector2(targetPosX, currentPos.y);
        }

        public override void OnRightButtonClicked()
        {
            base.OnRightButtonClicked();
        }

        public override void OnLeftButtonClicked()
        {
            base.OnLeftButtonClicked();
        }

    }
}
