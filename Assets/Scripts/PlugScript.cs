using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlugScript : MonoBehaviour
{

    [SerializeField] List<Color> availableColors;

    private int numColors = 0;
    private List<bool> freeColors;  //when player grabs this, we mark the color as no longer free or something

    private SpriteRenderer PowerSymbol;

    private Color ActiveColor;

    // Start is called before the first frame update
    void Start()
    {
        //getting references (no need to make an awake function yet)
        foreach (Transform childTransform in transform)
        {
            if (childTransform.gameObject.CompareTag("LightningBolt"))
            {
                PowerSymbol = childTransform.gameObject.GetComponent<SpriteRenderer>();
            }
        }


        //color management
        numColors = availableColors.Count;  //note: doesn't track which color was taken, maybe make this a "valid" array later on
        freeColors = new List<bool>();
        foreach (Color c in availableColors)
        {
            freeColors.Add(true);   //all specified colors start out available
        }

        Assert.IsTrue(numColors > 0);   //make sure we didnt screw up in the inspector
        ActiveColor = availableColors[0];
        PowerSymbol.color = ActiveColor;    //set the color aesthetically



    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //tell the player it's our box
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if the player enters our box, then tell them we're the wire spawn point
        if (collision.gameObject.CompareTag("Player")) {
            PlayerScript otherScript = collision.gameObject.GetComponent<PlayerScript>();
            otherScript.ActiveWireSpawner = gameObject;
        }
        //the player script will actually handle the logic of checking if we're in a trigger box, I think
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript otherScript = collision.gameObject.GetComponent<PlayerScript>();
            otherScript.ActiveWireSpawner = null;   //probably safe to just undo this once the player leaves, shouldn't affect anything though as long as playerscript has logic
        }
    }

    public void isColorAvailable()
    {

    }

    public void removeCurColor()
    {
        //TODO: this is a function that, when called, will remove the color from the freecolors, change the current color to the next one, or
        // if no more available, grey out the power box to show it's empty
    }

}
