using QuizGame.Resources;

namespace QuizGame.Store
{
    /// <summary>
    /// Outfits that are for sale. Only garments whose set is marked Shop in Docs/Outfit-Prices.csv
    /// have a product here; the ones that drop or are crafted have none, so they cannot be bought
    /// by accident.
    /// </summary>
    public class OutfitStoreProductsResourceManager
        : ResourceManager<OutfitStoreProductsResourceManager, InGameProductMetadataSO>
    {
        public override string ContentResourcePath => "InGameProducts/OutfitStore";
    }
}
