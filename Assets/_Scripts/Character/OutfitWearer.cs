using System.Collections.Generic;
using QuizGame.Item;
using UnityEngine;

namespace QuizGame.Character
{
    /// <summary>
    /// Puts worn garments on a character by adding sprite renderers of its own, rather than by
    /// swapping labels through a SpriteLibrary.
    ///
    /// The label route cannot work for the characters the game actually ships: only three
    /// SpriteLibrary assets exist, all for the retired 001_Rabbit / 002_Cat / 003_Dog prefabs, so
    /// the twelve catalogue avatars have no SpriteResolver to swap anything on. Garment art also
    /// lives in its own PSBs and was never in a character's library to begin with.
    ///
    /// Drawing them separately is sound because every sprite involved — the character's parts and
    /// the garments alike — is now authored as a full 600x600 frame with its pivot at the centre.
    /// Two such sprites placed at the same origin line up exactly, whatever the species, so a
    /// garment needs no per-character offset. That only became true once the art was re-exported;
    /// against the old cropped files this approach would misplace every garment.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OutfitWearer : MonoBehaviour
    {
        /// <summary>Marks the renderers this component owns, so a re-dress only clears its own.</summary>
        private const string WornPrefix = "Worn_";

        /// <summary>
        /// How far in front of the character each slot sits. Garments are banded rather than
        /// slotted between individual body parts: each one is already drawn in its right place
        /// within the shared frame, so a hat and a shirt never overlap and only need to beat the
        /// body underneath them. Interleaving by part would also be unreliable — a cat names its
        /// parts "head+base" and "lower" while a rabbit names all ten after the rabbit.
        /// </summary>
        private static readonly CharacterPartType[] FrontToBack =
        {
            CharacterPartType.BodyDecoration,
            CharacterPartType.HeadDecoration,
            CharacterPartType.Prop
        };

        private readonly List<GameObject> worn = new List<GameObject>();
        private SpriteRenderer[] ownParts;
        private Transform artRoot;
        private Vector3 partLocalPosition;
        private int lowestOwnOrder;
        private int highestOwnOrder;

        private void Awake() => CaptureOwnParts();

        /// <summary>
        /// Takes the character's own renderers once, before anything is worn, so the sorting range
        /// is measured against the body and not against garments from an earlier outfit.
        /// </summary>
        private void CaptureOwnParts()
        {
            var found = GetComponentsInChildren<SpriteRenderer>(true);
            var kept = new List<SpriteRenderer>(found.Length);
            foreach (var renderer in found)
            {
                if (renderer == null || renderer.name.StartsWith(WornPrefix)) continue;
                kept.Add(renderer);
            }
            ownParts = kept.ToArray();

            lowestOwnOrder = 0;
            highestOwnOrder = 0;
            for (int i = 0; i < ownParts.Length; i++)
            {
                var order = ownParts[i].sortingOrder;
                if (i == 0 || order < lowestOwnOrder) lowestOwnOrder = order;
                if (i == 0 || order > highestOwnOrder) highestOwnOrder = order;
            }

            // Garments hang off the same node as the body parts, at the same local position, so
            // they pick up the frame's placement and scale instead of having to reproduce it. The
            // importer puts every part under an "Art" child that is scaled by half and offsets the
            // parts within it, so attaching to the character root would draw them twice the size
            // and a unit and a half too low.
            if (ownParts.Length > 0)
            {
                artRoot = ownParts[0].transform.parent;
                partLocalPosition = ownParts[0].transform.localPosition;
            }
            else
            {
                artRoot = transform;
                partLocalPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Replaces whatever is currently worn with these garments. Passing an empty list undresses
        /// the character.
        /// </summary>
        public void Wear(IReadOnlyList<OutfitItemSO> garments)
        {
            Clear();
            if (garments == null) return;
            if (ownParts == null) CaptureOwnParts();

            foreach (var garment in garments)
            {
                if (garment == null) continue;
                if (!TryReadSlot(garment.GetSlotName(), out var slot))
                {
                    Debug.LogWarning($"[OutfitWearer] '{garment.GetID()}' has slot '{garment.GetSlotName()}', " +
                                     "which is not a known part. Skipped.", this);
                    continue;
                }

                var order = SortingOrderFor(slot);
                var sprites = garment.GetPieceSprites();
                if (sprites == null) continue;

                foreach (var sprite in sprites)
                {
                    if (sprite == null) continue;
                    Draw(sprite, order, garment.GetID() + "_" + sprite.name);
                }
            }
        }

        /// <summary>
        /// The generator writes the slot as the exact enum name, so an exact parse is tried first.
        /// The tolerant lookup is the fallback for hand-authored assets, and its result is checked
        /// by round-tripping the name: it returns the first enum value rather than failing, so an
        /// unrecognised slot would otherwise come back silently as Mount.
        /// </summary>
        private static bool TryReadSlot(string slotName, out CharacterPartType slot)
        {
            slot = default;
            if (string.IsNullOrWhiteSpace(slotName)) return false;
            if (System.Enum.TryParse(slotName, out slot)) return true;

            slot = CharacterSpriteUtilities.GetPartTypeByCategoryName(slotName);
            var canonical = CharacterSpriteUtilities.GetCategoryNameByPartType(slot);
            return canonical != null
                && CharacterSpriteUtilities.NormalizeCategoryName(canonical)
                   == CharacterSpriteUtilities.NormalizeCategoryName(slotName);
        }

        /// <summary>Removes every garment this component added, leaving the character's own art.</summary>
        public void Clear()
        {
            foreach (var piece in worn)
            {
                if (piece == null) continue;
                if (Application.isPlaying) Destroy(piece);
                else DestroyImmediate(piece);
            }
            worn.Clear();
        }

        private void Draw(Sprite sprite, int sortingOrder, string name)
        {
            var piece = new GameObject(WornPrefix + name);
            piece.transform.SetParent(artRoot != null ? artRoot : transform, false);
            piece.transform.localPosition = partLocalPosition;
            piece.transform.localRotation = Quaternion.identity;
            piece.transform.localScale = Vector3.one;

            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            if (ownParts != null && ownParts.Length > 0)
            {
                // Share the body's sorting layer, otherwise the order above means nothing.
                renderer.sortingLayerID = ownParts[0].sortingLayerID;
                renderer.maskInteraction = ownParts[0].maskInteraction;
            }

            worn.Add(piece);
        }

        /// <summary>
        /// Wings and back effects go behind the whole character; everything else goes in front of
        /// it, in a fixed order among themselves. A slot that is not listed lands in front too,
        /// since being visible and slightly out of order beats vanishing without a word.
        /// </summary>
        private int SortingOrderFor(CharacterPartType slot)
        {
            if (slot == CharacterPartType.BackDecoration) return lowestOwnOrder - 1;

            for (int i = 0; i < FrontToBack.Length; i++)
            {
                if (FrontToBack[i] == slot) return highestOwnOrder + 1 + i;
            }
            return highestOwnOrder + 1 + FrontToBack.Length;
        }
    }
}
