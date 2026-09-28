using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuizGame.Gameplay
{
    [Serializable]
    public class PlayerGameResultData
    {
        [JsonProperty("user_id")] public string UserId;
        [JsonProperty("name")] public string Name;
        [JsonProperty("rank_name")] public string RankName;
        [JsonProperty("point")] public int Point;
        [JsonProperty("character_id")] public string CharacterId;
        [JsonProperty("equipped_items")] public Dictionary<string, string> EquippedItems;
        [JsonIgnore] public bool IsLocalPlayer;

        public PlayerGameResultData(string name, int point) { Name = name; Point = point; }

        public PlayerGameResultData Copy() => new PlayerGameResultData(Name, Point) {
            UserId = UserId, RankName = RankName, CharacterId = CharacterId,
            IsLocalPlayer = IsLocalPlayer,
            EquippedItems = EquippedItems == null ? null : new Dictionary<string, string>(EquippedItems)
        };

        public static PlayerGameResultData[] FromJson(string json) => JsonConvert.DeserializeObject<PlayerGameResultData[]>(json);
    }
}
