using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI-based wire mini-game manager for the Canvas popup.
/// Instantiates UIWireHandle and UIWireSlot prefabs inside left/right containers,
/// creates temporary drag lines and permanent connection lines using UI Images.
/// </summary>
public class UIMiniGameManager : MonoBehaviour
{
    [Header("Prefabs & UI references")]
    public GameObject wireHandlePrefab; // UI prefab (Image + UIWireHandle)
    public GameObject wireSlotPrefab;   // UI prefab (Image + UIWireSlot)
    public RectTransform leftContainer; // child of panel
    public RectTransform rightContainer;
    public RectTransform panelRect;     // the central panel rect transform (for pointer coord conversions)
    public Canvas canvas;               // the Canvas in this prefab (set to Screen Space - Overlay)
    public GraphicRaycaster graphicRaycaster; // assigned from Canvas
    public EventSystem eventSystem;     // reference to scene EventSystem (if null try to auto find)

    [Header("Layout")]
    public float verticalSpacing = 100f; // in anchored units (UI pixels)
    public Color[] colorOptions = new Color[] { Color.red, Color.green, Color.blue };
    public int pairCount = 3;

    // runtime
    List<UIWireHandle> handles = new List<UIWireHandle>();
    List<UIWireSlot> slots = new List<UIWireSlot>();
    PlayerScript playerRef;
    System.Action onCompleteCallback;

    // temp lines parent
    RectTransform tempLinesParent;
    RectTransform permanentLinesParent;

    // cached white sprite used for generated Image elements
    private Sprite _whiteSprite = null;

    void Awake()
    {
        if (eventSystem == null) eventSystem = EventSystem.current;
        if (graphicRaycaster == null && canvas != null) graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

        // create parents for lines under the panel
        if (panelRect == null)
        {
            Debug.LogError("UIMiniGameManager: panelRect is not assigned.");
        }
        else
        {
            GameObject tempGO = new GameObject("TempLines", typeof(RectTransform));
            tempGO.transform.SetParent(panelRect, false);
            tempLinesParent = tempGO.GetComponent<RectTransform>();
            tempLinesParent.SetAsLastSibling();

            GameObject permGO = new GameObject("PermLines", typeof(RectTransform));
            permGO.transform.SetParent(panelRect, false);
            permanentLinesParent = permGO.GetComponent<RectTransform>();
            permanentLinesParent.SetAsLastSibling();
        }

        // create a simple white sprite from the built-in white texture (once)
        if (_whiteSprite == null)
        {
            Texture2D t = Texture2D.whiteTexture;
            _whiteSprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        }
    }

    /// <summary>
    /// Configure initial colors and pairs.
    /// </summary>
    public void Configure(Color[] colors, int pairs)
    {
        if (colors != null && colors.Length > 0) colorOptions = colors;
        pairCount = Mathf.Clamp(pairs, 1, colorOptions.Length);
    }

    /// <summary>
    /// Start: lock player controls and populate UI.
    /// </summary>
    public void StartGame(PlayerScript player, System.Action onComplete)
    {
        playerRef = player;
        onCompleteCallback = onComplete;
        if (playerRef != null) playerRef.SetControlLocked(true);

        Populate();
    }

    void Populate()
    {
        if (leftContainer == null || rightContainer == null)
        {
            Debug.LogError("UIMiniGameManager: leftContainer or rightContainer not assigned.");
            return;
        }

        // clear existing children in containers
        foreach (Transform t in leftContainer) Destroy(t.gameObject);
        foreach (Transform t in rightContainer) Destroy(t.gameObject);
        handles.Clear(); slots.Clear();

        // choose and shuffle colors
        List<Color> chosen = new List<Color>(colorOptions);
        for (int i = 0; i < chosen.Count; i++)
        {
            int r = Random.Range(i, chosen.Count);
            var tmp = chosen[i]; chosen[i] = chosen[r]; chosen[r] = tmp;
        }
        chosen = chosen.GetRange(0, pairCount);

        // left handles ordered top->down
        for (int i = 0; i < pairCount; i++)
        {
            Vector2 anchored = new Vector2(0f, -i * verticalSpacing);
            GameObject go = Instantiate(wireHandlePrefab, leftContainer);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchored;
            UIWireHandle wh = go.GetComponent<UIWireHandle>();
            wh.Initialize(this, chosen[i], i);
            handles.Add(wh);
        }

        // right slots shuffled
        List<int> idx = new List<int>();
        for (int i = 0; i < pairCount; i++) idx.Add(i);
        for (int i = 0; i < idx.Count; i++)
        {
            int r = Random.Range(i, idx.Count);
            int tmp = idx[i]; idx[i] = idx[r]; idx[r] = tmp;
        }

        for (int i = 0; i < pairCount; i++)
        {
            Vector2 anchored = new Vector2(0f, -i * verticalSpacing);
            GameObject go = Instantiate(wireSlotPrefab, rightContainer);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchored;
            UIWireSlot ws = go.GetComponent<UIWireSlot>();
            ws.Initialize(chosen[idx[i]], idx[i]);
            slots.Add(ws);
        }
    }

