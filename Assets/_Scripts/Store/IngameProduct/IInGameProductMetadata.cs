using QuizGame.Item.Interfaces;

namespace QuizGame.Store
{
    public interface IInGameProductMetadata : IItem
    {
        IItem GetItemProduct();
        ICurrencyInfo GetPurchasedCurrency();
        FilterType GetFilterType();
        int GetPrice();
    }
}
