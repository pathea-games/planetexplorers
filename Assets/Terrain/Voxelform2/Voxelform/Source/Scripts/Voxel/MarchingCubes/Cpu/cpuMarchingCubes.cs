using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class cpuMarchingCubes
{
	public const uint MAX_CHUNKS = 32;
	
	// calculated from the ocl version
	private uint _numChunks = 0;
	
	List<byte[]> chunkDataList;
	List<Mesh> outputMesh;
	
	public uint numChunks()
	{
		return _numChunks;
	}
	
	public void AddChunkVolumeData(byte[] volumeData)
	{
		_numChunks++;
		chunkDataList.Add(volumeData);
	}

    public void Cleanup()
    {
		swMarchingCubes = null;
    }
	
    public void computeIsosurface()
    {
		IEnumerator e = computeIsosurfaceAsyn();
		while(e.MoveNext());
	}

	MarchingCubesSW swMarchingCubes;
	
	public void clearOutputMesh()
	{
		for(int i = 0; i < outputMesh.Count; i++ )
		{
			outputMesh[i] = null;
		}
		outputMesh.Clear();
	}
	public List<Mesh> getOutputMesh()
	{
		return outputMesh;
	}
	public IEnumerator computeIsosurfaceAsyn()
	{
		if(chunkDataList == null)
			yield break;
		Mesh ret;
		for(int i = 0; i < chunkDataList.Count; i++)
		{
			if(chunkDataList[i] != null )
			{
				swMarchingCubes.setInputChunkData(chunkDataList[i]);
				ret = swMarchingCubes.RebuildMesh();
				outputMesh.Add(ret);
				
				yield return 0;
			}
		}
		chunkDataList.Clear();
		_numChunks = 0;
	}

	public void init()
	{
		swMarchingCubes = new MarchingCubesSW();
		outputMesh = new List<Mesh>();
		chunkDataList = new List<byte[]>();
	}
}