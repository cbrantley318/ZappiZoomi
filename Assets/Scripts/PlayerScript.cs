using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameObject PlayerFeet;
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] LayerMask MovingPlatform;
    [SerializeField] float jumpVelocity = 10;
    [SerializeField] float moveVelocity = 5;    //technically speed not velocity


    //things local to the player object
    private BoxCollider2D MyFeetHitbox;
    private Rigidbody2D MyRigidBody;


    private bool isOnGround = false;

    //things that we get from other objects
    private Rigidbody2D movingPlatformBody;

    // Start is called before the first frame update
    void Start()
    {
        MyRigidBody = GetComponent<Rigidbody2D>();
        MyFeetHitbox = PlayerFeet.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

        CheckDynamicCollisions();   //for now this is just if we're touching elevators
                                    //TODO: add the 'touching wire terminal' to this bit


        CheckPlayerInput();

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("enter");
        // Check if the collided object's layer is within the targetLayer mask
        if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
        {
            Debug.Log("Heyo!");
            isOnGround = true;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("exit");
        if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
        {
            Debug.Log("Bye-O!");

            isOnGround = false;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & MovingPlatform) != 0)
        {
            movingPlatformBody = collision.gameObject.GetComponent<Rigidbody2D>();  //update reference so we know which elevator we care about
        }
    }

    void CheckPlayerInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MyRigidBody.velocity = new Vector2(-moveVelocity, MyRigidBody.velocity.y);

        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            MyRigidBody.velocity = new Vector2(moveVelocity, MyRigidBody.velocity.y);

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MyFeetHitbox.IsTouchingLayers(GroundLayer))
            {
                MyRigidBody.velocity = MyRigidBody.velocity + new Vector2(0, jumpVelocity);
                Debug.Log(isOnGround);
            }
        }
    }

    void CheckDynamicCollisions()
    {
        //make velocity same as the elevator's
        if (movingPlatformBody != null && MyFeetHitbox.IsTouchingLayers(MovingPlatform))
        {
            MyRigidBody.velocity = movingPlatformBody.velocity;
        }

        //TODO: check for the "WireSource" layer and the "WireTerminal" layer (also create those layers)

    }




}
