using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QuizGame.Sound.Component
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropDownSFX : BaseSFXComponent
    {
        [SerializeField]
        protected List<AudioSO> audioRandomRange;

        [SerializeField]
        private TMP_Dropdown dropDown;

        private void OnValidate()
        {
            dropDown ??= GetComponent<TMP_Dropdown>();
        }

        private void Start()
        {
            dropDown.onValueChanged.AddListener(_ => PlaySFXAudio(GetRandomSFXInList(audioRandomRange)));
        }
    }
}