using UnityEngine;
using System.Collections;

public static class RSubTerrConstant
{
	public static int ChunkSize = 64;
	public static float ChunkSizeF = (float)(ChunkSize);
	public static int ChunkHeight = 512;
	public static float ChunkHeightF = ChunkHeight;
	public static IntVec3 ChunkCountPerAxis = new IntVec3 (9, 1, 9);
	public static Vector3 TerrainSize = new Vector3 (ChunkSize * ChunkCountPerAxis.x, ChunkHeightF, ChunkSize * ChunkCountPerAxis.z);
}
