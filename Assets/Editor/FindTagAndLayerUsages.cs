using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FindTagAndLayerUsages : EditorWindow
{
    [MenuItem("Tools/Scan Scripts for CompareTag and LayerMask Usage")]
    public static void FindUsages()
    {
        string[] scriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        List<string> compareTagHits = new List<string>();
        List<string> layerMaskHits = new List<string>();

        foreach (string path in scriptPaths)
        {
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Contains("CompareTag("))
                {
                    compareTagHits.Add($"{RelativePath(path)} (Line {i + 1}): {line.Trim()}");
                }

                if (line.Contains("LayerMask.NameToLayer("))
                {
                    layerMaskHits.Add($"{RelativePath(path)} (Line {i + 1}): {line.Trim()}");
                }
            }
        }

        Debug.Log($"🔍 Scan complete. Found {compareTagHits.Count} CompareTag calls and {layerMaskHits.Count} LayerMask.NameToLayer calls.");

        if (compareTagHits.Count > 0)
        {
            Debug.LogWarning("=== CompareTag() Usages ===");
            foreach (var hit in compareTagHits)
                Debug.Log(hit);
        }

        if (layerMaskHits.Count > 0)
        {
            Debug.LogWarning("=== LayerMask.NameToLayer() Usages ===");
            foreach (var hit in layerMaskHits)
                Debug.Log(hit);
        }
    }

    private static string RelativePath(string fullPath)
    {
        return "Assets" + fullPath.Replace(Application.dataPath, "").Replace("\\", "/");
    }
}
