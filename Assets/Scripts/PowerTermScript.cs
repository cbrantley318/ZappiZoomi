using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerTermScript : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector] public bool poweredOn;
    [HideInInspector] public GameObject CurrentWire;
    [SerializeField] bool isButton;             //should be false for everything except (except what? hmmm?)
    [SerializeField] Color termColor;           //TODO: add in multiple colors

    [SerializeField] Material poweredOnMat;     //for fun lighting effects
    [SerializeField] Material poweredOffMat;

    private LineRenderer powerLine;
    private SpriteRenderer fgSprite;

    void Start()
    {
        poweredOn = false;
        //set the wire line and the terminal FG color to the active color (Red)
        //TODO: make a colors enum/lookup-table using only the allowed in-game colors instead of relying on us to set the color in unity manually
        fgSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        powerLine = GetComponent<LineRenderer>();

        fgSprite.color = termColor;
        powerLine.startColor = termColor;  //this would've been so helpful to know in my solo game
        powerLine.endColor = termColor;

        fgSprite.material = poweredOffMat;
        powerLine.material = poweredOffMat;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PowerOn()
    {
        poweredOn = true;
        fgSprite.material = poweredOnMat;
        powerLine.material = poweredOnMat;
    }

    public void PowerOff()
    {
        poweredOn = false;
        fgSprite.material = poweredOffMat;
        powerLine.material = poweredOffMat;
    }

    public Color GetColor()
    {
        return termColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
            player.ActiveWireTerminal = gameObject;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
            player.ActiveWireTerminal = null;
        }
    }
}
