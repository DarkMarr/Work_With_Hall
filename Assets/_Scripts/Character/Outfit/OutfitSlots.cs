using System.Collections.Generic;

namespace QuizGame.Character.Outfit
{
    /// <summary>
    /// Translates the layer names used inside the fashion PSBs into decoration slots.
    ///
    /// This deliberately does NOT reuse <see cref="CharacterSpriteUtilities"/>. The two file
    /// families spell the same words with different meanings: in a character PSB "L_arm" is the
    /// animal's own arm, while in a fashion PSB it is the sleeve worn on that arm. Routing fashion
    /// layers through the character table would drop a sleeve onto the limb slot and erase the arm.
    /// </summary>
    public static class OutfitSlots
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

        /// <summary>Every slot an outfit can fill, in the order a set should be applied.</summary>
        public static IEnumerable<CharacterPartType> AllSlots => slotByLayerName.Values;

        /// <summary>
        /// Reads the rarity out of a file name shaped <c>Outfit_&lt;Rarity&gt;_&lt;SetName&gt;</c>,
        /// e.g. "Outfit_SR_Pirate". False when the name does not follow the convention.
        ///
        /// "Fashion" is accepted as well: the design sheet calls these outfits and the code follows
        /// it, but the art was delivered under the older word and renaming those files is the Art
        /// team's to do. Both spellings work so neither side has to wait for the other.
        /// </summary>
        public static bool TryParseAssetName(string assetName, out OutfitRarity rarity, out string setName)
        {
            rarity = default;
            setName = null;
            if (string.IsNullOrWhiteSpace(assetName)) return false;

            var parts = assetName.Split('_');
            if (parts.Length < 3) return false;
            if (parts[0] != "Outfit" && parts[0] != "Fashion") return false;

            switch (parts[1])
            {
                case "C": rarity = OutfitRarity.Common; break;
                case "UC": rarity = OutfitRarity.Uncommon; break;
                case "R": rarity = OutfitRarity.Rare; break;
                case "SR": rarity = OutfitRarity.SuperRare; break;
                default: return false;
            }

            setName = string.Join("_", parts, 2, parts.Length - 2);
            return !string.IsNullOrWhiteSpace(setName);
        }
    }
}
