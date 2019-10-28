using UnityEngine;
using System.Collections;

// Constants of Layered-subterrain system.
// zhouxun
public static class LSubTerrConstant
{
	public const int SizeShift = 8;
	public const int Size = 1 << SizeShift;
	public const float SizeF = (float)(Size);
	public const float HeightF = 3000F;
    public const int XCount = VoxelTerrainConstants._worldSideLenX >> SizeShift;
    public const int ZCount = VoxelTerrainConstants._worldSideLenZ >> SizeShift;
	public const int XZCount = XCount * ZCount;
}
