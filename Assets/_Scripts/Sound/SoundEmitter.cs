using System;
using System.Collections;
using UnityEngine;

namespace QuizGame.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField]
        private AudioSO audioSO;

        public AudioSO AudioSO => audioSO;

        [SerializeField]
        private AudioSource audioSource;

        public AudioSource AudioSource => audioSource;

        private Coroutine playingCoroutine;

        private void OnValidate()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        public void Init(AudioSO audioSO)
        {
            this.audioSO = audioSO;
            audioSource.name = (audioSO.GetID() + " - SoundEmitter");
            audioSource.ApplyAudio(audioSO);
        }

        public void Play()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
            }

            audioSource.Play();
            playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }

        private IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitForSeconds(audioSource.clip.length);
            AudioSourcePooling.Instance.ReturnToPool(this);
        }

        public void Stop()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }

            audioSource.Stop();
            AudioSourcePooling.Instance.ReturnToPool(this);
        }

        public IEnumerator FadeAudio(float start, float target, float duration, Action<SoundEmitter> onFinished = default)
        {
            AudioSource.volume = start;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                AudioSource.volume = Mathf.Lerp(start, target, elapsedTime / duration);
                yield return null;
            }

            onFinished?.Invoke(this);
        }
    }
}