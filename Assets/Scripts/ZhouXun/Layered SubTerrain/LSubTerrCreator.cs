using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LSubTerrCreator : MonoBehaviour
{
	public int LayerIndex = 0;
	public int xIndex = -1;
	public int zIndex = -1;
	public bool bProcessing = false;
	public bool bBillboardProcessing = false;

	public Dictionary<int, List<TreeInfo>> m_allTreesInLayer = null;

	public delegate void VoidNotify ();
	public static event VoidNotify OnRefreshRegion;

	public void AddTreeBatch( int index, List<TreeInfo> tree_list )
	{
		if ( m_allTreesInLayer == null )
			return;
		if ( m_allTreesInLayer.ContainsKey(index) )
		{
			Debug.LogError("Adding a batch of tree in this layer, but the index already exist in map, it will be replaced!");
			m_allTreesInLayer[index].Clear();
			m_allTreesInLayer[index] = tree_list;
		}
		else
		{
			m_allTreesInLayer.Add(index, tree_list);
		}
	}
	public void DelTreeBatch( int index )
	{
		if ( m_allTreesInLayer == null )
			return;
		if ( m_allTreesInLayer.ContainsKey(index) )
		{
			m_allTreesInLayer[index].Clear();
			m_allTreesInLayer.Remove(index);
		}
	}

	public TerrainData m_TerrData = null;
	public Dictionary<int, int> m_mapPrototype = null;
	[HideInInspector] public List<int> m_listPrototype = null;
	
	#region EDITOR_READONLY_VARS
	// Debug vars only !! Can NOT be used out of unity editor !!
	public int _TreePrototypeCount;
	public int _TreeInstanceCount;
	#endregion
	
	#region U3D_INTERNAL_PROCS
	void Awake ()
	{
		m_allTreesInLayer = new Dictionary<int, List<TreeInfo>> ();
		m_mapPrototype = new Dictionary<int, int> ();
		m_listPrototype = new List<int> ();
	}
	void Start ()
	{
		m_TerrData = new TerrainData ();
		m_TerrData.size = new Vector3 (LSubTerrConstant.SizeF * 3F, LSubTerrConstant.HeightF, LSubTerrConstant.SizeF * 3F);
		m_TerrData.heightmapResolution = 33;
		m_TerrData.baseMapResolution = 16;
		m_TerrData.alphamapResolution = 16;
		m_TerrData.SetDetailResolution(2,8);
		
		Terrain terr = gameObject.AddComponent<Terrain>();
		terr.terrainData = m_TerrData;
		terr.editorRenderFlags = ~TerrainRenderFlags.heightmap;
		terr.treeDistance = 1024F;
		terr.treeMaximumFullLODCount = 8192;
        terr.treeBillboardDistance = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
        terr.treeCrossFadeLength = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
		terr.gameObject.layer = Pathea.Layer.TreeStatic;
		
		TerrainCollider tc = gameObject.AddComponent<TerrainCollider>();
		tc.terrainData = m_TerrData;
	}
	void Update ()
	{
		if ( LSubTerrainMgr.Instance == null )
		{
			return;
		}
		
		// Get position
		IntVector3 iCamPos = LSubTerrainMgr.CameraPos;

		// Set graphic options
		Terrain terr = gameObject.GetComponent<Terrain>();
		if ( terr != null && LSubTerrainMgr.Instance != null)
		{
            terr.treeBillboardDistance = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
            terr.treeCrossFadeLength = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
		}
		
		// Need update ?
		if ( !bProcessing && !bBillboardProcessing && (iCamPos.x != xIndex || iCamPos.z != zIndex) )
		{
			Last_x = xIndex;
			Last_z = zIndex;
			StartCoroutine("RefreshRegion");	// use dirty flag to start refresh instead of checking position

			xIndex = iCamPos.x;
			zIndex = iCamPos.z;
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
			foreach ( KeyValuePair<int, List<TreeInfo>> kvp in m_allTreesInLayer )
			{
				kvp.Value.Clear();
			}
			m_allTreesInLayer.Clear();
			m_allTreesInLayer = null;
		}
	}
	#endregion

	private Dictionary<int, BillboardTerrain> m_BillboardTerrains = new Dictionary<int, BillboardTerrain>();
	private int Last_x = 0;
	private int Last_z = 0;
	private IEnumerator RefreshBillboards ()
	{
		// Check if the highest layer
		if ( LSubTerrainMgr.Instance.Layers[LayerIndex].MaxTreeHeight < 50 )
		{
			bBillboardProcessing = false;
			yield break;
		}

		bBillboardProcessing = true;

		// Delete Far BTerrains
		List<int> del_list = new List<int> ();
        long tempIndexX, tempIndexZ;
		foreach ( KeyValuePair<int, BillboardTerrain> kvp in m_BillboardTerrains )
		{
			IntVector3 pos = LSubTerrUtils.IndexToPos(kvp.Key);
            tempIndexX = pos.x - xIndex;
            tempIndexZ = pos.z - zIndex;
            //lz-2017.07.27 差值如果是Int.MinValue用Mathf.Abs会报错: OverflowException: Value is too small
            if (System.Math.Abs(tempIndexX) > 3 || System.Math.Abs(tempIndexZ) > 3
               || System.Math.Abs(tempIndexX) <= 1 && System.Math.Abs(tempIndexZ) <= 1)
            {
				kvp.Value.Reset();
				GameObject.Destroy(kvp.Value.gameObject);
				del_list.Add(kvp.Key);
			}
		}
		foreach ( int del in del_list )
			m_BillboardTerrains.Remove(del);

		// Add new BTerrains
		for ( int x = Last_x - 1; x <= Last_x + 1; ++x )
		{
			for ( int z = Last_z - 1; z <= Last_z + 1; ++z )
			{
				if ( x >= xIndex - 1 && x <= xIndex + 1 &&
				    z >= zIndex - 1 && z <= zIndex + 1 )
				{
					continue;
				}
				if ( x > xIndex + 3 || x < xIndex - 3 ||
				    z > zIndex + 3 || z < zIndex - 3 )
				{
					continue;
				}
				int idx = LSubTerrUtils.PosToIndex(x,z);
				while ( !m_allTreesInLayer.ContainsKey(idx) && LSubTerrainMgr.Node(idx) != null && LSubTerrainMgr.Node(idx).HasData )
				{
					yield return 0;
				}
				if ( m_allTreesInLayer.ContainsKey(idx) )
				{
					CreateOneBTerrain(x,z);
				}
			}
		}
		yield return 0;

		// Add new BTerrains
		for ( int x = xIndex - 3; x <= xIndex + 3; ++x )
		{
			for ( int z = zIndex - 3; z <= zIndex + 3; ++z )
			{
				if ( x >= xIndex - 1 && x <= xIndex + 1 &&
				    z >= zIndex - 1 && z <= zIndex + 1 )
				{
					continue;
				}
				if ( x >= Last_x - 1 && x <= Last_x + 1 &&
				    z >= Last_z - 1 && z <= Last_z + 1 )
				{
					continue;
				}
				int idx = LSubTerrUtils.PosToIndex(x,z);
				while ( !m_allTreesInLayer.ContainsKey(idx) && LSubTerrainMgr.Node(idx) != null && LSubTerrainMgr.Node(idx).HasData )
				{
					yield return 0;
				}
				if ( m_allTreesInLayer.ContainsKey(idx) )
				{
					CreateOneBTerrain(x,z);
				}
			}
			yield return 0;
		}
		Last_x = xIndex;
		Last_z = zIndex;
		bBillboardProcessing = false;
	}

	private void CreateOneBTerrain (int x, int z)
	{
		int idx = LSubTerrUtils.PosToIndex(x,z);
		List<TreeInfo> trees_in_zone = m_allTreesInLayer[idx];
		
		BillboardTerrain bt = BillboardTerrain.Instantiate(LSubTerrainMgr.Instance.BTerrainRes) as BillboardTerrain;
		bt.transform.parent = LSubTerrainMgr.Instance.BTerrainGroup.transform;
		bt.gameObject.name = "BTerrain [" + x.ToString() + " " + z.ToString() + "]";
		bt.transform.position = new Vector3 (x*LSubTerrConstant.SizeF, 0, z*LSubTerrConstant.SizeF);
		bt.transform.localScale = Vector3.one;
		bt.xPos = x;
		bt.zPos = z;
		bt.m_Center = new Vector3 ((xIndex+0.5f)*LSubTerrConstant.SizeF, 0, (zIndex+0.5f)*LSubTerrConstant.SizeF);
		
		if ( m_BillboardTerrains.ContainsKey(idx) )
		{
			m_BillboardTerrains[idx].Reset();
			GameObject.Destroy(m_BillboardTerrains[idx].gameObject);
			m_BillboardTerrains.Remove(idx);
		}
		bt.SetTrees(trees_in_zone);
		m_BillboardTerrains.Add(idx, bt);
	}

	private IEnumerator RefreshRegion ()
	{
		bProcessing = true;
		
		#region PREPARE_DATA
		yield return 0;
		m_mapPrototype.Clear();
		m_listPrototype.Clear();
		for ( int x = xIndex - 1; x <= xIndex + 1; ++x )
		{
			for ( int z = zIndex - 1; z <= zIndex + 1; ++z )
			{
				int idx = LSubTerrUtils.PosToIndex(x,z);
				while ( !m_allTreesInLayer.ContainsKey(idx) && LSubTerrainMgr.Node(idx) != null && LSubTerrainMgr.Node(idx).HasData )
				{
					yield return 0;
				}
				if ( m_allTreesInLayer.ContainsKey(idx) )
				{
					List<TreeInfo> trees_in_zone = m_allTreesInLayer[idx];
					foreach ( TreeInfo ti in trees_in_zone )
					{
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
			yield return 0;
		}
		
		TreePrototype [] FinalPrototypeArray = new TreePrototype [m_listPrototype.Count];
		for ( int i = 0; i < m_listPrototype.Count; ++i )
		{
			FinalPrototypeArray[i] = new TreePrototype ();
			FinalPrototypeArray[i].bendFactor = LSubTerrainMgr.Instance.GlobalPrototypeBendFactorList[m_listPrototype[i]];
			FinalPrototypeArray[i].prefab = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_listPrototype[i]];
		}
		
		// Calc Count
		int tree_count = 0;
		for ( int x = xIndex - 1; x <= xIndex + 1; ++x )
		{
			for ( int z = zIndex - 1; z <= zIndex + 1; ++z )
			{
				int idx = LSubTerrUtils.PosToIndex(x,z);
				if ( m_allTreesInLayer.ContainsKey(idx) )
				{
					tree_count += m_allTreesInLayer[idx].Count;
				}
			}
		}
		
		TreeInstance [] FinalTreeInstanceArray = new TreeInstance [tree_count];
		
		int t = 0;
		for ( int x = xIndex - 1; x <= xIndex + 1; ++x )
		{
			for ( int z = zIndex - 1; z <= zIndex + 1; ++z )
			{
				int idx = LSubTerrUtils.PosToIndex(x,z);
				if ( m_allTreesInLayer.ContainsKey(idx) )
				{
					List<TreeInfo> trees_in_zone = m_allTreesInLayer[idx];
					foreach ( TreeInfo ti in trees_in_zone )
					{
						if ( t < FinalTreeInstanceArray.Length )
						{
							Vector3 new_pos = ti.m_pos;
							new_pos += new Vector3 (x-xIndex+1, 0, z-zIndex+1);
							new_pos.x /= 3;
							new_pos.z /= 3;
							FinalTreeInstanceArray[t].color = ti.m_clr;
							FinalTreeInstanceArray[t].heightScale = ti.m_heightScale;
							FinalTreeInstanceArray[t].widthScale = ti.m_widthScale;
							FinalTreeInstanceArray[t].lightmapColor = ti.m_lightMapClr;
							FinalTreeInstanceArray[t].position = new_pos;
							if ( !m_mapPrototype.ContainsKey(ti.m_protoTypeIdx) )
							{
								FinalTreeInstanceArray[t].heightScale = 0;
								FinalTreeInstanceArray[t].widthScale = 0;
								FinalTreeInstanceArray[t].position = Vector3.zero;
								FinalTreeInstanceArray[t].prototypeIndex = 0;
								continue;
							}
							FinalTreeInstanceArray[t].prototypeIndex = m_mapPrototype[ti.m_protoTypeIdx];
						}
						t++;
					}
				}
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
		transform.position = LSubTerrUtils.PosToWorldPos(new IntVector3(xIndex - 1, 0, zIndex - 1));
		gameObject.SetActive(true);
		#endregion

		bProcessing = false;

		if ( OnRefreshRegion != null )
			OnRefreshRegion();
		StartCoroutine("RefreshBillboards");
	}
}
