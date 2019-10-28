using UnityEngine;
using System;
using System.Collections.Generic;

// Voxel Data Source for VOXEL CREATION SYSTEM
// LOD NOT USED
public class VFCreationDataSource : IVxDataSource
{
	private int m_ChunkNumX;
	private int m_ChunkNumY;
	private int m_ChunkNumZ;
	public IntVector3 ChunkNum { get { return new IntVector3(m_ChunkNumX, m_ChunkNumY, m_ChunkNumZ); } }
	private VFVoxelChunkData[,,] m_Chunks;
	private int[,,] m_ChunkDirtyFlags;
	public List<VFVoxelChunkData> DirtyChunkList { get { return null; } }
//	private static int s_SerialNumber = 0;

	// Constructor
	public VFCreationDataSource(int x_chunk_num, int y_chunk_num, int z_chunk_num)
	{
		m_ChunkNumX = x_chunk_num;
		m_ChunkNumY = y_chunk_num;
		m_ChunkNumZ = z_chunk_num;
		m_Chunks = new VFVoxelChunkData[m_ChunkNumX, m_ChunkNumY, m_ChunkNumZ];
		m_ChunkDirtyFlags = new int[m_ChunkNumX, m_ChunkNumY, m_ChunkNumZ];
	}
	
	// Read & Write chunkdata
	public VFVoxelChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		return m_Chunks[x & 31, y & 31, z & 31];
	}
	public void writeChunk(int x, int y , int z, VFVoxelChunkData data,int lod = 0)
	{
		try
		{
			m_Chunks[x & 31, y & 31, z & 31] = data;
			data.ChunkPosLod_w = new IntVector4(x,y,z,0);
		}
		catch ( Exception e )
		{
			Debug.LogError("[VC] Write Chunk ("+x.ToString()+","+y.ToString()+","+z.ToString()+") Error: " + e.ToString());
		}
	}

	// Read & Write voxel
	public byte this[IntVector3 idx, IntVector4 cposlod]
	{
		get
		{
			return m_Chunks[cposlod.x, cposlod.y, cposlod.z].ReadVoxelAtIdx(idx.x, idx.y, idx.z).Volume;
		}
	}
	const int SHIFT = VoxelTerrainConstants._shift;
	const int MASK = VoxelTerrainConstants._mask;
	const int VOXELCNT = VoxelTerrainConstants._numVoxelsPerAxis;
	public VFVoxel Read(int x, int y, int z, int lod = 0)
	{
		return m_Chunks[x>>SHIFT, y>>SHIFT, z>>SHIFT].ReadVoxelAtIdx(x&MASK, y&MASK, z&MASK);
	}
	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = false)
	{
		int cx = x>>SHIFT;
		int cy = y>>SHIFT;
		int cz = z>>SHIFT;
		int vx = x&MASK;
		int vy = y&MASK;
		int vz = z&MASK;
		
		VFVoxelChunkData chunkData = m_Chunks[cx, cy, cz];
		if ( !chunkData.WriteVoxelAtIdxNoReq(vx, vy, vz, voxel, false) )
			return 0;
		
		m_ChunkDirtyFlags[cx, cy, cz]++;
		int minIdx = VoxelTerrainConstants._numVoxelsPostfix;
		int maxIdx = VoxelTerrainConstants._numVoxelsPerAxis - VoxelTerrainConstants._numVoxelsPrefix;
		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// 0,1,2 bit for xyz dirty mask
		                        // 4,5,6 bit for sign (neg->1)
		                        // 7 bit not used
		
		// If write one edge's voxel may cause the other edge being modified
		if ( vx < minIdx && cx > 0 ) { fx = -1; dirtyMask |= 0x11; } else
		if ( vx >= maxIdx && cx < m_ChunkNumX - 1 ) { fx = 1; dirtyMask |= 0x01; }
		if ( vy < minIdx && cy > 0 ) { fy = -1; dirtyMask |= 0x22; } else
		if ( vy >= maxIdx && cy < m_ChunkNumY - 1 ) { fy = 1; dirtyMask |= 0x02; }
		if ( vz < minIdx && cz > 0 ) { fz = -1; dirtyMask |= 0x44; } else
		if ( vz >= maxIdx && cz < m_ChunkNumZ - 1 ) { fz = 1; dirtyMask |= 0x04; }
		
		if (dirtyMask != 0x80)
		{
			for ( int i = 1; i < 8; ++i )
			{
				if ((dirtyMask & i) == i)
				{
					int dx = fx*VFVoxelChunkData.S_NearChunkOfs[i,0];
					int dy = fy*VFVoxelChunkData.S_NearChunkOfs[i,1];
					int dz = fz*VFVoxelChunkData.S_NearChunkOfs[i,2];
					try
					{
						chunkData = m_Chunks[cx+dx, cy+dy, cz+dz];
						if (chunkData.WriteVoxelAtIdxNoReq(vx-dx*VOXELCNT, vy-dy*VOXELCNT, vz-dz*VOXELCNT, voxel, false) )
						{
							// Successful write
							m_ChunkDirtyFlags[cx+dx, cy+dy, cz+dz]++;
						}
						else
						{
							// Flag for unsuccessful write
							dirtyMask |= i<<8;
						}
					}
					catch ( Exception e )
					{
						Debug.LogError("Unexpected voxel write at ("+(cx+dx).ToString()+","+(cy+dy).ToString()+","+(cz+dz).ToString()+") Error :" + e.ToString());
					}
				}
			}
		}
		return dirtyMask;
	}

	public VFVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		if ( x < 0 || x >= m_ChunkNumX<<SHIFT || 
			 y < 0 || y >= m_ChunkNumY<<SHIFT || 
			 z < 0 || z >= m_ChunkNumZ<<SHIFT )
			return new VFVoxel(0);
		return Read(x,y,z,0);
	}
	public bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0)
	{
		if ( x < 0 || x >= m_ChunkNumX<<SHIFT || 
			 y < 0 || y >= m_ChunkNumY<<SHIFT || 
			 z < 0 || z >= m_ChunkNumZ<<SHIFT )
			return false;
		Write(x,y,z,voxel,0,false);
		return true;
	}

	public void ClearDirtyFlags ()
	{
		for ( int x = 0; x < m_ChunkNumX; ++x )
		{
			for ( int y = 0; y < m_ChunkNumY; ++y )
			{
				for ( int z = 0; z < m_ChunkNumZ; ++z )
				{
					m_ChunkDirtyFlags[x,y,z] = 0;
				}
			}
		}
	}

	public void SubmitReq ()
	{
		for ( int x = 0; x < m_ChunkNumX; ++x )
		{
			for ( int y = 0; y < m_ChunkNumY; ++y )
			{
				for ( int z = 0; z < m_ChunkNumZ; ++z )
				{
					if ( m_ChunkDirtyFlags[x,y,z] > 0 )
						m_Chunks[x,y,z].AddToBuildList();
				}
			}
		}
		ClearDirtyFlags();
	}

}
