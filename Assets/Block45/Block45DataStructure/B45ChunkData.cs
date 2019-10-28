//#define HideChunkGO	// hide chunk go for inactive node

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

// TBD : code for owner
// TODO : code for write voxel to unloaded chunk
// TODO : save and load save data
public class B45ChunkData{
	public enum EBuildStep{
		EBuildStep_NotStart,
		EBuildStep_StartDataLoading,
		EBuildStep_FinDataLoading,
		EBuildStep_StartGoCreating,
		EBuildStep_FinGoCreating,
	};
	public static uint DS_IN_QUEUE = 0x1;
	public static uint DS_BEING_DESTROYED = 0x2;
	private uint d_state;
	
	// static chunk data buffer
	public static GenericArrayPool<byte> s_ChunkDataPool = new GenericArrayPool<byte>(Block45Constants.VOXEL_ARRAY_LENGTH_VT);
	public static List<B45ChunkGo> s_ChunkGosToDestroy = new List<B45ChunkGo>();
	public static byte[] s_ChunkDataAir = new byte[B45Block.Block45Size]{0,0};
	public static byte[] S_ChunkDataNull = new byte[0];
	private BiLookup<int, B45ChunkData> _buildPosDatList;
	private LODOctreeNode _node = null;
	public BlockVectorNode _bvNode;
	private IntVector4 _chunkPosLod = new IntVector4(0,0,0,-1);
	private IntVector4 OldChunkPos;	// Var for onlinePlay
	private B45ChunkGo _goChunk = null;// chunkGo will be destroyed at condition:1:Hollow; 2:new GoAttached; 3:Destroyed by lod node updating
	public int _maskTrans = 0;	// This var should be put into ChunkGo, but for the sake of threading, it is here.
	public int svn_key;
	public int svn_key_ba;
	public List<UpdateVector> updateVectors;
	public List<UVKeyCount> uvVersionKeys;
	public int[] UpdateDatas { get { return updateVectors.Select(iter => iter.Data).ToArray(); } }
	
	private int _buildStep;
	private bool _bNoVerts = false;
	private bool _bFromPool = false;
	private byte[] _chunkData;
	private ChunkColliderMan colliderMan;
	// Get
	private Vector3 Pos{			get{return new Vector3(	
														_chunkPosLod.x<<Block45Constants._shift,
														_chunkPosLod.y<<Block45Constants._shift,
														_chunkPosLod.z<<Block45Constants._shift);	}	}
	public int LOD{					get{return _chunkPosLod.w;									}	}
	public IntVector4 ChunkPos{		get{return _chunkPosLod;							}	}
	public IntVector4 ChunkPosLod{	get{return _chunkPosLod;							}	}
	public int BuildStep{			get{return _buildStep;								}	}
	public byte[] DataVT{			get{return _chunkData;								}	}
	public bool IsHollow{			get{return _chunkData.Length == B45Block.Block45Size;	}	}
	public B45ChunkGo ChunkGo{	get{return (B45ChunkGo)_goChunk;				}	}
	// When chunk update too fast, a chunk may change its position with the same lod level because delay
	// So, it is not enough just to check lod level.
	public bool IsChunkInReq{		get{return _node==null || _node.IsInReq;			}	}
	public bool IsGoCreating{		get{return _buildStep>=(int)EBuildStep.EBuildStep_StartGoCreating;	}	}	// if mod too fast, _buildStep may be EBuildStep_FinGoCreating
	public bool IsDataLoading{		get{return _buildStep==(int)EBuildStep.EBuildStep_StartDataLoading;	}	}
	public int StampOfChnkUpdating{	get{return VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod);		}	}
	// Set
	public BiLookup<int, B45ChunkData> BuildList{	
									set{_buildPosDatList = value;	}}	// Used in old random map code, no need to use lock
	public IntVector4 ChunkPosLod_w{set{_chunkPosLod = value;		}}	// Used in StdVoxelDataSource/ lodupdate, no need to use lock
	public byte[] DataVT_w{			set{if(_bFromPool){	s_ChunkDataPool.Free(_chunkData);}	_chunkData = value;	}}	// Used in lodupdate, no need to use lock ??? need check again
	public bool IsStampIdentical(int stamp){	return stamp == VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod);	}	
	public void AddToBuildList(){	// Can be called from the main thread

		if(IsHollow){	FinHollowUpdate();	return;		}
		_buildStep = (int)EBuildStep.EBuildStep_StartGoCreating;
		_buildPosDatList.Add(VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod), this);
		
		if(colliderMan != null)
			colliderMan.addRebuildChunk(ChunkPos.XYZ);
		
		setInQueue();
	}
	public void safeToRemoveCollider()
	{
		colliderMan.colliderBuilt(ChunkPos.XYZ);
	}
