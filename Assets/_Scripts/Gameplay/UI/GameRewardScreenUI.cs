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
            rankingPointText.text = $"{Mathf.Round(currentRP)}<size=\"40\"><color=#F9DB79>({rpToAdd:+0;-0;0})</color></size>"; //TODO: Handle decrease RP case
        }

        public void SetupSinglePlayer(int score, int? bestScore, string message, string buttonText = "Main Menu")
        {
            isLerping = false;
            SetupRewards(Array.Empty<IQuantifiableItem>());
            watchAdsButton.gameObject.SetActive(false);
            rankingPointText.text = bestScore.HasValue ? $"{score} Points\n<size=60%>Best: {bestScore.Value}</size>" : $"{score} Points";
            ReplaceTitle("RPTitle-Text", "Single Player");
            ReplaceTitle("RewardTitle-Text", message);
            var nextText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (nextText != null) nextText.text = buttonText;
            nextButton.interactable = bestScore.HasValue || buttonText == "Retry";
        }

        public void SetupMatch(string heading, string detail, string buttonText)
        {
            isLerping = false;
            watchAdsButton.gameObject.SetActive(false);
            SetupRewards(Array.Empty<IQuantifiableItem>());
            rankingPointText.text = heading;
            ReplaceTitle("RPTitle-Text", "Multiplayer");
            ReplaceTitle("RewardTitle-Text", detail);
            var label = nextButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = buttonText;
            nextButton.interactable = true;
        }

        private void ReplaceTitle(string objectName, string text)
        {
            // The existing frame has baked English headings. Use a plain template panel
            // until a frame without text is supplied; the source sprite stays untouched.
            foreach (var background in GetComponentsInChildren<Image>(true))
                if (background.name == "Background-Image")
                { background.sprite = null; background.color = new Color(.78f, .48f, .20f); }
            foreach (var target in GetComponentsInChildren<RectTransform>(true))
            {
                if (target.name != objectName) continue;
                target.gameObject.SetActive(true);
                target.SetAsLastSibling();
                target.anchorMin = new Vector2(.14f, objectName == "RPTitle-Text" ? .72f : .43f);
                target.anchorMax = new Vector2(.86f, objectName == "RPTitle-Text" ? .80f : .56f);
                target.offsetMin = target.offsetMax = Vector2.zero;
                var image = target.GetComponent<Image>();
                if (image != null) image.enabled = false;
                var label = target.GetComponent<TextMeshProUGUI>();
                if (label == null) label = target.GetComponentInChildren<TextMeshProUGUI>();
                if (label == null)
                {
                    label = Instantiate(rankingPointText, target);
                    label.name = "LiveTitle";
                    label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                }
                label.text = text; label.richText = false; label.enableAutoSizing = true;
                label.fontSizeMin = 28; label.fontSizeMax = 54; label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
                label.raycastTarget = false;
                return;
            }
        }

        public void SetAdsEnable(bool isEnable)
        {
            watchAdsButton.interactable = isEnable;
        }
    }
}
