using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
class DumpComponent
{
    public string name;
    public bool active;
}

public class DumpObj : MonoBehaviour
{
    bool _active = false;

    [SerializeField]
    List<DumpComponent> _lst_components = new List<DumpComponent>();

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

        GameObject[] activeObjs = Selection.gameObjects;
        if (activeObjs.ToList().Contains(gameObject))
        {
            if (_on_state_change != null && _active != gameObject.activeSelf)
            {
                _on_state_change(gameObject, null);
            }

            _active = gameObject.activeSelf;
        }
    }

    public void OnBehaviorChange(string name, bool active)
    {
        _on_behavior_change(gameObject, name, active);
    }

    Action<UnityEngine.GameObject, Node> _on_state_change;
    Action<GameObject, string, bool> _on_behavior_change;
    internal void Init(bool active, Action<UnityEngine.GameObject, Node> onStateChange, Action<GameObject, string, bool> onBehaviorChange)
    {
        _active = active;
        _on_state_change = onStateChange;
        _on_behavior_change = onBehaviorChange;
    }

    internal void AddComp(string name, bool active)
    {
        _lst_components.Add(new DumpComponent()
        {
            name = name,
            active = active
        });
    }
}
