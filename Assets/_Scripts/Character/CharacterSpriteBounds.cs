using UnityEngine;

namespace QuizGame.Character
{
    public static class CharacterSpriteBounds
    {
        /// <summary>
        /// Bounds of every visible sprite under <paramref name="root"/>, expressed in <paramref name="space"/>'s local space.
        /// Uses sprite geometry rather than Renderer.bounds, so it also works on inactive objects.
        /// Empty (0x0) sprites from blank PSB layers are ignored.
        /// </summary>
        public static bool TryGetBounds(Transform root, Transform space, out Rect bounds)
        {
            bounds = default;
            bool found = false;
            var toSpace = space.worldToLocalMatrix;

            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var sprite = renderer.sprite;
                if (sprite == null || !renderer.enabled || sprite.rect.width < 1f || sprite.rect.height < 1f) continue;

                var matrix = toSpace * renderer.transform.localToWorldMatrix;
                var a = matrix.MultiplyPoint3x4(sprite.bounds.min);
                var b = matrix.MultiplyPoint3x4(sprite.bounds.max);
                var rect = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

                bounds = found
                    ? Rect.MinMaxRect(Mathf.Min(bounds.xMin, rect.xMin), Mathf.Min(bounds.yMin, rect.yMin), Mathf.Max(bounds.xMax, rect.xMax), Mathf.Max(bounds.yMax, rect.yMax))
                    : rect;
                found = true;
            }
            return found;
        }
    }
}
