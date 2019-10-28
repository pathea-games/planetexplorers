using UnityEngine;
using System;
using System.Collections.Generic;

public interface IB45DataSource
{
	// read a chunk at the specified location
    B45ChunkData readChunk(int cx, int cy, int cz, int lod = 0);
	void writeChunk(int cx, int cy, int cz, B45ChunkData data, int lod = 0);
	
	// read a voxel at the specified location
	B45Block Read(int x, int y, int z, int lod = 0);
	int Write(int x, int y, int z, B45Block voxel,int lod = 0);	// return affected chunks mask(dirty mask)

	B45Block SafeRead(int x, int y, int z, int lod = 0);
	bool SafeWrite(int x, int y, int z, B45Block voxel, int lod = 0);
	byte this[IntVector3 idx, int lod]{	get;	}
	
}
