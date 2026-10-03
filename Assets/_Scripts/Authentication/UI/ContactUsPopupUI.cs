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
    /// <summary>
    /// The support form, reachable from the title screen and the sign-in screen.
    ///
    /// This used to assemble itself in code, every rectangle and colour spelled out in BuildUI.
    /// That left it the one popup the artists could not touch: the message box sprites live
    /// outside Resources, so nothing running at runtime could reach them. It is a prefab now,
    /// like every other popup, and the look lives where the look belongs.
    /// </summary>
    public class ContactUsPopupUI : BaseUI
    {
        private const string SupportEmail = "waquizsupport@gmail.com";
        private const int MaxMessageLength = 1000;
        private const string PrefabPath = "UI/PopupUI/ContactUsPopupUI";

        [SerializeField]
        private TMP_InputField messageInput;

        [SerializeField]
        private TextMeshProUGUI counterText;

        [SerializeField]
        private Button sendMailButton;

        [SerializeField]
        private Button deleteAccountButton;

        [SerializeField]
        private Button closeButton;

        /// <summary>
        /// Shows the form under <paramref name="parent"/>, reusing the one already there rather
        /// than stacking a second copy — the two screens that open this both keep theirs around.
        /// </summary>
        public static ContactUsPopupUI Open(Transform parent)
        {
            var existing = parent.GetComponentInChildren<ContactUsPopupUI>(true);
            if (existing != null)
            {
                existing.Show();
                return existing;
            }

            // UnityEngine.Resources spelled out: the project has its own QuizGame.Resources
            // namespace, and an unqualified Resources here resolves to that one.
            var prefab = UnityEngine.Resources.Load<ContactUsPopupUI>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[ContactUs] Prefab not found at Resources/{PrefabPath}.");
                return null;
            }

            var popup = Instantiate(prefab, parent, false);
            popup.name = "ContactUsPopup";
            popup.Show();
            return popup;
        }

        protected override void Awake()
        {
            base.Awake();

            if (messageInput != null)
            {
                messageInput.characterLimit = MaxMessageLength;
                messageInput.onValueChanged.AddListener(UpdateCharacterCounter);
                UpdateCharacterCounter(messageInput.text);
            }

            sendMailButton?.onClick.AddListener(SendMail);
            deleteAccountButton?.onClick.AddListener(ConfirmDeleteAccount);
            closeButton?.onClick.AddListener(ClosePopup);
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
                onCancelButtonClicked: () => confirmPopup.Close(),
                // Deleting the account cannot be taken back, so the header says so before the text does.
                tone: PopupTone.Alert
            );
        }

        private void UpdateCharacterCounter(string value)
        {
            if (counterText == null) return;
            counterText.text = $"{(value ?? string.Empty).Length} / {MaxMessageLength}";
        }

        private void ClosePopup()
        {
            Close();
        }
    }
}
