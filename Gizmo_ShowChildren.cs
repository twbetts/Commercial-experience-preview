using System;
using UnityEditor;
using UnityEngine;

public class Gizmo_ShowChildren : MonoBehaviour
{
    public Color gizmoColour = Color.cyan;
    public float gizmoSize = 0.1f;
    public float drawDistance = 25f;

    [Tooltip("WireShapes may not be visible at greater distances")]
    public GizmoDrawOption drawOption = GizmoDrawOption.Sphere;

    private void OnDrawGizmosSelected()
    {
        if (SceneView.lastActiveSceneView.camera != Camera.current) return;

        Action<Vector3> draw = null;
        Gizmos.color = gizmoColour;

        switch (drawOption)
        {
            case GizmoDrawOption.Cube:
                draw = (Vector3 pos) => Gizmos.DrawCube(pos, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                break;

            case GizmoDrawOption.Sphere:
                draw = (Vector3 pos) => Gizmos.DrawSphere(pos, gizmoSize);
                break;

            case GizmoDrawOption.WireCube:
                draw = (Vector3 pos) => Gizmos.DrawWireCube(pos, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                break;

            case GizmoDrawOption.WireSphere:
                draw = (Vector3 pos) => Gizmos.DrawWireSphere(pos, gizmoSize);
                break;

            default:
                break;
        }
        
        var children = transform.GetComponentsInChildren<Transform>();
        children.Skip(1); // skips this.transform

        foreach (Transform trans in children)
        {
            float distance = Vector3.Distance(Camera.current.transform.position, trans.position);
            if (distance > drawDistance) continue;

            draw(trans.position);
        }
    }

    public enum GizmoDrawOption
    {
        Cube,
        Sphere,
        WireCube,
        WireSphere,
    }
}
