using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DumpObj), false)]
public class DumpObjEditor : Editor
{
    SerializedProperty _list_components;

    void OnEnable()
    {
        _list_components = serializedObject.FindProperty("_lst_components");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        int size = _list_components.arraySize;
        for (int i = 0; i < size; i++)
        {
            SerializedProperty obj = _list_components.GetArrayElementAtIndex(i);
            SerializedProperty name_property = obj.FindPropertyRelative("name");
            SerializedProperty active_property = obj.FindPropertyRelative("active");

            bool pre_state = active_property.boolValue;
            active_property.boolValue = EditorGUILayout.Toggle(name_property.stringValue, active_property.boolValue);

            if (pre_state != active_property.boolValue)
            {
                DumpObj dump = (DumpObj)serializedObject.targetObject;
                dump.OnBehaviorChange(name_property.stringValue, active_property.boolValue);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
