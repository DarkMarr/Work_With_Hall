using QuizGame.Resources;
using QuizGame.Store;

namespace QuizGame.Currency
{
    public class CurrencyResourceManager : ResourceManager<CurrencyResourceManager, CurrencyInfoSO>
    {
        public override string ContentResourcePath => "Currency";
    }
}
