using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI draggable handle. Works with Unity UI EventSystem. Creates drag-line while dragging.
/// When released, asks manager to check whether a slot was hit.
/// </summary>
[RequireComponent(typeof(Image))]
public class UIWireHandle : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [HideInInspector] public Color wireColor = Color.white;
    [HideInInspector] public int id = -1;

    RectTransform rt;
    Image img;
    UIMiniGameManager manager;

    // Drag visuals
    RectTransform dragLine; // an Image rect used to render the line while dragging

    // State
    Vector2 originAnchoredPos;
    bool connected = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
    }

    public void Initialize(UIMiniGameManager mgr, Color color, int myId)
    {
        manager = mgr;
        wireColor = color;
        id = myId;

        if (img != null) img.color = wireColor;
        originAnchoredPos = rt.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (connected) return;
        // create temp line UI element
        dragLine = manager.CreateTempLine();
        manager.SetLineColor(dragLine, wireColor);
        UpdateDragLine(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (connected || dragLine == null) return;
        UpdateDragLine(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (connected) return;

        // remove dragLine (we may replace with permanent on success)
        if (dragLine != null)
        {
            manager.DestroyTempLine(dragLine);
            dragLine = null;
        }

        // Ask the manager which slot (if any) is under pointer
        UIMiniGameManager.SlotRaycastResult result = manager.RaycastSlotAt(eventData);
        if (result.slot != null)
        {
            bool accepted = result.slot.TryConnect(this);
            if (accepted)
            {
                // mark connected
                connected = true;

                // hide handle image (like Among Us style) or keep it — here we hide
                img.enabled = false;

                // Snap to slot's anchored position
                rt.anchoredPosition = result.slot.RectTransform.anchoredPosition;

                // create a permanent line between origin and slot
                manager.CreatePermanentLineBetween(originAnchoredPos, result.slot.RectTransform.anchoredPosition, wireColor, transform.parent);
                manager.NotifyConnected(this, result.slot);
                return;
            }
        }

        // not connected => return to origin
        rt.anchoredPosition = originAnchoredPos;
    }

    void UpdateDragLine(PointerEventData eventData)
    {
        Vector2 localPointer;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(manager.panelRect, eventData.position, manager.canvas.worldCamera, out localPointer);
        manager.UpdateTempLine(dragLine, rt.anchoredPosition, localPointer);
    }
}
