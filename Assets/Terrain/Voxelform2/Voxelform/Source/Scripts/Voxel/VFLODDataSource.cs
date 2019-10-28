using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.IO;

// Class for ChunkData writing/reading management
public class VFLODDataSource : IVxDataSource
{
	private int _idxData;
	private LODOctreeNode[][,,] _lodNodes;
	private List<VFVoxelChunkData> _dirtyChunkList = new List<VFVoxelChunkData>();
	public List<VFVoxelChunkData> DirtyChunkList{ get{	return _dirtyChunkList;	}}
	
	public VFLODDataSource(LODOctreeNode[][,,] lodNodes, int idxData = 0){
		_lodNodes = lodNodes;
		_idxData = idxData;
	}
	public VFVoxelChunkData readChunk(int cx, int cy, int cz, int lod = 0)
	{
		int chunkNumX = LODOctreeMan._xChunkCount >> lod;
		int chunkNumY = LODOctreeMan._yChunkCount >> lod;
		int chunkNumZ = LODOctreeMan._zChunkCount >> lod;
		int cxround = (cx>>lod)%chunkNumX;
		int cyround = (cy>>lod)%chunkNumY;
		int czround = (cz>>lod)%chunkNumZ;
		if(cxround < 0)	cxround += chunkNumX;
		if(cyround < 0)	cyround += chunkNumY;
		if(czround < 0)	czround += chunkNumZ;
		VFVoxelChunkData chunkData = (VFVoxelChunkData)_lodNodes[lod][cxround, cyround, czround]._data[_idxData];
		IntVector4 poslod = chunkData.ChunkPosLod;
		if(poslod.x != cx || poslod.y != cy || poslod.z != cz || poslod.w != lod)
			return null;
		return chunkData;
	}
	public void writeChunk(int cposx, int cposy, int cposz, VFVoxelChunkData data, int lod = 0)
	{
		Debug.LogError("Unavailable interface: writeChunk");
	}
	
