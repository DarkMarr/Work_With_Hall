namespace QuizGame.Network
{
    using Firebase.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using Firebase.Auth;
    using Google;
    using QuizGame.Utilities;

    public enum SignInResult
    {
        Success,
        Cancelled,
        NetworkError,
        InvalidCredential,
        UserDisabled,
        TooManyRequests,
        UnknownError
    }

    public class NetworkAuth : MonoSingleton<NetworkAuth>
    {
        // Events for sign-in status
        public static event System.Action<SignInResult> OnSignInResult;
        public static event System.Action<string> OnSignInError;

        [Header("Google Sign-In")]
        [SerializeField]
        [Tooltip("Get this from your google-services.json file -> client -> oauth_client -> client_id (type 3)")]
        protected string webClientId = "164107989714-o2nnv7eudhigmtjp6rjmg92p6dri304m.apps.googleusercontent.com";
        protected Firebase.Auth.FirebaseAuth auth;
        protected Firebase.Auth.FirebaseUser user;

        Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

        // When the app starts, check to make sure that we have
        // the required dependencies to use Firebase, and if not,
        // add them if possible.
        public virtual void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                      "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }

        protected void InitializeFirebase()
        {
            DebugLog("Setting up Firebase Auth");
            auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            auth.StateChanged += AuthStateChanged;
            auth.IdTokenChanged += IdTokenChanged;
            // Specify valid options to construct a secondary authentication object.

            AuthStateChanged(this, null);
        }

        private void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (auth.CurrentUser != null)
            {
                DebugLog($"User already signed in: {auth.CurrentUser.DisplayName}");
                DebugLog($"Current User: {auth.CurrentUser.UserId}");
            }
            else
            {
                DebugLog("No user is signed in.");
            }
        }

        private void IdTokenChanged(object sender, EventArgs eventArgs)
        {
            // Handle ID token changes if needed.
        }

        private void DebugLog(string message)
        {
            Debug.Log($"[NetworkAuth] {message}");
        }
        
        public bool IsAlreadySignedIn()
        {
            return auth != null && auth.CurrentUser != null;
        }
        
        public async Task<bool> SigninWithGoogle()
        {
            DebugLog("Calling SigninWithGoogle");

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true
            };
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestEmail = true;

            try
            {
                GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

                if (googleUser == null)
                {
                    Debug.LogError("Google Sign-In returned null user");
                    return false;
                }

                Debug.Log($"Google sign-in successful: {googleUser.DisplayName} ({googleUser.UserId})");
                return await SignInToFirebaseWithGoogle(googleUser.IdToken);
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("Google Sign-In was cancelled by user");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error during Google Sign-In: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }
        
        private async Task<bool> SignInToFirebaseWithGoogle(string idToken)
        {
            Debug.Log("Attempting to sign in to Firebase with Google credential...");
            
            if (string.IsNullOrEmpty(idToken))
            {
                Debug.LogError("ID Token is null or empty");
                return false;
            }
            
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

            try
            {
                FirebaseUser newUser = await auth.SignInWithCredentialAsync(credential);
                
                if (newUser != null)
                {
                    Debug.Log($"Firebase sign-in successful: {newUser.DisplayName} ({newUser.UserId})");
                    OnSignInResult?.Invoke(SignInResult.Success);
                    return true;
                }
                else
                {
                    Debug.LogError("Firebase sign-in returned null user");
                    return false;
                }
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                HandleFirebaseError(firebaseEx);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error during Firebase sign-in: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }

        public void SignOutFromGoogle()
        {
            DebugLog("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
        }

        public void DisconnectFromGoogle()
        {
            DebugLog("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        public async Task<bool> SignUpWithEmailAndPassword(string email, string password)
        {
            DebugLog($"Attempting to sign up with email: {email}");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("Email or password is null or empty");
                OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                return false;
            }

            try
            {
                var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                FirebaseUser newUser = authResult?.User;
                
                if (newUser != null)
                {
                    Debug.Log($"Firebase sign-up successful: {newUser.Email} ({newUser.UserId})");
                    OnSignInResult?.Invoke(SignInResult.Success);
                    return true;
                }
                else
                {
                    Debug.LogError("Firebase sign-up returned null user");
                    OnSignInResult?.Invoke(SignInResult.UnknownError);
                    return false;
                }
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                HandleFirebaseError(firebaseEx);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error during Firebase sign-up: {ex.Message}");
                Debug.LogException(ex);
                OnSignInResult?.Invoke(SignInResult.UnknownError);
                return false;
            }
        }

        public async Task<bool> SignInWithEmailAndPassword(string email, string password)
        {
            DebugLog($"Attempting to sign in with email: {email}");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("Email or password is null or empty");
                OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                return false;
            }

            try
            {
                var authResult = await auth.SignInWithEmailAndPasswordAsync(email, password);
                FirebaseUser newUser = authResult?.User;
                
                if (newUser != null)
                {
                    Debug.Log($"Firebase email sign-in successful: {newUser.Email} ({newUser.UserId})");
                    OnSignInResult?.Invoke(SignInResult.Success);
                    return true;
                }
                else
                {
                    Debug.LogError("Firebase email sign-in returned null user");
                    OnSignInResult?.Invoke(SignInResult.UnknownError);
                    return false;
                }
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                HandleFirebaseError(firebaseEx);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error during Firebase email sign-in: {ex.Message}");
                Debug.LogException(ex);
                OnSignInResult?.Invoke(SignInResult.UnknownError);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmail(string email)
        {
            DebugLog($"Sending password reset email to: {email}");

            if (string.IsNullOrEmpty(email))
            {
                Debug.LogError("Email is null or empty");
                return false;
            }

            try
            {
                await auth.SendPasswordResetEmailAsync(email);
                Debug.Log($"Password reset email sent to: {email}");
                return true;
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                Debug.LogError($"Failed to send password reset email: {firebaseEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error sending password reset email: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }

        public async Task<bool> UpdateUserEmail(string newEmail)
        {
            if (auth.CurrentUser == null)
            {
                Debug.LogError("No user is currently signed in");
                return false;
            }

            DebugLog($"Updating user email to: {newEmail}");

            try
            {
                await auth.CurrentUser.SendEmailVerificationBeforeUpdatingEmailAsync(newEmail);
                Debug.Log($"Email verification sent before updating email to: {newEmail}");
                return true;
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                Debug.LogError($"Failed to update email: {firebaseEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error updating email: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }

        public async Task<bool> UpdateUserPassword(string newPassword)
        {
            if (auth.CurrentUser == null)
            {
                Debug.LogError("No user is currently signed in");
                return false;
            }

            DebugLog("Updating user password");

            try
            {
                await auth.CurrentUser.UpdatePasswordAsync(newPassword);
                Debug.Log("Password updated successfully");
                return true;
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                Debug.LogError($"Failed to update password: {firebaseEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error updating password: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }

        public void SignOut()
        {
            DebugLog("Signing out from Firebase");
            auth?.SignOut();
        }

        public bool IsUserSignedIn()
        {
            return auth != null && auth.CurrentUser != null;
        }

        public string GetCurrentUserEmail()
        {
            if (auth?.CurrentUser != null)
            {
                return auth.CurrentUser.Email;
            }
            return null;
        }

        public string GetCurrentUserId()
        {
            if (auth?.CurrentUser != null)
            {
                return auth.CurrentUser.UserId;
            }
            return null;
        }

        private void HandleFirebaseError(Firebase.FirebaseException firebaseEx)
        {
            switch (firebaseEx.ErrorCode)
            {
                case (int)Firebase.Auth.AuthError.InvalidCredential:
                case (int)Firebase.Auth.AuthError.WrongPassword:
                case (int)Firebase.Auth.AuthError.InvalidEmail:
                    Debug.LogError("Invalid Firebase credential or email/password");
                    OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                    break;
                case (int)Firebase.Auth.AuthError.NetworkRequestFailed:
                    Debug.LogError("Network request failed during Firebase authentication");
                    OnSignInResult?.Invoke(SignInResult.NetworkError);
                    break;
                case (int)Firebase.Auth.AuthError.AccountExistsWithDifferentCredentials:
                case (int)Firebase.Auth.AuthError.EmailAlreadyInUse:
                    Debug.LogError("Account exists with different credentials or email already in use");
                    OnSignInResult?.Invoke(SignInResult.UnknownError);
                    break;
                case (int)Firebase.Auth.AuthError.UserDisabled:
                    Debug.LogError("User account has been disabled");
                    OnSignInResult?.Invoke(SignInResult.UserDisabled);
                    break;
                case (int)Firebase.Auth.AuthError.TooManyRequests:
                    Debug.LogError("Too many requests. Please try again later");
                    OnSignInResult?.Invoke(SignInResult.TooManyRequests);
                    break;
                case (int)Firebase.Auth.AuthError.UserNotFound:
                    Debug.LogError("User not found");
                    OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                    break;
                case (int)Firebase.Auth.AuthError.WeakPassword:
                    Debug.LogError("Password is too weak");
                    OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                    break;
                default:
                    Debug.LogError($"Firebase error: {firebaseEx.ErrorCode} - {firebaseEx.Message}");
                    OnSignInResult?.Invoke(SignInResult.UnknownError);
                    break;
            }
        }
    }
}
