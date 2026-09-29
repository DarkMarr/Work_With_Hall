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

        private Task<bool> initializationTask;

        public virtual async void Start()
        {
            await EnsureReadyAsync();
        }

        public Task<bool> EnsureReadyAsync()
        {
            if (initializationTask == null || (initializationTask.IsCompleted && !initializationTask.Result))
                initializationTask = InitializeAsync();
            return initializationTask;
        }

        private async Task<bool> InitializeAsync()
        {
            try
            {
                dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
                if (this == null) return false;
                if (dependencyStatus != Firebase.DependencyStatus.Available)
                    throw new InvalidOperationException("Firebase dependencies: " + dependencyStatus);
                InitializeFirebase();
                return true;
            }
            catch (Exception ex)
            {
                LastAuthErrorMessage = "Sign-in services are not ready. Please try again.";
                Debug.LogError("[NetworkAuth] Initialization failed: " + ex.Message);
                return false;
            }
        }

        protected void InitializeFirebase()
        {
            DebugLog("Setting up Firebase Auth");
            auth = FirebaseConnection.Auth;
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
            if (!await EnsureReadyAsync()) return false;
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
            if (!await EnsureReadyAsync()) return false;
            DebugLog($"Attempting to sign up with email: {email}");
            LastAuthErrorMessage = null;
            LastAuthErrorCode = null;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                LastAuthErrorMessage = "Please enter both email and password.";
                Debug.LogError("Email or password is null or empty");
                OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                return false;
            }

            try
            {
                var authResult = await WithTimeout(auth.CreateUserWithEmailAndPasswordAsync(email, password));
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
                // Desktop/Editor builds report a duplicate email as a generic failure (AuthError 1), so keep the player's next step visible.
                if (firebaseEx.ErrorCode == (int)Firebase.Auth.AuthError.Failure)
                    LastAuthErrorMessage = "Account could not be created. If this email is already registered, please sign in or reset your password.";
                return false;
            }
            catch (TimeoutException)
            {
                HandleTimeout("sign-up");
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
            if (!await EnsureReadyAsync()) return false;
            DebugLog($"Attempting to sign in to account with email: {email}");
            LastAuthErrorMessage = null;
            LastAuthErrorCode = null;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                LastAuthErrorMessage = "Please enter both email and password.";
                Debug.LogError("Email or password is null or empty");
                OnSignInResult?.Invoke(SignInResult.InvalidCredential);
                return false;
            }

            try
            {
                var authResult = await WithTimeout(auth.SignInWithEmailAndPasswordAsync(email, password));
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
            catch (TimeoutException)
            {
                HandleTimeout("email sign-in");
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
            if (!await EnsureReadyAsync()) return false;
            DebugLog($"Sending password reset email to: {email}");
            LastAuthErrorMessage = null;
            LastAuthErrorCode = null;

            if (!IsValidEmail(email))
            {
                LastAuthErrorMessage = "Please enter a valid email address.";
                Debug.LogError("Email is null, empty or malformed");
                return false;
            }

            try
            {
                await WithTimeout(auth.SendPasswordResetEmailAsync(email.Trim()));
                Debug.Log($"Password reset email sent to: {email}");
                return true;
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                LastAuthErrorCode = firebaseEx.ErrorCode;
                LastAuthErrorMessage = firebaseEx.ErrorCode == (int)Firebase.Auth.AuthError.UserNotFound
                    ? "No account was found for this email."
                    : GetPlayerMessage(firebaseEx.ErrorCode);
                Debug.LogError($"Failed to send password reset email: {firebaseEx.ErrorCode} - {firebaseEx.Message}");
                return false;
            }
            catch (TimeoutException)
            {
                HandleTimeout("password reset");
                return false;
            }
            catch (Exception ex)
            {
                LastAuthErrorMessage = "Something went wrong. Please try again.";
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

        public async Task<bool> DeleteAccountAsync()
        {
            if (auth?.CurrentUser == null)
            {
                Debug.LogError("No user is currently signed in");
                return false;
            }

            var currentUser = auth.CurrentUser;
            DebugLog($"Deleting Firebase Authentication account: {currentUser.UserId}");

            try
            {
                await currentUser.DeleteAsync();
                DebugLog("Firebase Authentication account deleted successfully");
                return true;
            }
            catch (Firebase.FirebaseException firebaseEx)
            {
                Debug.LogError($"Failed to delete Firebase Authentication account: {firebaseEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error deleting Firebase Authentication account: {ex.Message}");
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

        /// <summary>
        /// Player-facing text for the most recent failed auth request (null when the last request succeeded or failed without a Firebase error).
        /// </summary>
        public string LastAuthErrorMessage { get; private set; }

        /// <summary>
        /// Firebase AuthError code of the most recent failed auth request (null when it did not fail with a Firebase error).
        /// </summary>
        public int? LastAuthErrorCode { get; private set; }

        public bool LastErrorWasEmailInUse =>
            LastAuthErrorCode == (int)Firebase.Auth.AuthError.EmailAlreadyInUse ||
            LastAuthErrorCode == (int)Firebase.Auth.AuthError.AccountExistsWithDifferentCredentials ||
            LastAuthErrorCode == (int)Firebase.Auth.AuthError.Failure; // Editor/desktop reports a duplicate email as a generic failure.

        public bool LastErrorWasWrongCredential =>
            LastAuthErrorCode == (int)Firebase.Auth.AuthError.WrongPassword ||
            LastAuthErrorCode == (int)Firebase.Auth.AuthError.InvalidCredential ||
            LastAuthErrorCode == (int)Firebase.Auth.AuthError.UserNotFound;

        private const int AuthRequestTimeoutMs = 30000;

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            email = email.Trim();
            int at = email.IndexOf('@');
            return at > 0 && at == email.LastIndexOf('@') && email.IndexOf('.', at) > at + 1 && !email.EndsWith(".") && !email.Contains(" ");
        }

        // Firebase requests can stall on a bad connection; fail after a fixed wait so the UI never waits forever.
        private static async Task<T> WithTimeout<T>(Task<T> task)
        {
            if (await Task.WhenAny(task, Task.Delay(AuthRequestTimeoutMs)) != task)
            {
                _ = task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                throw new TimeoutException();
            }
            return await task;
        }

        private static async Task WithTimeout(Task task)
        {
            if (await Task.WhenAny(task, Task.Delay(AuthRequestTimeoutMs)) != task)
            {
                _ = task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                throw new TimeoutException();
            }
            await task;
        }

        private void HandleTimeout(string operation)
        {
            LastAuthErrorCode = null;
            LastAuthErrorMessage = "The server is taking too long to respond. Please check your connection and try again.";
            Debug.LogError($"[NetworkAuth] {operation} timed out after {AuthRequestTimeoutMs / 1000}s");
            OnSignInResult?.Invoke(SignInResult.NetworkError);
        }

        private static string GetPlayerMessage(int errorCode)
        {
            switch (errorCode)
            {
                case (int)Firebase.Auth.AuthError.EmailAlreadyInUse:
                case (int)Firebase.Auth.AuthError.AccountExistsWithDifferentCredentials:
                    return "This email is already registered. Please sign in instead.";
                case (int)Firebase.Auth.AuthError.WeakPassword:
                    return "Password is too weak. Use at least 6 characters.";
                case (int)Firebase.Auth.AuthError.InvalidEmail:
                    return "Please enter a valid email address.";
                case (int)Firebase.Auth.AuthError.WrongPassword:
                case (int)Firebase.Auth.AuthError.UserNotFound:
                case (int)Firebase.Auth.AuthError.InvalidCredential:
                    return "Email or password is incorrect.";
                case (int)Firebase.Auth.AuthError.NetworkRequestFailed:
                    return "Network error. Please check your connection and try again.";
                case (int)Firebase.Auth.AuthError.UserDisabled:
                    return "This account has been disabled.";
                case (int)Firebase.Auth.AuthError.TooManyRequests:
                    return "Too many attempts. Please try again later.";
                default:
                    return "Something went wrong. Please try again.";
            }
        }

        private void HandleFirebaseError(Firebase.FirebaseException firebaseEx)
        {
            LastAuthErrorCode = firebaseEx.ErrorCode;
            LastAuthErrorMessage = GetPlayerMessage(firebaseEx.ErrorCode);
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
