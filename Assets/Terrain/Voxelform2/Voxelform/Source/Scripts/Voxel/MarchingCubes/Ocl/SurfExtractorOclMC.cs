//#define MC_DATA_TEST
#define MC_THREADING

using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class MCOutputData
{	
	static GenericPool<MCOutputData> s_dataPool = new GenericPool<MCOutputData> (672);	// 657/514

	Vector3[] _pos;
	Vector2[] _norm01;
	Vector2[] _norm2t;
	int[]     _indice;

	public MCOutputData()	// for pool
	{
		_pos    = new Vector3[SurfExtractorsMan.c_vertsCnt4Pool];
		_norm01 = new Vector2[SurfExtractorsMan.c_vertsCnt4Pool];
		_norm2t = new Vector2[SurfExtractorsMan.c_vertsCnt4Pool];
		_indice = null;
	}
	public MCOutputData(Vector3[] verts, Vector2[] norm01, Vector2[] norm2t, int[] indice)
	{
		_pos    = verts;
		_norm01 = norm01;
		_norm2t = norm2t;
		_indice = indice;
	}
	public void Clone(MCOutputData data)
	{
		_pos 	= data._pos;   
		_norm01 = data._norm01;
		_norm2t = data._norm2t;
		_indice = data._indice;
	}
	public void SetToMesh(Mesh mesh)
	{
		mesh.MarkDynamic ();
		mesh.vertices = _pos;
		mesh.uv  	  = _norm01;
		mesh.uv2 	  = _norm2t;
		mesh.triangles= _indice;
	}

	public static MCOutputData Get(Vector3[] srcPosArray, Vector2[] srcNorm01Array, Vector2[] srcNorm2tArray, int srcIdx, int len)
	{
		MCOutputData ret;
		if (len > SurfExtractorsMan.c_vertsCnt4Pool) {
			Vector3[] pos = new Vector3[len];
			Vector2[] norm01 = new Vector2[len];
			Vector2[] norm2t = new Vector2[len];
			int[] indice = new int[len];
			Array.Copy (srcPosArray, srcIdx, pos, 0, len);
			Array.Copy (srcNorm01Array, srcIdx, norm01, 0, len);
			Array.Copy (srcNorm2tArray, srcIdx, norm2t, 0, len);
			Array.Copy (SurfExtractorsMan.s_indiceMax, indice, len);
			ret = new MCOutputData (pos, norm01, norm2t, indice);
			return ret;
		} else {
			ret = s_dataPool.Get();
			Array.Copy (srcPosArray, srcIdx, ret._pos, 0, len);
			Array.Copy (srcNorm01Array, srcIdx, ret._norm01, 0, len);
			Array.Copy (srcNorm2tArray, srcIdx, ret._norm2t, 0, len);
			if(len == SurfExtractorsMan.c_vertsCnt4Pool){
				ret._indice = SurfExtractorsMan.s_indice4Pool;
			} else if(ret._indice == null || len != ret._indice.Length){
				ret._indice = new int[len];
				Array.Copy (SurfExtractorsMan.s_indiceMax, ret._indice, len);
			}
		}
		return ret;
	}
	public static MCOutputData Get(List<Vector3> srcPosLst, List<Vector2> srcNorm01Lst, List<Vector2> srcNorm2tLst, int srcIdx, int len)
	{
		MCOutputData ret;
		if (len > SurfExtractorsMan.c_vertsCnt4Pool) {
			Vector3[] pos = new Vector3[len];
			Vector2[] norm01 = new Vector2[len];
			Vector2[] norm2t = new Vector2[len];
			int[] indice = new int[len];
			srcPosLst.CopyTo(srcIdx, pos, 0, len);
			srcNorm01Lst.CopyTo(srcIdx, norm01, 0, len);
			srcNorm2tLst.CopyTo(srcIdx, norm2t, 0, len);
			Array.Copy (SurfExtractorsMan.s_indiceMax, indice, len);
			ret = new MCOutputData (pos, norm01, norm2t, indice);
			return ret;
		} else {
			ret = s_dataPool.Get();
			srcPosLst.CopyTo(srcIdx, ret._pos, 0, len);
			srcNorm01Lst.CopyTo(srcIdx, ret._norm01, 0, len);
			srcNorm2tLst.CopyTo(srcIdx, ret._norm2t, 0, len);
			if(len == SurfExtractorsMan.c_vertsCnt4Pool){
				ret._indice = SurfExtractorsMan.s_indice4Pool;
			} else if(ret._indice == null || len != ret._indice.Length){
				ret._indice = new int[len];
				Array.Copy (SurfExtractorsMan.s_indiceMax, ret._indice, len);
			}
		}
		return ret;
	}
	public static void Free(MCOutputData data)
	{
		if (data._pos.Length == SurfExtractorsMan.c_vertsCnt4Pool) {
			s_dataPool.Free (data);
			data._indice = null;
		} else {
			data._pos = null;
			data._norm01 = null;
			data._norm2t = null;
			data._indice = null;
		}
	}
}

