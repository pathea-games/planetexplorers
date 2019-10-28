#define ScopedCollider

using UnityEngine;
//using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using TownData;

public class TerrainLodDescPaser
{
	[Serializable()]
	public class TerrainLodDesc
	{
		[XmlElement("MaxLodLv")]
		public int lod { get; set; }
		[XmlElement("Lod0SizeX")]
		public int x { get; set; }
		[XmlElement("Lod0SizeY")]
		public int y { get; set; }
		[XmlElement("Lod0SizeZ")]
		public int z { get; set; }
	}
	public static TerrainLodDesc terLodDesc;
	public static void LoadTerLodDesc(string xmlPath)
	{
		TextAsset xmlResource = Resources.Load(xmlPath, typeof(TextAsset)) as TextAsset;
		StringReader reader = new StringReader(xmlResource.text);
        if (null == reader)
            return;

		XmlSerializer serializer = new XmlSerializer(typeof(TerrainLodDesc));
		terLodDesc = (TerrainLodDesc)serializer.Deserialize(reader);
        reader.Close();
	}
}

public partial class VFVoxelTerrain : SkillSystem.SkEntity, IVxChunkHelperProc, ILODNodeDataMan
{
	public const string ArchiveKey = "ArchiveKeyVoxelTerrain";

	//Astar path update
	public  delegate void DelegateChunksColliderRebuild(Bounds bounds);
	//public  static event DelegateChunksColliderRebuild ChunksColliderRebuildHandler;

	public  static VFVoxelTerrain self;
	// this tells if the terrain has finished loading the data from the disk.
	public  static bool TerrainVoxelComplete = false;
	public  static bool TerrainColliderComplete = false;
	public  static bool bChunkColliderRebuilding = true;

    public static string MapDataPath_Zip = null;
	//public VFStdVoxelDataSource Voxels{	get{	return _voxels;	}}
	//public VFStdVoxelDataSource voxelDataSource;
	public  bool saveTerrain;
	private int lastSaveChunkTime = 0;

	public VFVoxelSave SaveLoad;

	public  Material _defMat;
	private IVxDataSource _voxels;
	private IVxDataLoader _dataLoader;
	private LODDataUpdate _lodDataUpdate = null;
	private TransvoxelGoCreator _transGoCreator;

	public  IVxDataSource Voxels{		get{	return _voxels;	}	}
	public  IVxDataLoader DataLoader{	get{	return _dataLoader;	}	}
	public  LODDataUpdate LodDataUpdate{get{	return _lodDataUpdate;	}	}
	public  TransvoxelGoCreator TransGoCreator{	get{	return _transGoCreator;	}	}
	public  Bounds ViewBounds{		get{	return LodMan._viewBounds;}	}
		    
	public  bool IsInGenerating{	get{	return SurfExtractor.IsIdle;	}	}

#if UNITY_EDITOR
	public  bool InfiniteTerrainRefresh = false;
	public  bool InfiniteTerrainRepaint = false;
#endif	

	void Awake()
	{
		self = this;
		if(GameConfig.IsMultiMode &&  VoxelTerrainNetwork.Instance != null)
        {
            ImportNet();
            VoxelTerrainNetwork.Instance.Init();
        }
			
		int layer = Pathea.Layer.VFVoxelTerrain;
		VFVoxelChunkGo.DefMat = VFTransVoxelGo._defMat = _defMat;
		VFVoxelChunkGo.DefLayer = VFTransVoxelGo._defLayer = layer;
		VFVoxelChunkGo.DefParent = VFTransVoxelGo._defParent = transform;
		_transGoCreator = new TransvoxelGoCreator();
	}

    public static bool RandomMap
    {
        get;
        set;
    }
    public void Import(Pathea.PeRecordReader r)
    {
        if (Pathea.PeGameMgr.IsMulti)
            return;
		SaveLoad = new VFVoxelSave(ArchiveKey);
        SaveLoad.Import(r);				
		InitSkEntity();
	}

