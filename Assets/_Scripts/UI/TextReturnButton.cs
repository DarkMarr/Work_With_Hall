using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    /// <summary>
    /// Always return string on clicked event.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TextReturnButton : MonoBehaviour
    {
        public event Action<string> OnClicked;

        [SerializeField]
        private Button targetButton;

        [SerializeField]
        private TextMeshProUGUI returnText;

        private void OnValidate()
        {
            targetButton ??= GetComponent<Button>();
            returnText ??= GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            targetButton.onClick.AddListener(() => OnClicked?.Invoke(returnText.text));
        }
    }
}
