using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// The layered-subterrain Manager
// zhouxun
public class LSubTerrainMgr : MonoBehaviour
{
	public enum ColType
	{
		None,
		Collider,
		BoxCollider,
	}
	private static LSubTerrainMgr s_Instance = null;
	public static LSubTerrainMgr Instance { get { return s_Instance; } }

    public static Action<GameObject> OnTreeColliderCreated;
    public static Action<GameObject> OnTreeColliderDestroy;

    private LSubTerrIO m_IO = null;	
	private static IntVector3 _iTmpCamPos = new IntVector3();
	private static IntVector3 _iCurCamPos = new IntVector3();
	public static IntVector3 CameraPos { get { return _iCurCamPos; } }
	public static LSubTerrIO IO { get { return (s_Instance == null) ? (null) : (s_Instance.m_IO); } }	
	public static GameObject GO { get { return (s_Instance == null) ? (null) : (s_Instance.gameObject); } }
	
	private Dictionary<int, LSubTerrain> m_Nodes = null;
	public static LSubTerrain Node( int index ) { return (s_Instance == null) ? (null) : (s_Instance.m_Nodes.ContainsKey(index) ? s_Instance.m_Nodes[index] : null); }
	public Dictionary<int, List<TreeInfo>> m_map32Trees = null;
	private Dictionary<int, List<GameObject>> m_mapExistTempTrees = null;
	private Dictionary<GameObject, GlobalTreeInfo> m_mapTempTreeInfos = null;
	
	// Global tree prototype lists
	public GameObject [] GlobalPrototypePrefabList = null;
	[HideInInspector] public float [] GlobalPrototypeBendFactorList = null;
	public Material [] GlobalPrototypeBillboardList = null;
	[HideInInspector] public Bounds [] GlobalPrototypeBounds = null;
	public ColType [] GlobalPrototypeCollidersType = null;
	public GameObject [] GlobalPrototypeColliders = null;
	public GameObject [] GlobalPrototypeLights = null;
	public BillboardTerrain BTerrainRes;
	private LTreePlaceHolderInfo [] GlobalPrototypeTPHInfo = null;

	//Just temp value
	static List<GlobalTreeInfo> picklist = new List<GlobalTreeInfo>();
	static Stack<GlobalTreeInfo> globalTreeInfos = new Stack<GlobalTreeInfo>();
	static IntVector3 temp = new IntVector3();
	public static bool HasCollider( int prototype )
	{
		if ( s_Instance != null && s_Instance.GlobalPrototypeCollidersType[prototype] != ColType.None )
			return true;
		return false;
	}
	public static bool HasMultiCollider( int prototype )
	{
		if ( s_Instance != null && s_Instance.GlobalPrototypeCollidersType[prototype] == ColType.BoxCollider )
			return true;
		return false;
	}
	public static bool HasLight( int prototype )
	{
		if ( s_Instance != null && s_Instance.GlobalPrototypeLights[prototype] != null )
			return true;
		return false;
	}
	public static LTreePlaceHolderInfo GetTreePlaceHolderInfo( int prototype )
	{
		if ( s_Instance == null )
			return null;
		return s_Instance.GlobalPrototypeTPHInfo[prototype];
	}

	public const int TreePlaceHolderPrototypeIndex = 63;
	public const int NearTreeLayer = 21;
	public const int NearTreeLayerMask = 1 << NearTreeLayer;
	
	#region EDITOR_VARS
	public Transform CameraTransform = null;
	public Transform PlayerTransform = null;
	public VoxelEditor VEditor = null;
	public int NumDataExpands = 1;
	public int ExtraCacheSize = 23;
	public List<LSubTerrLayerOption> Layers;
	public GameObject LayerGroup = null;
	public GameObject BTerrainGroup = null;
	public LSubTerrCreator [] LayerCreators;
	public GameObject PrototypeGroup = null;
	public GameObject TempTreesGroup = null;
	#endregion
	
	private float m_BeginTime = 0;
	private float m_LifeTime = 0;
	private float m_LifeFrame = 0;
	private bool _bDataDirty = false;
	
	public LSubTerrEditor m_Editor;
	
	#region U3D_INTERNAL_PROCS
	void Awake ()
	{
		Debug.Log("Creating LSubTerrainMgr!");
		// Assign instance
		if ( s_Instance != null )
			Debug.LogError("Can not have a second instance of LSubTerrainMgr !");
		s_Instance = this;
    }

