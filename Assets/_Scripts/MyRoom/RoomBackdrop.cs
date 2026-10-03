using UnityEngine;

namespace QuizGame.MyRoom
{
    /// <summary>
    /// The room the player stands in.
    ///
    /// This used to be a full-screen Image on the overlay canvas, which drew over the world no
    /// matter what any sorting order said — it hid the player, the decoration slots and everything
    /// else the scene contains. It is a world sprite now, behind the rest.
    ///
    /// Wallpapers are drawn at whatever size suited the artist, so the backdrop measures the one it
    /// is given rather than trusting a scale set by hand: swapping a 1260x2048 wallpaper for a
    /// 2160x3840 one must not move the furniture out from under the decoration slots.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class RoomBackdrop : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The camera the wallpaper has to fill. The main camera is used when this is empty.")]
        private Camera viewCamera;

        [SerializeField]
        [Tooltip("How much wider than the screen the wallpaper is drawn. The overlay background it " +
                 "replaced was 10% wide, and the decoration slots were placed against that.")]
        private float widthOverscan = 1.1f;

        private SpriteRenderer spriteRenderer;

        private SpriteRenderer Renderer
        {
            get
            {
                if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
                return spriteRenderer;
            }
        }

        private void OnEnable() => Fit();

        private void OnValidate() => Fit();

        /// <summary>Hangs a different wallpaper, sized to the room it has to fill.</summary>
        public void SetWallpaper(Sprite wallpaper)
        {
            if (wallpaper == null) return;
            Renderer.sprite = wallpaper;
            Fit();
        }

        /// <summary>
        /// Scales the wallpaper to fill the camera's height. Width is taken from the height rather
        /// than from the image, so a wallpaper drawn at a different aspect still covers the screen
        /// edge to edge and still lines up with where the slots were placed.
        /// </summary>
        private void Fit()
        {
            var camera = viewCamera != null ? viewCamera : Camera.main;
            if (camera == null || !camera.orthographic || Renderer == null || Renderer.sprite == null) return;

            transform.localScale = Vector3.one;
            var drawn = Renderer.bounds.size;
            if (drawn.x <= 0f || drawn.y <= 0f) return;

            var viewHeight = camera.orthographicSize * 2f;
            var viewWidth = viewHeight * camera.aspect * widthOverscan;
            transform.localScale = new Vector3(viewWidth / drawn.x, viewHeight / drawn.y, 1f);
        }
    }
}