    public void ImportNet()
    {
        if (Pathea.PeGameMgr.IsSingle)
            return;
        SaveLoad = new VFVoxelSave(ArchiveKey);
        SaveLoad.Import(null);
        InitSkEntity();
    }

	void Start()
	{
		_transGoCreator.IsTransvoxelEnabled = LODOctreeMan._maxLod > 0;
		if(_transGoCreator.IsTransvoxelEnabled)
		{
			VFGoPool<VFTransVoxelGo>.PreAlloc(20<<LODOctreeMan._maxLod);	//128/36
		}
#if Win32Ver
		VFGoPool<VFVoxelChunkGo>.PreAlloc(256);
#else
		VFGoPool<VFVoxelChunkGo>.PreAlloc(4000);	// 3605(2386)/2039
#endif

		if(_lodDataUpdate != null)
		{
			_lodDataUpdate.init();
		}		
		PrepareColliderOrder();
		StartResetLOD();
	}
	void OnDestroy()
	{
		if(_lodDataUpdate != null)
		{
			_lodDataUpdate.Stop();
			_lodDataUpdate = null;
		}
		if(_transGoCreator != null)
		{
			_transGoCreator.IsTransvoxelEnabled = false;
			_transGoCreator = null;
		}
		if(SurfExtractor != null)
		{
			SurfExtractor.Reset();
		}
		if(_dataLoader != null){
			_dataLoader.Close();
		}

		// If something is left over... (in the editor)
		if (gameObject.transform.childCount > 0)
		{
			// Clean up chunks
			for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
			{
				var child = gameObject.transform.GetChild(i).gameObject;
				DestroyImmediate(child);
			}
		}
		Resources.UnloadUnusedAssets();
		TerrainVoxelComplete = false;
		TerrainColliderComplete = false;
		bChunkColliderRebuilding = true;
		self = null;
		VFVoxelSave.Clean();
	}
#if false	// Debug lod
	void OnDrawGizmos()
	{
		if(LodMan != null)
		{
			for(int i = 0; i <= VoxelTerrainConstants._maxLod; i++)
				Gizmos.DrawWireCube(LodMan._viewBoundsLod[i].center, LodMan._viewBoundsLod[i].size);
		}
	}
#endif
#if UNITY_EDITOR
	public void CountVData()
	{
		if(LodMan == null)
		{
			Debug.Log("No Lod Manager found.");
			return;
		}
		for(int lod = 0; lod <= LODOctreeMan._maxLod; lod++)
		{
			int nNull = 0;
			int nHollow = 0;
			int nNoVerts = 0;
			int nChunkGos = 0;
			
			int chunkNumX = LODOctreeMan._xChunkCount >> lod;
			int chunkNumY = LODOctreeMan._yChunkCount >> lod;
			int chunkNumZ = LODOctreeMan._zChunkCount >> lod;
			for(int cx = 0; cx < chunkNumX; cx++)
			{
				for(int cy = 0; cy < chunkNumY; cy++)
				{
					for(int cz = 0; cz < chunkNumZ; cz++)
					{
						VFVoxelChunkData cdata = (VFVoxelChunkData)LodMan.LodTreeNodes[lod][cx, cy, cz]._data[IdxInLODNodeData];
						if(cdata.ChunkGo != null)
						{
							nChunkGos++;
						}
						else if(cdata.DataVT.Length > VFVoxel.c_VTSize)
						{
							nNoVerts++;
							if(lod > 0)
							{
								Debug.Log("NoVerts"+cdata.ChunkPosLod);
							}
						}
						else if(cdata.DataVT.Length == VFVoxel.c_VTSize)
						{
							nHollow++;
						}
						else
						{
							nNull++;
						}
					}
				}
			}
			Debug.Log("VDATA[lod|chnkGos|noVert|nHollow|nNull]:"+lod+"|"+nChunkGos+"|"+nNoVerts+"|"+nHollow+"|"+nNull);
		}
	}
#endif
	public void StartResetLOD(Transform t = null)
	{
		StartCoroutine(ResetLOD());
	}
	IEnumerator ResetLOD(Transform t = null)
	{
		while(LodMan == null)	yield return 0;

		Transform old = LodMan.Observer;
		LodMan.Observer = null;
		TerrainVoxelComplete = false;
		TerrainColliderComplete = false;
		bChunkColliderRebuilding = true;
		LodMan.Reset();
		while(LodMan.IsFirstRefreshed)	yield return 0;

		LodMan.Observer = t != null ? t : old;
		yield return StartCoroutine(CheckTerrainInitStatus());
	}
	IEnumerator CheckTerrainInitStatus()
	{
		bool bFin = false;
		while(!bFin)
		{
			if(!TerrainVoxelComplete&&
			   LodMan.IsFirstRefreshed && DataLoader.IsIdle && SurfExtractor.IsAllClear)
			{
				TerrainVoxelComplete = true;
				yield return 0;
			}
			if(!TerrainColliderComplete && TerrainVoxelComplete && !bChunkColliderRebuilding)
			{
				TerrainColliderComplete = true;
				yield return 0;
			}
			yield return 0;
		}
	}
#region IVxChunkHelperProc_ILODNodeDataMan
	//private List<SurfExtractReqMC> _lstReqsToFin = new List<SurfExtractReqMC>();
	private int _idxInLODNodeData = 0;
	public int IdxInLODNodeData{	get{return _idxInLODNodeData;} set{_idxInLODNodeData = value;}	}
	public LODOctreeMan LodMan{	get;set;}
	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		VFVoxelChunkData data = new VFVoxelChunkData(node);
		data.HelperProc = this;
		return data;
	}
	public void ProcPostLodInit()
	{
		if(RandomMap)
		{
			_dataLoader = new VFDataRTGen(RandomMapConfig.RandSeed);
			_voxels = new VFLODDataSource(LodMan.LodTreeNodes, IdxInLODNodeData);
			//_lodDataUpdate = null;
		}
		else
		{
            _dataLoader = new VFDataReader(string.IsNullOrEmpty(VFVoxelTerrain.MapDataPath_Zip) ? string.Empty : (VFVoxelTerrain.MapDataPath_Zip + "/map"));
			_voxels = new VFLODDataSource(LodMan.LodTreeNodes, IdxInLODNodeData);
			_lodDataUpdate = new LODDataUpdate();
		}
	}
	public void ProcPostLodUpdate ()
	{
		if (!GameConfig.IsMultiMode && Environment.TickCount - lastSaveChunkTime > 15000 && saveTerrain == true)
		{
			StartCoroutine(VFVoxelSave.CoSaveAllChunksInList());
			lastSaveChunkTime = Environment.TickCount;
		}
		
		_transGoCreator.UpdateTransMesh();
	}
	public void ProcPostLodRefresh()
	{
		VFVoxelChunkData.EndAllReqs();
		_dataLoader.ProcessReqs();
	}

	public IVxSurfExtractor SurfExtractor{ get{	return SurfExtractorsMan.VxSurfExtractor;	} }
	public int  ChunkSig{ get{ return 0;	}	}
	public void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool)
	{
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		if((!GameConfig.IsMultiMode) && data == VFVoxelChunkData.S_ChunkDataNull && SaveLoad.ChunkSaveList.Contains(cdata))
		{
			SaveLoad.SaveChunksInListToTmpFile();
		}
	}
	public void ChunkProcPreLoadData(ILODNodeData ndata)
	{
		VFVoxelChunkData chunk = ndata as VFVoxelChunkData;
		byte[] data = SaveLoad.TryGetChunkData(chunk.ChunkPosLod);
		if(data != null)
		{
			chunk.OnDataLoaded(data, true);
		}
		else
		{
			DataLoader.AddRequest(chunk);
		}
	}
	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		byte volume = cdata.DataVT[0];
		byte type = cdata.DataVT[1];
		byte[] data = VFVoxelChunkData.s_ChunkDataPool.Get();
		if(volume != 0)
		{
			for(int i = 0; i < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT;)
			{
				data[i++] = volume;
				data[i++] = type;
			}
		}
		else
		{
			Array.Clear(data, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
		}		
		cdata.SetDataVT(data, true);
		return true;
	}
	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		return new VFVoxel(cdata.DataVT[0], cdata.DataVT[1]);
	}
	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqMC req = ireq as SurfExtractReqMC;
		VFVoxelChunkGo vfGo = VFVoxelChunkGo.CreateChunkGo(req);
		req._chunk.AttachChunkGo(vfGo, req);
