using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class EnemySpawnPatternEditorWindow : EditorWindow
{
    private const int GridSize = 10;
    private const int CellSize = 40; // For scaling the pixels
    private EnemyType[,] grid = new EnemyType[GridSize, GridSize];
    private EnemyType selectedType = EnemyType.NONE;

    [MenuItem("Tools/Enemy Spawn Pattern Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnemySpawnPatternEditorWindow>("Spawn Pattern Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Enemy Type", EditorStyles.boldLabel);
        selectedType = (EnemyType)EditorGUILayout.EnumPopup("Paint With", selectedType);

        GUILayout.Space(10);

        for (int y = 0; y < GridSize; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < GridSize; x++)
            {
                Color color = GetColorForType(grid[x, y]);
                GUI.backgroundColor = color;

                if (GUILayout.Button("", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                {
                    grid[x, y] = selectedType;
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Save Pattern"))
        {
            SaveSpawnPattern();
        }
    }

    private Color GetColorForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.REGULAR: return Color.yellow;
            case EnemyType.CHARGER: return Color.green;
            case EnemyType.CRABTANK: return Color.blue;
            default: return Color.black;
        }
    }

    private void SaveSpawnPattern()
    {
        var spawnPattern = ScriptableObject.CreateInstance<SOEnemySpawnPattern>();

        float spacing = 1.5f;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                EnemyType type = grid[x, y];

                // Optional: Skip empty
                if (!IsActuallySet(x, y)) continue;

                Vector3 position = new Vector3(x * spacing, 0f, y * spacing);
                spawnPattern.spawnPositions.Add(new EnemySpawnPosition(position, type));
            }
        }

        string folderPath = "Assets/SpawnPatterns";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/EnemySpawnPattern.asset");
        AssetDatabase.CreateAsset(spawnPattern, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Spawn Pattern saved to {assetPath}");
    }

    // Optional helper to avoid placing REGULARs everywhere by default
    private bool IsActuallySet(int x, int y)
    {
        return grid[x, y] != EnemyType.NONE;
    }
}
