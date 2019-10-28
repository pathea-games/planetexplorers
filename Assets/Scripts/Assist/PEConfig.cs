using UnityEngine;
using System.Collections;

public class PEConfig
{
    public const int TerrainLayer = 1<<12; //VFVoxelterrain
    public const int GroundedLayer = 1<<11 | 1<<12 | 1<<16; //VFVoxelterrain & SceneStatic & Unwalkable
    public const int GroundItemLayer = 1 << 13 | 1 << 11 | 1 << 16;
    public const int PlayerLayer = 1<<10 | 1<<8;
    public const int CreationLayer = 1 << 19;
    public const int TreeLayer = 1 << 14 | 1 << 21;
	public const int WaterLayer = 1 << 4 | 1<< 20;

    public const int CharacterLayer = PlayerLayer | CreationLayer;
    public const int ObstacleLayer = GroundItemLayer | TreeLayer;
    public const int BlockLayer = ObstacleLayer | GroundedLayer;
}
