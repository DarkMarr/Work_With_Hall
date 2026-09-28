using System;
using System.Linq;
using UnityEngine;

namespace QuizGame.Gameplay
{
    // The room/network adapter supplies a complete score snapshot for the current match.
    // No sample opponents or local estimates are substituted for missing server results.
    public class MultiplayerResultReceiver : MonoBehaviour
    {
        public event Action ResultsChanged;
        public string MatchId { get; private set; }
        public bool IsFinal { get; private set; }
        private PlayerGameResultData[] results = Array.Empty<PlayerGameResultData>();
        public PlayerGameResultData[] Results => results.Select(x => x.Copy()).ToArray();

        public void BeginMatch(string matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId)) throw new ArgumentException("Match ID is required.");
            MatchId = matchId;
            IsFinal = false;
            results = Array.Empty<PlayerGameResultData>();
            ResultsChanged?.Invoke();
        }

        // Call only when the room adapter has received the final result for every participant.
        public bool SetFinalResults(string matchId, PlayerGameResultData[] players)
        {
            if (players == null || players.Length != 4 || !SetResults(matchId, players)) return false;
            IsFinal = true;
            ResultsChanged?.Invoke();
            return true;
        }

        public bool SetResults(string matchId, PlayerGameResultData[] players)
        {
            if (IsFinal || string.IsNullOrEmpty(MatchId) || matchId != MatchId || players == null || players.Length < 1 || players.Length > 4
                || players.Any(p => p == null || string.IsNullOrWhiteSpace(p.UserId) || p.Point < 0)
                || players.Select(p => p.UserId).Distinct().Count() != players.Length) return false;
            results = players.Select(p => p.Copy()).ToArray();
            ResultsChanged?.Invoke();
            return true;
        }
    }
}
