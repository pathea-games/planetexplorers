using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.IO;

// Class for ChunkData writing/reading management, now not used
public class VFWaterLODDataSource : VFLODDataSource
{
	public VFWaterLODDataSource(LODOctreeNode[][,,] lodNodes) : base(lodNodes, 1)
	{}
	/*
	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = true )
	{
		if(lod != 0)										return (0x100);//1<<8
		VFVoxelChunkData cdata = readChunk(x>>VoxelTerrainConstants._shift,
		                                   y>>VoxelTerrainConstants._shift,
		                                   z>>VoxelTerrainConstants._shift,
		                                   lod);
		if(cdata == null)									return (0x100);//1<<8
		if(cdata.DataVT == VFVoxelChunkData.S_ChunkDataNull)return (0x100);//1<<8

		int vx = (x>>lod)&VoxelTerrainConstants._mask;
		int vy = (y>>lod)&VoxelTerrainConstants._mask;
		int vz = (z>>lod)&VoxelTerrainConstants._mask;
		//return cdata.ReadVoxelAtIdx(vx,vy,vz);
		return 0;
		int chunkNumX = VoxelTerrainConstants._xChunkCount >> lod;
		int chunkNumY = VoxelTerrainConstants._yChunkCount >> lod;
		int chunkNumZ = VoxelTerrainConstants._zChunkCount >> lod;
		int shift = VoxelTerrainConstants._shift + lod;
		int cx = (x>>shift);
		int cy = (y>>shift);
		int cz = (z>>shift);
		int cxround = cx%chunkNumX;
		int cyround = cy%chunkNumY;
		int czround = cz%chunkNumZ;
		if(cxround < 0)	cxround += chunkNumX;
		if(cyround < 0)	cyround += chunkNumY;
		if(czround < 0)	czround += chunkNumZ;
		int vx = (x>>lod)&VoxelTerrainConstants._mask;
		int vy = (y>>lod)&VoxelTerrainConstants._mask;
		int vz = (z>>lod)&VoxelTerrainConstants._mask;
		VFVoxelChunkData chunkData = (VFVoxelChunkData)_lodNodes[lod][cxround,cyround,czround]._data[_idxData];
		return (0x100);//1<<8
	}
	*/
}


