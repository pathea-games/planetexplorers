using UnityEngine;
using System.Collections;

public static class RSubTerrUtils
{
	public static int TilePosToIndex( IntVector2 tilePos )
	{
		return ChunkPosToIndex(tilePos.x*32/RSubTerrConstant.ChunkSize, tilePos.y*32/RSubTerrConstant.ChunkSize);
	}
	public static int ChunkPosToIndex( int x, int z )
	{
		return (((x+16384) & 32767) << 15) | ((z+16384) & 32767);
	}
	public static int IndexToChunkX( int index )
	{
		return (index >> 15) - 16384;
	}
	public static int IndexToChunkZ( int index )
	{
		return (index & 32767) - 16384;
	}
	public static IntVec3 IndexToChunkPos( int index )
	{
		return new IntVec3 ((index >> 15) - 16384, 0, (index & 32767) - 16384);
	}
	public static Vector3 ChunkOffset( int cnk_index )
	{
		return IndexToChunkPos(cnk_index).ToVector3() * RSubTerrConstant.ChunkSizeF;
	}
	public static Vector3 ChunkOffset( int x, int z )
	{
		return new Vector3(x,0,z) * RSubTerrConstant.ChunkSizeF;
	}
	public static Vector3 TreeChunkPosToWorldPos( Vector3 cnk_pos, int cnk_index )
	{
		return cnk_pos + ChunkOffset(cnk_index);
	}
	public static Vector3 TreeWorldPosToChunkPos( Vector3 world_pos, int cnk_index )
	{
		Vector3 cnk_pos = IndexToChunkPos(cnk_index).ToVector3() * RSubTerrConstant.ChunkSizeF;
		return world_pos - cnk_pos;
	}
	public static Vector3 TreeChunkPosToWorldPos( Vector3 cnk_pos, int cnk_x, int cnk_z )
	{
		return cnk_pos + ChunkOffset(cnk_x, cnk_z);
	}
	public static int GetChunkIdContainsTree( Vector3 world_pos )
	{
		int x = Mathf.FloorToInt(world_pos.x / RSubTerrConstant.ChunkSizeF);
		int z = Mathf.FloorToInt(world_pos.z / RSubTerrConstant.ChunkSizeF);
		return ChunkPosToIndex(x,z);
	}
	public static int TreeChunkPosToIndex( Vector3 cnk_pos )
	{
		int x = Mathf.FloorToInt(cnk_pos.x + RSubTerrConstant.ChunkSizeF);
		int y = Mathf.FloorToInt(cnk_pos.y);
		int z = Mathf.FloorToInt(cnk_pos.z + RSubTerrConstant.ChunkSizeF);
		return (x << 8) | (y << 16) | (z);
	}
	public static int TreeWorldPosToChunkIndex( Vector3 world_pos, int cnk_index )
	{
		Vector3 cnk_pos = TreeWorldPosToChunkPos(world_pos, cnk_index);
		int x = Mathf.FloorToInt(cnk_pos.x + RSubTerrConstant.ChunkSizeF);
		int y = Mathf.FloorToInt(cnk_pos.y);
		int z = Mathf.FloorToInt(cnk_pos.z + RSubTerrConstant.ChunkSizeF);
		return (x << 8) | (y << 16) | (z);
	}
	public static int Tree32PosTo32Index( int x, int z )
	{
		return (((x+16384) & 32767) << 15) | ((z+16384) & 32767);
	}
	public static IntVec3 Tree32KeyTo32Pos( int index )
	{
		return new IntVec3 ((index >> 15) - 16384, 0, (index & 32767) - 16384);
	}
}
