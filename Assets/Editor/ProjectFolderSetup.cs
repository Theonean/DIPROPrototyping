using UnityEditor;
using UnityEngine;
using System.IO;

public class ProjectFolderSetup : EditorWindow
{
    private static string projectName = "BlightProtocol";
    
    [MenuItem("Tools/Setup Project Folders")]
    public static void CreateProjectFolders()
    {
        string root = Path.Combine("Assets", projectName);
        
        string[] folders = new string[]
        {
            "Dev", "Scenes", "Player", "Gameplay", "Characters", "Environment", "Scripts", "VFX", "UI", "Rendering", "Editor"
        };
        
        if (!AssetDatabase.IsValidFolder(root))
        {
            AssetDatabase.CreateFolder("Assets", projectName);
        }
        
        foreach (string folder in folders)
        {
            string folderPath = Path.Combine(root, folder);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(root, folder);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("Project folder structure created successfully.");
    }
}
