using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DumpObj : MonoBehaviour
{
    bool _active = false;

    DumpObj()
    {
        EditorApplication.hierarchyChanged -= MyHierarchyChangedCallback;
        EditorApplication.hierarchyChanged += MyHierarchyChangedCallback;
    }

    private void MyHierarchyChangedCallback()
    {
        if (this == null)
        {
            return;
        }

        GameObject activeObjs = Selection.activeGameObject;
        if (activeObjs == gameObject)
        {
            if (_on_state_change != null && _active != gameObject.activeSelf)
            {
                _on_state_change(gameObject);
            }

            _active = gameObject.activeSelf;
        }
    }

    Action<GameObject> _on_state_change;
    internal void Init(bool active, Action<GameObject> onStateChange)
    {
        _active = active;
        _on_state_change = onStateChange;
    }


}
