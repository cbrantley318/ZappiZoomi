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
        if (poweredOn && newY > transform.position.y)                        //changed to this becase the RigidBody stuff doesn't like editing the transform.position directly :(
        {                                                       //also because the playerScript needs to get the velocity of this, so for that to work we need a velocity
            MyRigidBody.velocity = new Vector2(0, speed);
        } else if (newY < transform.position.y)
        {
            MyRigidBody.velocity = new Vector2(0, -speed);
        }

    }


}
