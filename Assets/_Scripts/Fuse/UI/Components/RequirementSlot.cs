using QuizGame.Item.Interfaces;
using QuizGame.Material;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Fuse.UI
{
    public class RequirementSlot : MonoBehaviour
    {
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI amountLabel;

        public void Setup(IQuantifiableItem itemData)
        {
            var info = MaterialResourceManager.Instance.GetResource(itemData.GetID());
            iconImage.sprite = info.GetSprite();
            amountLabel.text = itemData.GetQuantity().ToString();
        }

        public void Setup(IQuantifiableMaterial materialData)
        {
            var info = MaterialResourceManager.Instance.GetResource(materialData.GetID());
            iconImage.sprite = info.GetSprite();
            amountLabel.text = materialData.GetQuantity().ToString();
        }
    }
}