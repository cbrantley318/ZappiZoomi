using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlugScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //TODO: make this have a child gameobject called like "interactiveBox" or something idk
        //player can walk up to it and press "Z" to spawn a new wire in and grab it
        //if they aren't holding a wire, they can walk up to and press "X" to cycle the color it outputs?
        //when the new wire is spawned, it will exist on its own / independent of this thingamabob
        //okay also the spawning logic will likely happen in the player script, not in this script
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
