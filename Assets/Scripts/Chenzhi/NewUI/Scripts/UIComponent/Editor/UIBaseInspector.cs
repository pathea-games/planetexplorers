using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class UIBaseInspector : Editor 
{
	public SerializedObject uiobject;
	
	public static GUIContent
		insertContent = new GUIContent("+", "duplicate this item"),
		deleteContent = new GUIContent("-", "delete this item"),
		clearContent = new GUIContent("clear", "duplicate this item"),
		DelEventContent = new GUIContent("del", "delete this item");
	
	public static GUILayoutOption
		max_buttonWidth = GUILayout.Width(100f),
		max_buttonHeight= GUILayout.Height(20f),
		min_buttonWidth = GUILayout.Width (24f),
		min_buttonHeight = GUILayout.Height (16f),
		EidtLineWidth = GUILayout.Width(150);

	public int offset = 0;

	public string stringline = " --------------------------------------------------------------------------------------------";

	public virtual void OnEnable()
	{
		uiobject = new SerializedObject(target);
	}

	public override void OnInspectorGUI() 
	{
		uiobject.Update();
		
		GUILayout.Space(7);
		EditorGUILayout.BeginHorizontal();
		GUI.backgroundColor = (offset == 0) ? Color.blue : Color.gray;
		if (GUILayout.Button("Propertys",max_buttonWidth,max_buttonHeight))
			offset = 0;
		GUI.backgroundColor = (offset == 1) ? Color.blue : Color.gray;
		if (GUILayout.Button("Events",max_buttonWidth,max_buttonHeight))
			offset = 1;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(7);
		if (offset == 0)
		{
			OnInspectorGUI_Propertys();
		}
		else
		{
			OnInspectorGUI_Events();
		}

		uiobject.ApplyModifiedProperties();
		
	}



	public virtual void OnInspectorGUI_Propertys()
	{
		
	}
	
	
	public virtual void OnInspectorGUI_Events()
	{

	}



	public void DrawConpomentEvents(FieldInfo[]	infos,UIComponent conpoment)
	{
		GUI.backgroundColor = Color.gray;
		GameObject obj = EditorGUILayout.ObjectField("eventReceiver",conpoment.eventReceiver,typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
		if (obj != conpoment.eventReceiver)
		{
			conpoment.eventReceiver = obj;
		}
		
		DrawPartLine("events");

		for (int i =0 ; i < infos.Length ; i++)
		{
			if (infos[i].FieldType == typeof(UIConpomentEvent) )
			{
				EditorGUILayout.BeginHorizontal();
				
				if (infos[i] != null)
				{
					UIConpomentEvent ui_event = (UIConpomentEvent) infos[i].GetValue(conpoment);

					string str = EditorGUILayout.TextField(infos[i].Name,ui_event.functionName, GUILayout.ExpandWidth(true));
					if (str != ui_event.functionName)
					{
						ui_event.functionName = str;
					}
					
					GUI.backgroundColor = Color.green;
					
					if (GUILayout.Button(clearContent,GUILayout.Width(50),min_buttonHeight))
					{
						ui_event.functionName = "";
						GUI.FocusControl("");
					}
					GUI.backgroundColor = Color.gray;
				}
				
				EditorGUILayout.EndHorizontal();
			}
		}
	}

	public void DrawPartLine(string content)
	{
		GUI.color = Color.yellow;
		GUILayout.Label( content + stringline);
		GUI.color = Color.white;
		GUILayout.Space(1);

	}
}
