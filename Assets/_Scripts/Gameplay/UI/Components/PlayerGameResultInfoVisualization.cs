using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace QuizGame.Gameplay.UI
{
    public class PlayerGameResultInfoVisualization : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI pointText;

        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private Image rankImage;

        [SerializeField]
        private Button addFriendButton;

        [SerializeField]
        private TextMeshProUGUI addFriendButtonText;

        [SerializeField] private Transform characterSlot;
        [SerializeField] private float characterHeight = 3f;
        [SerializeField] private float characterWidth = 2.3f;
        private GameObject characterInstance;
        public string DisplayedUserId { get; private set; }
        public string DisplayedCharacterId { get; private set; }

        public void Init(Sprite rankSprite, PlayerGameResultData data)
        {
            gameObject.SetActive(true);
            DisplayedUserId = data.UserId;
            nameText.text = data.Name ?? "Player";
            pointText.text = $"{data.Point} Points";
            rankText.text = data.RankName ?? string.Empty;
            rankImage.sprite = rankSprite;
            // Friend requests require a real backend; never randomly display Requested.
            addFriendButton.onClick.RemoveAllListeners();
            addFriendButton.gameObject.SetActive(false);
            ShowCharacter(data);
        }

        private void ShowCharacter(PlayerGameResultData data)
        {
            if (characterSlot == null) characterSlot = transform.Find("PlayerCharacter");
            if (characterSlot == null) return;
            foreach (Transform child in characterSlot) child.gameObject.SetActive(false);
            if (characterInstance != null) Destroy(characterInstance);
            DisplayedCharacterId = null;
            if (string.IsNullOrEmpty(data.CharacterId)) return;
            var info = QuizGame.Character.CharacterResourceManager.Instance.GetAllResources()
                .FirstOrDefault(c => c.GetID() == data.CharacterId);
            if (info == null || info.GetCharacterPrefab() == null) return;
            characterInstance = Instantiate(info.GetCharacterPrefab(), characterSlot, false);
            characterInstance.name = "SelectedCharacter_" + data.CharacterId;
            var player = characterInstance.GetComponent<QuizGame.Character.PlayerCharacter>()
                ?? characterInstance.AddComponent<QuizGame.Character.PlayerCharacter>();
            player.SetCharacterId(data.CharacterId);
            var outfit = new System.Collections.Generic.Dictionary<CharacterPartType, string>();
            if (data.EquippedItems != null)
                foreach (var entry in data.EquippedItems)
                    if (System.Enum.TryParse(entry.Key, out CharacterPartType part)) outfit[part] = entry.Value;
            player.ApplyOutfit(outfit);

            var renderers = characterInstance.GetComponentsInChildren<SpriteRenderer>();
            var bounds = new Bounds();
            bool measured = false;
            foreach (var renderer in renderers)
            {
                if (!renderer.enabled || renderer.sprite == null || renderer.sprite.rect.width < 1 || renderer.sprite.rect.height < 1) continue;
                var b = renderer.sprite.bounds;
                var matrix = characterInstance.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                for (int corner = 0; corner < 4; corner++)
                {
                    var point = matrix.MultiplyPoint3x4(new Vector3((corner & 1) == 0 ? b.min.x : b.max.x, (corner & 2) == 0 ? b.min.y : b.max.y, 0));
                    if (!measured) { bounds = new Bounds(point, Vector3.zero); measured = true; }
                    else bounds.Encapsulate(point);
                }
                renderer.sortingLayerID = rankImage.canvas.sortingLayerID;
                renderer.sortingOrder += rankImage.canvas.sortingOrder + 1;
            }
            if (measured && bounds.size.y > 0 && bounds.size.x > 0)
            {
                float scale = Mathf.Min(characterHeight / bounds.size.y, characterWidth / bounds.size.x);
                characterInstance.transform.localScale = Vector3.one * scale;
                characterInstance.transform.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, 0);
            }
            DisplayedCharacterId = data.CharacterId;
        }

        public void Hide() { gameObject.SetActive(false); }
    }
}
