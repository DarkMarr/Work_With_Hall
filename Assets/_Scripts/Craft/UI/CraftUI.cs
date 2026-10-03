using QuizGame.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Craft.UI
{
    public class CraftUI : BaseUI
    {
        public event Action OnBackButtonClicked;

        public event Action<CraftTabModel> OnTabButtonClicked;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Button tabButtonPref;

        [SerializeField]
        private Transform tabContainer;

        private void Start()
        {
            backButton.onClick.AddListener(() => OnBackButtonClicked.Invoke());
        }

        public void Init(List<CraftTabModel> craftTabModelList)
        {
            foreach (var tabModel in craftTabModelList)
            {
                var tabButton = Instantiate(tabButtonPref, tabContainer);
                var label = tabButton.GetComponentInChildren<TextMeshProUGUI>();

                label.text = tabModel.GetName();
                tabButton.onClick.AddListener(() => OnTabButtonClicked.Invoke(tabModel));
            }
        }
    }
}