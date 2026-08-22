using QuizGame.Interfaces;
using QuizGame.Resources;

namespace QuizGame.Item.Interfaces
{
    /// <summary>
    /// Presume that all items have an ID, a name, and a sprite.
    /// </summary>
    public interface IItem : IHasID, IHasName, IHasSprite
    {
        ItemType GetItemType();
        ItemTier GetItemTier();
    }
}
