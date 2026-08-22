using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace QuizGame.Store
{
    [RequireComponent(typeof(Button), typeof(CodelessIAPButton))]
    public class QuizGameIAPButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private CodelessIAPButton iapButton;

        private Product product;

        private void OnValidate()
        {
            button = GetComponent<Button>();
            iapButton = GetComponent<CodelessIAPButton>();

            if (button != null && iapButton != null)
            {
                iapButton.button = button;
            }
            iapButton.enabled = false;
        }

        protected virtual void Awake()
        {
            button.onClick.AddListener(Purchase);
        }

        public virtual void Setup(Product product)
        {
            this.product = product;
            iapButton.productId = product.definition.id;
            iapButton.enabled = true;
        }

        public virtual void Clear()
        {
            product = null;
            iapButton.enabled = false;
            iapButton.productId = "";
        }

        public void Purchase()
        {
            if (product == null)
            {
                Debug.LogError("Product is not initialized. Please call Init() before purchasing.");
                return;
            }
            button.interactable = false;
            IAPManager.Instance.Purchase(product, this);
        }

        public void OnPurchaseCompleted()
        {
            button.interactable = true;
        }

        public CodelessIAPButton GetCodelessIAPButton() => iapButton;
    }
}
