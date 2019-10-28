using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ModelImporterScale : EditorWindow
{
    public GameObject model;
    //public GameObject ragdoll;

    public List<GameObject> clips = new List<GameObject>();
    public List<float> scales = new List<float>();

    [MenuItem("Window/Model Scale")]
    static void Init()
    {
        //ModelImporterScale window = (ModelImporterScale)EditorWindow.GetWindow(typeof(ModelImporterScale));
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

		model = EditorGUILayout.ObjectField("Model : ", model, typeof(GameObject), true) as GameObject;
        //ragdoll = EditorGUILayout.ObjectField("Ragdoll : ", ragdoll, typeof(GameObject)) as GameObject;

        for (int i = 0; i < clips.Count; i++)
        {
			clips[i] = EditorGUILayout.ObjectField("Animation clip " + i + " : ", clips[i], typeof(GameObject), true) as GameObject;
        }

        for (int i = 0; i < scales.Count; i++)
        {
            scales[i] = EditorGUILayout.FloatField("Scale " + i + " : ", scales[i]);
        }

        if (GUILayout.Button("Import Animation"))
        {
            ImportAnimation();
        }

        if (GUILayout.Button("Add Scale"))
        {
            AddScale();
        }

        if (GUILayout.Button("Clear Scale"))
        {
            ClearScale();
        }

        if (GUILayout.Button("Apply Scales"))
        {
            ApplyScale();
        }

        EditorGUILayout.EndVertical();
    }

    void ClearScale()
    {
        scales.Clear();
    }

    void AddScale()
    {
        scales.Add(1.0f);
    }

    void ApplyScale()
    {
        if (model == null)
            return;

        string path = AssetDatabase.GetAssetPath(model);
        path = path.Remove(0, 6);
        path = Path.GetDirectoryName(path);
        string[] paths = Directory.GetFiles(Application.dataPath + path, "*.fbx", SearchOption.AllDirectories);

        for (int i = 0; i < scales.Count; i++)
        {
            foreach (string p in paths)
            {
                int start = p.IndexOf("Assets");
                string assetPath = p.Substring(start, p.Length - start);

                string directory = Path.GetDirectoryName(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string extension = Path.GetExtension(assetPath);

                string newFileName = fileName;

                if (!fileName.Contains("@"))
                    newFileName += "_" + scales[i];
                else
                {
                    string[] fileNames = fileName.Split(new char[] { '@' });
                    fileNames[0] += "_" + scales[i] + "@";

                    newFileName = "";
                    foreach (string str in fileNames)
                    {
                        newFileName += str;
                    }
                }

                string newAssetPath = directory + "/" + newFileName + extension;

                if (AssetDatabase.CopyAsset(assetPath, newAssetPath))
                {
                    ModelImporter modelImporter = AssetImporter.GetAtPath(newAssetPath) as ModelImporter;
                    if (modelImporter != null)
                    {
                        modelImporter.globalScale *= scales[i];

                        AssetDatabase.ImportAsset(newAssetPath);
                    }

                    //Object ob = AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Object));
                    //Object pre = PrefabUtility.InstantiatePrefab(modelImporter);
                    //GameObject obj = GameObject.Instantiate(ob) as GameObject;
                    //if (!fileName.Contains("@") && ragdoll != null && obj != null)
                    //{
                    //    string newPrefabPath = Path.GetDirectoryName(newAssetPath) + "_ragdoll" + ".prefab";
                    //    Object prefab = PrefabUtility.CreatePrefab(newPrefabPath, obj);
                    //    PrefabUtility.SetPropertyModifications(prefab, PrefabUtility.GetPropertyModifications(ragdoll));

                    //    DestroyImmediate(obj);
                    //}
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    void ImportAnimation()
    {
        if (model == null)
            return;

        clips.Clear();

        string path = AssetDatabase.GetAssetPath(model);
        path = path.Remove(0, 6);
        path = Path.GetDirectoryName(path);
        string[] paths = Directory.GetFiles(Application.dataPath + path, "*.fbx", SearchOption.AllDirectories);

        foreach (string p in paths)
        {
            int start = p.IndexOf("Assets");
            string assetPath = p.Substring(start, p.Length - start);

            GameObject obj = AssetDatabase.LoadMainAssetAtPath(assetPath) as GameObject;

            if (obj != null && obj.name.Contains("@") && !clips.Contains(obj))
            {
                clips.Add(obj);
            }
        }
    }
}
