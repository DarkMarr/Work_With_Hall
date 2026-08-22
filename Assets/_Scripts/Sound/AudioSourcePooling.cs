using QuizGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace QuizGame.Sound
{
    public class AudioSourcePooling : MonoSingleton<AudioSourcePooling>
    {
        private IObjectPool<SoundEmitter> soundEmitterPool;

        private readonly List<SoundEmitter> activeSoundEmitters = new List<SoundEmitter>();

        public readonly Dictionary<AudioSO, int> poolCountByAudio = new Dictionary<AudioSO, int>();

        [SerializeField]
        private SoundEmitter soundEmitterPrefab;

        [SerializeField]
        private bool collectionCheck = false;

        [SerializeField]
        private int defaultCapacity = 10;

        [SerializeField]
        private int maxPoolSize = 100;

        [SerializeField]
        private int maxSoundInstance = 30;

        private void Start()
        {
            InitPool();
        }

        public bool CanPlaySound(AudioSO data)
        {
            return !poolCountByAudio.TryGetValue(data, out var count) || count < maxSoundInstance;
        }

        public SoundEmitter Get() => soundEmitterPool.Get();

        public void ReturnToPool(SoundEmitter soundEmitter)
        {
            soundEmitterPool.Release(soundEmitter);
        }

        private void InitPool()
        {
            soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize
            );
        }

        private SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        private void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
            activeSoundEmitters.Add(soundEmitter);
        }

        private void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            if (poolCountByAudio.TryGetValue(soundEmitter.AudioSO, out var count))
            {
                poolCountByAudio[soundEmitter.AudioSO] -= count > 0 ? 1 : 0;
            }

            soundEmitter.gameObject.SetActive(false);
            activeSoundEmitters.Remove(soundEmitter);
        }

        private void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            Destroy(soundEmitter.gameObject);
        }
    }
}