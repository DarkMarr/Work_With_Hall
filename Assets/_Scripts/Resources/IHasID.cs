namespace QuizGame.Resources
{
    /// <summary>
    /// To make any ScriptableObject work with ResourceManager they need IHasID
    /// </summary>
    public interface IHasID
    {
        string GetID();
    }
}
