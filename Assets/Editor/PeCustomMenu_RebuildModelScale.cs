using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class PeCustomMenu_RebuildModelScale : EditorWindow
{
    [MenuItem("PeCustomMenu/Build Modle scale")]
    static void CreateModleScaleMenu()
    {
        CreateModleScale();
    }

    [MenuItem("PeCustomMenu/Remove LegAnimator")]
    static void RemoveLegAnimatorMenu()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            RemoveLegAnimator(Selection.objects[i]);
        }
    }

    static void RemoveLegAnimator(Object obj)
    {
        GameObject go = obj as GameObject;

        //GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(obj), typeof(GameObject)) as GameObject;

        if (go == null)
            return;

        /*string path = */AssetDatabase.GetAssetPath(go);

        LegAnimator[] legs = go.GetComponentsInChildren<LegAnimator>();
        foreach (LegAnimator leg in legs)
        {
            if (leg.transform.GetComponent<Animation>() == null)
                DestroyImmediate(leg);
        }

        AlignmentTracker[] ats = go.GetComponentsInChildren<AlignmentTracker>();
        foreach (AlignmentTracker leg in ats)
        {
            if (leg.transform.GetComponent<Animation>() == null)
                DestroyImmediate(leg);
        }

        LegController[] legcs = go.GetComponentsInChildren<LegController>();
        foreach (LegController leg in legcs)
        {
            if (leg.transform.GetComponent<Animation>() == null)
                DestroyImmediate(leg);
        }

        //PrefabUtility.SetPropertyModifications(Selection.activeObject, PrefabUtility.GetPropertyModifications(go));
    }

    static void CreateModleScale()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path = path.Remove(0, 6);
        string[] paths = Directory.GetFiles(Application.dataPath + path, "*.fbx", SearchOption.AllDirectories);

        foreach (string p in paths)
        {
            int start = p.IndexOf("Assets");
            string assetPath = p.Substring(start, p.Length - start);

            string directory = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            string newFileName = fileName;

            //small
            if (!fileName.Contains("@"))
                newFileName += "_small";
            else
            {
                string[] fileNames = fileName.Split(new char[] { '@' });
                fileNames[0] += "_small" + "@";

                newFileName = "";
                foreach (string str in fileNames)
                {
                    newFileName += str;
                }
            }

            string newAssetPath = directory + "/" + newFileName + extension;

            bool result = AssetDatabase.CopyAsset(assetPath, newAssetPath);

            if (result)
            {
                ModelImporter model = AssetImporter.GetAtPath(newAssetPath) as ModelImporter;
                if (model != null)
                {
                    model.globalScale *= 0.5f;
                }

                AssetDatabase.SaveAssets();
            }

            //large
            newFileName = fileName;
            if (!fileName.Contains("@"))
                newFileName += "_large";
            else
            {
                string[] fileNames = fileName.Split(new char[] { '@' });
                fileNames[0] += "_large" + "@";

                newFileName = "";
                foreach (string str in fileNames)
                {
                    newFileName += str;
                }
            }

            string newAssetPathLarge = directory + "/" + newFileName + extension;

            /*bool resultLarge = */AssetDatabase.CopyAsset(assetPath, newAssetPathLarge);

            if (result)
            {
                ModelImporter model = AssetImporter.GetAtPath(newAssetPathLarge) as ModelImporter;
                if (model != null)
                {
                    model.globalScale *= 2f;
                }

                AssetDatabase.SaveAssets();
            }

            Debug.LogError(newAssetPath);
        }
    }
}
