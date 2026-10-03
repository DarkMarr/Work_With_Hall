using QuizGame.Interfaces;

namespace QuizGame.Item.Interfaces
{
    /// <summary>
    /// Anything the craft bench can make: an item with a recipe and something to say about itself.
    ///
    /// The bench used to be typed to decorations all the way down, which is why there was no way to
    /// put garments on it even though the item sheet has priced every one of them. Nothing in the
    /// craft screens ever needed a decoration in particular — they want an icon, a name, a
    /// description and a list of materials — so this is what they ask for now.
    /// </summary>
    public interface ICraftableItem : IItem, ICraftable, IHasDescription
    {
    }
}
