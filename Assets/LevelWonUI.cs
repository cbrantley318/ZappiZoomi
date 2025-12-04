using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelWonUI : MonoBehaviour
{
    [Header("Optional: override next scene by name")]
    public string nextSceneName = "";

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        if (PersistentUIManager.Instance != null)
            PersistentUIManager.Instance.HideLevelWon();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnHomeButton()
    {
        Time.timeScale = 1f;

        if (PersistentUIManager.Instance != null)
        {
            PersistentUIManager.Instance.HideLevelWon();
            PersistentUIManager.Instance.ShowWelcome();   // show level selection
        }
        else
        {
            // fallback
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                SceneManager.LoadScene("MainMenu");
        }
    }

    public void OnNextButton()
    {
        Time.timeScale = 1f;
        if (PersistentUIManager.Instance != null)
            PersistentUIManager.Instance.HideLevelWon();

        // Use explicit nextSceneName if set
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }
            else
            {
                Debug.LogError($"Next scene '{nextSceneName}' not found in Build Settings!");
            }
        }

        // otherwise go to next build index
        int current = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = current + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("No next level in Build Settings → returning to Welcome.");
            PersistentUIManager.Instance?.ShowWelcome();
        }
    }
}
