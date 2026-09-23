using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SplashScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider loadingBar;
    public TMP_Text loadingText;

    [Header("Settings")]
    [SerializeField] private float loadSpeed = 0.5f;
    [SerializeField] private string nextSceneName = "Init";

    private float currentProgress = 0f;

    void Start()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"SplashScreenManager: Scene '{nextSceneName}' is not in Build Settings.");
            return;
        }

        if (loadingBar != null)
        {
            loadingBar.minValue = 0f;
            loadingBar.maxValue = 1f;
            loadingBar.value = 0f;
            loadingBar.interactable = false;
        }

        StartCoroutine(LoadProgressRoutine());
    }

    IEnumerator LoadProgressRoutine()
    {
        // Load the next scene in the background; the bar can't finish before it's ready.
        var operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;

        while (currentProgress < 1f)
        {
            // AsyncOperation.progress stops at 0.9 until activation is allowed.
            float loadedProgress = Mathf.Clamp01(operation.progress / 0.9f);
            currentProgress = Mathf.Min(currentProgress + Time.deltaTime * loadSpeed, loadedProgress);

            if (loadingBar != null)
                loadingBar.value = currentProgress;

            if (loadingText != null)
                loadingText.text = $"Now Loading... {(currentProgress * 100):F0}%";

            yield return null;
        }

        if (loadingText != null)
            loadingText.text = "Loading Complete...";

        yield return new WaitForSeconds(0.5f);

        operation.allowSceneActivation = true;
    }
}
