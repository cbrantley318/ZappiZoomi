using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WorldWire : MonoBehaviour
{
    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = 0.06f;
        lr.numCapVertices = 8;
        lr.useWorldSpace = true;

        // optional defaults
        lr.startColor = lr.endColor = Color.yellow;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    /// <summary>
    /// Draws a straight line between two world points.
    /// </summary>
    public void Setup(Vector3 worldA, Vector3 worldB, Color color, float width = 0.06f)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, worldA);
        lr.SetPosition(1, worldB);
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = width;
    }

    /// <summary>
    /// Optional curved setup (adds slight sag in Y for a hanging-wire look).
    /// </summary>
    public void SetupCurved(Vector3 worldA, Vector3 worldB, Color color, float width = 0.06f, float sag = 0.1f)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        int points = 12;
        lr.positionCount = points;
        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            Vector3 p = Vector3.Lerp(worldA, worldB, t);
            p.y -= Mathf.Sin(t * Mathf.PI) * sag;
            lr.SetPosition(i, p);
        }
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = width;
    }
}
