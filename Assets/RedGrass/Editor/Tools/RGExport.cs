using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace RedGrass
{

	public class RedGrassExport 
	{

		[MenuItem("Assets/RedGrass/Environment Setting")]
		public static void CreateEvni ()
		{
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}


			EvniAsset ea = ScriptableObject.CreateInstance<EvniAsset>();
			
			path = AssetDatabase.GenerateUniqueAssetPath (path + "/New Environment Settings.asset");
			AssetDatabase.CreateAsset(ea, path);
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Assets/RedGrass/Prototype Mgr")]
		public static void CreatePrototype()
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}

			GameObject go = new GameObject();
			go.hideFlags = HideFlags.DontSaveInEditor;
			go.AddComponent<RGPrototypeMgr>();
			/*GameObject obj = */PrefabUtility.CreatePrefab(path + "/Grass Prototype.prefab", go);
			GameObject.DestroyImmediate(go);

		}
	}
}


