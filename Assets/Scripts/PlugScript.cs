using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlugScript : MonoBehaviour
{
    [Header("MiniGame (UI Popup)")]
    public GameObject wireMiniGameUIPrefab; // assign WireMiniGame_UI prefab (Canvas) in inspector
    public Color[] colorOptions = new Color[] { Color.red, Color.green, Color.blue };

    [Header("Optional: world wire on complete")]
    public GameObject worldWirePrefab; // assign Prefabs/WorldWire.prefab (optional)
    public Transform worldWireTarget;  // optional other end (e.g., terminal). If null, uses offset from plug

    [Header("Optional: give player a wire on complete")]
    public GameObject wirePrefabForPlayer; // optional prefab to spawn and give to player

    // You can expose a name or ID if needed
    public string plugName = "Battery";

    void Start() { }
    void Update() { }

    // NOTE: we expect PlayerScript.ActiveWireSpawner to be of type PlugScript.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript otherScript = collision.gameObject.GetComponent<PlayerScript>();
            if (otherScript != null)
            {
                otherScript.ActiveWireSpawner = this; // set PlugScript reference
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript otherScript = collision.gameObject.GetComponent<PlayerScript>();
            if (otherScript != null && otherScript.ActiveWireSpawner == this)
            {
                otherScript.ActiveWireSpawner = null;
            }
        }
    }

    // Called by PlayerScript when the player presses the use key (Z) while near this plug.
    public void TryOpenMiniGame(PlayerScript player)
    {
        if (wireMiniGameUIPrefab == null)
        {
            Debug.LogWarning("PlugScript: wireMiniGameUIPrefab not assigned.");
            return;
        }

        // instantiate the UI canvas prefab (Screen Space - Overlay).
        GameObject mg = Instantiate(wireMiniGameUIPrefab);
        UIMiniGameManager mgr = mg.GetComponent<UIMiniGameManager>();
        if (mgr == null)
        {
            Debug.LogError("PlugScript: WireMiniGameUI prefab missing UIMiniGameManager.");
            Destroy(mg);
            return;
        }

        mgr.Configure(colorOptions, colorOptions.Length);

        // IMPORTANT: pass a lambda that captures the player so OnMiniGameComplete receives it.
        mgr.StartGame(player, () => OnMiniGameComplete(player));
    }

    // This is now called with the player who started the minigame.
    private void OnMiniGameComplete(PlayerScript player)
    {
        Debug.Log($"Plug '{plugName}': mini game complete for player {player.name}!");

        // OPTION A — spawn a world-space visual wire (no player possession)
        if (worldWirePrefab != null)
        {
            Vector3 a = transform.position;
            Vector3 b = (worldWireTarget != null) ? worldWireTarget.position : transform.position + transform.right * 1.5f;

            GameObject w = Instantiate(worldWirePrefab);
            WorldWire ww = w.GetComponent<WorldWire>();
            if (ww != null)
            {
                Color wireColor = (colorOptions != null && colorOptions.Length > 0) ? colorOptions[0] : Color.yellow;
                ww.Setup(a, b, wireColor, 0.06f);
            }
        }

        // OPTION B — give the player a wire GameObject so they carry it
        // (uncomment to enable). This requires the PlayerScript to expose a public pickup method.
        if (wirePrefabForPlayer != null && player != null)
        {
            Vector3 spawnPos = player.transform.position + new Vector3(0f, 1.5f, 0f);
            GameObject spawned = Instantiate(wirePrefabForPlayer, spawnPos, Quaternion.identity);

            // If PlayerScript has a public method to accept a wire, call it:
            // Prefer: player.ReceiveWire(spawned);
            // If you haven't added such a method, see the snippet below to add it.
            player.ReceiveWire(spawned);
        }

        // TODO: any other completion logic (play SFX, set plug state, save task completion) goes here
    }
}
