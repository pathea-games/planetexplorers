//#define USE_SCOPED_COLLIDER

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class B45ChunkGoCreator {
	public delegate void OnComputeComplete(List<int> chunkStampsList, List<B45ChunkData> chunks, uint numChunks);
	public bool _bAsyncBuildMode;
	private List<int> 				_stampsInBuildList;
	private List<B45ChunkData> 	_chunksInBuildList;
	private BiLookup<int, B45ChunkData> _chunkToBuildList;
	private MonoBehaviour _mono;
	//private Stack<OnComputeComplete> _setMeshFuncStack;
	//private Stack<BiLookup<int, B45ChunkData>> _buildListStack;
	// delegate of set chunk mesh
	private OnComputeComplete _setChunkMesh;
	private cpuBlock45 b45proc;
	
	public bool IsIdle{	get{	return  oclMarchingCube.numChunks == 0 && _chunkToBuildList.Count == 0;	}	}
	
	public void Start(MonoBehaviour parentMono, 
								BiLookup<int, B45ChunkData> chunkToComputeList, 
								OnComputeComplete setChunkMeshFunc, cpuBlock45 _b45proc,
								bool bAsyncMode = false)
	{
		b45proc = _b45proc;
		_mono = parentMono;
		_setChunkMesh = setChunkMeshFunc;
		_chunkToBuildList = chunkToComputeList;
		_bAsyncBuildMode = bAsyncMode;
		_mono.StartCoroutine(rebuildChunksInList());
		//_setMeshFuncStack = new Stack<OnComputeComplete>();
		//_buildListStack = new Stack<BiLookup<int, B45ChunkData>>();
		b45proc.init();
	}
	
	void addChunksToOcl()
	{
		_stampsInBuildList.Clear();
		_chunksInBuildList.Clear();

		for(int curlod = 0; curlod <= LODOctreeMan.MaxLod; curlod++)
		{
			int nChunkToBuild = _chunkToBuildList.Count;
			for(int i = nChunkToBuild-1; i >= 0; i--)
			{
				//if(_chunkToBuildList[i].w == curlod)
				{
					int stamp = _chunkToBuildList.GetKeyByIdx_Unsafe(i);
					B45ChunkData chunkData = _chunkToBuildList.GetValueByKey_Unsafe(stamp);
					if(!chunkData.IsStampIdentical(stamp))
					{
						_chunkToBuildList.RemoveAt(i);
						Debug.Log("RemoveChunkInSet"+chunkData.ChunkPosLod+":"+stamp+"|"+chunkData.StampOfChnkUpdating);
						continue;
					}
					if(chunkData.LOD == curlod)
					{
						_chunkToBuildList.RemoveAt(i);
						
						_stampsInBuildList.Add(stamp);
						_chunksInBuildList.Add(chunkData);
						
						b45proc.AddChunkVolumeData(chunkData.DataVT);
						if(b45proc.numChunks() >= cpuBlock45.MAX_CHUNKS-1)
								return;
					}
					
				}
			}
		}
		return;
	}
	IEnumerator rebuildChunksInList()
	{
		_stampsInBuildList = new List<int>();
		_chunksInBuildList = new List<B45ChunkData>();
		//int idxChunk = 0;
		//int curTime;
		while(true)
		{
			
			if(_chunkToBuildList.Count > 0 && b45proc.numChunks() == 0)
			{
				// Fill ocl & _chunkInBuildList
				addChunksToOcl();
					
				if(b45proc.numChunks() > 0){
					uint numChunks = b45proc.numChunks();
					if(_bAsyncBuildMode){

						yield return _mono.StartCoroutine(b45proc.computeIsosurfaceAsyn());
						//yield return StartCoroutine(SetChunksMesh(chunkInComputeList, numChunks()));
						_setChunkMesh(_stampsInBuildList, _chunksInBuildList, numChunks);
						_chunksInBuildList.Clear();
					}
					else{
						// Sync to make time spent on starting shorter

						b45proc.computeIsosurface();
//						MonoBehaviour.print("mesh took:" + (Environment.TickCount - b4) + " @ " + numChunks);
//						b4 = Environment.TickCount;
						_setChunkMesh(_stampsInBuildList, _chunksInBuildList, numChunks);
//						MonoBehaviour.print("collider took:" + (Environment.TickCount - b4));
						_chunksInBuildList.Clear();
						continue;
					}
				}
			}
			
			yield return 0;
		}
	}
	public bool IsChunkInBuild(B45ChunkData chunk)
	{
		return _chunkToBuildList.ContainsValue(chunk) || _chunksInBuildList.Contains(chunk);
	}
}
