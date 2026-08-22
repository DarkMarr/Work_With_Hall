using QuizGame.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Setting.UI
{
    public class SettingUI : BaseUI
    {
        public event Action<float> OnMasterVolumeChanged;

        public event Action<float> OnMusicVolumeChanged;

        public event Action<float> OnSFXVolumeChanged;

        public event Action<int> OnLanguageChanged;

        public event Action OnCreditsButtonClicked;

        public event Action OnSignOutButtonClicked;

        public event Action OnBackButtonClicked;

        [SerializeField]
        private Slider masterSlider;

        [SerializeField]
        private Slider musicSlider;

        [SerializeField]
        private Slider sfxSlider;

        [SerializeField]
        private TMP_Dropdown languageDropdown;

        [SerializeField]
        private Button creditsButton;

        [SerializeField]
        private Button signOut;

        [SerializeField]
        private Button backButton;

        private void Start()
        {
            masterSlider.onValueChanged.AddListener(volume => OnMasterVolumeChanged?.Invoke(volume));
            musicSlider.onValueChanged.AddListener(volume => OnMusicVolumeChanged?.Invoke(volume));
            sfxSlider.onValueChanged.AddListener(volume => OnSFXVolumeChanged?.Invoke(volume));

            languageDropdown.onValueChanged.AddListener(index => OnLanguageChanged?.Invoke(index));

            creditsButton.onClick.AddListener(() => OnCreditsButtonClicked?.Invoke());
            signOut.onClick.AddListener(() => OnSignOutButtonClicked?.Invoke());
            backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
        }

        public void Setup(float soundVolume, float musicVolume, float sfxVolume)
        {
            UpdateVolumeSlider(masterSlider, soundVolume);
            UpdateVolumeSlider(musicSlider, musicVolume);
            UpdateVolumeSlider(sfxSlider, sfxVolume);
        }

        private void UpdateVolumeSlider(Slider slider, float volume)
        {
            slider.minValue = 0.0001f;
            slider.maxValue = 1;
            slider.value = volume;
        }
    }
}