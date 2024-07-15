using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WanderRadialBounds : MonoBehaviour
{
    public float radius;
    public Color color = new Color(1, 1, 1, 0.8f);

    public bool InBounds(Vector3 pos)
    {
        return Vector3.Distance(transform.position, pos) < radius;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = color;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, radius, 4);
    }
#endif
}
