using UnityEngine;
using UnityEditor;

public class FindGameObjectsWithInvalidLayers : EditorWindow
{
    [MenuItem("Tools/Find Objects With Invalid (Missing) Layers")]
    public static void FindInvalidLayers()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        int count = 0;

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject obj = allObjects[i];
            int layer = obj.layer;

            // If the layer index doesn't map to a name, it's invalid (was deleted)
            string layerName = LayerMask.LayerToName(layer);
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning($"[Invalid Layer] {GetFullPath(obj)} is on layer index {layer}", obj);
                count++;
            }
        }

        Debug.Log($"Scan complete. Found {count} GameObject(s) with missing/deleted layers.");
    }

    private static string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }
}