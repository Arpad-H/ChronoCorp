using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        // Get the actual target object
        object targetObject = property.serializedObject.targetObject;
        FieldInfo enumField = targetObject.GetType().GetField(showIf.enumFieldName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (enumField == null)
        {
            EditorGUI.LabelField(position, $"Missing enum: {showIf.enumFieldName}");
            return;
        }

        // Get the enum value
        int enumValue = (int)enumField.GetValue(targetObject);

        if (enumValue == showIf.enumValue)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        object targetObject = property.serializedObject.targetObject;
        FieldInfo enumField = targetObject.GetType().GetField(showIf.enumFieldName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (enumField == null) return EditorGUIUtility.singleLineHeight;

        int enumValue = (int)enumField.GetValue(targetObject);

        return (enumValue == showIf.enumValue) 
            ? EditorGUI.GetPropertyHeight(property, label, true) 
            : 0;
    }
}