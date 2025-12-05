using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ElevatorPlatform : MonoBehaviour
{
    // Checkbox in the Inspector to turn the elevator on or off
    public bool poweredOn = false; //obvi change this later to be on when powered on and off when not

    // elevator speed zoom zoom
    public float speed; //rn it's 2

    // distance it moves up
    public float moveDistance; //rn it's 3

    // is it an elevator (true) or a gate(false)
    [SerializeField] bool pingpong;     //what a great variable name

    // position state information
    private Vector3 startPosition;
    private Rigidbody2D MyRigidBody;
    private LineRenderer MyLineRenderer;

    private List<Vector3> path;
    private bool goingUp = true;    //start out incrementing path, when you hit the top pause and then go down, then go back
    private float pauseTime;
    [SerializeField] float pauseInterval = 0.2f; //pause duration (in seconds!)
    private int currPos = 0;

    private int moveDirection = 1;  //1 is up, 0 is back (dont edit this even though it's pretty much identical to goingUp
    private GameObject parentObj;

    //TODO: multiple possible changes (listed below, all optional so idk):
    /*
     * Add a flag for it to function as a gate or an elevator (move up and down or hold position at top)
     * Change the script from using PingPong timer function to smoother motion w/ setting velocity in direction of target
     * Add a linerenderer child to this and let it follow the points set by that line (allows us to set the path in the editor visually and also to add
     * more than two endpoints)
     * Add an InstantDeathZone Trigger below the elevator to kill the player (maybe don't make this part of the prefab, just for the one instance)
     */


    private void Awake()
    {
        parentObj = transform.parent.gameObject;
        MyRigidBody = GetComponent<Rigidbody2D>();
        MyLineRenderer = parentObj.GetComponent<LineRenderer>();

        startPosition = transform.position;

        //fill the path with the linerender points, then the elevator will move point-to-point
        path = new();
        for (int i = 0; i < MyLineRenderer.positionCount; i++)
        {
            path.Add(MyLineRenderer.GetPosition(i) + transform.position);   //for now applying the world->local offset here
        }
        MyLineRenderer.enabled = false; //make it disappear //optional todo: make it appear to the player if it's aesthetically pleasing (think mario 64 elevator paths)
        MyRigidBody.MovePosition(path[0]);

    }


    void Start()
    {
        goingUp = true;
    }

    void FixedUpdate()
    {
        poweredOn = parentObj.GetComponentInChildren<PowerTermScript>().poweredOn;  //there can be only one (PowerTermScript, that is)

        //float offset = Mathf.PingPong(Time.time * speed, moveDistance); // PingPong is an excellent function name btw   //wholeheartedly agree
        //float newY = startPosition.y + offset;

        //transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        if (poweredOn)
        {
            //move to the next position

            if (Time.time - pauseTime > pauseInterval)  //if 
            {
                //allow movement once we've paused long enough
                if (goingUp)
                {
                    MoveUp(pingpong);   //handles the logic for incrementing state and telling us when to move
                }
                else
                {
                    MoveDown(pingpong);
                }
            }
        }
        else
        {
            MoveDown(false);
        }
    }

    void MoveUp(bool continueMotion)
    {
        Assert.IsTrue(poweredOn);//sanity checking

        //if we havent reach the current target point, then move to it
        //if we have, then check if it's the last one or increment

        if (currPos == 0)
        {
            currPos++;
        }

        if (HitTarget(path[currPos-1], path[currPos], MyRigidBody.transform.position))
        {   

            if (currPos == path.Count - 1)
            {
                if (continueMotion)     //if false, will always stay here (for the non-elevator methods
                {
                    goingUp = false;    //start moving elevator back down
                }
                MyRigidBody.velocity = Vector3.zero;   //stop moving and pause
                pauseTime = Time.time;
            }
            else
            {
                currPos++;
            }
        } else
        {
            MyRigidBody.velocity = speed * (path[currPos] - transform.position).normalized;   //path[0] is the startPosition now
        }

    }

    void MoveDown(bool continueMotion)
    {
        if (currPos == path.Count -1)
        {
            currPos--;
        }


        if (HitTarget(path[currPos+1], path[currPos], MyRigidBody.transform.position))
        {
            if (currPos == 0)
            {
                if (continueMotion)     //if false, will always stay here (for the non-elevator methods
                {
                    goingUp = true;    //start moving elevator back down
                }
                MyRigidBody.velocity = Vector3.zero;   //stop moving and pause
                pauseTime = Time.time;
            }
            else
            {
                currPos--;
            }
        }
        else
        {
            MyRigidBody.velocity = speed * (path[currPos] - transform.position).normalized;   //path[0] is the startPosition now
        }

    }
    

    //------- HELPERS FOR FIXING UNITY'S FLOATING-POINT MATH (their threshold is wayyyy too low for this ----------//

    bool MyFloatApprox(float a, float b)
    {
        return MyFloatApprox(a, b, 0.0001f);
    }

    bool MyFloatApprox(float a, float b, float thresh)      //ended up not 
    {
        return (Mathf.Abs(a - b) < thresh);
    }

    bool HitTarget(Vector3 start, Vector3 goal, Vector3 curr)
    {
        // so now we need to check where we are with direction AND magnitude!

        if (MyFloatApprox((MyRigidBody.transform.position - path[currPos]).magnitude, 0))   //first check if we're close enough
            return true;

        //now check if we overshot (stupid floating-point precision ugh)
        if (Vector3.Dot((curr - goal), (start - goal)) < 0)                 //just draw the triangles
            return true;

        return false;
    }

}
