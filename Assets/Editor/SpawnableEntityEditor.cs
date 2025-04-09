// File: Editor/SpawnableEntityEditor.cs
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnableEntity))]
public class SpawnableEntityEditor : Editor
{
    private Texture2D previewTexture;
    private const int textureSize = 256;
    private Vector2 previewMapBoundsX = new Vector2(0, 50);
    private Vector2 previewMapBoundsZ = new Vector2(0, 50);

    //Properties to show dependent on spawn strategy
    private SerializedProperty spawnStrategyProp;
    private SerializedProperty customSpawnAreaXProp;
    private SerializedProperty customSpawnAreaZProp;
    private SerializedProperty voronoiSiteCountProp;
    private SerializedProperty siteSpreadRadiusProp;
    private SerializedProperty siteJitterProp;

    private void OnEnable()
    {
        spawnStrategyProp = serializedObject.FindProperty("spawnStrategy");
        customSpawnAreaXProp = serializedObject.FindProperty("customSpawnAreaBoundsX");
        customSpawnAreaZProp = serializedObject.FindProperty("customSpawnAreaBoundsZ");
        voronoiSiteCountProp = serializedObject.FindProperty("voronoiSiteCount");
        siteSpreadRadiusProp = serializedObject.FindProperty("siteSpreadRadius");
        siteJitterProp = serializedObject.FindProperty("siteJitter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all default fields *except* the ones we want to conditionally handle
        DrawPropertiesExcluding(serializedObject, "customSpawnAreaBoundsX", "customSpawnAreaBoundsZ", "voronoiSiteCount", "siteSpreadRadius", "siteJitter");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Strategy Settings", EditorStyles.boldLabel);

        SpawnStrategy strategy = (SpawnStrategy)spawnStrategyProp.enumValueIndex;

        switch (strategy)
        {
            case SpawnStrategy.RandomCustomArea:
                EditorGUILayout.PropertyField(customSpawnAreaXProp);
                EditorGUILayout.PropertyField(customSpawnAreaZProp);
                break;

            case SpawnStrategy.VoronoiNoise:
                EditorGUILayout.PropertyField(voronoiSiteCountProp);
                EditorGUILayout.PropertyField(siteSpreadRadiusProp);
                EditorGUILayout.PropertyField(siteJitterProp);
                break;

                // You can add more settings for other strategies if needed
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Spawn Preview"))
        {
            GeneratePreviewTexture();
        }

        if (previewTexture != null)
        {
            GUILayout.Label(previewTexture, GUILayout.Width(textureSize), GUILayout.Height(textureSize));
        }

        serializedObject.ApplyModifiedProperties();
    }


    private void GeneratePreviewTexture()
    {
        SpawnableEntity entity = (SpawnableEntity)target;
        Vector3[] positions = entity.GenerateSpawnPositions(previewMapBoundsX, previewMapBoundsZ);
        Vector2[] normalizedPositions = new Vector2[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            if (entity.spawnStrategy == SpawnStrategy.RandomCustomArea)
            {
                float boundsWidth = entity.customSpawnAreaBoundsX.y - entity.customSpawnAreaBoundsX.x;
                float boundsHeight = entity.customSpawnAreaBoundsZ.y - entity.customSpawnAreaBoundsZ.x;
                normalizedPositions[i] = new Vector2(
                    (positions[i].x - entity.customSpawnAreaBoundsX.x) / boundsWidth,
                    (positions[i].z - entity.customSpawnAreaBoundsZ.x) / boundsHeight
                );
            }
            else
            {
                float boundsWidth = previewMapBoundsX.y - previewMapBoundsX.x;
                float boundsHeight = previewMapBoundsZ.y - previewMapBoundsZ.x;
                normalizedPositions[i] = new Vector2(
                    (positions[i].x - previewMapBoundsX.x) / boundsWidth,
                    (positions[i].z - previewMapBoundsZ.x) / boundsHeight
                );
            }
        }

        previewTexture = new Texture2D(textureSize, textureSize);

        Color clear = Color.white;
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                previewTexture.SetPixel(x, y, clear);

        foreach (var pos in normalizedPositions)
        {
            int x = Mathf.FloorToInt(pos.x * textureSize);
            int y = Mathf.FloorToInt(pos.y * textureSize);

            foreach (int xMod in new int[] { -1, 0, 1 })
            {
                foreach (int yMod in new int[] { -1, 0, 1 })
                {
                    int xOffset = x + xMod;
                    int yOffset = y + yMod;
                    if (xOffset >= 0 && xOffset < textureSize && yOffset >= 0 && yOffset < textureSize)
                    {
                        previewTexture.SetPixel(xOffset, yOffset, Color.red);
                    }
                }
            }

            previewTexture.Apply();
        }
    }
}
