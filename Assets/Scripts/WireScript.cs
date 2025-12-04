using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class WireScript : MonoBehaviour
{
    //this script handles probably two things
    // - drawing the wire to follow after where the player moves
    // - snapping into certain positions when it's plugged in. 
    //it will not handle actually picking up and placing itself, currently that's implemented in the player logic but it could be here as well


    [SerializeField] LayerMask WireTerminalLayer;   //maybe make this both wire source and wire terminal, idk yet but change in Inspector

    //private bool hasPower = true;   //in case we want to play around later with some wires being duds, I guess
    private float segmentLength = 2.0f;
    private float recombineLength = 1.75f;

    //[SerializeField] int maxNumLinks;     //NEITHER OF THESE CAN BE IMPLEMENTED UNTIL WE ADD ABILITY TO PLACE WIRES BACK IN THE SPAWNER / RESET WIRE STATES WITHOUT DYING
    //[SerializeField] int maxLength;       //I MEAN IT CAN... BUT IT WON'T BE GOOD GAMEPLAY UNLESS YOU MAKE IT EASY TO UNDO ERRORS

    private Color WireColor;    //this is set by SpawnWire, and it's used to determine where you can shove it

    private LineRenderer MyLineRenderer;
    private SpriteRenderer fgSprite;

    private void Awake()
    {
        WireColor = Color.white;    //default val
        MyLineRenderer = GetComponent<LineRenderer>();  //needs to be called before instantiate returns
        fgSprite = transform.GetChild(0).GetComponent<SpriteRenderer>(); //this one doesnt but whatever, maybe it does
    }


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        HandleAddingLineSegment();
    }

    public void SetColor(Color c)
    {
        WireColor = c;
        MyLineRenderer.startColor = WireColor;
        MyLineRenderer.endColor   = WireColor;
        fgSprite.color = WireColor;

    }

    public Color GetColor()
    {
        //Debug.Log("WireColor:" + WireColor);
        return WireColor;
    }

    public void SetWireBase(Vector3 StartPos)
    {
        MyLineRenderer.positionCount = 2;
        MyLineRenderer.SetPosition(0, StartPos);
        MyLineRenderer.SetPosition(1, transform.position);
    }

    private void HandleAddingLineSegment()
    {
        //here are the rules for segments:
        /*
         * The player location is at MLR.positionCount - 1
         * The one right before is MLR.pCt - 2
         * The one before that is MLR.pCt - 3
         * The source is at index 0
         * 
         * Whenever the distance between PC-1 and PC-2 exceeds newThresh, we spawn a new segment
         * 
         * Whenever the distance between PC-1 and PC-3 is less than a minThresh, we remove PC-2
         * Could also adapt the above to be if the distance between PC-1 and ANY point that isnt PC-2 is small, collapse it all down (i.e. if we have 5 links and loop back to the start)
         * 
         * Should work with no bugs as long as minThresh is small compared to newThresh
         * Would probably work fine even if that wasn't true
         * Will need to ensure count > 2 to do the collapsing
         * 
         */

        Vector3 lastPos = MyLineRenderer.GetPosition(MyLineRenderer.positionCount - 2);           //TODO: add this in if you want segmented lines instead of a straight shot
        if ((lastPos - transform.position).magnitude > segmentLength)
        {
            MyLineRenderer.positionCount++;
        }

        //for (int i = 0; i < MyLineRenderer.positionCount - 2; i ++)
        //{
        //    Vector3 skipPos = MyLineRenderer.GetPosition(i);           //TODO: add this in if you want segmented lines instead of a straight shot
        //    if ((transform.position - skipPos).magnitude < recombineLength)
        //    {
        //        MyLineRenderer.positionCount--; //should be easy as that. let's watch it burn
        //    }
        //}

        if (MyLineRenderer.positionCount > 2)   //try removing segments
        {

            Vector3 skipPos = MyLineRenderer.GetPosition(MyLineRenderer.positionCount - 3);           //TODO: add this in if you want segmented lines instead of a straight shot
            if ((transform.position - skipPos).magnitude < recombineLength)
            {
                MyLineRenderer.positionCount--; //should be easy as that. let's watch it burn
            }
        }


        MyLineRenderer.SetPosition(MyLineRenderer.positionCount - 1, transform.position);
    }

    public void SnapToPosition(GameObject thingToSnapTo, Vector3 offset)
    {
        //note: the player script will have to properly "detach" us from it first
        //Assert.IsTrue(((1 << thingToSnapTo.layer) & WireTerminalLayer) != 0);   //make sure we're snapping to a terminal node thingy
        transform.position = thingToSnapTo.transform.position + offset;
    }
}
