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

        [SerializeField]
        [Tooltip("The drawn button for each tab, matched by tab name. A tab with no entry " +
                 "keeps whatever the button prefab shows.")]
        private TabButtonArt[] tabButtonArt = new TabButtonArt[0];

        /// <summary>
        /// Art drew one button per craft category, each with its own icon baked in, so the
        /// background is what distinguishes them rather than a separate icon slot.
        /// </summary>
        [Serializable]
        public struct TabButtonArt
        {
            public string TabName;
            public Sprite Background;
        }

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
                ApplyArt(tabButton, tabModel.GetName());
                tabButton.onClick.AddListener(() => OnTabButtonClicked.Invoke(tabModel));
            }
        }

        /// <summary>Gives the button its drawn background, if one was authored for this tab.</summary>
        private void ApplyArt(Button tabButton, string tabName)
        {
            for (var i = 0; i < tabButtonArt.Length; i++)
            {
                if (tabButtonArt[i].TabName != tabName || tabButtonArt[i].Background == null) continue;
                var background = tabButton.GetComponent<Image>();
                if (background == null) return;
                background.sprite = tabButtonArt[i].Background;
                background.type = Image.Type.Sliced;
                background.color = Color.white;
                return;
            }
        }
    }
}