//	public B45ChunkData(LODOctreeNode node)
//	{
//		_node = node;
//		_chunkData = new byte[0];
////		_lod = node != null ? node._lod : 0;
//		_buildStep = (int)EBuildStep.EBuildStep_NotStart;
//	}
//	public B45ChunkData(LODOctreeNode node, B45Block voxel)	// create hollow chunk
//	{
//		_node = node;
//		_chunkData = new byte[B45Block.Block45Size];
//		_chunkData[0] = voxel.blockType;
//		_chunkData[1] = voxel.materialType;
////		_lod = node != null ? node._lod : 0;
//		_buildStep = (int)EBuildStep.EBuildStep_FinDataLoading;
//	}
	public B45ChunkData(byte[] dataArray)
	{
		_chunkData = dataArray;
		_buildStep = (int)EBuildStep.EBuildStep_FinDataLoading;
	}
	public B45ChunkData(ChunkColliderMan _colliderMan)
	{
		_chunkData = s_ChunkDataPool.Get();
		_bFromPool = true;
		
		Array.Clear(_chunkData, 0, Block45Constants.VOXEL_ARRAY_LENGTH_VT);

		_buildStep = (int)EBuildStep.EBuildStep_FinDataLoading;
		
//		OpList.Add("init " + StackTraceUtility.ExtractStackTrace());
		
		d_state = 0;
		colliderMan = _colliderMan;
	}
	/*
	~B45ChunkData()
	{
		ClearMem();
	}
	*/
	public void bp()
	{
//		if(ChunkPos.x == 3056 &&
//			ChunkPos.y == 31 &&
//			ChunkPos.z == 1522 
//			)
//		{
//			int slkdf = 0;
//		}
	}
//	List<string> OpList = new List<string>();
	public bool isInQueue{get{return (d_state & DS_IN_QUEUE) > 0;}}
	public bool isBeingDestroyed{get{return (d_state & DS_BEING_DESTROYED) > 0;}}
	public void setInQueue()
	{
//		OpList.Add("setInQueue " + StackTraceUtility.ExtractStackTrace());
		d_state |= DS_IN_QUEUE;
	}
	public void setNotInQueue()
	{
//		OpList.Add("setNotInQueue " + StackTraceUtility.ExtractStackTrace());
		d_state &= ~B45ChunkData.DS_IN_QUEUE;
	}
	public void setBeingDestroyed()
	{
//		OpList.Add("setBeingDestroyed " + StackTraceUtility.ExtractStackTrace());
		d_state |= DS_BEING_DESTROYED;
	}
	public void InitUpdateVectors()
	{
		updateVectors = new List<UpdateVector>();
		uvVersionKeys = new List<UVKeyCount>();
		uvVersionKeys.Add(new UVKeyCount());
		svn_key = 1;
		svn_key_ba = 1;
	}
	public void ClearMem()
	{
		if(isInQueue)
		{
//			Debug.LogError("a chunk in process q is being destroyed!");
		}
//		OpList.Add("clear " + StackTraceUtility.ExtractStackTrace());
		d_state = 0;
		DataVT_w = S_ChunkDataNull;
		_bFromPool = false;
		_buildStep = (int)EBuildStep.EBuildStep_NotStart;
	}
	public void ApplyUpdateVectors()
	{
		if(updateVectors == null)return;
		
		//int opVec;
		B45Block blk;
		for(int i = 0; i < updateVectors.Count; i++)
		{
			//opVec = updateVectors[i].xyz0 + (updateVectors[i].xyz1 << 8);
			
			blk.blockType = updateVectors[i].voxelData0;
			blk.materialType = updateVectors[i].voxelData1;
			//WriteVoxelAtIdxNoPrefix(opVec, blk);
		}
	}
	
	public string GenChunkIdentifier()
	{
		return ""+ChunkPos.x + "_" + ChunkPos.y + "_" + ChunkPos.z;
	}
