#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class AircraftGizmos
{
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    public static void AircraftPhysicsGizmos(AircraftPhysics phys, GizmoType gizmoType)
    {
        AircraftPhysicsDisplaySettings settings = AircraftPhysicsDisplaySettings.Instance;
        Vector3 weight = phys.GetComponent<Rigidbody>().mass * Physics.gravity;
        float scale = settings.lengthScale;
        if (settings.scaleForcesByWeight)
            scale /= weight.magnitude;

        if (settings.showCenterOfMass)
        {
            Gizmos.color = settings.comColor;
            Vector3 com = phys.GetComponent<Rigidbody>().worldCenterOfMass;
            Gizmos.DrawWireSphere(com, 0.3f * settings.widthScale);
            DrawThinArrow(com, weight * scale, settings.comColor, 0.4f * settings.widthScale, 3);
        }

        Vector3 airspeed = new Vector3(0,
                Mathf.Sin(Mathf.Deg2Rad * settings.displayAngleOfAttack),
                -Mathf.Cos(Mathf.Deg2Rad * settings.displayAngleOfAttack))
                * settings.displayAirspeed;
        Vector3 center;
        Vector3 force;
        phys.CalculateCenterOfLift(out center, out force,
            phys.transform.TransformDirection(airspeed), settings.displayAirDensity);

        if (settings.showAerodynamicCenter)
        {
            Gizmos.color = settings.adcColor;
            Gizmos.DrawWireSphere(center, 0.3f * settings.widthScale);
            DrawThinArrow(center, force * scale, settings.adcColor, 0.4f * settings.widthScale, 3);
        }
    }

    public static void PrintColor(Color color)
    {
        Debug.Log("new Color(" + color.r.ToString("F3").Replace(',', '.') + "f, " + color.g.ToString("F3").Replace(',', '.') + "f, " + color.b.ToString("F3").Replace(',', '.') + "f, " + color.a.ToString("F3").Replace(',', '.') + "f);");
    }


    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
    public static void SurfaceGizmos(AeroSurface surf, GizmoType gizmoType)
    {
        Rigidbody rb = surf.GetComponentInParent<Rigidbody>();
        if (surf.Config == null || rb == null) return;
        AircraftPhysicsDisplaySettings settings = AircraftPhysicsDisplaySettings.Instance;

        // Selection shape
        if (settings.showSurfaces)
        {
            Gizmos.color = Color.clear;
            Gizmos.matrix = surf.transform.localToWorldMatrix;
            Gizmos.DrawCube(-Vector3.right * 0.25f * surf.Config.chord, new Vector3(surf.Config.chord, 0.1f, surf.Config.span));

            DrawSurface(surf.transform, surf.Config, surf.GetFlapAngle(), surf.IsAtStall);
        }

        if (settings.showForces)
        {
            float scale = settings.lengthScale;
            if (settings.scaleForcesByWeight)
            {
                scale /= rb.mass * Physics.gravity.magnitude;
            }
            DrawForces(surf.transform, surf.CurrentLift * scale, surf.CurrentDrag * scale, surf.CurrentTorque * scale);
        }
    }

    private static void DrawSurface(Transform transform, AeroSurfaceConfig config, float flapAngle, bool isAtStall = false)
    {
        AircraftPhysicsDisplaySettings settings = AircraftPhysicsDisplaySettings.Instance;
        float mainChord = config.chord * (1 - config.flapFraction);
        float flapChord = config.chord * config.flapFraction;

        DrawRectangle(transform.position + transform.right * (0.25f * config.chord - 0.5f * mainChord),
                transform.rotation,
                config.span,
                mainChord,
                isAtStall ? settings.wingAtStallColor : settings.wingColor);

        if (config.flapFraction > 0)
        {
            DrawRectangle(transform.position
                + transform.right * (0.25f * config.chord - mainChord - 0.02f - 0.5f * flapChord * Mathf.Cos(flapAngle))
                - transform.up * 0.5f * Mathf.Sin(flapAngle) * flapChord,
                    transform.rotation * Quaternion.AngleAxis(flapAngle * Mathf.Rad2Deg, Vector3.forward),
                    config.span,
                    flapChord,
                    isAtStall ? settings.flapAtStallColor : settings.flapColor);
        }
    }

    private static void DrawForces(Transform transform, Vector3 lift, Vector3 drag, Vector3 torq)
    {
        AircraftPhysicsDisplaySettings settings = AircraftPhysicsDisplaySettings.Instance;

        DrawArrow(transform.position, drag * 1, settings.dragColor, 0.08f * settings.widthScale);
        DrawArrow(transform.position, lift * 1, settings.liftColor, 0.08f * settings.widthScale);
        if (settings.showTorque)
        {
            DrawArrow(transform.position, torq * 1, settings.torqColor, 0.08f * settings.widthScale);
        }
    }

    private static void DrawArrow(Vector3 position, Vector3 vector, Color color, float width)
    {
        Vector3 cross = Vector3.Cross(vector.normalized, Camera.current.transform.forward).normalized;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Vector3[] vertices = new Vector3[4];
        vertices[0] = position - cross * width;
        vertices[1] = position + vector - cross * width * 0.25f;
        vertices[2] = position + vector + cross * width * 0.25f;
        vertices[3] = position + cross * width;
        Handles.color = color;
        Handles.DrawSolidRectangleWithOutline(vertices, color, color * 1.5f);
    }

    private static void DrawThinArrow(Vector3 position, Vector3 vector, Color color, float headSize, float width)
    {
        Vector3 vn = vector.normalized;
        Vector3 cross = Vector3.Cross(vn, Camera.current.transform.forward).normalized;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = color;
        Handles.DrawAAPolyLine(width, position, position + vector);
        Handles.DrawAAPolyLine(width, position + vector, position + vector - vn * headSize + cross * headSize * 0.25f);
        Handles.DrawAAPolyLine(width, position + vector, position + vector - vn * headSize - cross * headSize * 0.25f);
    }

    private static void DrawRectangle(Vector3 position, Quaternion rotation, float width, float height, Color color)
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(position,
                rotation,
                width,
                height),
            color,
            Color.black);
    }

    private static Vector3[] GetRectangleVertices(Vector3 position, Quaternion rotation, float width, float height)
    {
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(height, 0, -width) * 0.5f;
        vertices[1] = new Vector3(height, 0, width) * 0.5f;
        vertices[2] = new Vector3(-height, 0, width) * 0.5f;
        vertices[3] = new Vector3(-height, 0, -width) * 0.5f;
        for (int i = 0; i < 4; i++)
        {
            vertices[i] = rotation * vertices[i] + position;
        }
        return vertices;
    }
}
#endif
