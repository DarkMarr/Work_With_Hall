using QuizGame.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyDiary.UI
{
    public class StatisticHelpPopUpUI : BaseUI
    {
        [SerializeField]
        private TextMeshProUGUI textDetailPrefab;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Transform textDetailContainer;

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
        }

        public void Setup(List<string> detailsText)
        {
            for (int i = 0; i < detailsText.Count; i++)
            {
                var textDetail = Instantiate(textDetailPrefab, textDetailContainer);
                textDetail.text = detailsText[i];
            }
        }
    }
}