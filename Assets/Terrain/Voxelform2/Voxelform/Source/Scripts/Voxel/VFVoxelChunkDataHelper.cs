using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

public partial class VFVoxelChunkData : ILODNodeData
{
	public struct EndUpdateReq
	{
		public VFVoxelChunkData _chunk;
		public int _sig;
		public int _stamp;
		public EndUpdateReq(SurfExtractReqMC req){
			_chunk = req._chunk;
			_sig = req._chunkSig;
			_stamp = req._chunkStamp;
		}
		public bool IsValid(){
			return _chunk != null && _chunk.IsMatch (_sig, _stamp);
		}
	}

	static List<IntVector3> multiDirtyChunkPosList = new List<IntVector3>();

	// static chunk data buffer
	public static GenericArrayPool<byte> s_ChunkDataPool = new GenericArrayPool<byte>(VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT, 768);
	private static List<EndUpdateReq> s_lstReqsToEndUpdateNodeData = new List<EndUpdateReq>();	
	public static void EndAllReqs()
	{
		lock(s_lstReqsToEndUpdateNodeData)
		{
			int n = s_lstReqsToEndUpdateNodeData.Count;
			for(int i = 0; i < n; i++)
			{
				if(s_lstReqsToEndUpdateNodeData[i].IsValid())
				{
					s_lstReqsToEndUpdateNodeData[i]._chunk.EndUpdateNodeData();
				}
				//else
				//{
				//	Debug.LogError("[HERE IS THE ORIGINAL POS OF BUG, WE FIX IT]");
				//}
			}
			s_lstReqsToEndUpdateNodeData.Clear();
		}
	}
#region static_helper_members_and_funx
	public readonly static byte[][] S_ChunkDataSolid = new byte[256][];
	public readonly static byte[] S_ChunkDataNull = new byte[0];
	public readonly static byte[] S_ChunkDataAir = new byte[VFVoxel.c_VTSize]{0,0};
	public readonly static byte[] S_ChunkDataWaterSolid = new byte[VFVoxel.c_VTSize]{255, VFVoxelWater.c_iSeaWaterType};
	public readonly static byte[] S_ChunkDataWaterPlane = new byte[VFVoxel.c_VTSize]{VFVoxelWater.c_iSurfaceVol, VFVoxelWater.c_iSeaWaterType};
	// For batch write, index in a 3*3*3=27 cube centered by current chunk
	public static int[][] s_DirtyChunkIndexes = new int[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH][];
	public static int[] s_OfsVTInNeibourChunks = new int[27];
	public readonly static int[,] S_NearChunkOfs = new int[8,3]{ {0,0,0},{1,0,0},{0,1,0},{1,1,0},{0,0,1},{1,0,1},{0,1,1},{1,1,1}, };
	public readonly static int S_MinNoDirtyIdx = VoxelTerrainConstants._numVoxelsPostfix;
	public readonly static int S_MaxNoDirtyIdx = VoxelTerrainConstants._numVoxelsPerAxis - VoxelTerrainConstants._numVoxelsPrefix;
	static VFVoxelChunkData()
	{
		{// Init S_ChunkDataSolid
			for(int i = 0; i < 256; i++){
				S_ChunkDataSolid[i] = new byte[VFVoxel.c_VTSize]{255, (byte)i}; // order is VT
			}
		}
		{// Init s_OfsVTInNeibourChunks
			int idx = 0;
			for(int z = -1; z <= 1; z++)
			{
				for(int y = -1; y <= 1; y++)
				{
					for(int x = -1; x <= 1; x++)
					{
						s_OfsVTInNeibourChunks[idx++] = OneIndexNoPrefix((-x)<<VoxelTerrainConstants._shift, 
						                                                 (-y)<<VoxelTerrainConstants._shift, 
						                                                 (-z)<<VoxelTerrainConstants._shift)
														*VFVoxel.c_VTSize;
					}
				}
			}
		}
		{// Init s_DirtyChunkIndexes
			int start = -VoxelTerrainConstants._numVoxelsPrefix;
			int end = VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE + start;
			// bit 0,1,2 for xyz dirty mask;bit 4,5,6  for sign(neg->1);bit 7 for current pos(now not used)
			int fx = 0, fy = 0, fz = 0;
			int dirtyMaskX = 0, dirtyMaskY = 0, dirtyMaskZ = 0;
			// Note: To write at lod boundary will cause wrong chunk writing because cxround/cyround/czround is incorrect.
			// To write edge voxel will cause neibour chunks being modified
			int idx = 0;
			List<int> lstTmpIndexes = new List<int>(8);
			for(int z = start; z < end; z++)
			{
				if(z< S_MinNoDirtyIdx)	{fz=-1;dirtyMaskZ=0x44;}else
				if(z>=S_MaxNoDirtyIdx)	{fz= 1;dirtyMaskZ=0x04;}else 
										{fz= 0;dirtyMaskZ=   0;}
				for(int y = start; y < end; y++)
				{
					if(y< S_MinNoDirtyIdx)	{fy=-1;dirtyMaskY=0x22;}else
					if(y>=S_MaxNoDirtyIdx)	{fy= 1;dirtyMaskY=0x02;}else
											{fy= 0;dirtyMaskY=   0;}
					for(int x = start; x < end; x++)
					{
						if(x< S_MinNoDirtyIdx)	{fx=-1;dirtyMaskX=0x11;}else
						if(x>=S_MaxNoDirtyIdx)	{fx= 1;dirtyMaskX=0x01;}else
												{fx= 0;dirtyMaskX=   0;}
						int dirtyMask = dirtyMaskZ|dirtyMaskY|dirtyMaskX;
						if(dirtyMask != 0)
						{
							lstTmpIndexes.Clear();
							for(int i = 1; i < 8; i++)
							{
								if((dirtyMask&i)==i)
								{
									int dx = fx*S_NearChunkOfs[i,0], dy = fy*S_NearChunkOfs[i,1], dz = fz*S_NearChunkOfs[i,2];
									lstTmpIndexes.Add((dz+1)*9+(dy+1)*3+(dx+1));
								}
							}
							s_DirtyChunkIndexes[idx] = lstTmpIndexes.ToArray();
						}
						idx++;
					}
				}
			}
		}
	}
	public static int OneIndexNoPrefix(int x, int y, int z)
	{
		return	(z) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED + 
				(y) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE + 
				(x);
	}
	public static int OneIndex(int x, int y, int z)
	{
		return	(z + VoxelTerrainConstants._numVoxelsPrefix) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED + 
				(y + VoxelTerrainConstants._numVoxelsPrefix) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE + 
				(x + VoxelTerrainConstants._numVoxelsPrefix);
	}
	public static List<IntVector3> GetDirtyChunkPosListMulti(int x, int y, int z)
	{
		int cx = (x >> VoxelTerrainConstants._shift);
		int cy = (y >> VoxelTerrainConstants._shift);
		int cz = (z >> VoxelTerrainConstants._shift);
		int vx = (x & VoxelTerrainConstants._mask);
		int vy = (y & VoxelTerrainConstants._mask);
		int vz = (z & VoxelTerrainConstants._mask);

		multiDirtyChunkPosList.Clear();
		multiDirtyChunkPosList.Add(new IntVector3(cx, cy, cz));

		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;   // bit 0,1,2 for xyz dirty mask;bit 4,5,6  for sign(neg->1);bit 7 for current pos(now not used)
								// Note: To write at lod boundary will cause wrong chunk writing because cxround/cyround/czround is incorrect.
								// To write edge voxel will cause neibour chunks being modified
		if (vx < S_MinNoDirtyIdx) { fx = -1; dirtyMask |= 0x11; }
		else
		if (vx >= S_MaxNoDirtyIdx) { fx = 1; dirtyMask |= 0x01; }
		if (vy < S_MinNoDirtyIdx) { fy = -1; dirtyMask |= 0x22; }
		else
		if (vy >= S_MaxNoDirtyIdx) { fy = 1; dirtyMask |= 0x02; }
		if (vz < S_MinNoDirtyIdx) { fz = -1; dirtyMask |= 0x44; }
		else
		if (vz >= S_MaxNoDirtyIdx) { fz = 1; dirtyMask |= 0x04; }
		if (dirtyMask != 0x80)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((dirtyMask & i) == i)
				{
					int dx = fx * S_NearChunkOfs[i, 0], dy = fy * S_NearChunkOfs[i, 1], dz = fz * S_NearChunkOfs[i, 2];
					multiDirtyChunkPosList.Add(new IntVector3(cx + dx, cy + dy, cz + dz));
				}
			}
		}
		return multiDirtyChunkPosList;
	}
	public static List<IntVector3> GetDirtyChunkPosList(int x, int y, int z)
	{
		int cx = (x>>VoxelTerrainConstants._shift);
		int cy = (y>>VoxelTerrainConstants._shift);
		int cz = (z>>VoxelTerrainConstants._shift);
		int vx = (x&VoxelTerrainConstants._mask);
		int vy = (y&VoxelTerrainConstants._mask);
		int vz = (z&VoxelTerrainConstants._mask);
		List<IntVector3> dirtyChunkPosList = new List<IntVector3>();
		dirtyChunkPosList.Add(new IntVector3(cx,cy,cz));
		
		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// bit 0,1,2 for xyz dirty mask;bit 4,5,6  for sign(neg->1);bit 7 for current pos(now not used)
		// Note: To write at lod boundary will cause wrong chunk writing because cxround/cyround/czround is incorrect.
		// To write edge voxel will cause neibour chunks being modified
		if( vx< S_MinNoDirtyIdx)	{fx = -1;dirtyMask|=0x11;}else
		if( vx>=S_MaxNoDirtyIdx)	{fx =  1;dirtyMask|=0x01;}
		if( vy< S_MinNoDirtyIdx)	{fy = -1;dirtyMask|=0x22;}else
		if( vy>=S_MaxNoDirtyIdx)	{fy =  1;dirtyMask|=0x02;}
		if( vz< S_MinNoDirtyIdx)	{fz = -1;dirtyMask|=0x44;}else
		if( vz>=S_MaxNoDirtyIdx)	{fz =  1;dirtyMask|=0x04;}
		if(dirtyMask != 0x80)
		{
			for(int i = 1; i < 8; i++)
			{
				if((dirtyMask&i)==i)
				{
					int dx = fx*S_NearChunkOfs[i,0], dy = fy*S_NearChunkOfs[i,1], dz = fz*S_NearChunkOfs[i,2];
					dirtyChunkPosList.Add(new IntVector3(cx+dx,cy+dy,cz+dz));
				}
			}
		}
		return dirtyChunkPosList;		
	}	
	public static void ExpandHollowChunkData(VFVoxelChunkData chunk)
	{	
		byte volume = chunk._chunkData[0];
		byte type = chunk._chunkData[1];
		byte[] data = s_ChunkDataPool.Get();
		if(volume != 0)
		{
			for(int i = 0; i < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT;)
			{
				data[i++] = volume;
				data[i++] = type;
			}
		}
		else
		{
			Array.Clear(data, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
		}
		chunk._chunkData = data;
		chunk._bFromPool = true;
	}
	public static void ModVolume(VFVoxelChunkData curChunk, int idxVT, byte volume, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks)
	{
		// FIXME : Tmporarily use >>1 instead of /VFVoxel.c_SizeVT
		int[] dirtyChunkIndexes = VFVoxelChunkData.s_DirtyChunkIndexes [idxVT >> 1];
		if (dirtyChunkIndexes != null) { // Special code for edge
			int len = dirtyChunkIndexes.Length;
			for (int i = 0; i < len; i++) {
				int idx = dirtyChunkIndexes [i];
				VFVoxelChunkData chunk = dirtyNeibourChunks [idx];
				if (chunk == null) {
					chunk = neibourChunks [idx];
					if (chunk != null && chunk.BeginBatchWriteVoxels ()) {
						dirtyNeibourChunks [idx] = chunk;
					} else {
						// SideEffect: some chunks have been added into dirtyNeibourChunks
						//Debug.LogWarning("Neibour Chunk["+idx+"] is invalid.");
						return;
					}
				}
			}
			for (int i = 0; i < len; i++) {
				int idx = dirtyChunkIndexes [i];
				int curIdxVT = idxVT + VFVoxelChunkData.s_OfsVTInNeibourChunks [idx];
				VFVoxelChunkData chunk = dirtyNeibourChunks [idx];
				if (curIdxVT < chunk.DataVT.Length) {
					chunk.DataVT [curIdxVT] = volume;
				}
			}
		}
		if (idxVT < curChunk.DataVT.Length){
			curChunk.DataVT [idxVT] = volume;
		}
		dirtyNeibourChunks[13] = curChunk;
	}
	public static void ModVolumeType(VFVoxelChunkData curChunk, int idxVT, byte volume, byte type, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks)
	{
		// FIXME : Tmporarily use >>1 instead of /VFVoxel.c_SizeVT
		int[] dirtyChunkIndexes = VFVoxelChunkData.s_DirtyChunkIndexes[idxVT>>1];
		if(dirtyChunkIndexes != null) // Special code for edge
		{
			int len = dirtyChunkIndexes.Length;
			for(int i = 0; i < len; i++)
			{
				int idx = dirtyChunkIndexes[i];
				VFVoxelChunkData chunk = dirtyNeibourChunks[idx];
				if(chunk == null)
				{
					chunk = neibourChunks[idx];
					if(chunk != null && chunk.BeginBatchWriteVoxels())
					{
						dirtyNeibourChunks[idx] = chunk;
					}
					else
					{
						// SideEffect: some chunks have been added into dirtyNeibourChunks
						//Debug.LogWarning("Neibour Chunk["+idx+"] is invalid.");
						return;
					}
				}
			}
			for(int i = 0; i < len; i++)
			{
				int idx = dirtyChunkIndexes[i];
				int curIdxVT = idxVT + VFVoxelChunkData.s_OfsVTInNeibourChunks[idx];
				VFVoxelChunkData chunk = dirtyNeibourChunks[idx];
				if (curIdxVT < chunk.DataVT.Length) {
					chunk.DataVT[curIdxVT] = volume;
					chunk.DataVT[curIdxVT+1] = type;
				}
			}
		}
		if (idxVT < curChunk.DataVT.Length) {
			curChunk.DataVT [idxVT] = volume;
			curChunk.DataVT [idxVT + 1] = type;
		}
		dirtyNeibourChunks[13] = curChunk;
	}
	public static int GenStampOfUpdating(IntVector4 cposlod){
		// Calculate a stamp for pos/_lod, for convenience of adding stamp mask.
		return cposlod.GetHashCode();
	}
#endregion
}
