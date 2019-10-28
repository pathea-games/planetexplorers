//#define BoundCheckEnable // Issue: Bounds should be set not only in Doodad's lodcmpt but also doodad's posagent
#define AlwaysCheckDepForActive
//#define SceneWithOnlyPlayer
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

// Object Lifetime/(In)Visible Managment
public class SceneMan : MonoBehaviour, Pathea.ISerializable
{
    const string ArchiveKey = "ArchiveKeySceneManager";
	const int ArchiveVer = 0;
	public const int InvalidID = 0;	// use 0 because the default value of int is zero
	public static SceneMan self = null;
	public static int MaxLod{ 	get{ 	return LODOctreeMan._maxLod; 	}
								set{	LODOctreeMan._maxLod = value; 	}}
	public static readonly float MaxHitTstHeight = 999;
	public static readonly LayerMask DependenceLayer = 1 << Pathea.Layer.VFVoxelTerrain | 1 << Pathea.Layer.SceneStatic | 1 << Pathea.Layer.Unwalkable;
	public static RaycastHit[] DependenceHitTst(ISceneObjAgent agent){
		float rayLen = agent.TstYOnActivate ? (agent.Pos.y+1) : SceneMan.MaxHitTstHeight;
		Vector3 pos = new Vector3(agent.Pos.x, rayLen, agent.Pos.z);
		RaycastHit[] ret = Physics.RaycastAll(pos, Vector3.down, rayLen, SceneMan.DependenceLayer);
		return (ret == null || ret.Length == 0) ? null : ret;
	}
	public static Vector3 LastRefreshPos{
		get{			return (self!=null&& self._lodMan!=null) ? self._lodMan.LastRefreshPos : LODOctreeMan.InitPos;		}
	}
	public Transform Observer{ 
		get{			return (_lodMan != null) ? _lodMan.Observer : null;		}
		set{			if(_lodMan != null)	_lodMan.Observer = value;			}
	}

	Pathea.IdGenerator _idGen = new Pathea.IdGenerator(1, 1, int.MaxValue);	//max value of type int
	LODOctreeMan _lodMan;
	List<ISceneObjActivationDependence> _lstActDependences = new List<ISceneObjActivationDependence>();
	List<ISceneObjAgent> _lstIsActive = new List<ISceneObjAgent>();
	List<ISceneObjAgent> _lstToActive = new List<ISceneObjAgent>();	
	List<ISceneObjAgent> _lstInactive = new List<ISceneObjAgent>();	
	List<ISceneObjAgent> _lstIdle = new List<ISceneObjAgent>();	
	// Tmp list to save those objs to exec func and these execution might change the above list.
	List<ISceneObjAgent> _toConstruct = new List<ISceneObjAgent>();
	List<ISceneObjAgent> _toDestruct = new List<ISceneObjAgent>();
	List<ISceneObjAgent> _toActivate = new List<ISceneObjAgent>();
	List<ISceneObjAgent> _toDeactivate = new List<ISceneObjAgent>();

	Rect _curRectActive;
	Rect _curRectInactive;
	Rect _curRectInactiveE;		// extension for construct delay
	Bounds _curBoundsActive;	
	Bounds _curBoundsInactive;
	Bounds _curBoundsInactiveE;	// extension for construct delay
	Vector3 _vExtForDelay = new Vector3 (48, 48, 48);

	bool _bActivateFunctional = false;
	bool _bMoved = false;
	bool _dirtyLstIsActive;
	//bool _dirtyLstToActive;
	bool _dirtyLstInactive;
	bool _dirtyLstIdle;
	Vector3 _lastRefreshPos = LODOctreeMan.InitPos;
	int _sqrObjRefreshThreshold = 1;

	byte[] _dataToImport = null;
	List<long> _lstPosToSerialize = null;
	List<string> _lstTypeToSerialize = null;

	Transform _playerTrans = null;

	public bool CenterMoved{ 		get { return _bMoved; } }
	public bool ActivationStarted{ 	get { return _bActivateFunctional; } }
	public void StartWork()
	{
		if (!_bActivateFunctional) 
		{
			AddImportedObj ();
			_bActivateFunctional = true;
		}
	}

