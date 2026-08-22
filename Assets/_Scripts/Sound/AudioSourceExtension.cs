using UnityEngine;

namespace QuizGame.Sound
{
    public static class AudioSourceExtension
    {
        public static void ApplyAudio(this AudioSource audioSource, AudioSO audioSO)
        {
            audioSource.resource = audioSO.GetAudio();
            audioSource.outputAudioMixerGroup = SoundManager.Instance.AudioMixer.FindMatchingGroups(audioSO.GetSettings().MixerGroup.ToString())[0];
            audioSource.loop = audioSO.GetSettings().IsLooping;

            var volumePercentage = audioSO.GetSettings().IsRandomVolume
                ? audioSO.GetIntInRandomRange(randomRange: audioSO.GetSettings().RandomVolumeRange)
                : audioSO.GetSettings().Volume;
            audioSource.volume = (float)volumePercentage / 100;

            var pitchPercentage = audioSO.GetSettings().IsRandomPitch
                ? audioSO.GetIntInRandomRange(randomRange: audioSO.GetSettings().RandomPitchRange)
                : audioSO.GetSettings().Pitch;
            audioSource.pitch = (float)pitchPercentage / 100;

            audioSource.panStereo = audioSO.GetSettings().StereoPan;
            audioSource.spatialBlend = audioSO.GetSettings().SpatialBlend;
            audioSource.reverbZoneMix = audioSO.GetSettings().ReverbZoneMix;
        }
    }
}