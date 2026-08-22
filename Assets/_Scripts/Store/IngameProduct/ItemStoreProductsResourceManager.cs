using QuizGame.Resources;

namespace QuizGame.Store
{
    public class ItemStoreProductsResourceManager : ResourceManager<ItemStoreProductsResourceManager, InGameProductMetadataSO>
    {
        public override string ContentResourcePath => "InGameProducts/ItemStore";
    }
}
