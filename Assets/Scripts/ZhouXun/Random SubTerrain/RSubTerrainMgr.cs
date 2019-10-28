using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using SkillAsset;

public class RSubTerrainMgr : MonoBehaviour
{
	private static RSubTerrainMgr s_Instance = null;
	public static RSubTerrainMgr Instance { get { return s_Instance; } }
	
	//Just temp value
	static List<GlobalTreeInfo> picklist = new List<GlobalTreeInfo>();
	static Stack<GlobalTreeInfo> globalTreeInfos = new Stack<GlobalTreeInfo>();
	static IntVector3 temp = new IntVector3();
	static IntVector3 ipos = new IntVector3();
	static List<TreeInfo> treelist = new List<TreeInfo> ();
	
	// Terrain region ( Terrain always around camera )
	[SerializeField] private Bounds m_TerrainRegion;
	public Vector3 TerrainPos { get { return m_TerrainRegion.min; } }
	public List<int> ChunkListToRender()
	{
		List<int> cnk_list = new List<int> ();
		int xBegin = Mathf.FloorToInt(m_TerrainRegion.min.x / RSubTerrConstant.ChunkSizeF);
		int zBegin = Mathf.FloorToInt(m_TerrainRegion.min.z / RSubTerrConstant.ChunkSizeF);
		for ( int x = xBegin; x < xBegin + RSubTerrConstant.ChunkCountPerAxis.x; ++x )
		{
			for ( int z = zBegin; z < zBegin + RSubTerrConstant.ChunkCountPerAxis.z; ++z )
			{
				int cnkidx = RSubTerrUtils.ChunkPosToIndex(x, z);
				if ( m_Chunks.ContainsKey(cnkidx) && m_Chunks[cnkidx].TreeCount > 0 )
					cnk_list.Add(cnkidx);
			}
		}
		return cnk_list;
	}
	public bool IsChunkRendering(int index)
	{
		int x = RSubTerrUtils.IndexToChunkX(index);
		int z = RSubTerrUtils.IndexToChunkZ(index);
		return m_TerrainRegion.Contains(new Vector3((x+0.5f)*RSubTerrConstant.ChunkSizeF, 1, (z+0.5f)*RSubTerrConstant.ChunkSizeF));
	}
	
	// Data Chunks
	private Dictionary<int, RSubTerrainChunk> m_Chunks = null;
	public static RSubTerrainChunk ReadChunk( int index )
	{
		if ( s_Instance == null ) return null;
		if ( s_Instance.m_Chunks.ContainsKey(index) )
			return s_Instance.m_Chunks[index];
		else
			return null;
	}
	[SerializeField] private bool m_IsDirty = false;
	public bool IsDirty { get { return m_IsDirty; } set { m_IsDirty = value; } }
	
