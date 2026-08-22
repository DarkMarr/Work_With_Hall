using System;
using Newtonsoft.Json;

namespace QuizGame.MainMenu.UI
{
    [Serializable]
    public class SystemMessageData
    {
        [JsonProperty("message_header")]
        public string MessageHeader;

        [JsonProperty("is_new_message")]
        public bool isNewMessage;

        [JsonProperty("message_details")]
        public SystemMessageDetailsData messageDetails;

        public static string GetTempJsonData() => @"[
        {
            ""message_header"": ""Welcome Gift"",
            ""is_new_message"": true,
            ""message_details"": {
            ""body_message"": ""Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, 

when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. 

It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.

It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. 

Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose (injected humour and the like)."",
            ""is_already_accepted"": false,
            ""giving_items"": [
                { ""item_id"": ""CarryOn_Mockup1"", ""item_type"": 0, ""quantity"": 5 },
                { ""item_id"": ""CarryOn_Mockup2"", ""item_type"": 0, ""quantity"": 1 }
            ]
            }
        },
        {
            ""message_header"": ""Daily Reward"",
            ""is_new_message"": true,
            ""message_details"": null
        },
        {
            ""message_header"": ""Maintenance Compensation"",
            ""is_new_message"": false,
            ""message_details"": null
        },
        {
            ""message_header"": ""Special Event Report"",
            ""is_new_message"": true,
            ""message_details"": {
            ""body_message"": ""You placed in the top 10% of players!"",
            ""is_already_accepted"": false,
            ""giving_items"": null
            }
        },
        {
            ""message_header"": ""Friend Referral Bonus"",
            ""is_new_message"": false,
            ""message_details"": null
        },
        {
            ""message_header"": ""Weekly Challenge Completed"",
            ""is_new_message"": true,
            ""message_details"": {
            ""body_message"": ""You completed all weekly challenges!"",
            ""is_already_accepted"": false,
            ""giving_items"": null
            }
        },
        {
            ""message_header"": ""Level Up Reward"",
            ""is_new_message"": false,
            ""message_details"": null
        },
        {
            ""message_header"": ""Guild Contribution Gift"",
            ""is_new_message"": true,
            ""message_details"": null
        },
        {
            ""message_header"": ""Anniversary Celebration"",
            ""is_new_message"": true,
            ""message_details"": {
            ""body_message"": ""Happy anniversary! Enjoy these rewards."",
            ""is_already_accepted"": false,
            ""giving_items"": [
                { ""item_id"": ""CarryOn_Mockup5"", ""item_type"": 0, ""quantity"": 5 },
                { ""item_id"": ""CarryOn_Mockup7"", ""item_type"": 0, ""quantity"": 20 }
            ]
            }
        },
        {
            ""message_header"": ""Bug Report Thank You"",
            ""is_new_message"": false,
            ""message_details"": null
        }]";
    }
}
