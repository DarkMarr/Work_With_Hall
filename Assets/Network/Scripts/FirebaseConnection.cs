using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

namespace QuizGame.Network
{
    // One connection choice for authentication and player data. Builds always use Firebase defaults.
    public static class FirebaseConnection
    {
        public static bool IsUsingEmulator
        {
            get
            {
#if UNITY_EDITOR
                return UseLocalEmulator;
#else
                return false;
#endif
            }
        }

        // A stopped local service must not look like a bad password or a production outage.
        public static async System.Threading.Tasks.Task CheckLocalServiceAsync(bool firestore = false)
        {
#if UNITY_EDITOR
            if (!UseLocalEmulator) return;
            int port = firestore ? 8085 : 9099;
            using (var client = new System.Net.Sockets.TcpClient())
            {
                try
                {
                    var connect = client.ConnectAsync("127.0.0.1", port);
                    if (await System.Threading.Tasks.Task.WhenAny(connect, System.Threading.Tasks.Task.Delay(1500)) != connect)
                    {
                        _ = connect.ContinueWith(t => _ = t.Exception, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                        throw new System.TimeoutException();
                    }
                    await connect;
                }
                catch (System.Exception)
                {
                    throw new System.InvalidOperationException("Local test mode: Firebase " + (firestore ? "Firestore" : "Auth") +
                        " Emulator is not running on port " + port + ". Start the HALL900 test emulators and retry. Local test accounts are separate from real accounts.");
                }
            }
#else
            await System.Threading.Tasks.Task.CompletedTask;
#endif
        }

        public static FirebaseAuth Auth => FirebaseAuth.GetAuth(App);
        public static FirebaseFirestore Firestore
        {
            get
            {
                var db = FirebaseFirestore.GetInstance(App);
#if UNITY_EDITOR
                if (UseLocalEmulator && !configured)
                {
                    db.Settings.Host = "127.0.0.1:8085";
                    db.Settings.SslEnabled = false;
                    db.Settings.PersistenceEnabled = false;
                    configured = true;
                }
#endif
                return db;
            }
        }

        private static FirebaseApp App
        {
            get
            {
#if UNITY_EDITOR
                if (UseLocalEmulator)
                {
                    ConfigureEnvironment();
                    return FirebaseApp.GetInstance("HALL900-LocalTest") ?? FirebaseApp.Create(
                        new AppOptions { ProjectId = "demo-hall900-audit", AppId = "1:1234567890:android:abcdef0123456789", ApiKey = "fake-api-key" },
                        "HALL900-LocalTest");
                }
#endif
                return FirebaseApp.DefaultInstance;
            }
        }

#if UNITY_EDITOR
        private static bool configured;
        private static string PreferenceKey => "HALL900.LocalFirebase." + UnityEngine.Application.dataPath;
        public static bool UseLocalEmulator => UnityEditor.EditorPrefs.GetBool(PreferenceKey, false);
        private const string Menu = "HALL900/Testing/Use Local Firebase Emulator";

        [UnityEditor.MenuItem(Menu)]
        public static void ToggleLocalEmulator()
        {
            UnityEditor.EditorPrefs.SetBool(PreferenceKey, !UseLocalEmulator);
            ConfigureEnvironment();
            UnityEngine.Debug.Log(UseLocalEmulator
                ? "[HALL900 Test] Local Firebase enabled. Start the local emulators before Play. Auth 9099 / Firestore 8085; no production fallback."
                : "[HALL900 Test] Local Firebase disabled. Next Play uses the normal Firebase configuration.");
        }

        [UnityEditor.MenuItem(Menu, true)]
        private static bool ValidateToggle()
        {
            UnityEditor.Menu.SetChecked(Menu, UseLocalEmulator);
            return !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
        }

        [UnityEditor.InitializeOnLoadMethod]
        private static void ConfigureEnvironment()
        {
            var names = new[] { "USE_AUTH_EMULATOR", "FIREBASE_AUTH_EMULATOR_HOST", "AUTH_EMULATOR_PORT" };
            var values = new[] { "yes", "127.0.0.1:9099", "9099" };
            const string owned = "HALL900.LocalFirebase.EnvironmentOwned";
            if (!UseLocalEmulator && !UnityEditor.SessionState.GetBool(owned, false)) return;
            for (int i = 0; i < names.Length; i++)
            {
                string key = owned + "." + names[i];
                if (UseLocalEmulator && !UnityEditor.SessionState.GetBool(owned, false))
                    UnityEditor.SessionState.SetString(key, System.Environment.GetEnvironmentVariable(names[i]) ?? "");
                string value = UseLocalEmulator ? values[i] : UnityEditor.SessionState.GetString(key, "");
                System.Environment.SetEnvironmentVariable(names[i], string.IsNullOrEmpty(value) ? null : value);
#if UNITY_EDITOR_WIN
                SetUcrt(names[i], value);
                SetMsvcrt(names[i], value);
#endif
            }
            UnityEditor.SessionState.SetBool(owned, UseLocalEmulator);
        }

#if UNITY_EDITOR_WIN
        [System.Runtime.InteropServices.DllImport("ucrtbase.dll", EntryPoint = "_putenv_s", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int SetUcrt(string name, string value);
        [System.Runtime.InteropServices.DllImport("msvcrt.dll", EntryPoint = "_putenv_s", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int SetMsvcrt(string name, string value);
#endif
#endif
    }
}
