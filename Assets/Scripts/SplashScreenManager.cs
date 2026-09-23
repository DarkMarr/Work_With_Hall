using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider loadingBar;
    public Text loadingText;

    [Header("Settings")]
    [SerializeField] private float loadSpeed = 0.5f;
    [SerializeField] private string nextSceneName = "Init";

    private float currentProgress = 0f;

    void Start()
    {
        if (loadingBar == null || loadingText == null)
        {
            Debug.LogError("SplashScreenManager: Please assign the Slider and Text components in the Inspector.");
            return;
        }

        StartCoroutine(LoadProgressRoutine());
    }

    IEnumerator LoadProgressRoutine()
    {
        while (currentProgress < 1f)
        {
            currentProgress += Time.deltaTime * loadSpeed;

            if (currentProgress > 1f) currentProgress = 1f;

            if (loadingBar != null)
                loadingBar.value = currentProgress;

            if (loadingText != null)
                loadingText.text = $"Now Loading... {(currentProgress * 100):F0}%";

            yield return null;
        }

        if (loadingBar != null)
            loadingBar.value = 1f;

        if (loadingText != null)
            loadingText.text = "Loading Complete...";

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(nextSceneName);
    }
}