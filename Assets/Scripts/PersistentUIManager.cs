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

    [Header("Player input to disable")]
    [Tooltip("Drag movement, camera, shooting scripts here (any MonoBehaviour that reads input).")]
    public MonoBehaviour[] playerMovementScripts;

    #if ENABLE_INPUT_SYSTEM
    [Tooltip("If you use the new Input System, drag your PlayerInput component here (optional).")]
    public PlayerInput playerInput;
    #endif

    [Tooltip("If true, welcome will show automatically at Start via ShowWelcome()")]
    public bool showWelcomeOnStart = true;

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
