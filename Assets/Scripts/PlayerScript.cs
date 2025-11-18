using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Player controller with wire interaction support.
/// - Walk/jump
/// - Interact with plugs (open wire UI popup via PlugScript)
/// - Spawn/pickup wires from sources
/// - Place wires into terminals
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject PlayerFeet;
    [SerializeField] GameObject WirePrefab;                 // prefab used when spawning a wire for the player
    [SerializeField] LayerMask GroundLayers;                // layers considered ground for jumping
    [SerializeField] LayerMask MovingPlatform;              // moving platforms / elevators
    [SerializeField] LayerMask WireSourceLayer;             // the layer(s) that represent wire sources
    [SerializeField] LayerMask WireTerminalLayer;           // layer(s) for placing wires into terminals

    [Header("Movement")]
    [SerializeField] float jumpVelocity = 10f;
    [SerializeField] float moveVelocity = 5f;

    // Public-ish fields other scripts read
    [HideInInspector] public PlugScript ActiveWireSpawner;   // set by PlugScript when player enters the plug trigger
    [HideInInspector] public GameObject ActiveWireTerminal;  // set by your terminal detection code (optional)

    // Local state
    private BoxCollider2D MyFeetHitbox;
    private Rigidbody2D MyRigidBody;
    private bool isCarryingWire = false;
    private GameObject CurrentWire = null;
    private Rigidbody2D movingPlatformBody;

    // control lock used by UIMiniGameManager
    private bool controlsLocked = false;

    void Awake()
    {
        MyRigidBody = GetComponent<Rigidbody2D>();
        if (PlayerFeet != null) MyFeetHitbox = PlayerFeet.GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        // make sure required refs exist
        if (PlayerFeet == null) Debug.LogWarning("[PlayerScript] PlayerFeet not assigned.");
        if (WirePrefab == null) Debug.LogWarning("[PlayerScript] WirePrefab not assigned (required to spawn wires).");
    }

    void Update()
    {
        // always update wire-follow behavior (so a carried wire follows even while controls locked)
        HandleWires();

        // detect dynamic collisions (platform parenting etc.)
        CheckDynamicCollisions();

        // if controls are locked (e.g., UI popup open), don't accept movement or input
        if (controlsLocked) return;

        // movement and input
        CheckPlayerInput();
    }

    #region Movement & Input

    void CheckPlayerInput()
    {
        /*-----MOTION---------*/
        float vx = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) vx = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) vx = 1f;

        Vector2 newVel = new Vector2(vx * moveVelocity, MyRigidBody.velocity.y);
        MyRigidBody.velocity = newVel;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MyFeetHitbox != null && (MyFeetHitbox.IsTouchingLayers(GroundLayers) || MyFeetHitbox.IsTouchingLayers(MovingPlatform)))
            {
                MyRigidBody.velocity = MyRigidBody.velocity + new Vector2(0f, jumpVelocity);
            }
        }

        /*-----INTERACTION (Z)---------*/
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 1) Priority: if near a plug (ActiveWireSpawner) then open UI popup instead of spawning a wire immediately
            if (ActiveWireSpawner != null)
            {
                PlugScript plug = ActiveWireSpawner as PlugScript;
                if (plug != null)
                {
                    float maxUseDistance = 1.6f;
                    float dist = Vector2.Distance(transform.position, plug.transform.position);
                    if (dist <= maxUseDistance)
                    {
                        plug.TryOpenMiniGame(this);
                        return; // don't also spawn wire or interact with terminal
                    }
                    else
                    {
                        // stale reference - clear and continue to other checks
                        ActiveWireSpawner = null;
                    }
                }
            }

            // 2) If touching a wire source and not currently carrying a wire -> spawn and pick one up
            if (!isCarryingWire && GetComponent<BoxCollider2D>().IsTouchingLayers(WireSourceLayer))
            {
                SpawnWire(ActiveWireSpawner); // ActiveWireSpawner may be null; method handles null safely
                return;
            }

            // 3) If touching a terminal -> handle terminal interaction (existing logic)
            if (GetComponent<BoxCollider2D>().IsTouchingLayers(WireTerminalLayer))
            {
                HandleWireTermInteraction();
                return;
            }

            // otherwise no action
        }
    }

    #endregion

    #region Wire handling

    private void HandleWires()
    {
        if (isCarryingWire && CurrentWire != null)
        {
            // keep the wire positioned relative to the player (example offset)
            CurrentWire.transform.position = transform.position + new Vector3(0f, 1.5f, 0f);
        }
    }

    // Public wrapper so other scripts (PlugScript) can give the player a wire GameObject
    public void ReceiveWire(GameObject wire)
    {
        if (wire == null)
        {
            Debug.LogWarning("[PlayerScript] ReceiveWire called with null wire.");
            return;
        }
        PickUpWire(wire);
    }

    // internal pickup logic (keeps same behaviour as your original)
    private void PickUpWire(GameObject wire)
    {
        if (wire == null)
        {
            Debug.LogWarning("[PlayerScript] PickUpWire: wire is null.");
            return;
        }
        CurrentWire = wire;
        isCarryingWire = true;
    }

    // safe SpawnWire implementation - accepts a PlugScript (may be null).
    private void SpawnWire(PlugScript WireSpawner)
    {
        // logging to help debugging
        // Debug.Log($"SpawnWire called. WireSpawner={(WireSpawner==null?"null":WireSpawner.name)}, ActiveWireSpawner={(ActiveWireSpawner==null?"null":ActiveWireSpawner.name)}");

        // Determine start position for the wire base
        Vector3 startPos;
        if (WireSpawner != null)
        {
            startPos = WireSpawner.transform.position;
        }
        else if (ActiveWireSpawner != null)
        {
            startPos = ActiveWireSpawner.transform.position;
        }
        else
        {
            startPos = transform.position;
            Debug.LogWarning("[PlayerScript] SpawnWire: WireSpawner null, using player position as base.");
        }

        // spawn location relative to player
        Vector3 spawnPos = transform.position + new Vector3(0f, 1.5f, 0f);

        // safety checks
        if (WirePrefab == null)
        {
            Debug.LogError("[PlayerScript] SpawnWire failed: WirePrefab not assigned in inspector.");
            return;
        }

        CurrentWire = Instantiate(WirePrefab, spawnPos, Quaternion.identity);
        if (CurrentWire == null)
        {
            Debug.LogError("[PlayerScript] SpawnWire: Instantiate returned null.");
            return;
        }

        WireScript wireScript = CurrentWire.GetComponent<WireScript>();
        if (wireScript == null)
        {
            Debug.LogError("[PlayerScript] SpawnWire: spawned WirePrefab missing WireScript component.");
            return;
        }

        wireScript.SetWireBase(startPos);
        PickUpWire(CurrentWire);
    }

    #endregion

    #region Terminal & dynamic collisions

    private void PlaceWire(GameObject TargetTerminal)
    {
        if (CurrentWire == null)
        {
            Debug.LogWarning("[PlayerScript] PlaceWire called but CurrentWire is null.");
            return;
        }

        isCarryingWire = false;
        var termScript = TargetTerminal.GetComponent<PowerTermScript>();
        if (termScript != null)
        {
            termScript.PowerOn();
            termScript.CurrentWire = CurrentWire;
            CurrentWire.GetComponent<WireScript>().SnapToPosition(TargetTerminal, new Vector3(-0.75f, 0f, 0f));
            CurrentWire = null;
        }
        else
        {
            Debug.LogWarning("[PlayerScript] PlaceWire: target terminal missing PowerTermScript.");
        }
    }

    private void CheckDynamicCollisions()
    {
        // elevator / moving platform parenting
        if (movingPlatformBody != null && MyFeetHitbox != null && MyFeetHitbox.IsTouchingLayers(MovingPlatform))
        {
            transform.SetParent(movingPlatformBody.gameObject.transform);
        }
        else
        {
            transform.SetParent(null);
        }

        // TODO: update ActiveWireTerminal detection if you want automatic detection
    }

    #endregion

    #region Trigger handlers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // keep movingPlatformBody when we come in contact with elevators (if applicable)
        if (((1 << collision.gameObject.layer) & (int)MovingPlatform) != 0)
        {
            movingPlatformBody = collision.gameObject.GetComponent<Rigidbody2D>();
        }

        // Note: PlugScript should set ActiveWireSpawner when the player enters its interactiveBox
        // Terminals may also set ActiveWireTerminal in their own triggers (not handled here)
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & (int)MovingPlatform) != 0)
        {
            movingPlatformBody = null;
        }
    }

    // Handles placing or picking up wires at a powered terminal.
