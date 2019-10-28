#define BegEndUpdateNodeDataInSameThread	// Beg/End UpdateNodeData should be in the same thread.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

//
// Note: BuildStep/ChunkPos/DataVT? need lock/sync between thread. such as lod change the chunk while SurfExtractor is proceeding it.
// BegUpdateNodeData/EndUpdateNodeData keep mutual exclusion
public partial class VFVoxelChunkData : ILODNodeData
{
	public delegate void ProcOnWriteVoxel(LODOctreeNode node, byte oldVol, byte newVol, int idxVol);

	public const int BuildStep_NotInBuild 		= 0;
	public const int BuildStep_StartDataLoading = 1;	// Now, Invalid chunks will be stuck at this step
	public const int BuildStep_FinDataLoading 	= 2;
	public const int BuildStep_StartGoCreating  = 3;
	public const int BuildStepMask				= 0xf;
	public const int StampMask					= ~BuildStepMask;

	// members
	private IVxChunkHelperProc _helperProc;
	private LODOctreeNode _node;
	private IntVector4 _chunkPosLod = new IntVector4(0,0,0,-1);
	private VFVoxelChunkGo _goChunk = null;// chunkGo will be destroyed at condition:1:Hollow; 2:new GoAttached; 3:Destroyed by lod node updating
	private int  _stampOfUpdating = 0;
	private bool _bNoVerts = false;
	private bool _bFromPool = false;
	private byte[] _chunkData;
	// Get
	private Vector3 Pos{			get{return new Vector3(	
											   _chunkPosLod.x<<VoxelTerrainConstants._shift,
											   _chunkPosLod.y<<VoxelTerrainConstants._shift,
											   _chunkPosLod.z<<VoxelTerrainConstants._shift);					}}
	public int LOD{					get{return _chunkPosLod.w;													}}
	public IntVector4 ChunkPosLod{	get{return _chunkPosLod;													}}
	public byte[] DataVT{			get{return _chunkData;														}}
	public bool IsHollow{			get{return _chunkData.Length == VFVoxel.c_VTSize;							}}
	public bool IsEmpty{ 			get{return (System.Object)_goChunk == null;									}} // ILodNodeData
	public VFVoxelChunkGo ChunkGo{	get{return _goChunk;														}}
	public bool IsNoVerts{			get{return _bNoVerts;														}}
	// When chunk update too fast, a chunk may change its position with the same lod level because delay
	// So, it is not enough just to check lod level.
	public bool IsIdle{				get{return BuildStep == BuildStep_NotInBuild;								}} // ILodNodeData
	public int  StampOfUpdating{	get{return _stampOfUpdating;												}}
	public int  SigOfChnk{			get{return _chunkPosLod.GetHashCode()+(((LODOctreeMan.MaxLod+1)*_helperProc.ChunkSig)<<28);		}}
	public int  SigOfType{			get{return _helperProc.ChunkSig;											}}
	public int  VertsThreshold{		get{return (_node!=null) ? SurfExtractorsMan.c_vertsCnt4Pool : SurfExtractorsMan.c_vertsCntMax;		}}
	// Set
	public int  BuildStep{					get{return _stampOfUpdating&BuildStepMask;							} 
											set{_stampOfUpdating=(_stampOfUpdating&StampMask)|	value;			}}
	public IVxChunkHelperProc HelperProc{	get{return _helperProc;												}
											set{_helperProc = value;											}}
	public IntVector4 ChunkPosLod_w{		set{_chunkPosLod = value;											}}	
	
