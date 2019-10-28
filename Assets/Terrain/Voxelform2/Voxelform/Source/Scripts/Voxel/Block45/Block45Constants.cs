// choose to define only one of the two:
#define USE_HALF_METER_VOXEL
//#define USE_QUARTER_METER_VOXEL

public class Block45Constants
{
	// NOW voxel terrain mesh has an offset of (-1,-1,-1), all code on this offset has noted with VOXEL_OFFSET
	// constant on display 
	public const int MAX_LOD = 4;	//

	public const int _shift = 3;
	public const int _mask = (1<<_shift)-1;
	/*
	 * 3->8
	 * 4->16
	 * 5->32
	*/
	
	public const int _numVoxelsPerAxis = (1<<_shift);
	public const int _numVoxelsPrefix = 1;	// 0 if no prefix
	public const int _numVoxelsPostfix = 1;	// 0 if no postfix

	public const int _shiftToScale = -1;
	public const int _scaledShift = _shift+_shiftToScale;
	public const int _scaledSize = 1<<_scaledShift;

	public const int ChunkPhysicalSize = 1<<_scaledShift;
	public static int Size(int lod){		return 1<<(_scaledShift+lod);	}
	public static int CenterOfs(int lod){ 	return 1<<(_scaledShift+lod-1);	}
#if USE_HALF_METER_VOXEL
	public const float _scale = 0.5f;
	public const int _scaleInverted = 2;
#elif USE_QUARTER_METER_VOXEL
	public const float _scale = 0.25f;
	public const int _scaleInverted = 4;
#else
	public const float _scale = 1;
	public const int _scaleInverted = 1;
#endif

	public const int VOXEL_ARRAY_AXIS_SIZE = _numVoxelsPerAxis+_numVoxelsPrefix+_numVoxelsPostfix;
	public const int VOXEL_ARRAY_AXIS_SQUARED = VOXEL_ARRAY_AXIS_SIZE*VOXEL_ARRAY_AXIS_SIZE;
	public const int VOXEL_ARRAY_LENGTH = VOXEL_ARRAY_AXIS_SQUARED*VOXEL_ARRAY_AXIS_SIZE;
	public const int VOXEL_ARRAY_LENGTH_VT = VOXEL_ARRAY_LENGTH * B45Block.Block45Size;
	
	public const int _worldSideLenX = 20100;
    public const int _worldSideLenY = 2048;
	public const int _worldSideLenZ = 20100;
	public const int _worldMaxCX = _worldSideLenX>>_shift;
    public const int _worldMaxCY = _worldSideLenY>>_shift;
    public const int _worldMaxCZ = _worldSideLenZ>>_shift;
	
#if USE_HALF_METER_VOXEL
	public const int _worldSideLenXInVoxels = 512;
    public const int _worldSideLenYInVoxels = 128;
    public const int _worldSideLenZInVoxels = 512;
#else
	public const int _worldSideLenXInVoxels = 256;
    public const int _worldSideLenYInVoxels = 64;
    public const int _worldSideLenZInVoxels = 256;
#endif
	
	public const int _MeshDistanceX = 256;
	public const int _MeshDistanceY = 256;
	public const int _MeshDistanceZ = 256;
	
	public const int _LodMaxX = 256;
	public const int _LodMaxY = 256;
	public const int _LodMaxZ = 256;
	
	public const int MaxMaterialCount = 256;
	
	// number of chunks to compute in each frame.
	public const int NumChunksPerFrame = 30;

    //Other Utils
    public static IntVector4 ToWorldUnitPos(int x, int y, int z, int lod)
    {
        IntVector4 temp = IntVector4.Zero;
        //if (Block45Constants._shiftToScale >= 0)
        //{
        //    temp = new IntVector4(x >> Block45Constants._shiftToScale,
        //                   y >> Block45Constants._shiftToScale,
        //                   z >> Block45Constants._shiftToScale,
        //                   lod);
        //}
        //else
        {
            temp = new IntVector4(x >> -Block45Constants._shiftToScale,
                               y >> -Block45Constants._shiftToScale,
                               z >> -Block45Constants._shiftToScale,
                               lod);
        }
        return temp;
    }
	public static IntVector4 ToBlockUnitPos(int x, int y, int z, int lod)
	{
        //if(Block45Constants._shiftToScale >= 0)
        //{
        //    return new IntVector4(x << Block45Constants._shiftToScale,
        //                   y << Block45Constants._shiftToScale,
        //                   z << Block45Constants._shiftToScale,
        //                   lod);
        //}
        //else
        {
            return new IntVector4(x << -Block45Constants._shiftToScale,
                               y << -Block45Constants._shiftToScale,
                               z << -Block45Constants._shiftToScale,
                               lod);
        }
	}	
}
