using System;
using QuizGame.Item.Interfaces;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay
{
    public class GameRewardScreenUI : BaseUI
    {
        public event Action OnNextButtonClicked;
        public event Action OnAdsButtonClicked;

        [SerializeField]
        private ImageWithTextVisualization rewardVisualPrefab;

        [SerializeField]
        private Transform rewardVisualContainer;

        [SerializeField]
        private TextMeshProUGUI rankingPointText;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private Button watchAdsButton;

        [SerializeField]
        private float rpChangeLerpDuration = 2;

        private bool isLerping = false;
        private float startTime;
        private float currentRP;
        private float targetRP;
        private float maxRP;
        private float rpToAdd;

        private void Start()
        {
            nextButton.onClick.AddListener(() => OnNextButtonClicked?.Invoke());
            watchAdsButton.onClick.AddListener(() => OnAdsButtonClicked?.Invoke());
        }

        private void Update()
        {
            if (isLerping)
            {
                var timeElapsed = Time.time - startTime;
                var t = timeElapsed / rpChangeLerpDuration;

                t = Mathf.Clamp01(t);

                var tempRP = Mathf.Lerp(currentRP, targetRP, t);

                if (t >= 1.0f)
                {
                    tempRP = targetRP;
                    isLerping = false;
                }
                SetRankingPointText(tempRP, rpToAdd);
            }
        }

        public void SetupRewards(IQuantifiableItem[] itemRewards)
        {
            foreach (Transform child in rewardVisualContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in itemRewards)
            {
                var rewardVisual = Instantiate(rewardVisualPrefab, rewardVisualContainer);
                rewardVisual.Setup(item.GetSprite(), item.GetQuantity().ToString());
            }
        }

        public void SetRankingPointVisualize(int rpToAdd, int currentRP, int maxRP)
        {
            targetRP = rpToAdd + currentRP;
            this.currentRP = currentRP;
            this.maxRP = maxRP;
            this.rpToAdd = rpToAdd;
            startTime = Time.time;
            isLerping = true;
        }

        public void SetRankingPointText(float currentRP, float rpToAdd)
        {
            rankingPointText.text = $"{Mathf.Round(currentRP)}<size=\"40\"><color=#F9DB79>(+{rpToAdd})"; //TODO: Handle decrease RP case
        }

        public void SetAdsEnable(bool isEnable)
        {
            watchAdsButton.interactable = isEnable;
        }
    }
}
