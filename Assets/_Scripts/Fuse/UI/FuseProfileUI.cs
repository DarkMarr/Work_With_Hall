using QuizGame.UI;
using UnityEngine;

namespace QuizGame.Fuse.UI
{
    public class FuseProfileUI : BaseUI
    {
        [SerializeField]
        private Transform materialContainer;

        [SerializeField]
        private RequirementSlot materialSlotPref;

        public void Setup(FusingPlayerModel playerModel)
        {
            Clear();
            foreach (var data in playerModel.GetMaterials())
            {
                var slot = Instantiate(materialSlotPref, materialContainer);
                slot.Setup(data);
            }
        }

        public void Clear()
        {
            foreach (Transform item in materialContainer)
            {
                Destroy(item.gameObject);
            }
        }
    }
}