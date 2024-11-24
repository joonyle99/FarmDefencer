using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridMap))]
public class GridMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Compress Tilemap"))
        {
            GridMap t = target as GridMap;
            t.CompressTilemap();
        }
    }
}
