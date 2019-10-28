using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RSubTerrainChunk : MonoBehaviour
{
	public int m_Index = -1;
	public int XIndex { get { return RSubTerrUtils.IndexToChunkX(m_Index); } }
	public int ZIndex { get { return RSubTerrUtils.IndexToChunkZ(m_Index); } }
	public Vector3 wPos { get { return new Vector3(XIndex * RSubTerrConstant.ChunkSizeF,0,ZIndex * RSubTerrConstant.ChunkSizeF); } }
	
	private List<TreeInfo> m_listTrees = null;
	public List<TreeInfo> TreeList { get { return m_listTrees; } }
	public Dictionary<int, TreeInfo> m_mapTrees = null;
	public int TreeCount { get { return m_listTrees.Count; } }
	
	public void AddTree(TreeInfo ti)
	{
		m_listTrees.Add(ti);
		int tmpKey = RSubTerrUtils.TreeWorldPosToChunkIndex(ti.m_pos, m_Index);
		TreeInfo tmpTi;
		if (m_mapTrees.TryGetValue (tmpKey, out tmpTi)) {
			tmpTi.AttachTi(ti);
		} else {
			m_mapTrees.Add(tmpKey, ti);
		}
	}
	
	public void DelTree(TreeInfo ti)
	{
		int tmpKey = RSubTerrUtils.TreeWorldPosToChunkIndex(ti.m_pos, m_Index);
		if ( !TreeInfo.RemoveTiFromDict (m_mapTrees, tmpKey, ti) )
		{
			Debug.LogError("The tree to del dosen't exist");
			return;
		}
		if (!m_listTrees.Remove(ti))
		{
			Debug.LogError("The tree to del dosen't exist");
		}
	}
	public void Clear()
	{
		m_listTrees.Clear();
		m_mapTrees.Clear();
	}
	
	#region UNITY_INTERNAL_FUNCS
	void Awake ()
	{
		m_listTrees = new List<TreeInfo> ();
		m_mapTrees = new Dictionary<int, TreeInfo> ();
	}
	
#if false
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Application.isEditor )
		{
			// Draw a square in the scene
			float gap = 0.5f;
			Vector3 _va = wPos + (Vector3.right + Vector3.forward) * gap;
			Vector3 _vb = wPos + Vector3.right * (RSubTerrConstant.ChunkSizeF - gap) + Vector3.forward * gap;
			Vector3 _vc = wPos + (Vector3.right + Vector3.forward) * (RSubTerrConstant.ChunkSizeF - gap);
			Vector3 _vd = wPos + Vector3.forward * (RSubTerrConstant.ChunkSizeF - gap) + Vector3.right * gap;
			Color _linec = m_listTrees.Count > 0 ? Color.green : Color.white;
			Debug.DrawLine(_va, _vb, _linec);
			Debug.DrawLine(_vb, _vc, _linec);
			Debug.DrawLine(_vc, _vd, _linec);
			Debug.DrawLine(_vd, _va, _linec);
		}
	}
#endif
	#endregion
}
