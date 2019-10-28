using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(uLink.RegisterPrefabs))]
public class RegisterPrefabs : MonoBehaviour
{
	public List<string> Paths = new List<string>();
	private uLink.RegisterPrefabs Register;
	
	public void SavePath()
	{
		string filePath = Path.Combine(Application.dataPath, "PrefabsPath.txt");
		using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
		{
			if (null != fs)
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					if (null != sw)
					{
						foreach (string path in Paths)
							sw.WriteLine(path);
					}
				}
			}
		}
	}
#if UNITY_EDITOR	
	public void Reimport()
	{
		string filePath = Path.Combine(Application.dataPath, "PrefabsPath.txt");
		using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
		{
			if (null != fs)
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					Paths.Clear();
					while (!sr.EndOfStream)
						Paths.Add(sr.ReadLine());
				}
			}
		}
		
		Register = gameObject.GetComponent<uLink.RegisterPrefabs>();
		if (null == Register)
		{
			Debug.LogError("No script uLink.RegisterPrefabs.");
			return;
		}
		
		Register.prefabs.Clear();
		Register.replaceIfExists = true;
		
		foreach (string path in Paths)
		{
			string newPath = Path.Combine(Application.dataPath, path);
			if (Directory.Exists(newPath))
			{
				string[] files = Directory.GetFiles(newPath, "*.prefab");
				foreach (string file in files)
				{
					string fileName = @"Assets/" + path + "/" + Path.GetFileName(file);

					GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(fileName, typeof(GameObject)) as GameObject;

					if (null != obj)
					{
						uLink.NetworkView view = obj.GetComponent<uLink.NetworkView>();
						if (null == view)
							view = obj.AddComponent<uLink.NetworkView>();
						
						Register.prefabs.Add(obj);
					}
					else
					{
						Debug.LogError(fileName + " load failed.");
					}
				}
			}
			else
			{
				Debug.LogError(newPath + " does not exists.");
			}
		}
	}
#endif
}
