#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class RecalculateSpawnableEntitiyBounds
{
    [MenuItem("Tools/Spawnable Entities/Recalculate All Bounding Radii")]
    public static void RecalculateAllBoundingRadii()
    {
        string[] guids = AssetDatabase.FindAssets("t:SpawnableEntity");

        int updated = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpawnableEntity spawnable = AssetDatabase.LoadAssetAtPath<SpawnableEntity>(path);

            if (spawnable != null)
            {
                spawnable.RecalculateBoundingRadius();
                updated++;
            }
        }

        Debug.Log($"[BoundingRadiusAutoCalc] Recalculated bounding radius for {updated} SpawnableEntity assets.");
    }
}
#endif
