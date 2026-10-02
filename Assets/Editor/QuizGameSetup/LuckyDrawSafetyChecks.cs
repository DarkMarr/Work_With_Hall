using System;
using System.Linq;
using QuizGame.Gameplay;
using QuizGame.Network;
using QuizGame.Network.FirestoreDataModels;
using QuizGame.Item;
using UnityEditor;
using UnityEngine;

namespace QuizGame.EditorTools
{
    // Offline regression checks: no Firebase connection or player account is required.
    public static class LuckyDrawSafetyChecks
    {
        [MenuItem("HALL900/Testing/Check Lucky Draw Safety (Offline)")]
        public static void Run()
        {
            var host = new GameObject("LuckyDrawSafetyChecks");
            host.SetActive(false); // Avoid singleton Awake/Start, including Firebase initialization.
            try
            {
                var receiver = host.AddComponent<MultiplayerResultReceiver>();
                var players = Enumerable.Range(0, 4).Select(i => new PlayerGameResultData("Player " + i, 40 - i * 10)
                    { UserId = "audit-player-" + i }).ToArray();
                int notifications = 0;
                bool officialPreviewObserved = false;
                receiver.ResultsChanged += () =>
                {
                    notifications++;
                    if (receiver.MatchId == "standin-audit" && receiver.IsFinal && receiver.IsFromServer)
                        officialPreviewObserved = true;
                };
                Check(receiver.SetStandInResults("standin-audit", players), "stand-in accepted");
                Check(notifications == 1 && !officialPreviewObserved && !receiver.IsFromServer,
                    "listeners see one complete preview snapshot, never official rewards");
                players[0].Point = 999;
                Check(receiver.Results[0].Point == 40, "input snapshot is copied");
                var copy = receiver.Results;
                copy[0].Point = 888;
                Check(receiver.Results[0].Point == 40, "output snapshot is copied");
                Check(!receiver.SetStandInResults("standin-other", players), "final snapshot cannot be overwritten");

                receiver.BeginMatch("server-audit");
                Check(receiver.SetResults("server-audit", players.Take(2).ToArray()), "partial server snapshot accepted");
                Check(!receiver.SetStandInResults("standin-invalid", new[] { players[0], players[0], players[2], players[3] }),
                    "duplicate users rejected");
                Check(receiver.MatchId == "server-audit" && receiver.Results.Length == 2 && !receiver.IsFinal,
                    "rejected preview leaves existing match untouched");
                Check(receiver.SetFinalResults("server-audit", players) && receiver.IsFromServer, "server final remains official");
                Check(!receiver.SetStandInResults("standin-audit", players), "preview cannot overwrite server final");
                receiver.BeginMatch("standin-reserved");
                Check(!receiver.SetFinalResults("standin-reserved", players), "reserved preview IDs cannot become official");
                Check(!receiver.SetStandInResults("server-audit", players), "stand-ins require reserved prefix");

                var data = host.AddComponent<PlayerDataManager>();
                foreach (var matchId in new[] { "standin-audit", "STANDIN-audit" })
                {
                    var claim = data.ClaimLuckyDraw(matchId, "audit-player-0", new MatchRewardReceipt
                    { ItemId = "audit-item", Type = "Equipment", Quantity = 1, Place = 1 });
                    Check(claim.IsCompleted && claim.GetAwaiter().GetResult() == null,
                        "stand-in claims rejected before accessing Firebase: " + matchId);
                }
                // One asset per garment rather than per outfit, which is how the drop tables award
                // them, so the count is the number of garments and not the number of sets.
                var outfits = UnityEngine.Resources.LoadAll<EquipmentItemSO>("Items/Outfit");
                Check(outfits.Length > 0, "generated outfit rewards found: " + outfits.Length);
                foreach (var item in outfits)
                {
                    Check(!string.IsNullOrWhiteSpace(item.GetName()), "reward name available: " + item.GetID());
                    item.GetDescription();
                    item.GetSubDescription();
                }
                var baseballHat = outfits.SingleOrDefault(x => x.GetID() == "Fashion_C_Baseball__HeadDecoration");
                Check(baseballHat != null && baseballHat.GetName() == "Baseball Head Decoration",
                    "missing translation falls back to a readable outfit name");
                Debug.Log("[HALL900] Lucky Draw safety checks PASS: atomic preview notifications, snapshot isolation, "
                    + "server protection, offline claim rejection, " + outfits.Length + " outfit names.");
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }

        private static void Check(bool passed, string description)
        {
            if (!passed) throw new InvalidOperationException("Lucky Draw safety check failed: " + description);
        }
    }
}
