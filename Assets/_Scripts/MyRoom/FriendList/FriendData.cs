using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuizGame.MyRoom.FriendList
{
    [Serializable]
    public struct FriendData
    {
        [JsonProperty("uid")]
        public string UID;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("rank")]
        public string Rank;

        public FriendData(string uID, string name, string rank)
        {
            UID = uID;
            Name = name;
            Rank = rank;
        }

        public static List<FriendData> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<List<FriendData>>(json);
        }
    }
}
