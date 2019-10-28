using UnityEngine;
using UnityEditor;
using System.Collections;
using CameraForge;

namespace CameraForgeEditor
{
	[CustomEditor(typeof(ControllerAsset))]
	public class ControllerAssetEditor : Editor
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
			_r.width = 132;
			_r.y -= 3;
			EditorGUI.DropShadowLabel(_r, "Camera Controller");
			//rect = EditorGUILayout.GetControlRect(false, 20);
			rect.xMax -= 15;
			rect.xMin = rect.xMax - 100;

			if (GUI.Button(rect, "Edit >>"))
			{
				ControllerAsset asset = (target as ControllerAsset);
				if (asset != null)
				{
					asset.Load();
					CameraForgeWindow.asset = asset;
					CameraForgeWindow.Init();
					CameraForgeWindow.current = asset.controller;
				}
			}
		}
	}
}