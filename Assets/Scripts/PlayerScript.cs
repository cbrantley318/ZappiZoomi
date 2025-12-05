using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;



public class PlayerScript : MonoBehaviour
{
    //GameObjects
    [SerializeField] GameObject PlayerFeet;
    [SerializeField] GameObject WirePrefab;

    //layers
    [SerializeField] LayerMask GroundLayers;    //the layers the player can jump from
    [SerializeField] LayerMask MovingPlatform;  //layers that we snap our velocity to. Note that if we change the player physics to acceleration instead of velocity-based, this could possibly cause some weird stuff
    [SerializeField] LayerMask InstantDeathLayer;
    [SerializeField] LayerMask ElectrocutionLayer;
    [SerializeField] LayerMask WireSourceLayer;
    [SerializeField] LayerMask WireTerminalLayer;
    [SerializeField] LayerMask LevelEndLayer;

    //physics/movement
    [SerializeField] float jumpVelocity;    //10
    [SerializeField] float moveVelocity;    //5
    [SerializeField] float moveAccel;       //0.5
    [SerializeField] float airLoss;       //0.5



    //things we want other scripts to access but not the editor
    [HideInInspector] public GameObject ActiveWireSpawner;
    [HideInInspector] public GameObject ActiveWireTerminal;


    //things local to the player object
    private BoxCollider2D MyFeetHitbox;
    private Rigidbody2D MyRigidBody;

    private float jumpTimeout = 0.5f;   //only let them jump once every 0.5s
    private float jumpTime = 0;

    //wire management
    private bool isCarryingWire = false;
    private Vector3 holdWirePosition = new Vector3(0.2f, 0.0f, 0);
    private Vector3 spawnOffset = new Vector3(0.5f, -0.5f, 0.0f);

    //things that we get from other objects
    private GameObject CurrentWire;

    private Rigidbody2D movingPlatformBody;

    [Header("Level Completion")]
    [Tooltip("Episode/level id to mark completed (used by ProgressManager).")]
    [SerializeField] int episodeToComplete = 1;
    [Tooltip("Delay (seconds) before showing the Level Won modal (optional)")]
    [SerializeField] float levelCompleteDelay = 0.25f;
    bool levelCompleted = false;

    // audio 
    [SerializeField] private AudioSource elevatorAudio;
    private ElevatorPlatform currentElevator;

