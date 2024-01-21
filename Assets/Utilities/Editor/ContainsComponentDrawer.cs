using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ContainsComponentAttribute))]
public class ContainsComponentDrawer : PropertyDrawer
{
    private ContainsComponentAttribute Attribute => (ContainsComponentAttribute) attribute;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.serializedObject.Update();
        if (!(property.objectReferenceValue is GameObject gameObject)
            || gameObject.GetComponent(Attribute.ComponentType) == null)
        {
            property.objectReferenceValue = null;
        }
        property.serializedObject.ApplyModifiedProperties();
        EditorGUI.PropertyField(position, property, label);
    }
}