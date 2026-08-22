using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.Material;
using QuizGame.MyRoom.Decoration;
using System;
using UnityEngine;


namespace QuizGame.Destination
{
    [Serializable]
    public struct DestinationItemReward
    {
        [SerializeField]
        private DecorationItemSO[] trophyReward;

        [SerializeField]
        private EquipmentItemSO[] equipmentReward;

        [SerializeField]
        private MaterialInfoSO[] materialReward;

        public IItem[] GetTrophyRewardItems() => trophyReward;
        public IItem[] GetEquipmentRewardItems() => equipmentReward;
        public IItem[] GetMaterialRewardItems() => materialReward;
    }
}