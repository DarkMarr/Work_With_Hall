using Newtonsoft.Json;

namespace QuizGame.Gameplay
{
    public class PlayerGameResultData
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("rank_name")]
        public string RankName;

        [JsonProperty("point")]
        public int Point;

        public PlayerGameResultData(string name, int point)
        {
            Name = name;
            Point = point;
        }

        public static PlayerGameResultData[] FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PlayerGameResultData[]>(json);
        }

        public static string GetJsonTempData() => @"
        [
            {
                ""name"": ""Player1"",
                ""rank_name"": ""Kindergarten |||"",
                ""point"": 5000
            },
            {
                ""name"": ""Player2"",
                ""rank_name"": ""Kindergarten |||"",
                ""point"": 2200
            },
            {
                ""name"": ""Player3"",
                ""rank_name"": ""Kindergarten |||"",
                ""point"": 4000
            },
            {
                ""name"": ""Player4"",
                ""rank_name"": ""Kindergarten |||"",
                ""point"": 7000
            }
        ]";
    }
}
