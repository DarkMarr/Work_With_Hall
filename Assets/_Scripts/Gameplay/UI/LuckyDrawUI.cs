using System;
using System.Collections;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay
{
    public class LuckyDrawUI : BaseUI
    {
        public event Action OnStartDrawReward;
        public event Action OnEndDrawReward;
        public bool IsDrawing { get; private set; }
        public bool HasDrawn { get; private set; }
        [SerializeField] private TextMeshProUGUI bonusRateText;
        [SerializeField] private TextMeshProUGUI bonusRateDetailsText;
        [SerializeField] private GameObject contentHolder;
        [SerializeField] private RectTransform machineVisual;
        [SerializeField] private float drawDuration = 2f;
        private Button drawButton;
        private TMP_Text drawLabel;
        private Quaternion restRotation;

        protected override void Awake()
        {
            base.Awake();
            var backdrop = new GameObject("Template-Background", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(transform, false); backdrop.transform.SetAsFirstSibling();
            var backgroundRect = (RectTransform)backdrop.transform;
            backgroundRect.anchorMin = Vector2.zero; backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = backgroundRect.offsetMax = Vector2.zero;
            backdrop.GetComponent<Image>().color = new Color(.98f, .95f, .87f);
            backdrop.GetComponent<Image>().raycastTarget = false;
            bonusRateText.rectTransform.anchorMin = new Vector2(.1f, .82f);
            bonusRateText.rectTransform.anchorMax = new Vector2(.9f, .89f);
            bonusRateText.rectTransform.offsetMin = bonusRateText.rectTransform.offsetMax = Vector2.zero;
            bonusRateText.alignment = TextAlignmentOptions.Center;
            bonusRateDetailsText.rectTransform.anchorMin = new Vector2(.08f, .69f);
            bonusRateDetailsText.rectTransform.anchorMax = new Vector2(.92f, .81f);
            bonusRateDetailsText.rectTransform.offsetMin = bonusRateDetailsText.rectTransform.offsetMax = Vector2.zero;
            bonusRateDetailsText.alignment = TextAlignmentOptions.Center;
            bonusRateDetailsText.enableAutoSizing = true;
            bonusRateDetailsText.fontSizeMin = 24; bonusRateDetailsText.fontSizeMax = 40;
            bonusRateDetailsText.color = new Color(.46f, .27f, .10f);
            // A real uGUI button supports touch and the project's active Input System module.
            var buttonObject = new GameObject("Draw-Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(contentHolder.transform, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(.15f, .12f); rect.anchorMax = new Vector2(.85f, .27f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Image>().color = new Color(.95f, .56f, .12f);
            drawButton = buttonObject.GetComponent<Button>();
            drawButton.onClick.AddListener(DrawReward);
            drawLabel = Instantiate(bonusRateText, rect);
            drawLabel.name = "DrawLabel"; drawLabel.text = "Touch to draw"; drawLabel.raycastTarget = false;
            drawLabel.rectTransform.anchorMin = Vector2.zero; drawLabel.rectTransform.anchorMax = Vector2.one;
            drawLabel.rectTransform.offsetMin = drawLabel.rectTransform.offsetMax = Vector2.zero;
            drawLabel.alignment = TextAlignmentOptions.Center; drawLabel.enableAutoSizing = true;
            drawLabel.fontSizeMin = 24; drawLabel.fontSizeMax = 56;
            // Art slot: replace this labelled block with the supplied machine image/animation.
            if (machineVisual == null)
            {
                var slot = new GameObject("MachineArt-Placeholder", typeof(RectTransform), typeof(Image));
                slot.transform.SetParent(contentHolder.transform, false);
                machineVisual = (RectTransform)slot.transform;
                machineVisual.anchorMin = new Vector2(.24f, .34f); machineVisual.anchorMax = new Vector2(.76f, .65f);
                machineVisual.offsetMin = machineVisual.offsetMax = Vector2.zero;
                slot.GetComponent<Image>().color = new Color(.98f, .72f, .23f);
                slot.GetComponent<Image>().raycastTarget = false;
                var text = Instantiate(drawLabel, machineVisual); text.text = "Lucky Draw\n<size=55%>Machine art placeholder</size>";
            }
            restRotation = machineVisual.localRotation;
            foreach (var text in contentHolder.GetComponentsInChildren<TMP_Text>(true))
                if (text.name == "TouchToDraw-Text") text.gameObject.SetActive(false);
        }
        public void SetupBonusMessage(string info, string details) { bonusRateText.text = info; bonusRateDetailsText.text = details; }
        public void DrawReward()
        {
            if (HasDrawn || IsDrawing || !isActiveAndEnabled) return;
            HasDrawn = true; IsDrawing = true; drawButton.interactable = false; drawLabel.text = "Drawing...";
            OnStartDrawReward?.Invoke();
            if (this != null && isActiveAndEnabled) StartCoroutine(AnimateDraw());
        }
        private IEnumerator AnimateDraw()
        {
            float elapsed = 0;
            while (elapsed < drawDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                machineVisual.localRotation = restRotation * Quaternion.Euler(0, 0, Mathf.Sin(elapsed * 24) * 12);
                yield return null;
            }
            machineVisual.localRotation = restRotation; IsDrawing = false;
            drawLabel.text = "Saving reward...";
            OnEndDrawReward?.Invoke();
        }
        private void OnDisable() { StopAllCoroutines(); IsDrawing = false; }
    }
}
