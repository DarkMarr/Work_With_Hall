using System.Collections.Generic;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [FirestoreData]
    public class Inventory
    {
        [FirestoreProperty("gems")]
        public int Gems { get; set; }
        
        [FirestoreProperty("coins")]
        public int Coins { get; set; }
        
        [FirestoreProperty("materials")]
        public Dictionary<string, int> Materials { get; set; }
    }
}