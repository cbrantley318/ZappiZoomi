using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform target;     // GameObject
    public float smoothSpeed = 5f;
    public Vector3 offset;
    public float threshold;
    [SerializeField] int MapL, MapR, MapU, MapD;  

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position = GameObject+ offset
        Vector3 desiredPosition = target.position + offset;

        // z fixed so camera doesn’t drift
        desiredPosition.z = transform.position.z;

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
