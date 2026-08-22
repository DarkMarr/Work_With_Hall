using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class Energy
    {
        [FirestoreProperty("current")]
        public int Current { get; set; }
        
        [FirestoreProperty("max")]
        public int Max { get; set; }
        
        [FirestoreProperty("energyRegenRate")]
        public int EnergyRegenRate { get; set; }
        
        [FirestoreProperty("energyRegenIntervalSeconds")]
        public int EnergyRegenIntervalSeconds { get; set; }
        
        [FirestoreProperty("lastEnergyUpdateTimestamp")]
        public Timestamp LastEnergyUpdateTimestamp { get; set; }
    }
}