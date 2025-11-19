using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform target;     // GameObject
    public float smoothSpeed = 5f;
    public Vector3 offset;
    public float threshold;

    // camera limits (in inspector)
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

  

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position = GameObject+ offset
        Vector3 desiredPosition = target.position + offset;

        // z fixed so camera doesn’t drift
        desiredPosition.z = transform.position.z;

        // clamp inside camera limits
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        // move camera toward GameObject

        if ((desiredPosition-transform.position).magnitude > threshold)
        {
            transform.position = desiredPosition + threshold * ((transform.position - desiredPosition).normalized);
        } else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }


    }

    // // Start is called before the first frame update
    // void Start()
    // { 
    // }

    // // Update is called once per frame
    // void Update()
    // {
    // 
    //}
}
