/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(WaterToolScript))]

public class WaterToolEditor : Editor 
{
	public WaterToolScript waterScript;
	
	public void Awake()
	{
		waterScript = (WaterToolScript)target as WaterToolScript;
	}
	
	public void OnSceneGUI()
	{
		
	}
	
	public override void OnInspectorGUI() 
	{
		EditorGUIUtility.labelWidth = 0;
		EditorGUIUtility.fieldWidth = 0;

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		Rect createRiverButton = EditorGUILayout.BeginHorizontal();
		createRiverButton.x = createRiverButton.width / 2 - 100;
		createRiverButton.width = 200;
		createRiverButton.height = 18;
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
					
		if (GUI.Button(createRiverButton, "New River")) 
		{		
			waterScript.CreateRiver();
		}
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		if (GUI.changed) 
		{
			EditorUtility.SetDirty(waterScript);
		}
	}
}