#region create_destroy
	public void CreateGameObject()
	{
		//NOTE: If this chunkData is already in dataReading or in rebuildList or in computingList, out-of-date chunk mesh will be attached.
		int cx = _node.CX, cy = _node.CY, cz = _node.CZ, lod = _node.Lod;
		if(_chunkPosLod.x != cx || _chunkPosLod.y != cy || _chunkPosLod.z != cz || _chunkPosLod.w != lod)
		{
//			OldChunkPos = _chunkPosLod;
//			if (null != OldChunkPos)
//				ChunkManager.Self.DelCacheReq(OldChunkPos.ToVector3());
			
			//DestroyGameObject(); // postpond destroy to AttachChunkGo
			_chunkPosLod.x = cx; 
			_chunkPosLod.y = cy; 
			_chunkPosLod.z = cz; 
			_chunkPosLod.w = lod; 
			_buildStep = (int)EBuildStep.EBuildStep_StartDataLoading;
			DataVT_w = S_ChunkDataNull;
			_bFromPool = false;
			_bNoVerts = false;
		}

		if(_buildStep >= (int)EBuildStep.EBuildStep_FinGoCreating)
		{
			if(null == (System.Object)_goChunk && !_bNoVerts)
			{
				_buildStep = (_chunkData.Length <= 0) ? (int)EBuildStep.EBuildStep_StartDataLoading : (int)EBuildStep.EBuildStep_FinDataLoading;
			}
			else
			{
				_node.OnEndUpdateNodeData(null);
				return;
			}
		}
		if(_buildStep < (int)EBuildStep.EBuildStep_FinDataLoading)
		{
			// load save data
		}
		else if(_buildStep < (int)EBuildStep.EBuildStep_FinGoCreating)
		{
			if(!_buildPosDatList.Contains(VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod), this))
			{
				AddToBuildList();
			}
		}
	}
	public void OnDataLoaded(byte[] data, bool bFromPool = false)
	{
		DataVT_w = data;
		_bFromPool = bFromPool;
		_buildStep = (int)EBuildStep.EBuildStep_FinDataLoading;
		//if(_node==null || _node.IsInReq)		// Now dataload is executed in the same thread of lodRefresh synchronously, so Reqs always aren't OutOfDate.
		AddToBuildList();

//		if (GameConfig.IsMultiMode)
//			ChunkManager.Instance.AddCacheReq(_chunkPosLod.ToVector3());
	}
	public void DestroyChunkGO()
	{
		if(null != (System.Object)_goChunk)	// In thread, Monobehaviour's compare can not be used
		{
			if(_node == null)	_goChunk.Destroy();	// for those cases not in thread
			else				s_ChunkGosToDestroy.Add(_goChunk);
			_goChunk = null;
		}		
	}
	public void DestroyGameObject()
	{
		DestroyChunkGO();
		ClearMem();
	}	
	private bool FinHollowUpdate()
	{
		DestroyChunkGO();
		if(_node != null)	_node.OnEndUpdateNodeData(null);
		_bNoVerts = true;
		_buildStep = (int)EBuildStep.EBuildStep_FinGoCreating;
		return true;
	}
	#region DEBUG
	public static string OccupiedVecsStr(byte[] byteData){
		string str = "";
		for(int z = 0; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; z++)
		{
			for(int y = 0; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; y++)
			{
				for(int x = 0; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; x++)
				{
					B45Block blk;
					
					blk.blockType = byteData[B45ChunkData.OneIndexNoPrefix(x,y,z) * B45Block.Block45Size];
					//blk.materialType = _chunkData[OneIndexNoPrefix(x,y,z) * B45Block.Block45Size + 1];
					if( blk.blockType != 0)
					{
						str += "(" + (x-1) + "," + (y-1) + "," + (z-1) + "); ";
						
					}
						
				}
			}
		}
		return str;
	}
	public string OccupiedVecsStr(){
		string str = "";
		for(int z = 0; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; z++)
		{
			for(int y = 0; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; y++)
			{
				for(int x = 0; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; x++)
				{
					B45Block blk;
					
					blk.blockType = _chunkData[OneIndexNoPrefix(x,y,z) * B45Block.Block45Size];
					//blk.materialType = _chunkData[OneIndexNoPrefix(x,y,z) * B45Block.Block45Size + 1];
					if( blk.blockType != 0)
					{
						str += "(" + (x-1) + "," + (y-1) + "," + (z-1) + "); ";
						
					}
						
				}
			}
		}
		return str;
	}
	public List<BuildingEditOps.OpInfo> OccupiedVecs(){
		List<BuildingEditOps.OpInfo> ret = new List<BuildingEditOps.OpInfo>();
		
//		for(int z = 0; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; z++)
//		{
//			for(int y = 0; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; y++)
//			{
//				for(int x = 0; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; x++)
//				{
//					B45Block blk;
//					
//					blk.blockType = _chunkData[OneIndexNoPrefix(x,y,z) * B45Block.Block45Size];
//					//blk.materialType = _chunkData[OneIndexNoPrefix(x,y,z) * B45Block.Block45Size + 1];
//					if( blk.blockType != 0)
//					{
//						BuildingEditOps.OpInfo op;
//						op.voxelByte0 = blk.blockType;
//						op.voxelByte1 = _chunkData[OneIndexNoPrefix(x,y,z) * B45Block.Block45Size + 1];
//						op.x = (byte)(x - 1 + (_chunkPos.x << Block45Constants._shift));
//						op.y = (byte)(y - 1 + (_chunkPos.y << Block45Constants._shift));
//						op.z = (byte)(z - 1 + (_chunkPos.z << Block45Constants._shift));
//						ret.Add(op);
//					}
//				}
//			}
//		}
		return ret;
	}
	public void getCheckSum(out long volSum, out long typeSum)
	{
		long _volSum = 0;
		long _typeSum = 0;
		
		for(int i = 0; i < _chunkData.Length; i+=B45Block.Block45Size )
		{
			_volSum += _chunkData[i];
			_typeSum += _chunkData[i+1];
		}
		volSum = _volSum;
		typeSum = _typeSum;
	}
	public int getFillRate()
	{
		int _volSum = 0;
		
		for(int i = 0; i < _chunkData.Length; i+=B45Block.Block45Size )
		{
			if(_chunkData[i] != 0)
				_volSum ++;
		}
		return _volSum;
	}
	public static int numDots_d(byte[] d_array)
	{
		int _volSum = 0;
		
		for(int i = 0; i < d_array.Length; i+=B45Block.Block45Size )
		{
			if(d_array[i] != 0)
				_volSum ++;
		}
		return _volSum;
	}
	#endregion
	public void FillHollowPblc()
	{
		FillHollow();
	}
	private void FillHollow()
	{	
		byte volume = _chunkData[0];
		byte type = _chunkData[1];
		_chunkData = s_ChunkDataPool.Get();
		_bFromPool = true;
		if(volume != 0)
		{
			for(int i = 0; i < Block45Constants.VOXEL_ARRAY_LENGTH_VT;)
			{
				_chunkData[i++] = volume;
				_chunkData[i++] = type;
			}
		}
		else
		{
			Array.Clear(_chunkData, 0, Block45Constants.VOXEL_ARRAY_LENGTH_VT);
		}
	}
	public void AttachChunkGo(B45ChunkGo vfGo, int mat_idx = 0)
	{
		if(null != _goChunk)
		{
			GameObject.Destroy(_goChunk.gameObject);
			_goChunk = null;
		}
		if(vfGo != null)
		{
			float fScale = (1<<_chunkPosLod.w) * Block45Constants._scale;
			
			//float scale = (1<<0) * Block45Constants._scale;
			//_goChunk.transform.localScale = new Vector3(scale, scale, scale);
			//_goChunk.name = "b45Chnk_" + mat_idx + "_"+GenChunkIdentifier();
			
			_goChunk = vfGo;
			_goChunk.name = "B45Chnk_m"+mat_idx + "_" +_chunkPosLod.x+"_"+_chunkPosLod.y+"_"+_chunkPosLod.z+"_"+_chunkPosLod.w;
			_goChunk.transform.localScale = new Vector3(fScale,fScale,fScale);
			_goChunk.transform.localPosition = Pos * fScale;;
			_goChunk._data = this;
		}
		
		_bNoVerts = vfGo == null;
		_buildStep = (int)EBuildStep.EBuildStep_FinGoCreating;
		if(_node != null)
		{
			if(_bNoVerts && _chunkPosLod.w > 0)	// These chunks are not supposed to be accessed.
			{
				DataVT_w = S_ChunkDataNull;
				_bFromPool = false;
			}
			_node.OnEndUpdateNodeData(null);
		}
	}

