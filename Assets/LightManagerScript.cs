using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LightManagerScript : MonoBehaviour
{

    [SerializeField] GameObject goodLight;


    // Start is called before the first frame update
    void Start()
    {
        Assert.IsTrue(goodLight != null);

        GameObject goodNight = GameObject.FindGameObjectWithTag("TempLight");
        Assert.IsTrue(goodNight != null);
        goodNight.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
