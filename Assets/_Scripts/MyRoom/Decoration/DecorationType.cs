namespace QuizGame.MyRoom.Decoration
{
    /// <summary>
    /// What a decoration is, which is also what kind of slot will take it.
    ///
    /// New values go on the end. The order is what Unity stores in every decoration asset and every
    /// slot in the room, so inserting one in the middle silently re-types them all.
    /// </summary>
    public enum DecorationType
    {
        Room,
        WallTrophy,
        ShelfTrophy,
        FloorTrophy,

        /// <summary>
        /// The suitcase by the door. Art gave it a slot of its own rather than counting it as floor
        /// decor, so only a suitcase can stand there.
        /// </summary>
        Suitcase
    }
}