	// Monobehavior Event Methods
	void Awake()
	{
		self = this;
		Profiler.maxNumberOfSamplesPerFrame = -1;
		Pathea.ArchiveMgr.Instance.Register(ArchiveKey, this, true);
	}

	void Start()
	{
		List<ILODNodeDataMan> lodNodeDataMans = new List<ILODNodeDataMan> ();

		if (VFVoxelTerrain.self != null) {
			lodNodeDataMans.Add(VFVoxelTerrain.self);
			lodNodeDataMans.Add(VFVoxelWater.self);
		}
		if (Block45Man.self != null) {
			lodNodeDataMans.Add(Block45Man.self);
		}
		_lstActDependences.Add(SceneChunkDependence.Instance);
		_lstActDependences.Add(new SceneStaticObjDependence(_lstInactive));

		LODOctreeMan.ResetRootChunkCount();//(6+2*terLv,6,6+2*terLv);
#if UNITY_EDITOR
		LoadLodDesc();
#endif  
		int lodRefreshThreshold = 1;
		if(MaxLod > 3){
			Debug.Log("[LOD]Too many levels");
			MaxLod = 3;
		}
		//MaxLod = 3;
		//VFVoxelTerrain.MapDataPath_Zip = GameConfig.PEDataPath + GameConfig.MapDataDir_Zip + "/";		
		_lodMan = new LODOctreeMan(lodNodeDataMans.ToArray(), MaxLod, lodRefreshThreshold, PETools.PEUtil.MainCamTransform);
	}

