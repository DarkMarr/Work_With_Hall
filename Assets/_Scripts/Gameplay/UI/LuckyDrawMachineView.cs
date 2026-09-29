using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay
{
    public sealed class LuckyDrawMachineView : MonoBehaviour
    {
        public enum DrawPhase { Ready, WindUp, Spinning, BallDrop, Reveal }
        [Header("Art slots - keep pivots when replacing template Graphics")]
        [SerializeField] private RectTransform body;
        [SerializeField] private RectTransform drum;
        [SerializeField] private RectTransform crank;
        [SerializeField] private RectTransform ball;
        [SerializeField] private RectTransform glow;
        [SerializeField] private Graphic ballGraphic;
        [Header("Timing in seconds (unscaled)")]
        [Min(.05f)] public float windUpDuration = .25f;
        [Min(.05f)] public float spinDuration = 1.8f;
        [Min(.05f)] public float dropDuration = .65f;
        [Min(.05f)] public float revealDuration = .45f;
        public bool IsPlaying { get; private set; }
        public DrawPhase Phase { get; private set; }
        public float Duration => Mathf.Max(.05f, windUpDuration) + Mathf.Max(.05f, spinDuration)
            + Mathf.Max(.05f, dropDuration) + Mathf.Max(.05f, revealDuration);
        public bool IsConfigured => body && drum && crank && ball && glow && ballGraphic;
        private Action onComplete;
        private float speed = 1f;
        private static readonly Vector2 BallStart = new Vector2(92, -58);
        private static readonly Vector2 BallEnd = new Vector2(164, -171);

        private void Awake() { ResetPose(); }
        private void LateUpdate()
        {
            // Fixed design coordinates, fitted inside the host slot without stretching the artwork.
            var host = transform.parent as RectTransform;
            if (host == null) return;
            float scale = Mathf.Min(host.rect.width / 640f, host.rect.height / 560f);
            transform.localScale = Vector3.one * Mathf.Max(.001f, scale);
        }
        public bool Play(Action completed, float playbackSpeed = 1f)
        {
            if (!isActiveAndEnabled || IsPlaying || !IsConfigured) return false;
            ResetPose();
            speed = Mathf.Clamp(playbackSpeed, .25f, 3f);
            onComplete = completed;
            IsPlaying = true;
            StartCoroutine(Animate());
            return true;
        }
        public void SetBallColor(Color color) { if (ballGraphic) ballGraphic.color = color; }
        public void ResetPose()
        {
            StopAllCoroutines();
            IsPlaying = false; onComplete = null;
            if (IsConfigured) Sample(0);
        }
        private IEnumerator Animate()
        {
            float elapsed = 0;
            while (elapsed < Duration)
            {
                Sample(elapsed);
                yield return null;
                elapsed += Time.unscaledDeltaTime * speed;
            }
            Sample(Duration);
            IsPlaying = false;
            var callback = onComplete; onComplete = null;
            callback?.Invoke();
        }
        // Deterministic pose sampling also permits frame-by-frame inspection in the editor.
        public void Sample(float seconds)
        {
            if (!IsConfigured) return;
            float wind = Mathf.Max(.05f, windUpDuration);
            float spin = Mathf.Max(.05f, spinDuration);
            float drop = Mathf.Max(.05f, dropDuration);
            float reveal = Mathf.Max(.05f, revealDuration);
            float t = Mathf.Clamp(seconds, 0, Duration);
            float u = Mathf.Clamp01(t / wind);
            float s = Mathf.Clamp01((t - wind) / spin);
            float d = Mathf.Clamp01((t - wind - spin) / drop);
            float r = Mathf.Clamp01((t - wind - spin - drop) / reveal);
            Phase = t <= 0 ? DrawPhase.Ready : t < wind ? DrawPhase.WindUp :
                t < wind + spin ? DrawPhase.Spinning : t < wind + spin + drop ? DrawPhase.BallDrop : DrawPhase.Reveal;
            float ease = 1 - Mathf.Pow(1 - s, 3);
            drum.localRotation = Quaternion.Euler(0, 0, 12 * (1 - s) * Mathf.Sin(u * Mathf.PI) - 720 * ease);
            crank.localRotation = Quaternion.Euler(0, 0, 20 * (1 - s) * Mathf.Sin(u * Mathf.PI) - 1080 * ease);
            body.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(s * Mathf.PI * 18) * 2 * Mathf.Sin(s * Mathf.PI));
            ball.gameObject.SetActive(t >= wind + spin);
            float fall = Mathf.Clamp01(d / .6f);
            var pos = Vector2.Lerp(BallStart, BallEnd, fall * fall);
            pos.x = Mathf.Lerp(BallStart.x, BallEnd.x, fall);
            if (d > .6f) pos.y += Mathf.Sin((d - .6f) / .4f * Mathf.PI) * 28f;
            ball.anchoredPosition = pos;
            ball.localRotation = Quaternion.Euler(0, 0, -180 * d);
            float squash = d > .54f && d < .68f ? Mathf.Sin((d - .54f) / .14f * Mathf.PI) * .22f : 0;
            ball.localScale = new Vector3(1 + squash, 1 - squash, 1);
            glow.gameObject.SetActive(t >= wind + spin + drop);
            glow.localScale = Vector3.one * Mathf.Lerp(.6f, 1.25f, Mathf.SmoothStep(0, 1, r));
            glow.localRotation = Quaternion.Euler(0, 0, r * 35);
        }
        private void OnDisable() { ResetPose(); }
    }
}
