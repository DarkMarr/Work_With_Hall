using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuizGame.Item;
using QuizGame.Store;
using UnityEditor;
using UnityEngine;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Puts outfits on sale, reading what to sell and for how much from Docs/Outfit-Prices.csv.
    ///
    /// Prices live in a file rather than in this script because they are a design decision and are
    /// still provisional: the item sheet defines coin_cost and gem_cost but carries no values, so
    /// the numbers in the csv are a starting point. Changing them is editing a spreadsheet and
    /// running this menu again, with no code involved.
    /// </summary>
    public static class OutfitShopGenerator
    {
        private const string PRICE_CSV = "Docs/Outfit-Prices.csv";
        private const string OUTPUT_PATH = "Assets/Resources/InGameProducts/OutfitStore";
        private const string COIN_ASSET = "Assets/Resources/Currency/Coin.asset";
        private const string GEM_ASSET = "Assets/Resources/Currency/Gem.asset";

        private sealed class SetPricing
        {
            public string Source;
            public int Coin;
            public int Gem;
        }

        [MenuItem("QuizGame/Setup/Create Outfit Shop Products")]
        public static void CreateOutfitProducts()
        {
            var pricing = ReadPricing();
            if (pricing == null) return;

            var coin = AssetDatabase.LoadAssetAtPath<CurrencyInfoSO>(COIN_ASSET);
            var gem = AssetDatabase.LoadAssetAtPath<CurrencyInfoSO>(GEM_ASSET);
            if (coin == null || gem == null)
            {
                Debug.LogError("[OutfitShop] Currency assets not found at " + COIN_ASSET + " / " + GEM_ASSET);
                return;
            }
            EnsureFolder(OUTPUT_PATH);

            var garments = AssetDatabase.FindAssets("t:OutfitItemSO", new[] { "Assets/Resources/Items/Outfit" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<OutfitItemSO>)
                .Where(x => x != null)
                .OrderBy(x => x.GetID())
                .ToList();

            var wanted = new HashSet<string>();
            int created = 0, updated = 0;
            var unpriced = new List<string>();

            foreach (var garment in garments)
            {
                var setId = garment.GetOutfitSetID();
                if (string.IsNullOrEmpty(setId) || !pricing.TryGetValue(setId, out var price))
                {
                    unpriced.Add(garment.GetID());
                    continue;
                }
                if (!string.Equals(price.Source, "Shop", System.StringComparison.OrdinalIgnoreCase)) continue;

                // One product per currency, which is how the existing store assets are laid out —
                // CarryOn1_Coin beside CarryOn1_Gem — because a product carries a single price.
                if (price.Coin > 0) Write(garment, coin, price.Coin, "Coin", wanted, ref created, ref updated);
                if (price.Gem > 0) Write(garment, gem, price.Gem, "Gem", wanted, ref created, ref updated);
            }

            RemoveProductsNotGenerated(wanted);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[OutfitShop] {created} created, {updated} updated, {wanted.Count} on sale.");
            if (unpriced.Count > 0)
            {
                Debug.LogWarning($"[OutfitShop] {unpriced.Count} garment(s) have no row in {PRICE_CSV} and were skipped: "
                    + string.Join(", ", unpriced.Take(5)) + (unpriced.Count > 5 ? " ..." : ""));
            }
        }

        private static void Write(OutfitItemSO garment, CurrencyInfoSO currency, int price, string suffix,
                                  HashSet<string> wanted, ref int created, ref int updated)
        {
            var id = garment.GetID() + "_" + suffix;
            wanted.Add(id);

            var path = $"{OUTPUT_PATH}/{id}.asset";
            var product = AssetDatabase.LoadAssetAtPath<InGameProductMetadataSO>(path);
            var isNew = product == null;
            if (isNew)
            {
                product = ScriptableObject.CreateInstance<InGameProductMetadataSO>();
                AssetDatabase.CreateAsset(product, path);
            }

            var serialized = new SerializedObject(product);
            serialized.FindProperty("productID").stringValue = id;
            serialized.FindProperty("product").objectReferenceValue = garment;
            serialized.FindProperty("purchasedCurrency").objectReferenceValue = currency;
            serialized.FindProperty("price").intValue = price;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(product);

            if (isNew) created++; else updated++;
        }

        /// <summary>
        /// Clears out products for anything this run did not put on sale, so moving a set to Craft
        /// or Drop in the csv takes it off the shelf rather than leaving it quietly buyable.
        /// </summary>
        private static void RemoveProductsNotGenerated(HashSet<string> keep)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:InGameProductMetadataSO", new[] { OUTPUT_PATH }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var id = Path.GetFileNameWithoutExtension(path);
                if (keep.Contains(id)) continue;
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[OutfitShop] Removed '{id}': no longer on sale.");
            }
        }

        private static Dictionary<string, SetPricing> ReadPricing()
        {
            var full = Path.Combine(Directory.GetCurrentDirectory(), PRICE_CSV);
            if (!File.Exists(full))
            {
                Debug.LogError("[OutfitShop] Price file not found: " + PRICE_CSV);
                return null;
            }

            var result = new Dictionary<string, SetPricing>();
            foreach (var raw in File.ReadAllLines(full))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                var cells = line.Split(',');
                if (cells.Length < 5) continue;
                if (string.Equals(cells[0].Trim(), "Set", System.StringComparison.OrdinalIgnoreCase)) continue;

                int coin, gem;
                int.TryParse(cells[3].Trim(), out coin);
                int.TryParse(cells[4].Trim(), out gem);
                result[cells[0].Trim()] = new SetPricing { Source = cells[2].Trim(), Coin = coin, Gem = gem };
            }
            Debug.Log($"[OutfitShop] Read {result.Count} set(s) from {PRICE_CSV}.");
            return result;
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
