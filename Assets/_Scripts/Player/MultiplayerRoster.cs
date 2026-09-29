using System.Collections.Generic;

namespace QuizGame.Player
{
    public readonly struct OpponentInfo
    {
        public OpponentInfo(string displayName, string characterId)
        {
            DisplayName = displayName;
            CharacterId = characterId;
        }

        public string DisplayName { get; }

        /// <summary>Id of a CharacterInfoSO. Callers resolve it to a prefab themselves.</summary>
        public string CharacterId { get; }
    }

    /// <summary>
    /// The single answer to "who am I playing against". The lobby and the match both read it, so a
    /// player sees the same three opponents in both places.
    ///
    /// No room adapter exists yet, so <see cref="EnsurePlaceholders"/> invents a roster the first
    /// time it is asked and keeps it for the rest of the match. When the server arrives, call
    /// <see cref="SetFromServer"/> as soon as the room is known and nothing else has to change —
    /// <see cref="IsFromServer"/> tells the result screen whether these names can be trusted.
    /// </summary>
    public static class MultiplayerRoster
    {
        public const int OpponentCount = 3;

        private static readonly List<OpponentInfo> opponents = new List<OpponentInfo>(OpponentCount);

        public static IReadOnlyList<OpponentInfo> Opponents => opponents;

        /// <summary>True once a real roster has been supplied; false while these are stand-ins.</summary>
        public static bool IsFromServer { get; private set; }

        public static void SetFromServer(IEnumerable<OpponentInfo> roster)
        {
            opponents.Clear();
            if (roster != null) opponents.AddRange(roster);
            IsFromServer = true;
        }

        /// <summary>
        /// Fills the roster with stand-ins if it is empty. Character ids are passed in so this stays
        /// free of any dependency on how characters are stored.
        /// </summary>
        public static void EnsurePlaceholders(IReadOnlyList<string> availableCharacterIds)
        {
            if (opponents.Count >= OpponentCount) return;

            IsFromServer = false;
            opponents.Clear();

            // Draw without replacement while there are enough characters, so the table does not
            // show the same avatar twice.
            var pool = availableCharacterIds == null ? new List<string>() : new List<string>(availableCharacterIds);
            for (int i = 0; i < OpponentCount; i++)
            {
                string characterId = null;
                if (pool.Count > 0)
                {
                    var pick = UnityEngine.Random.Range(0, pool.Count);
                    characterId = pool[pick];
                    if (pool.Count > OpponentCount - i - 1) pool.RemoveAt(pick);
                }
                opponents.Add(new OpponentInfo($"Player {UnityEngine.Random.Range(1000, 10000)}", characterId));
            }
        }

        /// <summary>Call when a match ends so the next one draws a fresh roster.</summary>
        public static void Clear()
        {
            opponents.Clear();
            IsFromServer = false;
        }
    }
}
