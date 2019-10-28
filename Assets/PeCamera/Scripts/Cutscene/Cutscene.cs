using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public static class Cutscene
{
	private static Dictionary<int, string> paths = null;
	public static void LoadData ()
	{
		paths = new Dictionary<int, string> ();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("CutsceneClip");
		while (reader.Read())
		{
			int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			string path = reader.GetString(reader.GetOrdinal("clippath"));
			paths[id] = path;
		}
	}

	public static CutsceneClip PlayClip (int id)
	{
		if (paths.ContainsKey(id))
		{
			GameObject res = Resources.Load(paths[id]) as GameObject;
			GameObject go = GameObject.Instantiate(res) as GameObject;
			go.name = res.name;
			go.transform.position = res.transform.position;
			go.transform.rotation = res.transform.rotation;
			CutsceneClip clip = go.GetComponent<CutsceneClip>();
			clip.isEditMode = false;
			return clip;
		}
		return null;
	}
	public static bool TooFar(int id)
	{
		if (paths.ContainsKey(id))
		{
			GameObject res = Resources.Load(paths[id]) as GameObject;
			if(res == null)
				return false;
			if (Block45Man.self.LodMan._Lod0ViewBounds.Contains(res.transform.position))
			{
				return false;
			}
		}
		return true;
	}
}
