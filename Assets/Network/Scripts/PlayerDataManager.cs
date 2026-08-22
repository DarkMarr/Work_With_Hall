using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using QuizGame.Utilities;
using QuizGame.Network.FirestoreDataModels;

namespace QuizGame.Network
{

    public class PlayerDataManager : MonoSingleton<PlayerDataManager>
    {
        private FirebaseFirestore db;
        private FirebaseAuth auth;

        public virtual void Start()
        {
            InitializeFirestore();
        }

        private void InitializeFirestore()
        {
            db = FirebaseFirestore.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
        }

        private string GetCurrentUserId()
        {
            return auth.CurrentUser?.UserId;
        }


        public async Task<ProfileData> GetProfileData()
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return null;

                DocumentSnapshot snapshot = await db.Collection("users").Document(userId).GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    if (snapshot.TryGetValue("profileData", out ProfileData profileData))
                    {
                        return profileData;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting profile data: {e.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProfileName(string profileName)
        {
            Debug.Log($"[PlayerDataManager] UpdateProfileName >> profileName: {profileName}");
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return false;

                await db.Collection("users").Document(userId).UpdateAsync("profileData.profileName", profileName);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating profile data: {e.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProfileBodyType(int bodyType)
        {
            Debug.Log($"[PlayerDataManager] UpdateProfileBodyType >> bodyType: {bodyType}");
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return false;

                await db.Collection("users").Document(userId).UpdateAsync("profileData.bodyType", bodyType);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating profile body type: {e.Message}");
                return false;
            }
        }

        public async Task<Energy> GetEnergyData()
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return null;

                DocumentSnapshot snapshot = await db.Collection("users").Document(userId).GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    if (snapshot.TryGetValue("energy", out Energy energy))
                    {
                        return energy;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting energy data: {e.Message}");
                return null;
            }
        }

    }
}
