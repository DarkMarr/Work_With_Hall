using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace QuizGame.MyRoom.Decoration
{
    /// <summary>
    /// One place in the room where a decoration can stand.
    ///
    /// The art gives every slot its own shape — a shelf compartment is tall, a table top is square,
    /// the window is taller still — so a slot carries its size and everything else is measured from
    /// it: the dashed outline, the highlight when it is being edited, and the decoration itself.
    /// Nothing here is scaled by hand, because a decoration swapped at runtime has no way of knowing
    /// what was set in the inspector.
    /// </summary>
    [ExecuteAlways]
    public class DecorationSlot : MonoBehaviour
    {
        [SerializeField]
        private string SlotID;

        [SerializeField]
        private DecorationType decorationType;

        [SerializeField]
        private LocalizedString itemTypeLocalized;

        [SerializeField]
        [Tooltip("How much room this slot gives a decoration, in world units.")]
        private Vector2 slotSize = new Vector2(1.8f, 1.8f);

        [SerializeField]
        private SpriteRenderer decorationSpriteRenderer;

        [SerializeField]
        private Button changeSpriteButton;

        [SerializeField]
        private GameObject selectingVisual;

        [Header("Sized from slotSize")]
        [SerializeField]
        [Tooltip("The canvas holding the dashed outline. Its rect is set from the slot size.")]
        private RectTransform outlineCanvas;

        [SerializeField]
        [Tooltip("The highlight drawn round the slot being edited. Nine-sliced to the slot size.")]
        private SpriteRenderer selectedFrame;

        [SerializeField]
        [Tooltip("The gear badge the art hangs on the bottom edge of the highlight.")]
        private Transform selectedGear;

        public void Init(Action onChangeDecorationButtonClicked)
        {
            changeSpriteButton.onClick.AddListener(() => onChangeDecorationButtonClicked?.Invoke());
            SetSelectingVisual(false);
        }

        public DecorationType GetDecorationType() => decorationType;
        public string GetTypeName() => itemTypeLocalized.GetLocalizedString();
        public string GetID() => SlotID;

        private void Awake() => ApplySlotSize();

        private void OnValidate() => ApplySlotSize();

        public void SetChangeSpriteButtonActive(bool isActive)
        {
            changeSpriteButton.gameObject.SetActive(isActive);
        }

        public void SetSprite(Sprite sprite)
        {
            decorationSpriteRenderer.sprite = sprite;
            FitDecoration();
        }

        public void SetSelectingVisual(bool isActive)
        {
            selectingVisual.SetActive(isActive);
        }

        /// <summary>Lays the outline, the highlight and the decoration out to this slot's size.</summary>
        private void ApplySlotSize()
        {
            if (outlineCanvas != null)
            {
                // The canvas is scaled right down so its pixels read as world units; dividing by that
                // scale turns the slot size back into the rect the outline has to fill.
                var canvasScale = outlineCanvas.localScale.x;
                if (canvasScale > 0f) outlineCanvas.sizeDelta = slotSize / canvasScale;
            }

            if (selectedFrame != null)
            {
                selectedFrame.transform.localScale = Vector3.one;
                selectedFrame.drawMode = SpriteDrawMode.Sliced;
                selectedFrame.size = slotSize;

                if (selectedGear != null)
                {
                    selectedGear.localPosition = new Vector3(0f, -slotSize.y * 0.5f, 0f);
                }
            }

            FitDecoration();
        }

        /// <summary>
        /// Sits the decoration inside the slot without stretching it. Decorations are drawn at
        /// whatever size suited the artist, so a tall one and a wide one both have to be made to fit
        /// the same frame rather than filling it.
        ///
        /// Placing it is not the same as centring it. Most decorations rest on something — a shelf,
        /// a table top, the floor — so they stand on the bottom of their frame and grow upwards,
        /// which is also where their pivot usually is. Something seen through the window has no surface
        /// under it, so it sits in the middle of its frame instead.
        /// </summary>
        private void FitDecoration()
        {
            if (decorationSpriteRenderer == null || decorationSpriteRenderer.sprite == null) return;

            var drawnBounds = decorationSpriteRenderer.sprite.bounds;
            if (drawnBounds.size.x <= 0f || drawnBounds.size.y <= 0f) return;

            var fit = Mathf.Min(slotSize.x / drawnBounds.size.x, slotSize.y / drawnBounds.size.y);
            decorationSpriteRenderer.transform.localScale = Vector3.one * fit;

            // Worked from the sprite rather than the renderer's bounds, which are only right once
            // the renderer has been drawn at the new scale.
            var offsetX = -drawnBounds.center.x * fit;
            var offsetY = decorationType == DecorationType.Window
                ? -drawnBounds.center.y * fit
                : -slotSize.y * 0.5f - drawnBounds.min.y * fit;

            decorationSpriteRenderer.transform.localPosition = new Vector3(offsetX, offsetY, 0f);
        }
    }
}
