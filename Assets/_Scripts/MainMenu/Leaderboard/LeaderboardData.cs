using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace QuizGame.MainMenu.Leaderboard
{
    [Serializable]
    public class LeaderboardData
    {
        [JsonProperty("player_name")]
        public string PlayerName;

        [JsonProperty("rank_number")]
        public int RankNumber;

        [JsonProperty("rank_name")]
        public string RankName;

        [JsonProperty("rank_score")]
        public int RankScore;

        public static string GetTempJsonData() =>
        @"[
            {""player_name"":""ZaneTiger"",""rank_number"":1,""rank_name"":""Gold"",""rank_score"":1000},
            {""player_name"":""LunaSpark"",""rank_number"":2,""rank_name"":""Gold"",""rank_score"":980},
            {""player_name"":""Kirox"",""rank_number"":3,""rank_name"":""Gold"",""rank_score"":960},
            {""player_name"":""NovaStorm"",""rank_number"":4,""rank_name"":""Gold"",""rank_score"":940},
            {""player_name"":""EchoShade"",""rank_number"":5,""rank_name"":""Gold"",""rank_score"":920},
            {""player_name"":""RogueLynx"",""rank_number"":6,""rank_name"":""Gold"",""rank_score"":900},
            {""player_name"":""AxelFury"",""rank_number"":7,""rank_name"":""Gold"",""rank_score"":880},
            {""player_name"":""SeraBlitz"",""rank_number"":8,""rank_name"":""Gold"",""rank_score"":860},
            {""player_name"":""JunoStrike"",""rank_number"":9,""rank_name"":""Gold"",""rank_score"":840},
            {""player_name"":""CrimsonWisp"",""rank_number"":10,""rank_name"":""Gold"",""rank_score"":820},
            {""player_name"":""BlitzRaven"",""rank_number"":11,""rank_name"":""Silver"",""rank_score"":800},
            {""player_name"":""NyxVolt"",""rank_number"":12,""rank_name"":""Silver"",""rank_score"":780},
            {""player_name"":""FrostViper"",""rank_number"":13,""rank_name"":""Silver"",""rank_score"":760},
            {""player_name"":""VexShadow"",""rank_number"":14,""rank_name"":""Silver"",""rank_score"":740},
            {""player_name"":""SkyeNova"",""rank_number"":15,""rank_name"":""Silver"",""rank_score"":720},
            {""player_name"":""QuantumBlitz"",""rank_number"":16,""rank_name"":""Silver"",""rank_score"":700},
            {""player_name"":""EmberStrike"",""rank_number"":17,""rank_name"":""Silver"",""rank_score"":680},
            {""player_name"":""PhantomFlare"",""rank_number"":18,""rank_name"":""Silver"",""rank_score"":660},
            {""player_name"":""DriftHex"",""rank_number"":19,""rank_name"":""Silver"",""rank_score"":640},
            {""player_name"":""AshVortex"",""rank_number"":20,""rank_name"":""Silver"",""rank_score"":620},
            {""player_name"":""ZephyrClaw"",""rank_number"":21,""rank_name"":""Silver"",""rank_score"":600},
            {""player_name"":""NeonScythe"",""rank_number"":22,""rank_name"":""Silver"",""rank_score"":580},
            {""player_name"":""EchoBlade"",""rank_number"":23,""rank_name"":""Silver"",""rank_score"":560},
            {""player_name"":""FlintRider"",""rank_number"":24,""rank_name"":""Silver"",""rank_score"":540},
            {""player_name"":""MiraFox"",""rank_number"":25,""rank_name"":""Silver"",""rank_score"":520},
            {""player_name"":""BoltNova"",""rank_number"":26,""rank_name"":""Silver"",""rank_score"":500},
            {""player_name"":""TyroShade"",""rank_number"":27,""rank_name"":""Silver"",""rank_score"":480},
            {""player_name"":""ShadowLoom"",""rank_number"":28,""rank_name"":""Bronze"",""rank_score"":460},
            {""player_name"":""VantaFlicker"",""rank_number"":29,""rank_name"":""Bronze"",""rank_score"":440},
            {""player_name"":""BlazeSnare"",""rank_number"":30,""rank_name"":""Bronze"",""rank_score"":420},
            {""player_name"":""NekoPulse"",""rank_number"":31,""rank_name"":""Bronze"",""rank_score"":400},
            {""player_name"":""OrbitPyre"",""rank_number"":32,""rank_name"":""Bronze"",""rank_score"":380},
            {""player_name"":""JadeWhisper"",""rank_number"":33,""rank_name"":""Bronze"",""rank_score"":360},
            {""player_name"":""FalconZ"",""rank_number"":34,""rank_name"":""Bronze"",""rank_score"":340},
            {""player_name"":""KitsuNova"",""rank_number"":35,""rank_name"":""Bronze"",""rank_score"":320},
            {""player_name"":""GlitchTiger"",""rank_number"":36,""rank_name"":""Bronze"",""rank_score"":300},
            {""player_name"":""HoloStorm"",""rank_number"":37,""rank_name"":""Bronze"",""rank_score"":280},
            {""player_name"":""WispFang"",""rank_number"":38,""rank_name"":""Bronze"",""rank_score"":260},
            {""player_name"":""CypherWolf"",""rank_number"":39,""rank_name"":""Bronze"",""rank_score"":240},
            {""player_name"":""DuskRay"",""rank_number"":40,""rank_name"":""Bronze"",""rank_score"":220},
            {""player_name"":""NovaRogue"",""rank_number"":41,""rank_name"":""Bronze"",""rank_score"":200},
            {""player_name"":""OnyxFlame"",""rank_number"":42,""rank_name"":""Bronze"",""rank_score"":180},
            {""player_name"":""SparkFrost"",""rank_number"":43,""rank_name"":""Bronze"",""rank_score"":160},
            {""player_name"":""VoltSpecter"",""rank_number"":44,""rank_name"":""Bronze"",""rank_score"":140},
            {""player_name"":""BlitzKite"",""rank_number"":45,""rank_name"":""Bronze"",""rank_score"":120},
            {""player_name"":""EchoFlare"",""rank_number"":46,""rank_name"":""Bronze"",""rank_score"":100},
            {""player_name"":""ShadowTide"",""rank_number"":47,""rank_name"":""Bronze"",""rank_score"":80},
            {""player_name"":""TwilightAsh"",""rank_number"":48,""rank_name"":""Bronze"",""rank_score"":60},
            {""player_name"":""GaleNova"",""rank_number"":49,""rank_name"":""Bronze"",""rank_score"":40},
            {""player_name"":""PixelDrift"",""rank_number"":50,""rank_name"":""Bronze"",""rank_score"":20}
        ]";



        public static string GenerateLeaderboardJson()
        {
            StringBuilder jsonBuilder = new StringBuilder();

            // Start of the verbatim string literal content
            jsonBuilder.Append("[\n");

            System.Random random = new System.Random();

            // Define possible rank names and their associated score ranges
            List<(string name, int minScore, int maxScore)> rankTiers = new List<(string, int, int)>
        {
            ("Bronze", 0, 499),
            ("Silver", 500, 1499),
            ("Gold", 1500, 2999),
            ("Platinum", 3000, 4499),
            ("Diamond", 4500, 5999),
            ("Master", 6000, 7499),
            ("Grandmaster", 7500, 8999),
            ("Challenger", 9000, 10000)
        };

            // Generate 1000 users
            for (int i = 0; i < 1000; i++)
            {
                int rankNumber = i + 1; // Rank from 1 to 1000

                // Generate a random player name
                string playerName = GenerateRandomPlayerName(random);

                // Generate a random score
                int score = random.Next(0, 10001); // Score between 0 and 10000

                // Determine rank name based on score
                string rankName = "Unranked";
                foreach (var tier in rankTiers)
                {
                    if (score >= tier.minScore && score <= tier.maxScore)
                    {
                        rankName = tier.name;
                        break;
                    }
                }

                // Append the JSON for the current user entry with proper escaping for verbatim string literal
                // Each "" becomes one " in the final string content
                jsonBuilder.Append($"  {{\"\"player_name\"\":\"\"{playerName}\"\"," +
                                   $"\"\"rank_number\"\":{rankNumber}," +
                                   $"\"\"rank_name\"\":\"\"{rankName}\"\"," +
                                   $"\"\"rank_score\"\":{score}}}");

                // Add a comma and newline for all but the last entry
                if (i < 999)
                {
                    jsonBuilder.Append(",\n");
                }
                else
                {
                    jsonBuilder.Append("\n"); // Newline for the last entry before closing bracket
                }
            }

            jsonBuilder.Append("]"); // End of JSON array

            // Wrap the entire content in the verbatim string literal syntax
            // Example: @"[ ... ]"
            return "@\"" + jsonBuilder.ToString().Replace("\"", "\"\"") + "\"";
        }

        private static string GenerateRandomPlayerName(System.Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = random.Next(6, 12); // Random name length between 6 and 11 characters
            char[] nameChars = new char[length];
            for (int j = 0; j < length; j++)
            {
                nameChars[j] = chars[random.Next(chars.Length)];
            }
            return "Player_" + new string(nameChars);
        }
    }
}
