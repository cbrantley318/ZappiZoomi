using UnityEngine;

public class PlugTriggerRelay : MonoBehaviour
{
    private PlugScript parentPlug;

    void Awake()
    {
        parentPlug = GetComponentInParent<PlugScript>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (parentPlug != null)
            parentPlug.SendMessage("OnTriggerEnter2D", col, SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (parentPlug != null)
            parentPlug.SendMessage("OnTriggerExit2D", col, SendMessageOptions.DontRequireReceiver);
    }
}
