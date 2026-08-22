using NaughtyAttributes;
using UnityEngine;

namespace QuizGame.Sound
{
    [System.Serializable]
    public class AudioSettings
    {
        public MixerGroups MixerGroup;

        public bool IsLooping;

        [Space]
        [Label("Volume Mode"), Dropdown("IsRandomMode"), AllowNesting]
        public bool IsRandomVolume;

        [Label("Volume (%)"), HideIf("IsRandomVolume"), AllowNesting]
        [Range(0, 100)]
        public int Volume = 100;

        [Label("Volume (%)"), ShowIf("IsRandomVolume"), AllowNesting]
        [MinMaxSlider(0, 100)]
        public Vector2Int RandomVolumeRange = new Vector2Int(80, 100);

        [Space]
        [Label("Pitch Mode"), Dropdown("IsRandomMode"), AllowNesting]
        public bool IsRandomPitch;

        [Label("Pitch (%)"), HideIf("IsRandomPitch"), AllowNesting]
        [Range(-300, 300)]
        public int Pitch = 100;

        [Label("Pitch (%)"), ShowIf("IsRandomPitch"), AllowNesting]
        [MinMaxSlider(-300, 300)]
        public Vector2Int RandomPitchRange = new Vector2Int(80, 100);

        [Space]
        [Range(-1, 1)]
        public float StereoPan = 0;

        [Range(0, 1)]
        public float SpatialBlend = 0;

        [Range(0, 1.1f)]
        public float ReverbZoneMix = 1;

        private DropdownList<bool> IsRandomMode()
        {
            return new DropdownList<bool>()
            {
                { "Normal", false },
                { "Random", true }
            };
        }
    }
}