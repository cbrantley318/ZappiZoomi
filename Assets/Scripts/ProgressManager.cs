using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    private const string EPISODE1_COMPLETE = "Episode1Complete";

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
        }
    }

    public void CompleteEpisode1()
    {
        PlayerPrefs.SetInt(EPISODE1_COMPLETE, 1);
        PlayerPrefs.Save();
    }

    public bool IsEpisode1Complete()
    {
        return PlayerPrefs.GetInt(EPISODE1_COMPLETE, 0) == 1;
    }
}