    // Start is called before the first frame update
    void Start()
    {
        MyRigidBody = GetComponent<Rigidbody2D>();
        MyFeetHitbox = PlayerFeet.GetComponent<BoxCollider2D>();
        if (elevatorAudio != null)
        {
            elevatorAudio.loop = true;    // loop while standing on elevator
            elevatorAudio.playOnAwake = false;
            elevatorAudio.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleWires();

        CheckDynamicCollisions();   //for now this is just if we're touching elevators
                                    //TODO: add the 'touching wire terminal' to this bit (lol jk it's implemented elsewhere)
        CheckPlayerInput();         //for moving and jumping and (eventually) grabbing wires and placing them and probably a pause screen as well

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object's layer is within the targetLayer mask  //gotta love AI-generated comments lol
        //if (((1 << collision.gameObject.layer) & InstantDeathLayer) != 0)     //once again, polling proves better than interrupts
        //{
        //    KillPlayer();
        //}
        //if (((1 << collision.gameObject.layer) & ElectrocutionLayer) != 0)
        //{
        //    if (isCarryingWire)
        //    {
        //        KillPlayer();
        //    }
        //}

    }

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
    //    {
    //        isOnGround = false;
    //    }

    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & MovingPlatform) != 0)
        {
            movingPlatformBody = collision.gameObject.GetComponent<Rigidbody2D>();  //update reference so we know which elevator we care about
            if (collision.CompareTag("Elevator"))
                {
                    ElevatorPlatform elevator = collision.GetComponent<ElevatorPlatform>();
                    if (elevator == null)
                    {
                        // If ElevatorPlatform script is on the parent instead:
                        elevator = collision.GetComponentInParent<ElevatorPlatform>();
                    }

                    // If still null, this isn't a real elevator platform
                    if (elevator == null)
                        return;

                    // Check if elevator is powered on
                    if (elevator.poweredOn && !GameState.IsUIOpen)
                    {
                        if (elevatorAudio != null && !elevatorAudio.isPlaying)
                            elevatorAudio.Play();
                    }
                }
        }

        

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & MovingPlatform) != 0)
        {
            movingPlatformBody = null;
            if (collision.CompareTag("Elevator"))
                {
                    if (elevatorAudio != null && elevatorAudio.isPlaying)
                        elevatorAudio.Stop();
                }
        }
    }


    //- END COLLISIONS ----//


    //--------- CONTROL INPUT HELPERS and motion/physics-----------//
    void CheckPlayerInput()
    {
        /*-----MOTION---------*/

        float accel = (MyFeetHitbox.IsTouchingLayers(GroundLayers)) ? moveAccel : airLoss * moveAccel;

        if (Input.GetKey(KeyCode.LeftArrow))    //move left (no acceleration yet, just barebones for debugging until Hanchi pushes his code)
        {
            MyRigidBody.AddForce(accel * Vector2.left, ForceMode2D.Force);
        }
        if (Input.GetKey(KeyCode.RightArrow))   //move right
        {
            MyRigidBody.AddForce(accel * Vector2.right, ForceMode2D.Force);
        }

        RescaleXSpeed();
          

        //Jumping
        if (Input.GetKeyDown(KeyCode.Space) && (Time.time - jumpTime) > jumpTimeout)    //jump
        {

            if (MyFeetHitbox.IsTouchingLayers(GroundLayers) || MyFeetHitbox.IsTouchingLayers(MovingPlatform))
            {
                MyRigidBody.velocity = MyRigidBody.velocity + new Vector2(0, jumpVelocity);
            }
        }
        
        /*-----Grabbing Things---------*/
        if (Input.GetKeyDown(KeyCode.Z))    //grab wire if not holding one already
        {
            if (GetComponent<BoxCollider2D>().IsTouchingLayers(WireTerminalLayer))  //if it's touching one of these layers, we have two options (see below)
            {
                HandleWireTermInteraction();
            }
            else if (GetComponent<BoxCollider2D>().IsTouchingLayers(WireSourceLayer))    //grab wire if not holding one already and if we're at a thing you can grab a wire from
            {
                Assert.IsFalse(ActiveWireSpawner == null);  //todo: remove assertion once verified
                if (!isCarryingWire && ActiveWireSpawner.GetComponent<PlugScript>().IsColorAvailable())
                {  //try to "pick up" a wire if we can
                    SpawnWire(ActiveWireSpawner);
                }
            }
            else if (isCarryingWire)
            {
                //if holding a wire and there's no terminals or sources nearby, then maybe let the player drop it? (currently does nothing)
                //Debug.Log("Not touching term layer");
            }
        }



    }


    private void RescaleXSpeed()
    {
        float xSpeed = Mathf.Abs(MyRigidBody.velocity.x);
        float newSpeed = MyRigidBody.velocity.x;
        if (xSpeed > moveVelocity)
        {
            newSpeed = (MyRigidBody.velocity.x / xSpeed) * moveVelocity;
        } 
        MyRigidBody.velocity = new Vector2(newSpeed, MyRigidBody.velocity.y);
    }


    private void CheckDynamicCollisions()
    {
        //make velocity same as the elevator's
        if (movingPlatformBody != null && MyFeetHitbox.IsTouchingLayers(MovingPlatform))
        {
            transform.SetParent(movingPlatformBody.gameObject.transform);
            //MyRigidBody.velocity = movingPlatformBody.velocity;     //todo: fix problem of jumping and moving off of this without it snapping the player back down
        }
        else
        {
            transform.SetParent(null);
        }

        BoxCollider2D MyHitbox = GetComponent<BoxCollider2D>();

        if (MyHitbox.IsTouchingLayers(InstantDeathLayer))
        {
            KillPlayer();
        }

        if (isCarryingWire && MyHitbox.IsTouchingLayers(ElectrocutionLayer))
        {
            KillPlayer();
        }

        //if (!levelCompleted && MyHitbox.IsTouchingLayers(LevelEndLayer))      //moved to LevelEndScript
        //{
        //    levelCompleted = true;
        //    OnLevelComplete();
        //}



        //TODO: check for the "WireSource" layer and the "WireTerminal" layer (also create those layers in the Inspector
        // (idk why i said to check for them here... for now will not include)
    }

    //- END CONTROL INPUT HELPERS ----//

    //-------- PLAYER STATE ----------------------//
    void KillPlayer()
    {   
        //TODO: add in a death animation or something to hide the latency (maybe just a UI popup yeah that's easy).
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    //- END PLAYER STATE -----//

    //--------- WIRE INTERACTION HELPERS --------//
    private void HandleWires()
    {
        if (isCarryingWire)
        {
            CurrentWire.transform.position = transform.position + holdWirePosition;
        }
    }

    private void SpawnWire(GameObject WireSpawner)
    {   
        Assert.IsTrue(WireSpawner.GetComponent<PlugScript>().IsColorAvailable());  //dummy check

        //this assumes we've already checked a wire is available before calling this, so make sure to do that
        Vector3 startPos = ActiveWireSpawner.transform.position + spawnOffset;
        Vector3 spawnPos = transform.position + holdWirePosition;
        Color wireColor = ActiveWireSpawner.GetComponent<PlugScript>().GetCurrentColor();

        //the wire head is purely animation only, it will not alter collision hitboxes in any way so it doesn't need to be treated as a rigid body.
        //while holding it, the player will just always fix it to a fixed location relative to the player
        //eventually, we'll replace this with a new animation of the player holding the wire and then it'll only be the ine renderer.
        CurrentWire = Instantiate(WirePrefab, spawnPos, Quaternion.identity);
        CurrentWire.GetComponent<WireScript>().SetWireBase(startPos);
        CurrentWire.GetComponent<WireScript>().SetColor(wireColor);
        PickUpWire(CurrentWire);

        WireSpawner.GetComponent<PlugScript>().RemoveCurColor();


        //now, tell the spawner we picked it up


    }

    private void PickUpWire(GameObject wire)
    {
        CurrentWire = wire;
        isCarryingWire = true;
    }

    private void PlaceWire(GameObject TargetTerminal)
    {
        //places a wire at the target terminal, "setting it free" from the player and letting it exist until we pick it up again from there
        PowerTermScript pts = TargetTerminal.GetComponent<PowerTermScript>();
        WireScript ws = CurrentWire.GetComponent<WireScript>();
        if (pts.GetColor() == ws.GetColor())
        {
            isCarryingWire = false;
            pts.PowerOn();
            pts.CurrentWire = CurrentWire;   //save a reference to the wire here
            ws.SnapToPosition(TargetTerminal, new Vector3(-.05f, 0, 0));    //todo: play around with different offsets
        }
    }

    public Color GetCarriedColor()
    {
        //returns the color of the wire we're carrying, if any
        if (!isCarryingWire)
        {
            return Color.white;
        }

        return CurrentWire.GetComponent<WireScript>().GetColor();

    }

    private void HandleWireTermInteraction()
    {
        //Option A: overload the Z key and run through 4 cases:
        //  not holding wire and no wire in the terminal already                => do nothing
        //  already holding a wire but there's already a wire in there          => either do nothing or maybe swap wires
        //  holding a wire and terminal is empty                                => place wire in terminal
        //  hands are free and wire in the terminal                             => grab the wire (DONT SPAWN A NEW ONE)

        int switchVal = (isCarryingWire?1:0) << 1 | (ActiveWireTerminal.GetComponent<PowerTermScript>().poweredOn? 1:0);
        switch (switchVal)
        {
            case 0:         //00 = do nothing
                break;
            case 1:         //01 = pick up wire
                PickUpWire(ActiveWireTerminal.GetComponent<PowerTermScript>().CurrentWire);
                ActiveWireTerminal.GetComponent<PowerTermScript>().PowerOff();
                break;
            case 2:         //10 = place down wire (ONLY IF COLORS MATCH)
                PlaceWire(ActiveWireTerminal);
                break;
            case 3:         //11 = do nothing for now (maybe swap later?)
                Debug.Log("tried placing a wire somewhere already full");
                break;
            default:
                break;
        }

    }

    //- END WIRE INTERACTION HELPERS --//





}