    #region TempLine helpers (dragging)
    /// <summary>
    /// Creates a temporary UI Image (white pixel) used as a line.
    /// </summary>
    public RectTransform CreateTempLine()
    {
        if (panelRect == null || tempLinesParent == null)
        {
            Debug.LogWarning("CreateTempLine: panelRect/tempLinesParent is null.");
            return null;
        }

        GameObject go = new GameObject("tempLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(tempLinesParent, false);
        Image img = go.GetComponent<Image>();
        img.raycastTarget = false; // don't block UI events

        if (_whiteSprite != null) img.sprite = _whiteSprite;
        img.type = Image.Type.Sliced;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        return rt;
    }

    public void DestroyTempLine(RectTransform rt)
    {
        if (rt != null) Destroy(rt.gameObject);
    }

    /// <summary>
    /// Update a UI Image rect to cover between two anchored positions within panelRect (panel local coords).
    /// anchoredA and anchoredB should be local anchored positions (RectTransform.anchoredPosition).
    /// </summary>
    public void UpdateTempLine(RectTransform rt, Vector2 anchoredA, Vector2 anchoredB)
    {
        if (rt == null) return;
        Vector2 diff = anchoredB - anchoredA;
        float dist = diff.magnitude;
        rt.sizeDelta = new Vector2(dist, 8f); // thickness 8 px (tweak)
        Vector2 mid = (anchoredA + anchoredB) * 0.5f;
        rt.anchoredPosition = mid;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        rt.localEulerAngles = new Vector3(0, 0, angle);
    }
    #endregion

    #region Permanent Line creation
    /// <summary>
    /// Create a permanent line between two anchored positions. parentTransform is optional for organizing.
    /// </summary>
    public RectTransform CreatePermanentLineBetween(Vector2 anchoredA, Vector2 anchoredB, Color color, Transform parentForHandles = null)
    {
        if (panelRect == null || permanentLinesParent == null)
        {
            Debug.LogWarning("CreatePermanentLineBetween: panelRect/permanentLinesParent is null.");
            return null;
        }

        GameObject go = new GameObject("permLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(permanentLinesParent, false);
        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;
        if (_whiteSprite != null) img.sprite = _whiteSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 diff = anchoredB - anchoredA;
        float dist = diff.magnitude;
        rt.sizeDelta = new Vector2(dist, 8f); // thickness 8 px
        Vector2 mid = (anchoredA + anchoredB) * 0.5f;
        rt.anchoredPosition = mid;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        rt.localEulerAngles = new Vector3(0, 0, angle);

        return rt;
    }
    #endregion

    /// <summary>
    /// Raycast UI to find a slot under pointer. Uses GraphicRaycaster and EventSystem.
    /// </summary>
    public struct SlotRaycastResult { public UIWireSlot slot; public RaycastResult rawResult; }

    public SlotRaycastResult RaycastSlotAt(PointerEventData pointerEvent)
    {
        SlotRaycastResult outRes = new SlotRaycastResult { slot = null };
        if (graphicRaycaster == null || eventSystem == null) return outRes;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEvent, results);
        foreach (var r in results)
        {
            GameObject go = r.gameObject;
            UIWireSlot s = go.GetComponent<UIWireSlot>();
            if (s != null) { outRes.slot = s; outRes.rawResult = r; return outRes; }
            s = go.GetComponentInParent<UIWireSlot>();
            if (s != null) { outRes.slot = s; outRes.rawResult = r; return outRes; }
        }
        return outRes;
    }

    /// <summary>
    /// Called by handle when connected to slot
    /// </summary>
    public void NotifyConnected(UIWireHandle handle, UIWireSlot slot)
    {
        // check all slots filled
        foreach (var s in slots) if (!s.IsOccupied) return;
        StartCoroutine(SuccessCoroutine());
    }

    IEnumerator SuccessCoroutine()
    {
        yield return new WaitForSeconds(0.35f);
        if (playerRef != null) playerRef.SetControlLocked(false);
        onCompleteCallback?.Invoke();
        Destroy(gameObject); // destroy canvas prefab
    }

    /// <summary>
    /// Raycast helper: create PointerEventData for current pointer position.
    /// </summary>
    public PointerEventData CreatePointerEventForScreenPos(Vector2 screenPos)
    {
        if (eventSystem == null) eventSystem = EventSystem.current;
        PointerEventData pd = new PointerEventData(eventSystem);
        pd.position = screenPos;
        return pd;
    }

    /// <summary>
    /// Convenience to set color on a temp line's Image (rt contains Image).
    /// </summary>
    public void SetLineColor(RectTransform rt, Color c)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img != null) img.color = c;
    }
}
