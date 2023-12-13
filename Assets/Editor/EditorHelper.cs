using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

public static class EditorHelper
{
    //Static function to be utilised by the scripts to edit their inspectors to hide unnecessary values

    public static List<SerializedProperty> GetExposedProperties(SerializedObject so, IEnumerable<string> namesToHide = null)
    {
        if (namesToHide == null) namesToHide = new string[] { };
        IEnumerable<FieldInfo> componentFields = so.targetObject.GetType().GetFields();
        List<SerializedProperty> exposedFields = new List<SerializedProperty>();

        foreach (FieldInfo info in componentFields)
        {
            bool displayInInspector = info.IsPublic && !Attribute.IsDefined(info, typeof(HideInInspector));
            displayInInspector = displayInInspector || (info.IsPrivate && Attribute.IsDefined(info, typeof(SerializeField)));
            displayInInspector = displayInInspector && !namesToHide.Contains(info.Name);

            if (displayInInspector)
            {
                SerializedProperty prop = so.FindProperty(info.Name);
                if (prop != null)
                    exposedFields.Add(prop);
            }
        }

        return exposedFields;
    }
}