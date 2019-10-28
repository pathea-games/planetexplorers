using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RegisterPrefabs))]
public class RegisterPrefabsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		RegisterPrefabs rp = target as RegisterPrefabs;
		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Reimport", GUILayout.Width(80)))
		{
			rp.Reimport();
		}
		
		if (GUILayout.Button("SavePaths", GUILayout.Width(80)))
		{
			rp.SavePath();
		}
		
		GUILayout.EndHorizontal();
		
		DrawDefaultInspector();
		GUILayout.EndVertical();
	}
}
