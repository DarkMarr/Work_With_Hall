using System;
using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [Serializable]
    [FirestoreData]
    public class Achievement
    {
        [FirestoreProperty("achievementId")]
        public string AchievementId { get; set; }

        [FirestoreProperty("earnedAt")]
        public Timestamp EarnedAt { get; set; }

        [FirestoreProperty("progress")]
        public int Progress { get; set; }
    }
}
