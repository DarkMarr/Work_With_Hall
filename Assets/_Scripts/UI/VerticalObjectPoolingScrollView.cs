using System.Collections.Generic;
using QuizGame.Interfaces;
using UnityEngine;

namespace QuizGame.UI
{
    public abstract class VerticalObjectPoolingScrollView<TElement> : MonoBehaviour where TElement : MonoBehaviour, IHasRectTransform
    {
        public delegate void OnElementDataUpdateHandler(int dataIndex, TElement element);
        private event OnElementDataUpdateHandler onElementDataUpdate;

        //This make sure UI visualization behalf seamlessly
        public const int ExceedElementCount = 2;

        [SerializeField]
        private TElement element;

        [SerializeField]
        private RectTransform viewPort;

        [SerializeField]
        private RectTransform contentContainer;

        [Header("Content config")]
        [SerializeField]
        private float spacing;

        protected List<TElement> elementPool;
        private Vector2[] elementPositions;

        private int upperElementIndex;
        private int bottomElementIndex;
        private int poolStartIndex = 0;
        private float lastScrollY;

        private void OnValidate()
        {
            if (element == null)
            {
                Debug.LogError("Element is not assigned in VerticalObjectPoolingScrollView.");
                return;
            }
            var elementRect = element.GetRectTransform();
            if (elementRect.anchorMax.x != 0.5f || elementRect.anchorMin.x != 0.5f ||
                elementRect.anchorMax.y != 1f || elementRect.anchorMin.y != 1f ||
                elementRect.pivot.x != 0.5f || elementRect.pivot.y != 1f)
            {
                Debug.LogError("Element RectTransform should be anchored to the top center.");
            }
        }

        protected virtual void Update()
        {
            var currentScrollY = contentContainer.anchoredPosition.y;
            var isScrollingDown = currentScrollY > lastScrollY;
            var isScrollingUp = currentScrollY < lastScrollY;

            lastScrollY = currentScrollY;

            var viewPortTopY = viewPort.transform.position.y;
            var viewPortBottomY = viewPortTopY - viewPort.rect.height;
            var elementHeight = element.GetRectTransform().rect.height;
            
            var newUpperIndex = GetDataIndexAtPosition(viewPortTopY, elementHeight);
            var newBottomIndex = GetDataIndexAtPosition(viewPortBottomY, elementHeight);
            
            newUpperIndex = Mathf.Max(0, newUpperIndex - 1);
            newBottomIndex = Mathf.Min(elementPositions.Length - 1, newBottomIndex + 1);
            
            if (newUpperIndex != upperElementIndex || newBottomIndex != bottomElementIndex)
            {
                RepositionElements(newUpperIndex, newBottomIndex);
            }
            else
            {
                if (isScrollingDown)
                {
                    var upperElement = GetElement(0);
                    var upperElementBottomY = upperElement.transform.position.y - upperElement.GetRectTransform().rect.height;

                    if (upperElementBottomY > viewPortTopY)
                    {
                        if (bottomElementIndex + 1 < elementPositions.Length)
                        {
                            poolStartIndex = (poolStartIndex + 1) % elementPool.Count;

                            var movedElement = GetElement(elementPool.Count - 1);

                            upperElementIndex++;
                            bottomElementIndex++;

                            onElementDataUpdate?.Invoke(bottomElementIndex, movedElement);
                            movedElement.GetRectTransform().anchoredPosition = elementPositions[bottomElementIndex];
                        }
                    }
                }
                else if (isScrollingUp)
                {
                    var bottomElement = GetElement(elementPool.Count - 1);

                    if (bottomElement.transform.position.y < viewPortBottomY)
                    {
                        if (upperElementIndex - 1 >= 0)
                        {
                            poolStartIndex = (poolStartIndex - 1 + elementPool.Count) % elementPool.Count;

                            upperElementIndex--;
                            bottomElementIndex--;

                            var movedElement = GetElement(0);
                            onElementDataUpdate?.Invoke(upperElementIndex, movedElement);
                            movedElement.GetRectTransform().anchoredPosition = elementPositions[upperElementIndex];
                        }
                    }
                }
            }
        }

