using QuizGame.Interfaces;
using QuizGame.Resources;

namespace QuizGame.Store
{
    public interface ICurrencyInfo : IHasID, IHasSprite
    {
        CurrencyType GetCurrencyType();
    }
}
