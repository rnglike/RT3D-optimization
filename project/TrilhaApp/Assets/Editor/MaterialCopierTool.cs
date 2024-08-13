using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MaterialCopierTool : EditorWindow
{
    private string folderPath = "Assets/CopiedMaterials";
    private const int batchSize = 50;
    private IEnumerator coroutine;

    [MenuItem("Tools/Material Copier Tool")]
    public static void ShowWindow()
    {
        GetWindow<MaterialCopierTool>("Material Copier Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Copy and Reassign Materials", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Copy and Reassign Materials"))
        {
            coroutine = CopyAndReassignMaterialsInScene();
            EditorApplication.update += EditorUpdate;
        }
    }

    private void EditorUpdate()
    {
        if (coroutine != null && !coroutine.MoveNext())
        {
            EditorApplication.update -= EditorUpdate;
            coroutine = null;
            EditorUtility.ClearProgressBar();
        }
    }

    private IEnumerator CopyAndReassignMaterialsInScene()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();

        int totalRenderers = 0;
        foreach (GameObject rootObject in rootObjects)
        {
            totalRenderers += rootObject.GetComponentsInChildren<MeshRenderer>().Length;
            totalRenderers += rootObject.GetComponentsInChildren<SkinnedMeshRenderer>().Length;
        }

        int processedCount = 0;

        foreach (GameObject rootObject in rootObjects)
        {
            MeshRenderer[] meshRenderers = rootObject.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (MeshRenderer renderer in meshRenderers)
            {
                Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material originalMat = renderer.sharedMaterials[i];
                    if (originalMat != null)
                    {
                        if (!materialMap.ContainsKey(originalMat) && !IsBuiltInMaterial(originalMat))
                        {
                            string path = AssetDatabase.GetAssetPath(originalMat);
                            string newPath = Path.Combine(folderPath, Path.GetFileName(path));
                            if (AssetDatabase.CopyAsset(path, newPath))
                            {
                                Material newMat = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                                materialMap[originalMat] = newMat;
                            }
                            else
                            {
                                Debug.LogError($"Failed to copy material: {originalMat.name}");
                            }
                        }
                        newMaterials[i] = materialMap.ContainsKey(originalMat) ? materialMap[originalMat] : originalMat;
                    }
                    else
                    {
                        Debug.LogWarning($"Null material found in MeshRenderer on GameObject '{renderer.gameObject.name}'");
                        newMaterials[i] = null;
                    }
                }
                renderer.sharedMaterials = newMaterials;
                EditorUtility.SetDirty(renderer);

                processedCount++;
                if (processedCount % batchSize == 0)
                {
                    float progress = (float)processedCount / totalRenderers;
                    EditorUtility.DisplayProgressBar("Copying and Reassigning Materials", "Processing renderers...", progress);
                    yield return null;
                }
            }

            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
            {
                Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material originalMat = renderer.sharedMaterials[i];
                    if (originalMat != null)
                    {
                        if (!materialMap.ContainsKey(originalMat) && !IsBuiltInMaterial(originalMat))
                        {
                            string path = AssetDatabase.GetAssetPath(originalMat);
                            string newPath = Path.Combine(folderPath, Path.GetFileName(path));
                            if (AssetDatabase.CopyAsset(path, newPath))
                            {
                                Material newMat = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                                materialMap[originalMat] = newMat;
                            }
                            else
                            {
                                Debug.LogError($"Failed to copy material: {originalMat.name}");
                            }
                        }
                        newMaterials[i] = materialMap.ContainsKey(originalMat) ? materialMap[originalMat] : originalMat;
                    }
                    else
                    {
                        Debug.LogWarning($"Null material found in SkinnedMeshRenderer on GameObject '{renderer.gameObject.name}'");
                        newMaterials[i] = null;
                    }
                }
                renderer.sharedMaterials = newMaterials;
                EditorUtility.SetDirty(renderer);

                processedCount++;
                if (processedCount % batchSize == 0)
                {
                    float progress = (float)processedCount / totalRenderers;
                    EditorUtility.DisplayProgressBar("Copying and Reassigning Materials", "Processing renderers...", progress);
                    yield return null;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Debug.Log("Materials copied, reassigned, and scene saved.");
    }

    private bool IsBuiltInMaterial(Material mat)
    {
        string path = AssetDatabase.GetAssetPath(mat);
        return string.IsNullOrEmpty(path) || path.StartsWith("Resources/unity_builtin_extra");
    }
}
