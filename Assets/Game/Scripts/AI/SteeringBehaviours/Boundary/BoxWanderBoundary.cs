using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoxWanderBoundary : WanderBoundary
{
    public Vector2 size;
    public Color color = new Color(1, 1, 1, 0.8f);

    public override bool InBounds(Vector3 pos)
    {
        Vector3 localPos = transform.InverseTransformPoint(pos);
        Vector2 halfSize = size * 0.5f;
        return Mathf.Abs(localPos.x) < halfSize.x && Mathf.Abs(localPos.z) < halfSize.y;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = color;
        Vector3 center = transform.position;
        Vector3 halfSize = new Vector3(size.x / 2, 0, size.y / 2);

        Vector3[] corners = new Vector3[4];
        corners[0] = center + new Vector3(-halfSize.x, 0, -halfSize.z); // Bottom left
        corners[1] = center + new Vector3(-halfSize.x, 0, halfSize.z);  // Top left
        corners[2] = center + new Vector3(halfSize.x, 0, halfSize.z);   // Top right
        corners[3] = center + new Vector3(halfSize.x, 0, -halfSize.z);  // Bottom right

        Handles.DrawAAPolyLine(10, corners[0], corners[1], corners[2], corners[3], corners[0]);
    }
#endif
}
