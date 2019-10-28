using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;

public class EditorUtil
{
    public static string GetSystemPath(Object assetObject)
    {
        string path = AssetDatabase.GetAssetPath(assetObject);
        if (path.Length > 0)
            return Path.Combine(Application.dataPath, path.Substring(("Assets/").Length)).Replace(@"\", @"/");
        else
            return "";
    }
}
