using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QuizGame.Sound.Component
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldSFX : BaseSFXComponent
    {
        [SerializeField]
        protected List<AudioSO> onSelectAudioRandomRange;

        [SerializeField]
        protected List<AudioSO> onValueChangedAudioRandomRange;

        [SerializeField]
        protected List<AudioSO> onEndEditAudioRandomRange;

        [SerializeField]
        private TMP_InputField inputField;

        private void OnValidate()
        {
            inputField ??= GetComponent<TMP_InputField>();
        }

        private void Start()
        {
            inputField.onSelect.AddListener((text) => SoundManager.Instance.Play(GetRandomSFXInList(onSelectAudioRandomRange)));
            inputField.onEndEdit.AddListener((text) => SoundManager.Instance.Play(GetRandomSFXInList(onEndEditAudioRandomRange)));
            inputField.onValueChanged.AddListener((text) => SoundManager.Instance.Play(GetRandomSFXInList(onValueChangedAudioRandomRange)));
        }
    }
}