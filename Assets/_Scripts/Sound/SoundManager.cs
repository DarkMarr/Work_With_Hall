using NaughtyAttributes;
using QuizGame.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace QuizGame.Sound
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        [SerializeField]
        private AudioMixer audioMixer;

        public AudioMixer AudioMixer => audioMixer;

        [SerializeField]
        private float TransitionTime = 0.05f;

        [SerializeField, ReadOnly]
        private SoundEmitter currentBGEmitter;

        public void Play(AudioSO audioSO)
        {
            switch (audioSO.GetSettings().MixerGroup)
            {
                case MixerGroups.BGM:
                    PlayBGM(audioSO);
                    break;

                case MixerGroups.SFX:
                    PlaySFX(audioSO);
                    break;

                default:
                    Debug.LogWarning("[SoundManager] No matching mixer group" + audioSO.GetSettings().MixerGroup.ToString());
                    break;
            }
        }

        private void PlayBGM(AudioSO audioSO)
        {
            if (currentBGEmitter && currentBGEmitter.AudioSO == audioSO)
            {
                return;
            }

            if (currentBGEmitter != null)
            {
                StartCoroutine(currentBGEmitter.FadeAudio(
                    start: currentBGEmitter.AudioSource.volume,
                    target: 0f,
                    duration: TransitionTime,
                    onFinished: soundEmitter => soundEmitter.Stop())
                );
            }

            var newSoundEmitter = CreateSoundEmitter(audioSO);

            StartCoroutine(newSoundEmitter.FadeAudio(
                start: 0f,
                target: newSoundEmitter.AudioSource.volume,
                duration: TransitionTime)
            );

            newSoundEmitter.Play();
            currentBGEmitter = newSoundEmitter;
        }

        private void PlaySFX(AudioSO audioSO)
        {
            var soundEmitter = CreateSoundEmitter(audioSO);
            soundEmitter.Play();
        }

        public SoundEmitter CreateSoundEmitter(AudioSO audioSO)
        {
            var pool = AudioSourcePooling.Instance;

            if (!pool.CanPlaySound(audioSO))
                return null;

            var soundEmitter = pool.Get();
            soundEmitter.transform.SetParent(transform);
            soundEmitter.Init(audioSO);
            pool.poolCountByAudio[audioSO] = pool.poolCountByAudio.TryGetValue(audioSO, out var count) ? count + 1 : 1;

            return soundEmitter;
        }

        public float GetFloatMixerGroup(MixerGroups mixerGroup)
        {
            audioMixer.GetFloat(mixerGroup.ToString(), out float volume);
            return Mathf.Pow(10, volume / 20);
        }

        public void SetFloatMixerGroup(MixerGroups mixerGroup, float volume)
        {
            audioMixer.SetFloat(mixerGroup.ToString(), Mathf.Log10(volume) * 20);
        }
    }
}