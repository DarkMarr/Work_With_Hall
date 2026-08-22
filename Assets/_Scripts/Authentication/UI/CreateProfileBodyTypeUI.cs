using UnityEngine;
using UnityEngine.UI;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class CreateProfileBodyTypeUI : BaseUI
    {
        public delegate void OnCreateProfileBodyType(int bodyID);

        [SerializeField]
        private LoopImageCategorySelectionButtons imageCategorySelection;

        [SerializeField]
        private Button nextButton;

        public void Init(Sprite[] bodySprites, OnCreateProfileBodyType onCreateProfileBodyTypeClicked)
        {
            imageCategorySelection.Init(bodySprites, (imageContent, spriteData) =>
            {
                imageContent.sprite = spriteData;
            });
            nextButton.onClick.AddListener(() => onCreateProfileBodyTypeClicked?.Invoke(imageCategorySelection.SelectingIndex));
        }
    }
}
