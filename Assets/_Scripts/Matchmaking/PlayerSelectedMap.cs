using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Matchmaking
{
    public class PlayerSelectedMap : MonoBehaviour
    {
        [SerializeField]
        private Image mapImage;

        [SerializeField]
        private Color shadingColor;

        public void SetMapSprite(Sprite mapSprite)
        {
            if (mapImage != null && mapSprite != null)
            {
                mapImage.sprite = mapSprite;
                mapImage.gameObject.SetActive(true);
            }
            else
            {
                mapImage.gameObject.SetActive(false);
            }
        }

        public void SetShading(bool enabled)
        {
            if (mapImage != null)
            {
                mapImage.color = enabled ? shadingColor : Color.white;
            }
        }
    }
}
