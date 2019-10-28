using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Net.Mail;


public partial class PeCustomMenuc : EditorWindow
{
	[MenuItem("PeCustomMenu/TestMail")]
	static void TestMail()
	{
		Debug.Log("Test Mail");
		BugReporter.SendEmailAsync ("Tst Mail in Editor", 5);
	}
	[MenuItem("PeCustomMenu/Perf Comp")]
	static void PerfCompareCast ()
	{
		int n = 100;
		RaycastHit ret;
		long t0, t1, t2;
		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
		watch.Start ();
		for (int x = 0; x < n; x++) {
			for(int z = 0; z < n; z++){
				Physics.Raycast(new Vector3(x, 1000.0f, z), Vector3.down, out ret);
			}
		}
		watch.Stop ();
		t0 = watch.ElapsedTicks;
		watch.Reset ();
		watch.Start ();
		for (int x = 0; x < n; x++) {
			for(int z = 0; z < n; z++){
				Physics.SphereCast(new Vector3(x, 1000.0f, z), 1.0f, Vector3.down, out ret);
			}
		}
		watch.Stop ();
		t1 = watch.ElapsedTicks;
		watch.Reset ();
		watch.Start ();
		for (int x = 0; x < n; x++) {
			for(int z = 0; z < n; z++){
				Physics.SphereCast(new Vector3(x, 50.0f, z), 1.0f, Vector3.down, out ret);
			}
		}
		watch.Stop ();
		t2 = watch.ElapsedTicks;

		Debug.LogError ("[Sphere]:"+t2 + ":" + t1+"[Ray]"+t0);
	}



	[MenuItem("PeCustomMenu/Unlock localData.sldb")]
	static void UnlockLocalDataBase ()
	{
		LocalDatabase.Instance.CloseDB();
	}

	[MenuItem("GameObject/SavePosRotScl", false, 48)]
	static void SaveAssetsPosRotScl(MenuCommand menuCommand){
		//Prevent executing multiple times when right-clicking.
		if (Selection.objects.Length > 1){
			if (menuCommand.context != Selection.objects[0]){
				return;
			}
		}

		XmlDocument xmlDoc = new XmlDocument();
		string filePath = GameConfig.PEDataPath + "PosRotScl.xml";
		if(File.Exists(filePath)){
			try{
				xmlDoc.Load (filePath);
			} catch {
				Debug.LogError("Invalid xml file:"+filePath);
			}
		}

		XmlElement rootNode = (XmlElement)xmlDoc.SelectSingleNode("SceneAssetsList");
		if(null == rootNode)
		{
			rootNode = xmlDoc.CreateElement("SceneAssetsList");
			xmlDoc.AppendChild(rootNode);
		}
		
		for(int i = 0; i < Selection.gameObjects.Length; i++){
			GameObject go = Selection.gameObjects[i];
			string name = go.name;
			if(name.Contains(' ')){				name = name.Replace(' ', '_');			}
			if(name.Contains('(')){				name = name.Replace('(', '_');			}
			if(name.Contains(')')){				name = name.Replace(')', '_');			}
			if(name.Contains('[')){				name = name.Replace('[', '_');			}
			if(name.Contains(']')){				name = name.Replace(']', '_');			}
			XmlElement node = xmlDoc.CreateElement(name);
			node.SetAttribute("pos",go.transform.position.ToString());
			node.SetAttribute("rot",go.transform.rotation.ToString());
			node.SetAttribute("scl",go.transform.localScale.ToString());
			rootNode.AppendChild(node);
		}
		using (FileStream fs = new FileStream (filePath, FileMode.Create, FileAccess.Write)) {
			xmlDoc.Save (fs);
		}
	}

	//[MenuItem("PeCustomMenu/TmpAddTreeColliderToPrefab")]
	static void CheckAddTreeColToPrefab()
	{
		string modelDir = @"Assets/Resources/Model/scene/Tree-Rotated-Ambient-Occlusion/";
		string prefabDir = @"Assets/Prefabs/tree-Ambient-Occlusion/";
		string[] modelPathNames = Directory.GetFiles(modelDir, "*.fbx");
		string[] prefabPathNames = Directory.GetFiles(prefabDir, "*.prefab");
#if false
		// check if a prefab is valid
		List<string> invalidPrefabPathNames = new List<string>();
		if(prefabPathNames != null)
		{
			foreach(string prefabPathName in prefabPathNames)
			{
				GameObject go = (GameObject)Instantiate(Resources.LoadAssetAtPath(prefabPathName, typeof(GameObject)));
				MeshFilter mf = go.GetComponent<MeshFilter>();
				if(mf == null || !prefabPathName.Contains(mf.sharedMesh.name))
				{
					invalidPrefabPathNames.Add(prefabPathName);
				}
				DestroyImmediate(go);
			}
		}
		for(int i = 0; i < invalidPrefabPathNames.Count; i++)
		{
			// TODO: create prefab
		}
#endif
		if(modelPathNames != null)
		{
			foreach(string modelPathName in modelPathNames)
			{
				string modelName = Path.GetFileNameWithoutExtension(modelPathName);
				bool bExistPrefab = false;
				foreach(string prefabPathName in prefabPathNames)
				{
					if(prefabPathName.Contains(modelName))
					{
						bExistPrefab = true;
						GameObject prefabGo = UnityEditor.AssetDatabase.LoadAssetAtPath(prefabPathName, typeof(GameObject)) as GameObject;
						if(prefabGo.GetComponent<CapsuleCollider>() == null)
						{
							prefabGo.AddComponent<CapsuleCollider>();
						}
						break;
					}
				}
				if(!bExistPrefab)
				{
					GameObject go = (GameObject)Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath(modelPathName, typeof(GameObject)));
					go.AddComponent<CapsuleCollider>();
					PrefabUtility.CreatePrefab(prefabDir+modelName+".prefab", go, ReplacePrefabOptions.ConnectToPrefab);
					DestroyImmediate(go);
				}
			}
		}
	}
	
	static bool DetectWindows7or2008R2()
	{
		Version ver = Environment.OSVersion.Version;
		if(ver.Major == 6 && ver.Minor == 1)	// windows7& server2008R2
		{
			return true;
		}
		return false;
	}
	/// <summary>
	/// Executes a shell command synchronously.
	/// </summary>
	/// <param name="command">string command</param>
	/// <returns>string, as output of the command.</returns>
	public static void ExecuteCommandSync (string command)
	{
		try {
			// create the ProcessStartInfo using "cmd" as the program to be run,
			// and "/c " as the parameters. 
			// Incidentally, /c tells cmd that we want it to execute the command that follows,
			// and then exit.
			System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo ("cmd", "/c " + command);
			
			// The following commands are needed to redirect the standard output.
			// This means that it will be redirected to the Process.StandardOutput StreamReader.
			procStartInfo.RedirectStandardOutput = true;
			procStartInfo.UseShellExecute = false;
			// Do not create the black window.
			procStartInfo.CreateNoWindow = true;
			// Now we create a process, assign its ProcessStartInfo and start it
			System.Diagnostics.Process proc = new System.Diagnostics.Process ();
			proc.StartInfo = procStartInfo;
			proc.Start ();
			// Get the output into a string
			string result = proc.StandardOutput.ReadToEnd ();
			// Display the command output.
			Debug.Log(result);
		} catch {
			// Log the exception
		}
	}
}
