using System.Collections.Generic;

namespace QuizGame.Character
{
    /// <summary>
    /// Serializable outfit data: which character is selected and which cosmetics are equipped.
    /// </summary>
    [System.Serializable]
    public class PlayerOutfit
    {
        public string CharacterId;
        public Dictionary<string, string> EquippedItems = new Dictionary<string, string>();

        public PlayerOutfit() { }

        public PlayerOutfit(string characterId, Dictionary<string, string> equippedItems)
        {
            CharacterId = characterId;
            EquippedItems = equippedItems ?? new Dictionary<string, string>();
        }
    }
}
