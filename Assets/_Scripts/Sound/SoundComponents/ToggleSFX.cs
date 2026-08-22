using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Sound.Component
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleSFX : BaseSFXComponent
    {
        [SerializeField]
        protected List<AudioSO> audioRandomRange;

        [SerializeField]
        private Toggle toggle;

        private void OnValidate()
        {
            toggle ??= GetComponent<Toggle>();
        }

        private void Start()
        {
            toggle.onValueChanged.AddListener(isOn => HandleToggleOn(isOn));
        }

        private void HandleToggleOn(bool isOn)
        {
            if (!isOn)
                return;

            PlaySFXAudio(GetRandomSFXInList(audioRandomRange));
        }
    }
}