#if !ScopedCollider
		if(vfGo != null){
			bChunkColliderRebuilding = true;
			vfGo._mc.sharedMesh = null;
			vfGo._mc.sharedMesh = vfGo._mf.mesh;
			foreach(Transform subGo in vfGo.transform)
			{
				VFVoxelChunkGo vfGoSub = subGo.GetComponent<VFVoxelChunkGo>();
				if(vfGoSub != null)
				{
					vfGoSub._mc.sharedMesh = null;
					vfGoSub._mc.sharedMesh = vfGoSub._mf.mesh;
				}
			}
			bChunkColliderRebuilding = false;
		}
#endif
	}
	public void OnBegUpdateNodeData(ILODNodeData ndata){		
		if(LODOctreeMan._maxLod == 0)
		{
			VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
			if(cdata.LOD == 0 && cdata.IsNodePosChange()){
				SceneChunkDependence.Instance.ValidListRemove(cdata.ChunkPosLod, EDependChunkType.ChunkTerMask);
			}
		}
	}
	public void OnEndUpdateNodeData(ILODNodeData ndata){
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		if(cdata.LOD == 0 && cdata.IsEmpty)
		{
			SceneChunkDependence.Instance.ValidListAdd(cdata.ChunkPosLod, EDependChunkType.ChunkTerEmp);
		}
	}
	public void OnDestroyNodeData(ILODNodeData ndata){
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		if(cdata.LOD == 0)
		{
			SceneChunkDependence.Instance.ValidListRemove(cdata.ChunkPosLod, EDependChunkType.ChunkTerMask);
		}
	}
