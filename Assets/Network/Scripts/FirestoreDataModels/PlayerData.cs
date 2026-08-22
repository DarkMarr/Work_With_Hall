using System;
using System.Collections.Generic;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class PlayerData
    {        
        [FirestoreProperty("profiileData")]
        public ProfileData ProfileData { get; set; }
                        
        [FirestoreProperty("energy")]
        public Energy Energy { get; set; }
        
        [FirestoreProperty("multiPlayerStats")]
        public MultiPlayerStats MultiPlayerStats { get; set; }
        
        [FirestoreProperty("singlePlayerStats")]
        public Dictionary<string, SinglePlayerStats> SinglePlayerStats { get; set; }
        
        [FirestoreProperty("inventory")]
        public Inventory Inventory { get; set; }
    }
}