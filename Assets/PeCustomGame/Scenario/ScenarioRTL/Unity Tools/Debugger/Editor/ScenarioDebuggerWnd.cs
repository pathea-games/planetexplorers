using UnityEngine;
using UnityEditor;
using System.IO;
using ScenarioRTL;
using ScenarioRTL.IO;

public class ScenarioDebuggerWnd : EditorWindow
{
	[MenuItem ("Window/Scenario Debugger", false, 50)]
	public static void Init ()
	{
		// Get existing open window or if none, make a new one:
		ScenarioDebuggerWnd window = (ScenarioDebuggerWnd)EditorWindow.GetWindow(typeof(ScenarioDebuggerWnd));
		window.titleContent = new GUIContent( "Scenario Debugger");
		window.minSize = new Vector2(500, 340);
		window.Show();
	}

	void BeginContent ()
	{
		GUILayout.Space(16);
		GUILayout.BeginHorizontal();
		GUILayout.Space(16);
		GUILayout.BeginVertical();
	}

	void EndContent ()
	{
		GUILayout.EndVertical();
		GUILayout.Space(16);
		GUILayout.EndHorizontal();
		GUILayout.Space(16);
	}

	bool LayoutButton (string text, int height)
	{
		return GUILayout.Button(text, GUILayout.Height(height));
	}

	int tick = 0;
	void Update ()
	{
		if (tick++ % 10 == 0)
			Repaint();
	}

	Vector2 mission_sc;
	Vector2 threads_sc;
	void OnGUI ()
	{
		BeginContent();

		if (Scenario.debugTarget != null)
		{
			EditorGUILayout.BeginHorizontal();

			mission_sc = EditorGUILayout.BeginScrollView(mission_sc);
			EditorGUILayout.BeginVertical();
			TreeNode("Mission Instances", true);

			EditorGUI.indentLevel++;
			foreach (var kvp in Scenario.debugTarget.missionInsts)
			{
				MissionRaw raw = null;
				if (Scenario.debugTarget.missionRaws.ContainsKey(kvp.Value.dataId))
					raw = Scenario.debugTarget.missionRaws[kvp.Value.dataId];
				if (raw != null)
				{
					TreeNode(raw.name, true);
				}
				else
				{
					GUI.contentColor = Color.red;
					GUILayout.Label("     <No data>");
					GUI.contentColor = Color.white;
				}
			}
			EditorGUI.indentLevel--;

			TreeNode("Scenario Variables", true);

			EditorGUI.indentLevel++;
			string[] names = Scenario.debugTarget.scenarioVars.declaredVars;
			foreach (var varname in names)
			{
				GUILayout.Label("     " + varname + " = " + Scenario.debugTarget.scenarioVars[varname].data);
			}
			EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			threads_sc = EditorGUILayout.BeginScrollView(threads_sc);
			EditorGUILayout.BeginVertical();

			TreeNode("Action Threads", true);
			EditorGUI.indentLevel++;
			for (int i = 0; i < Scenario.debugTarget.actionThreads.Count; ++i)
			{
				TreeNode("Thread[" + i + "]", true);
			}
			EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.Label("No debug target.");
		}

		EndContent();
	}

	Color lineColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
	Rect TreeNode (string name, bool expand)
	{
		var rect = EditorGUILayout.GetControlRect();
		EditorGUI.Foldout(rect, expand, name);
		var rect2 = rect;
		rect2.height = 1;
		rect2.y = rect.yMax + 1;
		EditorGUI.DrawRect(rect2, lineColor);
		return rect;
	}
}
