using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    // Checkbox in the Inspector to turn the elevator on or off
    public bool poweredOn = true;

    // elevator speed zoom zoom
    public float speed = 2f;

    // distance it moves up
    public float moveDistance = 3f;

    // start position
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // If not powered, do nothing
        if (!poweredOn)
            return;

        
        float offset = Mathf.PingPong(Time.time * speed, moveDistance); // PingPong is an excellent function name btw
        float newY = startPosition.y + offset;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
