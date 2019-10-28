using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class cpuBlock45
{
	public const uint MAX_CHUNKS = 32;
	private static cpuBlock45 inst;
	public static cpuBlock45 Inst
	{
		get 
		{
			if (inst == null)
			{
				inst = new cpuBlock45();
				inst.init();
			}
			return inst;
		}
	}

	List<byte[]> chunkDataList;
	List<Mesh> outputMesh;
	public List<int[]> usedMaterialIndicesList;
	List<List<Mesh>> outputMeshByMaterial;
	
	public uint numChunks()
	{
		return (uint)chunkDataList.Count;
	}
	
	public void AddChunkVolumeData(byte[] volumeData)
	{
		chunkDataList.Add(volumeData);
	}

    public void Cleanup()
    {
		swBlock45 = null;
    }
	
    public void computeIsosurface()
    {
		IEnumerator e = computeIsosurfaceAsyn();
		while(e.MoveNext());
	}

	Block45Kernel swBlock45;
	
	public void clearOutputMesh()
	{
		for(int i = 0; i < outputMesh.Count; i++ )
		{
			outputMesh[i] = null;
		}
		outputMesh.Clear();
	
		for(int i = 0; i < outputMeshByMaterial.Count; i++ )
		{
			outputMeshByMaterial[i] = null;
		}
		outputMeshByMaterial.Clear();
		usedMaterialIndicesList.Clear();
	}
	public List<Mesh> getOutputMesh()
	{
		return outputMesh;
	}
	public List<List<Mesh>> getOutputMeshByMaterial()
	{
		return outputMeshByMaterial;
	}
	public IEnumerator computeIsosurfaceAsyn()
	{
		int chunksRebuilt = 0;
		if(chunkDataList == null)
			yield break;
		Mesh ret;
		for(int i = 0; i < chunkDataList.Count; i++)
		{
			if(chunkDataList[i] != null && chunkDataList[i].Length > 0)
			{
				swBlock45.setInputChunkData(chunkDataList[i]);
//				MonoBehaviour.print("blocks in chunk : " + B45ChunkData.numDots_d(chunkDataList[i]));
				ret = swBlock45.RebuildMeshSM();
				usedMaterialIndicesList.Add(swBlock45.getMaterialMap());
				outputMesh.Add(ret);
				
//				retbm = swBlock45.RebuildMeshByMaterial();
//				outputMeshByMaterial.Add(retbm);
				chunksRebuilt++;
				if((chunksRebuilt % Block45Constants.NumChunksPerFrame) == 0){
					chunksRebuilt = 0;
					yield return 0;
				}
			}
		}
		chunkDataList.Clear();
	}

	public void init()
	{
		swBlock45 = new Block45Kernel();
		outputMesh = new List<Mesh>();
		outputMeshByMaterial = new List<List<Mesh>>();
		chunkDataList = new List<byte[]>();
		usedMaterialIndicesList = new List<int[]>();
	}

}
