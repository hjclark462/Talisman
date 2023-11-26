using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(ManaPipe))]
[CanEditMultipleObjects]
public class ManaPipeEditor : Editor
{
    private List<SerializedProperty> properties;

    private void OnEnable()
    {
        string[] hiddenProperties = new string[] { "m_input", "m_output", "m_interactMessage", "m_bridge", "m_rumble"}; 
        properties = EditorHelper.GetExposedProperties(this.serializedObject, hiddenProperties);
    }

    public override void OnInspectorGUI()
    {        
        foreach (SerializedProperty property in properties)
        {
            EditorGUILayout.PropertyField(property, true);
        }
        serializedObject.ApplyModifiedProperties();
    }
}