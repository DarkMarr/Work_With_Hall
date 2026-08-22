using System;
using System.Threading.Tasks;
using QuizGame.UI;
using TMPro;
using UnityEngine;

namespace QuizGame.Gameplay
{
    public class LuckyDrawUI : BaseUI
    {
        public event Action OnStartDrawReward;
        public event Action OnEndDrawReward;

        public bool IsDrawing { get; private set; }

        [SerializeField]
        private TextMeshProUGUI bonusRateText;

        [SerializeField]
        private TextMeshProUGUI bonusRateDetailsText;

        [SerializeField]
        private GameObject contentHolder;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !IsDrawing)
            {
                DrawReward();
            }
        }

        public void SetupBonusMessage(string bonusInfo, string bonusDetails)
        {
            bonusRateText.text = bonusInfo;
            bonusRateDetailsText.text = bonusDetails;
        }

        private async void DrawReward()
        {
            IsDrawing = true;
            OnStartDrawReward?.Invoke();
            contentHolder.gameObject.SetActive(false);
            Debug.Log("[DrawResultUI] Start Draw Reward");

            await Task.Delay(3000);

            IsDrawing = false;
            OnEndDrawReward?.Invoke();
            Debug.Log("[DrawResultUI] End Draw Reward");
        }
    }
}
