using System.Collections;
using System.Collections.Generic;
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
    private bool isOnMovingPlatform = false;

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

        if (MyFeetHitbox.IsTouchingLayers(MovingPlatform))
        {
            MyRigidBody.velocity = movingPlatformBody.velocity;
        }

        CheckPlayerInput();


    }


    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object's layer is within the targetLayer mask
        if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
        {
            GameObject touchedObject = collision.gameObject;
            isOnGround = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Check if the collided object's layer is within the targetLayer mask
        if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
        {
            //Debug.Log("Touching object on target layer: " + collision.gameObject.name);
            GameObject touchedObject = collision.gameObject;
            
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
            }
        }
    }
}
