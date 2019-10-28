using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathea;


public class CampPathDb 
{
	static  Dictionary<string, PEPathData> m_PathDic;
	public static void  LoadData(string[] paths)
	{
		if(m_PathDic == null)
			m_PathDic = new Dictionary<string, PEPathData>();

		if(paths == null)
			return ;

		for(int i=0;i<paths.Length;i++)
		{
			GameObject pathObj = Resources.Load(paths[i]) as GameObject;
			if (pathObj != null)
			{
				PEPath pePath = pathObj.GetComponent<PEPath>();
				if (pePath != null)
				{
					PEPathData data;
					data.warpMode = pePath.wrapMode;
					data.path = GetPathWay(pePath.gameObject);
					
					//string name = PETools.PEUtil.ToPrefabName(pePath.name);
					if(!m_PathDic.ContainsKey(paths[i]))
						m_PathDic.Add(paths[i], data);
				}
			}
		}

	}

	public static void Release()
	{
		m_PathDic.Clear();
		m_PathDic = null;
	}

	public static PEPathData GetPathData(string str)
	{
		return m_PathDic[str];
	}

	private static Vector3[] GetPathWay(GameObject obj)
	{
		List<Vector3> pathList = new List<Vector3>();
		foreach (Transform tr in obj.transform)
		{
			pathList.Add(tr.position);
		}
		
		return pathList.ToArray();
	}
}
