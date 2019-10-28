using UnityEngine;
using UnityEditor;
using System.IO;

public class ScenarioScriptWizardWnd : EditorWindow
{
	[MenuItem ("Assets/Create/Scenario Script", false, 50)]
	public static void Init ()
	{
		// Get existing open window or if none, make a new one:
		ScenarioScriptWizardWnd window = (ScenarioScriptWizardWnd)EditorWindow.GetWindow(typeof(ScenarioScriptWizardWnd));
		window.titleContent = new GUIContent("Scenario Script");
		window.minSize = new Vector2(400, 340);
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

	int scriptType = 0;
	bool immediately = false;
	string namespaceName = "";
	string className = "Example";
	string statementName = "EXAMPLE";

	public static string activeDirectory
	{
		get
		{
			var selectedObject = Selection.activeObject;
			var path = AssetDatabase.GetAssetPath(selectedObject);

			if (string.IsNullOrEmpty(path))
			{
				return "Assets";
			}
			else
			{
				if (Directory.Exists(path))
				{
					return path;
				}
				else
				{
					return path.Substring(0, path.LastIndexOf('/'));
				}
			}
		}
	}

	void OnGUI ()
	{
		BeginContent();
		GUILayout.Label("Select Script Type :", EditorStyles.boldLabel);
		if (EditorGUILayout.ToggleLeft(" EventListener", scriptType == 1))
			scriptType = 1;
		if (EditorGUILayout.ToggleLeft(" Condition", scriptType == 2))
			scriptType = 2;
		if (EditorGUILayout.ToggleLeft(" Action", scriptType == 3))
			scriptType = 3;
		
		GUILayout.Space(8);
		if (scriptType == 3)
		{
			GUILayout.Label("Action Settings :", EditorStyles.boldLabel);
			immediately = EditorGUILayout.ToggleLeft(" Does the action execute immediately ?", immediately);
			GUILayout.Space(8);
		}
		GUILayout.Label("Script Settings :", EditorStyles.boldLabel);
		statementName = EditorGUILayout.TextField("Statement Name :", statementName);
		className = EditorGUILayout.TextField("Script Class Name :", className);
		namespaceName = EditorGUILayout.TextField("Namespace :", namespaceName);
		GUILayout.Space(8);
		GUILayout.Label("Target Path :", EditorStyles.boldLabel);
		GUILayout.TextArea(activeDirectory + "/" + className + ".cs", GUILayout.Width(360));
		GUILayout.Space(8);

		if (LayoutButton("Create Script", 30))
		{
			try {
				CreateScript();
			} catch { Debug.LogError("create script failed!"); }
		}

		EndContent();
	}

	#region GENERATE_SCRIPT
	string script = "";
	bool indent = false;

	void AddLine (string line)
	{
		if (indent)
			script = script + "    " + line + "\r\n";
		else
			script = script + line + "\r\n";
	}

	void CreateScript ()
	{
		string path = activeDirectory;
		if (scriptType != 1 && scriptType != 2 && scriptType != 3)
		{
			Debug.LogWarning("select script type!");
			return;
		}
		if (string.IsNullOrEmpty(className))
		{
			Debug.LogWarning("classname is empty!");
			return;
		}
		if (string.IsNullOrEmpty(statementName))
		{
			Debug.LogWarning("statement name is empty!");
			return;
		}
		if (string.IsNullOrEmpty(path))
		{
			Debug.LogWarning("Target path invalid!");
			return;
		}
		if (path.Length < 6)
		{
			Debug.LogWarning("Target path invalid!");
			return;
		}
		if (className.Length < 2)
		{
			Debug.LogWarning("classname is too short!");
			return;
		}
		if (path.Substring(0,6) != "Assets")
		{
			Debug.LogWarning("Target path invalid!");
			return;
		}

		string filename = Application.dataPath + "/../" + path + "/" + className + ".cs";
		if (File.Exists(filename))
		{
			Debug.LogWarning("Target file exist!");
			return;
		}

		string baseClass = "";
		if (scriptType == 1)
			baseClass = "ScenarioRTL.EventListener";
		else if (scriptType == 2)
			baseClass = "ScenarioRTL.Condition";
		else if (scriptType == 3)
			baseClass = "ScenarioRTL.Action";		

		script = "";
		AddLine("using UnityEngine;");
		AddLine("using ScenarioRTL;");
		if (scriptType == 3 && !immediately)
			AddLine("using System.IO;");
		AddLine("");
		if (!string.IsNullOrEmpty(namespaceName))
		{
			AddLine("namespace " + namespaceName);
			AddLine("{");
			indent = true;
		}
		if (scriptType == 3 && immediately)
			AddLine("[Statement(\"" + statementName + "\", true)]");
		else
			AddLine("[Statement(\"" + statementName + "\")]");
		AddLine("public class " + className + " : " + baseClass);
		AddLine("{");
		AddLine("    // 在此列举参数");
		AddLine("    ");
		AddLine("    // 在此初始化参数");
		AddLine("    protected override void OnCreate()");
		AddLine("    {");
		AddLine("    ");
		AddLine("    }");
		AddLine("    ");
		if (scriptType == 1)
		{
			AddLine("    // 打开事件监听");
			AddLine("    public override void Listen()");
			AddLine("    {");
			AddLine("    ");
			AddLine("    }");
			AddLine("    ");
			AddLine("    // 关闭事件监听");
			AddLine("    public override void Close()");
			AddLine("    {");
			AddLine("    ");
			AddLine("    }");
		}
		else if (scriptType == 2)
		{
			AddLine("    // 判断条件");
			AddLine("    public override bool? Check()");
			AddLine("    {");
			AddLine("        return true;");
			AddLine("    }");
		}
		else if (scriptType == 3)
		{
			AddLine("    // 执行动作");
			AddLine("    // 若为瞬间动作，返回true；");
			AddLine("    // 若为持续动作，该函数会每帧被调用，直到返回true");
			AddLine("    public override bool Logic()");
			AddLine("    {");
			AddLine("        return true;");
			AddLine("    }");
			if (!immediately)
			{
				AddLine("    ");
				AddLine("    // 恢复动作状态");
				AddLine("    public override void RestoreState(BinaryReader r)");
				AddLine("    {");
				AddLine("    ");
				AddLine("    }");
				AddLine("    // 存储动作状态");
				AddLine("    public override void StoreState(BinaryWriter w)");
				AddLine("    {");
				AddLine("    ");
				AddLine("    }");
			}
		}
		AddLine("}");
		if (!string.IsNullOrEmpty(namespaceName))
		{
			indent = false;
			AddLine("}");
		}
		Debug.Log(activeDirectory + "/" + className + ".cs");
		FileStream fs = new FileStream(Application.dataPath + "/../" + path + "/" + className + ".cs", FileMode.Create, FileAccess.Write);
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(script);
		fs.Write(buffer, 0, buffer.Length);
		fs.Close();
		AssetDatabase.Refresh();
	}
	#endregion
}
