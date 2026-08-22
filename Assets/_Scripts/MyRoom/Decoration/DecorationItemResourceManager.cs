using System;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Resources;
using UnityEngine;

namespace QuizGame.MyRoom.Decoration
{
    public class DecorationItemResourceManager : ResourceManager<DecorationItemResourceManager, DecorationItemSO>
    {
        public override string ContentResourcePath => "Items/Decoration";

        private Dictionary<DecorationType, List<DecorationItemSO>> decorationsByType = new Dictionary<DecorationType, List<DecorationItemSO>>();

        protected override void Awake()
        {
            base.Awake();

            var allDecorationsType = Enum.GetValues(typeof(DecorationType)).Cast<DecorationType>();
            foreach (var decorationType in allDecorationsType)
            {
                decorationsByType.Add(decorationType, new List<DecorationItemSO>());
            }

            foreach (var decorationSO in GetAllResources())
            {
                var decorationType = decorationSO.GetDecorationType();
                if (decorationsByType.ContainsKey(decorationType))
                {
                    decorationsByType[decorationType].Add(decorationSO);
                }
                else
                {
                    Debug.LogError($"[{GetType().Name}] No decoration type: {decorationType}");
                }
            }
        }

        public Dictionary<DecorationType, List<DecorationItemSO>> GetDecorationAllTypes() => decorationsByType;

        public DecorationItemSO[] GetDecorationByType(DecorationType decorationType)
        {
            if (decorationsByType.TryGetValue(decorationType, out var decorationItemSOs))
            {
                return decorationItemSOs.ToArray();
            }
            Debug.LogError($"[{GetType().Name}] No decoration type: {decorationsByType}");
            return null;
        }
    }
}
