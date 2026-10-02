using System.Collections.Generic;
using QuizGame.Item;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Character.Outfit.UI
{
    /// <summary>
    /// The dressing screen. Shows what the player owns for one slot at a time and puts it on the
    /// character behind the panel, so the result is seen on the real avatar rather than a copy.
    ///
    /// The rows and tabs are built in code from a single template each, which keeps the prefab to
    /// a handful of objects: a grid that grows with the wardrobe is not something to lay out by
    /// hand, and one template is easier to restyle than forty.
    /// </summary>
    public sealed class WardrobeUI : BaseUI
    {
        [Header("Wiring")]
        [SerializeField, Tooltip("Tab buttons for the four slots are created under here.")]
        private RectTransform tabBar;

        [SerializeField, Tooltip("Garment buttons are created under here.")]
        private RectTransform itemGrid;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button undressButton;
        [SerializeField] private TMP_Text emptyLabel;

        [Header("Templates")]
        [SerializeField, Tooltip("Hidden button copied for each tab.")]
        private Button tabTemplate;

        [SerializeField, Tooltip("Hidden button copied for each garment.")]
        private Button itemTemplate;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly WardrobeController wardrobe = new WardrobeController();
        private CharacterPartType currentSlot = WardrobeController.Slots[0];
        private bool busy;

        public void Init(System.Action onClose)
        {
            if (tabTemplate != null) tabTemplate.gameObject.SetActive(false);
            if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);

            if (closeButton != null) closeButton.onClick.AddListener(() => onClose?.Invoke());
            if (undressButton != null) undressButton.onClick.AddListener(UndressAll);

            BuildTabs();
            Refresh();
        }

        private async void Refresh()
        {
            await wardrobe.Load();
            if (this == null) return;
            ShowSlot(currentSlot);
        }

        /// <summary>
        /// Shows a fixed set of garments instead of the player's inventory, so the screen can be
        /// laid out and checked without a signed-in player behind it. Tapping still equips for
        /// real, so this is for inspecting the screen, not for handing anyone free clothes.
        /// </summary>
        public void ShowPreview(IEnumerable<OutfitItemSO> garments,
                                IReadOnlyDictionary<string, string> equipped = null)
        {
            if (tabTemplate != null) tabTemplate.gameObject.SetActive(false);
            if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
            if (tabBar != null && tabBar.childCount <= 1) BuildTabs();
            wardrobe.LoadFrom(garments, equipped);
            ShowSlot(currentSlot);
        }

        private void BuildTabs()
        {
            if (tabBar == null || tabTemplate == null) return;

            foreach (var slot in WardrobeController.Slots)
            {
                var captured = slot;
                var tab = Instantiate(tabTemplate, tabBar);
                tab.gameObject.SetActive(true);
                tab.onClick.AddListener(() => ShowSlot(captured));
                SetLabel(tab, Readable(slot));
                spawned.Add(tab.gameObject);
            }
        }

        private void ShowSlot(CharacterPartType slot)
        {
            currentSlot = slot;
            ClearGrid();

            var owned = wardrobe.Owned(slot);
            if (emptyLabel != null)
            {
                emptyLabel.gameObject.SetActive(owned.Count == 0);
                emptyLabel.text = "No " + Readable(slot).ToLowerInvariant() + " yet";
            }
            if (itemGrid == null || itemTemplate == null) return;

            foreach (var garment in owned)
            {
                var captured = garment;
                var button = Instantiate(itemTemplate, itemGrid);
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() => Toggle(captured));

                // Skip the button's own Image, which is its background: GetComponentInChildren
                // returns self first, so searching for the icon that way finds the wrong one.
                var icon = FindIcon(button.transform);
                if (icon != null && garment.GetSprite() != null)
                {
                    icon.sprite = garment.GetSprite();
                    icon.enabled = true;
                }
                SetLabel(button, garment.GetName());

                // A worn garment is marked rather than hidden, so taking it off is the same tap.
                var tint = button.GetComponent<Image>();
                if (tint != null)
                {
                    tint.color = wardrobe.IsEquipped(garment) ? new Color(1f, 0.92f, 0.5f) : Color.white;
                }

                spawned.Add(button.gameObject);
            }
        }

        /// <summary>
        /// One tap wears or removes. Ignored while a save is in flight, otherwise a second tap
        /// would race the first and the screen could end up disagreeing with what was stored.
        /// </summary>
        private async void Toggle(OutfitItemSO garment)
        {
            if (busy) return;
            busy = true;
            await wardrobe.Toggle(garment);
            busy = false;
            if (this == null) return;
            ShowSlot(currentSlot);
        }

        private async void UndressAll()
        {
            if (busy) return;
            busy = true;
            await wardrobe.UndressAll();
            busy = false;
            if (this == null) return;
            ShowSlot(currentSlot);
        }

        private void ClearGrid()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                var go = spawned[i];
                if (go == null || go.transform.parent != itemGrid) continue;
                spawned.RemoveAt(i);
                Destroy(go);
            }
        }

        /// <summary>The Image on a child, never the one on the button itself.</summary>
        private static Image FindIcon(Transform button)
        {
            for (int i = 0; i < button.childCount; i++)
            {
                var image = button.GetChild(i).GetComponent<Image>();
                if (image != null) return image;
            }
            return null;
        }

        private static void SetLabel(Component root, string text)
        {
            var label = root.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text;
        }

        /// <summary>Slot names the player sees: "Head" rather than "HeadDecoration".</summary>
        private static string Readable(CharacterPartType slot)
        {
            switch (slot)
            {
                case CharacterPartType.HeadDecoration: return "Head";
                case CharacterPartType.BodyDecoration: return "Body";
                case CharacterPartType.Prop: return "Hand";
                case CharacterPartType.BackDecoration: return "Back";
                default: return slot.ToString();
            }
        }
    }
}
