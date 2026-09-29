using QuizGame.Scene;
using QuizGame.Setting.UI;
using QuizGame.Sound;
using QuizGame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace QuizGame.Setting
{
    public class SettingController
    {
        public event Action OnSignOut;

        private SettingUI settingUI;

        private SoundManager soundManager = SoundManager.Instance;

        public void Init()
        {
            UIManager.Instance.CloseAll();
            settingUI = UIManager.Instance.Create<SettingUI>();

            var masterVolume = soundManager.GetFloatMixerGroup(MixerGroups.Master);
            var musicVolume = soundManager.GetFloatMixerGroup(MixerGroups.BGM);
            var sfxVolume = soundManager.GetFloatMixerGroup(MixerGroups.SFX);

            settingUI.Setup(masterVolume, musicVolume, sfxVolume);

            settingUI.OnMasterVolumeChanged += HandleMasterVolumeChanged;
            settingUI.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            settingUI.OnSFXVolumeChanged += HandleSFXVolumeChanged;

            settingUI.OnLanguageChanged += HandleLanguageChanged;

            settingUI.OnCreditsButtonClicked += HandleCreditsButtonClicked;
            settingUI.OnSignOutButtonClicked += HandleSignOutButtonClicked;
            settingUI.OnBackButtonClicked += HandleBackButtonClicked;

            SetupLanguageDropdown();
        }

        private void SetupLanguageDropdown()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            var names = new List<string>(locales.Count);
            foreach (var locale in locales)
            {
                var culture = locale.Identifier.CultureInfo;
                names.Add(culture != null ? culture.NativeName : locale.Identifier.Code);
            }
            settingUI.SetupLanguages(names, Mathf.Max(0, locales.IndexOf(LocalizationSettings.SelectedLocale)));
        }

        private void HandleMasterVolumeChanged(float volume)
        {
            Debug.Log("[SettingController] Master volume changed");
            soundManager.SetFloatMixerGroup(MixerGroups.Master, volume);
        }

        private void HandleMusicVolumeChanged(float volume)
        {
            Debug.Log("[SettingController] Music volume changed");
            soundManager.SetFloatMixerGroup(MixerGroups.BGM, volume);
        }

        private void HandleSFXVolumeChanged(float volume)
        {
            Debug.Log("[SettingController] SFX volume changed");
            soundManager.SetFloatMixerGroup(MixerGroups.SFX, volume);
        }

        private void HandleLanguageChanged(int index)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (index < 0 || index >= locales.Count)
            {
                Debug.LogError($"[SettingController] Language index {index} is out of range.");
                return;
            }

            LocalizationSettings.SelectedLocale = locales[index];
            Debug.Log($"[SettingController] Language changed to {locales[index].Identifier.Code}");
        }

        private void HandleSignOutButtonClicked()
        {
            Debug.Log("[SettingController] Sign out button clicked");
            OnSignOut.Invoke();
        }

        private void HandleCreditsButtonClicked()
        {
            Debug.Log("[SettingController] Credits button clicked");
        }

        private void HandleBackButtonClicked()
        {
            Debug.Log("[SettingController] Back button clicked.");
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }
    }
}