        private int GetDataIndexAtPosition(float worldY, float elementHeight)
        {
            var localY = contentContainer.InverseTransformPoint(new Vector3(0, worldY, 0)).y;
            var index = Mathf.FloorToInt(-localY / (elementHeight + spacing));
            return Mathf.Clamp(index, 0, elementPositions.Length - 1);
        }

        private void RepositionElements(int newUpperIndex, int newBottomIndex)
        {
            var requiredElements = newBottomIndex - newUpperIndex + 1;
            requiredElements = Mathf.Min(requiredElements, elementPool.Count);
            
            poolStartIndex = 0;
            upperElementIndex = newUpperIndex;
            bottomElementIndex = newUpperIndex + requiredElements - 1;
            
            for (int i = 0; i < elementPool.Count; i++)
            {
                int dataIndex = newUpperIndex + i;
                
                if (dataIndex < elementPositions.Length)
                {
                    var poolElement = elementPool[i];
                    poolElement.GetRectTransform().anchoredPosition = elementPositions[dataIndex];
                    onElementDataUpdate?.Invoke(dataIndex, poolElement);
                }
            }
        }

        public void Init(int dataAmount, OnElementDataUpdateHandler onElementDataUpdate)
        {
            this.onElementDataUpdate = onElementDataUpdate;

            var elementCount = ShouldUsePooling(dataAmount) ? GetFitItemCount() + ExceedElementCount : dataAmount;
            SetupElementPool(elementCount);
            SetupElements(dataAmount, elementCount);

            upperElementIndex = 0;
            bottomElementIndex = elementCount - 1;
            poolStartIndex = 0;
            lastScrollY = contentContainer.anchoredPosition.y;
        }

        public void UpdateUI(int dataAmount)
        {
            contentContainer.anchoredPosition = Vector2.zero;

            foreach (var element in elementPool)
            {
                element.gameObject.SetActive(false);
            }

            var elementCount = Mathf.Min(elementPool.Count, dataAmount);
            SetupElements(dataAmount, elementCount);

            upperElementIndex = 0;
            bottomElementIndex = elementCount - 1;
            poolStartIndex = 0;
            lastScrollY = 0;
        }

        private int SetupElementPool(int elementCount)
        {
            elementPool = new List<TElement>(elementCount);
            for (int i = 0; i < elementCount; i++)
            {
                var newElement = Instantiate(element, contentContainer);
                elementPool.Add(newElement);
            }
            return elementCount;
        }

        private void SetupElements(int dataAmount, int elementCount)
        {
            var containerHeight = CalculateContainerHeight(dataAmount);
            contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, containerHeight);

            var elementRect = element.GetRectTransform();
            for (int i = 0; i < elementCount; i++)
            {
                var e = elementPool[i];
                e.gameObject.SetActive(true);
                e.GetRectTransform().anchoredPosition = CalculateElementPosition(i, elementRect.rect.height);
                onElementDataUpdate?.Invoke(i, e);
            }

            UpdateElementPositions(dataAmount, elementRect.rect.height);
        }

        private bool ShouldUsePooling(int dataAmount) => dataAmount > GetFitItemCount() + ExceedElementCount;

        private float CalculateContainerHeight(int dataAmount)
        {
            var height = element.GetRectTransform().rect.height;
            return dataAmount * height + (dataAmount - 1) * spacing;
        }

        private Vector2 CalculateElementPosition(int index, float height)
        {
            var yPos = index * (height + spacing);
            return new Vector2(0, -yPos);
        }

        private void UpdateElementPositions(int dataAmount, float height)
        {
            elementPositions = new Vector2[dataAmount];
            for (int i = 0; i < dataAmount; i++)
            {
                elementPositions[i] = CalculateElementPosition(i, height);
            }
        }

        private TElement GetElement(int i) => elementPool[(poolStartIndex + i) % elementPool.Count];

        public int GetFitItemCount() => Mathf.FloorToInt((viewPort.rect.height + spacing) / (element.GetRectTransform().rect.height + spacing));
    }
}