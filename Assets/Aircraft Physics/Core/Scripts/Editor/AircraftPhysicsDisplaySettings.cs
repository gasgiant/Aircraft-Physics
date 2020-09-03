using UnityEngine;

//[CreateAssetMenu(fileName = "AircraftPhysicsDisplaySettings", menuName = "AircraftPhysicsDisplaySettings")]
public class AircraftPhysicsDisplaySettings : ScriptableObject
{
    private static AircraftPhysicsDisplaySettings displaySettings;
    public static AircraftPhysicsDisplaySettings Instance
    {
        get
        {
            if (displaySettings == null)
            {
                displaySettings = Load();
            }
            return displaySettings;
        }
    }

    [Header("Scaling")]
    public bool scaleForcesByWeight = true;
    public float lengthScale = 1;
    public float widthScale = 1;

    [Header("Center of mass")]
    public bool showCenterOfMass = true;
    public Color comColor = new Color(1.000f, 0.445f, 0.000f, 1.000f);

    [Header("Aerodynamic center")]
    public bool showAerodynamicCenter = true;
    public Color adcColor = new Color(0.373f, 0.682f, 1.000f, 1.000f);
    public float displayAngleOfAttack = 5;
    public float displayAirspeed = 100;
    public float displayAirDensity = 1.2f;

    [Header("Surfaces")]
    public bool showSurfaces = true;
    public Color wingColor = new Color(0.199f, 0.254f, 0.981f, 0.235f);
    public Color flapColor = new Color(1.000f, 0.633f, 0.241f, 0.325f);
    public Color wingAtStallColor = new Color(0.632f, 0.051f, 0.180f, 0.353f);
    public Color flapAtStallColor = new Color(1.000f, 0.078f, 0.181f, 0.612f);

    [Header("ForcesOnSurfaces")]
    public bool showForces = true;
    public bool showTorque = true;
    public Color liftColor = new Color(0.373f, 0.682f, 1.000f, 0.949f);
    public Color dragColor = new Color(0.679f, 0.000f, 0.095f, 0.922f);
    public Color torqColor = new Color(0.220f, 0.679f, 0.000f, 0.700f);


#if UNITY_EDITOR
    public static AircraftPhysicsDisplaySettings Load()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AircraftPhysicsDisplaySettings");
        if (guids.Length == 0)
        {
            Debug.LogWarning("Could not find AircraftPhysicsDisplaySettings asset. Will use default settings instead.");
            return CreateInstance<AircraftPhysicsDisplaySettings>();
        }
        else
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AircraftPhysicsDisplaySettings>(path);
        }
    }
#endif
}

