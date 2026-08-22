using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.UI
{
    public abstract class LoopCategorySelectionButtons<TObject, TData> : BaseCategorySelectionButtons where TObject : MonoBehaviour
    {
        public override int ContentCount => contentData.Length;

        [SerializeField] private TObject contentPrefab;
        [SerializeField] private Transform collection;
        [SerializeField] private float animationSpeed;
        [SerializeField] private float gapX = 70f;

        private TData[] contentData;
        private List<TObject> contentPool = new List<TObject>();

        private RectTransform collectionRect;
        private int centerIndex = 0;
        private float itemWidth;

        protected override void Awake()
        {
            base.Awake();
            collectionRect = collection.GetComponent<RectTransform>();
            itemWidth = contentPrefab.GetComponent<RectTransform>().rect.width;
        }

        private void Update()
        {
            var currentPos = collectionRect.anchoredPosition;
            var targetPos = GetScrollPosition();
            collectionRect.anchoredPosition = Vector2.Lerp(currentPos, targetPos, animationSpeed * Time.deltaTime);
        }

        public void Init(TData[] data, Action<TObject, TData> onContentPrefabSpawned)
        {
            contentData = data;
            contentPool.Clear();

            foreach (Transform child in collection)
                Destroy(child.gameObject);

            int totalCount = contentData.Length * 2;

            for (int i = 0; i < totalCount; i++)
            {
                var content = Instantiate(contentPrefab, collection);
                contentPool.Add(content);

                var contentIndex = GetWrappedIndex(i);
                onContentPrefabSpawned?.Invoke(content, contentData[contentIndex]);

                var rt = content.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2((i - contentData.Length) * (itemWidth + gapX), 0);
            }

            collectionRect.anchoredPosition = GetScrollPosition();
        }

        public override void OnRightButtonClicked()
        {
            centerIndex++;
            SelectingIndex = GetWrappedIndex(centerIndex);
            ShiftRight();
            RefreshUIAndRaiseEvent();
        }

        public override void OnLeftButtonClicked()
        {
            centerIndex--;
            SelectingIndex = GetWrappedIndex(centerIndex);
            ShiftLeft();
            RefreshUIAndRaiseEvent();
        }

        private void ShiftRight()
        {
            // Move the leftmost contet to the rightmost position
            var first = contentPool[0];
            contentPool.RemoveAt(0);
            contentPool.Add(first);

            var lastRT = contentPool[^2].GetComponent<RectTransform>();
            var movedRT = first.GetComponent<RectTransform>();
            movedRT.anchoredPosition = lastRT.anchoredPosition + new Vector2(itemWidth + gapX, 0);
        }

        private void ShiftLeft()
        {
            // Move the rightmost content to the leftmost position
            var last = contentPool[^1];
            contentPool.RemoveAt(contentPool.Count - 1);
            contentPool.Insert(0, last);

            var firstRT = contentPool[1].GetComponent<RectTransform>();
            var movedRT = last.GetComponent<RectTransform>();
            movedRT.anchoredPosition = firstRT.anchoredPosition - new Vector2(itemWidth + gapX, 0);
        }

        private int GetWrappedIndex(int index)
        {
            return (index % contentData.Length + contentData.Length) % contentData.Length;
        }

        private Vector2 GetScrollPosition()
        {
            float targetPosX = centerIndex * (itemWidth + gapX);
            return new Vector2(-targetPosX, collectionRect.anchoredPosition.y);
        }
    }
}
