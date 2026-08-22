using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.Sound.Component
{
    public abstract class BaseSFXComponent : MonoBehaviour
    {
        protected void PlaySFXAudio(AudioSO audioSO)
        {
            SoundManager.Instance?.Play(audioSO);
        }

        protected AudioSO GetRandomSFXInList(List<AudioSO> audioSO)
        {
            return audioSO[Random.Range(0, audioSO.Count)];
        }
    }
}