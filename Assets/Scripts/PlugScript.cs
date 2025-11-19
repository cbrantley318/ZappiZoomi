using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlugScript : MonoBehaviour
{

    [SerializeField] List<Color> availableColors;

    private int numColors = 0;
    private List<bool> freeColors;

    // Start is called before the first frame update
    void Start()
    {
        numColors = availableColors.Count;  //note: doesn't track which color was taken, maybe make this a "valid" array later on
        freeColors = new List<bool>();
        foreach (Color c in availableColors)
        {
            freeColors.Add(true);   //all specified colors start out available
        }
        //all this does rn is tell the player that this is the active box location, will eventually need to edit this to keep track of how many are available
        //and what colors are available
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

}
