#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FBXMaterialRemapperWindow : EditorWindow
{
    private DefaultAsset _fbxFolder;
    private DefaultAsset _materialsFolder;

    private Vector2 _scrollPosition;
    private bool _isProcessing;

    [MenuItem("Tools/FBX Material Remapper")]
    private static void Open()
    {
        GetWindow<FBXMaterialRemapperWindow>("FBX Material Remapper");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("FBX Material Remapper", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Processes all FBX files recursively. Existing materials are reused from the Materials Folder. Missing materials are extracted from the FBX.", MessageType.Info);
        EditorGUILayout.Space(8);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        _fbxFolder = (DefaultAsset)EditorGUILayout.ObjectField("FBX Folder", _fbxFolder, typeof(DefaultAsset), false);

        _materialsFolder = (DefaultAsset)EditorGUILayout.ObjectField("Materials Folder", _materialsFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space(12);

        using (new EditorGUI.DisabledScope(_isProcessing))
        {
            if (GUILayout.Button("Process FBXs", GUILayout.Height(32)))
            {
                Process();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Process()
    {
        string fbxFolderPath = GetFolderPath(_fbxFolder);
        string materialsFolderPath = GetFolderPath(_materialsFolder);

        if (string.IsNullOrEmpty(fbxFolderPath))
        {
            EditorUtility.DisplayDialog("Invalid FBX Folder", "Please select a valid FBX folder.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(materialsFolderPath))
        {
            EditorUtility.DisplayDialog("Invalid Materials Folder", "Please select a valid Materials folder.", "OK");
            return;
        }

        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { fbxFolderPath });

        List<string> fbxPaths = new();

        foreach (string guid in fbxGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (Path.GetExtension(path).ToLowerInvariant() == ".fbx")
            {
                fbxPaths.Add(path);
            }
        }

        if (fbxPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("No FBXs Found", $"No FBX files were found inside:\n{fbxFolderPath}", "OK");
            return;
        }

        _isProcessing = true;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < fbxPaths.Count; i++)
            {
                string fbxPath = fbxPaths[i];
                EditorUtility.DisplayProgressBar("Processing FBXs", fbxPath, (float)i / fbxPaths.Count);
                ProcessFBX(fbxPath, materialsFolderPath);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _isProcessing = false;
        }

        EditorUtility.DisplayDialog("FBX Material Remapper", $"Finished processing {fbxPaths.Count} FBX file(s).", "OK");
    }

    private static void ProcessFBX(string fbxPath, string materialsFolderPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

        if (importer == null)
        {
            Debug.LogWarning($"Could not get ModelImporter for: {fbxPath}");
            return;
        }

        AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

        List<Material> embeddedMaterials = new();

        foreach (Object asset in assets)
        {
            if (asset is Material material)
            {
                embeddedMaterials.Add(material);
            }
        }

        if (embeddedMaterials.Count == 0)
        {
            Debug.Log($"No embedded materials found in: {fbxPath}");
            return;
        }

        Dictionary<string, Material> materialsByName = FindMaterials(materialsFolderPath);

        bool changed = false;

        foreach (Material embeddedMaterial in embeddedMaterials)
        {
            if (embeddedMaterial == null)
            {
                continue;
            }

            string materialName = embeddedMaterial.name;

            if (string.IsNullOrEmpty(materialName))
            {
                continue;
            }

            Material targetMaterial;

            if (materialsByName.TryGetValue(materialName, out Material existingMaterial))
            {
                targetMaterial = existingMaterial;
                Debug.Log($"Using existing material '{materialName}' for '{fbxPath}'.");
            }
            else
            {
                targetMaterial = ExtractMaterial(embeddedMaterial, materialsFolderPath);

                if (targetMaterial == null)
                {
                    Debug.LogWarning($"Failed to extract material '{materialName}' from '{fbxPath}'.");
                    continue;
                }

                materialsByName[materialName] = targetMaterial;

                Debug.Log($"Extracted material '{materialName}' from '{fbxPath}'.");
            }

            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), materialName), targetMaterial);
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static Dictionary<string, Material> FindMaterials(
        string materialsFolderPath)
    {
        Dictionary<string, Material> result = new();

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsFolderPath });

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                continue;
            }

            if (!result.ContainsKey(material.name))
            {
                result.Add(material.name, material);
            }
        }

        return result;
    }

    private static Material ExtractMaterial(Material embeddedMaterial, string materialsFolderPath)
    {
        string materialName = embeddedMaterial.name;
        string materialPath = AssetDatabase.GenerateUniqueAssetPath($"{materialsFolderPath}/{materialName}.mat");
        string extractedPath = AssetDatabase.ExtractAsset(embeddedMaterial, materialPath);

        if (string.IsNullOrEmpty(extractedPath))
        {
            Debug.LogWarning($"Failed to extract material '{materialName}' to '{materialPath}'.");
            return null;
        }

        AssetDatabase.ImportAsset(extractedPath, ImportAssetOptions.ForceUpdate);

        Material extractedMaterial = AssetDatabase.LoadAssetAtPath<Material>(extractedPath);

        if (extractedMaterial == null)
        {
            Debug.LogWarning($"Material was extracted but could not be loaded: {extractedPath}");
            return null;
        }

        return extractedMaterial;
    }

    private static string GetFolderPath(DefaultAsset folder)
    {
        if (folder == null)
        {
            return null;
        }

        string path = AssetDatabase.GetAssetPath(folder);

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            return null;
        }

        return path;
    }
}

#endif