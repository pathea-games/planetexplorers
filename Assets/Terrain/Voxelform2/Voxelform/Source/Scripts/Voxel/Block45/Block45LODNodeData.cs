using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

#if true
public class Block45LODNodeData : ILODNodeData
{
	public delegate void ProcOnWriteVoxel(LODOctreeNode node, byte oldVol, byte newVol, int idxVol);
	
	public const int BuildStep_NotInBuild 		= 0;
	public const int BuildStep_StartDataLoading = 1;	// Now, Invalid chunks will be stuck at this step
	public const int BuildStep_FinDataLoading 	= 2;
	public const int BuildStep_StartGoCreating  = 3;
	public const int BuildStepMask				= 0xf;
	public const int StampMask					= ~BuildStepMask;

	private const int Block45OctNodeStatus_Idle 		= 0;
	private const int Block45OctNodeStatus_InBuild 		= 1;

	// members
	private IVxChunkHelperProc _helperProc;
	private LODOctreeNode _node;
	private IntVector4 _chunkPosLod = new IntVector4(0,0,0,-1);
	private int  _stampOfUpdating = 0;
	// _lstBlock45DatasStatus' meaning is status of Block45OctNode;
	private List<Block45OctNode> _lstBlock45Datas;
	private List<int> _lstBlock45DatasStatus;
	// Get
	private Vector3 Pos{			get{return new Vector3(	
			                                   _chunkPosLod.x<<VoxelTerrainConstants._shift,
			                                   _chunkPosLod.y<<VoxelTerrainConstants._shift,
			                                   _chunkPosLod.z<<VoxelTerrainConstants._shift);					}}
	public int LOD{					get{return _chunkPosLod.w;													}}
	public IntVector4 ChunkPosLod{	get{return _chunkPosLod;													}}
	// When chunk update too fast, a chunk may change its position with the same lod level because delay
	// So, it is not enough just to check lod level.
	public int  StampOfChnkUpdating{get{return _stampOfUpdating;												}}
	public bool IsEmpty{ 			get{return IsAllOctNodeEmpty();												}} // ILodNodeData
	public bool IsIdle{				get{return BuildStep == BuildStep_NotInBuild;								}} // ILodNodeData
	public bool IsReady{			get{return BuildStep == BuildStep_NotInBuild;								}} // ILodNodeData TODO : tmp code
	public bool IsInReq{			get{return _node.IsInReq;													}}
	//public int  Signature{			get{return _chunkPosLod.GetHashCode()+(((LODOctreeMan.MaxLod+1)*_helperProc.ChunkSig)<<28);		}}
	//public int  Sig{				get{return _helperProc.ChunkSig;											}}
	// Set
	public int  BuildStep{						get{return _stampOfUpdating&BuildStepMask;} 
												set{_stampOfUpdating=(_stampOfUpdating&StampMask)|	value;		}}
	public IVxChunkHelperProc HelperProc{		set{_helperProc = value;										}}	// Used in old random map code, no need to use lock
	public IntVector4 ChunkPosLod_w{			set{_chunkPosLod = value;										}}	

