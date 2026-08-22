using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class ProfileData
    {
        [FirestoreProperty("gameUid")]
        public string GameUid { get; set; }

        [FirestoreProperty("profileName")]
        public string ProfileName { get; set; }

        [FirestoreProperty("bodyType")]
        public int BodyType { get; set; }

        [FirestoreProperty("createdAt")]
        public Firebase.Firestore.Timestamp CreatedAt { get; set; }

        [FirestoreProperty("lastLogin")]
        public Firebase.Firestore.Timestamp LastLogin { get; set; }

        // Helper properties to convert string to DateTime
        // public DateTime? CreatedAt 
        // { 
        //     get => string.IsNullOrEmpty(CreatedAtString) ? null : DateTime.Parse(CreatedAtString);
        //     set => CreatedAtString = value?.ToString("yyyy-MM-ddTHH:mm:ssZ");
        // }

        // public DateTime? LastLogin 
        // { 
        //     get => string.IsNullOrEmpty(LastLoginString) ? null : DateTime.Parse(LastLoginString);
        //     set => LastLoginString = value?.ToString("yyyy-MM-ddTHH:mm:ssZ");
        // }
    }
                
}