using UnityEngine;
using System.Collections.Generic;
using System;

// no lod 
public class B45StdDataSource : IB45DataSource
{
	// chunk size 8*8*8
	public readonly int[,] nearChunkOfs = new int[8,3]{
		{0,0,0},
		{1,0,0},
		{0,1,0},
		{1,1,0},
		{0,0,1},
		{1,0,1},
		{0,1,1},
		{1,1,1},
	};
	
	int _chunkNumX;
	int _chunkNumY;
	int _chunkNumZ;
	
	Dictionary<IntVector3, B45ChunkData> _chunks;
	BiLookup<int, B45ChunkData> _chunkRebuildList;
	
	Dictionary<IntVector3, B45Block> _SaveData;
	
//	B45ChunkData[,,] _chunks;
	public B45StdDataSource(int width, int height, int depth, BiLookup<int, B45ChunkData> chunkRebuildList){
		_chunkNumX = width;
		_chunkNumY = height;
		_chunkNumZ = depth;
		_chunkRebuildList = chunkRebuildList;
//		_chunks = new B45ChunkData[_chunkNumX, _chunkNumY, _chunkNumZ];
		_chunks = new Dictionary<IntVector3, B45ChunkData>();
		_SaveData = new Dictionary<IntVector3, B45Block>();
	}
	public B45ChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		IntVector3 index = new IntVector3(x %  _chunkNumX,y % _chunkNumY,z % _chunkNumZ);
		if(!_chunks.ContainsKey(index))
			_chunks[index] = CreateChunk(index);
		