#endregion
#region read_write
	public B45Block this[IntVector3 idx]{
		get{
			return ReadVoxelAtIdx(idx.x, idx.y, idx.z);
		}
	}
	public B45Block ReadVoxelAtPos(int x, int y, int z)
	{
		return ReadVoxelAtIdx((x>>LOD)&VoxelTerrainConstants._mask,
							(y>>LOD)&VoxelTerrainConstants._mask,
							(z>>LOD)&VoxelTerrainConstants._mask);
	}
	public B45Block ReadVoxelAtIdx(int x, int y, int z)
	{
		if(_chunkData == S_ChunkDataNull)
		{
			return new B45Block(0);	// Transvoxel will get this pos
		}
		
		try{
			int index = IsHollow ? 0 : OneIndex(x,y,z);
			int indexVT = index*B45Block.Block45Size;
			return new B45Block(_chunkData[indexVT], _chunkData[indexVT+1]);
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to read voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod);
			return new B45Block(0);
		}
	}
	public bool WriteVoxelAtIdx(int x, int y, int z, B45Block voxel, bool bLodUpdate = false)
	{
		if(_chunkData == S_ChunkDataNull)
		{
			return false;
		}
		
		try{
			if(IsHollow)			FillHollow();
			
			int index = OneIndex(x,y,z);
			int indexVT = index*B45Block.Block45Size;
			_chunkData[indexVT] = voxel.blockType;
			_chunkData[indexVT+1] = voxel.materialType;
			if(_node == null)		AddToBuildList();
			else if(_node.IsInReq)	AddToBuildList();
		}
		catch(Exception ex)
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod + " : " + ex.ToString());
		}

		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(x,y,z,_chunkPosLod);
