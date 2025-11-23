using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CamShowcase))]
public class CameraControllerEditor : Editor
{
    SerializedProperty framePrefabProp;
    SerializedProperty numberOfFramesProp;
    SerializedProperty cameraModeProp;
    SerializedProperty heightStepProp;
    SerializedProperty radiusProp;
    SerializedProperty angleStepProp;
    SerializedProperty heightOffsetProp;
    SerializedProperty lookAheadAngleProp;
    SerializedProperty spacingProp;
    SerializedProperty sideAngleProp;
    SerializedProperty sideZOffsetProp;
    SerializedProperty sideScaleProp;
    SerializedProperty scrollSpeedProp;
    SerializedProperty lerpSpeedPropl;

    private void OnEnable()
    {
        framePrefabProp = serializedObject.FindProperty("framePrefab");
        numberOfFramesProp = serializedObject.FindProperty("numberOfFrames");
        cameraModeProp = serializedObject.FindProperty("cameraMode");
        heightStepProp = serializedObject.FindProperty("heightStep");
        radiusProp = serializedObject.FindProperty("radius");
        angleStepProp = serializedObject.FindProperty("angleStep");
        heightOffsetProp = serializedObject.FindProperty("heightOffset");
        lookAheadAngleProp = serializedObject.FindProperty("lookAheadAngle");
        spacingProp = serializedObject.FindProperty("spacing");
        sideAngleProp = serializedObject.FindProperty("sideAngle");
        sideZOffsetProp = serializedObject.FindProperty("sideZOffset");
        sideScaleProp = serializedObject.FindProperty("sideScale");
        scrollSpeedProp = serializedObject.FindProperty("scrollSpeed");
        lerpSpeedPropl = serializedObject.FindProperty("lerpSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(framePrefabProp);
        EditorGUILayout.PropertyField(numberOfFramesProp);
        EditorGUILayout.PropertyField(cameraModeProp);

        CamShowcase controller = (CamShowcase)target;
        CameraMode mode = controller.cameraMode;

        EditorGUILayout.Space();

        if (mode == CameraMode.StackedTower)
        {
          //  EditorGUILayout.LabelField("Stacked Tower Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightStepProp);
            EditorGUILayout.PropertyField(radiusProp);
            EditorGUILayout.PropertyField(angleStepProp);
            EditorGUILayout.PropertyField(heightOffsetProp);
            EditorGUILayout.PropertyField(lookAheadAngleProp);
        }
        else if (mode == CameraMode.CoverFlow)
        {
            // EditorGUILayout.LabelField("Cover Flow Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spacingProp);
            EditorGUILayout.PropertyField(sideAngleProp);
            EditorGUILayout.PropertyField(sideZOffsetProp);
            EditorGUILayout.PropertyField(sideScaleProp);
            EditorGUILayout.PropertyField(scrollSpeedProp);
            EditorGUILayout.PropertyField(lerpSpeedPropl);
        }

        // Apply changes and call OnValidate if anything changed
        if (serializedObject.ApplyModifiedProperties())
        {
            controller.OnValidate();
        }
    }
}
