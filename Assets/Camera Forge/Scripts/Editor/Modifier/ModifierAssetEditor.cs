using UnityEngine;
using UnityEditor;
using System.Collections;
using CameraForge;

namespace CameraForgeEditor
{
	[CustomEditor(typeof(ModifierAsset))]
	public class ModifierAssetEditor : Editor
	{
		GUISkin skin;
		public override void OnInspectorGUI ()
		{
			if (skin == null)
				skin = Resources.Load<GUISkin>("GUI/EditorSkin") as GUISkin;

			Rect rect;
			GUILayout.Space(10);
		
			rect = EditorGUILayout.GetControlRect(false, 20);
			Rect _r = rect;
			_r.width = 120;
			_r.y -= 3;
			EditorGUI.DropShadowLabel(_r, "Camera Modifier");
			//rect = EditorGUILayout.GetControlRect(false, 20);
			rect.xMax -= 15;
			rect.xMin = rect.xMax - 100;

			if (GUI.Button(rect, "Edit >>"))
			{
				ModifierAsset asset = (target as ModifierAsset);
				if (asset != null)
				{
					asset.Load(true);
					CameraForgeWindow.asset = asset;
					CameraForgeWindow.Init();
					CameraForgeWindow.current = asset.modifier;
				}
			}
		}
	}
}