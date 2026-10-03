using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using QuizGame.Item;
using QuizGame.MyRoom.Decoration;
using UnityEditor;
using UnityEngine;

namespace QuizGame.EditorTools
{
    /// <summary>
    /// Builds the decoration catalogue from the item sheet.
    ///
    /// The sheet is the only place that says which picture belongs to which item. Before it was
    /// found, the assets had been numbered by hand and the icons by Art, and the two numberings
    /// looked alike enough to pair up and were not the same catalogue at all — item 33101 was a
    /// bucket while icon 331001 is a houseplant. So nothing here is inferred: the id, the type, the
    /// rarity and the name all come from the sheet, and the icon is found by the id the sheet gives.
    ///
    /// Re-running it is safe. Assets are matched by id and updated in place, so a row the sheet has
    /// changed is corrected rather than duplicated, and an asset whose row has gone is reported
    /// rather than deleted — removing player-owned items is not this tool's call to make.
    /// </summary>
    public static class DecorationCatalogueImporter
    {
        private const string DecorCsv = "Docs/Decor-Items.csv";
        private const string RoomCsv = "Docs/Room-Items.csv";
        private const string DecorFolder = "Assets/Resources/Items/Decoration";
        private const string IconFolder = "Assets/Art/UI/ITEM/DECOR";

        private static readonly Dictionary<string, ItemTier> TierByRarity = new Dictionary<string, ItemTier>(StringComparer.OrdinalIgnoreCase)
        {
            { "Common", ItemTier.Common },
            { "Uncommon", ItemTier.Uncommon },
            { "Rare", ItemTier.Rare },
            { "Ultra Rare", ItemTier.SuperRare },
        };

        private static readonly Dictionary<string, DecorationType> TypeByName = new Dictionary<string, DecorationType>(StringComparer.OrdinalIgnoreCase)
        {
            { "Window", DecorationType.Window },
            { "Small", DecorationType.Small },
            { "Big", DecorationType.Big },
            { "Suitcase", DecorationType.Suitcase },
        };

        [MenuItem("QuizGame/Items/Rebuild decoration catalogue from the sheet")]
        public static void Rebuild()
        {
            var report = new StringBuilder();
            var written = 0;
            var noIcon = new List<string>();

            foreach (DecorationType type in Enum.GetValues(typeof(DecorationType)))
            {
                var folder = $"{DecorFolder}/{type}";
                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(DecorFolder, type.ToString());
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var row in ReadCsv(DecorCsv))
                {
                    var id = Field(row, "ID");
                    if (string.IsNullOrEmpty(id)) continue;

                    DecorationType type;
                    if (!TypeByName.TryGetValue(Field(row, "Type"), out type))
                    {
                        report.AppendLine($"  {id}: unknown Type \"{Field(row, "Type")}\", skipped");
                        continue;
                    }

                    if (Write(id, type, Field(row, "Rarity"), Field(row, "EN name"), noIcon)) written++;
                }

                foreach (var row in ReadCsv(RoomCsv))
                {
                    // The room tab's id column has no header, so it is read by position. Taking
                    // the first value instead picked up Rarity and named four assets "Common".
                    var id = Field(row, "ID");
                    if (string.IsNullOrEmpty(id)) id = Field(row, "column0");
                    if (string.IsNullOrEmpty(id)) continue;

                    if (Write(id, DecorationType.Room, Field(row, "Rarity"), Field(row, "EN name"), noIcon)) written++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            report.Insert(0, $"[DecorationCatalogue] wrote {written} assets; {noIcon.Count} have no icon drawn yet.\n");
            if (noIcon.Count > 0) report.AppendLine("  no icon: " + string.Join(", ", noIcon));
            Debug.Log(report.ToString());
        }

        /// <summary>Creates or updates one decoration asset. Returns false only if it could not be written.</summary>
        private static bool Write(string id, DecorationType type, string rarity, string englishName, List<string> noIcon)
        {
            ItemTier tier;
            if (!TierByRarity.TryGetValue(rarity ?? string.Empty, out tier)) tier = ItemTier.NoTier;

            var path = $"{DecorFolder}/{type}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<DecorationItemSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DecorationItemSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var icon = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconFolder}/{id}.png");
            if (icon == null) noIcon.Add(id);

            var serialized = new SerializedObject(asset);
            serialized.FindProperty("itemID").stringValue = id;
            serialized.FindProperty("decorationType").enumValueIndex = (int)type;
            serialized.FindProperty("decorationTier").enumValueIndex = (int)tier;
            // Only set the icon when there is one, so a re-run before Art finishes does not wipe an
            // icon somebody assigned by hand in the meantime.
            if (icon != null) serialized.FindProperty("itemSprite").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            if (!string.IsNullOrEmpty(englishName)) asset.name = id;
            return true;
        }

        private static string Field(Dictionary<string, string> row, string column)
        {
            string value;
            return row.TryGetValue(column, out value) ? (value ?? string.Empty).Trim() : string.Empty;
        }

        /// <summary>
        /// Reads the sheet export. Descriptions contain commas and quotes, so the quoting rules
        /// have to be honoured rather than splitting on commas.
        /// </summary>
        private static IEnumerable<Dictionary<string, string>> ReadCsv(string projectRelativePath)
        {
            var full = Path.Combine(Directory.GetParent(Application.dataPath).FullName, projectRelativePath);
            if (!File.Exists(full))
            {
                Debug.LogError($"[DecorationCatalogue] {projectRelativePath} not found.");
                yield break;
            }

            var rows = ParseCsv(File.ReadAllText(full, Encoding.UTF8));
            if (rows.Count == 0) yield break;

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = new Dictionary<string, string>();
                for (var c = 0; c < header.Count; c++)
                {
                    var key = header[c].Trim();
                    if (string.IsNullOrEmpty(key)) key = "column" + c;
                    if (row.ContainsKey(key)) continue;
                    row[key] = c < rows[i].Count ? rows[i][c] : string.Empty;
                }
                yield return row;
            }
        }

        private static List<List<string>> ParseCsv(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var quoted = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (quoted)
                {
                    if (c != '"') { field.Append(c); continue; }
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; continue; }
                    quoted = false;
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(field.ToString()); field.Length = 0; }
                else if (c == '\n')
                {
                    row.Add(field.ToString().TrimEnd('\r'));
                    field.Length = 0;
                    rows.Add(row);
                    row = new List<string>();
                }
                else field.Append(c);
            }

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString().TrimEnd('\r'));
                rows.Add(row);
            }
            return rows;
        }
    }
}
