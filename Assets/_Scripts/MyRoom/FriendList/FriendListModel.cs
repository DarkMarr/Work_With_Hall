using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace QuizGame.MyRoom.FriendList
{
    [Serializable]
    public class FriendListModel
    {
        public event Action<FriendData> OnFriendListUpdate;
        public event Action<FriendData> OnFriendRequestedUpdate;

        private List<FriendData> FriendRequestedList;
        private List<FriendData> FriendList;

        public FriendListModel(List<FriendData> friendListDatas, List<FriendData> friendRequestedDatas)
        {
            FriendList = friendListDatas;
            FriendRequestedList = friendRequestedDatas;
        }

        public int GetFriendAmount() => FriendList.Count;
        public FriendData GetFriendAtIndex(int index) => FriendList[index];
        public List<FriendData> GetAllFriends() => FriendList.ToList();

        public void AddFriend(FriendData friend)
        {
            FriendList.Add(friend);
            OnFriendListUpdate?.Invoke(friend);
        }

        public void DeleteFriendAtInDex(int friendIndex)
        {
            FriendList.RemoveAt(friendIndex);
        }

        public void DeleteFriend(FriendData friend)
        {
            FriendList.Remove(friend);
            OnFriendListUpdate?.Invoke(friend);
        }

        public List<FriendData> GetAllFriendRequestedList() => FriendRequestedList.ToList();

        public void DeleteFriendRequested(FriendData friend)
        {
            FriendRequestedList.Remove(friend);
            OnFriendRequestedUpdate?.Invoke(friend);
        }

        public FriendData GetFriendRequestedAtIndex(int index) => FriendRequestedList[index];


        public static string GetFriendTempDataJson() => @"[
                { ""uid"": ""user001"", ""name"": ""Alice"", ""rank"": ""Gold"" },
                { ""uid"": ""user002"", ""name"": ""Bob"", ""rank"": ""Silver"" },
                { ""uid"": ""user003"", ""name"": ""Charlie"", ""rank"": ""Bronze"" },
                { ""uid"": ""user004"", ""name"": ""Diana"", ""rank"": ""Platinum"" },
                { ""uid"": ""user005"", ""name"": ""Ethan"", ""rank"": ""Gold"" },
                { ""uid"": ""user006"", ""name"": ""Fiona"", ""rank"": ""Silver"" },
                { ""uid"": ""user007"", ""name"": ""George"", ""rank"": ""Bronze"" },
                { ""uid"": ""user008"", ""name"": ""Hannah"", ""rank"": ""Gold"" },
                { ""uid"": ""user009"", ""name"": ""Ivan"", ""rank"": ""Platinum"" },
                { ""uid"": ""user010"", ""name"": ""Judy"", ""rank"": ""Silver"" },
                { ""uid"": ""user011"", ""name"": ""Kevin"", ""rank"": ""Bronze"" },
                { ""uid"": ""user012"", ""name"": ""Luna"", ""rank"": ""Gold"" },
                { ""uid"": ""user013"", ""name"": ""Mike"", ""rank"": ""Silver"" },
                { ""uid"": ""user014"", ""name"": ""Nina"", ""rank"": ""Bronze"" },
                { ""uid"": ""user015"", ""name"": ""Oscar"", ""rank"": ""Gold"" },
                { ""uid"": ""user016"", ""name"": ""Paula"", ""rank"": ""Platinum"" },
                { ""uid"": ""user017"", ""name"": ""Quinn"", ""rank"": ""Silver"" },
                { ""uid"": ""user018"", ""name"": ""Rita"", ""rank"": ""Bronze"" },
                { ""uid"": ""user019"", ""name"": ""Sam"", ""rank"": ""Gold"" },
                { ""uid"": ""user020"", ""name"": ""Tina"", ""rank"": ""Silver"" }
            ]";

        public static string GetFriendRequestedTempDataJson() => @"[
            { ""uid"": ""user021"", ""name"": ""Uma"", ""rank"": ""Gold"" },
            { ""uid"": ""user022"", ""name"": ""Victor"", ""rank"": ""Silver"" },
            { ""uid"": ""user023"", ""name"": ""Wendy"", ""rank"": ""Bronze"" },
            { ""uid"": ""user024"", ""name"": ""Xander"", ""rank"": ""Platinum"" },
            { ""uid"": ""user025"", ""name"": ""Yara"", ""rank"": ""Gold"" },
            { ""uid"": ""user026"", ""name"": ""Zane"", ""rank"": ""Silver"" },
            { ""uid"": ""user027"", ""name"": ""Ava"", ""rank"": ""Bronze"" },
            { ""uid"": ""user028"", ""name"": ""Ben"", ""rank"": ""Gold"" },
            { ""uid"": ""user029"", ""name"": ""Cara"", ""rank"": ""Platinum"" },
            { ""uid"": ""user030"", ""name"": ""Dan"", ""rank"": ""Silver"" },
            { ""uid"": ""user031"", ""name"": ""Elle"", ""rank"": ""Bronze"" },
            { ""uid"": ""user032"", ""name"": ""Finn"", ""rank"": ""Gold"" },
            { ""uid"": ""user033"", ""name"": ""Gina"", ""rank"": ""Silver"" },
            { ""uid"": ""user034"", ""name"": ""Hugo"", ""rank"": ""Bronze"" },
            { ""uid"": ""user035"", ""name"": ""Isla"", ""rank"": ""Gold"" },
            { ""uid"": ""user036"", ""name"": ""Jack"", ""rank"": ""Platinum"" },
            { ""uid"": ""user037"", ""name"": ""Kira"", ""rank"": ""Silver"" },
            { ""uid"": ""user038"", ""name"": ""Leo"", ""rank"": ""Bronze"" },
            { ""uid"": ""user039"", ""name"": ""Maya"", ""rank"": ""Gold"" },
            { ""uid"": ""user040"", ""name"": ""Noah"", ""rank"": ""Silver"" }
        ]";
    }
}
