using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerTermScript : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector] public bool poweredOn;


    void Start()
    {
        poweredOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PowerOn()
    {
        poweredOn = true;
    }

    public void PowerOff()
    {
        poweredOn = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("I got the power");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Ain't got the power no more");
    }
}