public class SurfExtractReqMC : IVxSurfExtractReq
{
	// return data
	List<MCOutputData> _outDatas = new List<MCOutputData>(4);

	public	int _chunkSig;
	public  int _chunkStamp;
	public	byte[] _chunkData;
	public  VFVoxelChunkData _chunk;
	int _nVertsPerMesh;	// must be times of 3
	int _validChunkDataLen;
	Action<SurfExtractReqMC> _customFinHandler;
	
	public byte[] VolumeData{		get{ return _chunkData;		}	}
	public bool IsInvalid{			get{ return _customFinHandler != null ? 
												(!_chunk.IsMatch(_chunkSig, _chunkStamp)) :
												(!_chunk.IsMatch(_chunkSig, _chunkStamp) || _chunkData == null || _chunkData.Length != VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);	}	}
	public int  Signature{			get{ return _chunkSig;		}	}
	public int  Priority{			get{ return _chunk.LOD;		}	}	// for sort
	public int  MeshSplitThreshold{	get{ return _nVertsPerMesh;	}	}
	public int  FillMesh(Mesh mesh)
	{
		int n = _outDatas.Count;
		if(mesh == null)	return n;
		
		--n;
		//mesh.name 	  = "ocl_mesh";
		mesh.triangles= null;
		
		_outDatas [n].SetToMesh (mesh);
		//mesh.SetIndices(splitMeshesData[n].indice,MeshTopology.Points,0); // point sprites
		//mesh.Optimize(); --- this will down 7~8fps
		MCOutputData.Free(_outDatas[n]);
		_outDatas.RemoveAt(n);
		return n;		
	}
	public void AddOutData(MCOutputData data){		_outDatas.Add (data);	}
	public void FillOutData(Vector3[] srcPosArray, Vector2[] srcNorm01Array, Vector2[] srcNorm2tArray, int srcIdx, int len)
	{
		while(len > 0)
		{
			int chunkVertsCurCnt = len > _nVertsPerMesh ? _nVertsPerMesh : len;					
			MCOutputData data = MCOutputData.Get (srcPosArray, srcNorm01Array, srcNorm2tArray, srcIdx, chunkVertsCurCnt);
			_outDatas.Add (data);

			srcIdx += chunkVertsCurCnt;
			len  -= chunkVertsCurCnt;
		}
	}
	public void FillOutData(List<Vector3> srcPosLst, List<Vector2> srcNorm01Lst, List<Vector2> srcNorm2tLst, int srcIdx, int len)
	{
		while(len > 0)
		{
			int chunkVertsCurCnt = len > _nVertsPerMesh ? _nVertsPerMesh : len;					
			MCOutputData data = MCOutputData.Get (srcPosLst, srcNorm01Lst, srcNorm2tLst, srcIdx, chunkVertsCurCnt);
			_outDatas.Add (data);
			
			srcIdx += chunkVertsCurCnt;
			len  -= chunkVertsCurCnt;
		}
	}
	public bool OnReqFinished()
	{
		bool ret = false;
		if (!IsInvalid) {
			if(_customFinHandler != null){
				_customFinHandler(this);
			} else if(_chunk.HelperProc != null){
				_chunk.HelperProc.ChunkProcPostGenMesh(this);
			}
			ret = true;
		}
		Free (this);
		return ret;
	}

	static GenericPool<SurfExtractReqMC> s_reqPool = new GenericPool<SurfExtractReqMC>(768);
	public static SurfExtractReqMC Get(VFVoxelChunkData chunk, Action<SurfExtractReqMC> finHandler)
	{
		SurfExtractReqMC req	= Get (chunk);
		req._customFinHandler	= finHandler;
		return req;
	}
	public static SurfExtractReqMC Get(VFVoxelChunkData chunk, int nVertsPerMesh = SurfExtractorsMan.c_vertsCnt4Pool)
	{
		//SurfExtractReqMC req = new SurfExtractReqMC ();
		SurfExtractReqMC req	= s_reqPool.Get ();
		req._chunk				= chunk;
		req._chunkData			= chunk.DataVT;
		req._chunkSig			= chunk.SigOfChnk;
		req._chunkStamp			= chunk.StampOfUpdating;
		req._customFinHandler	= null;
		req._nVertsPerMesh		= nVertsPerMesh;
		return req;
	}
	public static void Free(SurfExtractReqMC req)
	{
		int n = req._outDatas.Count;
		if (n > 0) {
			// If mesh data not been filled 
			for (int i = 0; i < n; i++) 	
				MCOutputData.Free (req._outDatas [i]);
			req._outDatas.Clear ();
		}
		s_reqPool.Free (req);
	}
}

