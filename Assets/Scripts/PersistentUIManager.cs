using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// small global flag you can optionally check in player scripts
public static class GameState
{
    public static bool IsUIOpen = false;
}

public class PersistentUIManager : MonoBehaviour
{
    public static PersistentUIManager Instance;

    [Header("Windows")]
    public GameObject welcomeScreen;
    public GameObject screenBlocker;
    public GameObject pauseModal;
    public GameObject gameOverModal;

    [Header("Player input to disable")]
    [Tooltip("Drag movement, camera, shooting scripts here (any MonoBehaviour that reads input).")]
    public MonoBehaviour[] playerMovementScripts;

    #if ENABLE_INPUT_SYSTEM
    [Tooltip("If you use the new Input System, drag your PlayerInput component here (optional).")]
    public PlayerInput playerInput;
    #endif

    [Tooltip("If true, welcome will show automatically at Start via ShowWelcome()")]
    public bool showWelcomeOnStart = true;

    [Header("Game Over / Death (UI only)")]
    public AudioSource deathAudioSource;
    [Tooltip("Optional particle system to trigger on death (optional).")]
    public ParticleSystem deathParticle;
    [Tooltip("How long (seconds, unscaled) to wait before hiding the panel. If <= 0 and deathAudioSource.clip exists, the clip length will be used.")]
    public float gameOverDisplayDelay = 0f; // 0 = use audio clip length when available


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Ensure consistent initial state
        if (welcomeScreen != null) welcomeScreen.SetActive(false);
        if (pauseModal != null) pauseModal.SetActive(false);
        if (screenBlocker != null) screenBlocker.SetActive(false);
        if (gameOverModal != null) gameOverModal.SetActive(false);
        GameState.IsUIOpen = false;
    }

    void Start()
    {
        // Use the manager to show welcome so it blocks properly
        if (showWelcomeOnStart && welcomeScreen != null)
            ShowWelcome();
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // restore input and time on scene load to avoid stuck states
        SetPlayerScriptsEnabled(true);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = true;
        #endif
        GameState.IsUIOpen = false;
        Time.timeScale = 1f;
    }

    // -------------------------
    // Public helpers (use these)
    // -------------------------


    /// <summary>
    /// Show the Game Over panel, play audio/particle, then hide and restore input.
    /// This does NOT restart or reload any scene.
    /// </summary>
    public void ShowGameOverPanel()
    {
        // show modal and block input
        if (screenBlocker != null) screenBlocker.SetActive(true);
        if (gameOverModal != null) gameOverModal.SetActive(true);

        GameState.IsUIOpen = true;
        SetPlayerScriptsEnabled(false);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = false;
        #endif

        // Ensure normal time so audio plays at expected speed
        Time.timeScale = 1f;

        // Play particle if assigned
        if (deathParticle != null)
            deathParticle.Play();

        // Play audio if available
        float wait = gameOverDisplayDelay;
        if (deathAudioSource != null && deathAudioSource.clip != null)
        {
            deathAudioSource.Play();
            if (gameOverDisplayDelay <= 0f)
                wait = deathAudioSource.clip.length;
        }

        // if nothing set, use a small default so player sees the panel
        if (wait <= 0f) wait = 0.8f;

        StartCoroutine(HideGameOverPanelAfterDelayUnscaled(wait));
    }

    /// <summary>
    /// Hides the Game Over panel after waiting (unscaled time), and restores input/time.
    /// </summary>
    IEnumerator HideGameOverPanelAfterDelayUnscaled(float delayUnscaled)
    {
        float t = 0f;
        while (t < delayUnscaled)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // hide UI
        if (gameOverModal != null) gameOverModal.SetActive(false);
        if (!IsAnyModalOpen() && screenBlocker != null) screenBlocker.SetActive(false);

        // stop particle if desired (optional)
        if (deathParticle != null) deathParticle.Stop();

        // restore inputs
        SetPlayerScriptsEnabled(true);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = true;
        #endif

        GameState.IsUIOpen = false;
    }

    /// <summary>Show the welcome modal (blocks input and pauses the game)</summary>
    public void ShowWelcome()
    {
        if (screenBlocker != null) screenBlocker.SetActive(true);
        if (welcomeScreen != null) welcomeScreen.SetActive(true);

        // mark UI open and disable player input
        GameState.IsUIOpen = true;
        SetPlayerScriptsEnabled(false);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = false;
        #endif

        // match Pause behavior: pause time
        Time.timeScale = 0f;
    }

    /// <summary>Hide the welcome modal and restore input/time</summary>
    public void HideWelcome()
    {
        if (welcomeScreen != null) welcomeScreen.SetActive(false);

        // only hide blocker if no other modal is open
        if (!IsAnyModalOpen() && screenBlocker != null) screenBlocker.SetActive(false);

        SetPlayerScriptsEnabled(true);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = true;
        #endif

        GameState.IsUIOpen = false;

        // restore time
        Time.timeScale = 1f;
    }
    [Header("Level Won")]
public GameObject levelWonModal;

// Call to show level-won UI. You can pass optional scene names/index if needed.
public void ShowLevelWon()
{
    if (screenBlocker != null) screenBlocker.SetActive(true);
    if (levelWonModal != null) levelWonModal.SetActive(true);

    GameState.IsUIOpen = true;
    SetPlayerScriptsEnabled(false);
    #if ENABLE_INPUT_SYSTEM
    if (playerInput != null) playerInput.enabled = false;
    #endif

    Time.timeScale = 0f;
}

// Hide LevelWon modal (restore input/time)
public void HideLevelWon()
{
    if (levelWonModal != null) levelWonModal.SetActive(false);
    if (!IsAnyModalOpen() && screenBlocker != null) screenBlocker.SetActive(false);

    SetPlayerScriptsEnabled(true);
    #if ENABLE_INPUT_SYSTEM
    if (playerInput != null) playerInput.enabled = true;
    #endif

    GameState.IsUIOpen = false;
    Time.timeScale = 1f;
}


    /// <summary>Open pause modal (blocks input and pauses the game)</summary>
    public void PauseGame()
    {
        if (screenBlocker != null) screenBlocker.SetActive(true);
        if (pauseModal != null) pauseModal.SetActive(true);

        GameState.IsUIOpen = true;
        SetPlayerScriptsEnabled(false);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = false;
        #endif

        Time.timeScale = 0f;
    }

    /// <summary>Resume from pause (restore input and time)</summary>
    public void ResumeGame()
    {
        if (pauseModal != null) pauseModal.SetActive(false);
        if (!IsAnyModalOpen() && screenBlocker != null) screenBlocker.SetActive(false);

        SetPlayerScriptsEnabled(true);
        #if ENABLE_INPUT_SYSTEM
        if (playerInput != null) playerInput.enabled = true;
        #endif

        GameState.IsUIOpen = false;
        Time.timeScale = 1f;
    }

    // -------------------------
    // Internal helpers
    // -------------------------

    bool IsAnyModalOpen()
    {
        if (welcomeScreen != null && welcomeScreen.activeSelf) return true;
        if (pauseModal != null && pauseModal.activeSelf) return true;
        if (levelWonModal != null && levelWonModal.activeSelf) return true;
        if (gameOverModal != null && gameOverModal.activeSelf) return true;
        return false;
    }

    void SetPlayerScriptsEnabled(bool enabled)
    {
        if (playerMovementScripts == null) return;
        for (int i = 0; i < playerMovementScripts.Length; i++)
        {
            var s = playerMovementScripts[i];
            if (s != null) s.enabled = enabled;
        }
    }

    
}
