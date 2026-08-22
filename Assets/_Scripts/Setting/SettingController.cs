using QuizGame.Scene;
using QuizGame.Setting.UI;
using QuizGame.Sound;
using QuizGame.UI;
using System;
using UnityEngine;
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
            Debug.Log("[SettingController] Language changed");
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