	// Methods
	public VFVoxelChunkData(LODOctreeNode node)
	{
		_node = node;
		_chunkData = S_ChunkDataNull;
	}
	public VFVoxelChunkData(LODOctreeNode node, byte[] dataArray, bool bFromPool = false)
	{
		_node = node;
		_chunkData = dataArray;
		_bFromPool = bFromPool;
		BuildStep = BuildStep_FinDataLoading;
	}
	/*~VFVoxelChunkData()
	{
		ClearMem();
	}*/
#region helper
	public void ClearMem() // for all not lod0 chunks
	{
		SetDataVT(S_ChunkDataNull);
		BuildStep = BuildStep_NotInBuild;
	}
	public void SetDataVT(byte[] data, bool bFromPool = false)
	{			
		if(_helperProc != null)	_helperProc.ChunkProcPreSetDataVT(this, data, bFromPool);
		if(_bFromPool)			s_ChunkDataPool.Free(_chunkData);	
		_chunkData = data;
		_bFromPool = bFromPool;
	}
	public VFVoxelChunkData GetNodeData(int idx)
	{
		return _node == null ? null : (VFVoxelChunkData)_node._data[idx];
	}
	public bool IsStampIdentical(int stamp){	return stamp == _stampOfUpdating;						}
	public bool IsMatch(int sig, int stamp){	return stamp == _stampOfUpdating && sig == SigOfChnk;	}
	public bool IsNodePosChange(){				return _node.CX != _chunkPosLod.x || _node.CY != _chunkPosLod.y || _node.CZ != _chunkPosLod.z || _node.Lod != _chunkPosLod.w;	}
#endregion
#region interface 
	public void UpdateTimeStamp(){				_stampOfUpdating+=0x10;				}
	public void BegUpdateNodeData()
	{
		//NOTE: If this chunkData is already in dataReading or in rebuildList or in computingList, out-of-date chunk mesh will be attached.
		//Note: ChunkPosLod is a new IntVector4, so it is independent, and ensure _chunkPosLod will be modified in this method.
		_helperProc.OnBegUpdateNodeData (this);

		int cx = _node.CX, cy = _node.CY, cz = _node.CZ, lod = _node.Lod;
		if(_chunkPosLod.x != cx || _chunkPosLod.y != cy || _chunkPosLod.z != cz || _chunkPosLod.w != lod)
		{
			//DestroyGameObject(); // postpond destroy to AttachChunkGo
			SetDataVT(S_ChunkDataNull);	// reset data before setting chunkPos to assure delegate in setDataVt can get the correct data.
			_chunkPosLod.x = cx; 				
			_chunkPosLod.y = cy; 				
			_chunkPosLod.z = cz; 				
			_chunkPosLod.w = lod; 
			BuildStep = BuildStep_StartDataLoading;
			_bNoVerts = false;
		}

		if(BuildStep == BuildStep_NotInBuild)
		{
			//Debug.LogWarning("[VFVoxelChunkData]:BuildStep_NotInBuild"+((null != (System.Object)_goChunk)?"!=" : "==" ) + "NULL : NoVerts :" + _bNoVerts + _chunkPosLod);
			if(_chunkData.Length == 0 && _chunkPosLod.w == 0)			//need update data
			{
				BuildStep = BuildStep_StartDataLoading;
			}
			else if(null == (System.Object)_goChunk && !_bNoVerts)		//need update mesh
			{
				BuildStep = (_chunkData.Length == 0) ? BuildStep_StartDataLoading : BuildStep_FinDataLoading;
			}
			else
			{
				EndUpdateNodeData();
				return;
			}
		}
		if(BuildStep == BuildStep_StartDataLoading)
		{
			_helperProc.ChunkProcPreLoadData(this);
		}
		else if(BuildStep >= BuildStep_FinDataLoading) // If req from 1->2->1, Buildstep will keep 3, and we should add it to buildList
		{
			AddToBuildList();
		}
	}
	public void EndUpdateNodeData()
	{
		BuildStep = BuildStep_NotInBuild;
		if(!IsHollow && _bNoVerts && _chunkPosLod.w > 0)	// These chunks are not supposed to be accessed.
		{
			SetDataVT(S_ChunkDataNull);
		}
		_helperProc.OnEndUpdateNodeData(this);
		_node.OnEndUpdateNodeData(this);
	}
	public void OnDestroyNodeData()
	{
		_helperProc.OnDestroyNodeData(this);
		DestroyChunkGO();
		ClearMem();
	}
#endregion
	public void DestroyChunkGO()
	{
		if(null != (System.Object)_goChunk)	// In thread, Monobehaviour's compare can not be used
		{
			if(_node == null)	VFGoPool<VFVoxelChunkGo>.FreeGo(_goChunk);		// for those cases not in thread
			else				VFGoPool<VFVoxelChunkGo>.ReqFreeGo(_goChunk);	// for those cases in thread use lock
			_goChunk = null;
		}		
	}


