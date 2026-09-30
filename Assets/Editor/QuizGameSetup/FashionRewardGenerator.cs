using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuizGame.Character;
using QuizGame.Character.Fashion;
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
    /// through FashionResourceManager without the Item assembly needing to know about Character.
    /// </summary>
    public static class FashionRewardGenerator
    {
        private const string FASHION_SET_PATH = "Assets/Resources/Fashion";
        private const string ITEM_OUTPUT_PATH = "Assets/Resources/Items/Fashion";
        private const string DESTINATION_PATH = "Assets/Resources/DestinationInfo";

        [MenuItem("QuizGame/Setup/Create Fashion Reward Items + Fill Destination Pools")]
        public static void CreateFashionRewards()
        {
            var sets = LoadSets();
            if (sets.Count == 0)
            {
                Debug.LogError("[FashionRewards] No FashionSetSO found. Run 'Create Fashion Set SO Assets' first.");
                return;
            }
            EnsureFolder(ITEM_OUTPUT_PATH);

            var itemsBySetId = new Dictionary<string, EquipmentItemSO>();
            int created = 0, updated = 0;
            foreach (var set in sets)
            {
                var item = CreateOrUpdateItem(set, ref created, ref updated);
                itemsBySetId[set.GetID()] = item;
            }
            AssetDatabase.SaveAssets();

            var filled = FillDestinationPools(itemsBySetId);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FashionRewards] Items: {created} created, {updated} updated. Destinations wired: {filled}.");
            ReportRemainingGaps();
        }

        private static List<FashionSetSO> LoadSets()
        {
            return AssetDatabase.FindAssets("t:FashionSetSO", new[] { FASHION_SET_PATH })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<FashionSetSO>)
                .Where(x => x != null)
                .OrderBy(x => x.GetID())
                .ToList();
        }

        private static EquipmentItemSO CreateOrUpdateItem(FashionSetSO set, ref int created, ref int updated)
        {
            var path = $"{ITEM_OUTPUT_PATH}/{set.GetID()}.asset";
            var item = AssetDatabase.LoadAssetAtPath<EquipmentItemSO>(path);
            var isNew = item == null;
            if (isNew)
            {
                item = ScriptableObject.CreateInstance<EquipmentItemSO>();
                AssetDatabase.CreateAsset(item, path);
            }

            var serialized = new SerializedObject(item);
            serialized.FindProperty("itemID").stringValue = set.GetID();
            serialized.FindProperty("equipmentTier").enumValueIndex = (int)ToItemTier(set.GetRarity());
            serialized.FindProperty("itemSprite").objectReferenceValue = PickIcon(set);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);

            if (isNew) created++; else updated++;
            return item;
        }

        /// <summary>
        /// The body piece reads best at icon size; a set without one falls back to whatever it has.
        /// </summary>
        private static Sprite PickIcon(FashionSetSO set)
        {
            return set.GetPiece(CharacterPartType.BodyDecoration)
                ?? set.GetPiece(CharacterPartType.HeadDecoration)
                ?? set.GetPieces().Select(p => p.Sprite).FirstOrDefault(s => s != null);
        }

        private static ItemTier ToItemTier(FashionRarity rarity)
        {
            switch (rarity)
            {
                case FashionRarity.Common: return ItemTier.Common;
                case FashionRarity.Uncommon: return ItemTier.Uncommon;
                case FashionRarity.Rare: return ItemTier.Rare;
                case FashionRarity.SuperRare: return ItemTier.SuperRare;
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
        private static int FillDestinationPools(Dictionary<string, EquipmentItemSO> itemsBySetId)
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
                    Debug.LogWarning($"[FashionRewards] {cityName} is not in the Map drop sheet; " +
                        "it keeps every Common set so its pool does not fall empty.");
                }

                foreach (var pair in itemsBySetId.OrderBy(x => x.Key))
                {
                    if (!FashionSlots.TryParseAssetName(pair.Key, out var rarity, out var setName)) continue;

                    bool belongsHere;
                    switch (rarity)
                    {
                        case FashionRarity.Rare:
                        case FashionRarity.Uncommon:
                            belongsHere = string.Equals(setName, cityName, System.StringComparison.OrdinalIgnoreCase);
                            break;
                        case FashionRarity.Common:
                            belongsHere = commonSet == null
                                || string.Equals(setName, commonSet, System.StringComparison.OrdinalIgnoreCase);
                            break;
                        default:
                            belongsHere = true;
                            break;
                    }

                    if (belongsHere) wanted.Add(pair.Value);
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
                    Debug.Log($"[FashionRewards] {destination.GetID()}: all pools filled.");
                }
                else
                {
                    Debug.LogWarning($"[FashionRewards] {destination.GetID()}: still missing {missing}.");
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
