using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class EnemySpawnPatternEditorWindow : EditorWindow
{
    private bool showPreviewInScene = true;
    private Vector3 previewOrigin = Vector3.zero;

    private string inputName = "Falcon";
    private Vector2Int gridSize = new Vector2Int(3, 3);
    private Vector2Int previousGridSize;
    private const int CellSize = 40;
    private EnemyType[,] grid;
    private EnemyType selectedType = EnemyType.NONE;

    [MenuItem("Tools/Enemy Spawn Pattern Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnemySpawnPatternEditorWindow>("Spawn Pattern Editor");
    }
    private void OnEnable()
    {
        InitializeGrid(gridSize);
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Enemy Spawn Pattern Editor", EditorStyles.boldLabel);

        inputName = EditorGUILayout.TextField("Pattern Name", inputName);

        Vector2Int newSize = EditorGUILayout.Vector2IntField("Grid Size", gridSize);
        if (newSize != gridSize)
        {
            ResizeGrid(newSize);
        }

        selectedType = (EnemyType)EditorGUILayout.EnumPopup("Paint With", selectedType);
        
        GUILayout.Space(10);
        
        showPreviewInScene = EditorGUILayout.Toggle("Show Scene Preview", showPreviewInScene);
        previewOrigin = EditorGUILayout.Vector3Field("Preview Origin", previewOrigin);

        GUILayout.Space(10);

        // Draw grid
        for (int y = 0; y < gridSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridSize.x; x++)
            {
                GUI.backgroundColor = GetColorForType(grid[x, y]);
                if (GUILayout.Button("", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                {
                    grid[x, y] = selectedType;
                }
                HandleEraser(x, y);

                Rect cellRect = GUILayoutUtility.GetLastRect();
                HandleClickDragPainting(cellRect, x, y);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Save Pattern"))
        {
            SaveSpawnPattern();
        }
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!showPreviewInScene || grid == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        float spacing = 1.5f;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                EnemyType type = grid[x, y];
                if (type == EnemyType.NONE) continue;

                Vector3 pos = previewOrigin + new Vector3(x * spacing, 0, y * spacing);
                Handles.color = GetColorForType(type);
                Handles.DrawWireCube(pos, Vector3.one * 1.2f);

                GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    normal = { textColor = Handles.color },
                    alignment = TextAnchor.MiddleCenter
                };

                Handles.Label(pos + Vector3.up * 0.6f, type.ToString(), labelStyle);
            }
        }
    }


    private void InitializeGrid(Vector2Int size)
    {
        gridSize = size;
        grid = new EnemyType[size.x, size.y];
    }

    private void ResizeGrid(Vector2Int newSize)
    {
        var newGrid = new EnemyType[newSize.x, newSize.y];

        for (int y = 0; y < Mathf.Min(newSize.y, gridSize.y); y++)
        {
            for (int x = 0; x < Mathf.Min(newSize.x, gridSize.x); x++)
            {
                newGrid[x, y] = grid[x, y];
            }
        }

        grid = newGrid;
        gridSize = newSize;
    }

    private Color GetColorForType(EnemyType type)
    {
        return type switch
        {
            EnemyType.REGULAR => Color.yellow,
            EnemyType.CHARGER => Color.green,
            EnemyType.CRABTANK => Color.blue,
            _ => Color.black,
        };
    }

    private void SaveSpawnPattern()
    {
        var spawnPattern = ScriptableObject.CreateInstance<SOEnemySpawnPattern>();
        float spacing = 1.5f;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (!IsActuallySet(x, y)) continue;
                Vector3 pos = new Vector3(x * spacing, 0f, y * spacing);
                spawnPattern.spawnPositions.Add(new EnemySpawnPosition(pos, grid[x, y]));
            }
        }

        string folderPath = "Assets/BlightProtocol/Characters/Enemies/SpawnPatterns";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string cleanName = string.IsNullOrEmpty(inputName) ? "EnemySpawnPattern" : inputName;
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{cleanName}.asset");

        AssetDatabase.CreateAsset(spawnPattern, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Spawn Pattern '{cleanName}' saved to {assetPath}");
    }

    private bool IsActuallySet(int x, int y)
    {
        return grid[x, y] != EnemyType.NONE;
    }

    private void HandleEraser(int x, int y)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1) // Right-click
        {
            grid[x, y] = EnemyType.NONE;
            Event.current.Use(); // Prevent context menu
        }
    }
    private void HandleClickDragPainting(Rect cellRect, int x, int y)
    {
        if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
        {
            Vector2 mousePos = Event.current.mousePosition;

            if (cellRect.Contains(mousePos) && Event.current.button == 0) // Left-click drag
            {
                grid[x, y] = selectedType;
                Event.current.Use();
            }
        }
    }

}
