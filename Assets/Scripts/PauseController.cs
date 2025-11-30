using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject screenBlocker;   // full-screen semi-transparent Image
    public GameObject pauseModal;      // panel that contains pause UI (should have CanvasGroup)
    public float fadeDuration = 0.18f;
    public bool pauseAudio = true;

    CanvasGroup pauseCg;
    bool isPaused = false;
    Coroutine runningFade;

    void Awake()
    {
        if (pauseModal != null)
        {
            pauseCg = pauseModal.GetComponent<CanvasGroup>();
            if (pauseCg == null) pauseCg = pauseModal.AddComponent<CanvasGroup>();

            pauseModal.SetActive(false);
            pauseCg.alpha = 0f;
            pauseCg.interactable = false;
            pauseCg.blocksRaycasts = false;
        }

        if (screenBlocker != null)
            screenBlocker.SetActive(false);
    }

    void Update()
    {
        // // Ignore toggle if an input field (or UI) is selected so typing "p" doesn't pause
        // if (IsTypingInUI()) return;

        // if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        // {
        //     TogglePause();
        // }

        if (GameState.IsUIOpen) 
        return;

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    bool IsTypingInUI()
    {
        // If the EventSystem has a currently selected object and it's an input field,
        // don't toggle pause. This avoids pausing while typing.
        if (EventSystem.current == null) return false;
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;

        // crude check: if it has an InputField or TMP_InputField component
        if (selected.GetComponent<UnityEngine.UI.InputField>() != null) return true;
#if TMP_PRESENT
        if (selected.GetComponent<TMPro.TMP_InputField>() != null) return true;
#endif
        return false;
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        if (screenBlocker != null) screenBlocker.SetActive(true);

        if (runningFade != null) StopCoroutine(runningFade);
        runningFade = StartCoroutine(Fade(true));

        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        if (runningFade != null) StopCoroutine(runningFade);
        runningFade = StartCoroutine(Fade(false));

        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;
    }

    IEnumerator Fade(bool open)
    {
        if (pauseCg == null || pauseModal == null) yield break;

        if (open)
        {
            pauseModal.SetActive(true);
            pauseCg.interactable = true;
            pauseCg.blocksRaycasts = true;
        }

        float start = pauseCg.alpha;
        float end = open ? 1f : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            pauseCg.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }

        pauseCg.alpha = end;

        if (!open)
        {
            pauseCg.interactable = false;
            pauseCg.blocksRaycasts = false;
            pauseModal.SetActive(false);
            if (screenBlocker != null) screenBlocker.SetActive(false);
        }
    }

    // Hook these to your buttons:
    public void OnResumeButton() => ResumeGame();
    public void OnRestartButton() {
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OnMainMenuButton(string mainMenuSceneName) {
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
