using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Xml;

public class ScenarioIntegrityCheck
{
	// 地图是否通过完整性检查：null 正在执行检查，true 通过，false 未通过。
	public bool? integrated { get; private set; }
	// 未通过完整性检查的错误报告
	public string error { get; private set; }
	
	private bool used = false;
	private string path = "";
	private string uid = "";

	private string [] worldFiles = new string[]
	{
		"map_x0_y0.voxelform",
		"map_x0_y0_1.voxelform",
		"map_x0_y0_2.voxelform",
		"map_x0_y0_3.voxelform",
		"map_x0_y0_4.voxelform",
		"water_x0_y0.voxelform",
		"water_x0_y0_1.voxelform",
		"water_x0_y0_2.voxelform",
		"water_x0_y0_3.voxelform",
		"water_x0_y0_4.voxelform",
		"subter.dat",
		"subTerG_x0_y0.dat",
		"ProjectSettings.dat",
		"Minimap.png",
		"ProjectData.xml",
		"WorldEntity.xml"
	};
	private string [] worldFileCodes = new string[]
	{
		"T0", "T1", "T2", "T3", "T4",
		"W0", "W1", "W2", "W3", "W4",
		"TR", "GR",
		"PJ", "MM", "PD", "WD"
	};
	private string sp = ",sdof3.21d';;dasmv.23vla.D.FAdavaz;;s";
	
	public ScenarioIntegrityCheck (string scenario_path)
	{
		if (!used)
		{
			try
			{
				// Reset
				used = true;
				path = scenario_path;
				integrated = null;
				error = "";

				// Invalid path ?
				if (string.IsNullOrEmpty(path))
				{
					integrated = false;
					error = "Map file doesn't exist";
					return;
				}

				path = scenario_path + "/";
				string uidfile = path + "MAP.uid";

				if (!File.Exists(uidfile))
				{
					integrated = false;
					error = "Map uid file doesn't exist";
					return;
				}

				// Read UID
				uid = Pathea.IO.StringIO.LoadFromFile(uidfile, System.Text.Encoding.UTF8);
				uid = uid.Trim();
				if (uid.Length != 32)
				{
					integrated = false;
					error = "Invalid UID";
					return;
				}

				// Main Check Thread
				Thread TaskThread = new Thread(new ThreadStart(CheckThread));
				TaskThread.Start();
			}
			catch (Exception e)
			{
				error = "TryCatch " + e.ToString();
				integrated = false;
				return;
			}
		}
	}

	public class Signature
	{
		public Signature (string _code)
		{
			code = _code;
			sig = "";
		}
		
		public Signature (string _code, string _sig)
		{
			code = _code;
			sig = _sig;
		}
		
		public string code;
		public string sig;
		
		public static int Compare (Signature lhs, Signature rhs)
		{
			return string.Compare(lhs.code, rhs.code);
		}
	}
	
	List<Signature> signatures = null;

	private void CheckThread ()
	{
		try
		{
			signatures = new List<Signature> ();

			string sroot = path + "Scenario/";
			string wroot = path + "Worlds/";
			string mroot = sroot + "Missions/";
			string fpath = sroot + "ForceSettings.xml";
			string wpath = sroot + "WorldSettings.xml";

			// World settings
			if (!AddFileSig("_WS", wpath))
			{
				integrated = false;
				error = "WorldSettings.xml corrupt";
				return;
			}

			// Force settings
			if (!AddFileSig("_FS", fpath))
			{
				integrated = false;
				error = "ForceSettings.xml corrupt";
				return;
			}

			// Missions
			if (Directory.Exists(mroot))
			{
				string[] mfs = Directory.GetFiles(mroot);
				foreach (string mf in mfs)
				{
					FileInfo fi = new FileInfo (mf);
					int id = 0;
					if (fi.Name.Length < 5)
					{
						integrated = false;
						error = "Corrupt mission filename (5)";
						return;
					}
					string idstr = fi.Name.Substring(0, fi.Name.Length - 4);
					if (int.TryParse(idstr, out id))
					{
						if (!AddFileSig("M" + idstr, mf))
						{
							integrated = false;
							error = "Mission " + idstr + " corrupt";
							return;
						}
					}
					else
					{
						integrated = false;
						error = "Corrupt mission filename (x)";
						return;
					}
				}
			}
			else
			{
				integrated = false;
				error = "Mission folder lost";
				return;
			}

			// Worlds
			if (Directory.Exists(wroot))
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(wpath);
				List<string> worldpaths = new List<string> ();
				foreach (XmlNode node in doc.DocumentElement.ChildNodes)
				{
					if (node.Name == "WORLD")
					{
						worldpaths.Add(node.Attributes["path"].Value);
					}
					else
					{
						integrated = false;
						error = "WorldSettings.xml corrupt";
						return;
					}
				}

				for (int w = 0; w < worldpaths.Count; ++w)
				{
					string worldpath = wroot + worldpaths[w] + "/";
					for (int i = 0; i < worldFiles.Length; ++i)
					{
						string filepath = worldpath + worldFiles[i];
						AddFileSig("W" + w.ToString() + worldFileCodes[i], filepath);
					}
				}
			}
			else
			{
				integrated = false;
				error = "World folder lost";
				return;
			}

			// Hash
			signatures.Sort(Signature.Compare);
			string s = "";
			for (int i = 0; i < signatures.Count; ++i)
			{
				s += signatures[i].code;
				s += ":";
				s += signatures[i].sig;
				s += ";";
			}

			string tstuid = MD5Hash.MD5Encoding(s + sp);
			if (uid == tstuid)
			{
				integrated = true;
				return;
			}
			else
			{
				integrated = false;
				error = "One or more file has been tampered";
				return;
			}
		}
		catch (Exception e)
		{
			error = "TryCatch " + e.ToString();
			integrated = false;
			return;
		}
	}

	private bool AddFileSig (string code, string filepath)
	{
		Signature sig = new Signature(code);
		if (File.Exists(filepath))
		{
			try
			{
				using (FileStream fs = new FileStream (filepath, FileMode.Open, FileAccess.Read))
				{
					sig.sig = MD5Hash.MD5Encoding(fs);
					signatures.Add(sig);
					return true;
				}
			}
			catch (Exception) { return false; }
		}
		else
		{
			return false;
		}
	}
}
