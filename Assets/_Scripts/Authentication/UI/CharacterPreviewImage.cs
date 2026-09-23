using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Authentication.UI
{
    /// <summary>
    /// Shows a SpriteRenderer-based character prefab inside a uGUI canvas by copying
    /// each renderer's sprite into an Image, keeping layout and draw order, scaled to fit this rect.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CharacterPreviewImage : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1f)]
        private float fillRatio = 0.95f;

        private readonly List<Image> parts = new List<Image>();

        public void Show(GameObject characterPrefab)
        {
            Clear();
            if (characterPrefab == null) return;

            var root = characterPrefab.transform;
            var toRoot = root.worldToLocalMatrix;

            var renderers = new List<SpriteRenderer>(characterPrefab.GetComponentsInChildren<SpriteRenderer>(false));
            // Empty PSB layers import as 0x0 sprites, which make uGUI produce an invalid AABB.
            renderers.RemoveAll(r => r.sprite == null || !r.enabled || r.sprite.rect.width < 1f || r.sprite.rect.height < 1f);
            if (renderers.Count == 0) return;

            // Stable sort by draw order; hierarchy order breaks ties like the SpriteRenderer batching does.
            var hierarchyOrder = new Dictionary<SpriteRenderer, int>();
            for (int i = 0; i < renderers.Count; i++) hierarchyOrder[renderers[i]] = i;
            renderers.Sort((a, b) =>
            {
                int layer = SortingLayer.GetLayerValueFromID(a.sortingLayerID).CompareTo(SortingLayer.GetLayerValueFromID(b.sortingLayerID));
                if (layer != 0) return layer;
                int order = a.sortingOrder.CompareTo(b.sortingOrder);
                return order != 0 ? order : hierarchyOrder[a].CompareTo(hierarchyOrder[b]);
            });

            // Measure each sprite in the prefab root's local space.
            var rects = new List<Rect>(renderers.Count);
            var total = new Rect();
            for (int i = 0; i < renderers.Count; i++)
            {
                var r = renderers[i];
                var matrix = toRoot * r.transform.localToWorldMatrix;
                var b = r.sprite.bounds;
                var min = matrix.MultiplyPoint3x4(b.min);
                var max = matrix.MultiplyPoint3x4(b.max);
                var rect = Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
                rects.Add(rect);
                total = i == 0 ? rect : Rect.MinMaxRect(Mathf.Min(total.xMin, rect.xMin), Mathf.Min(total.yMin, rect.yMin), Mathf.Max(total.xMax, rect.xMax), Mathf.Max(total.yMax, rect.yMax));
            }

            var area = ((RectTransform)transform).rect.size * fillRatio;
            float scale = Mathf.Min(area.x / total.width, area.y / total.height);

            for (int i = 0; i < renderers.Count; i++)
            {
                var r = renderers[i];
                var go = new GameObject(r.name, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(transform, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = rects[i].size * scale;
                rt.anchoredPosition = (rects[i].center - total.center) * scale;
                rt.localScale = new Vector3(r.flipX ? -1f : 1f, r.flipY ? -1f : 1f, 1f);

                var image = go.GetComponent<Image>();
                image.sprite = r.sprite;
                image.color = r.color;
                image.raycastTarget = false;
                parts.Add(image);
            }
        }

        public void Clear()
        {
            foreach (var part in parts)
            {
                if (part != null) Destroy(part.gameObject);
            }
            parts.Clear();
        }
    }
}
