using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    // Checkbox in the Inspector to turn the elevator on or off
    public bool poweredOn = false; //obvi change this later to be on when powered on and off when not

    // elevator speed zoom zoom
    public float speed; //rn it's 2

    // distance it moves up
    public float moveDistance; //rn it's 3

    // start position
    private Vector3 startPosition;
    private Rigidbody2D MyRigidBody;

    //TODO: multiple possible changes (listed below, all optional so idk):
    /*
     * Add a flag for it to function as a gate or an elevator (move up and down or hold position at top)
     * Change the script from using PingPong timer function to smoother motion w/ setting velocity in direction of target
     * Add a linerenderer child to this and let it follow the points set by that line (allows us to set the path in the editor visually and also to add
     * more than two endpoints)
     * Add an InstantDeathZone Trigger below the elevator to kill the player (maybe don't make this part of the prefab, just for the one instance)
     */

    private int moveDirection = 1;  //1 is up, 0 is back
    private GameObject parentObj;


    void Start()
    {
        startPosition = transform.position;
        MyRigidBody = GetComponent<Rigidbody2D>();
        parentObj = transform.parent.gameObject;
    }

    void Update()
    {
        poweredOn = parentObj.GetComponentInChildren<PowerTermScript>().poweredOn;  //there can be only one (PowerTermScript, that is)
                

        float offset = Mathf.PingPong(Time.time * speed, moveDistance); // PingPong is an excellent function name btw   //wholeheartedly agree
        float newY = startPosition.y + offset;

        //transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        if (poweredOn)
        {
            if (newY > transform.position.y)                        //changed to this becase the RigidBody stuff doesn't like editing the transform.position directly :(
            {                                                       //also because the playerScript needs to get the velocity of this, so for that to work we need a velocity
                MyRigidBody.velocity = new Vector2(0, speed);
                moveDirection = 1;
            } else if (newY < transform.position.y)
            {
                MyRigidBody.velocity = new Vector2(0, -speed);
                if (moveDirection == 1 && GameObject.FindWithTag("Player").transform.IsChildOf(transform))
                {
                    GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().velocity = MyRigidBody.velocity;
                }
                moveDirection = 0;
            }
        } else
        {
            //if poweredOff, move back to resting position
            if (!Mathf.Approximately((MyRigidBody.transform.position - startPosition).magnitude, 0))
            {
                MyRigidBody.velocity = speed * (startPosition - transform.position).normalized;
            } else
            {
                MyRigidBody.velocity = Vector3.zero;
            }
        }

    }


}