	// sync chunks data
	private List<int> _lstChnkIdxActive = new List<int>();
	private List<int> _lstChnkIdxToDel = new List<int>();
	public void SyncChunksData()
	{
		bool can_read = Monitor.TryEnter(VFDataRTGen.s_dicTreeInfoList);
		if ( can_read )
		{
			// Add trees
			_lstChnkIdxActive.Clear();
			foreach ( KeyValuePair<IntVector2, List<TreeInfo>> kvp in VFDataRTGen.s_dicTreeInfoList )
			{
				int cnkidx = RSubTerrUtils.TilePosToIndex(kvp.Key);
				_lstChnkIdxActive.Add(cnkidx);
				if ( m_Chunks.ContainsKey(cnkidx) )
					continue;
				int cnkx = RSubTerrUtils.IndexToChunkX(cnkidx);
				int cnkz = RSubTerrUtils.IndexToChunkZ(cnkidx);
				GameObject chunkgo = new GameObject ("Tile [" + cnkx + "," + cnkz + "]");
				chunkgo.transform.parent = s_Instance.ChunkGroup.transform;
				chunkgo.transform.position = new Vector3( (cnkx+0.5f)*RSubTerrConstant.ChunkSizeF, 0, (cnkz+0.5f)*RSubTerrConstant.ChunkSizeF );
				RSubTerrainChunk chunk = chunkgo.AddComponent<RSubTerrainChunk>();
				chunk.m_Index = cnkidx;
				s_Instance.m_Chunks.Add(cnkidx, chunk);
				foreach ( TreeInfo ti in kvp.Value )
				{
					bool was_deleted = false;
                    // Delete cutted trees
                    if (RSubTerrSL.m_mapDelPos != null)
                    {
                        for (int x = cnkx - 1; x <= cnkx + 1; ++x)
                        {
                            for (int z = cnkz - 1; z <= cnkz + 1; ++z)
                            {
                                int idx = RSubTerrUtils.ChunkPosToIndex(x, z);
                                if (RSubTerrSL.m_mapDelPos.ContainsKey(idx))
                                {
                                    foreach (Vector3 pos in RSubTerrSL.m_mapDelPos[idx])
                                    {
                                        float diff = Mathf.Abs(pos.x - ti.m_pos.x) + Mathf.Abs(pos.y - ti.m_pos.y) + Mathf.Abs(pos.z - ti.m_pos.z);
                                        if (diff < 0.05f)
                                        {
                                            was_deleted = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ( was_deleted )
						continue;

					chunk.AddTree(ti);
					int idx32 = RSubTerrUtils.Tree32PosTo32Index(Mathf.FloorToInt(ti.m_pos.x/32), Mathf.FloorToInt(ti.m_pos.z/32));
					if ( !m_map32Trees.ContainsKey(idx32) )
						m_map32Trees.Add(idx32, new List<TreeInfo>());
					if ( HasCollider(ti.m_protoTypeIdx) || HasLight(ti.m_protoTypeIdx) )
						m_map32Trees[idx32].Add(ti);
				}
				if ( IsChunkRendering(cnkidx) )
					m_IsDirty = true;
			}
			
			_lstChnkIdxToDel.Clear();
			foreach ( KeyValuePair<int, RSubTerrainChunk> kvp in m_Chunks )
			{
				if ( !_lstChnkIdxActive.Contains(kvp.Key) )
					_lstChnkIdxToDel.Add(kvp.Key);
			}
			foreach ( int key in _lstChnkIdxToDel )
			{
				RemoveChunk(key);
				if ( IsChunkRendering(key) )
					m_IsDirty = true;
			}
			Monitor.Exit(VFDataRTGen.s_dicTreeInfoList);
		}
	}
	
//	// Add tree to data chunk
//	public static void AddTree( TreeInfo treeinfo, int chunkidx )
//	{
//		if ( s_Instance == null ) return;
//		int cnkidx = RSubTerrUtils.GetChunkIdContainsTree(treeinfo.m_pos);
//		if ( !s_Instance.m_Chunks.ContainsKey(cnkidx) )
//		{
//			int cnkx = RSubTerrUtils.IndexToChunkX(cnkidx);
//			int cnkz = RSubTerrUtils.IndexToChunkZ(cnkidx);
//			GameObject chunkgo = new GameObject ("Node [" + cnkx + "," + cnkz + "]");
//			chunkgo.transform.parent = s_Instance.ChunkGroup.transform;
//			chunkgo.transform.position = new Vector3( cnkx*RSubTerrConstant.ChunkSizeF, 0, cnkz*RSubTerrConstant.ChunkSizeF );
//			s_Instance.m_Chunks.Add(cnkidx, chunkgo.AddComponent<RSubTerrainChunk>());
//		}
//		s_Instance.m_Chunks[cnkidx].AddTree(treeinfo);
//		if (s_Instance.m_TerrainRegion.Contains(treeinfo.m_pos))
//			s_Instance.m_IsDirty = true;
//	}
//	
//	// Delete tree from data chunk
//	public static void DelTree( TreeInfo treeinfo )
//	{
//		if ( s_Instance == null ) return;
//		int cnkidx = RSubTerrUtils.GetChunkIdContainsTree(treeinfo.m_pos);
//		if ( !s_Instance.m_Chunks.ContainsKey(cnkidx) )
//		{
//			Debug.LogError("The tree to del dosen't exist");
//			return;
//		}
//		s_Instance.m_Chunks[cnkidx].DelTree(treeinfo);
//		if (s_Instance.m_TerrainRegion.Contains(treeinfo.m_pos))
//			s_Instance.m_IsDirty = true;
//	}
//

	// Remove / disable a data chunk
	public void RemoveChunk( int index )
	{
		if ( m_Chunks.ContainsKey(index) )
		{
			GameObject.Destroy(m_Chunks[index].gameObject);
			m_Chunks.Remove(index);
			if ( IsChunkRendering(index) )
				m_IsDirty = true;
			
			#region DANGEROUS_CODE
			// !!!! Code below maybe dangerous !!!!
			// ( but it running well when chunksize = 32 )
			if ( m_map32Trees.ContainsKey(index) )
			{
				m_map32Trees[index].Clear();
				m_map32Trees.Remove(index);
			}
			#endregion
		}
	}
	
	// Tree maps
	public Dictionary<int, List<TreeInfo>> m_map32Trees = null;
	private Dictionary<int, List<GameObject>> m_mapExistTempTrees = null;
	private Dictionary<GameObject, TreeInfo> m_mapTempTreeInfos = null;
	
	#region EDITOR_VARS
	public Transform CameraTransform = null;
	public Transform PlayerTransform = null;
	public VoxelEditor VEditor = null;
	public List<LSubTerrLayerOption> Layers;
	public RSubTerrCreator [] LayerCreators;
	public GameObject ChunkGroup;
	public GameObject TerrainGroup;
	public GameObject PrototypeGroup = null;
	public GameObject TempTreesGroup = null;
	#endregion
	
	public static GameObject GO { get { return (s_Instance == null) ? (null) : (s_Instance.gameObject); } }
	public static IntVec3 CameraIntPos
	{
		get
		{
			return (s_Instance == null || s_Instance.CameraTransform == null) ? 
				   (IntVec3.zero) : 
				   RSubTerrUtils.IndexToChunkPos(RSubTerrUtils.GetChunkIdContainsTree(s_Instance.CameraTransform.position));
		}
	}
	
	// Global tree prototype lists
	public GameObject [] GlobalPrototypePrefabList = null;
	[HideInInspector] public float [] GlobalPrototypeBendFactorList = null;
	[HideInInspector] public Bounds [] GlobalPrototypeBounds = null;
	public GameObject [] GlobalPrototypeColliders = null;
	public GameObject [] GlobalPrototypeLights = null;
	private RTreePlaceHolderInfo [] GlobalPrototypeTPHInfo = null;
	public static bool HasCollider( int prototype )
	{
		if ( s_Instance == null )
			return false;
		if ( s_Instance.GlobalPrototypeColliders[prototype] == null )
			return false;
		if ( s_Instance.GlobalPrototypeColliders[prototype].GetComponent<Collider>() != null )
			return true;
		else
			return false;
	}
	public static bool HasLight( int prototype )
	{
		if ( s_Instance == null )
			return false;
		if ( s_Instance.GlobalPrototypeColliders[prototype] == null )
			return false;
		if ( s_Instance.GlobalPrototypeColliders[prototype].GetComponentsInChildren<Light>(true).Length > 0 )
			return true;
		else
			return false;
	}
	public static bool HasMultiCollider( int prototype )
	{
		if ( s_Instance == null )
			return false;
		if ( s_Instance.GlobalPrototypeColliders[prototype] == null )
			return false;
		if ( s_Instance.GlobalPrototypeColliders[prototype].GetComponent<BoxCollider>() != null )
			return true;
		else
			return false;
	}
	public static RTreePlaceHolderInfo GetTreePlaceHolderInfo( int prototype )
	{
		if ( s_Instance == null )
			return null;
		return s_Instance.GlobalPrototypeTPHInfo[prototype];
	}
		
	public const int TreePlaceHolderPrototypeIndex = 63;
	public const int NearTreeLayer = 21;
	public const int NearTreeLayerMask = 1 << NearTreeLayer;
	
	#region UNITY_INTERNAL_FUNCS
	void Awake ()
	{
		Debug.Log("Creating RSubTerrainMgr!");
		// Assign instance
		if ( s_Instance != null )
			Debug.LogError("Can not have a second instance of RSubTerrainMgr !");
		s_Instance = this;
    }
    void Init()
    {
		// Assign global tree prototype lists
		GlobalPrototypePrefabList = VEditor.m_treePrototypeList;
		GlobalPrototypeBendFactorList = VEditor.m_treePrototypeBendfactor;
		GlobalPrototypeBounds = new Bounds [GlobalPrototypePrefabList.Length];
		GlobalPrototypeColliders = new GameObject [GlobalPrototypePrefabList.Length];
		GlobalPrototypeLights = new GameObject [GlobalPrototypePrefabList.Length];
		GlobalPrototypeTPHInfo = new RTreePlaceHolderInfo [GlobalPrototypePrefabList.Length];
		
		// Get bounds
		for ( int i = 0; i < GlobalPrototypeBounds.Length; ++i )
		{
			GameObject go = Instantiate( GlobalPrototypePrefabList[i] ) as GameObject;
			go.transform.parent = PrototypeGroup.transform;
			go.transform.position = Vector3.zero;
			go.transform.rotation = Quaternion.identity;
			MeshRenderer mr = go.GetComponent<MeshRenderer>();
			MeshFilter mf = go.GetComponent<MeshFilter>();
			Animator ani = go.GetComponent<Animator>();
			Animation anim = go.GetComponent<Animation>();
			GlobalPrototypeBounds[i] = mf.mesh.bounds;
			if ( i == TreePlaceHolderPrototypeIndex )
				GlobalPrototypeBounds[i].extents = new Vector3 (1,2,1);
			Component.Destroy(mr);
			Component.Destroy(mf);

			if ( ani != null )
				Component.Destroy(ani);
			if ( anim != null )
				Component.Destroy(anim);
			if ( go.GetComponent<Collider>() != null || go.GetComponentsInChildren<Light>(true).Length >= 1 )
			{
				BoxCollider bc = go.GetComponent<BoxCollider>();
				if ( bc != null )
					GlobalPrototypeTPHInfo[i] = new RTreePlaceHolderInfo (bc.center, bc.size.y * 0.5f, bc.size.x * 0.5f + bc.size.z * 0.5f);
				else
					GlobalPrototypeTPHInfo[i] = null;
				//Collider[] cs = go.GetComponents<Collider>(); 
#if !UNITY_5
				foreach ( Collider c in cs )
				{
					c.isTrigger = true;
				}
#endif
				go.name = "Prototype [" + i.ToString() + "]'s Collider";
				go.SetActive(false);
				GlobalPrototypeColliders[i] = go;
			}
			else
			{
				Destroy(go);
				GlobalPrototypeColliders[i] = null;
			}
		}
	}
	
	// Use this for initialization
	void Start ()
	{
        Init();
		m_Chunks = new Dictionary<int, RSubTerrainChunk> ();
		m_map32Trees = new Dictionary<int, List<TreeInfo>> ();
		m_mapExistTempTrees = new Dictionary<int, List<GameObject>> ();
		m_mapTempTreeInfos = new Dictionary<GameObject, TreeInfo> ();
		m_TerrainRegion = new Bounds (Vector3.zero, Vector3.zero);
		
		LayerCreators = new RSubTerrCreator [Layers.Count];
		for ( int l = Layers.Count - 1; l >= 0; --l )
		{
			GameObject layer_go = new GameObject ("[" + l.ToString() + "] " + Layers[l].Name + " Layer");
			layer_go.transform.parent = TerrainGroup.transform;
			layer_go.transform.position = Vector3.zero;
			layer_go.transform.rotation = Quaternion.identity;
			RSubTerrCreator creator = layer_go.AddComponent<RSubTerrCreator>();
			creator.LayerIndex = l;
			LayerCreators[l] = creator;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
/*
		if (Input.GetKeyDown (KeyCode.Minus)) {
			if(Monitor.TryEnter(VFDataRTGen.s_dicTreeInfoList)){
				KeyValuePair<IntVector2, List<TreeInfo>> item = VFDataRTGen.s_dicTreeInfoList.ElementAt(0);
				string log = "Remove ter at "+item.Key.ToString()+item.Value.Count;
				foreach(TreeInfo ti in item.Value){
					log += ti.m_pos.ToString() + "," + ti.m_protoTypeIdx + "\n";
				}
				Debug.LogError(log);
				VFDataRTGen.s_dicTreeInfoList.Remove(item.Key);
				Monitor.Exit(VFDataRTGen.s_dicTreeInfoList);
			}
		}
*/

		// Determine node area
		IntVec3 ipos = CameraIntPos;
		Vector3 min = Vector3.zero;
		Vector3 max = Vector3.zero;
		if ( RSubTerrConstant.ChunkCountPerAxis.x % 2 == 0 )
			min.x = (ipos.x-(RSubTerrConstant.ChunkCountPerAxis.x/2-1)) * RSubTerrConstant.ChunkSizeF;
		else
			min.x = (ipos.x-(RSubTerrConstant.ChunkCountPerAxis.x/2)) * RSubTerrConstant.ChunkSizeF;
		if ( RSubTerrConstant.ChunkCountPerAxis.z % 2 == 0 )
			min.z = (ipos.z-(RSubTerrConstant.ChunkCountPerAxis.z/2-1)) * RSubTerrConstant.ChunkSizeF;
		else
			min.z = (ipos.z-(RSubTerrConstant.ChunkCountPerAxis.z/2)) * RSubTerrConstant.ChunkSizeF;
		min.y = 0;
		max.x = (ipos.x+(RSubTerrConstant.ChunkCountPerAxis.x/2)+1) * RSubTerrConstant.ChunkSizeF;
		max.y = RSubTerrConstant.ChunkHeightF;
		max.z = (ipos.z+(RSubTerrConstant.ChunkCountPerAxis.z/2)+1) * RSubTerrConstant.ChunkSizeF;
		
		m_TerrainRegion.SetMinMax(min, max);
		if ( Application.isEditor )
		{
			Bounds terr_region_base_bound = m_TerrainRegion;
			terr_region_base_bound.center = new Vector3 (terr_region_base_bound.center.x, 0, terr_region_base_bound.center.z);
			terr_region_base_bound.extents = new Vector3 (terr_region_base_bound.extents.x, 0, terr_region_base_bound.extents.z);
			AiUtil.DrawBounds(transform, terr_region_base_bound, Color.blue);
		}
		
		// Determine near tree area
        if ( PlayerTransform == null )
        {
			PlayerTransform = CameraTransform;
        }
        else
		{
			if ( Time.frameCount % 32 == 0 )
			{
				SyncChunksData();
			}
			if ( Time.frameCount % 128 == 0 && IsDirty )
			{
				foreach ( RSubTerrCreator cr in LayerCreators )
				{
					if ( !cr.bProcessing )
					{
						cr.StartCoroutine("RefreshRegion");
						m_IsDirty = false;
					}
				}
			}
			RefreshTempGOsIn32Meter();
		}
	}
	
	void OnDestroy ()
	{
		if ( m_Chunks != null )
		{
			m_Chunks.Clear();
			m_Chunks = null;
		}
		s_Instance = null;
	}
	#endregion

	
	public static TreeInfo GetTreeinfo(Collider col)
	{
		if(null == col || null == s_Instance)
			return null;
		
		if ( s_Instance.m_mapTempTreeInfos.ContainsKey(col.gameObject) )
		{
			return s_Instance.m_mapTempTreeInfos[col.gameObject];
		}
		
		return null;
	}
	
	public static TreeInfo RayCast(Ray ray, float distance, out RaycastHit hitinfo)
	{
		hitinfo = new RaycastHit ();
		if ( s_Instance == null )
			return null;
		if ( Physics.Raycast(ray, out hitinfo, distance, NearTreeLayerMask) )
		{
			if ( s_Instance.m_mapTempTreeInfos.ContainsKey(hitinfo.collider.gameObject) )
			{
				return s_Instance.m_mapTempTreeInfos[hitinfo.collider.gameObject];
			}
			else
			{
				return null;
			}
		}
		else
		{
			return null;
		}
	}
	public static TreeInfo RayCast(Ray ray, float distance)
	{
		RaycastHit rch;
		return RayCast(ray, distance, out rch);
	}
	
	public List<TreeInfo> TreesAtPos( IntVector3 pos )
	{
		treelist.Clear();
		int X = Mathf.FloorToInt(pos.x / RSubTerrConstant.ChunkSizeF);
		int Z = Mathf.FloorToInt(pos.z / RSubTerrConstant.ChunkSizeF);
		TreeInfo tmpTi;
		for ( int x = X - 1; x <= X + 1; ++x )
		{
			for ( int z = Z - 1; z <= Z + 1; ++z )
			{
				int idx = RSubTerrUtils.ChunkPosToIndex(x,z);
				int treeidx = RSubTerrUtils.TreeWorldPosToChunkIndex(pos.ToVector3(), idx);
				if ( m_Chunks.ContainsKey(idx) && m_Chunks[idx].m_mapTrees.TryGetValue(treeidx, out tmpTi) )
				{
					TreeInfo.AddTiToList(treelist, tmpTi);
				}
			}
		}
		return treelist;
	}

	public static TreeInfo TreesAtPosF(Vector3 pos)
	{
		IntVector3 ipos = new IntVector3 (Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		List<TreeInfo> list = s_Instance.TreesAtPos(ipos);
		foreach ( TreeInfo ti in list )
		{
			if ( Vector3.Distance(ti.m_pos, pos) < 0.005f ) 
				return ti;
		}
		return null;
	}

	public static List<GlobalTreeInfo> Picking(Vector3 position, Vector3 direction, bool includeTrees = false, float distance = 1.5f, float angle = 45f)
	{
		if ( s_Instance == null )
			return picklist;
		ClearTreeinfo();

		distance = Mathf.Clamp(distance, 0.1f, 2f);		
		ipos.x = Mathf.FloorToInt(position.x);
		ipos.y = Mathf.FloorToInt(position.y);
		ipos.z = Mathf.FloorToInt(position.z);

		for ( int x = ipos.x - 2; x <= ipos.x + 2; ++x )
		{
			for ( int y = ipos.y - 2; y <= ipos.y + 2; ++y )
			{
				for ( int z = ipos.z - 2; z <= ipos.z + 2; ++z )
				{
					temp.x = x;
					temp.y = y;
					temp.z = z;
					List<TreeInfo> list_currcell = s_Instance.TreesAtPos(temp);
					for (int i = 0; i < list_currcell.Count; ++i)
					{
						TreeInfo _ti = list_currcell[i];
						if ( HasCollider(_ti.m_protoTypeIdx) && !includeTrees )
							continue;
						Vector3 diff = _ti.m_pos - position;
						diff.y = 0;
						if ( diff.magnitude > distance )
							continue;
						direction.y = 0;
						if ( Vector3.Angle(diff, direction) < angle )
						{
							GlobalTreeInfo gti = GetGlobalTreeInfo();
							gti._terrainIndex = -1;
							gti._treeInfo = _ti;
							picklist.Add(gti);
						}
					}
				}
			}
		}
		return picklist;
	}
	
	public static List<GlobalTreeInfo> Picking(IntVector3 position, bool includeTrees)
	{
		if ( s_Instance == null )
			return picklist;
		ClearTreeinfo();

		ipos.x = position.x;
		ipos.y = position.y;
		ipos.z = position.z;

		List<TreeInfo> list_currcell = s_Instance.TreesAtPos(ipos);

		for(int i = 0; i < list_currcell.Count; ++i)
		{
			TreeInfo _ti = list_currcell[i];
			if ( HasCollider(_ti.m_protoTypeIdx) && !includeTrees )
				continue;
			
			GlobalTreeInfo gti = GetGlobalTreeInfo();
			gti._terrainIndex = -1;
			gti._treeInfo = _ti;
			picklist.Add(gti);
		}
		return picklist;
	}

	static void ClearTreeinfo()
	{
		for(int i = 0; i < picklist.Count; ++i)
			globalTreeInfos.Push(picklist[i]);
		picklist.Clear();
	}
	
	static GlobalTreeInfo GetGlobalTreeInfo()
	{
		GlobalTreeInfo gti;
		if(globalTreeInfos.Count > 0)
			gti = globalTreeInfos.Pop();
		else
			gti = new GlobalTreeInfo (-1, null);		
		return gti;
	}

	public static void DeleteTree(GameObject nearTreeGo)
	{
		if ( s_Instance == null )
			return;
		TreeInfo treeInfo = null;
		if(s_Instance.m_mapTempTreeInfos.TryGetValue(nearTreeGo, out treeInfo)){
			DeleteTree(treeInfo);
		}
	}
	public static void DeleteTree(TreeInfo treeinfo)
	{
		if ( s_Instance == null )
			return;
		if ( treeinfo == null )
			return;
		
		// For two feet trees
		TreeInfo SecondFoot = null;
		
		// Delete it in Mgr's m_map32Trees
		int idx32 = RSubTerrUtils.Tree32PosTo32Index(Mathf.FloorToInt(treeinfo.m_pos.x/32), Mathf.FloorToInt(treeinfo.m_pos.z/32));
		if ( s_Instance.m_map32Trees.ContainsKey(idx32) )
		{
			s_Instance.m_map32Trees[idx32].Remove(treeinfo);
			if ( s_Instance.m_map32Trees[idx32].Count == 0 )
			{
				s_Instance.m_map32Trees.Remove(idx32);
			}
		}
		
		// Delete it in Mgr's m_mapExistTempTrees and m_mapTempTreeInfos
		if ( s_Instance.m_mapExistTempTrees.ContainsKey(idx32) )
		{
			GameObject gameobject_to_delete = null;
			foreach ( GameObject go in s_Instance.m_mapExistTempTrees[idx32] )
			{
				if ( s_Instance.m_mapTempTreeInfos.ContainsKey(go) )
				{
					if ( s_Instance.m_mapTempTreeInfos[go] == treeinfo )
					{
						// Found it!
						gameobject_to_delete = go;
						GameObject.Destroy(go);
						s_Instance.m_mapTempTreeInfos.Remove(go);
					}
				}
				else
				{
					Debug.LogError("Can not find the GameObject key in m_mapTempTreeInfos when delete tree");
				}
			}
			if ( gameobject_to_delete != null )
				s_Instance.m_mapExistTempTrees[idx32].Remove(gameobject_to_delete);
		}
		
		// Delete it in Node's m_mapTrees and m_listTrees
		int X = Mathf.FloorToInt(treeinfo.m_pos.x / RSubTerrConstant.ChunkSizeF);
		int Z = Mathf.FloorToInt(treeinfo.m_pos.z / RSubTerrConstant.ChunkSizeF);
		int del_count = 0;
		for ( int x = X - 1; x <= X + 1; ++x )
		{
			for ( int z = Z - 1; z <= Z + 1; ++z )
			{
				int idx = RSubTerrUtils.ChunkPosToIndex(x,z);
				int treeidx = RSubTerrUtils.TreeWorldPosToChunkIndex(treeinfo.m_pos, idx);
				if ( s_Instance.m_Chunks.ContainsKey(idx) )
				{
					RSubTerrainChunk chunk = s_Instance.m_Chunks[idx];
					if ( chunk.TreeList.Remove(treeinfo) )
					{
						del_count++;
					}
					if ( TreeInfo.RemoveTiFromDict(chunk.m_mapTrees, treeidx, treeinfo) )
					{
						del_count++;
					}
				}
			}
		}
		if ( del_count != 2 )
		{
			Debug.LogError("RSubTerrain Remove: count doesn't match");
		}
		
		// Delete it in layers
		foreach ( RSubTerrCreator creator in s_Instance.LayerCreators )
		{
			creator.m_allTreesInLayer.Remove(treeinfo);
		}

        RSubTerrSL.AddDeletedTree(treeinfo);
		
		// Delete 2nd foot
		if ( SecondFoot != null )
		{
			DeleteTree(SecondFoot);
		}
	}
	public static void DeleteTreesAtPos(IntVector3 position)
	{
		List<TreeInfo> del_list = s_Instance.TreesAtPos(position);
		foreach ( TreeInfo ti in del_list )
		{
			DeleteTree(ti);
		}
	}
    public static TreeInfo TryGetTreeInfo(GameObject tree)
    {
        if (s_Instance == null)
            return null;
        TreeInfo treeInfo = null;
        if (s_Instance.m_mapTempTreeInfos.TryGetValue(tree, out treeInfo))
            return treeInfo;
        return null;
    }

	public static void DeleteTreesAtPosF(Vector3 pos)
	{
		IntVector3 ipos = new IntVector3 (Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		List<TreeInfo> del_list = s_Instance.TreesAtPos(ipos);
		foreach ( TreeInfo ti in del_list )
		{
			if ( Vector3.Distance(ti.m_pos, pos) < 0.005f ) 
				DeleteTree(ti);
		}
	}

    /// <summary>
    /// ����ģʽ���ڵ�ɾ����Χ��Դ
    /// </summary>
    /// <param name="digger"></param>
    /// <param name="position"></param>
    public static void DeleteTreesAtPosForMultiMode(/*SkillRunner digger,*/IntVector3 position)
    {
        List<TreeInfo> del_list = s_Instance.TreesAtPos(position);
        List<Vector3> del_PosList = new List<Vector3>();
        foreach (TreeInfo t in del_list)
        {
            del_PosList.Add(t.m_pos);
        }
		//if(del_PosList.Count>0)
		//{
		//	digger.RPC("RPC_C2S_DeleteTreeAtPosArea", del_PosList.ToArray(), position);
		//}
    }

    /// <summary>
    /// ����ģʽ�½���ɾ����Χ��Դ
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="position"></param>
    public static void DeleteTreesAtPosListForMultiMode(/*SkillRunner builder, */List<IntVector3> posList)
    {
        List<IntVector3> posList_del = new List<IntVector3>();
        List<Vector3> del_PosList = new List<Vector3>();
        if (posList.Count == 0)
        {
            return;
        }
        int xMin = posList[0].x;
        int xMax = posList[0].x;
        int yMin = posList[0].y;
        int yMax = posList[0].y;
        int zMin = posList[0].z;
        int zMax = posList[0].z;
        foreach(IntVector3 pos in posList)
        {
            if (pos.x<xMin)
            {
                xMin = pos.x;
            }
            if (pos.y < xMin)
            {
                yMin = pos.y;
            }
            if (pos.z < xMin)
            {
                zMin = pos.z;
            }
            if (pos.x>xMax)
            {
                xMax = pos.x;
            }
            if (pos.y > yMax)
            {
                yMax = pos.y;
            }
            if (pos.z > zMax)
            {
                zMax = pos.z;
            }
            List<TreeInfo> del_list = s_Instance.TreesAtPos(pos);
            foreach (TreeInfo t in del_list)
            {
                del_PosList.Add(t.m_pos);
            }
            if (del_list.Count>0)
            {
                posList_del.Add(pos);
            }
        }

        //����ɾ����
        for (int x = xMin; x < xMax + 1; x++)
        {
            for (int z = zMin; z < zMax + 1; z++)
            {
                for (int y = yMin - 2; y < yMin; y++) {
                    IntVector3 morePos = new IntVector3(x, y, z);

                    List<TreeInfo> del_list = s_Instance.TreesAtPos(morePos);
                    foreach (TreeInfo t in del_list)
                    {
                        del_PosList.Add(t.m_pos);
                    }
                    if (del_list.Count > 0)
                    {
                        posList_del.Add(morePos);
                    }
                }
            }
        }


		//if (del_PosList.Count > 0)
		//{
		//	builder.RPC("RPC_C2S_DeleteTreeAtPosListArea", del_PosList.ToArray(), posList_del.ToArray());
		//}
    }



	public void RefreshTempGOsIn32Meter()
	{
		#region _TEMP_TREE_BY_32_METER
		int x32 = Mathf.FloorToInt(PlayerTransform.position.x / 32);
		int z32 = Mathf.FloorToInt(PlayerTransform.position.z / 32);
		for ( int x = x32 - 2; x <= x32 + 2; ++x )
		{
			for ( int z = z32 - 2; z <= z32 + 2; ++z )
			{
				int idx = RSubTerrUtils.Tree32PosTo32Index(x,z);
				int oldCount = 0;
				int newCount = 0;
				if ( m_map32Trees.ContainsKey(idx) )
				{
					newCount = m_map32Trees[idx].Count;
				}
				if ( m_mapExistTempTrees.ContainsKey(idx) )
				{
					oldCount = m_mapExistTempTrees[idx].Count;
				}
				
				if ( newCount != oldCount )
				{
					// Delete old
					if ( oldCount != 0 )
					{
						foreach ( GameObject go in m_mapExistTempTrees[idx] )
						{
							m_mapTempTreeInfos.Remove(go);
							GameObject.Destroy(go);
						}
						m_mapExistTempTrees[idx].Clear();
						m_mapExistTempTrees.Remove(idx);
					}
					// Add new
					if ( newCount != 0 )
					{
						if ( !m_mapExistTempTrees.ContainsKey(idx) )
						{
							m_mapExistTempTrees.Add(idx, new List<GameObject> ());
						}
						List<GameObject> tmptreelist = m_mapExistTempTrees[idx];
						foreach ( TreeInfo _ti in m_map32Trees[idx] )
						{
							if ( GlobalPrototypeColliders[_ti.m_protoTypeIdx] == null )
								continue;
							GameObject temptree_go = GameObject.Instantiate( GlobalPrototypeColliders[_ti.m_protoTypeIdx], 
								                     _ti.m_pos, Quaternion.identity ) as GameObject;
							temptree_go.transform.parent = TempTreesGroup.transform;
							temptree_go.transform.localScale = new Vector3 (_ti.m_widthScale, _ti.m_heightScale, _ti.m_widthScale);
							temptree_go.name = temptree_go.transform.position.ToString() + " Type " + _ti.m_protoTypeIdx;
							temptree_go.layer = NearTreeLayer;
							temptree_go.SetActive(true);
							tmptreelist.Add(temptree_go);
							m_mapTempTreeInfos.Add(temptree_go, _ti);
						}
					}
				}
			}
		}
		List<int> keys_to_del = new List<int> ();
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_mapExistTempTrees )
		{
			IntVec3 pos32 = RSubTerrUtils.Tree32KeyTo32Pos(kvp.Key);
			if ( Mathf.Abs(pos32.x - x32) > 2 || Mathf.Abs(pos32.z - z32) > 2 )
			{
				keys_to_del.Add(kvp.Key);
				foreach ( GameObject go in kvp.Value )
				{
					m_mapTempTreeInfos.Remove(go);
					GameObject.Destroy(go);
				}
			}
		}
		foreach ( int k in keys_to_del )
		{
			m_mapExistTempTrees.Remove(k);
		}
		#endregion
	}
	
	public static void RefreshAllLayerTerrains()
	{
		if ( s_Instance == null )
			return;
		s_Instance.StartCoroutine("RefreshAllLayerTerrains_Coroutine");
	}
	
	IEnumerator RefreshAllLayerTerrains_Coroutine()
	{
		foreach ( RSubTerrCreator creator in LayerCreators )
		{
			while ( creator.bProcessing )
				yield return 0;
			creator.StartCoroutine("RefreshRegion");
		}		
	}
}