	void Update ()
	{
		if (_playerTrans == null) {
			if(Pathea.PeCreature.Instance.mainPlayer != null && Pathea.PeCreature.Instance.mainPlayer.peTrans != null)
			{
				_playerTrans = Pathea.PeCreature.Instance.mainPlayer.peTrans.trans;
			}
		}

		if (PeCamera.isFreeLook || _playerTrans == null || PlotLensAnimation.IsPlaying) {
			SetObserverTransform(PETools.PEUtil.MainCamTransform);
		}
#if UNITY_EDITOR
		else if(FreeCamera.FreeCameraMode){
			SetObserverTransform(PETools.PEUtil.MainCamTransform);
		}
#endif
		else {
			SetObserverTransform(_playerTrans);
		}

		bool bret = (!_lodMan.ReqRefresh ());
		if(bret)			return;

		VFGoPool<VFVoxelChunkGo>.SwapReq();
		VFGoPool<Block45ChunkGo>.SwapReq();

		Vector3 vlodPos = _lodMan.LastRefreshPos;
		Vector3 vDist = vlodPos - _lastRefreshPos; 
		_bMoved = vDist.sqrMagnitude > _sqrObjRefreshThreshold;
		if(_bMoved)
		{
			//Code to update Bounds
			_curBoundsInactive = LODOctreeMan.self._viewBounds;
			if(LODOctreeMan._maxLod == 0){
				_curBoundsInactive.extents *= 1.5f;
			}
			Vector3 min = _curBoundsInactive.min; 
			Vector3 siz = _curBoundsInactive.size;
			_curRectInactive = new Rect(min.x, min.z, siz.x, siz.z);
			_curRectInactiveE = new Rect(min.x-_vExtForDelay.x, min.z-_vExtForDelay.z, siz.x+_vExtForDelay.x, siz.z+_vExtForDelay.z);
			_curBoundsInactiveE = new Bounds(_curBoundsInactive.center, (_curBoundsInactive.extents+_vExtForDelay)*2);

			_curBoundsActive = LODOctreeMan.self._Lod0ViewBounds;
			min = _curBoundsActive.min; 
			siz = _curBoundsActive.size;
			_curRectActive = new Rect(min.x, min.z, siz.x, siz.z);
			_lastRefreshPos = vlodPos;
		}
		
		if(_bMoved || _dirtyLstIdle)			UpdateIdle();
		if(_bMoved || _dirtyLstInactive)		UpdateInactive();

		if (_bActivateFunctional) 
		{
			// Update all active agent because agent can move
#if AlwaysCheckDepForActive
			_dirtyLstIsActive = true;
#endif
			UpdateToActive ();
			UpdateIsActive ();
		}
		
		VFGoPool<VFVoxelChunkGo>.ExecFreeGo();
		VFGoPool<Block45ChunkGo>.ExecFreeGo();
	}
	void OnDestroy()
	{
		SceneChunkDependence.Instance.Reset ();
		if(_lodMan != null)
		{
			_lodMan.ReqDestroy();
			_lodMan = null;
		}
		self = null;
	}
	// Static Methods for external using
	//==================================
	public static void SetObserverTransform(Transform trans)
	{
		if(null != self)	self.Observer = trans;
	}
	public static void AddSceneObj(ISceneObjAgent obj)
	{
		if(null == self)	return;
#if SceneWithOnlyPlayer
		if (!(obj is Pathea.LodCmpt))
			return;
		if (((Pathea.LodCmpt)obj).Entity.commonCmpt == null)
			return;
		if (!((Pathea.LodCmpt)obj).Entity.commonCmpt.IsPlayer)
			return;
#endif

		if(obj.Id == InvalidID)
		{
			obj.Id = self._idGen.Fetch();
		}
		self._lstIdle.Add(obj);
		self._dirtyLstIdle = true;
	}
	public static void AddSceneObjs<T>(List<T> objs) where T : ISceneObjAgent
	{
		if(null == self)	return;
#if SceneWithOnlyPlayer
		return;
#endif

		int n = objs.Count;
		for (int i = 0; i < n; i++) {
			ISceneObjAgent obj = objs[i];
			if(obj.Id == InvalidID)
			{
				obj.Id = self._idGen.Fetch();
			}
			self._lstIdle.Add (obj);
		}
		self._dirtyLstIdle = true;
	}
	// Return : 0:not in sceneman; 1: in idle; 2: in inactive; 3: in toactive; 4: in active
	public static int RemoveSceneObj(ISceneObjAgent obj)
	{
		if(null == self)	return 0;

		if (self._lstIsActive.Remove (obj))		return 4;
		if (self._lstToActive.Remove (obj))		return 3;
		if (self._lstInactive.Remove (obj))		return 2;
		if (self._lstIdle.Remove (obj))			return 1;		
		return 0;
	}
	// Return : count of removed objs
	public static int RemoveSceneObjs<T>(List<T> objs) where T : ISceneObjAgent
	{
		if(null == self)	return 0;

		int ret = 0;
		int n = objs.Count;
		for (int i = 0; i < n; i++) {
			ISceneObjAgent obj = objs[i];
			if(	
				self._lstIsActive.Remove(obj) ||
				self._lstToActive.Remove(obj) ||
				self._lstInactive.Remove(obj) ||
				self._lstIdle.Remove(obj))
			{
				ret++;
			}
		}
		return ret;
	}
	public static bool RemoveSceneObjByGo(GameObject go)
	{
		if(null == self || go == null)	return false;

		return
			self._lstIsActive.RemoveAll(it=>it.Go == go) > 0 ||
			self._lstToActive.RemoveAll(it=>it.Go == go) > 0 ||
			self._lstInactive.RemoveAll(it=>it.Go == go) > 0 ||
			self._lstIdle.RemoveAll(it=>it.Go == go) > 0;
	}

    //public static bool RemoveSceneObjById(int id)
    //{
    //    if (null == self || InvalidID == id) return false;

    //    return
    //        self._lstIsActive.RemoveAll(it => it.Id == id) > 0 ||
    //        self._lstToActive.RemoveAll(it => it.Id == id) > 0 ||
    //        self._lstInactive.RemoveAll(it => it.Id == id) > 0 ||
    //        self._lstIdle.RemoveAll(it => it.Id == id) > 0;
    //}

    public static ISceneObjAgent GetSceneObjById(int id)
    {
        if (null == self || InvalidID == id) return null;

        ISceneObjAgent obj = null;
        return (
            (obj = self._lstIsActive.Find(it => it.Id == id)) != null ||
            (obj = self._lstToActive.Find(it => it.Id == id)) != null ||
            (obj = self._lstInactive.Find(it => it.Id == id)) != null ||
            (obj = self._lstIdle.Find(it => it.Id == id)) != null ) ? obj : null;
    }

