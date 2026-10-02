using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuizGame.Item;
using QuizGame.Network;
using UnityEngine;

namespace QuizGame.Character.Outfit
{
    /// <summary>
    /// What the player can wear, and what they are wearing, with no reference to any UI.
    ///
    /// Keeping the two apart means the dressing screen can be rebuilt or replaced without touching
    /// the rules, and that these rules can be exercised in the editor before a single prefab exists.
    /// </summary>
    public sealed class WardrobeController
    {
        /// <summary>The slots a player can fill, in the order a dressing screen should show them.</summary>
        public static readonly CharacterPartType[] Slots =
        {
            CharacterPartType.HeadDecoration,
            CharacterPartType.BodyDecoration,
            CharacterPartType.Prop,
            CharacterPartType.BackDecoration
        };

        private readonly Dictionary<CharacterPartType, List<OutfitItemSO>> ownedBySlot =
            new Dictionary<CharacterPartType, List<OutfitItemSO>>();

        /// <summary>Slot to the id of the garment worn there. Empty slots are absent.</summary>
        public IReadOnlyDictionary<string, string> Equipped { get; private set; } =
            new Dictionary<string, string>();

        /// <summary>
        /// Reads the player's inventory and keeps only entries that resolve to a garment. An
        /// inventory holds materials, currency and quiz items too, and ids that no longer match an
        /// asset — a reward from a build that has since been changed — simply drop out.
        /// </summary>
        public async Task Load()
        {
            var inventory = await PlayerDataManager.Instance.GetInventoryItems();
            var profile = await PlayerDataManager.Instance.GetProfileData();

            var garments = new List<OutfitItemSO>();
            if (inventory != null)
            {
                foreach (var entry in inventory)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.ItemId)) continue;
                    var garment = OutfitItemResourceManager.Instance?.GetResource(entry.ItemId);
                    if (garment != null) garments.Add(garment);
                }
            }

            LoadFrom(garments, profile?.EquippedItems);
        }

        /// <summary>
        /// Groups a given set of garments, separately from fetching them. Splitting the two lets
        /// the screen be driven from a fixed list — a preview, or a check that it renders before
        /// the player owns anything — without a signed-in player behind it.
        /// </summary>
        public void LoadFrom(IEnumerable<OutfitItemSO> garments, IReadOnlyDictionary<string, string> equipped)
        {
            ownedBySlot.Clear();
            foreach (var slot in Slots) ownedBySlot[slot] = new List<OutfitItemSO>();
            Equipped = equipped ?? new Dictionary<string, string>();

            if (garments != null)
            {
                foreach (var garment in garments)
                {
                    if (garment == null) continue;
                    if (!OutfitSlots.TryReadSlot(garment.GetSlotName(), out var slot)) continue;
                    if (!ownedBySlot.TryGetValue(slot, out var list)) continue;
                    if (list.Any(x => x.GetID() == garment.GetID())) continue;
                    list.Add(garment);
                }
            }

            // Rarest first, then by name, so the screen opens on what the player most wants to see.
            foreach (var slot in Slots)
            {
                ownedBySlot[slot].Sort((a, b) =>
                {
                    var byTier = b.GetItemTier().CompareTo(a.GetItemTier());
                    return byTier != 0 ? byTier : string.CompareOrdinal(a.GetID(), b.GetID());
                });
            }
        }

        /// <summary>Garments the player owns for that slot. Never null.</summary>
        public IReadOnlyList<OutfitItemSO> Owned(CharacterPartType slot)
        {
            return ownedBySlot.TryGetValue(slot, out var list) ? list : new List<OutfitItemSO>();
        }

        /// <summary>The garment worn in that slot, or null when the slot is empty.</summary>
        public OutfitItemSO EquippedIn(CharacterPartType slot)
        {
            if (!Equipped.TryGetValue(slot.ToString(), out var id) || string.IsNullOrEmpty(id)) return null;
            return Owned(slot).FirstOrDefault(x => x.GetID() == id);
        }

        public bool IsEquipped(OutfitItemSO garment)
        {
            return garment != null
                && Equipped.TryGetValue(garment.GetSlotName(), out var id)
                && id == garment.GetID();
        }

        /// <summary>
        /// Wears the garment, or takes it off if it is already on — one tap does both, which is how
        /// the slot is cleared without a separate control for it.
        /// </summary>
        public async Task Toggle(OutfitItemSO garment)
        {
            if (garment == null) return;
            var manager = PlayerCharacterManager.Instance;
            if (manager == null) return;

            if (IsEquipped(garment))
            {
                if (OutfitSlots.TryReadSlot(garment.GetSlotName(), out var slot))
                {
                    await manager.UnequipPart(slot);
                }
            }
            else
            {
                await manager.EquipGarment(garment);
            }

            var profile = await PlayerDataManager.Instance.GetProfileData();
            Equipped = profile?.EquippedItems ?? new Dictionary<string, string>();
        }

        /// <summary>Takes everything off, one slot at a time so each is saved.</summary>
        public async Task UndressAll()
        {
            var manager = PlayerCharacterManager.Instance;
            if (manager == null) return;

            foreach (var slot in Slots)
            {
                if (EquippedIn(slot) == null) continue;
                await manager.UnequipPart(slot);
            }

            var profile = await PlayerDataManager.Instance.GetProfileData();
            Equipped = profile?.EquippedItems ?? new Dictionary<string, string>();
        }
    }
}
