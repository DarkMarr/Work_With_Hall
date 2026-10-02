using System.Collections.Generic;
using System.IO;
using QuizGame.Item;
using UnityEditor;
using UnityEngine;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Gives every garment the inventory icon the art team drew for it.
    ///
    /// Until now a garment's icon was the same 600x600 sprite it wears on the character, so a shop
    /// tile showed one small object adrift in a mostly empty square — the frame is sized for a whole
    /// body, not for a hat. Assets/Art/UI/ITEM/OUTFIT holds a 256x256 icon per garment, named after
    /// the item id in ALL ITEM DATA, and Docs/Outfit-Icons.csv is the bridge between those numbers
    /// and this build's asset names.
    ///
    /// Only <c>itemSprite</c> changes. <c>pieceSprites</c> stays the art that is actually worn.
    /// </summary>
    public static class OutfitIconImporter
    {
        private const string ICON_CSV = "Docs/Outfit-Icons.csv";
        private const string ICON_FOLDER = "Assets/Art/UI/ITEM/OUTFIT";
        private const string ITEM_FOLDER = "Assets/Resources/Items/Outfit";

        [MenuItem("QuizGame/Setup/Apply Outfit Icons")]
        public static void Apply()
        {
            var byItem = ReadMapping();
            if (byItem == null) return;

            int applied = 0, unchanged = 0;
            var noIcon = new List<string>();
            var noRow = new List<string>();

            foreach (var guid in AssetDatabase.FindAssets("t:OutfitItemSO", new[] { ITEM_FOLDER }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var garment = AssetDatabase.LoadAssetAtPath<OutfitItemSO>(path);
                if (garment == null) continue;

                if (!byItem.TryGetValue(garment.GetID(), out var iconId))
                {
                    noRow.Add(garment.GetID());
                    continue;
                }

                var icon = LoadIcon(iconId);
                if (icon == null)
                {
                    noIcon.Add(garment.GetID() + " -> " + iconId);
                    continue;
                }

                var serialized = new SerializedObject(garment);
                var property = serialized.FindProperty("itemSprite");
                if (property == null) continue;

                if (property.objectReferenceValue == icon) { unchanged++; continue; }

                property.objectReferenceValue = icon;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(garment);
                applied++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[OutfitIcons] {applied} icon(s) applied, {unchanged} already correct.");
            if (noRow.Count > 0)
            {
                Debug.LogWarning($"[OutfitIcons] {noRow.Count} garment(s) have no row in {ICON_CSV}: "
                    + string.Join(", ", noRow.ToArray(), 0, Mathf.Min(5, noRow.Count)));
            }
            if (noIcon.Count > 0)
            {
                Debug.LogWarning($"[OutfitIcons] {noIcon.Count} icon file(s) named in {ICON_CSV} are missing "
                    + $"from {ICON_FOLDER}: " + string.Join(", ", noIcon.ToArray(), 0, Mathf.Min(5, noIcon.Count)));
            }
        }

        /// <summary>
        /// The icon pngs import as Multiple, so the file holds a sprite named after itself with a
        /// _0 suffix rather than being a sprite in its own right. Take whichever sprite is in there
        /// instead of relying on that spelling, in case the import settings are ever changed.
        /// </summary>
        private static Sprite LoadIcon(string iconId)
        {
            var path = $"{ICON_FOLDER}/{iconId}.png";
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite) return sprite;
            }
            return null;
        }

        private static Dictionary<string, string> ReadMapping()
        {
            var full = Path.Combine(Directory.GetCurrentDirectory(), ICON_CSV);
            if (!File.Exists(full))
            {
                Debug.LogError("[OutfitIcons] Mapping not found: " + ICON_CSV);
                return null;
            }

            var result = new Dictionary<string, string>();
            foreach (var raw in File.ReadAllLines(full))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                var cells = line.Split(',');
                if (cells.Length < 2) continue;
                if (string.Equals(cells[0].Trim(), "ItemID", System.StringComparison.OrdinalIgnoreCase)) continue;

                result[cells[0].Trim()] = cells[1].Trim();
            }
            Debug.Log($"[OutfitIcons] Read {result.Count} mapping(s) from {ICON_CSV}.");
            return result;
        }
    }
}
