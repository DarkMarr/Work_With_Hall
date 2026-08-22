using QuizGame.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyDiary.UI
{
    public class MyDiaryUI : BaseUI
    {
        public event Action OnCloseButtonClicked;
        public event Action OnStatisticToggled;
        public event Action OnTravelDataToggled;

        [SerializeField] 
        private Button closeButton;

        [SerializeField]
        private ToggleGroup tabSelectToggleGroup;
        private Toggle currentTab;

        private void Start()
        {
            closeButton.onClick.AddListener(() => OnCloseButtonClicked?.Invoke());
        }

        public void Init()
        {
            RefreshUI();
            foreach (var toggle in tabSelectToggleGroup.GetComponentsInChildren<Toggle>())
            {
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        OnToggleChanged(toggle);
                    }
                });
            }
        }

        public void RefreshUI()
        {
            var currentActiveToggele = tabSelectToggleGroup.ActiveToggles().FirstOrDefault();
            if (currentTab == currentActiveToggele) return;

            OnToggleChanged(currentActiveToggele);
        }

        private void OnToggleChanged(Toggle newlyOn)
        {
            switch (newlyOn.name)
            {
                case "Statistic":
                    OnStatisticToggled?.Invoke();
                    break;

                case "TravelData":
                    OnTravelDataToggled?.Invoke();
                    break;

                default:
                    Debug.LogWarning($"Unknown toggle: {newlyOn.name}");
                    return;
            }
            currentTab = newlyOn;
        }
    }
}