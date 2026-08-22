// filepath: /Users/supakij/Workspaces/wa-japan-quiz/quizgame-unity/Assets/Network/Scripts/FirestoreData/Models/Friend.cs
using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class Friend
    {
        [FirestoreProperty("userId")]
        public string UserId { get; set; }

        [FirestoreProperty("username")]
        public string Username { get; set; }

        [FirestoreProperty("rank")]
        public string Rank { get; set; }

        [FirestoreProperty("friendRequest")]
        public string FriendRequest { get; set; }
    }
}