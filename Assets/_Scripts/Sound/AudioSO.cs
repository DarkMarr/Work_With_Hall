using NaughtyAttributes;
using QuizGame.Resources;
using System.Threading.Tasks;
using UnityEngine;

namespace QuizGame.Sound
{
    [CreateAssetMenu(fileName = "NewAudio", menuName = "QuizGame/Sound/Audio", order = 1)]
    public class AudioSO : ScriptableObject, IHasID
    {
        [SerializeField]
        protected AudioClip audio;

        [SerializeField, ReadOnly]
        protected string audioID;

        [SerializeField]
        protected AudioSettings settings;

        protected virtual void OnValidate()
        {
            audioID = name;
        }

        public AudioClip GetAudio() => audio;

        public AudioSettings GetSettings() => settings;

        public string GetID() => audioID;

        public int GetIntInRandomRange(Vector2Int randomRange) => Random.Range(randomRange.x, randomRange.y + 1);

        [Button("Editor Audio Test\n(If not working, play one time)", EButtonEnableMode.Editor)]
        private async void TestAudio()
        {
            var audioSource = new GameObject(name, typeof(AudioSource)).GetComponent<AudioSource>();
            audioSource.ApplyAudio(this);

            audioSource.PlayOneShot(audio);
            await Task.Delay((int)((GetAudio().length + 0.001f) * 1000));
            DestroyImmediate(audioSource.gameObject);
        }
    }
}