using QuizGame.Interfaces;
using TMPro;
using UnityEngine;

public class LeaderboardElement : MonoBehaviour, IHasRectTransform
{
    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private TextMeshProUGUI rankNumberText;

    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [SerializeField]
    private TextMeshProUGUI rankNameText;

    [SerializeField]
    private TextMeshProUGUI rankScoreText;

    public void Setup(int rankNumber, string playerName, string rankName, int rankScore)
    {
        rankNumberText.text = rankNumber.ToString();
        playerNameText.text = playerName;
        rankNameText.text = rankName;
        rankScoreText.text = rankScore.ToString();
    }

    private void OnValidate()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public RectTransform GetRectTransform() => rectTransform;
}
