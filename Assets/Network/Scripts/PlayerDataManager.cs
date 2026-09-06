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
    /// <summary>
    /// Central connector for receiving/sending the signed-in user's data from/to Firestore.
    /// Every gameplay system that needs user data should go through this manager.
    /// All methods are async and fail-safe: they return null/false on errors instead of throwing.
    /// </summary>
    public class PlayerDataManager : MonoSingleton<PlayerDataManager>
    {
        public const string USERS_COLLECTION = "users";

        // Default values used when creating a new user document.
        private const int DEFAULT_GEMS = 100;
        private const int DEFAULT_COINS = 1000;
        private const int DEFAULT_ENERGY = 5;
        private const int DEFAULT_MAX_ENERGY = 5;

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

        private DocumentReference GetUserDocument()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("[PlayerDataManager] No signed-in user. Firestore call skipped.");
                return null;
            }
            return db.Collection(USERS_COLLECTION).Document(userId);
        }

        #region User Document

        /// <summary>
        /// Creates the user document with default data if it does not exist yet.
        /// Must be called once after a successful sign-in (any provider) before any Update calls,
        /// because Firestore UpdateAsync fails when the document doesn't exist.
        /// </summary>
        public async Task<bool> EnsureUserDocumentExists()
        {
            var userDoc = GetUserDocument();
            if (userDoc == null) return false;

            try
            {
                var snapshot = await userDoc.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    // Keep lastLogin fresh.
                    await userDoc.UpdateAsync("profileData.lastLogin", Timestamp.GetCurrentTimestamp());
                    return true;
                }

                var newPlayerData = CreateDefaultPlayerData();
                await userDoc.SetAsync(newPlayerData, SetOptions.MergeAll);
                Debug.Log("[PlayerDataManager] Created new user document with default data.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error ensuring user document: {e.Message}");
                return false;
            }
        }

        private PlayerData CreateDefaultPlayerData()
        {
            return new PlayerData
            {
                ProfileData = new ProfileData
                {
                    GameUid = GetCurrentUserId(),
                    ProfileName = string.Empty,
                    BodyType = -1,
                    CharacterId = string.Empty,
                    EquippedItems = new Dictionary<string, string>(),
                    CreatedAt = Timestamp.GetCurrentTimestamp(),
                    LastLogin = Timestamp.GetCurrentTimestamp()
                },
                Energy = new Energy
                {
                    Current = DEFAULT_ENERGY,
                    Max = DEFAULT_MAX_ENERGY,
                    EnergyRegenRate = 1,
                    EnergyRegenIntervalSeconds = 600,
                    LastEnergyUpdateTimestamp = Timestamp.GetCurrentTimestamp()
                },
                Inventory = new Inventory
                {
                    Gems = DEFAULT_GEMS,
                    Coins = DEFAULT_COINS,
                    Materials = new Dictionary<string, int>(),
                    Items = new List<InventoryItem>()
                },
                MultiPlayerStats = new MultiPlayerStats(),
                SinglePlayerStats = new Dictionary<string, SinglePlayerStats>()
            };
        }

        /// <summary>
        /// Receives the whole player data document from the database.
        /// </summary>
        public async Task<PlayerData> GetPlayerData()
        {
            var userDoc = GetUserDocument();
            if (userDoc == null) return null;

            try
            {
                var snapshot = await userDoc.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    return snapshot.ConvertTo<PlayerData>();
                }
                Debug.LogWarning("[PlayerDataManager] User document does not exist.");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting player data: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Profile Data

        public async Task<ProfileData> GetProfileData()
        {
            try
            {
                var playerData = await GetPlayerData();
                return playerData?.ProfileData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting profile data: {e.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProfileName(string profileName)
        {
            Debug.Log($"[PlayerDataManager] UpdateProfileName >> profileName: {profileName}");
            return await UpdateUserField("profileData.profileName", profileName);
        }

        public async Task<bool> UpdateProfileBodyType(int bodyType)
        {
            Debug.Log($"[PlayerDataManager] UpdateProfileBodyType >> bodyType: {bodyType}");
            return await UpdateUserField("profileData.bodyType", bodyType);
        }

        /// <summary>
        /// Saves the currently selected character (e.g. "001_Rabbit_base").
        /// </summary>
        public async Task<bool> UpdateSelectedCharacter(string characterId)
        {
            Debug.Log($"[PlayerDataManager] UpdateSelectedCharacter >> characterId: {characterId}");
            return await UpdateUserField("profileData.characterId", characterId ?? string.Empty);
        }

        /// <summary>
        /// Saves the whole equipped items map (part name -> item id) at once.
        /// </summary>
        public async Task<bool> UpdateEquippedItems(Dictionary<string, string> equippedItems)
        {
            Debug.Log($"[PlayerDataManager] UpdateEquippedItems >> {equippedItems?.Count ?? 0} part(s)");
            return await UpdateUserField("profileData.equippedItems", equippedItems ?? new Dictionary<string, string>());
        }

        /// <summary>
        /// Updates a single equipped part. Pass an empty/null itemId to unequip the part.
        /// </summary>
        public async Task<bool> UpdateEquippedPart(string partName, string itemId)
        {
            var profileData = await GetProfileData();
            var equipped = profileData?.EquippedItems ?? new Dictionary<string, string>();

            if (string.IsNullOrEmpty(itemId))
            {
                equipped.Remove(partName);
            }
            else
            {
                equipped[partName] = itemId;
            }

            return await UpdateEquippedItems(equipped);
        }

        #endregion

        #region Energy

        public async Task<Energy> GetEnergyData()
        {
            try
            {
                var playerData = await GetPlayerData();
                return playerData?.Energy;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting energy data: {e.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateEnergy(int current, int max)
        {
            var updates = new Dictionary<string, object>
            {
                { "energy.current", current },
                { "energy.max", max },
                { "energy.lastEnergyUpdateTimestamp", Timestamp.GetCurrentTimestamp() }
            };
            return await UpdateUserFields(updates);
        }

        #endregion

        #region Inventory / Currency

        /// <summary>
        /// Receives the player inventory (currencies, materials and items) from the database.
        /// </summary>
        public async Task<Inventory> GetInventory()
        {
            try
            {
                var playerData = await GetPlayerData();
                return playerData?.Inventory;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting inventory: {e.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCurrencies(int gems, int coins)
        {
            var updates = new Dictionary<string, object>
            {
                { "inventory.gems", gems },
                { "inventory.coins", coins }
            };
            return await UpdateUserFields(updates);
        }

        /// <summary>
        /// Tries to spend the given amount of currency. Returns false when the player
        /// doesn't have enough or when the update fails.
        /// </summary>
        public async Task<bool> TrySpendCurrency(CurrencyType currencyType, int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("[PlayerDataManager] TrySpendCurrency amount must be positive.");
                return false;
            }

            var inventory = await GetInventory();
            if (inventory == null)
            {
                Debug.LogWarning("[PlayerDataManager] TrySpendCurrency failed: no inventory data.");
                return false;
            }

            var isGem = currencyType == CurrencyType.Gem;
            var currentAmount = isGem ? inventory.Gems : inventory.Coins;
            if (currentAmount < amount)
            {
                Debug.Log($"[PlayerDataManager] Not enough {currencyType}. Has: {currentAmount}, Needs: {amount}");
                return false;
            }

            var newAmount = currentAmount - amount;
            var updates = new Dictionary<string, object>
            {
                { isGem ? "inventory.gems" : "inventory.coins", newAmount }
            };
            return await UpdateUserFields(updates);
        }

        public async Task<bool> AddCurrency(CurrencyType currencyType, int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("[PlayerDataManager] AddCurrency amount must be positive.");
                return false;
            }

            var inventory = await GetInventory();
            if (inventory == null) return false;

            var isGem = currencyType == CurrencyType.Gem;
            var updates = new Dictionary<string, object>
            {
                { isGem ? "inventory.gems" : "inventory.coins", (isGem ? inventory.Gems : inventory.Coins) + amount }
            };
            return await UpdateUserFields(updates);
        }

        /// <summary>
        /// Adds (or increases the quantity of) an item in the player inventory.
        /// </summary>
        public async Task<bool> AddInventoryItem(string itemId, string itemName, string itemType, int quantity)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0)
            {
                Debug.LogWarning("[PlayerDataManager] AddInventoryItem called with invalid itemId or quantity.");
                return false;
            }

            var inventory = await GetInventory();
            if (inventory == null) return false;

            var items = inventory.Items ?? new List<InventoryItem>();
            var existing = items.Find(item => item.ItemId == itemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                items.Add(new InventoryItem
                {
                    ItemId = itemId,
                    Name = itemName,
                    Type = itemType,
                    Quantity = quantity,
                    AcquiredAt = Timestamp.GetCurrentTimestamp(),
                    Equipped = false
                });
            }

            return await UpdateUserField("inventory.items", items);
        }

        /// <summary>
        /// Removes the given quantity of an item from the inventory. Removes the entry when it reaches zero.
        /// </summary>
        public async Task<bool> RemoveInventoryItem(string itemId, int quantity)
        {
            var inventory = await GetInventory();
            if (inventory == null) return false;

            var items = inventory.Items ?? new List<InventoryItem>();
            var existing = items.Find(item => item.ItemId == itemId);
            if (existing == null)
            {
                Debug.LogWarning($"[PlayerDataManager] RemoveInventoryItem: item '{itemId}' not found.");
                return false;
            }

            existing.Quantity -= quantity;
            if (existing.Quantity <= 0)
            {
                items.Remove(existing);
            }

            return await UpdateUserField("inventory.items", items);
        }

        /// <summary>
        /// Receives all owned inventory item entries.
        /// </summary>
        public async Task<List<InventoryItem>> GetInventoryItems()
        {
            var inventory = await GetInventory();
            return inventory?.Items ?? new List<InventoryItem>();
        }

        public async Task<bool> SetInventoryItemEquipped(string itemId, bool equipped)
        {
            var inventory = await GetInventory();
            if (inventory == null) return false;

            var items = inventory.Items ?? new List<InventoryItem>();
            var existing = items.Find(item => item.ItemId == itemId);
            if (existing == null)
            {
                Debug.LogWarning($"[PlayerDataManager] SetInventoryItemEquipped: item '{itemId}' not found.");
                return false;
            }

            existing.Equipped = equipped;
            return await UpdateUserField("inventory.items", items);
        }

        /// <summary>
        /// Receives all materials owned by the player.
        /// </summary>
        public async Task<Dictionary<string, int>> GetMaterials()
        {
            var inventory = await GetInventory();
            return inventory?.Materials ?? new Dictionary<string, int>();
        }

        public async Task<bool> UpdateMaterials(Dictionary<string, int> materials)
        {
            return await UpdateUserField("inventory.materials", materials ?? new Dictionary<string, int>());
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Receives single player statistics of a quiz category (e.g. "General").
        /// </summary>
        public async Task<SinglePlayerStats> GetSinglePlayerStats(string category)
        {
            try
            {
                var playerData = await GetPlayerData();
                if (playerData?.SinglePlayerStats != null && playerData.SinglePlayerStats.TryGetValue(category, out var stats))
                {
                    return stats;
                }
                return new SinglePlayerStats();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting single player stats: {e.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateSinglePlayerStats(string category, SinglePlayerStats stats)
        {
            if (string.IsNullOrEmpty(category) || stats == null) return false;
            return await UpdateUserField($"singlePlayerStats.{category}", stats);
        }

        public async Task<MultiPlayerStats> GetMultiPlayerStats()
        {
            try
            {
                var playerData = await GetPlayerData();
                return playerData?.MultiPlayerStats ?? new MultiPlayerStats();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting multiplayer stats: {e.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateMultiPlayerStats(MultiPlayerStats stats)
        {
            if (stats == null) return false;
            return await UpdateUserField("multiPlayerStats", stats);
        }

        public async Task<Achievement[]> GetAchievements()
        {
            try
            {
                var userDoc = GetUserDocument();
                if (userDoc == null) return null;

                var snapshot = await userDoc.Collection("achievements").GetSnapshotAsync();
                var achievements = new List<Achievement>();
                foreach (var document in snapshot.Documents)
                {
                    achievements.Add(document.ConvertTo<Achievement>());
                }
                return achievements.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting achievements: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Friends

        /// <summary>
        /// Receives the friend list of the current user.
        /// </summary>
        public async Task<List<Friend>> GetFriends()
        {
            try
            {
                var userDoc = GetUserDocument();
                if (userDoc == null) return new List<Friend>();

                var snapshot = await userDoc.Collection("friends").GetSnapshotAsync();
                var friends = new List<Friend>();
                foreach (var document in snapshot.Documents)
                {
                    friends.Add(document.ConvertTo<Friend>());
                }
                return friends;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error getting friends: {e.Message}");
                return new List<Friend>();
            }
        }

        public async Task<bool> AddFriend(Friend friend)
        {
            if (friend == null || string.IsNullOrEmpty(friend.UserId)) return false;

            try
            {
                var userDoc = GetUserDocument();
                if (userDoc == null) return false;

                await userDoc.Collection("friends").Document(friend.UserId).SetAsync(friend, SetOptions.MergeAll);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error adding friend: {e.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFriend(string friendUserId)
        {
            if (string.IsNullOrEmpty(friendUserId)) return false;

            try
            {
                var userDoc = GetUserDocument();
                if (userDoc == null) return false;

                await userDoc.Collection("friends").Document(friendUserId).DeleteAsync();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error removing friend: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Shared Update Helpers

        private async Task<bool> UpdateUserField(string field, object value)
        {
            var userDoc = GetUserDocument();
            if (userDoc == null) return false;

            try
            {
                await userDoc.UpdateAsync(field, value);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error updating field '{field}': {e.Message}");
                return false;
            }
        }

        private async Task<bool> UpdateUserFields(Dictionary<string, object> updates)
        {
            var userDoc = GetUserDocument();
            if (userDoc == null) return false;

            try
            {
                await userDoc.UpdateAsync(updates);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Error updating fields: {e.Message}");
                return false;
            }
        }

        #endregion
    }
}