#endregion

#region Collider
	static int _frameCnt;
	static List<VFVoxelChunkGo> s_tmpVfGoSubs = new List<VFVoxelChunkGo> (4);
	public static List<IntVector3> OrderedOffsetList = new List<IntVector3>();
	public static int CompareCPosForColliderSort(IntVector3 c1, IntVector3 c2)
	{
		int ret = (c1.x*c1.x+c1.z*c1.z) - (c2.x*c2.x+c2.z*c2.z);
		if(ret == 0)	return c2.y - c1.y;	// larger y has prior to avoid monster underground
		return ret;
	}
	public static void PrepareColliderOrder()
	{
		OrderedOffsetList.Clear ();
#if ScopedCollider
		int yhalf = (LODOctreeMan._yLodRootChunkCount >> 1)-1;
		if(yhalf < 2) yhalf = 2;
		int xhalf = 3;
		int zhalf = 3;
		for(int i = -xhalf; i <= xhalf; i++)
		{
			for(int k = -zhalf; k <= zhalf; k++)
			{
				for(int j = -yhalf; j <= +yhalf; j++)
				{
					OrderedOffsetList.Add(new IntVector3(i,j,k));
				}
			}
		}
		OrderedOffsetList.Sort(new Comparison<IntVector3>(CompareCPosForColliderSort));
#endif
	}
	public static VFVoxelChunkGo GenOneCollider(VFVoxelChunkData chunk)
	{
		VFVoxelChunkGo vfGo;
		if (chunk == null || (vfGo = chunk.ChunkGo) == null) {
			return null;
		}
		if (null != vfGo.Mc.sharedMesh) {
			return null;
		}

		bChunkColliderRebuilding = true;

		s_tmpVfGoSubs.Clear ();
		vfGo.GetComponentsInChildren<VFVoxelChunkGo> (s_tmpVfGoSubs);
		int nVfGoSubs = s_tmpVfGoSubs.Count;
		for(int i = 0; i < nVfGoSubs; i++)
		{
			s_tmpVfGoSubs[i].Mc.sharedMesh = null;
			s_tmpVfGoSubs[i].Mc.sharedMesh = s_tmpVfGoSubs[i].Mf.sharedMesh;
		}
		return vfGo;
	}
	void FixedUpdate()
	{
		if (LodMan != null && LodMan.Observer != null && _frameCnt != Time.frameCount)
		{
			_frameCnt = Time.frameCount;
			VFVoxelChunkGo vfgo;
			VFVoxelChunkData chunk;
			Vector3 posCurrent = LodMan.Observer.position;
			Vector3 posOffset  = posCurrent - LodMan.LastRefreshPos;
			int cx = ((int)posCurrent.x)>>VoxelTerrainConstants._shift;
			int cy = ((int)posCurrent.y)>>VoxelTerrainConstants._shift;
			int cz = ((int)posCurrent.z)>>VoxelTerrainConstants._shift;
			vfgo = null;
			if(LodMan.LastRefreshPos.y > 0 && posOffset.sqrMagnitude > 256)	// for fast speed
			{
				chunk = _voxels.readChunk( cx, cy, cz);
				vfgo = GenOneCollider(chunk);
				if(null == vfgo)
				{
					Vector3 posPredict = posOffset + posCurrent;
					int cx1 = ((int)posPredict.x)>>VoxelTerrainConstants._shift;
					int cy1 = ((int)posPredict.y)>>VoxelTerrainConstants._shift;
					int cz1 = ((int)posPredict.z)>>VoxelTerrainConstants._shift;
					chunk = _voxels.readChunk( cx1, cy1, cz1);
					vfgo = GenOneCollider(chunk);
				}
				if(null != vfgo)
				{
					if(chunk.ChunkGo == vfgo)
					{
						SceneChunkDependence.Instance.ValidListAdd(vfgo.Data.ChunkPosLod, EDependChunkType.ChunkTerCol);
						vfgo.OnColliderReady();
					}
				}
			}

			if(null == vfgo)
			{
				int nOffset = OrderedOffsetList.Count;
				for(int n = 0; n < nOffset; n++)
				{
					IntVector3 vecOffset = OrderedOffsetList[n];
					chunk = _voxels.readChunk(vecOffset.x+cx, vecOffset.y+cy, vecOffset.z+cz );
					vfgo = GenOneCollider(chunk);
					if(null != vfgo)
					{
						if(chunk.ChunkGo == vfgo)
						{
							SceneChunkDependence.Instance.ValidListAdd(vfgo.Data.ChunkPosLod, EDependChunkType.ChunkTerCol);
							vfgo.OnColliderReady();
						}
						break;
					}
				}
			}
			if(null == vfgo)
			{
				bChunkColliderRebuilding = false;
			}
		}
	}