		return _chunks[index];
//		return _chunks[x %  _chunkNumX, y % _chunkNumY , z % _chunkNumZ ];
	}
	public void writeChunk(int x, int y , int z, B45ChunkData vc, int lod = 0){
		try
		{
			IntVector3 index = new IntVector3(x %  _chunkNumX,y % _chunkNumY,z % _chunkNumZ);
			_chunks[index] = vc;
	//		_chunks[x % _chunkNumX, y % _chunkNumY, z % _chunkNumZ] = vc;
		}
		catch(Exception)
		{
			Debug.LogError(string.Format("writeB45Chunk Exception. Max Length:({0}, {1}, {2}}), CurPos({3}, {4}, {5})"));
		}
		//vc. = new IntVector4(x,y,z,0);
	}
	public byte this[IntVector3 idx, int lod]{
		get{	return Read(idx.x, idx.y, idx.z, lod).blockType;	}
	}
	public B45Block Read(int x, int y, int z, int lod = 0){
		int _shift = Block45Constants._shift;
		int cx = (x>>_shift)%_chunkNumX;
		int cy = (y>>_shift)%_chunkNumY;
		int cz = (z>>_shift)%_chunkNumZ;
		int vx = (x)&Block45Constants._mask;
		int vy = (y)&Block45Constants._mask;
		int vz = (z)&Block45Constants._mask;
		IntVector3 index = new IntVector3(cx,cy,cz);
		if(!_chunks.ContainsKey(index))
			return new B45Block(0, 0);
		
		return _chunks[index].ReadVoxelAtIdx(vx,vy,vz);
	}
	public int Write(int x, int y, int z, B45Block voxel, int lod = 0){
		int _shift = Block45Constants._shift;
		int cx = (x>>_shift);
		int cy = (y>>_shift);
		int cz = (z>>_shift);
		int cxround = cx%_chunkNumX;
		int cyround = cy%_chunkNumY;
		int czround = cz%_chunkNumZ;
		int vx = (x)&Block45Constants._mask;
		int vy = (y)&Block45Constants._mask;
		int vz = (z)&Block45Constants._mask;
		
		IntVector3 index = new IntVector3(cxround,cyround,czround);
		if(!_chunks.ContainsKey(index))
			_chunks[index] = CreateChunk(index);
		
		if(!_chunks[index].WriteVoxelAtIdx(vx,vy,vz,voxel))
			return 0;
		
		if((voxel.blockType>>2) == 0)
			_SaveData.Remove(new IntVector3(x,y,z));
		else
			_SaveData[new IntVector3(x,y,z)] = voxel;

		int minIdx = Block45Constants._numVoxelsPostfix;
		int maxIdx = Block45Constants._numVoxelsPerAxis - Block45Constants._numVoxelsPrefix;
		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// 0,1,2 bit for xyz dirty mask;4,5,6 bit for sign(neg->1);7 bit for current pos(now not used)
		
		// If write one edge's voxel may cause the other edge being modified
		if( vx< minIdx&&cx>0)									{fx = -1;dirtyMask|=0x11;}else
		if( vx>=maxIdx&&cx<Block45Constants._worldMaxCX-1)	{fx =  1;dirtyMask|=0x01;}
		if( vy< minIdx&&cy>0)									{fy = -1;dirtyMask|=0x22;}else
		if( vy>=maxIdx&&cy<Block45Constants._worldMaxCY-1)	{fy =  1;dirtyMask|=0x02;}
		if( vz< minIdx&&cz>0)									{fz = -1;dirtyMask|=0x44;}else
		if( vz>=maxIdx&&cz<Block45Constants._worldMaxCZ-1)	{fz =  1;dirtyMask|=0x04;}
		
		if(dirtyMask != 0x80)
		{
			for(int i = 1; i < 8; i++)
			{
				if((dirtyMask&i)==i)
				{
					int dx = fx*nearChunkOfs[i,0], dy = fy*nearChunkOfs[i,1], dz = fz*nearChunkOfs[i,2];
					cxround = (cx+dx)%_chunkNumX;
					cyround = (cy+dy)%_chunkNumY;
					czround = (cz+dz)%_chunkNumZ;
					
					index = new IntVector3(cxround,cyround,czround);
					if(!_chunks.ContainsKey(index))
						_chunks[index] = CreateChunk(index);
					
					try
					{
						if(!_chunks[index].WriteVoxelAtIdx(
								vx-dx*Block45Constants._numVoxelsPerAxis,
								vy-dy*Block45Constants._numVoxelsPerAxis, 
								vz-dz*Block45Constants._numVoxelsPerAxis, voxel))
						{
							dirtyMask |= i<<8;	// flag for unsuccessful write
						}
					}catch(Exception)
					{
						Debug.LogError("Unexpected block45 write at("+cxround+","+cyround+","+czround+")");
					}
				}
			}
		}
		return dirtyMask;
	}
	public B45Block SafeRead(int x, int y, int z, int lod = 0){
        //if(	x<0 || x>=Block45Constants._worldSideLenX || 
        //    y<0 || y>=Block45Constants._worldSideLenY || 
        //    z<0 || z>=Block45Constants._worldSideLenZ )
        //    return new B45Block(0,0);
		return Read(x,y,z,lod);
	}
	public bool SafeWrite(int x, int y, int z, B45Block voxel, int lod = 0){
        //if(	x<0 || x>=(_chunkNumX<<Block45Constants._shift) || 
        //    y<1 || y>=(_chunkNumY<<Block45Constants._shift) || 
        //    z<0 || z>=(_chunkNumZ<<Block45Constants._shift) )
        //    return false;
		
		Write(x,y,z,voxel,lod);
		return true;
	}
	
	B45ChunkData CreateChunk(IntVector3 index)
	{
		//LODOctreeNode tmpNode = new LODOctreeNode(null, 0, 
		//	new IntVector3(
		//	(index.x<<Block45Constants._shift) / Block45Constants._scaleInverted, 
		//	(index.y<<Block45Constants._shift) / Block45Constants._scaleInverted, 
		//	(index.z<<Block45Constants._shift) / Block45Constants._scaleInverted));
		//B45ChunkData chunk = new B45ChunkData(tmpNode, new byte[B45Block.Block45Size]);
		B45ChunkData chunk = new B45ChunkData(new byte[B45Block.Block45Size]);
		chunk.BuildList = _chunkRebuildList;
        writeChunk( index.x, index.y, index.z, chunk );
		
		chunk.ChunkPosLod_w = new IntVector4(
			(index.x), 
			(index.y), 
			(index.z), 0);
//					chunk.ChunkPosLod_w = new IntVector4((x<<Block45Constants._shift) * Block45Constants._scale,
//												(y<<Block45Constants._shift) * Block45Constants._scale,
//												(z<<Block45Constants._shift) * Block45Constants._scale, 0);
		chunk.AddToBuildList();
		
		return chunk;
	}
	public void CleanupOBChunks()
	{
		IntVector3 cursorPos = IntVector3.Zero;
		IntVector3 viewBound = IntVector3.Zero;
		List<IntVector3> vecToDispose = new List<IntVector3>();
		foreach(IntVector3 key in _chunks.Keys)
		{
			if(Mathf.Abs(key.x - cursorPos.x) > viewBound.x ||
			   Mathf.Abs(key.y - cursorPos.y) > viewBound.y ||
			   Mathf.Abs(key.z - cursorPos.z) > viewBound.z )
			{
				vecToDispose.Add(key);
			}
		}
		for(int i = 0; i < vecToDispose.Count; i++){
			IntVector3 vec = vecToDispose[i];
			B45ChunkData cd = _chunks[vec];
			if(cd != null)
			{
				cd.ClearMem();
				cd.DestroyChunkGO();
			}
		}
	}
	
	public Dictionary<IntVector3, B45Block> GetSaveDate()
	{
		return _SaveData;
	}
	
	public void ApplySaveData(Dictionary<IntVector3, B45Block> saveData)
	{
		foreach(IntVector3 index in saveData.Keys)
		{
			SafeWrite(index.x, index.y, index.z, saveData[index], 0);
		}
	}
}

