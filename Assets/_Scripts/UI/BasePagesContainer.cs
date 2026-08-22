using System;
using System.Collections.Generic;
using QuizGame.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public abstract class BasePagesContainer<TObject, TData> : MonoBehaviour, IPageContainable where TObject : UnityEngine.Object
    {
        public Action OnPageChange;

        public int CurrentPageIndex { get; private set; }
        public int PageLength { get; set; }

        [SerializeField]
        private TObject objectPrefab;

        [SerializeField]
        private Transform objectContainer;

        [SerializeField]
        private GridLayoutGroup containerGridLayoutGroup;

        private RectTransform containerRect;
        private List<TObject> spawnedObjects = new List<TObject>();
        private Dictionary<int, TData[]> dataByPageNumber;

        private void Awake()
        {
            containerRect = objectContainer.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Create "TContent" for each data and save it to container.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onTContentclicked"></param>
        public void Setup(TData[] data, Action<int, TObject> onObjectCreate)
        {
            var itemAmountInAPage = GetObjectAmountFitInPage();
            dataByPageNumber = new Dictionary<int, TData[]>(itemAmountInAPage);
            PageLength = Mathf.FloorToInt(data.Length / GetObjectAmountFitInPage()) + 1;

            for (int i = 0; i < PageLength; i++)
            {
                if (i == PageLength - 1)
                {
                    var lastPageItemAmount = data.Length % GetObjectAmountFitInPage();
                    dataByPageNumber.Add(i, new TData[lastPageItemAmount]);
                }
                else
                {
                    dataByPageNumber.Add(i, new TData[itemAmountInAPage]);
                }
            }

            for (int i = 0; i < itemAmountInAPage; i++)
            {
                TObject spawnedObject;
                if (i > spawnedObjects.Count - 1)
                {
                    spawnedObject = Instantiate(objectPrefab, objectContainer);
                    spawnedObjects.Add(spawnedObject);
                }
                else
                {
                    spawnedObject = spawnedObjects[i];
                }
                var objectIndex = i;
                onObjectCreate?.Invoke(objectIndex, spawnedObject);
            }

            for (int i = 0; i < data.Length; i++)
            {
                var pageIndex = GetPageIndexFromData(i);
                var dataInPageIndex = GetDataIndexInPage(i);
                if (pageIndex < dataByPageNumber.Count && dataInPageIndex < dataByPageNumber[pageIndex].Length)
                {
                    dataByPageNumber[pageIndex][dataInPageIndex] = data[i];
                }
            }
        }

        public void DebugObjectAmountInAPage()
        {
            containerRect = objectContainer.GetComponent<RectTransform>();
            Debug.Log($"You get col: {GetColumnCount()}, row: {GetRowCount()}, A page in the container can have {GetObjectAmountFitInPage()} objects");
        }

        public bool TryGetSpawnedObject(int index, out TObject spawnedObject)
        {
            if (index > -1 && index < spawnedObjects.Count)
            {
                spawnedObject = spawnedObjects[index];
                return true;
            }
            spawnedObject = null;
            return false;
        }

        [NaughtyAttributes.Button]
        public void DebugItemAmountInAPage()
        {
            DebugObjectAmountInAPage();
        }

        public List<TObject> GetSpawnedObjects() => spawnedObjects;
        public int CountSpawnedObject() => spawnedObjects.Count;
        public int PageCount() => PageLength;
        public int GetColumnCount() => Mathf.FloorToInt((containerRect.rect.width + containerGridLayoutGroup.spacing.x -
                                                            containerGridLayoutGroup.padding.right - containerGridLayoutGroup.padding.left) /
                                                            (containerGridLayoutGroup.cellSize.x + containerGridLayoutGroup.spacing.x));
        public int GetRowCount() => Mathf.FloorToInt((containerRect.rect.height + containerGridLayoutGroup.spacing.y -
                                                        containerGridLayoutGroup.padding.top - containerGridLayoutGroup.padding.bottom) /
                                                        (containerGridLayoutGroup.cellSize.y + containerGridLayoutGroup.spacing.y));
        public int GetObjectAmountFitInPage() => GetColumnCount() * GetRowCount();
        public int GetPageIndexFromData(int dataIndex) => Mathf.FloorToInt(dataIndex / GetObjectAmountFitInPage());
        public int GetDataIndexInPage(int dataIndex) => dataIndex % GetObjectAmountFitInPage();

        /// <summary>
        /// This function help you get index of a data in the spawned object while considered current page index.
        /// </summary>
        /// <param name="objectIndex"></param>
        /// <returns></returns>
        public int GetDataIndexFromObjectOnCurrentPage(int objectIndex) => CurrentPageIndex * GetObjectAmountFitInPage() + objectIndex;

        public bool IsDataBelongToCurrentPage(int objectIndex)
        {
            int start = CurrentPageIndex * GetObjectAmountFitInPage();
            int end = start + GetObjectAmountFitInPage();
            return objectIndex >= start && objectIndex < end;
        }

        public void OpenNextPage()
        {
            var nextPageIndex = CurrentPageIndex + 1;
            if (nextPageIndex > PageLength - 1)
            {
                return;
            }
            OpenPage(nextPageIndex);
        }

        public void OpenPreviousPage()
        {
            var previousPageIndex = CurrentPageIndex - 1;
            if (previousPageIndex < 0)
            {
                return;
            }
            OpenPage(previousPageIndex);
        }

        public TObject GetObjectFromDataIndex(int index)
        {
            var page = GetPageIndexFromData(index);
            var indexInPage = GetDataIndexInPage(index);
            if (!dataByPageNumber.ContainsKey(page) || indexInPage >= dataByPageNumber[page].Length)
            {
                Debug.LogError("Object out of range");
                return null;
            }
            return spawnedObjects[indexInPage];
        }

        public void OpenPage(int pageIndex)
        {
            if (!dataByPageNumber.ContainsKey(pageIndex))
            {
                Debug.LogError($"[Container] The container doesn't contain page index: {pageIndex}");
                return;
            }
            CurrentPageIndex = pageIndex;
            var data = dataByPageNumber[pageIndex];
            OnOpenPage(spawnedObjects, data);
        }

        protected virtual void OnOpenPage(List<TObject> objectInPage, TData[] dataInPage)
        {
            OnPageChange?.Invoke();
        }
    }
}
