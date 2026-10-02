using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuizGame.Character;
using QuizGame.Character.Outfit;
using QuizGame.Destination;
using QuizGame.Item;
using UnityEditor;
using UnityEngine;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Turns each fashion set into a drawable equipment item and puts it in the reward pools.
    ///
    /// The lucky draw refuses to run while any rarity pool is empty — rerolling into another
    /// rarity would quietly change the published odds — and every destination was missing Rare,
    /// Uncommon and Common. The fashion sets are exactly those three tiers, so wiring them in is
    /// what lets the draw open at all.
    ///
    /// The equipment item's id is the fashion set's id, so winning one resolves back to the outfit
    /// through OutfitSetResourceManager without the Item assembly needing to know about Character.
    /// </summary>
    public static class OutfitRewardGenerator
    {
        private const string FASHION_SET_PATH = "Assets/Resources/Outfit";
        private const string ITEM_OUTPUT_PATH = "Assets/Resources/Items/Outfit";
        private const string DESTINATION_PATH = "Assets/Resources/DestinationInfo";

        [MenuItem("QuizGame/Setup/Create Outfit Reward Items + Fill Destination Pools")]
        public static void CreateFashionRewards()
        {
            var sets = LoadSets();
            if (sets.Count == 0)
            {
                Debug.LogError("[OutfitRewards] No OutfitSetSO found. Run 'Create Fashion Set SO Assets' first.");
                return;
            }
            EnsureFolder(ITEM_OUTPUT_PATH);

            var piecesBySetId = new Dictionary<string, List<OutfitItemSO>>();
            int created = 0, updated = 0;
            foreach (var set in sets)
            {
                piecesBySetId[set.GetID()] = CreateOrUpdatePieces(set, ref created, ref updated);
            }
            RemoveItemsNotGenerated(new HashSet<string>(
                piecesBySetId.Values.SelectMany(x => x).Select(x => x.GetID())));
            AssetDatabase.SaveAssets();

            var filled = FillDestinationPools(piecesBySetId);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[OutfitRewards] Items: {created} created, {updated} updated. Destinations wired: {filled}.");
            ReportRemainingGaps();
        }

        private static List<OutfitSetSO> LoadSets()
        {
            return AssetDatabase.FindAssets("t:OutfitSetSO", new[] { FASHION_SET_PATH })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<OutfitSetSO>)
                .Where(x => x != null)
                .OrderBy(x => x.GetID())
                .ToList();
        }

        /// <summary>
        /// The item list defines exactly four wearable slots — head, body, hand and back — so the
        /// left and right sleeves, which the art keeps as their own layers, belong to the body
        /// garment rather than being items of their own. Anything else is left out entirely.
        /// </summary>
        private static readonly Dictionary<CharacterPartType, CharacterPartType> itemSlotForPiece =
            new Dictionary<CharacterPartType, CharacterPartType>
            {
                { CharacterPartType.HeadDecoration, CharacterPartType.HeadDecoration },
                { CharacterPartType.BodyDecoration, CharacterPartType.BodyDecoration },
                { CharacterPartType.ArmDecorationLeft, CharacterPartType.BodyDecoration },
                { CharacterPartType.ArmDecorationRight, CharacterPartType.BodyDecoration },
                { CharacterPartType.Prop, CharacterPartType.Prop },
                { CharacterPartType.BackDecoration, CharacterPartType.BackDecoration }
            };

        /// <summary>
        /// One asset per garment, not per outfit: the drop tables award "Head of set ฤดูร้อน" as a
        /// single reward. Ids read Fashion_C_Summer__HeadDecoration so the set is still legible.
        /// </summary>
        private static List<OutfitItemSO> CreateOrUpdatePieces(OutfitSetSO set, ref int created, ref int updated)
        {
            var results = new List<OutfitItemSO>();

            // Gather the art first: a body garment ends up holding its lower piece and both sleeves.
            var spritesBySlot = new Dictionary<CharacterPartType, List<Sprite>>();
            foreach (var piece in set.GetPieces())
            {
                if (piece.Sprite == null) continue;
                if (!itemSlotForPiece.TryGetValue(piece.Slot, out var itemSlot))
                {
                    Debug.LogWarning($"[OutfitRewards] {set.GetID()}: '{piece.Slot}' is not one of the " +
                        "four item slots and was left out.");
                    continue;
                }
                if (!spritesBySlot.TryGetValue(itemSlot, out var list))
                {
                    spritesBySlot[itemSlot] = list = new List<Sprite>();
                }
                list.Add(piece.Sprite);
            }

            foreach (var entry in spritesBySlot)
            {
                var itemId = set.GetID() + "__" + entry.Key;
                var path = $"{ITEM_OUTPUT_PATH}/{itemId}.asset";
                var item = AssetDatabase.LoadAssetAtPath<OutfitItemSO>(path);
                var isNew = item == null;
                if (isNew)
                {
                    item = ScriptableObject.CreateInstance<OutfitItemSO>();
                    AssetDatabase.CreateAsset(item, path);
                }

                var serialized = new SerializedObject(item);
                serialized.FindProperty("itemID").stringValue = itemId;
                serialized.FindProperty("equipmentTier").enumValueIndex =
                    (int)ToItemTier(set.GetRarity(), entry.Key);
                // The lower piece reads better as an icon than a sleeve does.
                serialized.FindProperty("itemSprite").objectReferenceValue = entry.Value[0];
                serialized.FindProperty("slot").stringValue = entry.Key.ToString();
                serialized.FindProperty("outfitSetID").stringValue = set.GetID();

                var sprites = serialized.FindProperty("pieceSprites");
                sprites.ClearArray();
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    sprites.InsertArrayElementAtIndex(i);
                    sprites.GetArrayElementAtIndex(i).objectReferenceValue = entry.Value[i];
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(item);

                if (isNew) created++; else updated++;
                results.Add(item);
            }
            return results;
        }

        /// <summary>
        /// Rarity belongs to the garment, not to the PSB it was drawn in. The item list gives each
        /// city two Rare garments — the head and the held item — and two Ultra Rare ones, the body
        /// and the back effect, and the art keeps all four in the one Fashion_R_&lt;City&gt; file.
        /// Reading the tier off the file name alone would mark the two Ultra Rare garments as Rare
        /// and hand them out at eight times their intended odds.
        ///
        /// Source: sheet 15_KKnDD..., OUTFIT tab, rows 213007-244010.
        /// </summary>
        private static ItemTier ToItemTier(OutfitRarity setRarity, CharacterPartType slot)
        {
            if (setRarity == OutfitRarity.Rare)
            {
                return slot == CharacterPartType.HeadDecoration || slot == CharacterPartType.Prop
                    ? ItemTier.Rare
                    : ItemTier.SuperRare;
            }

            switch (setRarity)
            {
                case OutfitRarity.Common: return ItemTier.Common;
                case OutfitRarity.Uncommon: return ItemTier.Uncommon;
                case OutfitRarity.SuperRare: return ItemTier.SuperRare;
                default: return ItemTier.NoTier;
            }
        }

        /// <summary>
        /// Which Common set each destination hands out, from the design sheet's "Map drop" tab
        /// (spreadsheet 1JfXVq2D...). Half the maps give the summer set and half the lollipop one;
        /// the sheet names no map for Baseball, CloudVibe, Floral or LeisureDays, so those four do
        /// not drop from maps at all and are left out rather than guessed at.
        /// </summary>
        private static readonly Dictionary<string, string> commonSetByCity =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "Bangkok", "Summer" },
                { "Tokyo", "Summer" },
                { "Cairo", "Summer" },
                { "NewYork", "Lollipop" },
                { "Paris", "Lollipop" },
                { "London", "Lollipop" }
            };

        /// <summary>
        /// Rare and Uncommon sets are the sheet's "map unique" entries, so each goes only to the
        /// city it is named after. Common follows <see cref="commonSetByCity"/>. SuperRare is
        /// given to every map: the sheet calls the red ball map-unique too, but the art is four
        /// untitled sets rather than one per city, so there is nothing to key on yet.
        /// </summary>
        /// <summary>
        /// Deletes anything in the output folder this run did not produce: the whole-outfit assets
        /// an earlier version wrote, and the separate sleeve items from when the sleeves were
        /// mistaken for garments of their own. Without this a pool could hold a set, its garments
        /// and its sleeves all at once.
        /// </summary>
        private static void RemoveItemsNotGenerated(HashSet<string> keepIds)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:EquipmentItemSO", new[] { ITEM_OUTPUT_PATH }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var id = Path.GetFileNameWithoutExtension(path);
                if (keepIds.Contains(id)) continue;
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[OutfitRewards] Removed '{id}': not a garment in the current item list.");
            }
        }

        private static int FillDestinationPools(Dictionary<string, List<OutfitItemSO>> piecesBySetId)
        {
            var destinations = AssetDatabase.FindAssets("t:DestinationInfoSO", new[] { DESTINATION_PATH })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<DestinationInfoSO>)
                .Where(x => x != null)
                .ToList();

            int wired = 0;
            foreach (var destination in destinations)
            {
                var cityName = destination.GetDestinationType().ToString();
                var wanted = new List<EquipmentItemSO>();

                commonSetByCity.TryGetValue(cityName, out var commonSet);
                if (commonSet == null)
                {
                    Debug.LogWarning($"[OutfitRewards] {cityName} is not in the Map drop sheet; " +
                        "it keeps every Common set so its pool does not fall empty.");
                }

                foreach (var pair in piecesBySetId.OrderBy(x => x.Key))
                {
                    if (!OutfitSlots.TryParseAssetName(pair.Key, out var rarity, out var setName)) continue;

                    bool belongsHere;
                    switch (rarity)
                    {
                        case OutfitRarity.Rare:
                        case OutfitRarity.Uncommon:
                            belongsHere = string.Equals(setName, cityName, System.StringComparison.OrdinalIgnoreCase);
                            break;
                        case OutfitRarity.Common:
                            belongsHere = commonSet == null
                                || string.Equals(setName, commonSet, System.StringComparison.OrdinalIgnoreCase);
                            break;
                        default:
                            belongsHere = true;
                            break;
                    }

                    if (belongsHere) wanted.AddRange(pair.Value);
                }

                if (ApplyEquipmentPool(destination, wanted)) wired++;
            }
            return wired;
        }

        /// <summary>
        /// Rewrites only the fashion part of the pool so hand-authored equipment survives, and so
        /// running this twice does not duplicate entries.
        /// </summary>
        private static bool ApplyEquipmentPool(DestinationInfoSO destination, List<EquipmentItemSO> fashionItems)
        {
            var serialized = new SerializedObject(destination);
            var pool = serialized.FindProperty("destinationItemRewards").FindPropertyRelative("equipmentReward");

            var kept = new List<Object>();
            for (int i = 0; i < pool.arraySize; i++)
            {
                var existing = pool.GetArrayElementAtIndex(i).objectReferenceValue as EquipmentItemSO;
                if (existing == null) continue;
                if (existing.GetID() != null && existing.GetID().StartsWith("Fashion_")) continue;
                kept.Add(existing);
            }
            kept.AddRange(fashionItems);

            pool.ClearArray();
            for (int i = 0; i < kept.Count; i++)
            {
                pool.InsertArrayElementAtIndex(i);
                pool.GetArrayElementAtIndex(i).objectReferenceValue = kept[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(destination);
            return true;
        }

        /// <summary>Says plainly whether the lucky draw can now open for each destination.</summary>
        private static void ReportRemainingGaps()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:DestinationInfoSO", new[] { DESTINATION_PATH }))
            {
                var destination = AssetDatabase.LoadAssetAtPath<DestinationInfoSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (destination == null) continue;
                var missing = Gameplay.LuckyDrawRules.MissingPools(destination);
                if (string.IsNullOrEmpty(missing))
                {
                    Debug.Log($"[OutfitRewards] {destination.GetID()}: all pools filled.");
                }
                else
                {
                    Debug.LogWarning($"[OutfitRewards] {destination.GetID()}: still missing {missing}.");
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
