using UnityEngine;
using System.Collections.Generic;
using Pathea;

public class WorldCollider : MonoBehaviour
{
	private static WorldCollider s_Instance = null;
	public static WorldCollider Instance { get { return s_Instance; } }
	private static Rect s_Rect;
	private static Rect g_Rect
	{
		get
		{
			if(null != s_Instance)
				return s_Rect;
			return new Rect(0,500,18000,17500);
		}
	}
	
	public static bool IsPointInWorld( Vector3 point )
	{
		return g_Rect.Contains(new Vector2(point.x, point.z));
	}

	public Vector3[] storyBoarderLine;

	public Vector3[] demoBoarderLine;
	
	public float wallThickness = 3.0f;

	List<GameObject> m_BoarderObj;

	void Awake ()
	{
		s_Instance = this;
		m_BoarderObj = new List<GameObject>();
#if DemoVersion
		storyBoarderLine = demoBoarderLine;
#endif
	}

	void Start ()
	{
		if (PeGameMgr.IsSingleStory || PeGameMgr.IsMultiStory) 
		{
			if(storyBoarderLine.Length < 2)
				return;
			ResetBoarder(storyBoarderLine);
        }
		else if(PeGameMgr.randomMap)
		{
			Vector3[] boarderLine = new Vector3[4];
			boarderLine[0] = new Vector3(RandomMapConfig.Instance.boundaryWest-RandomMapConfig.Instance.BorderOffset, 0, RandomMapConfig.Instance.boundarySouth-RandomMapConfig.Instance.BorderOffset);
			boarderLine[1] = new Vector3(RandomMapConfig.Instance.boundaryWest-RandomMapConfig.Instance.BorderOffset, 0, RandomMapConfig.Instance.boundaryNorth+RandomMapConfig.Instance.BorderOffset);
			boarderLine[2] = new Vector3(RandomMapConfig.Instance.boundaryEast+RandomMapConfig.Instance.BorderOffset, 0, RandomMapConfig.Instance.boundaryNorth+RandomMapConfig.Instance.BorderOffset);
			boarderLine[3] = new Vector3(RandomMapConfig.Instance.boundaryEast+RandomMapConfig.Instance.BorderOffset, 0, RandomMapConfig.Instance.boundarySouth-RandomMapConfig.Instance.BorderOffset);
			ResetBoarder(boarderLine);
        }
	}

	void CreatWallCube(Vector3 pos1, Vector3 pos2)
	{
		Vector3 centPos = (pos1 + pos2) * 0.5f;
		Vector3 dir = pos2 - pos1;
		GameObject obj = new GameObject();
		BoxCollider col = obj.AddComponent<BoxCollider>();
		col.center = Vector3.zero;
		col.size = Vector3.one;
		obj.transform.parent = transform;
		obj.transform.position = new Vector3(centPos.x, VoxelTerrainConstants._worldSideLenY * 0.5f, centPos.z);
		obj.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
		obj.transform.localScale = new Vector3(wallThickness, VoxelTerrainConstants._worldSideLenY, dir.magnitude);
		m_BoarderObj.Add(obj);
	}

	public void ResetBoarder(Vector3[] boarderLine)
	{
		for(int i = 0; i < m_BoarderObj.Count; i++)
			GameObject.Destroy(m_BoarderObj[i]);

		m_BoarderObj.Clear();

		Vector3 minVec = boarderLine[0], maxVec = boarderLine[0];
		
		CreatWallCube(boarderLine[boarderLine.Length - 1], boarderLine[0]);
		
		for(int i = 0; i < boarderLine.Length - 1; i++)
		{
			minVec.x = Mathf.Min(minVec.x, boarderLine[i].x, boarderLine[i + 1].x);
			minVec.z = Mathf.Min(minVec.z, boarderLine[i].z, boarderLine[i + 1].z);
			maxVec.x = Mathf.Max(maxVec.x, boarderLine[i].x, boarderLine[i + 1].x);
			maxVec.z = Mathf.Max(maxVec.z, boarderLine[i].z, boarderLine[i + 1].z);
			CreatWallCube(boarderLine[i], boarderLine[i + 1]);
		}
		
		s_Rect.Set(minVec.x, minVec.z, maxVec.x - minVec.x, maxVec.z - minVec.z);
	}
}
