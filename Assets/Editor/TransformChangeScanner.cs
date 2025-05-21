#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class TransformChangeScanner : EditorWindow
{
    [MenuItem("Tools/Scan Scripts for Transform Changes in Update")]
    public static void ScanScripts()
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        int totalWarnings = 0;

        foreach (string file in files)
        {
            string code = File.ReadAllText(file);
            bool insideUpdate = false;
            bool insideLateUpdate = false;
            int braceDepth = 0;
            int lineNumber = 0;

            string[] lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                lineNumber = i + 1;
                string trimmed = line.Trim();

                // Detect method entry
                if (Regex.IsMatch(trimmed, @"\bvoid\s+Update\s*\(\s*\)"))
                {
                    insideUpdate = true;
                    braceDepth = 0;
                    continue;
                }
                if (Regex.IsMatch(trimmed, @"\bvoid\s+LateUpdate\s*\(\s*\)"))
                {
                    insideLateUpdate = true;
                    braceDepth = 0;
                    continue;
                }

                // Track brace depth
                if (insideUpdate || insideLateUpdate)
                {
                    braceDepth += CountChar(trimmed, '{');
                    braceDepth -= CountChar(trimmed, '}');

                    if (Regex.IsMatch(trimmed, @"\btransform\.(position|rotation|Translate|Rotate)\b"))
                    {
                        string method = insideUpdate ? "Update" : "LateUpdate";
                        Debug.LogWarning($"[TransformChangeScanner] {Path.GetFileName(file)} line {lineNumber}: modifying transform in {method}()", AssetDatabase.LoadAssetAtPath<MonoScript>(ToAssetPath(file)));
                        totalWarnings++;
                    }

                    // Exit method body
                    if (braceDepth <= 0)
                    {
                        insideUpdate = false;
                        insideLateUpdate = false;
                    }
                }
            }
        }

        if (totalWarnings == 0)
            Debug.Log("TransformChangeScanner: No suspicious transform modifications found.");
        else
            Debug.Log($"TransformChangeScanner: Found {totalWarnings} potential issues.");
    }

    private static int CountChar(string line, char c)
    {
        int count = 0;
        foreach (char ch in line)
            if (ch == c) count++;
        return count;
    }

    private static string ToAssetPath(string fullPath)
    {
        return "Assets" + fullPath.Replace(Application.dataPath, "").Replace("\\", "/");
    }
}
#endif
