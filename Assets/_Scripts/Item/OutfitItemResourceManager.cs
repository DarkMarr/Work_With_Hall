using QuizGame.Resources;

namespace QuizGame.Item
{
    /// <summary>
    /// Loads the wearable garments so a saved outfit can be turned back into sprites at runtime.
    /// The art itself sits outside Resources; these assets are what keep it in the build.
    /// </summary>
    public class OutfitItemResourceManager : ResourceManager<OutfitItemResourceManager, OutfitItemSO>
    {
        public override string ContentResourcePath => "Items/Outfit";
    }
}