	// Methods
	public Block45LODNodeData(LODOctreeNode node)
	{
		_node = node;
	}
	/*
	~Block45LODNodeData()
	{
		// Clear Mem
		if(_lstBlock45Datas != null)
		{
			int n = _lstBlock45Datas.Count;
			for(int i = 0; i < n ; i++)
			{
				_lstBlock45Datas[i].DetachLODNode();
			}
		}
		_lstBlock45Datas = null;
		_lstBlock45DatasStatus = null;
	}
	*/
	public bool IsNodePosChange(){		return _node.CX != _chunkPosLod.x || _node.CY != _chunkPosLod.y || _node.CZ != _chunkPosLod.z || _node.Lod != _chunkPosLod.w;	}
	bool IsAllOctNodeEmpty()
	{
		if(_lstBlock45Datas == null)	return true;

		int n = _lstBlock45Datas.Count;
		for(int i = 0; i < n; i++)
		{
			if(null != (System.Object)_lstBlock45Datas[i].ChunkGo)						return false;
		}
		return true;
	}
	public bool IsAllOctNodeReady()
	{
		if(!IsIdle)						return false;
		if(_lstBlock45Datas == null)	return true;

		int n = _lstBlock45Datas.Count;
		for(int i = 0; i < n; i++)
		{
			Block45ChunkGo chunkGo = _lstBlock45Datas[i].ChunkGo;
			if(null != (System.Object)chunkGo && null == (System.Object)chunkGo._mc.sharedMesh)	return false;
		}
		return true;
	}
	public Block45OctNode PickNodeToSetCol()
	{
		if(!IsIdle)						return null;
		if(_lstBlock45Datas == null)	return null;
		
		int n = _lstBlock45Datas.Count;
		for(int i = 0; i < n; i++)
		{
			Block45ChunkGo chunkGo = _lstBlock45Datas[i].ChunkGo;
			if(null != (System.Object)chunkGo && null == (System.Object)chunkGo._mc.sharedMesh)	return _lstBlock45Datas[i];
		}
		return null;
	}
	public void AddOctNode(Block45OctNode octNode)
	{
		if(_lstBlock45Datas == null)
		{
			_lstBlock45Datas = new List<Block45OctNode>();
			_lstBlock45DatasStatus = new List<int>();
		}
		_lstBlock45Datas.Add(octNode);
		_lstBlock45DatasStatus.Add(Block45OctNodeStatus_Idle);
		if(IsInReq && octNode.VecData != null)
		{
			BuildStep = BuildStep_StartGoCreating;
			_lstBlock45DatasStatus[_lstBlock45DatasStatus.Count - 1] = Block45OctNodeStatus_InBuild;
			_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(octNode.GetStamp(), octNode, _helperProc.ChunkProcPostGenMesh, SurfExtractorsMan.c_vertsCntMax));
		}
	}
	public void SetBlock45Datas(List<Block45OctNode> lstDatas)
	{
		if(_lstBlock45Datas != null)
		{
			int n = _lstBlock45Datas.Count;
			for(int i = 0; i < n ; i++)
			{
				_lstBlock45Datas[i].DetachLODNode();
			}
		}
		_lstBlock45Datas = null;
		_lstBlock45DatasStatus = null;
		if(lstDatas != null && lstDatas.Count > 0)
		{
			int n = lstDatas.Count;
			for(int i = 0; i < n; i++)
			{
				lstDatas[i].AttachLODNode(this);
			}
		}
		if(BuildStep != BuildStep_StartGoCreating)
		{
			BuildStep = BuildStep_NotInBuild;
			EndUpdateNodeData();
		}
	}
	private void AddToBuildList()	// Can be called from the main thread
	{
		if(_lstBlock45Datas != null && _lstBlock45Datas.Count > 0)
		{
			int n = _lstBlock45Datas.Count;
			for(int i = 0; i < n; i++)
			{
				if(_lstBlock45Datas[i].VecData != null)
				{
					BuildStep = BuildStep_StartGoCreating;
					_lstBlock45DatasStatus[i] = Block45OctNodeStatus_InBuild;
					_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(_lstBlock45Datas[i].GetStamp(), _lstBlock45Datas[i], _helperProc.ChunkProcPostGenMesh, SurfExtractorsMan.c_vertsCntMax));
				}
			}
		}
		
		if(BuildStep != BuildStep_StartGoCreating)
		{
			BuildStep = BuildStep_NotInBuild;
			EndUpdateNodeData();
		}
	}
	public void AddToBuildList(Block45OctNode octNode)	// need test
	{
		if(_lstBlock45Datas != null)
		{
			int n = _lstBlock45Datas.Count;
			for(int i = 0; i < n; i++)
			{
				if(_lstBlock45Datas[i] == octNode)
				{
					_lstBlock45DatasStatus[i] = Block45OctNodeStatus_InBuild;
					BuildStep = BuildStep_StartGoCreating;
					break;
				}
			}
		}
		_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(octNode.GetStamp(), octNode, _helperProc.ChunkProcPostGenMesh, SurfExtractorsMan.c_vertsCntMax));
	}
	public void EndUpdateOctNode(Block45OctNode octNode)
	{
		if(_lstBlock45Datas != null)
		{
			int n = _lstBlock45Datas.Count;
			for(int i = 0; i < n; i++)
			{
				if(_lstBlock45Datas[i] == octNode)
				{
					_lstBlock45DatasStatus[i] = Block45OctNodeStatus_Idle;
					break;
				}
			}
			for(int i = 0; i < n; i++)
			{
				if(_lstBlock45DatasStatus[i] != Block45OctNodeStatus_Idle){	
					return;
				}
			}
		}
		BuildStep = BuildStep_NotInBuild;
		EndUpdateNodeData();
	}

	#region ILODNodeData
	public void UpdateTimeStamp(){				_stampOfUpdating+=0x10;	}
	public void BegUpdateNodeData()
	{
		//NOTE: If this chunkData is already in dataReading or in rebuildList or in computingList, out-of-date chunk mesh will be attached.
		//Note: ChunkPosLod is a new IntVector4, so it is independent, and ensure _chunkPosLod will be modified in this method.
		_helperProc.OnBegUpdateNodeData (this);

		int cx = _node.CX, cy = _node.CY, cz = _node.CZ, lod = _node.Lod;
		if(_chunkPosLod.x != cx || _chunkPosLod.y != cy || _chunkPosLod.z != cz || _chunkPosLod.w != lod)
		{
			_chunkPosLod.x = cx; 				
			_chunkPosLod.y = cy; 				
			_chunkPosLod.z = cz; 				
			_chunkPosLod.w = lod; 
			BuildStep = BuildStep_StartDataLoading;
		}

		if(BuildStep == BuildStep_NotInBuild)
		{
			//if(_lstBlock45Datas != null)	Debug.LogWarning("[Block45LODNode]:BuildStep_NotInBuild_" + _lstBlock45Datas.Count + "NULL @" + _chunkPosLod);
			if(_lstBlock45Datas == null || _lstBlock45Datas.Count == 0)			//need update data
			{
				EndUpdateNodeData();
				return;
			}
			else
			{
				BuildStep = BuildStep_FinDataLoading;
			}
		}
		if(BuildStep == BuildStep_StartDataLoading)
		{
			_helperProc.ChunkProcPreLoadData(this);	// Load data from bvRoot
			BuildStep = BuildStep_FinDataLoading;
		}
		if(BuildStep >= BuildStep_FinDataLoading) // If req from 1->2->1, Buildstep will keep 3, and we should add it to buildList
		{
			AddToBuildList();
		}
	}
	public void EndUpdateNodeData()
	{
		_helperProc.OnEndUpdateNodeData(this);
		_node.OnEndUpdateNodeData(this);
	}
	public void OnDestroyNodeData()	// destroy all chunk gos
	{
        try
        {
            _helperProc.OnDestroyNodeData(this);
            if (_lstBlock45Datas != null)
            {
                int n = _lstBlock45Datas.Count;
                for (int i = 0; i < n; i++)
                {
                    _lstBlock45Datas[i].DestroyChunkGo();
                    _lstBlock45DatasStatus[i] = Block45OctNodeStatus_Idle;
                }
            }
            BuildStep = BuildStep_NotInBuild;
        }
        catch (Exception e)
        {
            Debug.Log("Block45LODNodeData.OnDestroyNodeData:"+e.Message);
        }
	}
#endregion

}
#endif