public class SurfExtractorOclMC : IVxSurfExtractor
{
	Dictionary<int,IVxSurfExtractReq> _reqList;
	List<IVxSurfExtractReq> _reqInProcessList;
	List<IVxSurfExtractReq> _reqFinishedList;
	List<KeyValuePair<int,IVxSurfExtractReq>> _tmpReqList;
	Thread _threadMC;
	bool _bThreadOn;
	int _errCnt;
	void PickReqToProceed(){
		if(_reqList.Count == 0)			return;

		// Remove those out of date.
		foreach(var entry in _reqList.Where(kvp => kvp.Value.IsInvalid).ToList())
		{
			//SurfExtractReqMC r = (SurfExtractReqMC)entry.Value;
			//Debug.LogError("[RemoveChunkInReq]"+r._chunk.ChunkPosLod+":"
			//          							+r._chunkSig+"|"+r._chunk.SigOfChnk+";"
			//									+r._chunkStamp+"|"+r._chunk.StampOfUpdating);
		    _reqList.Remove(entry.Key);
		}

		// Add to processList
		_tmpReqList.Clear();
		for(int curlod = 0; curlod <= LODOctreeMan.MaxLod && _tmpReqList.Count < oclMarchingCube.MAX_CHUNKS-1; curlod++)
		{
			_tmpReqList.AddRange(_reqList.Where (kvp => kvp.Value.Priority == curlod));
		}
		int n = 0, nMax = _tmpReqList.Count;
		while(oclMarchingCube.numChunks < oclMarchingCube.MAX_CHUNKS-1 && n < nMax)
		{
			IVxSurfExtractReq req = _tmpReqList[n].Value;
			_reqInProcessList.Add(req);
			oclMarchingCube.AddChunkVolumeData(req.VolumeData);
			_reqList.Remove(_tmpReqList[n].Key);
			n++;
		}
		return;
	}
	
	
	// interfaces
	public bool IsIdle{		get{	return _reqList.Count == 0 && _reqInProcessList.Count == 0;	}	}
	public bool IsAllClear{	get{	return _reqList.Count == 0 && _reqInProcessList.Count == 0 && _reqFinishedList.Count == 0;	}	}
	public bool Pause{		get;set;}
	
	public void Init(){
		_reqList = new Dictionary<int,IVxSurfExtractReq>(); 
		_reqInProcessList = new List<IVxSurfExtractReq>(oclMarchingCube.MAX_CHUNKS);
		_reqFinishedList = new List<IVxSurfExtractReq>();
		_tmpReqList = new List<KeyValuePair<int, IVxSurfExtractReq>>();
		Pause = false;

#if MC_DATA_TEST
		NV_Init(1);
#endif		
		
		_errCnt = 0;
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
				try{Exec();}
				catch(Exception e){
					Debug.LogError("[SurfMC_OCL]Error in thread:"+System.Environment.NewLine+e.ToString());
					_errCnt++;
					break;
				}
			}
			Thread.Sleep(1);
		}
	}
	public void Exec(){
		lock(_reqList)
			PickReqToProceed();
		
		int nNumChunks = oclMarchingCube.numChunks;
		if(nNumChunks == 0)	return;

		IEnumerator e = oclMarchingCube.computeIsosurfaceAsyn();
		while(e.MoveNext());

		Vector3[] posArray = oclMarchingCube.hPosArray.Target as Vector3[];
		Vector2[] srcNorm01Array = oclMarchingCube.hNorm01Array.Target as Vector2[];
		Vector2[] srcNorm2tArray = oclMarchingCube.hNorm2tArray.Target as Vector2[];
		for(int i = 0; i < nNumChunks; i++)
		{
			SurfExtractReqMC req = _reqInProcessList[i] as SurfExtractReqMC;
			int prevChunkVerts = oclScanLaucher.ofsData[i];
			int chunkTotalVerts = i==nNumChunks-1 ? (oclScanLaucher.sumData[0]-prevChunkVerts) : (oclScanLaucher.ofsData[i+1]-prevChunkVerts);
			if(chunkTotalVerts > 0)
			{
				req.FillOutData(posArray, srcNorm01Array, srcNorm2tArray, prevChunkVerts, chunkTotalVerts);
			}
		}

		lock(_reqFinishedList)
			_reqFinishedList.AddRange(_reqInProcessList);
		
		_reqInProcessList.Clear();
	}
	const int MaxReqsToProceedOnce = 12;
	private bool bNeedUpdateTrans = false;
	public int  OnFin(){
#if !MC_THREADING
		if(_reqList.Count > 0 && !Pause)
		{
			Exec();
		}
#endif		
		if (_errCnt > 0) {
			if(_reqFinishedList.Count == 0){
				SurfExtractorsMan.SwitchToVxCpu(_reqInProcessList, _reqList);
				CleanUp();
				return 0;
			}
		}

		if(_reqFinishedList.Count == 0)
		{
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
			int num = MaxReqsToProceedOnce;
			int cnt = 0;
			int nReq = _reqFinishedList.Count;
			for(int i = 0; i < nReq; i++){
				cnt++;
				if(_reqFinishedList[i].OnReqFinished() && --num == 0){
					break;
				}
			}
			_reqFinishedList.RemoveRange(0, cnt);
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
			Debug.Log("[SurfMC_OCL]Thread Reset");
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
			Debug.Log("[SurfMC_OCL]Thread stopped");
		}
		catch{
			MonoBehaviour.print("[SurfMC_OCL]Thread stopped with exception");
		}
#endif
		_reqFinishedList.Clear();
		oclMarchingCube.Cleanup();
	}	
	public bool AddSurfExtractReq(IVxSurfExtractReq req){
		lock(_reqList){
			//SurfExtractReqMC r = (SurfExtractReqMC)req;
			_reqList[req.Signature] = req;
		}
		
		return true;
	}
	public IVxSurfExtractReq Contains(VFVoxelChunkData chunk)
	{
		lock(_reqList){
			List<IVxSurfExtractReq> rs = _reqList.Values.ToList();
			IVxSurfExtractReq ret = rs.Find(r=>((SurfExtractReqMC)r)._chunk == chunk);
			if(ret == null)
			{
				ret = rs.Find(r=>((SurfExtractReqMC)r)._chunk.ChunkPosLod.Equals(chunk.ChunkPosLod));
				if(ret == null)
				{
					_reqList.TryGetValue(chunk.SigOfChnk, out ret);
				}
			}
			return ret;
		}
	}

