using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class SinglePlayerStats
    {
        [FirestoreProperty("score")]
        public int Score { get; set; }
        
        [FirestoreProperty("gamesPlayed")]
        public int GamesPlayed { get; set; }
        
        [FirestoreProperty("gamesWon")]
        public int GamesWon { get; set; }
        
        [FirestoreProperty("rank")]
        public int Rank { get; set; }
    }
}