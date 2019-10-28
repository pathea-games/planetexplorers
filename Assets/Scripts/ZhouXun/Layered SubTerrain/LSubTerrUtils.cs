using UnityEngine;
using System.Collections;

// Utilities of Layered-subterrain system.
// zhouxun
public static class LSubTerrUtils
{
	// Get an index by layered-subterrain's position
	public static int PosToIndex( int x_index, int z_index )
	{
		return z_index * LSubTerrConstant.XCount + x_index;
	}
	public static int WorldPosToIndex( Vector3 wpos )
	{
		return ((int)(wpos.z / LSubTerrConstant.SizeF)) * LSubTerrConstant.XCount + ((int)(wpos.x / LSubTerrConstant.SizeF));
	}
	// Get layered-subterrain's position by index
	public static IntVector3 IndexToPos( int index )
	{
		return new IntVector3 ( index % LSubTerrConstant.XCount, 0, index / LSubTerrConstant.XCount );
	}
	public static Vector3 IndexToWorldPos( int index )
	{
		return (new IntVector3 ( index % LSubTerrConstant.XCount, 0, index / LSubTerrConstant.XCount )).ToVector3() * LSubTerrConstant.SizeF;
	}
	// World position to indexed-position
	public static IntVector3 WorldPosToPos( Vector3 wpos, IntVector3 ipos )
	{
		ipos.x = (int)(wpos.x / LSubTerrConstant.SizeF);
		ipos.y = 0;
		ipos.z = (int)(wpos.z / LSubTerrConstant.SizeF);
		return ipos;
	}
	// Indexed-position to world position
	public static Vector3 PosToWorldPos( IntVector3 ipos )
	{
		return ipos.ToVector3() * LSubTerrConstant.SizeF;
	}
	// Tree position (in node) to key
	public static int TreePosToKey( IntVector3 pos )
	{
		return ((pos.x + 128) | (pos.y << 18) | ((pos.z + 128) << 9));
	}
	public static IntVector3 KeyToTreePos( int key )
	{
		return new IntVector3 ( (key & 0x1ff) - 128, (key >> 18), ((key >> 9) & 0x1ff) - 128 );
	}
	// Tree position to 32-key
	public static int TreeWorldPosTo32Key( Vector3 tree_world_pos )
	{
		return (Mathf.FloorToInt(tree_world_pos.x/32)) | ((Mathf.FloorToInt(tree_world_pos.z/32)) << 16);
	}
	public static IntVector3 Tree32KeyTo32Pos( int _32key )
	{
		return new IntVector3 ( _32key & 0xffff, 0, _32key >> 16 );
	}
	public static int Tree32PosTo32Key( int x, int z )
	{
		return (x) | (z << 16);
	}
	// Tree Terrain pos to World pos
	public static Vector3 TreeTerrainPosToWorldPos( int tx, int tz, Vector3 tpos )
	{
		Vector3 worldPos = tpos;
		worldPos.x *= LSubTerrConstant.SizeF;
		worldPos.y *= LSubTerrConstant.HeightF;
		worldPos.z *= LSubTerrConstant.SizeF;
		worldPos.x += (tx*LSubTerrConstant.SizeF);
		worldPos.z += (tz*LSubTerrConstant.SizeF);
		return worldPos;
	}

	public static Vector3 TreeTerrainPosToWorldPos( int terrainIndex, Vector3 tpos)
	{
		int tx = terrainIndex % LSubTerrConstant.XCount;
		int tz = terrainIndex / LSubTerrConstant.XCount;
		return TreeTerrainPosToWorldPos(tx, tz, tpos);
	}

	public static Vector3 TreeWorldPosToTerrainPos( Vector3 wpos )
	{
		wpos.x /= LSubTerrConstant.SizeF;
		wpos.y /= LSubTerrConstant.HeightF;
		wpos.z /= LSubTerrConstant.SizeF;
		wpos.x -= Mathf.Floor(wpos.x);
		wpos.z -= Mathf.Floor(wpos.z);

		return wpos;
	}
}
