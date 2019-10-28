using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using CameraForge;

namespace CameraForgeEditor
{
	public static class EditorMenus
	{
		[MenuItem("Assets/Create/Camera Forge/Camera Controller")]
		public static void CreateController ()
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
			
			if (path.Substring(0, 6) != "Assets")
			{
				EditorUtility.DisplayDialog("Please select a target folder first", "Error", "OK");
				return;
			}
			
			ControllerAsset asset = ScriptableObject.CreateInstance<ControllerAsset>();
			
			asset.controller = new Controller ("New Camera Controller");

			HistoryPose node = new HistoryPose ();
			node.editorPos = new Vector2 (-200, 0);
			asset.controller.AddPoseNode(node);
			asset.controller.final.Final.input = node;
			asset.Save();
			
			path = AssetDatabase.GenerateUniqueAssetPath (path + "/New Camera Controller.asset");
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Assets/Create/Camera Forge/Camera Modifier")]
		public static void CreateModifier ()
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
			
			if (path.Substring(0, 6) != "Assets")
			{
				EditorUtility.DisplayDialog("Please select a target folder first", "Error", "OK");
				return;
			}
			
			ModifierAsset asset = ScriptableObject.CreateInstance<ModifierAsset>();
			
			asset.modifier = new Modifier ("New Camera Modifier");
			asset.Save();
			
			path = AssetDatabase.GenerateUniqueAssetPath (path + "/New Camera Modifier.asset");
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		}

		public static void CreateModifier (Modifier modifier)
		{
			string name = modifier.Name.value.value_str;
			if (string.IsNullOrEmpty(name))
				name = "New Camera Modifier";
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}
			
			if (path.Substring(0, 6) != "Assets")
			{
				EditorUtility.DisplayDialog("Please select a target folder first", "Error", "OK");
				return;
			}
			
			ModifierAsset asset = ScriptableObject.CreateInstance<ModifierAsset>();

			asset.modifier = modifier;
			asset.Save();
			asset.modifier = null;
			
			path = AssetDatabase.GenerateUniqueAssetPath (path + "/" + name + ".asset");
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		}

		public static void CreateController (Controller controller)
		{
			string name = controller.Name.value.value_str;
			if (string.IsNullOrEmpty(name))
				name = "New Camera Controller";
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}
			
			if (path.Substring(0, 6) != "Assets")
			{
				EditorUtility.DisplayDialog("Please select a target folder first", "Error", "OK");
				return;
			}
			
			ControllerAsset asset = ScriptableObject.CreateInstance<ControllerAsset>();
			
			asset.controller = controller;
			asset.Save();
			asset.controller = null;
			
			path = AssetDatabase.GenerateUniqueAssetPath (path + "/" + name + ".asset");
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		}
	}
}