private void HandleWireTermInteraction()
{
    // 0bXY bit pattern: X = carrying wire?, Y = terminal already powered?
    int switchVal = (isCarryingWire ? 1 : 0) << 1 |
                    (ActiveWireTerminal != null &&
                     ActiveWireTerminal.GetComponent<PowerTermScript>()?.poweredOn == true ? 1 : 0);

    switch (switchVal)
    {
        case 0: // 00 = not holding wire & terminal empty
            Debug.Log("Nothing to do at terminal.");
            break;
        case 1: // 01 = pick up wire from terminal
            if (ActiveWireTerminal != null)
            {
                var term = ActiveWireTerminal.GetComponent<PowerTermScript>();
                PickUpWire(term.CurrentWire);
                term.PowerOff();
            }
            break;
        case 2: // 10 = place down wire
            if (ActiveWireTerminal != null)
                PlaceWire(ActiveWireTerminal);
            break;
        case 3: // 11 = both full; maybe swap later
            Debug.Log("Tried placing a wire somewhere already full.");
            break;
    }
}

    #endregion

    #region Utility: control lock used by popup

    // Called by UIMiniGameManager (via PlugScript) to lock/unlock player controls
    public void SetControlLocked(bool locked)
    {
        controlsLocked = locked;
        if (locked && MyRigidBody != null)
        {
            MyRigidBody.velocity = Vector2.zero;
        }
    }

    #endregion
}
