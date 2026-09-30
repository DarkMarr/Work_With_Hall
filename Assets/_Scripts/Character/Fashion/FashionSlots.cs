using System.Collections.Generic;

namespace QuizGame.Character.Fashion
{
    /// <summary>
    /// Translates the layer names used inside the fashion PSBs into decoration slots.
    ///
    /// This deliberately does NOT reuse <see cref="CharacterSpriteUtilities"/>. The two file
    /// families spell the same words with different meanings: in a character PSB "L_arm" is the
    /// animal's own arm, while in a fashion PSB it is the sleeve worn on that arm. Routing fashion
    /// layers through the character table would drop a sleeve onto the limb slot and erase the arm.
    /// </summary>
    public static class FashionSlots
    {
        private static readonly Dictionary<string, CharacterPartType> slotByLayerName =
            new Dictionary<string, CharacterPartType>
            {
                { "hat", CharacterPartType.HeadDecoration },
                { "lower", CharacterPartType.BodyDecoration },
                { "back", CharacterPartType.BackDecoration },
                { "larm", CharacterPartType.ArmDecorationLeft },
                { "rarm", CharacterPartType.ArmDecorationRight },
                { "prop", CharacterPartType.Prop }
            };

        /// <summary>
        /// False for a layer that is not a wearable piece — the "template" guide layer, say — so
        /// callers can skip it rather than inventing a slot for it.
        /// </summary>
        public static bool TryGetSlot(string layerName, out CharacterPartType slot)
        {
            slot = default;
            if (string.IsNullOrWhiteSpace(layerName)) return false;
            return slotByLayerName.TryGetValue(
                CharacterSpriteUtilities.NormalizeCategoryName(layerName), out slot);
        }

        /// <summary>Every slot a fashion set can fill, in the order a set should be applied.</summary>
        public static IEnumerable<CharacterPartType> AllSlots => slotByLayerName.Values;

        /// <summary>
        /// Reads the rarity out of a file name shaped <c>Fashion_&lt;Rarity&gt;_&lt;SetName&gt;</c>,
        /// e.g. "Fashion_SR_Pirate". False when the name does not follow the convention.
        /// </summary>
        public static bool TryParseAssetName(string assetName, out FashionRarity rarity, out string setName)
        {
            rarity = default;
            setName = null;
            if (string.IsNullOrWhiteSpace(assetName)) return false;

            var parts = assetName.Split('_');
            if (parts.Length < 3 || parts[0] != "Fashion") return false;

            switch (parts[1])
            {
                case "C": rarity = FashionRarity.Common; break;
                case "UC": rarity = FashionRarity.Uncommon; break;
                case "R": rarity = FashionRarity.Rare; break;
                case "SR": rarity = FashionRarity.SuperRare; break;
                default: return false;
            }

            setName = string.Join("_", parts, 2, parts.Length - 2);
            return !string.IsNullOrWhiteSpace(setName);
        }
    }
}
