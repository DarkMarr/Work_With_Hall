namespace QuizGame.MyRoom.Decoration
{
    /// <summary>
    /// What a decoration is, which is also what kind of slot will take it.
    ///
    /// These are the four types the item sheet uses, and they are named for the space a decoration
    /// occupies rather than the furniture it sits on. The old names — WallTrophy, ShelfTrophy,
    /// FloorTrophy — were a guess made before the sheet was found; they happened to sit on the same
    /// numbers, so renaming them left every stored value meaning what it already meant.
    ///
    /// New values go on the end. The order is what Unity stores in every decoration asset and every
    /// slot in the room, so inserting one in the middle silently re-types them all.
    /// </summary>
    public enum DecorationType
    {
        /// <summary>The room itself: the wallpaper everything else stands in.</summary>
        Room,

        /// <summary>What can be seen through the window.</summary>
        Window,

        /// <summary>Something that stands on a shelf or a table top.</summary>
        Small,

        /// <summary>Something that stands on the floor.</summary>
        Big,

        /// <summary>The suitcase by the door, which has a slot of its own and takes nothing else.</summary>
        Suitcase
    }
}
