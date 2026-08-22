using System;
using UnityEngine;

namespace QuizGame.Gameplay.UI
{
    public class GameResultScreenWorldSpace : MonoBehaviour
    {
        [SerializeField]
        private PlayerGameResultInfoVisualization[] playerGameResultVisualizations;

        [SerializeField]
        private Sprite[] rankSprites;

        public void Init(PlayerGameResultData[] playerGameResultDatas)
        {
            for (int i = 0; i < playerGameResultVisualizations.Length; i++)
            {
                var visual = playerGameResultVisualizations[i];
                var isDataExist = i < playerGameResultDatas.Length;
                if (isDataExist)
                {
                    var data = playerGameResultDatas[i];
                    visual.Init(rankSprites[i], data.RankName, data.Name, data.Point);
                }
                else
                {
                    visual.Hide();
                }
            }
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
