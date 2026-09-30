using System;
using System.Linq;
using QuizGame.Destination;
using QuizGame.Item;
using QuizGame.Item.Interfaces;

namespace QuizGame.Gameplay
{
    // GAME DESIGN DOC 02: multiplayer!H21:K25, ranking point!I2:I5.
    // Integer hundredths avoid rounding the 1.25/8.75/45.25 percentages.
    //
    // A later sheet (1JfXVq2D..., "Roulette" tab) carries a different table: SuperRare at double
    // these figures, Common higher, and no Grey at all in first place. Rare and Uncommon match.
    // Confirmed 2026-09-30 that DOC 02 is the one to follow, so the difference is deliberate —
    // please do not "correct" these numbers against that sheet without asking first.
    public static class LuckyDrawRules
    {
        private static readonly int[,] Weights = {
            { 125, 1000, 3000, 5000, 875 },
            { 100, 800, 2400, 4000, 2700 },
            { 75, 600, 1800, 3000, 4525 },
            { 50, 400, 1200, 2000, 6350 }
        };
        private static readonly ItemTier[] Tiers = { ItemTier.SuperRare, ItemTier.Rare, ItemTier.Uncommon, ItemTier.Common, ItemTier.NoTier };
        public static int RankingPoints(int place) { ValidatePlace(place); return new[] { 10, 5, 0, -5 }[place - 1]; }
        public static int Weight(int place, ItemTier tier) { ValidatePlace(place); return Weights[place - 1, Array.IndexOf(Tiers, tier)]; }
        public static ItemTier RollTier(int place, int roll)
        {
            ValidatePlace(place);
            if (roll < 0 || roll >= 10000) throw new ArgumentOutOfRangeException(nameof(roll));
            var total = 0;
            for (int i = 0; i < Tiers.Length; i++) { total += Weights[place - 1, i]; if (roll < total) return Tiers[i]; }
            throw new InvalidOperationException("Invalid drop table.");
        }
        private static void ValidatePlace(int place) { if (place < 1 || place > 4) throw new ArgumentOutOfRangeException(nameof(place)); }

        public static IItem[] Pool(IDestinationInfo destination, ItemTier tier)
        {
            if (destination == null) return Array.Empty<IItem>();
            var rewards = destination.GetItemRewardInDestination();
            var pool = tier == ItemTier.NoTier ? rewards.GetMaterialRewardItems() :
                (rewards.GetTrophyRewardItems() ?? Array.Empty<IItem>()).Concat(rewards.GetEquipmentRewardItems() ?? Array.Empty<IItem>()).ToArray();
            return (pool ?? Array.Empty<IItem>()).Where(x => x != null && !string.IsNullOrWhiteSpace(x.GetID()) &&
                (tier == ItemTier.NoTier || x.GetItemTier() == tier)).GroupBy(x => x.GetItemType() + ":" + x.GetID()).Select(x => x.First()).ToArray();
        }

        public static string MissingPools(IDestinationInfo destination)
        {
            if (destination == null) return "Destination";
            return string.Join(", ", Tiers.Where(t => Pool(destination, t).Length == 0).Select(t => t == ItemTier.NoTier ? "Grey (materials)" : t.ToString()));
        }

        // No reroll into another rarity when a pool is empty: that changes the published odds.
        public static ItemWithQuantityPair Choose(IDestinationInfo destination, int place, int tierRoll, int itemRoll, int greyQuantity)
        {
            if (greyQuantity <= 0 || !string.IsNullOrEmpty(MissingPools(destination))) return null;
            var tier = RollTier(place, tierRoll);
            var pool = Pool(destination, tier);
            if (itemRoll < 0 || itemRoll >= pool.Length) throw new ArgumentOutOfRangeException(nameof(itemRoll));
            return new ItemWithQuantityPair(pool[itemRoll], tier == ItemTier.NoTier ? greyQuantity : 1);
        }
        public static string OddsText(int place)
        {
            ValidatePlace(place);
            return string.Join("  |  ", Tiers.Select(t => (t == ItemTier.NoTier ? "Material" : t.ToString()) + " " +
                (Weight(place, t) / 100m).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%"));
        }
    }
}
