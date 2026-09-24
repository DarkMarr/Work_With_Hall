using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuizGame.Authentication.UI;
using QuizGame.Character;
using QuizGame.UI;
using QuizGame.Scene;
using QuizGame.Network;
using QuizGame.Network.FirestoreDataModels;

namespace QuizGame.Authentication
{
    public class AuthenticationController : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] bodyTypeSprites; //TODO: replace with real sprites

        private BaseUI currentUI;
        private bool profileOperationRunning;

        private void Start()
        {
            UIManager.Instance.CloseAll();

            var startGameUI = UIManager.Instance.Replace<StartGameUI>(ref currentUI);
            startGameUI.OnStartGameButtonClicked += async () =>
            {
                // Check if the user is already signed in
                if (NetworkAuth.Instance.IsAlreadySignedIn())
                {
                    await ContinueAfterSignIn();
                    return;
                }
                // If not signed in, open the login or register UI
                OpenLoginOrRegisterUI();
            };
        }

        private void Update()
        {
            //Back button on phone
            if (!profileOperationRunning && Input.GetKeyDown(KeyCode.Escape))
            {
                switch (currentUI)
                {
                    case RecoverSubmittedUI:
                    case CreateNewAccountUI:
                    case RecoverAccountUI:
                    case SignInUI:
                        OpenLoginOrRegisterUI();
                        break;

                    case CreateProfileNameUI:
                        OpenSignInUI();
                        break;

                    case CreateProfileBodyTypeUI:
                    case CreateCharacterUI:
                        OpenCreateProfileNameUI();
                        break;
                }
            }
        }

        public void OpenLoginOrRegisterUI()
        {
            var loginAndRegisterUI = UIManager.Instance.Replace<LoginAndRegisterUI>(ref currentUI);
            loginAndRegisterUI.Init(
                onGoogleSignInButtonClicked: () =>
                {
                    SignInWithGoogle();
                },
                onAppleSignInButtonClicked: () =>
                {
                    SignInWithApple();
                },
                onSignInWithEmailButtonClicked: () =>
                {
                    OpenSignInUI();
                },
                onCreateAccountButtonClicked: () =>
                {
                    OpenCreateNewAccountUI();
                },
                onRecoverAccountButtonClicked: () =>
                {
                    OpenRecoverAccountUI();
                }
            );
        }

        public void OpenMainMenu()
        {
            var gotResponse = false;
            var defaultTransitionUI = UIManager.Instance.Replace<DefaultTransitionUI>(ref currentUI);
            defaultTransitionUI.Init(
                completeCondition: () => gotResponse,
                onTransitionEnd: () =>
                {
                    SceneManager.LoadScene(SceneList.MainMenu.ToString());
                });

            // await Task.Delay(2000);
            gotResponse = true;
        }

        private void OpenSignInUI()
        {
            var signInUI = UIManager.Instance.Replace<SignInUI>(ref currentUI);
            signInUI.Init((email, password) =>
            {
                SignInAccount(email, password);
                signInUI.Close();
            });
        }

        private void OpenCreateNewAccountUI()
        {
            var createNewAccountUI = UIManager.Instance.Replace<CreateNewAccountUI>(ref currentUI);
            createNewAccountUI.Init((email, password, reEnterPassword) =>
            {
                CreateNewAccount(email, password, reEnterPassword, () => createNewAccountUI.Close());
            });
        }

        private void OpenRecoverAccountUI()
        {
            var recoverAccountUI = UIManager.Instance.Replace<RecoverAccountUI>(ref currentUI);
            recoverAccountUI.Init(email =>
            {
                RecoverAccount(email);
                recoverAccountUI.Close();
            });
        }

        private void OpenCreateBodyTypeUI()
        {
            var createProfileBodyTypeUI = UIManager.Instance.Replace<CreateProfileBodyTypeUI>(ref currentUI);
            createProfileBodyTypeUI.Init(
                bodySprites: bodyTypeSprites,
                onCreateProfileBodyTypeClicked: bodyTypeID =>
                {
                    CreateProfileBodyType(bodyTypeID);
                    createProfileBodyTypeUI.Close();
                }
            );
        }

        private void OpenCreateCharacterUI()
        {
            // Starter avatars: every Uncommon avatar, in design-sheet ID order.
            var starterCharacters = CharacterResourceManager.Instance.GetAllResources()
                .Where(character => character.GetRarity() == CharacterRarity.Uncommon)
                .OrderBy(character => character.GetAvatarCode(), StringComparer.Ordinal)
                .ToArray();

            var createCharacterUI = UIManager.Instance.Replace<CreateCharacterUI>(ref currentUI);
            createCharacterUI.Init(starterCharacters, character =>
            {
                CreateCharacter(character);
            });
        }

        private void OpenCreateProfileNameUI()
        {
            var createProfileNameUI = UIManager.Instance.Replace<CreateProfileNameUI>(ref currentUI);
            createProfileNameUI.Init(profileName =>
            {
                CreateProfileName(profileName);
            });
        }

        private void SignInWithApple()
        {
            // Do not admit an unauthenticated player while this provider is unimplemented.
            ShowProfileMessage("Apple sign-in is not available yet. Please use another sign-in method.");
        }

        private async void SignInWithGoogle()
        {
            //TODO: [Network]  Replace with real API
            Debug.Log("[Authentication] Sign in with Google");

            bool success = await NetworkAuth.Instance.SigninWithGoogle();
            if (success)
            {
                Debug.Log("[Authentication] Sign in with Google completed successfully.");
                await ContinueAfterSignIn();
            }
            else
            {
                Debug.LogError("[Authentication] Sign in with Google failed.");
                // Handle failure (e.g., show error message to user)
            }

            // SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        public async void SignInAccount(string email, string password)
        {
            Debug.Log($"[Authentication] Sign in for email:{email}");

            bool isSuccess = await NetworkAuth.Instance.SignInWithEmailAndPassword(email, password);
            if (isSuccess)
            {
                await ContinueAfterSignIn();
            }
            else
            {
                Debug.LogError("[Authentication] Sign in failed. Please check your credentials.");
                // TODO: Handle failure (e.g., show error message to user)
                // You might want to show a popup or a message in the UI
                OpenSignInUI();
            }
        }

        public async void CreateProfileName(string profileName)
        {
            if (profileOperationRunning || string.IsNullOrWhiteSpace(profileName)) return;
            profileOperationRunning = true;
            var ui = currentUI as CreateProfileNameUI;
            ui?.SetBusy(true);
            try
            {
                if (!await PlayerDataManager.Instance.UpdateProfileName(profileName.Trim()))
                {
                    ShowProfileMessage("Your profile name could not be saved. Please try again.");
                    return;
                }
                if (this != null) OpenCreateCharacterUI();
            }
            catch (Exception e) { Debug.LogException(e); if (this != null) ShowProfileMessage("Your profile name could not be saved. Please try again."); }
            finally { profileOperationRunning = false; if (ui != null) ui.SetBusy(false); }
        }

        public async void CreateCharacter(CharacterInfoSO character)
        {
            if (profileOperationRunning || character == null || character.GetCharacterPrefab() == null) return;
            profileOperationRunning = true;
            var ui = currentUI as CreateCharacterUI;
            ui?.SetBusy(true);
            try
            {
                if (!await PlayerDataManager.Instance.SelectCharacterAndClearOutfit(character.GetID()))
                {
                    if (this != null) ShowProfileMessage("Your character could not be saved. Please try again.");
                    return;
                }
                if (this != null) SceneManager.LoadScene(SceneList.MainMenu.ToString());
            }
            catch (Exception e) { Debug.LogException(e); if (this != null) ShowProfileMessage("Your character could not be saved. Please try again."); }
            finally { profileOperationRunning = false; if (ui != null) ui.SetBusy(false); }
        }

        private async Task ContinueAfterSignIn()
        {
            if (profileOperationRunning) return;
            profileOperationRunning = true;
            try
            {
                if (!await PlayerDataManager.Instance.EnsureUserDocumentExists())
                    throw new InvalidOperationException("User document could not be prepared.");
                var profile = await PlayerDataManager.Instance.GetProfileData();
                if (this == null) return;
                if (profile == null) throw new InvalidOperationException("Profile could not be read.");
                if (string.IsNullOrWhiteSpace(profile.ProfileName)) OpenCreateProfileNameUI();
                else if (string.IsNullOrWhiteSpace(profile.CharacterId) || !CharacterResourceManager.Instance.GetAllResources().Any(c => c.GetID() == profile.CharacterId && c.GetCharacterPrefab() != null)) OpenCreateCharacterUI();
                else OpenMainMenu();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Authentication] Profile check failed: {e.Message}");
                if (this != null)
                {
                    OpenLoginOrRegisterUI();
                    ShowProfileMessage("Your profile could not be loaded. Please sign in again to retry.");
                }
            }
            finally { profileOperationRunning = false; }
        }

        private void ShowProfileMessage(string message)
        {
            var popup = UIManager.Instance.Create<MessagePopupUI>(currentUI);
            popup.Setup("Profile", message, "OK", () => popup.Close());
        }

        public async void CreateProfileBodyType(int bodyTypeID)
        {
            Debug.Log($"[Authentication] Create profile body type >> body type id:{bodyTypeID}");

            //TODO: [Network]  Replace with real API
            bool isSuccess = await PlayerDataManager.Instance.UpdateProfileBodyType(bodyTypeID);
            // TODO: Handle failure ??
            if (!isSuccess)
            {
                Debug.LogError("[Authentication] Failed to create profile body type.");
                return;
            }

            SceneManager.LoadScene(SceneList.MainMenu.ToString());
            Debug.Log("[Authentication] Create profile body type done!");
        }

        public async void CreateNewAccount(string email, string password, string reEnterPassword, Action onActionValid)
        {
            if (password != reEnterPassword)
            {
                Debug.Log("[Authentication] Password dosn't match!"); //TODO: Maybe Popup UI?
                return;
            }

            onActionValid?.Invoke();
            Debug.Log($"[Authentication] Create new account >> email:{email}");

            var gotResponse = false;

            bool isSuccess = await NetworkAuth.Instance.SignUpWithEmailAndPassword(email, password);
            if (isSuccess)
            {
                Debug.Log("[Authentication] Create new account completed successfully.");
                await ContinueAfterSignIn();
            }
            else
            {
                Debug.LogError("[Authentication] Create new account failed. Please check your credentials.");
                // Handle success (e.g., transition to next UI)
            }
        }

        public async void RecoverAccount(string email)
        {
            Debug.Log($"[Authentication] Recover account >> email:{email}");

            var gotResponse = false;
            var isSuccess = false;
            var defaultTransitionUI = UIManager.Instance.Replace<DefaultTransitionUI>(ref currentUI);
            defaultTransitionUI.Init(
                completeCondition: () => gotResponse,
                onTransitionEnd: () =>
                {
                    if (isSuccess)
                    {
                        UIManager.Instance.Replace<RecoverSubmittedUI>(ref currentUI);
                        Debug.Log("[Authentication] Recover email sent!");
                        return;
                    }

                    Debug.LogError("[Authentication] Failed to send recover email.");
                    OpenRecoverAccountUI();
                });

            isSuccess = await NetworkAuth.Instance.SendPasswordResetEmail(email);
            gotResponse = true;
        }
    }
}
