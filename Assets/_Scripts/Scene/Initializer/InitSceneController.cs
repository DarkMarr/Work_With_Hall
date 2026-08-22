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
        private async void Start()
        {
            EnhancedTouchSupport.Enable();
            var localizationTask = LocalizationSettings.InitializationOperation.Task;
            var iAPTask = IAPManager.Instance.InitAsync();
            var QuizCollectionsTask = QuizCollections.Initialize();

            await Task.WhenAll(localizationTask, iAPTask, QuizCollectionsTask);
            SceneManager.LoadScene(SceneList.Authentication.ToString());
        }
    }
}
