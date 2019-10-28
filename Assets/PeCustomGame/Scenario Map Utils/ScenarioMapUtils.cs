using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// 地图描述
public class ScenarioMapDesc
{
	public ScenarioMapDesc ()
	{
		UID = "";
		Name = "";
		Path = "";
	}

	public ScenarioMapDesc (string uid, string path)
	{
		UID = uid;
		Path = path;
		DirectoryInfo di = new DirectoryInfo (path);
		if (di != null)
		{
			Name = di.Name;
			Path = di.FullName;
		}
		else
		{
			Name = "No Name";
		}
	}

	public string UID;
	public string Name;
	public string Path;
}

// Custom 模式地图工具
public static class ScenarioMapUtils
{
	static Dictionary<string, ScenarioMapDesc> detectedMaps = new Dictionary<string, ScenarioMapDesc> ();

	// 获取指定路径下所有可用的 Custom 模式地图绝对路径及其 UID ，但不做文件完整性检测。
	public static ScenarioMapDesc[] GetMapList (string path)
	{
		string[] maps = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
		List<ScenarioMapDesc> mapList = new List<ScenarioMapDesc> ();
		foreach (string map in maps)
		{
			string uidfile = map + "/MAP.uid";
			if (File.Exists(uidfile))
			{
				string uid = Pathea.IO.StringIO.LoadFromFile(uidfile, System.Text.Encoding.UTF8);
				uid = uid.Trim();

				if (uid.Length != 32)
					continue;

				if (mapList.Find(iter => iter.UID == uid) != null)
					continue;

				ScenarioMapDesc desc = new ScenarioMapDesc(uid, map);
				mapList.Add(desc);
				detectedMaps[uid] = desc;
			}
		}
		return mapList.ToArray();
	}

	// 尝试获取指定 UID 的地图路径，不做文件完整性检测。
	public static ScenarioMapDesc GetMapByUID (string uid, string rootpath)
	{
		if (detectedMaps.ContainsKey(uid))
			return detectedMaps[uid];
		string[] mapuids = Directory.GetFiles(rootpath, "MAP.uid", SearchOption.AllDirectories);

		foreach (string mapuid in mapuids)
		{
			string tstuid = Pathea.IO.StringIO.LoadFromFile(mapuid, System.Text.Encoding.UTF8);
			tstuid = tstuid.Trim();
			if (tstuid == uid && tstuid.Length == 32)
			{
				FileInfo fi = new FileInfo (mapuid);
				ScenarioMapDesc retval = new ScenarioMapDesc(uid, fi.DirectoryName);
				detectedMaps[uid] = retval;
				return retval;
			}
		}

		return null;
	}

	// 获取指定地图的UID。
	public static string GetMapUID (string path)
	{
		string uidfile = path + "/MAP.uid";
		if (File.Exists(uidfile))
		{
			string uid = Pathea.IO.StringIO.LoadFromFile(uidfile, System.Text.Encoding.UTF8);
			uid = uid.Trim();
			if (uid.Length == 32)
				return uid;
		}
		return null;
	}

	// 检测指定 UID 的地图文件完整性。
	// 将开启一个新的线程，返回 ScenarioIntegrityCheck 对象以供读取检测结果。
	public static ScenarioIntegrityCheck CheckIntegrityByUID (string uid, string rootpath)
	{
		ScenarioMapDesc desc = GetMapByUID(uid, rootpath);
		if (desc == null)
			return new ScenarioIntegrityCheck("");
		return new ScenarioIntegrityCheck(desc.Path);
	}

	// 检测指定路径的地图文件完整性。
	// 将开启一个新的线程，返回 ScenarioIntegrityCheck 对象以供读取检测结果。
	public static ScenarioIntegrityCheck CheckIntegrityByPath (string path)
	{
		return new ScenarioIntegrityCheck(path);
	}
}