//		VFVoxelTerrain.self.lodDataUpdate.threadProc();
		
		return true;
	}
	public bool WriteVoxelAtIdx4LodUpdate(int x, int y, int z, B45Block voxel)
	{
		if(_chunkData == null)	return false;

		try{
			if(IsHollow)			FillHollow();
			
			int index = OneIndexNoPrefix(x,y,z);
			int indexVT = index*B45Block.Block45Size;
			_chunkData[indexVT] = voxel.blockType;
			_chunkData[indexVT+1] = voxel.materialType;
			if(_node == null)		AddToBuildList();
			else if(_node.IsInReq)	AddToBuildList();
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write lod voxel["+x+","+y+","+z+"] of ChunkData"+ChunkPosLod);
		}
		return true;
	}	
#endregion
	
	// static funx
	public static int OneIndexNoPrefix(int x, int y, int z)
	{
		return	(z) * Block45Constants.VOXEL_ARRAY_AXIS_SQUARED + 
				(y) * Block45Constants.VOXEL_ARRAY_AXIS_SIZE + 
				(x);
	}
	public static int OneIndex(int x, int y, int z)
	{
		return	(z + Block45Constants._numVoxelsPrefix) * Block45Constants.VOXEL_ARRAY_AXIS_SQUARED + 
				(y + Block45Constants._numVoxelsPrefix) * Block45Constants.VOXEL_ARRAY_AXIS_SIZE + 
				(x + Block45Constants._numVoxelsPrefix);
	}
}

