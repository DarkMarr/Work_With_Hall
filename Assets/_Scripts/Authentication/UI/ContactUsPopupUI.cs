using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QuizGame.Network;
using QuizGame.Scene;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class ContactUsPopupUI : BaseUI
    {
        private const string SupportEmail = "waquizsupport@gmail.com";
        private const int MaxMessageLength = 1000;

        private TMP_InputField messageInput;

        public static ContactUsPopupUI Open(Transform parent)
        {
            var existing = parent.GetComponentInChildren<ContactUsPopupUI>(true);
            if (existing != null)
            {
                existing.Show();
                return existing;
            }

            var popupObject = new GameObject("ContactUsPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ContactUsPopupUI));
            popupObject.transform.SetParent(parent, false);

            var popup = popupObject.GetComponent<ContactUsPopupUI>();
            popup.BuildUI();
            popup.Show();
            return popup;
        }

        private void BuildUI()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var overlay = GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.65f);
            overlay.raycastTarget = true;

            var panel = CreatePanel("Panel", transform, new Vector2(900f, 700f), Vector2.zero, new Color(0.96f, 0.96f, 0.96f, 1f));

            CreateText("Title", panel.transform, "Contact Us", new Vector2(0f, 265f), new Vector2(760f, 70f), 46f, TextAlignmentOptions.Center);
            CreateText("Instruction", panel.transform, "Tell us how we can help. Maximum 1000 characters.", new Vector2(0f, 205f), new Vector2(760f, 60f), 26f, TextAlignmentOptions.Center);

            var inputObject = new GameObject("MessageInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            var inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.SetParent(panel.transform, false);
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = new Vector2(0f, 50f);
            inputRect.sizeDelta = new Vector2(760f, 250f);

            inputObject.GetComponent<Image>().color = Color.white;
            messageInput = inputObject.GetComponent<TMP_InputField>();
            messageInput.characterLimit = MaxMessageLength;
            messageInput.lineType = TMP_InputField.LineType.MultiLineNewline;
            messageInput.richText = false;
            messageInput.textComponent = CreateInputText(inputRect, "Text");
            messageInput.placeholder = CreatePlaceholder(inputRect, "Placeholder", "Write your message here...");

            CreateText("Counter", panel.transform, "0 / 1000", new Vector2(315f, -90f), new Vector2(120f, 40f), 20f, TextAlignmentOptions.Right);

            var sendButton = CreateButton("SendMailButton", panel.transform, "Send Mail", new Vector2(-190f, -180f), new Vector2(320f, 90f));
            sendButton.onClick.AddListener(SendMail);

            var deleteButton = CreateButton("DeleteAccountButton", panel.transform, "Delete Account", new Vector2(190f, -180f), new Vector2(320f, 90f));
            deleteButton.onClick.AddListener(ConfirmDeleteAccount);

            var closeButton = CreateButton("CloseButton", panel.transform, "Close", new Vector2(0f, -285f), new Vector2(320f, 75f));
            closeButton.onClick.AddListener(ClosePopup);

            messageInput.onValueChanged.AddListener(UpdateCharacterCounter);
        }

        private void SendMail()
        {
            var message = messageInput?.text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning("[ContactUs] Cannot send empty support message.");
                return;
            }

            var email = NetworkAuth.Instance.GetCurrentUserEmail() ?? "Unknown";
            var subject = "WaQuiz Support";
            var body = $"User Email: {email}\n\n{message}";
            var mailto = $"mailto:{SupportEmail}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";

            Application.OpenURL(mailto);
        }

        private void ConfirmDeleteAccount()
        {
            if (!NetworkAuth.Instance.IsUserSignedIn())
            {
                Debug.LogWarning("[ContactUs] No signed-in user to delete.");
                return;
            }

            var confirmPopup = UIManager.Instance.Create<ConfirmPopupUI>();
            confirmPopup.Setup(
                title: "Delete Account?",
                description: "This permanently deletes your Firebase Authentication account. You may need to sign in again before deletion.",
                onConfirmButtonClicked: async () =>
                {
                    confirmPopup.Close();
                    var success = await NetworkAuth.Instance.DeleteAccountAsync();
                    if (success)
                    {
                        SceneManager.LoadScene(SceneList.Authentication.ToString());
                    }
                    else
                    {
                        Debug.LogError("[ContactUs] Account deletion failed. Firebase may require recent authentication.");
                    }
                },
                onCancelButtonClicked: () => confirmPopup.Close()
            );
        }

        private void UpdateCharacterCounter(string value)
        {
            var counter = transform.Find("Panel/Counter")?.GetComponent<TextMeshProUGUI>();
            if (counter != null)
            {
                counter.text = $"{value.Length} / {MaxMessageLength}";
            }
        }

        private void ClosePopup()
        {
            Close();
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 position, Color backgroundColor)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = panelObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            panelObject.GetComponent<Image>().color = backgroundColor;
            return panelObject;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.black;
            text.raycastTarget = false;
            return text;
        }

        private static TextMeshProUGUI CreateInputText(RectTransform inputRect, string name)
        {
            var text = CreateText(name, inputRect, string.Empty, Vector2.zero, Vector2.zero, 30f, TextAlignmentOptions.TopLeft);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(20f, 15f);
            text.rectTransform.offsetMax = new Vector2(-20f, -15f);
            text.enableWordWrapping = true;
            return text;
        }

        private static TextMeshProUGUI CreatePlaceholder(RectTransform inputRect, string name, string value)
        {
            var placeholder = CreateText(name, inputRect, value, Vector2.zero, Vector2.zero, 28f, TextAlignmentOptions.TopLeft);
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = new Vector2(20f, 15f);
            placeholder.rectTransform.offsetMax = new Vector2(-20f, -15f);
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            placeholder.enableWordWrapping = true;
            return placeholder;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            buttonObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            var button = buttonObject.GetComponent<Button>();
            var labelText = CreateText("Label", buttonObject.transform, label, Vector2.zero, Vector2.zero, 30f, TextAlignmentOptions.Center);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.color = Color.white;
            return button;
        }
    }
}