	public static ISceneObjAgent GetSceneObjByGo(GameObject go)
	{
		if(null == self || go == null)	return null;
		
		ISceneObjAgent obj = null;
		return (
			(obj = self._lstIsActive.Find(it=>it.Go == go)) != null ||
			(obj = self._lstToActive.Find(it=>it.Go == go)) != null ||
			(obj = self._lstInactive.Find(it=>it.Go == go)) != null ||
			(obj = self._lstIdle.Find(it=>it.Go == go)) != null ) ? obj : null;
	}

    #region GET_SCENE_OBJS 
    // Add By WuYiqiu
    public static List<ISceneObjAgent> GetSceneObjs<T> () where T : ISceneObjAgent
    {
        if (self == null)
            return null;

        List<ISceneObjAgent> objs = new List<ISceneObjAgent>();
        List<ISceneObjAgent> list_temp = null;

        list_temp = self._lstIsActive.FindAll(item0 => (item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T))));
        objs.AddRange(list_temp);

        list_temp = self._lstToActive.FindAll(item0 => (item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T))));
        objs.AddRange(list_temp);

        list_temp = self._lstInactive.FindAll(item0 => (item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T))));
        objs.AddRange(list_temp);

        list_temp = self._lstIdle.FindAll(item0 => (item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T))));
        objs.AddRange(list_temp);

        return objs;
    }
    
    public static List<ISceneObjAgent> GetSceneObjs (System.Type type)
    {
        if (self == null && !type.IsSubclassOf(typeof(ISceneObjAgent)))
            return null;

        List<ISceneObjAgent> objs = new List<ISceneObjAgent>();
        List<ISceneObjAgent> list_temp = null;

        list_temp = self._lstIsActive.FindAll(item0 => item0.GetType() == type);
        objs.AddRange(list_temp);

        list_temp = self._lstToActive.FindAll(item0 => item0.GetType() == type);
        objs.AddRange(list_temp);

        list_temp = self._lstInactive.FindAll(item0 => item0.GetType() == type);
        objs.AddRange(list_temp);

        list_temp = self._lstIdle.FindAll(item0 => item0.GetType() == type);
        objs.AddRange(list_temp);

        return objs;
    }

	public static List<ISceneObjAgent> GetActiveSceneObjs (System.Type type, bool includChild = false)
	{
		if (self == null && !type.IsSubclassOf(typeof(ISceneObjAgent)))
			return null;

		List<ISceneObjAgent> objs = new List<ISceneObjAgent>();
		objs.AddRange (self._lstInactive.FindAll(item0 => includChild ? type.IsAssignableFrom(item0.GetType()) : (item0.GetType() == type)));
		objs.AddRange (self._lstIsActive.FindAll (item0 => includChild ? type.IsAssignableFrom(item0.GetType()) : (item0.GetType() == type)));
		return objs;
	}

    #endregion

    public static void OnActivationDependenceDirty(bool bAdd)
	{
		if(null == (System.Object)self)	return;	// for threading
		
		//if(bAdd)	self._dirtyLstToActive = true;
		else 		self._dirtyLstIsActive = true;
	}
	public static int SetDirty(ISceneObjAgent obj)
	{
		if(null == self)	return 0;

#if false //these 2 are always true
		if(!self._dirtyLstToActive && self._lstToActive.Contains(obj) && !self._curBoundsActive.Contains(obj.Pos))
		{
			self._dirtyLstToActive = true;
			return 3;
		}
		if(!self._dirtyLstIsActive && self._lstIsActive.Contains(obj))
		{
			self._dirtyLstIsActive = true;
			return 4;
		}
#endif
		//Profiler.BeginSample ("SetDirty");
		// should not in Inactive
		if(!self._dirtyLstInactive && (!self._curBoundsInactive.Contains(obj.Pos) || self._curBoundsActive.Contains(obj.Pos)) && self._lstInactive.Contains(obj))
		{
			self._dirtyLstInactive = true;
			//Profiler.EndSample();
			return 2;
		}
		// should not in Idle
		if(!self._dirtyLstIdle && self._curBoundsInactive.Contains(obj.Pos) && self._lstIdle.Contains(obj))
		{
			self._dirtyLstIdle = true;
			//Profiler.EndSample();
			return 1;
		}
		//Profiler.EndSample();
		return 5;
	}

	// pos check
	//===========
	bool IntoInactive(ISceneObjAgent obj)
	{
#if BoundCheckEnable
		if (obj.Bound != null)
			return obj.Bound.Intersection (ref _curBoundsInactive, obj.TstYOnActivate);
#endif
		if(obj.TstYOnActivate)
			return _curBoundsInactive.Contains(obj.Pos);		
		return _curRectInactive.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}
	bool ExitInactive(ISceneObjAgent obj)
	{
#if BoundCheckEnable
		if (obj.Bound != null)
			return !obj.Bound.Intersection (ref _curBoundsInactiveE, obj.TstYOnActivate);
#endif
		if(obj.TstYOnActivate)
			return !_curBoundsInactiveE.Contains(obj.Pos);		
		return !_curRectInactiveE.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}
	bool IntoActive(ISceneObjAgent obj)
	{
#if BoundCheckEnable
		if (obj.Bound != null)
			return obj.Bound.Intersection (ref _curBoundsActive, obj.TstYOnActivate);
#endif
		if(obj.TstYOnActivate)
			return _curBoundsActive.Contains(obj.Pos);		
		return _curRectActive.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}
	bool ExitActive(ISceneObjAgent obj)
	{
#if BoundCheckEnable
		if (obj.Bound != null)
			return !obj.Bound.Intersection (ref _curBoundsActive, obj.TstYOnActivate);
#endif
		if(obj.TstYOnActivate)
			return !_curBoundsActive.Contains(obj.Pos);		
		return !_curRectActive.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}
	// Update function set
	//=====================
	void UpdateIdle()
	{
		int n = _lstIdle.Count;
		//Profiler.BeginSample ("UpdateIdle:"+n);
		for(int i = n-1; i >= 0; i--)
		{
			ISceneObjAgent obj = _lstIdle[i];
			if(IntoInactive(obj))
			{
				_lstIdle.RemoveAt(i);
				_toConstruct.Add(obj);
				_lstInactive.Add(obj);
				_dirtyLstInactive = true;
			}
		}
		n = _toConstruct.Count;
		if(n > 0)
		{
			for(int i = n-1; i >= 0; i--)
			{
				_toConstruct[i].OnConstruct();
			}
			_toConstruct.Clear();
		}
		_dirtyLstIdle = false;
		//Profiler.EndSample ();
	}
	void UpdateInactive()
	{
		int n = _lstInactive.Count;
		//Profiler.BeginSample ("UpdateInactive:"+n);
		for(int i = n-1; i >= 0; i--)
		{
			ISceneObjAgent obj = _lstInactive[i];
			if(ExitInactive(obj))
			{
				_lstInactive.RemoveAt(i);
				_toDestruct.Add(obj);
				_lstIdle.Add(obj);
				_dirtyLstIdle = true;
			}
			else if(obj.NeedToActivate && IntoActive(obj))
			{
				_lstInactive.RemoveAt(i);
				_lstToActive.Add(obj);
				//_dirtyLstToActive = true;
			}
		}
		n = _toDestruct.Count;
		if(n > 0)
		{
			for(int i = n-1; i >= 0; i--)
			{
				_toDestruct[i].OnDestruct();
			}
			_toDestruct.Clear();
		}
		_dirtyLstInactive = false;
		//Profiler.EndSample ();
	}
	void UpdateToActive()
	{
		int n = _lstToActive.Count;
		//Profiler.BeginSample ("UpdateToActive:"+n);
		for(int i = n-1; i >= 0; i--)
		{
			ISceneObjAgent obj = _lstToActive[i];
			if(!IntoActive(obj))
			{
				_lstToActive.RemoveAt(i);
				_lstInactive.Add(obj);
				_dirtyLstInactive = true;
			}
			else 	// Test collider and Activate it
			{
				bool allDp = true;
				EDependChunkType type = 0;
				int nDp = _lstActDependences.Count;
				for(int iDp = 0; iDp < nDp; iDp++)
				{
					ISceneObjActivationDependence dp = _lstActDependences[iDp];
					if(!dp.IsDependableForAgent(obj, ref type))
					{
						allDp = false;
						break;
					}
				}
				if(allDp)
				{
					//if((type&EDependChunkType.ChunkColMask) == 0 || null != DependenceHitTst(obj))
					{
						_lstToActive.RemoveAt(i);
						_toActivate.Add(obj);
						_lstIsActive.Add(obj);
					}
				}
			}
		}
		n = _toActivate.Count;
		if(n > 0)
		{
			for(int i = n-1; i >= 0; i--)
			{
				_toActivate[i].OnActivate();
			}
			_toActivate.Clear();
		}
		//_dirtyLstToActive = false;
		//Profiler.EndSample ();
	}
	void UpdateIsActive()
	{
		int n = _lstIsActive.Count;
		//Profiler.BeginSample ("UpdateIsActive:"+n);
		for(int i = n-1; i >= 0; i--)
		{
			ISceneObjAgent obj = _lstIsActive[i];
			if(ExitActive(obj))
			{
				_lstIsActive.RemoveAt(i);
				_toDeactivate.Add(obj);
				_lstInactive.Add(obj);
				_dirtyLstInactive = true;
			}
			else if(_dirtyLstIsActive)
			{
				bool allDp = true;
				EDependChunkType type = 0;
				int nDp = _lstActDependences.Count;
				for(int iDp = 0; iDp < nDp; iDp++)
				{
					ISceneObjActivationDependence dp = _lstActDependences[iDp];
					if(!dp.IsDependableForAgent(obj, ref type))
					{
						allDp = false;
						break;
					}
				}
				if(!allDp)
				{
					_lstIsActive.RemoveAt(i);
					_toDeactivate.Add(obj);
					_lstInactive.Add(obj);
					_dirtyLstInactive = true;
				}
			}
		}
		n = _toDeactivate.Count;
		if(n > 0)
		{
			for(int i = n-1; i >= 0; i--)
			{
				_toDeactivate[i].OnDeactivate();
			}
			_toDeactivate.Clear();
		}
		_dirtyLstIsActive = false;
		//Profiler.EndSample ();
	}
	void LoadLodDesc()
	{
		try{
			TerrainLodDescPaser.LoadTerLodDesc("TerrainLodDesc");
		}
		catch{
			Debug.Log("TerrainLodDesc not found.");
		}
		if(TerrainLodDescPaser.terLodDesc != null)
		{
			LODOctreeMan.ResetRootChunkCount(TerrainLodDescPaser.terLodDesc.x, TerrainLodDescPaser.terLodDesc.y, TerrainLodDescPaser.terLodDesc.z);
			MaxLod = TerrainLodDescPaser.terLodDesc.lod;
		}	
	}

	// Pathea.ISerializable : Save Load
	//=================================
	byte[] Export()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(ms);
		bw.Write(ArchiveVer);

		_idGen.Serialize(bw);
		
		List<ISceneSerializableObjAgent> lstObjToSerialize = new List<ISceneSerializableObjAgent>();
		_lstIsActive.ForEach(it => {
			ISceneSerializableObjAgent itSerializable = it as ISceneSerializableObjAgent;
			if(itSerializable != null)		lstObjToSerialize.Add(itSerializable);
		});
		_lstToActive.ForEach(it => {
			ISceneSerializableObjAgent itSerializable = it as ISceneSerializableObjAgent;
			if(itSerializable != null)		lstObjToSerialize.Add(itSerializable);
		});
		_lstInactive.ForEach(it => {
			ISceneSerializableObjAgent itSerializable = it as ISceneSerializableObjAgent;
			if(itSerializable != null)		lstObjToSerialize.Add(itSerializable);
		});
		_lstIdle.ForEach(it => {
			ISceneSerializableObjAgent itSerializable = it as ISceneSerializableObjAgent;
			if(itSerializable != null)		lstObjToSerialize.Add(itSerializable);
		});

		int n = lstObjToSerialize.Count;
		bw.Write(n);
		for(int i = 0; i < n; i++)
		{
			ISceneSerializableObjAgent obj = lstObjToSerialize[i];
			bw.Write(obj.GetType().Name);
			bw.Write(0);	// placeholder
			try{
				long posBeg = bw.BaseStream.Position;
				obj.Serialize(bw);
				long posEnd = bw.BaseStream.Position;
				bw.BaseStream.Seek(posBeg-sizeof(int), SeekOrigin.Begin);
				bw.Write((int)(posEnd-posBeg));
				bw.BaseStream.Seek(posEnd, SeekOrigin.Begin);
			} catch{
				Debug.LogError("[SceneMan]Failed to export "+obj.GetType()+obj.Id);
			}
		}
		bw.Close();
		ms.Close();
		return ms.ToArray();
	}	
	void Import(byte[] buffer)
	{
		_dataToImport = buffer;
		if (buffer == null)			return;		
		if (buffer.Length == 0)		return;
		
		MemoryStream ms = new MemoryStream(buffer);
		BinaryReader br = new BinaryReader(ms);
		int ver = br.ReadInt32 ();
		if (ver != ArchiveVer)
		{
			throw new System.Exception("Wrong save data version for scene manager:"+ver+"|"+ArchiveVer);
		}

		_idGen.Deserialize(br);
		
		int count = br.ReadInt32();
		_lstPosToSerialize = new List<long> (count);
		_lstTypeToSerialize = new List<string> (count);
		for(int i=0; i<count; i++)
		{
			string typeName = br.ReadString();
			int len = br.ReadInt32();
			if(len > 0){
				_lstTypeToSerialize.Add(typeName);
				_lstPosToSerialize.Add(br.BaseStream.Position);
				br.BaseStream.Seek(len, SeekOrigin.Current);
			}
		}
	}
	public void AddImportedObj(string typeNameMask = null)
	{
		if (_dataToImport == null || _lstPosToSerialize == null || _lstTypeToSerialize == null)
			return;

		MemoryStream ms = new MemoryStream(_dataToImport);
		BinaryReader br = new BinaryReader(ms);
		int n = _lstTypeToSerialize.Count;
		List<ISceneSerializableObjAgent> lstObjToSerialize = new List<ISceneSerializableObjAgent>(n);
		for (int i = n-1; i >= 0; i--)
		{
			string typeName = _lstTypeToSerialize[i];
			long pos = _lstPosToSerialize[i];
			if(typeNameMask != null && !typeName.Equals(typeNameMask))
			{
				continue;
			}

			try
			{
				System.Type t = System.Type.GetType(typeName);	//If necessary, we can use a hashset to optimize this.
				ISceneSerializableObjAgent obj = System.Activator.CreateInstance(t) as ISceneSerializableObjAgent;
				br.BaseStream.Seek(pos, SeekOrigin.Begin);
				obj.Deserialize(br);
				lstObjToSerialize.Add(obj);
			}
			catch(Exception e)
			{
				throw new System.Exception("[SceneMan]Wrong save data format: DataType "+typeName+"\nDetail:"+e);
			}
			_lstTypeToSerialize.RemoveAt(i);
			_lstPosToSerialize.RemoveAt(i);
		}
		SceneMan.AddSceneObjs(lstObjToSerialize);
	}
	public void New()
    {    
    }
    public void Restore()
    {
		Import(Pathea.ArchiveMgr.Instance.GetData(ArchiveKey));
    }
    void Pathea.ISerializable.Serialize(Pathea.PeRecordWriter w)
    {
        w.Write(Export());
    }

	#region interface
	public static void RemoveTerrainDependence(){
		if(self!=null&&self._lstActDependences!=null)
			self._lstActDependences.Remove(SceneChunkDependence.Instance);
	}
	public static void AddTerrainDependence(){
		if(self!=null&&self._lstActDependences!=null&&(!self._lstActDependences.Contains(SceneChunkDependence.Instance)))
			self._lstActDependences.Add(SceneChunkDependence.Instance);
	}
	public static List<ISceneObjAgent> FindAllDragItemInDungeon(){
		List<ISceneObjAgent> result = new List<ISceneObjAgent> ();
		if(self!=null){
			result.AddRange(self._lstIsActive.FindAll(it => it is DragItemAgent && it.Pos.y<0));
			result.AddRange(self._lstToActive.FindAll(it => it is DragItemAgent && it.Pos.y<0));
			result.AddRange(self._lstInactive.FindAll(it => it is DragItemAgent && it.Pos.y<0));
			result.AddRange(self._lstIdle.FindAll(it => it is DragItemAgent && it.Pos.y<0));
		}
		return result;
	}
	#endregion
}
