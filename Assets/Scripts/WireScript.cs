using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class WireScript : MonoBehaviour
{
    //this script handles probably two things
    // - drawing the wire to follow after where the player moves
    // - snapping 


    [SerializeField] LayerMask WireTerminalLayer;

    private bool hasPower = true;   //in case we want to play around later with some wires being duds, I guess


    private LineRenderer MyLineRenderer;


    

    void Start()
    {
        MyLineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetWireBase(Vector3 StartPos)
    {
        MyLineRenderer.positionCount = 2;
        MyLineRenderer.SetPosition(0, StartPos);
        MyLineRenderer.SetPosition(1, transform.position);
    }


    void SnapToPosition(GameObject thingToSnapTo)
    {
        //note: the player script will have to properly "detach" us from it first, unless we track that in this script, idk haven't decided yet
        Assert.IsTrue(((1 << thingToSnapTo.layer) & WireTerminalLayer) != 0);   //make sure we're snapping to a terminal node thingy

        //this shouldnt be a rigidbody or anything like that, so modifying the transform directly is A-OK

        Vector3 offset = new(-0.5f, -0.2f, 0);      //complete guess here, this also has the implication that every plug needs to be the same which I'm fine with
        transform.position = thingToSnapTo.transform.position + offset;
    }
}
