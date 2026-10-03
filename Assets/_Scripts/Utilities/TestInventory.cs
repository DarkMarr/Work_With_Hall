namespace QuizGame.Utilities
{
    /// <summary>
    /// Fills every inventory screen with the whole catalogue while there is no server behind them.
    ///
    /// None of this is a game rule. It exists so the room, the wardrobe, the bag, the trade board
    /// and the craft bench can be walked through end to end before the player's real inventory is
    /// readable, and it is meant to be deleted rather than configured.
    ///
    /// TO REMOVE ONCE THE SERVER IS CONNECTED:
    ///   1. Search the project for "TestInventory". Every stand-in is marked with that word, so the
    ///      search is the removal list — nothing is hidden behind a setting or a build flag.
    ///   2. At each site, keep the branch that reads the player's real data and delete the other.
    ///   3. Delete this file. The project will then fail to compile at any site that was missed,
    ///      which is the point: a forgotten stand-in should break the build, not ship quietly.
    ///
    /// Flipping <see cref="UnlockEverything"/> to false is a quick way to see what each screen
    /// looks like on a real, mostly empty account without unpicking anything.
    /// </summary>
    public static class TestInventory
    {
        /// <summary>True while inventory screens should show the whole catalogue.</summary>
        public const bool UnlockEverything = true;

        /// <summary>
        /// How many of each material the player appears to hold. Higher than any recipe asks for,
        /// so crafting can be exercised repeatedly without the bench running dry mid-test.
        /// </summary>
        public const int MaterialStock = 999;

        /// <summary>How many of each carry-on item the player appears to hold.</summary>
        public const int CarryOnStock = 99;
    }
}