#endregion

	public bool IsPosHasCollider(IntVector4 nodePos)
	{
		if(nodePos.w != 0)	return false;

		VFVoxelChunkData chkData = _voxels.readChunk(nodePos.x >> VoxelTerrainConstants._shift,
		                                             nodePos.y >> VoxelTerrainConstants._shift,
		                                             nodePos.z >> VoxelTerrainConstants._shift,
		                                             nodePos.w);
		return (chkData != null && chkData.ChunkGo != null && chkData.ChunkGo.Mc != null);
	}
	public bool IsPosInGenerating(ref Vector3 pos)
	{
		int cx = ((int)pos.x)>>VoxelTerrainConstants._shift;
		int cy = ((int)pos.y)>>VoxelTerrainConstants._shift;
		int cz = ((int)pos.z)>>VoxelTerrainConstants._shift;
		VFVoxelChunkData data = _voxels.readChunk(cx, cy, cz);
		if(data == null)
			return true;
		if(data.ChunkGo != null && data.ChunkGo.Mc.sharedMesh != null)
			return false;
		if(data.BuildStep == VFVoxelChunkData.BuildStep_NotInBuild && data.ChunkGo == null)
			return false;
		
		return true;
	}
	public bool IsInTerrain(float fx, float fy, float fz)
	{
		int x = Mathf.RoundToInt(fx);
		int y = Mathf.RoundToInt(fy);
		int z = Mathf.RoundToInt(fz);
		int cx = x >> VoxelTerrainConstants._shift;
		int cy = y >> VoxelTerrainConstants._shift;
		int cz = z >> VoxelTerrainConstants._shift;
		VFVoxelChunkData chunk = _voxels.readChunk(cx, cy, cz);
		if(chunk == null || chunk.DataVT.Length < VFVoxel.c_VTSize)	return false;
		byte cVol;
		if(chunk.IsHollow)
		{
			cVol = chunk.DataVT[0];
			if(cVol < 128)	return false;
			else			return true;
		}
		int idx = VFVoxelChunkData.OneIndex(x&VoxelTerrainConstants._mask,
		                                    y&VoxelTerrainConstants._mask,
		                                    z&VoxelTerrainConstants._mask);
		int idxVT = idx<<VFVoxel.c_shift;
		cVol = chunk.DataVT[idxVT];
		if(cVol == 0)	return false;
		if(cVol == 255)	return true;
		byte oVol;
		if(cVol < 128)
		{
			oVol = chunk.DataVT[idxVT-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
			if(oVol >= 128) return ((128-oVol)/(float)(cVol-oVol)+y-1) > fy;
			oVol = chunk.DataVT[idxVT+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
			if(oVol >= 128) return ((128-cVol)/(float)(oVol-cVol)+y) < fy;
			return false;
		}
		else
		{
			oVol = chunk.DataVT[idxVT-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
			if(oVol < 128) return ((128-oVol)/(float)(cVol-oVol)+y-1) < fy;
			oVol = chunk.DataVT[idxVT+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
			if(oVol < 128) return ((128-cVol)/(float)(oVol-cVol)+y) > fy;
			return true;
		}
	}
	public void SafeCheckVelocity(PhysicsCharacterMotor rigidbody)
	{
		Vector3 pos = rigidbody.transform.position;
		if(pos.y < 0 )
		{
			rigidbody.transform.position = pos - pos.y * Vector3.up;
			return;
		}
		
		if(IsPosInGenerating(ref pos))
			rigidbody.FreezeGravity = true;
		else //if(!rigidbody.grounded)
		{
			rigidbody.FreezeGravity = false;
			float upRayLen = 1500.0f;
			float dnRayLen = 400.0f;
			int vfterrainLayer = Pathea.Layer.VFVoxelTerrain;
			RaycastHit hit;
			if(Physics.Raycast(pos+Vector3.up*upRayLen, Vector3.down,out hit,upRayLen-0.1f,1<<vfterrainLayer))
			{
				// Add check if part of player is under terrain, may cause player jump up out of cave
				if(pos.y >= hit.point.y-1.5f || !Physics.Raycast(pos+Vector3.up, Vector3.down,dnRayLen,1<<vfterrainLayer))
				{
					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
					if(0 != voxel.Type && voxel.Volume > 127)
					{
						voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y - 1, (int)pos.z);
						if(0 != voxel.Type && voxel.Volume > 127)
						{
							Debug.Log("PlayerFallenCheckFix");
							rigidbody.transform.position = hit.point;
						}
					}
				}
			}
		}
	}
}
