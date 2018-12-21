using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Update"))
        {
            ((TerrainGenerator)target).DebugUpdate();
        }
        if (GUILayout.Button("Save"))
        {
            var td = ((TerrainGenerator)target).debugTerrain.GetComponent<Terrain>().terrainData;
            AssetDatabase.DeleteAsset("Assets/savedTerrain.asset");
            AssetDatabase.CreateAsset(td, "Assets/savedTerrain.asset");
        }
    }
}