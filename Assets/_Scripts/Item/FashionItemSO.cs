using NaughtyAttributes;
using UnityEngine;

namespace QuizGame.Item
{
    /// <summary>
    /// A single wearable garment — one slot of one outfit, such as the hat from the summer set.
    ///
    /// The drop tables award garments individually rather than whole outfits ("Head of set
    /// ฤดูร้อน" is one reward), so this is what goes in a reward pool and what a player owns.
    /// <c>FashionSetSO</c> stays the description of a complete look; these are its parts.
    ///
    /// Derives from <see cref="EquipmentItemSO"/> so it drops into the existing
    /// <c>DestinationItemReward.equipmentReward</c> array without widening that type.
    /// </summary>
    [CreateAssetMenu(fileName = "newFashionItem", menuName = "QuizGame/Item/Fashion Piece", order = 2)]
    public class FashionItemSO : EquipmentItemSO
    {
        [Header("Fashion")]
        [SerializeField]
        [Tooltip("Body slot, spelled as a CharacterPartType name — HeadDecoration, BodyDecoration, " +
                 "BackDecoration, ArmDecorationLeft, ArmDecorationRight or Prop.")]
        private string slot;

        [SerializeField, ReadOnly, Tooltip("Id of the FashionSetSO this garment belongs to.")]
        private string fashionSetID;

        [SerializeField, Tooltip("The garment's own artwork, drawn in the shared 600x600 frame.")]
        [ShowAssetPreview]
        private Sprite pieceSprite;

        /// <summary>
        /// The slot as a name rather than a CharacterPartType, because QuizGame.Character already
        /// references QuizGame.Item and the reverse edge would make the two assemblies circular.
        /// Character-side callers resolve it with CharacterSpriteUtilities.GetPartTypeByCategoryName,
        /// which normalises spelling, so this is the same interchange PlayerOutfit.EquippedItems uses.
        /// </summary>
        public string GetSlotName() => slot;

        /// <summary>
        /// Which outfit this garment completes. Resolve it through FashionResourceManager rather
        /// than holding a direct reference, for the same assembly reason.
        /// </summary>
        public string GetFashionSetID() => fashionSetID;

        /// <summary>
        /// The sprite to put on the character. <see cref="BaseItemSO.GetSprite"/> stays the
        /// inventory icon, which may be framed or cropped differently.
        /// </summary>
        public Sprite GetPieceSprite() => pieceSprite;
    }
}
