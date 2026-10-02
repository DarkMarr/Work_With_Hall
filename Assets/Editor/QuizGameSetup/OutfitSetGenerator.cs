using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuizGame.Character;
using QuizGame.Character.Outfit;
using UnityEditor;
using UnityEngine;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Builds one <see cref="OutfitSetSO"/> per fashion PSB so the sets are loadable at runtime.
    ///
    /// The art lives outside Resources, so the sprites cannot be found by path at runtime — the
    /// generated assets in Resources/Fashion hold the references that keep them in the build.
    /// </summary>
    public static class OutfitSetGenerator
    {
        private const string SOURCE_PATH = "Assets/Art/Sprites/Fashion";
        private const string OUTPUT_PATH = "Assets/Resources/Outfit";

        [MenuItem("QuizGame/Setup/Create Outfit Set SO Assets (from outfit PSBs)")]
        public static void CreateFashionSets()
        {
            if (!AssetDatabase.IsValidFolder(SOURCE_PATH))
            {
                Debug.LogError($"[OutfitSetGenerator] '{SOURCE_PATH}' not found.");
                return;
            }
            EnsureFolder(OUTPUT_PATH);

            var created = 0;
            var updated = 0;
            var skipped = new List<string>();

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SOURCE_PATH }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var assetName = Path.GetFileNameWithoutExtension(assetPath);

                if (!OutfitSlots.TryParseAssetName(assetName, out var rarity, out var setName))
                {
                    skipped.Add($"{assetName} (name is not Fashion_<Rarity>_<Set>)");
                    continue;
                }

                var pieces = ReadPieces(assetPath, out var unmapped);
                if (pieces.Count == 0)
                {
                    skipped.Add($"{assetName} (no recognised slot sprites)");
                    continue;
                }
                if (unmapped.Count > 0)
                {
                    Debug.LogWarning($"[OutfitSetGenerator] {assetName}: ignored layers {string.Join(", ", unmapped)}.");
                }

                var soPath = $"{OUTPUT_PATH}/{assetName}.asset";
                var so = AssetDatabase.LoadAssetAtPath<OutfitSetSO>(soPath);
                var isNew = so == null;
                if (isNew)
                {
                    so = ScriptableObject.CreateInstance<OutfitSetSO>();
                    AssetDatabase.CreateAsset(so, soPath);
                }

                var serialized = new SerializedObject(so);
                serialized.FindProperty("outfitSetID").stringValue = assetName;
                serialized.FindProperty("setName").stringValue = setName;
                serialized.FindProperty("rarity").enumValueIndex = (int)rarity;

                var piecesProperty = serialized.FindProperty("pieces");
                piecesProperty.ClearArray();
                for (int i = 0; i < pieces.Count; i++)
                {
                    piecesProperty.InsertArrayElementAtIndex(i);
                    var element = piecesProperty.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Slot").enumValueIndex = (int)pieces[i].Slot;
                    element.FindPropertyRelative("Sprite").objectReferenceValue = pieces[i].Sprite;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);

                if (isNew) created++; else updated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[OutfitSetGenerator] Created {created}, updated {updated} fashion sets in {OUTPUT_PATH}.");
            if (skipped.Count > 0)
            {
                Debug.LogWarning($"[OutfitSetGenerator] Skipped {skipped.Count}: {string.Join("; ", skipped)}");
            }
        }

        /// <summary>
        /// A PSB exports only its visible layer group, so the sub-assets are the set's own pieces,
        /// one sprite per slot.
        /// </summary>
        private static List<OutfitPiece> ReadPieces(string assetPath, out List<string> unmapped)
        {
            var pieces = new List<OutfitPiece>();
            unmapped = new List<string>();

            foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>())
            {
                if (OutfitSlots.TryGetSlot(sprite.name, out var slot))
                {
                    if (pieces.Any(p => p.Slot == slot))
                    {
                        unmapped.Add($"{sprite.name} (duplicate slot {slot})");
                        continue;
                    }
                    pieces.Add(new OutfitPiece { Slot = slot, Sprite = sprite });
                }
                else
                {
                    unmapped.Add(sprite.name);
                }
            }

            pieces.Sort((a, b) => a.Slot.CompareTo(b.Slot));
            return pieces;
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
