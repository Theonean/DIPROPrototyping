using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralTileGenerator))]
public class TileMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //or DrawDefaultInspector();

        if(GUILayout.Button("Regenerate"))
        {
            ProceduralTileGenerator tileGenerator = (ProceduralTileGenerator)target;
            tileGenerator.BuildMesh();
        }
    }
}