#if MC_DATA_TEST
	byte[] buckyVolumeData;
	void NV_Init(int nTstChunk = 1)
	{
		string dataFilePathName = Application.dataPath + "/Scripts/OclMC/Bucky.raw"+(1<<VoxelTerrainConstants._shift);
        byte[] volume = System.IO.File.ReadAllBytes(dataFilePathName);
		buckyVolumeData = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
		int idxDst = 0;
		int idxSrc = 0;
		for(int z = 1; z < 33; z++)
		{
			for(int y = 1; y < 33; y++)
			{
				idxDst = z*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT + y*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT + VFVoxel.c_VTSize;
				idxSrc = ((z-1)<<(VoxelTerrainConstants._shift+VoxelTerrainConstants._shift)) + ((y-1)<<VoxelTerrainConstants._shift);
				for(int x = 1; x < 33; x++)
				{
					buckyVolumeData[idxDst] = volume[idxSrc];
					idxDst+=2;
					idxSrc++;
				}
			}
		}
		VFVoxelChunkData cdata = new VFVoxelChunkData(null, buckyVolumeData, false);
		cdata.ChunkPosLod_w = new IntVector4();
		for(int n = 0; n < nTstChunk; n++)
			AddSurfExtractReq(new SurfExtractReqMC(n, cdata, NV_Handler,2001));
	}
	void NV_Handler(SurfExtractReqMC req)
	{
		int nMesh = req.Ret.FillMesh(null);
		if(nMesh == 0)
		{
			req._chunkData.AttachChunkGo(null);
			return;			
		}
		Material defMat = null;
		
		GameObject go = new GameObject();
		MeshFilter mf = go.AddComponent<MeshFilter>();
		MeshCollider mc = go.AddComponent<MeshCollider>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
		mc.sharedMesh = null;
		mr.sharedMaterial = defMat;
		nMesh = req.Ret.FillMesh(mf.mesh);
		go.name = "chunk_"+req.Signature;
		while(nMesh > 0)
		{
			GameObject goChild = new GameObject();
			MeshFilter mfChild = goChild.AddComponent<MeshFilter>();
	        MeshCollider mcChild = goChild.AddComponent<MeshCollider>();
	        MeshRenderer mrChild = goChild.AddComponent<MeshRenderer>();
			mcChild.sharedMesh = null;
			mrChild.sharedMaterial = defMat;
			nMesh = req.Ret.FillMesh(mfChild.mesh);
			goChild.transform.parent = go.transform;
			goChild.transform.localPosition = Vector3.zero;
			goChild.name = "chunk_sub";				
		}
	}
#endif	
}
