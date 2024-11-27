using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilemapCompressor))]
public class TilemapCompressorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Compress Tilemap"))
        {
            TilemapCompressor t = target as TilemapCompressor;
            t.CompressTilemap();
        }
    }
}
