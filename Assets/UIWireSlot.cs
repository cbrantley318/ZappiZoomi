using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIWireSlot : MonoBehaviour
{
    public Color requiredColor = Color.white;
    public int id = -1;
    public bool IsOccupied { get; private set; } = false;

    Image img;
    RectTransform rt;
    public RectTransform RectTransform => rt;

    void Awake()
    {
        img = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
    }

    public void Initialize(Color color, int myId)
    {
        requiredColor = color;
        id = myId;
        IsOccupied = false;
        if (img != null)
        {
            img.color = requiredColor * 0.65f;
        }
    }

    public bool TryConnect(UIWireHandle handle)
    {
        if (IsOccupied) return false;
        if (AreColorsEqual(handle.wireColor, requiredColor, 0.02f))
        {
            IsOccupied = true;
            if (img != null) img.color = requiredColor;
            return true;
        }
        return false;
    }

    bool AreColorsEqual(Color a, Color b, float tol)
    {
        return Mathf.Abs(a.r - b.r) < tol &&
               Mathf.Abs(a.g - b.g) < tol &&
               Mathf.Abs(a.b - b.b) < tol;
    }
}
