using System;
using System.Collections.Generic;
using QuizGame.Utilities;
using UnityEngine;

namespace QuizGame.UI
{
    public class UIManager : MonoSingleton<UIManager>
    {
        public BaseUI LastestUI => onGoingUIs.Count > 0 ? onGoingUIs[onGoingUIs.Count - 1] : null;

        [SerializeField]
        private Canvas mainCanvas;

        private Dictionary<Type, BaseUI> uiByType;
        private List<BaseUI> onGoingUIs = new List<BaseUI>();

        protected override void Awake()
        {
            base.Awake();
            var uiGameObjects = Resources.LoadAll<BaseUI>("UI");
            uiByType = new Dictionary<Type, BaseUI>(uiGameObjects.Length);
            foreach (var ui in uiGameObjects)
            {
                if (uiByType.TryAdd(ui.GetType(), ui))
                {
                    // Debug.Log($"[UIManager] Add UI type \"{ui.GetType()}\".");
                }
                else
                {
                    Debug.LogError($"[UIManager] The UI type \"{ui.GetType()}\" already exist.");
                }
            }
        }

        public T Create<T>(Transform container = null) where T : BaseUI
        {
            var targetUI = uiByType[typeof(T)];
            var newUI = Instantiate(targetUI, container == null ? mainCanvas.transform : container);
            onGoingUIs.Add(newUI);
            return newUI as T;
        }

        public T Create<T>(BaseUI parentUI, Transform container = null) where T : BaseUI
        {
            var newUI = Create<T>(container);
            parentUI.AddChild(newUI);
            return newUI;
        }

        public T CreateSpecificUI<T>(T specificUI, BaseUI parentUI = null, Transform container = null) where T : BaseUI
        {
            var newUI = Instantiate(specificUI, container == null ? mainCanvas.transform : container);
            if (parentUI != null)
            {
                newUI.AddChild(newUI);
            }
            onGoingUIs.Add(newUI);
            return newUI;
        }

        public void CloseAll()
        {
            foreach (var ui in onGoingUIs)
            {
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                }
            }
            onGoingUIs.Clear();
        }

        public void CloseLastestUI()
        {
            if (LastestUI == null) return;
        }

        public void CloseUI<T>(T target) where T : BaseUI
        {
            if (target == null) return;
            onGoingUIs.Remove(target);
            Destroy(target.gameObject);
        }

        public T Replace<T>(ref BaseUI ui, BaseUI parentUI, Transform parentTransform = null) where T : BaseUI
        {
            ui?.Close();

            if (parentUI != null)
            {
                ui = Create<T>(parentUI);
            }
            else
            {
                ui = Create<T>();
            }

            if (parentTransform != null)
            {
                ui.transform.SetParent(parentTransform);
            }
            return ui as T;
        }

        public T Replace<T>(ref BaseUI ui, Transform parentTransform = null) where T : BaseUI
        {
            ui?.Close();
            if (parentTransform != null)
            {
                ui = Create<T>(parentTransform);
            }
            else
            {
                ui = Create<T>();
            }
            return ui as T;
        }
    }
}
