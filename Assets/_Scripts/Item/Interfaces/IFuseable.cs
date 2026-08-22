namespace QuizGame.Item.Interfaces
{
    public interface IFuseable
    {
        IQuantifiableItem[] GetFuseRequirementItems();

        IItem GetFuseResult();
    }
}