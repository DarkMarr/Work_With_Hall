namespace QuizGame.Item.Interfaces
{
    public interface ICraftable
    {
        IQuantifiableItem[] GetCraftRequirementItems();

        IItem GetCraftResult();
    }
}