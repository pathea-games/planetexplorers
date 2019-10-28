#define MC_THREADING

using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class SurfExtractorCpuMC : IVxSurfExtractor
{
	MarchingCubesSW swMarchingCubes;
	byte[] _chunkDataBuffer;
	byte[] _chunkDataToProceed = null;
	bool _bThreadOn;
	Thread _threadMC = null;
	
	Dictionary<int,IVxSurfExtractReq> _reqList;
	List<IVxSurfExtractReq> _reqFinishedList;
	IVxSurfExtractReq reqToProceed;
	IVxSurfExtractReq PickReqToProceed(){
		if(_reqList.Count == 0)			return null;
		
		// Remove those out of date.
		foreach(var entry in _reqList.Where(kvp => kvp.Value.IsInvalid).ToList())
		{
			//Debug.Log("RemoveChunkInReq"+((SurfExtractReqMC)entry.Value)._chunkData.ChunkPosLod+":"
			//          +((SurfExtractReqMC)entry.Value)._chunkStamp+"|"
			//          +((SurfExtractReqMC)entry.Value)._chunkData.StampOfChnkUpdating);
		    _reqList.Remove(entry.Key);
		}
		
		// Add to process
		for(int curlod = 0; curlod <= LODOctreeMan.MaxLod; curlod++)
		{
			foreach(KeyValuePair<int, IVxSurfExtractReq> pair in _reqList)
			{
				if(pair.Value.Priority == curlod)
				{
					IVxSurfExtractReq req = pair.Value;
					Array.Copy(req.VolumeData, _chunkDataBuffer, _chunkDataBuffer.Length);	
					_chunkDataToProceed = _chunkDataBuffer;
					_reqList.Remove(pair.Key);
					return req;
				}
			}
		}
		return null;
	}
	
	// interfaces
	public bool IsIdle{		get{	return _reqList.Count == 0 && _chunkDataToProceed == null;	}	}
	public bool IsAllClear{	get{	return _reqList.Count == 0 && _chunkDataToProceed == null && _reqFinishedList.Count == 0;	}	}
	public bool Pause{		get;set;}
	
	public void Init(){
		swMarchingCubes = new MarchingCubesSW();
		_chunkDataBuffer = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
		_chunkDataToProceed = null;
		_reqList = new Dictionary<int,IVxSurfExtractReq>(); 
		_reqFinishedList = new List<IVxSurfExtractReq>();
		Pause = false;

#if MC_THREADING
		_threadMC = new Thread(new ThreadStart(ThreadExec));
		_threadMC.Start();
#endif	
	}
	private void ThreadExec()
	{
		_bThreadOn = true;
		while(_bThreadOn)
		{
			if(_reqList.Count > 0 && !Pause)
			{
				try{Exec();	}
				catch(Exception e){
					Debug.LogError("[SurfMC_CPU]Error in thread:"+System.Environment.NewLine+e.ToString());
					Recover();
				}
			}
			Thread.Sleep(1);
		}
	}	
	private void Recover()
	{
		//if(reqToProceed != null)	AddSurfExtractReq(reqToProceed);
		reqToProceed = null;
	}
	public void Exec(){
		reqToProceed = null;
		lock(_reqList)
			reqToProceed = PickReqToProceed();
		if(reqToProceed == null)			return;
		
		swMarchingCubes.setInputChunkData(_chunkDataToProceed);
		swMarchingCubes.Rebuild();
		
		SurfExtractReqMC req = reqToProceed as SurfExtractReqMC;
		int chunkTotalVerts = swMarchingCubes.VertexList.Count;
		if(chunkTotalVerts > 0)
		{
			req.FillOutData(swMarchingCubes.VertexList, swMarchingCubes.Norm01List, swMarchingCubes.Norm2tList, 0, chunkTotalVerts);
		}
		
		lock(_reqFinishedList)
			_reqFinishedList.Add(reqToProceed);
		_chunkDataToProceed = null;
	}
	private bool bNeedUpdateTrans = false;
	public int  OnFin()
	{
#if !MC_THREADING
		if(_reqList.Count > 0 && !Pause)
		{
			Exec();
		}
#endif	
		if(_reqFinishedList.Count == 0){
			if(bNeedUpdateTrans)
			{
				if(VFVoxelTerrain.self != null && VFVoxelTerrain.self.TransGoCreator.IsTransvoxelEnabled)
				{
					if(VFVoxelTerrain.self.TransGoCreator.PrepChunkList())
					{
						bNeedUpdateTrans = false;
					}
				}
			}
			return 0;
		}

		if(Monitor.TryEnter(_reqFinishedList))
		{
			int idx = 0;
			int nReq = _reqFinishedList.Count;
			for(;idx < nReq; idx++){
				_reqFinishedList[idx].OnReqFinished();
			}
			_reqFinishedList.Clear();
			Monitor.Exit(_reqFinishedList);
		}

		bNeedUpdateTrans = true;
		return _reqFinishedList.Count;
	}
	public void Reset(){
		lock(_reqList){
			_reqList.Clear();
		}
		Pause = false;
#if MC_THREADING
		if(!IsIdle)
		{
			_bThreadOn = false;
			_threadMC.Join();
			_threadMC = new Thread(new ThreadStart(ThreadExec));
			_threadMC.Start();
			Debug.Log("[SurfMC_CPU]Thread Reset");
		}
#endif	
		_reqFinishedList.Clear();
	}
	public void CleanUp()
	{
		lock(_reqList){
			_reqList.Clear();
		}
#if MC_THREADING
		_bThreadOn = false;
		try{
			_threadMC.Join();
			Debug.Log("[SurfMC_CPU]Thread stopped");
		}
		catch{
			MonoBehaviour.print("[SurfMC_CPU]Thread stopped with exception");
		}
#endif
		_reqFinishedList.Clear();
	}
	public bool AddSurfExtractReq(IVxSurfExtractReq req){
		lock(_reqList){
			_reqList[req.Signature] = req;
		}
		
		return true;
	}
}
