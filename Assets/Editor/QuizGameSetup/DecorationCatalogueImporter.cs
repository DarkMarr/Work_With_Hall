using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using QuizGame.Item;
using QuizGame.MyRoom.Decoration;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

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
        private const string CraftCsv = "Docs/Craft-List.csv";
        private const string DecorFolder = "Assets/Resources/Items/Decoration";
        private const string IconFolder = "Assets/Art/UI/ITEM/DECOR";
        private const string TextCsvOut = "Docs/Decoration-Text.csv";

        /// <summary>The Item string table collection, as a LocalizedString stores a reference to it.</summary>
        private const string ItemTableReference = "GUID:ed13262006708df4fa4aa50d09b40b1c";

        /// <summary>
        /// The sub-description shows what kind of decoration it is, which is the one piece of text
        /// the sheet does not spell out per item because the Type column already says it.
        /// </summary>
        private static readonly Dictionary<DecorationType, long> TypeLabelKey = new Dictionary<DecorationType, long>
        {
            { DecorationType.Room, 21822891311857664L },
            { DecorationType.Window, 170781847283818497L },
            { DecorationType.Small, 170781847116046336L },
            { DecorationType.Big, 170781847283818496L },
            { DecorationType.Suitcase, 170766645997854720L },
        };

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

        [MenuItem("QuizGame/Items/Import decoration names and descriptions")]
        public static void ImportText()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("Item");
            if (collection == null)
            {
                Debug.LogError("[DecorationText] No string table collection named Item.");
                return;
            }

            var shared = collection.SharedData;
            var exported = new List<string[]>();
            var missingText = new List<string>();
            var notFound = new List<string>();
            var wired = 0;

            foreach (var source in new[] { DecorCsv, RoomCsv })
            {
                foreach (var row in ReadCsv(source))
                {
                    var id = Field(row, "ID");
                    if (string.IsNullOrEmpty(id)) id = Field(row, "column0");
                    if (string.IsNullOrEmpty(id)) continue;

                    var asset = FindDecoration(id);
                    if (asset == null) { notFound.Add(id); continue; }

                    var english = Field(row, "EN name");
                    if (string.IsNullOrEmpty(english)) { missingText.Add(id); continue; }

                    var nameKey = WriteEntry(collection, shared, $"decoration.{id}.name",
                        english, Field(row, "JP name"), Field(row, "Name"), exported);
                    var descriptionKey = WriteEntry(collection, shared, $"decoration.{id}.description",
                        Field(row, "Description EN"), Field(row, "Description JP"), Field(row, "Description TH"), exported);

                    var serialized = new SerializedObject(asset);
                    Bind(serialized, "localizedName", nameKey);
                    Bind(serialized, "localizedDescription", descriptionKey);

                    long typeKey;
                    if (TypeLabelKey.TryGetValue(asset.GetDecorationType(), out typeKey))
                    {
                        Bind(serialized, "localizedSubDescription", typeKey);
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(asset);
                    wired++;
                }
            }

            EditorUtility.SetDirty(shared);
            foreach (var table in collection.StringTables) EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            WriteCsv(exported);

            var report = new StringBuilder();
            report.AppendLine($"[DecorationText] wired {wired} decorations, wrote {exported.Count} rows to {TextCsvOut}.");
            report.AppendLine("  Paste them into the Localization sheet's Item tab before anyone pulls, or they will be deleted.");
            if (missingText.Count > 0) report.AppendLine("  no name in the sheet: " + string.Join(", ", missingText));
            if (notFound.Count > 0) report.AppendLine("  no asset for id: " + string.Join(", ", notFound));
            Debug.Log(report.ToString());
        }

        /// <summary>Adds or updates one key in all three pulled locales, and records it for the CSV.</summary>
        private static long WriteEntry(StringTableCollection collection, SharedTableData shared,
            string key, string english, string japanese, string thai, List<string[]> exported)
        {
            var entry = shared.GetEntry(key) ?? shared.AddKey(key);

            foreach (var table in collection.StringTables)
            {
                var code = table.LocaleIdentifier.Code;
                string value;
                if (code.StartsWith("ja")) value = japanese;
                else if (code.StartsWith("th")) value = thai;
                else value = english;   // English is also what the untranslated locales fall back to.
                if (!string.IsNullOrEmpty(value)) table.AddEntry(entry.Id, value);
            }

            exported.Add(new[] { key, english, japanese, thai });
            return entry.Id;
        }

        /// <summary>Points a LocalizedString field at a key in the Item collection.</summary>
        private static void Bind(SerializedObject serialized, string field, long keyId)
        {
            serialized.FindProperty($"{field}.m_TableReference.m_TableCollectionName").stringValue = ItemTableReference;
            serialized.FindProperty($"{field}.m_TableEntryReference.m_KeyId").longValue = keyId;
            serialized.FindProperty($"{field}.m_TableEntryReference.m_Key").stringValue = string.Empty;
        }

        private static DecorationItemSO FindDecoration(string id)
        {
            foreach (DecorationType type in Enum.GetValues(typeof(DecorationType)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<DecorationItemSO>($"{DecorFolder}/{type}/{id}.asset");
                if (asset != null) return asset;
            }
            return null;
        }

        /// <summary>
        /// Writes the rows in the column order the Localization sheet pulls: key, English, Japanese,
        /// Thai. Descriptions carry commas and quotes, so every field is quoted.
        /// </summary>
        private static void WriteCsv(List<string[]> rows)
        {
            var text = new StringBuilder();
            text.AppendLine("Key,English(en),Japanese(ja),Thai(th)");
            foreach (var row in rows)
            {
                text.AppendLine(string.Join(",", row.Select(cell => "\"" + (cell ?? string.Empty).Replace("\"", "\"\"") + "\"")));
            }

            var full = Path.Combine(Directory.GetParent(Application.dataPath).FullName, TextCsvOut);
            File.WriteAllText(full, text.ToString(), new UTF8Encoding(true));
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Fills in what each garment costs to make, from the craft list's OUTFIT section.
        ///
        /// That section prices by rarity and slot, not item by item — every Common head piece costs
        /// the same — so the ID column being empty, which has blocked the decoration recipes, does
        /// not block these. The rows name a garment, and the names match what the outfit assets
        /// already say, so a row finds its item without needing an id at all.
        /// </summary>
        [MenuItem("QuizGame/Items/Import outfit craft recipes")]
        public static void ImportOutfitRecipes()
        {
            var materials = new Dictionary<string, BaseItemSO>();
            foreach (var column in new[] { "Fine Fabric", "Blueprint", "Hardwood", "Shiny Crystal", "Paint Bucket" })
            {
                materials[column] = null;
            }
            materials["Fine Fabric"] = LoadMaterial("50001");
            materials["Blueprint"] = LoadMaterial("50002");
            materials["Hardwood"] = LoadMaterial("50003");
            materials["Shiny Crystal"] = LoadMaterial("50004");
            materials["Paint Bucket"] = LoadMaterial("50005");

            // Matched on the English table rather than GetName(), which answers in whichever locale
            // the editor happens to be set to and so found nothing at all the first time.
            var english = AssetDatabase.LoadAssetAtPath<StringTable>(
                "Assets/Settings/Localization/Tables/GameItem/Item_en.asset");
            if (english == null)
            {
                Debug.LogError("[OutfitRecipes] Item_en string table not found.");
                return;
            }

            var garmentsByName = new Dictionary<string, OutfitItemSO>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Resources/Items/Outfit" }))
            {
                var garment = AssetDatabase.LoadAssetAtPath<OutfitItemSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (garment == null) continue;

                var keyId = new SerializedObject(garment)
                    .FindProperty("localizedName.m_TableEntryReference.m_KeyId").longValue;
                var entry = keyId == 0 ? null : english.GetEntry(keyId);
                var englishName = entry == null ? null : entry.LocalizedValue;
                if (!string.IsNullOrEmpty(englishName)) garmentsByName[englishName] = garment;
            }

            var rows = ParseCsv(File.ReadAllText(
                Path.Combine(Directory.GetParent(Application.dataPath).FullName, CraftCsv), Encoding.UTF8));

            var section = string.Empty;
            var priced = 0;
            var unmatched = new List<string>();

            foreach (var row in rows)
            {
                if (row.Count == 0) continue;
                var first = row[0].Trim();
                if (first == "ROOM" || first == "DECOR" || first == "OUTFIT") section = first;
                if (section != "OUTFIT" || row.Count <= 10) continue;

                var englishName = row[5].Trim();
                if (string.IsNullOrEmpty(englishName)) continue;   // two rows have no name yet

                OutfitItemSO garment;
                if (!garmentsByName.TryGetValue(englishName, out garment)) { unmatched.Add(englishName); continue; }

                var recipe = new List<ItemSOWithQuantityPair>();
                AddMaterial(recipe, materials["Fine Fabric"], row[6]);
                AddMaterial(recipe, materials["Blueprint"], row[7]);
                AddMaterial(recipe, materials["Hardwood"], row[8]);
                AddMaterial(recipe, materials["Shiny Crystal"], row[9]);
                AddMaterial(recipe, materials["Paint Bucket"], row[10]);

                var serialized = new SerializedObject(garment);
                var requirements = serialized.FindProperty("craftRequirementItems");
                requirements.arraySize = recipe.Count;
                for (var i = 0; i < recipe.Count; i++)
                {
                    var element = requirements.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Item").objectReferenceValue = recipe[i].Item;
                    element.FindPropertyRelative("Quantity").intValue = recipe[i].Quantity;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(garment);
                priced++;
            }

            AssetDatabase.SaveAssets();
            var report = $"[OutfitRecipes] priced {priced} garments.";
            if (unmatched.Count > 0) report += " No garment named: " + string.Join(", ", unmatched);
            Debug.Log(report);
        }

        private static BaseItemSO LoadMaterial(string id)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Resources" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("/Material")) continue;
                var item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);
                if (item != null && item.GetID() == id) return item;
            }
            Debug.LogError($"[OutfitRecipes] No material with id {id}.");
            return null;
        }

        /// <summary>A dash means this recipe does not use that material at all.</summary>
        private static void AddMaterial(List<ItemSOWithQuantityPair> recipe, BaseItemSO material, string cell)
        {
            int quantity;
            if (material == null || !int.TryParse((cell ?? string.Empty).Trim(), out quantity) || quantity <= 0) return;
            recipe.Add(new ItemSOWithQuantityPair { Item = material, Quantity = quantity });
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