	public void OnDataLoaded(byte[] data, bool bFromPool = false)
	{
		SetDataVT(data, bFromPool);
		BuildStep = BuildStep_FinDataLoading;
		AddToBuildList();
		
		if (GameConfig.IsMultiMode && SigOfType == VFVoxelTerrain.self.ChunkSig)
			ChunkManager.Instance.AddCacheReq(_chunkPosLod.ToVector3());
	}
	private bool FinHollowUpdate()
	{
		DestroyChunkGO();
		_bNoVerts = true;
		BuildStep = BuildStep_NotInBuild;
		if(_node != null){
			EndUpdateNodeData();
		}
		return true;
	}
	public void AddToBuildList()	// Can be called from the main thread
	{
		if(IsHollow){	FinHollowUpdate();	return;		}
		BuildStep = BuildStep_StartGoCreating;
		_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqMC.Get (this, VertsThreshold));
	}
	// The above functions can be invoked in other thread
	// The below function will be invoked in main thread.
	public void AttachChunkGo(VFVoxelChunkGo vfGo, SurfExtractReqMC req = null)
	{
		if(vfGo != null)
		{
			float fScale = (1<<_chunkPosLod.w);
#if UNITY_EDITOR
			vfGo.name = PatheaScript.Util.GetChunkName(_chunkPosLod);
#endif
			vfGo.transform.localScale = new Vector3(fScale,fScale,fScale);
			vfGo.transform.position = Pos;
			vfGo.Data = this;
			vfGo.OriginalChunkGo = null;
			if(null != _goChunk && _goChunk.name.Equals(vfGo.name))
			{
				if(_goChunk.OriginalChunkGo != null)
				{
					vfGo.OriginalChunkGo = _goChunk.OriginalChunkGo;
					_goChunk.OriginalChunkGo = null;
				}
				else if(_goChunk.Mc.sharedMesh != null)
				{
					vfGo.OriginalChunkGo = _goChunk;
					_goChunk.Mr.enabled = false;
					_goChunk = null;
				}
			}
			else if(_node != null && SigOfType == VFVoxelTerrain.self.ChunkSig)
			{
				_node.OnMeshCreated();
			}
		}
		if(null != _goChunk)	VFGoPool<VFVoxelChunkGo>.FreeGo(_goChunk);
		_goChunk = vfGo;		
		_bNoVerts = vfGo == null;

		if(_node != null && req != null )
		{
			if(!req.IsInvalid)
			{
#if BegEndUpdateNodeDataInSameThread
				lock(s_lstReqsToEndUpdateNodeData)
				{
					s_lstReqsToEndUpdateNodeData.Add(new EndUpdateReq(req));
				}
#else
				EndUpdateNodeData();
#endif
			}
			return;
		}

		BuildStep = BuildStep_NotInBuild;
	}
#region EventHandlers
	public void OnGoColliderReady()
	{
		if(_node != null && SigOfType == VFVoxelTerrain.self.ChunkSig)
		{
			_node.OnPhyxReady();
		}
	}
