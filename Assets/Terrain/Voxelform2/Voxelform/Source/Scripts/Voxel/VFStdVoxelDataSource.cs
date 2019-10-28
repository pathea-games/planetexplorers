using UnityEngine;
using System.Collections.Generic;
using System;

// no lod 
public class VFStdVoxelDataSource : IVxDataSource
{
	private List<VFVoxelChunkData> _dirtyChunkList = new List<VFVoxelChunkData>();
	public List<VFVoxelChunkData> DirtyChunkList{ get{	return _dirtyChunkList;	}}
	// chunk size 8*8*8
	
	int _chunkNumX;
	int _chunkNumY;
	int _chunkNumZ;
	VFVoxelChunkData[,,] _chunks;
	public VFStdVoxelDataSource(int width, int height, int depth){
		_chunkNumX = width;
		_chunkNumY = height;
		_chunkNumZ = depth;
		_chunks = new VFVoxelChunkData[_chunkNumX, _chunkNumY, _chunkNumZ];
	}
	public VFVoxelChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		return _chunks[x %  _chunkNumX, y % _chunkNumY , z % _chunkNumZ ];
	}
	public void writeChunk(int x, int y , int z, VFVoxelChunkData vc, int lod = 0){
		try
		{
			_chunks[x % _chunkNumX, y % _chunkNumY, z % _chunkNumZ] = vc;
			vc.ChunkPosLod_w = new IntVector4(x,y,z,lod);
		}
		catch
		{
			Debug.LogError(string.Format("writeChunk Exception. Max Length:({0}, {1}, {2}}), CurPos({3}, {4}, {5})"));
		}
	}
	public byte this[IntVector3 idx, IntVector4 cposlod]{
		get{	
			int cx = cposlod.x%_chunkNumX;
			int cy = cposlod.y%_chunkNumY;
			int cz = cposlod.z%_chunkNumZ;
			return _chunks[cx,cy,cz].ReadVoxelAtIdx(idx.x,idx.y,idx.z).Volume;
		}
	}
	public VFVoxel Read(int x, int y, int z, int lod = 0){
		int _shift = VoxelTerrainConstants._shift;
		int cx = (x>>_shift)%_chunkNumX;
		int cy = (y>>_shift)%_chunkNumY;
		int cz = (z>>_shift)%_chunkNumZ;
		int vx = (x)&VoxelTerrainConstants._mask;
		int vy = (y)&VoxelTerrainConstants._mask;
		int vz = (z)&VoxelTerrainConstants._mask;
		return _chunks[cx,cy,cz].ReadVoxelAtIdx(vx,vy,vz);
	}
	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = true)
	{
		_dirtyChunkList.Clear();
		
		int _shift = VoxelTerrainConstants._shift;
		int cx = (x>>_shift);
		int cy = (y>>_shift);
		int cz = (z>>_shift);
		int cxround = cx%_chunkNumX;
		int cyround = cy%_chunkNumY;
		int czround = cz%_chunkNumZ;
		int vx = (x)&VoxelTerrainConstants._mask;
		int vy = (y)&VoxelTerrainConstants._mask;
		int vz = (z)&VoxelTerrainConstants._mask;
		VFVoxelChunkData chunkData = _chunks[cxround,cyround,czround];
		if(!chunkData.WriteVoxelAtIdx(vx,vy,vz,voxel))
			return 0;
		
		_dirtyChunkList.Add(chunkData);

		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// 0,1,2 bit for xyz dirty mask;4,5,6 bit for sign(neg->1);7 bit for current pos(now not used)
		
		// If write one edge's voxel may cause the other edge being modified
		if( vx< VFVoxelChunkData.S_MinNoDirtyIdx)	{fx = -1;dirtyMask|=0x11;}else
		if( vx>=VFVoxelChunkData.S_MaxNoDirtyIdx)	{fx =  1;dirtyMask|=0x01;}
		if( vy< VFVoxelChunkData.S_MinNoDirtyIdx)	{fy = -1;dirtyMask|=0x22;}else
		if( vy>=VFVoxelChunkData.S_MaxNoDirtyIdx)	{fy =  1;dirtyMask|=0x02;}
		if( vz< VFVoxelChunkData.S_MinNoDirtyIdx)	{fz = -1;dirtyMask|=0x44;}else
		if( vz>=VFVoxelChunkData.S_MaxNoDirtyIdx)	{fz =  1;dirtyMask|=0x04;}

		if(dirtyMask != 0x80)
		{
			for(int i = 1; i < 8; i++)
			{
				if((dirtyMask&i)==i)
				{
					int dx = fx*VFVoxelChunkData.S_NearChunkOfs[i,0], dy = fy*VFVoxelChunkData.S_NearChunkOfs[i,1], dz = fz*VFVoxelChunkData.S_NearChunkOfs[i,2];
					cxround = (cx+dx)%_chunkNumX;
					cyround = (cy+dy)%_chunkNumY;
					czround = (cz+dz)%_chunkNumZ;
					try
					{
						chunkData = _chunks[cxround,cyround,czround];
						if(!chunkData.WriteVoxelAtIdx(
								vx-dx*VoxelTerrainConstants._numVoxelsPerAxis,
								vy-dy*VoxelTerrainConstants._numVoxelsPerAxis, 
								vz-dz*VoxelTerrainConstants._numVoxelsPerAxis, voxel))
						{
							dirtyMask |= i<<8;	// flag for unsuccessful write
						}
						else
						{
							_dirtyChunkList.Add(chunkData);
						}
					}catch(Exception ex)
					{
						Debug.LogError("Unexpected voxel write at("+cxround+","+cyround+","+czround+")" + ex.ToString());
					}
				}
			}
		}
		return dirtyMask;
	}
	public VFVoxel SafeRead(int x, int y, int z, int lod = 0){
		if(	x<0 || x>=VoxelTerrainConstants._worldSideLenX || 
			y<0 || y>=VoxelTerrainConstants._worldSideLenY || 
			z<0 || z>=VoxelTerrainConstants._worldSideLenZ )
			return new VFVoxel(0);
		return Read(x,y,z,lod);
	}
	public bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0){
		if(	x<0 || x>=(_chunkNumX<<VoxelTerrainConstants._shift) || 
			y<1 || y>=(_chunkNumY<<VoxelTerrainConstants._shift) || 
			z<0 || z>=(_chunkNumZ<<VoxelTerrainConstants._shift) )
			return false;
		
		Write(x,y,z,voxel,lod);
		return true;
	}
}

