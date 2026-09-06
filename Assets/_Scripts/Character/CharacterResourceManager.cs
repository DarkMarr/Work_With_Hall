using QuizGame.Resources;

namespace QuizGame.Character
{
    public class CharacterResourceManager : ResourceManager<CharacterResourceManager, CharacterInfoSO>
    {
        public override string ContentResourcePath => "Characters";
    }
}
