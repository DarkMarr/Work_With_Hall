using QuizGame.MyRoom.Decoration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizGame.Fuse
{
    public class FuseTabModel
    {
        private string tabName;
        private DecorationType[] decorationTypes;
        private Dictionary<DecorationType, string> decorationLabelNameByID;
        private Dictionary<DecorationType, List<DecorationItemSO>> decorationByID;

        public FuseTabModel(string tabName, string[] labelName, DecorationType[] decorationTypes)
        {
            this.tabName = tabName;
            this.decorationTypes = decorationTypes;
            SetupDecoration(labelName, decorationTypes);
        }

        public string GetName() => tabName;

        public DecorationType[] GetDecorationTypes() => decorationTypes;

        public string GetDecorationLabelName(DecorationType type) => decorationLabelNameByID[type];

        public List<IDecorationItem> GetDecorationByType(DecorationType type) => decorationByID[type].Cast<IDecorationItem>().ToList();

        private void SetupDecoration(string[] labelName, DecorationType[] decorationTypes)
        {
            decorationLabelNameByID = new Dictionary<DecorationType, string>();
            decorationByID = new Dictionary<DecorationType, List<DecorationItemSO>>();

            for (int i = 0; i < decorationTypes.Length; i++)
            {
                var decorations = DecorationItemResourceManager.Instance.GetDecorationByType(decorationTypes[i]);
                if (decorations == null)
                {
                    Debug.LogWarning($"[FuseTabModel] No decoration type of {decorationTypes[i].ToString()}");
                }

                decorationLabelNameByID.Add(decorationTypes[i], labelName[i]);
                decorationByID.Add(decorationTypes[i], decorations.ToList());
            }
        }

        public static List<FuseTabModel> FuseTabList = new List<FuseTabModel>()
        {
            new FuseTabModel(
                tabName: "Room",
                labelName: new string[]
                {
                    "Room"
                },
                decorationTypes: new DecorationType[]
                {
                    DecorationType.Room
                }
            ),

            new FuseTabModel(
                tabName: "Trophy",
                labelName: new string[]
                {
                    "Shelf",
                    "Floor",
                    "Wall"
                },
                decorationTypes: new DecorationType[]
                {
                    DecorationType.ShelfTrophy,
                    DecorationType.FloorTrophy,
                    DecorationType.WallTrophy
                }
            )
        };
    }
}