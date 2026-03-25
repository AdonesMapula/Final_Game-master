using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class PrefabToPNG : EditorWindow
{
    private GameObject prefab;
    private int width = 512;
    private int height = 512;
    private string savePath = "Assets/Exports";
    private float padding = 1.2f;

    private bool logRenderers = true;
    private string exportOnlyChildRoot = "";
    private bool exportLargestClusterOnly = true;
    private float clusterDistance = 3f;

    [MenuItem("Tools/Prefab to PNG")]
    public static void ShowWindow()
    {
        GetWindow<PrefabToPNG>("Prefab to PNG");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab to PNG Converter", EditorStyles.boldLabel);
        GUILayout.Space(10);

        prefab = EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false) as GameObject;
        width = EditorGUILayout.IntField("Image Width", width);
        height = EditorGUILayout.IntField("Image Height", height);
        padding = EditorGUILayout.FloatField("Padding", padding);
        savePath = EditorGUILayout.TextField("Save Folder", savePath);

        GUILayout.Space(5);
        GUILayout.Label("Optional", EditorStyles.boldLabel);
        exportOnlyChildRoot = EditorGUILayout.TextField("Only Export Child Root", exportOnlyChildRoot);
        logRenderers = EditorGUILayout.Toggle("Log Renderers", logRenderers);
        exportLargestClusterOnly = EditorGUILayout.Toggle("Export Largest Cluster Only", exportLargestClusterOnly);
        clusterDistance = EditorGUILayout.FloatField("Cluster Distance", clusterDistance);

        GUILayout.Space(10);

        GUI.enabled = prefab != null;
        if (GUILayout.Button("Render to PNG", GUILayout.Height(40)))
        {
            RenderPrefabToPNG();
        }
        GUI.enabled = true;
    }

    private void RenderPrefabToPNG()
    {
        if (prefab == null)
        {
            Debug.LogError("No prefab assigned.");
            return;
        }

        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Width and Height must be greater than 0.");
            return;
        }

        if (!savePath.StartsWith("Assets"))
        {
            Debug.LogError("Save Folder must start with 'Assets'. Example: Assets/Exports");
            return;
        }

        Scene previewScene = default;
        GameObject instance = null;
        GameObject camObj = null;
        RenderTexture rt = null;
        Texture2D tex = null;

        try
        {
            previewScene = EditorSceneManager.NewPreviewScene();

            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, previewScene);
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab into preview scene.");
                return;
            }

            instance.name = prefab.name + "_PreviewInstance";
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Transform exportRoot = instance.transform;

            if (!string.IsNullOrWhiteSpace(exportOnlyChildRoot))
            {
                Transform found = FindDeepChildByName(instance.transform, exportOnlyChildRoot);
                if (found == null)
                {
                    Debug.LogError("Child root not found: " + exportOnlyChildRoot);
                    return;
                }

                exportRoot = found;
            }

            Renderer[] renderers = exportRoot.GetComponentsInChildren<Renderer>(false)
                .Where(r => r.enabled && r.gameObject.activeInHierarchy)
                .ToArray();

            if (renderers.Length == 0)
            {
                Debug.LogError("No active renderers found in prefab/export root.");
                return;
            }

            if (exportLargestClusterOnly)
            {
                renderers = GetLargestRendererCluster(renderers, clusterDistance).ToArray();
            }

            if (renderers.Length == 0)
            {
                Debug.LogError("No renderers left after cluster filtering.");
                return;
            }

            if (logRenderers)
            {
                Debug.Log("==== RENDERERS INCLUDED IN EXPORT ====");
                foreach (Renderer r in renderers)
                {
                    Debug.Log(GetHierarchyPath(r.transform) + " | Pos: " + r.bounds.center + " | Size: " + r.bounds.size);
                }
                Debug.Log("======================================");
            }

            Bounds bounds = GetRenderBounds(renderers);

            camObj = new GameObject("TempRenderCamera");
            SceneManager.MoveGameObjectToScene(camObj, previewScene);

            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.orthographic = true;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.cullingMask = ~0;
            cam.aspect = (float)width / height;

            float verticalSize = bounds.extents.y;
            float horizontalSize = bounds.extents.x / cam.aspect;
            float requiredSize = Mathf.Max(verticalSize, horizontalSize) * padding;

            cam.orthographicSize = Mathf.Max(requiredSize, 0.01f);
            cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            cam.transform.rotation = Quaternion.identity;

            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 1;
            rt.filterMode = FilterMode.Point;
            rt.Create();

            cam.targetTexture = rt;

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            cam.Render();

            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            RenderTexture.active = previous;

            if (!AssetDatabase.IsValidFolder(savePath))
                CreateFoldersRecursively(savePath);

            string fileName = prefab.name + ".png";
            if (!string.IsNullOrWhiteSpace(exportOnlyChildRoot))
                fileName = prefab.name + "_" + exportOnlyChildRoot + ".png";

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), savePath, fileName);
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());

            AssetDatabase.Refresh();
            Debug.Log("PNG saved: " + fullPath);
            EditorUtility.DisplayDialog("Success", "PNG saved:\n" + fullPath, "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Prefab to PNG failed:\n" + ex);
        }
        finally
        {
            if (rt != null)
            {
                rt.Release();
                DestroyImmediate(rt);
            }

            if (tex != null)
                DestroyImmediate(tex);

            if (camObj != null)
                DestroyImmediate(camObj);

            if (instance != null)
                DestroyImmediate(instance);

            if (previewScene.IsValid())
                EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    private List<Renderer> GetLargestRendererCluster(Renderer[] renderers, float maxDistance)
    {
        List<List<Renderer>> clusters = new List<List<Renderer>>();
        HashSet<Renderer> visited = new HashSet<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (visited.Contains(renderers[i]))
                continue;

            List<Renderer> cluster = new List<Renderer>();
            Queue<Renderer> queue = new Queue<Renderer>();
            queue.Enqueue(renderers[i]);
            visited.Add(renderers[i]);

            while (queue.Count > 0)
            {
                Renderer current = queue.Dequeue();
                cluster.Add(current);

                for (int j = 0; j < renderers.Length; j++)
                {
                    if (visited.Contains(renderers[j]))
                        continue;

                    float dist = Vector2.Distance(current.bounds.center, renderers[j].bounds.center);
                    if (dist <= maxDistance)
                    {
                        visited.Add(renderers[j]);
                        queue.Enqueue(renderers[j]);
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters.OrderByDescending(c => c.Count).FirstOrDefault() ?? new List<Renderer>();
    }

    private Bounds GetRenderBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private Transform FindDeepChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChildByName(child, childName);
            if (result != null)
                return result;
        }

        return null;
    }

    private string GetHierarchyPath(Transform t)
    {
        List<string> parts = new List<string>();
        while (t != null)
        {
            parts.Add(t.name);
            t = t.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    private void CreateFoldersRecursively(string assetPath)
    {
        string[] parts = assetPath.Split('/');
        if (parts.Length == 0 || parts[0] != "Assets")
            return;

        string current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}