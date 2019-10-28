//#define USE_SOFTWARE_MC

public class VoxelTerrainConstants
{
	// NOW voxel terrain mesh has an offset of (-1,-1,-1), all code on this offset has noted with VOXEL_OFFSET
	// constant on display 
	// Constant on VFPieceData
#if USE_SOFTWARE_MC
	public const int _shift = 3;	//32	// 16	// 8
#else
	public const int _shift = 5;	//32	// 16	// 8
#endif
	
	public const int _mask = (1<<_shift)-1;
	public const int _nmask = ~_mask;
	public const int _numVoxelsPerAxis = (1<<_shift);
	public const int _numVoxelsPrefix = 1;	// 0 if no prefix
	public const int _numVoxelsPostfix = 2;	// 0 if no postfix
	
	public const float _scale = 1f;
	public const float _isolevel = .5f;
	public const byte _isolevelInByte = 128;
	
	public const int _mapFileWidth = 6144;//2048;
	public const int _mapFileHeight = _worldSideLenY;
	public const int _mapFileCountX = _worldSideLenX / _mapFileWidth;
	public const int _mapFileCountZ = _worldSideLenZ / _mapFileWidth;
	public const int _mapPieceChunkShift = 2;
	public const int _mapPieceShift = _mapPieceChunkShift+_shift;	//128
	public const int _mapPieceSize = (1<<_mapPieceShift);
	public const int _mapPieceCountXorZ = _mapFileWidth/_mapPieceSize;
	public const int _mapPieceCountY = _mapFileHeight/_mapPieceSize;
	public const int _mapPieceCountXYZ = _mapPieceCountXorZ * _mapPieceCountXorZ * _mapPieceCountY;
	public const int _mapChunkCountXorZ = _mapFileWidth>>_shift;
	public const int _mapChunkCountY = _mapFileHeight>>_shift;
	public const int _mapChunkCountXYZ = _mapChunkCountXorZ * _mapChunkCountXorZ * _mapChunkCountY;

    public const float _normalHiltPivotY = 0.1f;
    public const float _spHiltPivotY = 0.12f;
	
	// number of chunks contained in one chunkFile(Piece, lz4 compress data unit) on the x,y,z axis;
    public const int _ChunksPerPiecePerAxis = (1<<_mapPieceChunkShift);	//4
    public const int _xChunkPerPiece = _ChunksPerPiecePerAxis;
    public const int _yChunkPerPiece = _ChunksPerPiecePerAxis;
    public const int _zChunkPerPiece = _ChunksPerPiecePerAxis;
	public const int _ChunksPerPiece = _xChunkPerPiece*_yChunkPerPiece*_zChunkPerPiece;
	
	// does not include the transvoxel cells 
	public const int TRANSVOXEL_EDGE_SIZE = _numVoxelsPerAxis * 2 + 1;
	
	public const int VOXEL_ARRAY_AXIS_SIZE = _numVoxelsPerAxis+_numVoxelsPrefix+_numVoxelsPostfix;
	public const int VOXEL_ARRAY_AXIS_SIZE_VT = VOXEL_ARRAY_AXIS_SIZE*VFVoxel.c_VTSize;
	public const int VOXEL_ARRAY_AXIS_SQUARED = VOXEL_ARRAY_AXIS_SIZE*VOXEL_ARRAY_AXIS_SIZE;
	public const int VOXEL_ARRAY_AXIS_SQUARED_VT = VOXEL_ARRAY_AXIS_SIZE*VOXEL_ARRAY_AXIS_SIZE*VFVoxel.c_VTSize;
	public const int VOXEL_ARRAY_LENGTH = VOXEL_ARRAY_AXIS_SQUARED*VOXEL_ARRAY_AXIS_SIZE;
	public const int VOXEL_ARRAY_LENGTH_VT = VOXEL_ARRAY_LENGTH*VFVoxel.c_VTSize;
	public const int VOXEL_NUM_PER_PIECE = VOXEL_ARRAY_LENGTH*_ChunksPerPiece;
	
	/// world size in chunks.
#if USE_SOFTWARE_MC
	public const int _worldMaxCX = 128;
    public const int _worldMaxCY = 128;
    public const int _worldMaxCZ = 128;
#else
	public const int _worldSideLenX = 18*1024;
    public const int _worldSideLenY = 2944;
    public const int _worldSideLenZ = 18*1024;
	public const int _worldMaxCX = _worldSideLenX>>_shift;
    public const int _worldMaxCY = _worldSideLenY>>_shift;
    public const int _worldMaxCZ = _worldSideLenZ>>_shift;
	public static int WorldMaxCY(int lod){
		int pieceChunkShift = _mapPieceChunkShift+lod;
		return (_worldMaxCY>>pieceChunkShift)<<pieceChunkShift;
	}
#endif
}
