using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    // Checkbox in the Inspector to turn the elevator on or off
    public bool poweredOn = true; //obvi change this later to be on when powered on and off when not

    // elevator speed zoom zoom
    public float speed; //rn it's 2

    // distance it moves up
    public float moveDistance; //rn it's 3

    // start position
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        
        if (!poweredOn)
            return;

        float offset = Mathf.PingPong(Time.time * speed, moveDistance); // PingPong is an excellent function name btw   //wholeheartedly agree
        float newY = startPosition.y + offset;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
