using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(TutorialRotateMana))]
[CanEditMultipleObjects]
public class TutorialRotateManaEditor: Editor
{
    private List<SerializedProperty> properties;

    private void OnEnable()
    {
        string[] hiddenProperties = new string[] { "m_rewindMana", "m_bridge", "m_input", "m_output"}; 
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