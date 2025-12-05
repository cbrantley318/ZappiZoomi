using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button Tutorial1Button;
    public Button Tutorial2Button;
    public Button Tutorial3Button;


    void Start()
    {
        if (level1Button == null || level2Button == null)
        {
            Debug.LogError("[LevelSelectionUI] Please assign level1Button and level2Button in the Inspector.");
            return;
        }

        level1Button.onClick.RemoveAllListeners();
        level2Button.onClick.RemoveAllListeners();
        level3Button.onClick.RemoveAllListeners();
        Tutorial1Button.onClick.RemoveAllListeners();
        Tutorial2Button.onClick.RemoveAllListeners();
        Tutorial3Button.onClick.RemoveAllListeners();
        

        level1Button.onClick.AddListener(() => LoadLevel("Level1"));
        level2Button.onClick.AddListener(() => LoadLevel("Level2"));
        level3Button.onClick.AddListener(() => LoadLevel("Level3"));
        Tutorial1Button.onClick.AddListener(() => LoadLevel("Tutorial1"));
        Tutorial2Button.onClick.AddListener(() => LoadLevel("Tutorial2"));
        Tutorial3Button.onClick.AddListener(() => LoadLevel("Tutorial3"));

    }

    void LoadLevel(string sceneName)
    {
        // hide welcome UI if manager exists
        if (PersistentUIManager.Instance != null)
            PersistentUIManager.Instance.HideWelcome();

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
