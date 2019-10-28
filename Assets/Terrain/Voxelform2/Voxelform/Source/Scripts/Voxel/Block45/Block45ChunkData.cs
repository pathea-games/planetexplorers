using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

//Block45ChunkData
public partial class Block45OctNode
{
	// All chunk data are always from the following pool
	public static GenericArrayPool<byte> s_ChunkDataPool = new GenericArrayPool<byte>(Block45Constants.VOXEL_ARRAY_LENGTH_VT);
	
	private Block45LODNodeData _nData;
	private Block45ChunkGo _goChunk;
	private byte[] _chkData;
	private int _stamp = 0;
	// Get
	private Vector3 Pos{						get{return _pos.ToVector3();										}}
	public int LOD{								get{return _pos.w;													}}
	//public IntVector4 ChunkPos{					get{return _chkPosLod;												}}
	//public IntVector4 ChunkPosLod{				get{return _chkPosLod;												}}
	//public IntVector4 ChunkPosLod_w{			set{_chkPosLod = value;												}}	
	public Block45LODNodeData NodeData{			get{return _nData;													}}
	public Block45ChunkGo ChunkGo{				get{return _goChunk;												}}
	public byte[] DataVT{						get{if(_chkData == null && VecData != null) _chkData = BlockVecs2ByteArray();
													return _chkData;												}}
	public int Signature{						get{return _pos.GetHashCode();										}}
	//public int  Signature{					get{return _chunkPosLod.GetHashCode()+(((LODOctreeMan.MaxLod+1)*_helperProc.ChunkSig)<<28);		}}
	public int GetStamp(){						return _nData == null ? _stamp : 
														(_nData.StampOfChnkUpdating<<12)|(_stamp&0xfff);			}
	public bool IsStampIdentical(int stamp){	return stamp == GetStamp();											}

	public void AttachLODNode(Block45LODNodeData nData)
	{
		_nData = nData;
		if(_nData != null)
		{
			_nData.AddOctNode(this);
		}
	}
	public void DetachLODNode()
	{
		DestroyChunkGo();
		_nData = null;
	}
	public void AttachChunkGo(Block45ChunkGo b45Go)
	{
		Block45ChunkGo oldGo = _goChunk;
		_goChunk = null;
		if(b45Go != null)
		{
			float fScale = (1<<_pos.w) * Block45Constants._scale;
			_goChunk = b45Go;
			_goChunk.name = "b45Chnk_" +_pos.x+"_"+_pos.y+"_"+_pos.z+"_"+_pos.w;
			_goChunk.transform.localScale = new Vector3(fScale,fScale,fScale);
			_goChunk.transform.localPosition = _pos.ToVector3();
			_goChunk._data = this;
            if (oldGo != null && oldGo._mc != null && oldGo._mc.sharedMesh != null)
            {
				_goChunk.OnSetCollider();
				//_goChunk._mc.sharedMesh = _goChunk._mf.sharedMesh;
			}
		}
		if(oldGo != null)
		{
			VFGoPool<Block45ChunkGo>.FreeGo(oldGo);
			oldGo = null;
		}
		if(_nData != null)	_nData.EndUpdateOctNode(this);
	}
	public void DestroyChunkGo()
	{
		if(null != (System.Object)_goChunk)	// In thread, Monobehaviour's compare can not be used
		{
			if(_nData == null)	VFGoPool<Block45ChunkGo>.FreeGo(_goChunk);		// for those cases not in thread
			else				VFGoPool<Block45ChunkGo>.ReqFreeGo(_goChunk);	// for those cases in thread use lock
			_goChunk = null;
		}
	}
	public void FreeChkData()
	{
		if(_chkData != null)
		{
			s_ChunkDataPool.Free(_chkData);
			_chkData = null;
		}
	}
	public void Clear()
	{
		DetachLODNode();
		FreeChkData();
		_pos = null;
	}
	public void WriteToByteArray(int x, int y, int z, byte b0, byte b1)
	{
		if(_nData != null && _nData.IsInReq)
		{
			if(_chkData == null)
			{
				_chkData = s_ChunkDataPool.Get();
				Array.Clear(_chkData, 0, _chkData.Length);
			}

			int idx = Block45Kernel.OneIndexNoPrefix(x, y, z);
			_chkData[idx] = b0;
			_chkData[idx + 1] = b1;
			if(_nData.LOD == 0){
				SceneChunkDependence.Instance.ValidListRemove(_nData.ChunkPosLod, EDependChunkType.ChunkBlkMask);
			}
			_nData.AddToBuildList(this);
		}
		else if(_chkData != null)
		{
			int idx = Block45Kernel.OneIndexNoPrefix(x, y, z);
			_chkData[idx] = b0;
			_chkData[idx + 1] = b1;
		}
	}

}

