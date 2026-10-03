namespace QuizGame.UI
{
    /// <summary>
    /// Which header a popup wears. The art provides two capsules for the same box, and the colour
    /// is the only thing that tells a player, before reading a word, whether what they just tried
    /// worked.
    /// </summary>
    public enum PopupTone
    {
        /// <summary>Green. Something is being reported, and nothing has gone wrong.</summary>
        Notice,

        /// <summary>Red. What the player just tried did not happen.</summary>
        Alert
    }
}