    void Init()
    {
		// Assign global tree prototype lists
		GlobalPrototypePrefabList = VEditor.m_treePrototypeList;
		GlobalPrototypeBendFactorList = VEditor.m_treePrototypeBendfactor;
		GlobalPrototypeBounds = new Bounds [GlobalPrototypePrefabList.Length];
		GlobalPrototypeCollidersType = new ColType[GlobalPrototypePrefabList.Length];
		GlobalPrototypeColliders = new GameObject [GlobalPrototypePrefabList.Length];
		GlobalPrototypeLights = new GameObject [GlobalPrototypePrefabList.Length];
		GlobalPrototypeTPHInfo = new LTreePlaceHolderInfo [GlobalPrototypePrefabList.Length];
		
		// Get bounds
		for ( int i = 0; i < GlobalPrototypeBounds.Length; ++i )
		{
			{
				GameObject go = Instantiate( GlobalPrototypePrefabList[i] ) as GameObject;
				go.transform.parent = PrototypeGroup.transform;
				go.transform.position = Vector3.zero;
				go.transform.rotation = Quaternion.identity;
				MeshRenderer mr = go.GetComponent<MeshRenderer>();
				MeshFilter mf = go.GetComponent<MeshFilter>();
//				Light[] ls = go.GetComponentsInChildren<Light>();
				Animator ani = go.GetComponent<Animator>();
				Animation anim = go.GetComponent<Animation>();
				GlobalPrototypeBounds[i] = mf.mesh.bounds;
				if ( i == TreePlaceHolderPrototypeIndex )
					GlobalPrototypeBounds[i].extents = new Vector3 (1,2,1);
				Component.Destroy(mr);
				Component.Destroy(mf);
//				foreach (Light l in ls)
//					Light.Destroy(l);
				if ( ani != null )
					Component.Destroy(ani);
				if ( anim != null )
					Component.Destroy(anim);

				Collider col = go.GetComponent<Collider>();
				if ( col != null || go.GetComponentsInChildren<Light>(true).Length >= 1 )
				{
					BoxCollider bc = go.GetComponent<BoxCollider>();
					if ( bc != null )
						GlobalPrototypeTPHInfo[i] = new LTreePlaceHolderInfo (bc.center, bc.size.y * 0.5f, bc.size.x * 0.5f + bc.size.z * 0.5f);
					else
						GlobalPrototypeTPHInfo[i] = null;
#if !UNITY_5
					Collider[] cs = go.GetComponents<Collider>();
					foreach ( Collider c in cs )
					{
						c.isTrigger = true;
					}
#endif
					go.name = "Prototype [" + i.ToString() + "]'s Collider";
					go.SetActive(false);
					GlobalPrototypeColliders[i] = go;
					GlobalPrototypeCollidersType[i] = bc != null ? ColType.BoxCollider : (col != null ? ColType.Collider : ColType.None);
				}
				else
				{
					Destroy(go);
					GlobalPrototypeColliders[i] = null;
					GlobalPrototypeCollidersType[i] = ColType.None;
				}
			}
			{
				GameObject go = Instantiate( GlobalPrototypePrefabList[i] ) as GameObject;
				go.transform.parent = PrototypeGroup.transform;
				go.transform.position = Vector3.zero;
				go.transform.rotation = Quaternion.identity;
				MeshRenderer mr = go.GetComponent<MeshRenderer>();
				MeshFilter mf = go.GetComponent<MeshFilter>();
				Collider[] cs = go.GetComponentsInChildren<Collider>();
				Animator ani = go.GetComponent<Animator>();
				Animation anim = go.GetComponent<Animation>();
				GlobalPrototypeBounds[i] = mf.mesh.bounds;
				if ( i == TreePlaceHolderPrototypeIndex )
					GlobalPrototypeBounds[i].extents = new Vector3 (1,2,1);
				Component.Destroy(mr);
				Component.Destroy(mf);
				foreach (Collider c in cs)
					Collider.Destroy(c);
				if ( ani != null )
					Component.Destroy(ani);
				if ( anim != null )
					Component.Destroy(anim);
				if ( go.GetComponentsInChildren<Light>(true).Length >= 1 )
				{
					go.name = "Prototype [" + i.ToString() + "]'s Light";
					go.SetActive(false);
					GlobalPrototypeLights[i] = go;
				}
				else
				{
					Destroy(go);
					GlobalPrototypeLights[i] = null;
				}
			}
		}

		// Create i/o
		m_IO = LSubTerrIO.CreateInst ();
	}
	void Start ()
	{
        Init();

		m_Nodes = new Dictionary<int, LSubTerrain> ();
		m_map32Trees = new Dictionary<int, List<TreeInfo>> ();
		m_mapExistTempTrees = new Dictionary<int, List<GameObject>> ();
		m_mapTempTreeInfos = new Dictionary<GameObject, GlobalTreeInfo> ();
		TempTreesGroup.AddComponent<LSubTerrTempTrees>().m_TempMap = m_mapTempTreeInfos;
		m_BeginTime = Time.time;

		LayerCreators = new LSubTerrCreator [Layers.Count];
		for ( int l = Layers.Count - 1; l >= 0; --l )
		{
			GameObject layer_go = new GameObject ("[" + l.ToString() + "] " + Layers[l].Name + " Layer");
			layer_go.transform.parent = LayerGroup.transform;
			layer_go.transform.position = Vector3.zero;
			layer_go.transform.rotation = Quaternion.identity;
			LSubTerrCreator creator = layer_go.AddComponent<LSubTerrCreator>();
			creator.LayerIndex = l;
			LayerCreators[l] = creator;
		}
	}
	void Update ()
	{
		// Determine node area
		LSubTerrUtils.WorldPosToPos (s_Instance.CameraTransform.position, _iTmpCamPos);
		if (!m_IO.TryFill_T (_iTmpCamPos, m_Nodes))
			return;

		// Apply CamPos
		_iCurCamPos.x = _iTmpCamPos.x;
		_iCurCamPos.y = _iTmpCamPos.y;
		_iCurCamPos.z = _iTmpCamPos.z;

		// Determine near tree area
		if (PlayerTransform == null) {
			PlayerTransform = CameraTransform;
		} else if (IsAllNodeFinishedProcess ()) {
			RefreshTreeGos ();
		}
#if false
		if ( Application.isEditor )			DrawOutline();
#endif		
		// Delete the LSubTerrains out of cache
		DeleteNodesOutfield (_iCurCamPos);
		if (_bDataDirty) {			
			_bDataDirty = false;
			LSubTerrainMgr.RefreshAllLayerTerrains ();
		}
	}
	void LateUpdate ()
	{
		m_LifeTime = Time.time - m_BeginTime;
		m_LifeFrame++;
	}
	void OnDestroy ()
	{
		if ( m_Nodes != null ){
			foreach ( KeyValuePair<int, LSubTerrain> kvp in m_Nodes ){
				kvp.Value.Release();
			}
			m_Nodes.Clear();
		}
		if (m_map32Trees != null) 			m_map32Trees.Clear ();
		if ( m_mapExistTempTrees != null )	m_mapExistTempTrees.Clear();
		if ( m_mapTempTreeInfos != null )	m_mapTempTreeInfos.Clear();

		LSubTerrIO.DestroyInst (m_IO);
		s_Instance = null;
	}
	#endregion
	
