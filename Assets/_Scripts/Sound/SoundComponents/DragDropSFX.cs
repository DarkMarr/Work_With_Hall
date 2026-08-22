using QuizGame.UI;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.Sound.Component
{
    [RequireComponent(typeof(DraggableUI))]
    public class DragDropSFX : BaseSFXComponent
    {
        [SerializeField]
        protected List<AudioSO> onDragAudioRandomRange;

        [SerializeField]
        protected List<AudioSO> onEndDragAudioRandomRange;

        [SerializeField]
        private DraggableUI draggableUI;

        private void OnValidate()
        {
            draggableUI ??= GetComponent<DraggableUI>();
        }

        private void Start()
        {
            draggableUI.OnStartDrag += () => SoundManager.Instance.Play(GetRandomSFXInList(onDragAudioRandomRange));
            draggableUI.OnEndDrag += () => SoundManager.Instance.Play(GetRandomSFXInList(onEndDragAudioRandomRange));
        }
    }
}