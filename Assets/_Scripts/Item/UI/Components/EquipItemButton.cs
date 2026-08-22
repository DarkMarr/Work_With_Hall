using QuizGame.Item.UI;
using UnityEngine;

namespace Item.UI
{
    public class EquipItemButton : SelectItemButton
    {
        [SerializeField]
        private GameObject equippedVisualize;

        public void SetEquippedVisualizeActive(bool isActive)
        {
            equippedVisualize.SetActive(isActive);
        }
    }
}
