using QuizGame.Interfaces;
using QuizGame.Item.Interfaces;
using QuizGame.Material;

namespace QuizGame.MyRoom.Decoration
{
    public interface IDecorationItem : ICraftableItem, IRecyclable
    {
        DecorationType GetDecorationType();

        /// <summary>
        /// The decoration as it stands in the room, which is not the icon the bag and the pickers
        /// show: an icon is framed and tinted by rarity, and that frame has no business on a shelf.
        /// </summary>
        UnityEngine.Sprite GetRoomSprite();
    }
}
