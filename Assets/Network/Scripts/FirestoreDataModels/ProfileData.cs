using System;
using System.Collections.Generic;
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

        [FirestoreProperty("characterId")]
        public string CharacterId { get; set; }

        [FirestoreProperty("equippedItems")]
        public Dictionary<string, string> EquippedItems { get; set; }

        [FirestoreProperty("createdAt")]
        public Firebase.Firestore.Timestamp CreatedAt { get; set; }

        [FirestoreProperty("lastLogin")]
        public Firebase.Firestore.Timestamp LastLogin { get; set; }
    }
}
