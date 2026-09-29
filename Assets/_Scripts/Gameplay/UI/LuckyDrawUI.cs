using System;
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
        [SerializeField] private LuckyDrawMachineView machinePrefab;
        private LuckyDrawMachineView machine;
        private Button drawButton;
        private TMP_Text drawLabel;

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
            if (machinePrefab != null)
            {
                var slot = new GameObject("Machine-Slot", typeof(RectTransform));
                slot.transform.SetParent(contentHolder.transform, false);
                var machineRect = (RectTransform)slot.transform;
                machineRect.anchorMin = new Vector2(.04f, .30f); machineRect.anchorMax = new Vector2(.96f, .69f);
                machineRect.offsetMin = machineRect.offsetMax = Vector2.zero;
                machine = Instantiate(machinePrefab, machineRect);
            }
            if (machine == null || !machine.IsConfigured)
            {
                drawButton.interactable = false;
                drawLabel.text = "Machine unavailable";
                Debug.LogError("[LuckyDrawUI] Configure the LuckyDrawMachine prefab before drawing.", this);
            }
            foreach (var text in contentHolder.GetComponentsInChildren<TMP_Text>(true))
                if (text.name == "TouchToDraw-Text") text.gameObject.SetActive(false);
        }
        public void SetupBonusMessage(string info, string details) { bonusRateText.text = info; bonusRateDetailsText.text = details; }
        public void DrawReward()
        {
            if (HasDrawn || IsDrawing || !isActiveAndEnabled || machine == null || !machine.IsConfigured) return;
            HasDrawn = true; IsDrawing = true; drawButton.interactable = false; drawLabel.text = "Drawing...";
            OnStartDrawReward?.Invoke();
            // The existing controller selects the pending award. Animation never grants or rerolls it.
            if (this != null && isActiveAndEnabled) machine.Play(FinishAnimation);
        }
        private void FinishAnimation()
        {
            IsDrawing = false;
            drawLabel.text = "Saving reward...";
            OnEndDrawReward?.Invoke();
        }
        private void OnDisable() { if (machine != null) machine.ResetPose(); IsDrawing = false; }
    }
}
