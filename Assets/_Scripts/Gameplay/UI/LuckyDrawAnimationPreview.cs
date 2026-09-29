using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay
{
    // Visual-only review scene: no account, reward roll, inventory, or network calls.
    public sealed class LuckyDrawAnimationPreview : MonoBehaviour
    {
        public LuckyDrawMachineView machine;
        public Button playButton;
        public Button speedButton;
        public Button colorButton;
        public Text statusText;
        public Text speedText;
        public Text colorText;
        public GameObject rewardPanel;
        private readonly float[] speeds = { 1f, .5f, 1.5f };
        private readonly Color[] colors = { new Color(.95f,.86f,.66f), new Color(.38f,.72f,.96f),
            new Color(.46f,.81f,.62f), new Color(.85f,.47f,.90f), new Color(1f,.72f,.24f) };
        private int speedIndex;
        private int colorIndex;
        private void Start()
        {
            playButton.onClick.AddListener(Replay);
            speedButton.onClick.AddListener(() => { speedIndex = (speedIndex + 1) % speeds.Length; UpdateLabels(); });
            colorButton.onClick.AddListener(() => { colorIndex = (colorIndex + 1) % colors.Length; UpdateLabels(); });
            UpdateLabels(); rewardPanel.SetActive(false);
        }
        public void Replay()
        {
            if (machine.IsPlaying) return;
            rewardPanel.SetActive(false);
            machine.Play(() => rewardPanel.SetActive(true), speeds[speedIndex]);
        }
        private void UpdateLabels()
        {
            speedText.text = "SPEED  " + speeds[speedIndex].ToString("0.0") + "x";
            colorText.text = "BALL  " + (colorIndex + 1) + " / 5";
            machine.SetBallColor(colors[colorIndex]);
        }
        private void Update()
        {
            bool busy = machine.IsPlaying;
            playButton.interactable = speedButton.interactable = colorButton.interactable = !busy;
            statusText.text = machine.Phase == LuckyDrawMachineView.DrawPhase.Ready ? "Tap below to try the timing" :
                machine.Phase == LuckyDrawMachineView.DrawPhase.Reveal ? "Reveal - template reward only" :
                machine.Phase == LuckyDrawMachineView.DrawPhase.BallDrop ? "Ball drop + bounce" :
                machine.Phase == LuckyDrawMachineView.DrawPhase.WindUp ? "Wind up" : "Spinning + slowing down";
        }
    }
}
