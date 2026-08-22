using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class BundleBanner : MonoBehaviour
    {
        public struct Info
        {
            public string BundleName;
            public string LimitedTime;
            public Sprite BundleSprite;

            public Info(string bundleName, string limitedTime, Sprite bundleSprite)
            {
                BundleName = bundleName;
                LimitedTime = limitedTime;
                BundleSprite = bundleSprite;
            }
        }

        [SerializeField]
        private TextMeshProUGUI bundleNameText;

        [SerializeField]
        private TextMeshProUGUI limitedTimeText;

        [SerializeField]
        private Image bundleImage;

        private Info info;

        public void Setup(Info info)
        {
            bundleNameText.text = info.BundleName;
            limitedTimeText.text = info.LimitedTime;
            bundleImage.sprite = info.BundleSprite;
        }

        public void Setup(string bundleName, string limitedTime, Sprite bundleSprite)
        {
            bundleNameText.text = bundleName;
            limitedTimeText.text = limitedTime;
            bundleImage.sprite = bundleSprite;
        }

        public Info GetInfo() => info;
    }
}