public class B45ChunkDataHeader
{
	public IntVector3 _chunkPos;
	public int _lod;
	public int svn_key;
	public int svn_key_ba;

	public static void WriteChunkHeader(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		B45ChunkDataHeader header = obj as B45ChunkDataHeader;
		if (null != header)
		{
			stream.Write(header._chunkPos);
			stream.Write(header.svn_key);
			stream.Write(header.svn_key_ba);
		}
	}

	public static B45ChunkDataHeader ReadChunkHeader(uLink.BitStream stream, params object[] codecOptions)
	{
		B45ChunkDataHeader header = new B45ChunkDataHeader();
		stream.TryRead<IntVector3>(out header._chunkPos);
		stream.TryRead<int>(out header.svn_key);
		stream.TryRead<int>(out header.svn_key_ba);
		return header;
	}
}

public class B45ChunkDataBase{
	public IntVector3 _chunkPos;
	public int _lod;
	public int svn_key;
	public int svn_key_ba;
	public List<UpdateVector> updateVectors;
	public List<UVKeyCount> uvVersionKeys;
	public int[] UpdateDatas { get { return updateVectors.Select(iter => iter.Data).ToArray(); } }
	public byte[] _chunkData;	//tmp public

	public static void WriteChunkBase(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		B45ChunkDataBase data = obj as B45ChunkDataBase;
		if (null != data)
		{
			stream.Write(data._chunkPos);
			stream.Write(data._lod);
			stream.Write(data.svn_key);
			stream.Write(data.svn_key_ba);
			stream.Write(data._chunkData);
		}
	}

	public static B45ChunkDataBase ReadChunkBase(uLink.BitStream stream, params object[] codecOptions)
	{
		B45ChunkDataBase data = new B45ChunkDataBase(null);
		stream.TryRead<IntVector3>(out data._chunkPos);
		stream.TryRead<int>(out data._lod);
		stream.TryRead<int>(out data.svn_key);
		stream.TryRead<int>(out data.svn_key_ba);
		stream.TryRead<byte[]>(out data._chunkData);
		return data;
	}
	
	public B45ChunkDataBase(byte[] _rawData){
		_chunkData = _rawData;
	}
	
	public B45ChunkDataBase(B45Block blk){
		_chunkData = new byte[B45Block.Block45Size];
		
		_chunkData[0] = blk.blockType;
		_chunkData[1] = blk.materialType;
	}
	
	public bool IsHollow{		get{return _chunkData.Length == B45Block.Block45Size;	}	}
	
	public void InitUpdateVectors()
	{
		updateVectors = new List<UpdateVector>();
		uvVersionKeys = new List<UVKeyCount>();
		uvVersionKeys.Add(new UVKeyCount());
		svn_key = 1;
		svn_key_ba = 1;
	}

	
}