using System.Threading.Tasks;
using QuizGame.Gameplay.QuizManagement;
using QuizGame.Store;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace QuizGame.Scene
{
    public class InitSceneController : MonoBehaviour
    {
        // Store connectivity must not prevent players from reaching authentication.
        private static async Task InitializePurchasesAsync()
        {
            try { await IAPManager.Instance.InitAsync(); }
            catch (System.Exception ex) { Debug.LogWarning("[Init] Purchases unavailable: " + ex.Message); }
        }

        private async void Start()
        {
            EnhancedTouchSupport.Enable();
            var localizationTask = LocalizationSettings.InitializationOperation.Task;
            _ = InitializePurchasesAsync();
            var QuizCollectionsTask = QuizCollections.Initialize();

            await Task.WhenAll(localizationTask, QuizCollectionsTask);
            if (this == null) return;
            SceneManager.LoadScene(SceneList.Authentication.ToString());
        }
    }
}
