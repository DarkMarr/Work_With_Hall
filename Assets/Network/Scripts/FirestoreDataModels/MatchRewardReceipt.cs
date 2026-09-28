using Firebase.Firestore;

namespace QuizGame.Network.FirestoreDataModels
{
    [FirestoreData]
    public class MatchRewardReceipt
    {
        [FirestoreProperty("itemId")] public string ItemId { get; set; }
        [FirestoreProperty("name")] public string Name { get; set; }
        [FirestoreProperty("type")] public string Type { get; set; }
        [FirestoreProperty("quantity")] public int Quantity { get; set; }
        [FirestoreProperty("place")] public int Place { get; set; }
        [FirestoreProperty("rankingPointsDelta")] public int RankingPointsDelta { get; set; }
        [FirestoreProperty("rankingPointsAfter")] public int RankingPointsAfter { get; set; }
    }
}
