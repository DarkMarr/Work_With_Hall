using QuizGame.Interfaces;
using QuizGame.Item.Interfaces;
using QuizGame.Material;

namespace QuizGame.MyRoom.Decoration
{
    public interface IDecorationItem : IItem, IRecyclable, IFuseable, IHasDescription
    {
        DecorationType GetDecorationType();
    }
}
