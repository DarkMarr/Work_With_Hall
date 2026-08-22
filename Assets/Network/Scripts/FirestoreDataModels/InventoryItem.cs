using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class InventoryItem
    {
        [FirestoreProperty("itemId")]
        public string ItemId { get; set; }
        
        [FirestoreProperty("name")]
        public string Name { get; set; }
        
        [FirestoreProperty("type")]
        public string Type { get; set; }
        
        [FirestoreProperty("quantity")]
        public int Quantity { get; set; }
        
        [FirestoreProperty("acquiredAt")]
        public Timestamp AcquiredAt { get; set; }
        
        [FirestoreProperty("equipped")]
        public bool Equipped { get; set; }
    }
}