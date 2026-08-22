using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class RoomDecoration
    {
        [FirestoreProperty("decorationId")]
        public string DecorationId { get; set; }
        
        [FirestoreProperty("placedAt")]
        public Timestamp PlacedAt { get; set; }
    }
}