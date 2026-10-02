using System.Collections.Generic;
using System.IO;
using System.Text;
using QuizGame.Item;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Gives every garment its name and description, from Docs/Outfit-Text.csv.
    ///
    /// The text itself is the designer's, written in ALL ITEM DATA (tab OUTFIT) with a Thai,
    /// English and Japanese column per garment. The csv is that sheet reduced to the 76 garments
    /// this build actually has, keyed the way the localization tables expect.
    ///
    /// IMPORTANT: the Item table collection pulls from the Localization sheet with
    /// m_RemoveMissingPulledKeys set, so a pull deletes any key the sheet does not carry. The rows
    /// this menu writes must therefore also be pasted into that sheet's Item tab — see
    /// Docs/Outfit-Text-README.md — or the next pull will silently empty them again.
    /// </summary>
    public static class OutfitTextImporter
    {
        private const string TEXT_CSV = "Docs/Outfit-Text.csv";
        private const string COLLECTION = "Item";
        private const string ITEM_FOLDER = "Assets/Resources/Items/Outfit";

        private sealed class Row
        {
            public string ItemId;
            public string Key;
            public string English;
            public string Japanese;
            public string Thai;

            public bool IsName => Key.EndsWith(".name");
            public bool IsDescription => Key.EndsWith(".description");

            public bool HasText => !string.IsNullOrWhiteSpace(English)
                                || !string.IsNullOrWhiteSpace(Japanese)
                                || !string.IsNullOrWhiteSpace(Thai);
        }

        [MenuItem("QuizGame/Setup/Apply Outfit Text")]
        public static void Apply()
        {
            var rows = ReadRows();
            if (rows == null) return;

            var collection = LocalizationEditorSettings.GetStringTableCollection(COLLECTION);
            if (collection == null)
            {
                Debug.LogError($"[OutfitText] String table collection '{COLLECTION}' not found.");
                return;
            }

            var tables = new Dictionary<string, StringTable>();
            foreach (var table in collection.StringTables)
            {
                tables[table.LocaleIdentifier.Code] = table;
            }

            var usable = UsableKeys(rows, out var unnamed);
            var shared = collection.SharedData;
            int keysWritten = 0, valuesWritten = 0, keysRemoved = 0;

            foreach (var row in rows)
            {
                if (usable.Contains(row.Key))
                {
                    if (shared.GetEntry(row.Key) == null) shared.AddKey(row.Key);
                    keysWritten++;

                    valuesWritten += Write(tables, "en", row.Key, row.English);
                    valuesWritten += Write(tables, "ja", row.Key, row.Japanese);
                    valuesWritten += Write(tables, "th", row.Key, row.Thai);
                }
                else if (shared.GetEntry(row.Key) != null)
                {
                    // Re-running after the sheet changes has to be able to take text away as well
                    // as add it, or a line that was withdrawn would live on in the build.
                    foreach (var table in tables.Values) table.RemoveEntry(row.Key);
                    shared.RemoveKey(row.Key);
                    keysRemoved++;
                }
            }

            EditorUtility.SetDirty(shared);
            foreach (var table in tables.Values) EditorUtility.SetDirty(table);

            var bound = BindGarments(rows, usable, shared, out var cleared);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[OutfitText] {keysWritten} key(s), {valuesWritten} translation(s), "
                + $"{bound} garment field(s) bound. Removed {keysRemoved} key(s) and cleared "
                + $"{cleared} stale binding(s).");

            if (unnamed.Count > 0)
            {
                Debug.LogWarning($"[OutfitText] {unnamed.Count} garment(s) have no name in any language, "
                    + "so nothing was taken from their row — several of them hold a note to the art team "
                    + "in the description column rather than item copy, and that would have shown to "
                    + "players as the item's description: " + string.Join(", ", unnamed.ToArray()));
            }

            Debug.LogWarning("[OutfitText] These keys live only in the project until the same rows are "
                + "added to the Localization sheet's Item tab. The collection pulls with "
                + "'remove missing pulled keys', so a pull before that would delete them. "
                + "Paste-ready rows: " + TEXT_CSV);
        }

        /// <summary>
        /// Which keys may be used. A garment's text counts only when the garment has a name:
        /// a row with a description but no name is one the designer has not written yet, and in
        /// this sheet that description column is where notes to the art team were parked
        /// ("ยังนึกไม่ออกเลยแฮะ"). Taking it would put a working note in front of the player.
        /// </summary>
        private static HashSet<string> UsableKeys(List<Row> rows, out List<string> unnamed)
        {
            var named = new HashSet<string>();
            var seen = new HashSet<string>();
            foreach (var row in rows)
            {
                seen.Add(row.ItemId);
                if (row.IsName && row.HasText) named.Add(row.ItemId);
            }

            unnamed = new List<string>();
            foreach (var item in seen)
            {
                if (!named.Contains(item)) unnamed.Add(item);
            }
            unnamed.Sort();

            var usable = new HashSet<string>();
            foreach (var row in rows)
            {
                if (row.HasText && named.Contains(row.ItemId)) usable.Add(row.Key);
            }
            return usable;
        }

        private static int Write(IReadOnlyDictionary<string, StringTable> tables, string code, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (!tables.TryGetValue(code, out var table)) return 0;

            var entry = table.GetEntry(key) ?? table.AddEntry(key, string.Empty);
            entry.Value = value;
            return 1;
        }

        /// <summary>
        /// Points each garment at its own keys, and unpoints it from any this run rejected. A
        /// garment left unbound keeps the readable fallback <see cref="EquipmentItemSO"/> builds
        /// from its id, which beats showing an empty line.
        /// </summary>
        private static int BindGarments(List<Row> rows, HashSet<string> usable,
                                        SharedTableData shared, out int cleared)
        {
            var sharedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shared));
            var tableReference = "GUID:" + sharedGuid;

            var byItem = new Dictionary<string, List<Row>>();
            foreach (var row in rows)
            {
                if (!byItem.TryGetValue(row.ItemId, out var list))
                {
                    list = new List<Row>();
                    byItem[row.ItemId] = list;
                }
                list.Add(row);
            }

            int bound = 0, wiped = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:OutfitItemSO", new[] { ITEM_FOLDER }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var garment = AssetDatabase.LoadAssetAtPath<OutfitItemSO>(path);
                if (garment == null) continue;
                if (!byItem.TryGetValue(garment.GetID(), out var itemRows)) continue;

                var serialized = new SerializedObject(garment);
                foreach (var row in itemRows)
                {
                    var field = row.IsName ? "localizedName" : row.IsDescription ? "localizedDescription" : null;
                    if (field == null) continue;

                    var property = serialized.FindProperty(field);
                    if (property == null) continue;

                    var entry = usable.Contains(row.Key) ? shared.GetEntry(row.Key) : null;
                    if (entry != null)
                    {
                        SetReference(property, tableReference, entry.Id);
                        bound++;
                    }
                    else if (!string.IsNullOrEmpty(
                        property.FindPropertyRelative("m_TableReference.m_TableCollectionName").stringValue))
                    {
                        SetReference(property, string.Empty, 0);
                        wiped++;
                    }
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(garment);
            }

            cleared = wiped;
            return bound;
        }

        private static void SetReference(SerializedProperty property, string tableReference, long keyId)
        {
            property.FindPropertyRelative("m_TableReference.m_TableCollectionName").stringValue = tableReference;
            property.FindPropertyRelative("m_TableEntryReference.m_Key").stringValue = string.Empty;

            var keyIdProperty = property.FindPropertyRelative("m_TableEntryReference.m_KeyId");
            if (keyIdProperty == null) return;
            if (keyIdProperty.numericType == SerializedPropertyNumericType.UInt64) keyIdProperty.ulongValue = (ulong)keyId;
            else keyIdProperty.longValue = keyId;
        }

        private static List<Row> ReadRows()
        {
            var full = Path.Combine(Directory.GetCurrentDirectory(), TEXT_CSV);
            if (!File.Exists(full))
            {
                Debug.LogError("[OutfitText] Text file not found: " + TEXT_CSV);
                return null;
            }

            var cells = ParseCsv(File.ReadAllText(full, Encoding.UTF8));
            var rows = new List<Row>();
            foreach (var line in cells)
            {
                if (line.Count < 5) continue;
                if (string.Equals(line[0].Trim(), "ItemID", System.StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(line[0]) || string.IsNullOrWhiteSpace(line[1])) continue;

                rows.Add(new Row
                {
                    ItemId = line[0].Trim(),
                    Key = line[1].Trim(),
                    English = line[2],
                    Japanese = line[3],
                    Thai = line[4]
                });
            }
            Debug.Log($"[OutfitText] Read {rows.Count} row(s) from {TEXT_CSV}.");
            return rows;
        }

        /// <summary>
        /// A real csv reader rather than Split(','), because the descriptions carry commas and
        /// quotes of their own — "A closet full of clothes and ""nothing to wear.""" is one cell.
        /// </summary>
        private static List<List<string>> ParseCsv(string text)
        {
            var result = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuotes)
                {
                    if (c != '"') { cell.Append(c); continue; }
                    if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; continue; }
                    inQuotes = false;
                    continue;
                }

                if (c == '"') { inQuotes = true; continue; }
                if (c == ',') { row.Add(cell.ToString()); cell.Clear(); continue; }
                if (c == '\r') continue;
                if (c == '\n')
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                    result.Add(row);
                    row = new List<string>();
                    continue;
                }
                cell.Append(c);
            }

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                result.Add(row);
            }
            return result;
        }
    }
}
