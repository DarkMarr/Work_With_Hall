using UnityEngine;

namespace QuizGame.Sound.Component
{
    public class OnStartAudio : MonoBehaviour
    {
        [SerializeField]
        private AudioSO audioSO;

        private void Start()
        {
            SoundManager.Instance?.Play(audioSO);
        }
    }
}