using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay
{
    // Vector placeholder only. Replace with an Image on the same RectTransform for final art.
    [AddComponentMenu("QuizGame/Lucky Draw/Template Shape")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LuckyDrawTemplateShape : MaskableGraphic
    {
        [Range(3, 64)] public int sides = 8;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var centre = r.center;
            vh.AddVert(centre, color, new Vector2(.5f, .5f));
            int count = Mathf.Clamp(sides, 3, 64);
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)count * 360f + 22.5f) * Mathf.Deg2Rad;
                var uv = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .5f;
                vh.AddVert(centre + Vector2.Scale(uv, r.size), color, uv + Vector2.one * .5f);
            }
            for (int i = 0; i < count; i++) vh.AddTriangle(0, i + 1, (i + 1) % count + 1);
        }
    }
}