	public byte this[IntVector3 idx, IntVector4 cposlod]{
		get{	
#if true
			int lod = cposlod.w;
			int chunkNumX = LODOctreeMan._xChunkCount >> lod;
			int chunkNumY = LODOctreeMan._yChunkCount >> lod;
			int chunkNumZ = LODOctreeMan._zChunkCount >> lod;
			int cxround = (cposlod.x>>lod)%chunkNumX;
			int cyround = (cposlod.y>>lod)%chunkNumY;
			int czround = (cposlod.z>>lod)%chunkNumZ;
			if(cxround < 0)	cxround += chunkNumX;
			if(cyround < 0)	cyround += chunkNumY;
			if(czround < 0)	czround += chunkNumZ;
			VFVoxelChunkData chunkData = (VFVoxelChunkData)_lodNodes[lod][cxround, cyround, czround]._data[_idxData];
			if(!cposlod.Equals(chunkData.ChunkPosLod))
				return 0;

			if(idx.x < -1 || idx.y < -1 || idx.z < -1) // Invoked in transvoxel
			{
				//Debug.LogError("Error index of lod data source array");
				int vx = idx.x < -1 ? -1 : idx.x;
				int vy = idx.y < -1 ? -1 : idx.y;
				int vz = idx.z < -1 ? -1 : idx.z;
				return chunkData.ReadVoxelAtIdx(vx,vy,vz).Volume;
			}
			return chunkData.ReadVoxelAtIdx(idx.x,idx.y,idx.z).Volume;	
#else
			return Read((cposlod.x<<VoxelTerrainConstants._shift)+idx.x,
						(cposlod.y<<VoxelTerrainConstants._shift)+idx.y,
						(cposlod.z<<VoxelTerrainConstants._shift)+idx.z,
						cposlod.w).Volume;
#endif
		}
	}
	public VFVoxel Read(int x, int y, int z, int lod = 0){
		VFVoxelChunkData cdata = readChunk(x>>VoxelTerrainConstants._shift,
		                                   y>>VoxelTerrainConstants._shift,
		                                   z>>VoxelTerrainConstants._shift,
		                                   lod);
		if(cdata == null)
			return new VFVoxel(0);

		int vx = (x>>lod)&VoxelTerrainConstants._mask;
		int vy = (y>>lod)&VoxelTerrainConstants._mask;
		int vz = (z>>lod)&VoxelTerrainConstants._mask;
		return cdata.ReadVoxelAtIdx(vx,vy,vz);
	}
	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = true )
	{
		_dirtyChunkList.Clear();
		
		int chunkNumX = LODOctreeMan._xChunkCount >> lod;
		int chunkNumY = LODOctreeMan._yChunkCount >> lod;
		int chunkNumZ = LODOctreeMan._zChunkCount >> lod;
		int cx = x>>VoxelTerrainConstants._shift;
		int cy = y>>VoxelTerrainConstants._shift;
		int cz = z>>VoxelTerrainConstants._shift;
		int cxround = (cx>>lod)%chunkNumX;
		int cyround = (cy>>lod)%chunkNumY;
		int czround = (cz>>lod)%chunkNumZ;
		if(cxround < 0)	cxround += chunkNumX;
		if(cyround < 0)	cyround += chunkNumY;
		if(czround < 0)	czround += chunkNumZ;
		int vx = (x>>lod)&VoxelTerrainConstants._mask;
		int vy = (y>>lod)&VoxelTerrainConstants._mask;
		int vz = (z>>lod)&VoxelTerrainConstants._mask;
		LODOctreeNode node = _lodNodes[lod][cxround,cyround,czround];
		VFVoxelChunkData chunkData = (VFVoxelChunkData)node._data[_idxData];
		IntVector4 poslod = chunkData.ChunkPosLod;
		if(poslod == null || poslod.x != cx || poslod.y != cy || poslod.z != cz)
			return 0;

		VFVoxelChunkData.ProcOnWriteVoxel onWriteVoxel = null;
		if(_idxData==0&&VFVoxelWater.self!=null) onWriteVoxel =	VFVoxelWater.self.OnWriteVoxelOfTerrain;
		if(!chunkData.WriteVoxelAtIdx(vx,vy,vz,voxel,bUpdateLod,onWriteVoxel))
			return (0x100);//1<<8
		
		_dirtyChunkList.Add(chunkData);
		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// bit 0,1,2 for xyz dirty mask;bit 4,5,6  for sign(neg->1);bit 7 for current pos(now not used)
		
		// Note: To write at lod boundary will cause wrong chunk writing because cxround/cyround/czround is incorrect.
		// To write edge voxel will cause neibour chunks being modified
		if( vx< VFVoxelChunkData.S_MinNoDirtyIdx)	{fx = -1;dirtyMask|=0x11;}else
		if( vx>=VFVoxelChunkData.S_MaxNoDirtyIdx)	{fx =  1;dirtyMask|=0x01;}
		if( vy< VFVoxelChunkData.S_MinNoDirtyIdx)	{fy = -1;dirtyMask|=0x22;}else
		if( vy>=VFVoxelChunkData.S_MaxNoDirtyIdx)	{fy =  1;dirtyMask|=0x02;}
		if( vz< VFVoxelChunkData.S_MinNoDirtyIdx)	{fz = -1;dirtyMask|=0x44;}else
		if( vz>=VFVoxelChunkData.S_MaxNoDirtyIdx)	{fz =  1;dirtyMask|=0x04;}
		
		if(dirtyMask != 0x80)
		{
			int cxlod = (cx>>lod);
			int cylod = (cy>>lod);
			int czlod = (cz>>lod);
			for(int i = 1; i < 8; i++)
			{
				if((dirtyMask&i)==i)
				{
					int dx = fx*VFVoxelChunkData.S_NearChunkOfs[i,0], dy = fy*VFVoxelChunkData.S_NearChunkOfs[i,1], dz = fz*VFVoxelChunkData.S_NearChunkOfs[i,2];
					cxround = (cxlod+dx)%chunkNumX;
					cyround = (cylod+dy)%chunkNumY;
					czround = (czlod+dz)%chunkNumZ;
					if(cxround < 0)	cxround += chunkNumX;
					if(cyround < 0)	cyround += chunkNumY;
					if(czround < 0)	czround += chunkNumZ;
					chunkData = (VFVoxelChunkData)_lodNodes[lod][cxround,cyround,czround]._data[_idxData];
					if(!chunkData.WriteVoxelAtIdx(
							vx-dx*VoxelTerrainConstants._numVoxelsPerAxis,
							vy-dy*VoxelTerrainConstants._numVoxelsPerAxis, 
							vz-dz*VoxelTerrainConstants._numVoxelsPerAxis, voxel, bUpdateLod))
					{
						dirtyMask |= 1<<(i+8);	// flag for unsuccessful write
					}
					else
					{
						_dirtyChunkList.Add(chunkData);
					}
				}
			}
		}
		return dirtyMask;
	}

	public VFVoxel SafeRead(int x, int y, int z, int lod = 0){
#if false	// Inifinit terrain's xz may be any number.
		if(	x<0 || x>=VoxelTerrainConstants._worldSideLenX || 
			y<0 || y>=VoxelTerrainConstants._worldSideLenY || 
			z<0 || z>=VoxelTerrainConstants._worldSideLenZ )
			return new VFVoxel(0);
#endif
		return Read(x,y,z,lod);
	}
	public bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0){
#if false	// Inifinit terrain's xz may be any number.
		if(	x<0 || x>=VoxelTerrainConstants._worldSideLenX || 
			y<1 || y>=VoxelTerrainConstants._worldSideLenY || 
			z<0 || z>=VoxelTerrainConstants._worldSideLenZ )
			return false;
#else
		if(y<1)	return false;	//Keep the very ground
#endif
			
		Write(x,y,z,voxel,lod);
		return true;
	}
}

