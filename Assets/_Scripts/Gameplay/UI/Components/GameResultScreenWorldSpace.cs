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
                visual.gameObject.name = "ResultSlot_" + (i + 1);
                var isDataExist = playerGameResultDatas != null && i < playerGameResultDatas.Length;
                if (isDataExist)
                {
                    var data = playerGameResultDatas[i];
                    visual.Init(i < rankSprites.Length ? rankSprites[i] : null, data);
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
