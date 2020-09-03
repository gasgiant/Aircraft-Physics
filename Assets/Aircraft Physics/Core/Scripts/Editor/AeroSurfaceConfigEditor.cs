using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AeroSurfaceConfig)), CanEditMultipleObjects()]
public class AeroSurfaceConfigEditor : Editor
{
    SerializedProperty liftSlope;
    SerializedProperty skinFriction;
    SerializedProperty zeroLiftAoA;
    SerializedProperty stallAngleHigh;
    SerializedProperty stallAngleLow;
    SerializedProperty chord;
    SerializedProperty flapFraction;
    SerializedProperty span;
    SerializedProperty autoAspectRatio;
    SerializedProperty aspectRatio;
    AeroSurfaceConfig config;

    private void OnEnable()
    {
        liftSlope = serializedObject.FindProperty("liftSlope");
        skinFriction = serializedObject.FindProperty("skinFriction");
        zeroLiftAoA = serializedObject.FindProperty("zeroLiftAoA");
        stallAngleHigh = serializedObject.FindProperty("stallAngleHigh");
        stallAngleLow = serializedObject.FindProperty("stallAngleLow");
        chord = serializedObject.FindProperty("chord");
        flapFraction = serializedObject.FindProperty("flapFraction");
        span = serializedObject.FindProperty("span");
        autoAspectRatio = serializedObject.FindProperty("autoAspectRatio");
        aspectRatio = serializedObject.FindProperty("aspectRatio");
        config = target as AeroSurfaceConfig;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(liftSlope);
        EditorGUILayout.PropertyField(skinFriction);
        EditorGUILayout.PropertyField(zeroLiftAoA);
        EditorGUILayout.PropertyField(stallAngleHigh);
        EditorGUILayout.PropertyField(stallAngleLow);
        EditorGUILayout.PropertyField(chord);
        EditorGUILayout.PropertyField(flapFraction);
        EditorGUILayout.PropertyField(span);
        EditorGUILayout.PropertyField(autoAspectRatio);
        if (config.autoAspectRatio)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(aspectRatio);
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.PropertyField(aspectRatio);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
