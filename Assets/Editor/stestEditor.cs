using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(stest))]
[CanEditMultipleObjects]
class stestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Update", GUILayout.Height(50)))
        {
            foreach (var targ in targets)
            {
                ((stest)targ).UpdateButton();
            }
        }
        DrawDefaultInspector();
        if (GUILayout.Button("Update", GUILayout.Height(50)))
        {
            foreach (var targ in targets)
            {
                ((stest)targ).UpdateButton();
            }
        }
    }
}