	private void RefreshTreeGos()
	{
		int x32 = Mathf.FloorToInt(PlayerTransform.position.x / 32);
		int z32 = Mathf.FloorToInt(PlayerTransform.position.z / 32);
		List<TreeInfo> tmpTis;
		for ( int x = x32 - 2; x <= x32 + 2; ++x )
		{
			for ( int z = z32 - 2; z <= z32 + 2; ++z )
			{
				int idx = LSubTerrUtils.Tree32PosTo32Key(x,z);
				if ( !m_mapExistTempTrees.ContainsKey(idx) && m_map32Trees.TryGetValue(idx, out tmpTis) )
				{
					List<GameObject> tmptreelist = new List<GameObject> ();
					int nTis = tmpTis.Count;
					for(int i = 0; i < nTis; i++){
						TreeInfo ti = tmpTis[i];
						if ( GlobalPrototypeColliders[ti.m_protoTypeIdx] == null )
							continue;
						
						GameObject temptree_go = GameObject.Instantiate( GlobalPrototypeColliders[ti.m_protoTypeIdx], 
						                                                LSubTerrUtils.TreeTerrainPosToWorldPos(x/8,z/8,ti.m_pos), Quaternion.identity ) as GameObject;
						temptree_go.transform.parent = TempTreesGroup.transform;
						temptree_go.transform.localScale = new Vector3 (ti.m_widthScale, ti.m_heightScale, ti.m_widthScale);
						temptree_go.name = temptree_go.transform.position.ToString() + " Type " + ti.m_protoTypeIdx;
						temptree_go.layer = NearTreeLayer;
						temptree_go.SetActive(true);

                        if (OnTreeColliderCreated != null)
                            OnTreeColliderCreated(temptree_go);

                        tmptreelist.Add(temptree_go);
						m_mapTempTreeInfos.Add(temptree_go, new GlobalTreeInfo(x/8,z/8,ti));
					}
					m_mapExistTempTrees.Add(idx, tmptreelist);
				}
			}
		}
		List<int> keys_to_del = new List<int> ();
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_mapExistTempTrees )
		{
			IntVector3 pos32 = LSubTerrUtils.Tree32KeyTo32Pos(kvp.Key);
			if ( Mathf.Abs(pos32.x - x32) > 2 || Mathf.Abs(pos32.z - z32) > 2 )
			{
				keys_to_del.Add(kvp.Key);
				foreach ( GameObject go in kvp.Value )
				{
                    if (OnTreeColliderDestroy != null)
                        OnTreeColliderDestroy(go);
                    m_mapTempTreeInfos.Remove(go);
					GameObject.Destroy(go);
				}
			}
		}
		foreach ( int k in keys_to_del )
		{
			m_mapExistTempTrees.Remove(k);
		}
	}	
	// Delete a layered-subterrain node
	private void DeleteNode( int index )
	{		
		_bDataDirty = true;
		LSubTerrain node;
		if ( m_Nodes.TryGetValue(index, out node) )
		{
			m_Nodes.Remove(index);
			node.Release();
			//GameObject.Destroy(node.gameObject);
		}
		else
		{
			Debug.LogError("Deleting an LSubTerrain node but it doesn't exist in the map !");
		}
	}
	private void DeleteNodesOutfield(IntVector3 iCamPos)
	{
		int ecs = m_LifeTime > 90 ? ExtraCacheSize : 0;
		int delete_cnt = m_Nodes.Count - ((NumDataExpands*2+1) * (NumDataExpands*2+1) + ecs);
		for ( int i = 0; i < delete_cnt; i++ )
		{
			float max_dist = 0;
			int max_index = -1;
			foreach ( KeyValuePair<int, LSubTerrain> kvp in m_Nodes )
			{
				IntVector3 node_pos = kvp.Value.iPos;
				int dist = Mathf.Max(Mathf.Abs(node_pos.x - iCamPos.x), Mathf.Abs(node_pos.z - iCamPos.z));
				if ( dist >= max_dist )
				{
					max_dist = dist;
					max_index = kvp.Key;
				}
			}
			DeleteNode(max_index);
		}
	}
	private void DrawOutline()
	{
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_mapExistTempTrees )
		{
			IntVector3 pos = LSubTerrUtils.Tree32KeyTo32Pos(kvp.Key);
			int x = pos.x;
			int z = pos.z;
			Debug.DrawLine(new Vector3(x*32, PlayerTransform.position.y, z*32), new Vector3(x*32+32, PlayerTransform.position.y, z*32), Color.yellow);
			Debug.DrawLine(new Vector3(x*32+32, PlayerTransform.position.y, z*32), new Vector3(x*32+32, PlayerTransform.position.y, z*32+32), Color.yellow);
			Debug.DrawLine(new Vector3(x*32+32, PlayerTransform.position.y, z*32+32), new Vector3(x*32, PlayerTransform.position.y, z*32+32), Color.yellow);
			Debug.DrawLine(new Vector3(x*32, PlayerTransform.position.y, z*32+32), new Vector3(x*32, PlayerTransform.position.y, z*32), Color.yellow);
		}
	}

	public static GlobalTreeInfo GetTreeinfo(Collider col)
	{
		if(null == col)
			return null;
		
		if ( s_Instance.m_mapTempTreeInfos.ContainsKey(col.gameObject) )
		{
			return s_Instance.m_mapTempTreeInfos[col.gameObject];
		}

		return null;
	}
	
	public static GlobalTreeInfo RayCast(Ray ray, float distance, out RaycastHit hitinfo)
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
	public static GlobalTreeInfo RayCast(Ray ray, float distance)
	{
		RaycastHit rch;
		return RayCast(ray, distance, out rch);
	}
	
	public static List<GlobalTreeInfo>  Picking(Vector3 position, Vector3 direction, bool includeTrees = false, float distance = 1.5f, float angle = 45f)
	{
		if ( s_Instance == null )
			return picklist;
		ClearTreeinfo();

		distance = Mathf.Clamp(distance, 0.1f, 2f);
		int ix = Mathf.FloorToInt(position.x);
		int iy = Mathf.FloorToInt(position.y);
		int iz = Mathf.FloorToInt(position.z);
		TreeInfo tmpTi;
		for ( int x = ix - 2; x <= ix + 2; ++x )
		{
			for ( int y = iy - 2; y <= iy + 2; ++y )
			{
				for ( int z = iz - 2; z <= iz + 2; ++z )
				{
					int idx = LSubTerrUtils.WorldPosToIndex(new Vector3(x+.5F,0,z+.5F));
					if ( Node(idx) != null )
					{
						temp.x = x % LSubTerrConstant.Size;
						temp.y = y;
						temp.z = z % LSubTerrConstant.Size;
						tmpTi = Node(idx).GetTreeInfoListAtPos(temp);
						while (tmpTi != null){
							TreeInfo ti = tmpTi;
							tmpTi = tmpTi.Next;
							if ( HasCollider(ti.m_protoTypeIdx) && !includeTrees )
								continue;
							Vector3 treepos = LSubTerrUtils.TreeTerrainPosToWorldPos(x>>LSubTerrConstant.SizeShift, z>>LSubTerrConstant.SizeShift, ti.m_pos);
							Vector3 diff = treepos - position;
							diff.y = 0;
							if ( diff.magnitude > distance )
								continue;
							direction.y = 0;
							if ( Vector3.Angle(diff, direction) < angle )
							{
								GlobalTreeInfo gti = GetGlobalTreeInfo();
								gti._terrainIndex = idx;
								gti._treeInfo = ti;
								picklist.Add(gti);
							}
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

		int x = position.x;
		int y = position.y;
		int z = position.z;
		int idx = LSubTerrUtils.WorldPosToIndex(new Vector3(x+.5F,0,z+.5F));
		TreeInfo tmpTi;
		if ( Node(idx) != null )
		{
			int rx = x % LSubTerrConstant.Size;
			int ry = y;
			int rz = z % LSubTerrConstant.Size;
			tmpTi = Node(idx).GetTreeInfoListAtPos(new IntVector3(rx,ry,rz));
			while (tmpTi != null){
				TreeInfo ti = tmpTi;
				tmpTi = tmpTi.Next;
				if ( HasCollider(ti.m_protoTypeIdx) && !includeTrees )
					continue;
				
				GlobalTreeInfo gti = GetGlobalTreeInfo();
				gti._terrainIndex = idx;
				gti._treeInfo = ti;
				picklist.Add(gti);
			}
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
	
	public static void AddTree( Vector3 wpos, int prototype, float width_scale = 1, float height_scale = 1 )
	{
		if ( s_Instance == null)
			return;

		int terIdx = LSubTerrUtils.WorldPosToIndex (wpos);
		IntVector3 terrpos = LSubTerrUtils.IndexToPos(terIdx);
		IntVector3 campos = _iCurCamPos;
		if ( Mathf.Abs(terrpos.x - campos.x) > 1 )
			return;
		if ( Mathf.Abs(terrpos.z - campos.z) > 1 )
			return;
		
		if ( s_Instance.m_Nodes.ContainsKey(terIdx) )
		{
			LSubTerrain TargetNode = Node(terIdx);
			if ( TargetNode == null )
				return;
			
			// Read TreeInfo
			TreeInfo ti = TargetNode.AddTreeInfo(wpos, prototype, width_scale, height_scale);	
			if(ti != null){
				float height = s_Instance.GlobalPrototypeBounds[ti.m_protoTypeIdx].extents.y * 2F;
				for ( int l = s_Instance.Layers.Count - 1; l >= 0; --l )
				{
					// Trees in this layer
					if ( s_Instance.Layers[l].MinTreeHeight <= height && height < s_Instance.Layers[l].MaxTreeHeight )
					{
						s_Instance.LayerCreators[l].m_allTreesInLayer[terIdx].Add(ti);
						break;
					}
				}
			}
		}
	}

	public static void DeleteTree(GameObject nearTreeGo)
	{
		if ( s_Instance == null )
			return;
		GlobalTreeInfo treeInfo = null;
		if(s_Instance.m_mapTempTreeInfos.TryGetValue(nearTreeGo, out treeInfo)){
			DeleteTree(treeInfo);
		}
	}
	public static void DeleteTree(GlobalTreeInfo treeinfo)
	{
		if ( s_Instance == null || treeinfo == null )
			return;
		
		// Delete it in Mgr's m_map32Trees
		int tmpKey = LSubTerrUtils.TreeWorldPosTo32Key(treeinfo.WorldPos);
		List<TreeInfo> tmpTis;
		if (s_Instance.m_map32Trees.TryGetValue (tmpKey, out tmpTis)) {
			tmpTis.Remove(treeinfo._treeInfo);
			if ( tmpTis.Count == 0 ){
				s_Instance.m_map32Trees.Remove(tmpKey);
			}
		}

		// Delete it in Mgr's m_mapExistTempTrees and m_mapTempTreeInfos
		List<GameObject> existGos;
		if ( s_Instance.m_mapExistTempTrees.TryGetValue(tmpKey, out existGos) )
		{
			GameObject gameobject_to_delete = null;
			GlobalTreeInfo gti;
			foreach ( GameObject go in existGos )
			{
				if ( s_Instance.m_mapTempTreeInfos.TryGetValue(go, out gti) )
				{
					if ( gti._treeInfo == treeinfo._treeInfo )
					{
						// Found it!
						gameobject_to_delete = go;

                        if (OnTreeColliderDestroy != null)
                            OnTreeColliderDestroy(gameobject_to_delete);

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
				existGos.Remove(gameobject_to_delete);
		}
		
		// Delete it in Node's m_mapTrees and m_listTrees
		TreeInfo secondFoot = null;	// For two feet trees
		if ( Node(treeinfo._terrainIndex) != null )
		{
			secondFoot = Node(treeinfo._terrainIndex).DeleteTreeInfo(treeinfo._treeInfo);
		}
		else
		{
			Debug.LogError("Can not find the subterrain node when delete tree");
		}
		
		// Delete it in layers
		foreach ( LSubTerrCreator creator in s_Instance.LayerCreators )
		{
			if ( creator.m_allTreesInLayer.TryGetValue(treeinfo._terrainIndex, out tmpTis) )
			{
				tmpTis.Remove(treeinfo._treeInfo);
			}
			else
			{
				Debug.LogError("Can not find the key in layer's m_allTreesInLayer when delete tree");
			}
		}

        LSubTerrSL.AddDeletedTree( treeinfo._terrainIndex, treeinfo._treeInfo );
		
		// Delete 2nd foot ?? Is this really necessary??
		if ( secondFoot != null )
		{
			GlobalTreeInfo gti_2ndfoot = new GlobalTreeInfo (treeinfo._terrainIndex, secondFoot);
			DeleteTree(gti_2ndfoot);
		}
	}
	public static bool IsAllNodeFinishedProcess()
	{
		if ( s_Instance == null )
			return true;
		foreach ( KeyValuePair<int, LSubTerrain> kvp in s_Instance.m_Nodes )
		{
			if (!kvp.Value.FinishedProcess && kvp.Value.HasData)
				return false;
		}
		return true;
	}
	public static void DeleteTreesAtPos(IntVector3 position, int filter_min = 0, int filter_max = 65536 )
	{
		List<GlobalTreeInfo> del_list = Picking(position, true);
		foreach ( GlobalTreeInfo gti in del_list )
		{
			if ( gti._treeInfo.m_protoTypeIdx >= filter_min && gti._treeInfo.m_protoTypeIdx <= filter_max )
				DeleteTree(gti);
		}
	}
    public static GlobalTreeInfo TryGetTreeInfo(GameObject tree) 
    {
        if (s_Instance == null)
            return null;
        GlobalTreeInfo treeInfo = null;
        if (s_Instance.m_mapTempTreeInfos.TryGetValue(tree, out treeInfo))
            return treeInfo;
        return null;
    }
	
	public static void RefreshAllLayerTerrains()
	{
		if ( s_Instance == null )
			return;
		s_Instance.StartCoroutine("RefreshAllLayerTerrains_Coroutine");
	}
	
	IEnumerator RefreshAllLayerTerrains_Coroutine()
	{
		foreach ( LSubTerrCreator creator in LayerCreators )
		{
			while ( creator.bProcessing || creator.bBillboardProcessing )
				yield return 0;
			creator.StartCoroutine("RefreshRegion");
		}		
	}
	
	// for editor
	public static void CacheAllNodes()
	{
		if ( s_Instance == null )
			return;
		foreach ( KeyValuePair<int, LSubTerrain> kvp in s_Instance.m_Nodes )
		{
			kvp.Value.SaveCache();
		}
	}
}
