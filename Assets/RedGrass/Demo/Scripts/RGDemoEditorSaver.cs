using UnityEngine;
using System.Collections;
using RedGrass;
using System.IO;

public class RGDemoEditorSaver : MonoBehaviour 
{
	public RGSimpleEditor editor;

	public static string s_FileDirectory
	{
		get
		{
			return Application.dataPath + "/Caches/";
		}
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 350, 50, 200, 50), "Save Caches"))
		{
			SaveCaches();
		}
	}

	void OnDestroy()
	{
		SaveCaches();
	}

	void Awake()
	{

	}
	
	public void SaveCaches()
	{
		if (editor == null || editor.isEmpty)
			return;



		string MyDocPath = "." ;

		if (!Directory.Exists( MyDocPath + "/Caches" ))
			Directory.CreateDirectory( MyDocPath + "/Caches" );

		string[] existFiles = Directory.GetFiles(MyDocPath + "/Caches/");

		string path = MyDocPath  + "/Caches/";

		// Find the cache version
		int version = -1;

		for (int i = 0; i < existFiles.Length; ++i)
		{
			// the expanded-name must be "gs"
			string ext = existFiles[i].Substring(existFiles[i].LastIndexOf(".") + 1);
			if ( ext != "gs")
				continue;

			using (FileStream exsit_file = new FileStream(existFiles[i], FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
                BinaryReader _in = new BinaryReader(exsit_file);
                int old_ver = _in.ReadInt32();
                version = old_ver > version ? old_ver : version;
                exsit_file.Close();
			}
		}

		version++;

		// Save the cachse
		using (FileStream fs = new FileStream(path + "grass_cache_" + version.ToString() + ".gs", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			BinaryWriter _w = new BinaryWriter(fs);
			RedGrassInstance[] add_grasses = editor.addGrasses;
			_w.Write(version);
			_w.Write(add_grasses.Length);
			foreach (RedGrassInstance rgi in add_grasses)
			{
				rgi.WriteToStream(_w);
			}
			fs.Close();
		}

		editor.Clear();
	}

}
