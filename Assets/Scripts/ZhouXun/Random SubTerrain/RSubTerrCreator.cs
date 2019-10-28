using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RSubTerrCreator : MonoBehaviour
{
	public int LayerIndex = 0;
	public List<TreeInfo> m_allTreesInLayer = null;
	
	public TerrainData m_TerrData = null;
	public Dictionary<int, int> m_mapPrototype = null;
	[HideInInspector] public List<int> m_listPrototype = null;
	
	#region EDITOR_READONLY_VARS
	// Debug vars only !! Can NOT be used out of unity editor !!
	public int _TreePrototypeCount;
	public int _TreeInstanceCount;
	#endregion
	
	public bool bProcessing = false;
	private Vector3 _TargetPos = Vector3.zero;
	
	#region UNITY_INTERNAL_FUNCS
	// Use this for initialization
	void Awake ()
	{
		m_allTreesInLayer = new List<TreeInfo> ();
		m_mapPrototype = new Dictionary<int, int> ();
		m_listPrototype = new List<int> ();
	}
	void Start ()
	{
		m_TerrData = new TerrainData ();
		m_TerrData.size = RSubTerrConstant.TerrainSize;
		m_TerrData.heightmapResolution = 33;
		m_TerrData.baseMapResolution = 16;
		m_TerrData.alphamapResolution = 16;
		m_TerrData.SetDetailResolution(2,8);
		
		Terrain terr = gameObject.AddComponent<Terrain>();
		terr.terrainData = m_TerrData;
		terr.editorRenderFlags = ~TerrainRenderFlags.heightmap;
		terr.treeDistance = 1024F;
		terr.treeMaximumFullLODCount = 8192;
        terr.treeBillboardDistance = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
        terr.treeCrossFadeLength = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
		
		TerrainCollider tc = gameObject.AddComponent<TerrainCollider>();
		tc.terrainData = m_TerrData;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( RSubTerrainMgr.Instance == null ) return;
		
		// Set graphic options
		Terrain terr = gameObject.GetComponent<Terrain>();
		if ( terr != null )
		{
            terr.treeBillboardDistance = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
            terr.treeCrossFadeLength = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
		}
		
		// Need update ?
		bool bNeedupdate = false;
		if ( (transform.position - RSubTerrainMgr.Instance.TerrainPos).magnitude > 1 )
			bNeedupdate = true;
		
		if ( !bProcessing && bNeedupdate )
		{
			_TargetPos = RSubTerrainMgr.Instance.TerrainPos;
			StartCoroutine("RefreshRegion");
			//Debug.Log("Coroutine RSubTerrCreator[" + LayerIndex.ToString() + "]::RefreshRegion started");
		}
	}
	void OnDestroy ()
	{
		if ( m_TerrData != null )
		{
			Object.Destroy(m_TerrData);
			m_TerrData = null;
		}
		if ( m_allTreesInLayer != null )
		{
			m_allTreesInLayer.Clear();
			m_allTreesInLayer = null;
		}
	}
	
	#endregion
	private IEnumerator RefreshRegion ()
	{
		bProcessing = true;
		
		#region PREPARE_DATA
		yield return 0;
		m_allTreesInLayer.Clear();
		m_mapPrototype.Clear();
		m_listPrototype.Clear();
		
		List<int> cnk_list_to_render = RSubTerrainMgr.Instance.ChunkListToRender();
		int cnk_count = cnk_list_to_render.Count;
		for ( int i = 0; i < cnk_count; ++i )
		{
			int idx = cnk_list_to_render[i];
			List<TreeInfo> trees_in_zone = RSubTerrainMgr.ReadChunk(idx).TreeList;
			foreach ( TreeInfo ti in trees_in_zone )
			{
				// Add tree
				float height = RSubTerrainMgr.Instance.GlobalPrototypeBounds[ti.m_protoTypeIdx].extents.y * 2F;
				// Trees in this layer
				if ( RSubTerrainMgr.Instance.Layers[LayerIndex].MinTreeHeight <= height 
					 && height < RSubTerrainMgr.Instance.Layers[LayerIndex].MaxTreeHeight )
				{
					m_allTreesInLayer.Add(ti);
					// New prototype ?
					if ( !m_mapPrototype.ContainsKey(ti.m_protoTypeIdx) )
					{
						int next_index = m_listPrototype.Count;
						m_mapPrototype.Add(ti.m_protoTypeIdx, next_index);
						m_listPrototype.Add(ti.m_protoTypeIdx);
					}
				}
			}
		}
		
		TreePrototype [] FinalPrototypeArray = new TreePrototype [m_listPrototype.Count];
		for ( int i = 0; i < m_listPrototype.Count; ++i )
		{
			FinalPrototypeArray[i] = new TreePrototype ();
			FinalPrototypeArray[i].bendFactor = RSubTerrainMgr.Instance.GlobalPrototypeBendFactorList[m_listPrototype[i]];
			FinalPrototypeArray[i].prefab = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_listPrototype[i]];
		}
		
		yield return 0;
		
		int tree_cnt = m_allTreesInLayer.Count;
		TreeInstance [] FinalTreeInstanceArray = new TreeInstance [tree_cnt];
		
		for ( int t = 0; t < tree_cnt; ++t )
		{
			TreeInfo ti = m_allTreesInLayer[t];
			Vector3 new_pos = ti.m_pos - _TargetPos;
			new_pos.x /= RSubTerrConstant.TerrainSize.x;
			new_pos.y /= RSubTerrConstant.TerrainSize.y;
			new_pos.z /= RSubTerrConstant.TerrainSize.z;
			
#if false
			if (new_pos.x < 0 || new_pos.y < 0 || new_pos.z < 0 ||
				new_pos.x > 1 || new_pos.y > 1 || new_pos.z > 1 )
			{
				Debug.LogError("a tree was out of terrain bound, error!");
				continue;
			}		
#endif
			if(m_mapPrototype.ContainsKey(ti.m_protoTypeIdx)){	// Add key present check
				FinalTreeInstanceArray[t].color = ti.m_clr;
				FinalTreeInstanceArray[t].heightScale = ti.m_heightScale;
				FinalTreeInstanceArray[t].widthScale = ti.m_widthScale;
				FinalTreeInstanceArray[t].lightmapColor = ti.m_lightMapClr;
				FinalTreeInstanceArray[t].position = new_pos;
				FinalTreeInstanceArray[t].prototypeIndex = m_mapPrototype[ti.m_protoTypeIdx];
			}
		}

		yield return 0;
		#endregion
		
		#region ASSIGN_DATA
		gameObject.SetActive(false);
		m_TerrData.treeInstances = new TreeInstance[0] ;
		m_TerrData.treePrototypes = FinalPrototypeArray;
		m_TerrData.treeInstances = FinalTreeInstanceArray;
		
		if ( Application.isEditor )
		{
			_TreePrototypeCount = m_TerrData.treePrototypes.Length;
			_TreeInstanceCount = m_TerrData.treeInstances.Length;
		}
		transform.position = _TargetPos;
		gameObject.SetActive(true);
		#endregion
		
		bProcessing = false;
	}
}
