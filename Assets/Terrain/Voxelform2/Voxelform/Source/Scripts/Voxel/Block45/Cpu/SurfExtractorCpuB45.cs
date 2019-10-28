#define B45_THREADING

using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class SurfExtractReqB45 : IVxSurfExtractReq
{
	// return data
	//B45chunk has multi mat, spliting mesh would be much complicated. So here MeshSplitThreshold would not be applied, only one mesh.
	public Vector3[]   verts;
	public Vector2[]   uvs;
	public int matCnt = 0;
	public int[] materialMap;
	public List<int[]> subMeshIndices = new List<int[]>();

	private int _nVertsPerMesh;	// must be times of 3
	public  int _chunkStamp;
	public  Block45OctNode _chunkData;
	public  Action<SurfExtractReqB45> _finHandler;
	
	public byte[] VolumeData{		get{ return _chunkData.DataVT;	}	}
	public bool IsInvalid{			get{ byte[] vData = VolumeData;
										 return !_chunkData.IsStampIdentical(_chunkStamp) || 
										 		vData == null || 
												vData.Length != Block45Constants.VOXEL_ARRAY_LENGTH_VT;	}	}
	public int  Signature{			get{ return _chunkData.Signature;	}	}
	public int  Priority{			get{ return _chunkData.LOD;	}	}	// for sort
	public int  MeshSplitThreshold{	get{ return _nVertsPerMesh;	}	}
	public int  FillMesh(Mesh mesh)
	{
		if(mesh == null)	return (verts != null && verts.Length > 0) ? 1 : 0;
		
		mesh.name 	  	  = "b45_mesh";
		mesh.vertices 	  = verts;
		mesh.uv  	  	  = uvs;
		mesh.subMeshCount = matCnt;
		for(int i = 0; i < matCnt; i++)
		{
			mesh.SetTriangles(subMeshIndices[i], i);
		}
		
		mesh.RecalculateNormals();
		//MonoBehaviour.print("rebuild mesh took " + (Environment.TickCount - before) + " ms.");
		Block45Kernel.TangentSolver(mesh);	
		return 0;		
	}
	public bool OnReqFinished()
	{
		bool ret = false;
		if (!IsInvalid && _finHandler != null) {
			_finHandler(this);
			ret = true;
		}
		Free (this);
		return ret;
	}

	static GenericPool<SurfExtractReqB45> s_reqPool = new GenericPool<SurfExtractReqB45>();
	public static SurfExtractReqB45 Get(int chunkStamp, Block45OctNode chunkData, Action<SurfExtractReqB45> finHandler, int nVertsPerMesh = SurfExtractorsMan.c_vertsCntMax)
	{
		//SurfExtractReqB45 req = new SurfExtractReqB45 ();
		SurfExtractReqB45 req = s_reqPool.Get ();
		req._chunkStamp		= chunkStamp;
		req._chunkData		= chunkData;
		req._nVertsPerMesh	= nVertsPerMesh;
		req._finHandler		= finHandler;
		return req;
	}
	public static void Free(SurfExtractReqB45 req)
	{
		req.verts = null;
		req.uvs = null;
		req.materialMap = null;
		req.subMeshIndices.Clear ();
		s_reqPool.Free (req);
	}
} 

public class SurfExtractorCpuB45 : IVxSurfExtractor
{
	Block45Kernel swBlock45 = null;
	byte[] _chunkDataBuffer;
	byte[] _chunkDataToProceed = null;
	bool _bThreadOn;
	Thread _threadB45 = null;
	
	Dictionary<int,IVxSurfExtractReq> _reqList;
	List<IVxSurfExtractReq> _reqFinishedList;
	IVxSurfExtractReq reqToProceed;
	IVxSurfExtractReq PickReqToProceed(){
		if(_reqList.Count == 0)			return null;
		
		// Remove those out of date.
		foreach(var entry in _reqList.Where(kvp => kvp.Value.IsInvalid).ToList())
		{
			_reqList.Remove(entry.Key);
		}

		// TODO : mod code for b45
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

	public bool IsIdle{		get{	return _reqList.Count == 0 && _chunkDataToProceed == null;	}	}
	public bool IsAllClear{	get{	return _reqList.Count == 0 && _chunkDataToProceed == null && _reqFinishedList.Count == 0;	}	}
	public bool Pause{		get;set;}

	public void Init()
	{
		//MCOutputData.InitConstData();
		swBlock45 = new Block45Kernel();
		_chunkDataBuffer = new byte[Block45Constants.VOXEL_ARRAY_LENGTH_VT];
		_chunkDataToProceed = null;
		_reqList = new Dictionary<int,IVxSurfExtractReq>(); 
		_reqFinishedList = new List<IVxSurfExtractReq>();
		Pause = false;
		
		#if B45_THREADING
		_threadB45 = new Thread(new ThreadStart(ThreadExec));
		_threadB45.Start();
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
					Debug.LogError("[SurfB45_CPU]Error in thread:"+System.Environment.NewLine+e.ToString());
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

		swBlock45.setInputChunkData(_chunkDataToProceed);
		swBlock45.Rebuild();
	
		SurfExtractReqB45 req = reqToProceed as SurfExtractReqB45;
		//usedMaterialIndicesList.Add(swBlock45.getMaterialMap());

		int chunkTotalVerts = swBlock45.verts.Count;
		if(chunkTotalVerts > 0)
		{
			int chunkVertsCntThreshold = reqToProceed.MeshSplitThreshold;
			if(chunkTotalVerts <= chunkVertsCntThreshold)
			{
				req.verts  		= swBlock45.verts.ToArray();
				req.uvs	   		= swBlock45.uvs.ToArray();
				req.matCnt 		= swBlock45.matCnt;
				req.materialMap = swBlock45.materialMap;
				foreach(List<int> indice in swBlock45.subMeshIndices)
				{
					req.subMeshIndices.Add(indice.ToArray());
				}
			}
			else
			{
				Debug.LogError("[SurfB45_CPU]:Out Of chunkVertsCntThreshold.");
			}
		}
		
		lock(_reqFinishedList)
			_reqFinishedList.Add(reqToProceed);
		_chunkDataToProceed = null;
	}
	//private bool bNeedUpdateTrans = false;
	public int  OnFin()
	{
		#if !B45_THREADING
		if(_reqList.Count > 0 && !Pause)
		{
			Exec();
		}
		#endif	

		if(Monitor.TryEnter(_reqFinishedList))
		{
			for (int i = 0; i < _reqFinishedList.Count; i++) {
				IVxSurfExtractReq req = _reqFinishedList [i];
				req.OnReqFinished ();
			}
			_reqFinishedList.Clear();
			Monitor.Exit(_reqFinishedList);
		}
		
		return _reqFinishedList.Count;
	}
	public void Reset(){
		lock(_reqList){
			_reqList.Clear();
		}
		Pause = false;
		#if B45_THREADING
		if(!IsIdle)
		{
			_bThreadOn = false;
			_threadB45.Join();
			_threadB45 = new Thread(new ThreadStart(ThreadExec));
			_threadB45.Start();
			Debug.Log("[SurfB45_CPU]Thread Reset");
		}
		#endif	
		_reqFinishedList.Clear();
	}
	public void CleanUp()
	{
		lock(_reqList){
			_reqList.Clear();
		}
		#if B45_THREADING
		_bThreadOn = false;
		try{
			_threadB45.Join();
			Debug.Log("[SurfB45_CPU]Thread stopped");
		}
		catch{
			MonoBehaviour.print("[SurfB45_CPU]Thread stopped with exception");
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
