using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Sound.Component
{
    [RequireComponent(typeof(Button))]
    public class ButtonSFX : BaseSFXComponent
    {
        [SerializeField]
        protected List<AudioSO> audioRandomRange;

        [SerializeField]
        private Button button;

        private void OnValidate()
        {
            button ??= GetComponent<Button>();
        }

        private void Start()
        {
            button.onClick.AddListener(() => PlaySFXAudio(GetRandomSFXInList(audioRandomRange)));
        }
    }
}