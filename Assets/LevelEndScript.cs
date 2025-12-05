using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LevelEndScript : MonoBehaviour
{
    [SerializeField] Sprite onLight;
    [SerializeField] Sprite offLight;
    [SerializeField] float levelCompleteDelay;

    private PowerTermScript myWireTerminal;
    private bool levelCompleted = false;
    private Light2D myLight;
    private SpriteRenderer mySpriteRenderer;
    

    // Start is called before the first frame update
    void Start()
    {
        myLight = GetComponent<Light2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myWireTerminal = GetComponentInChildren<PowerTermScript>();
        myLight.enabled = false;
        mySpriteRenderer.sprite = offLight;

    }

    // Update is called once per frame
    void Update()
    {
        if (!levelCompleted && myWireTerminal.poweredOn)   //turning this on is the end of the level (for now)
        {
            levelCompleted = true;
            OnLevelComplete();

            mySpriteRenderer.sprite = onLight;  //turn on the light
            myLight.enabled = true;
            GetComponent<AudioSource>().Play(); //play a little ding
        }
    }


    void OnLevelComplete()
    {
        // Optionally disable player movement immediately so nothing else happens
        // You can disable this GameObject or set a flag — here we disable this script  //what??
        //this.enabled = false;

        // Optionally play a sfx or animation here, then show modal after delay
        if (levelCompleteDelay > 0f)
            Invoke(nameof(ShowLevelWonModal), levelCompleteDelay);
        else
            ShowLevelWonModal();
    }

    void ShowLevelWonModal()
    {
        // Prefer using your PersistentUIManager so the modal blocks input consistently
        if (PersistentUIManager.Instance != null)
        {
            PersistentUIManager.Instance.ShowLevelWon();
        }
        else
        {
            // Fallback: pause the game and log
            Time.timeScale = 0f;
            Debug.Log("Level complete - PersistentUIManager not found.");
        }
    }



}