#endregion
#region read_write
	public VFVoxel this[IntVector3 idx]{
		get{
			return ReadVoxelAtIdx(idx.x, idx.y, idx.z);
		}
	}
	public VFVoxel ReadVoxelAtPos(int x, int y, int z)
	{
		return ReadVoxelAtIdx((x>>LOD)&VoxelTerrainConstants._mask,
							(y>>LOD)&VoxelTerrainConstants._mask,
							(z>>LOD)&VoxelTerrainConstants._mask);
	}
	public VFVoxel ReadVoxelAtIdx(int x, int y, int z)
	{
		if(_chunkData == S_ChunkDataNull)
		{
			return new VFVoxel(0);	// Transvoxel will get this pos
		}
		
		try{
			if(IsHollow) return _helperProc.ChunkProcExtractData(this, x, y, z);

			int indexVT = OneIndex(x,y,z)*VFVoxel.c_VTSize;
			return new VFVoxel(_chunkData[indexVT], _chunkData[indexVT+1]);
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to read voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod);
			return new VFVoxel(0);
		}
	}
	public bool WriteVoxelAtIdx(int x, int y, int z, VFVoxel voxel, bool bLodUpdate = false, ProcOnWriteVoxel OnWrite = null)
	{
		if(_chunkData == S_ChunkDataNull)							return false;
		try{
			if(IsHollow && !_helperProc.ChunkProcExtractData(this))	return false;

			int index = OneIndex(x,y,z);
			int indexVT = index*VFVoxel.c_VTSize;
			if(OnWrite != null) OnWrite(_node, _chunkData[indexVT], voxel.Volume, indexVT);
			_chunkData[indexVT] = voxel.Volume;
			_chunkData[indexVT+1] = voxel.Type;
			UpdateTimeStamp();
			if(_node == null)		AddToBuildList();
			else if(_node.IsInReq)	AddToBuildList();
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod);
		}

		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(x,y,z,_chunkPosLod);
//		VFVoxelTerrain.self.lodDataUpdate.threadProc();
		
		return true;
	}
	public bool WriteVoxelAtIdxNoReq(int x, int y, int z, VFVoxel voxel, bool bLodUpdate = false)
	{
		if(_chunkData == S_ChunkDataNull)							return false;
		try{
			if(IsHollow && !_helperProc.ChunkProcExtractData(this))	return false;

			int index = OneIndex(x,y,z);
			int indexVT = index*VFVoxel.c_VTSize;
			_chunkData[indexVT] = voxel.Volume;
			_chunkData[indexVT+1] = voxel.Type;
			UpdateTimeStamp();
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod);
		}

		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(x,y,z,_chunkPosLod);
//		VFVoxelTerrain.self.lodDataUpdate.threadProc();
		
		return true;
	}	
	public bool WriteVoxelAtIdx4LodUpdate(int x, int y, int z, VFVoxel voxel)
	{
		if(_chunkData == null || _chunkData == S_ChunkDataNull)		return false;
		try{
			if(IsHollow && !_helperProc.ChunkProcExtractData(this))	return false;

			int index = OneIndexNoPrefix(x,y,z);
			int indexVT = index*VFVoxel.c_VTSize;
			_chunkData[indexVT] = voxel.Volume;
			_chunkData[indexVT+1] = voxel.Type;
			UpdateTimeStamp();
			if(_node == null)		AddToBuildList();
			else if(_node.IsInReq)	AddToBuildList();
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write lod voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod);
		}
		return true;
	}
	public bool BeginBatchWriteVoxels()
	{
		if(_chunkData == S_ChunkDataNull)						return false;
		if(IsHollow && !_helperProc.ChunkProcExtractData(this))	return false;
		return true;
	}
	public void EndBatchWriteVoxels(bool bLodUpdate = false)
	{
		UpdateTimeStamp();
		if(_node == null)		AddToBuildList();
		else if(_node.IsInReq)	AddToBuildList();
		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(0,0,0,_chunkPosLod);
		//		VFVoxelTerrain.self.lodDataUpdate.threadProc();
	}
#endregion
}
