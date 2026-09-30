using System;
using System.Linq;
using UnityEngine;

namespace QuizGame.Gameplay
{
    // The room/network adapter supplies a complete score snapshot for the current match.
    // Local stand-ins are explicitly marked as previews, never as official results.
    public class MultiplayerResultReceiver : MonoBehaviour
    {
        public event Action ResultsChanged;
        public string MatchId { get; private set; }
        public bool IsFinal { get; private set; }

        /// <summary>
        /// False while <see cref="SetStandInResults"/> supplied the scores. Anything that reports a
        /// placement as official — a leaderboard, a rank change the player is told about — should
        /// check this first.
        /// </summary>
        public bool IsFromServer { get; private set; } = true;
        private PlayerGameResultData[] results = Array.Empty<PlayerGameResultData>();
        public PlayerGameResultData[] Results => results.Select(x => x.Copy()).ToArray();

        public void BeginMatch(string matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId)) throw new ArgumentException("Match ID is required.");
            MatchId = matchId;
            IsFinal = false;
            IsFromServer = true;
            results = Array.Empty<PlayerGameResultData>();
            ResultsChanged?.Invoke();
        }

        /// <summary>
        /// Closes the match with locally composed scores, for use only while no room adapter
        /// exists. Without it <see cref="IsFinal"/> never becomes true, every match ends on
        /// "results pending", and the reward and lucky draw screens are unreachable.
        ///
        /// This is the same seam as MultiplayerRoster: when the server arrives, call
        /// <see cref="SetFinalResults"/> from the adapter instead and delete the single call site.
        /// Refuses to overwrite a real snapshot, so wiring the server up cannot regress into this.
        /// </summary>
        public bool SetStandInResults(string matchId, PlayerGameResultData[] players)
        {
            if (IsFinal || string.IsNullOrEmpty(matchId) || !matchId.StartsWith("standin-", StringComparison.Ordinal)
                || players == null || players.Length != 4 || !AreValidPlayers(players)) return false;
            // Publish one complete snapshot. Calling SetFinalResults first would notify listeners
            // while IsFromServer was still true, allowing preview scores into the reward path.
            MatchId = matchId;
            results = players.Select(p => p.Copy()).ToArray();
            IsFromServer = false;
            IsFinal = true;
            ResultsChanged?.Invoke();
            return true;
        }

        // Call only when the room adapter has received the final result for every participant.
        public bool SetFinalResults(string matchId, PlayerGameResultData[] players)
        {
            if (IsFinal || string.IsNullOrEmpty(MatchId) || matchId != MatchId
                || matchId.StartsWith("standin-", StringComparison.Ordinal)
                || players == null || players.Length != 4 || !AreValidPlayers(players)) return false;
            results = players.Select(p => p.Copy()).ToArray();
            IsFromServer = true;
            IsFinal = true;
            ResultsChanged?.Invoke();
            return true;
        }

        public bool SetResults(string matchId, PlayerGameResultData[] players)
        {
            if (IsFinal || string.IsNullOrEmpty(MatchId) || matchId != MatchId || players == null || players.Length < 1 || players.Length > 4
                || !AreValidPlayers(players)) return false;
            results = players.Select(p => p.Copy()).ToArray();
            ResultsChanged?.Invoke();
            return true;
        }

        private static bool AreValidPlayers(PlayerGameResultData[] players) =>
            !players.Any(p => p == null || string.IsNullOrWhiteSpace(p.UserId) || p.Point < 0)
            && players.Select(p => p.UserId).Distinct().Count() == players.Length;
    }
}
