#define USE_ZIPPED_DATA
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;


public class VFPieceDataClone
{
#region LZ4_DLL
    [DllImport("lz4_dll")]
    public static extern int LZ4_DllLoad();
    [DllImport("lz4_dll")]
    public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);
    [DllImport("lz4_dll")]
    public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);
    [DllImport("lz4_dll")]
    public static extern int LZ4_uncompress_unknownOutputSize(byte[] source, byte[] dest, int isize, int maxOutputSize);
#endregion
	public static byte[] unzippedDataBuffer = new byte[VoxelTerrainConstants.VOXEL_NUM_PER_PIECE*VFVoxel.c_VTSize + 256]; // Plus offset data
	public static int unzippedDataLen = 0;
	public static IntVector4 unzippedDataDesc = new IntVector4(-1,-1,-1,-1);
	
	public IntVector3 _pos = new IntVector3(-1,-1,-1);
	public int _lod;
	public byte[] _data;
	
	public static bool Match(VFPieceDataClone pieceData, IntVector3 piecePos, int lod){
		return pieceData._pos.Equals(piecePos) && pieceData._lod == lod;
	}
	
	public bool IsHollow(){
		return _data.Length == VFVoxel.c_VTSize;
	}
	public void Decompress()
	{
#if USE_ZIPPED_DATA
		if(IsHollow())
			return;
		
		if(unzippedDataDesc.x == _pos.x && unzippedDataDesc.y == _pos.y && 
			unzippedDataDesc.z == _pos.z && unzippedDataDesc.w == _lod)
			return;
		
		//int curTick = Environment.TickCount;
		unzippedDataLen = LZ4_uncompress_unknownOutputSize(_data, unzippedDataBuffer, _data.Length, unzippedDataBuffer.Length);
		if(unzippedDataLen < 0)
		{
			Debug.LogError("[VFDATAReaderClone]Failed to decompress vfdata." + "@"+unzippedDataDesc);
		}
		//Debug.Log("Decompress(LZ4DLL) use " + (Environment.TickCount - curTick) + "ms " );
		unzippedDataDesc.x = _pos.x;
		unzippedDataDesc.y = _pos.y;
		unzippedDataDesc.z = _pos.z;
		unzippedDataDesc.w = _lod;
#endif
	}
	public void SetChunkData(VFVoxelChunkData chunkData)
	{
		byte[] chunkDataVT;
		if(IsHollow()){
			chunkDataVT = new byte[VFVoxel.c_VTSize];
			chunkDataVT[0] = _data[0];
			chunkDataVT[1] = _data[1];
			chunkData.SetDataVT(chunkDataVT);
			return;
		}
		
#if USE_ZIPPED_DATA
		byte[] chunkDataSet = unzippedDataBuffer;
		int lenChunkDataSet = unzippedDataLen;
		int ofsDataAddr = 0;
#else
		byte[] chunkDataSet = data;
		int lenChunkDataSet = (int)data.Length;
		int ofsDataAddr = 13;
#endif
        int xIndexLocal = (chunkData.ChunkPosLod.x>>_lod) % VoxelTerrainConstants._xChunkPerPiece;
        int yIndexLocal = (chunkData.ChunkPosLod.y>>_lod) % VoxelTerrainConstants._yChunkPerPiece;
        int zIndexLocal = (chunkData.ChunkPosLod.z>>_lod) % VoxelTerrainConstants._zChunkPerPiece;
		int idxChunk = (VoxelTerrainConstants._zChunkPerPiece * VoxelTerrainConstants._yChunkPerPiece * xIndexLocal +
                    	VoxelTerrainConstants._zChunkPerPiece * yIndexLocal +
                    	zIndexLocal);
		int idChunkMax = VoxelTerrainConstants._ChunksPerPiece - 1;
		ofsDataAddr += 4*idxChunk;
		
		int vxDataAddr = (chunkDataSet[ofsDataAddr])+(chunkDataSet[ofsDataAddr+1]<<8)+(chunkDataSet[ofsDataAddr+2]<<16)+(chunkDataSet[ofsDataAddr+3]<<24);
		int vxDataNextAddr = (idxChunk < idChunkMax) ? (chunkDataSet[ofsDataAddr+4])+(chunkDataSet[ofsDataAddr+5]<<8)+(chunkDataSet[ofsDataAddr+6]<<16)+(chunkDataSet[ofsDataAddr+7]<<24)
												: lenChunkDataSet;
		int dataLen = vxDataNextAddr - vxDataAddr;
		chunkDataVT = new byte[dataLen];
		Array.Copy(chunkDataSet, vxDataAddr, chunkDataVT, 0, dataLen);
		chunkData.SetDataVT(chunkDataVT);
	}
}
public class VFFileDataClone
{
	public const int FSetDataCountXZ = VoxelTerrainConstants._mapPieceCountXorZ;
	public const int FSetDataCountY = VoxelTerrainConstants._mapPieceCountY;
	public const int FSetOfsDataLen = FSetDataCountXZ*FSetDataCountXZ*FSetDataCountY*4;
	public static byte[] ofsData = new byte[FSetOfsDataLen];	// tmp 

	public FileStream fs;
	public int len;
	public IntVector3 _pos = new IntVector3(-1,-1,-1);
	public int _lod;
		
	public void GetOfs(IntVector4 piecePos, out int pieceDataOfs, out int pieceDataLen)
	{
#if USE_ZIPPED_DATA
		// TODO: optimization
		int ofsx = (piecePos.x%VoxelTerrainConstants._mapPieceCountXorZ)>>_lod;
		int ofsz = (piecePos.z%VoxelTerrainConstants._mapPieceCountXorZ)>>_lod;
		int ofsy = (piecePos.y)>>_lod;
		
		// X,Z,then Y ----> correspongding to data making
		int idx = (ofsx*(FSetDataCountXZ>>_lod) + ofsz)*(FSetDataCountY>>_lod) + ofsy;
		idx *= 4;
		pieceDataOfs = ofsData[idx] + (ofsData[idx+1]<<8) + (ofsData[idx+2]<<16) + (ofsData[idx+3]<<24);
		int fSetOfsDataLenLod = (VFFileDataClone.FSetDataCountXZ>>_lod)*
								(VFFileDataClone.FSetDataCountXZ>>_lod)*
								(VFFileDataClone.FSetDataCountY >>_lod)*4;
		if(idx >= fSetOfsDataLenLod-4)
		{
			pieceDataLen = len - pieceDataOfs;
		}
		else
		{
			pieceDataLen = ofsData[idx+4] + (ofsData[idx+5]<<8) + (ofsData[idx+6]<<16) + (ofsData[idx+7]<<24) - pieceDataOfs;
		}
#else
		pieceDataOfs = 0;
		pieceDataLen = len;
#endif
	}
	
	public static void PiecePos2FileIndex(IntVector4 piecePos, out IntVector4 fileIndex)
	{
#if USE_ZIPPED_DATA
		fileIndex = new IntVector4(	piecePos.x/VoxelTerrainConstants._mapPieceCountXorZ,
									0,
									piecePos.z/VoxelTerrainConstants._mapPieceCountXorZ,
									piecePos.w);
#else		
		fileIndex = new IntVector4(piecePos);
#endif
	}	
	public static void WorldChunkPosToPiecePos(IntVector4 chunkPos,	out IntVector4 piecePos)
    {
		int lod = chunkPos.w;
		int shift = lod + VoxelTerrainConstants._mapPieceChunkShift;
		piecePos = new IntVector4(	(chunkPos.x>>shift)<<lod,
		                          (chunkPos.y>>shift)<<lod,
		                          (chunkPos.z>>shift)<<lod,
		                          chunkPos.w);
    }	
	public static bool Match(VFFileDataClone fileData, IntVector4 fileIndex)
	{
		return fileData._pos.Equals(fileIndex.XYZ) && fileData._lod == fileIndex.w;
	}
}

// Now all lod data is loaded with this reader, in future this reader can be devided to corresponding each lod.
public static class VFDataReaderClone
{
	//const int fileLoadPerFrame = 2;
	//const int fileSetCacheW = 3;
	//const int fileDataCacheW = 3;
	//private static List<VFFileDataClone> fileSetCache = new List<VFFileDataClone>();
	//private static List<VFPieceDataClone> pieceDataCache = new List<VFPieceDataClone>();

	public static VFFileDataClone GetFileSetSub(IntVector4 fileIndex)
	{
		int lod = fileIndex.w;
		VFFileDataClone fileData = new VFFileDataClone();
		fileData._pos.x = fileIndex.x;
		fileData._pos.y = fileIndex.y;
		fileData._pos.z = fileIndex.z;
		fileData._lod   = lod;
        if (string.IsNullOrEmpty(VFVoxelTerrain.MapDataPath_Zip))
        {
            fileData.fs = null;
        }
        else
        {
#if USE_ZIPPED_DATA
            string fileName = VFVoxelTerrain.MapDataPath_Zip + "/map_x" + fileIndex.x + "_y" + fileIndex.z + (lod != 0 ? ("_" + lod + ".voxelform") : ".voxelform");
            try
            {
                int fSetOfsDataLenLod = (VFFileDataClone.FSetDataCountXZ >> lod) * (VFFileDataClone.FSetDataCountXZ >> lod) * (VFFileDataClone.FSetDataCountY >> lod) * 4;
                fileData.fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                fileData.len = (int)fileData.fs.Length;
                fileData.fs.Read(VFFileDataClone.ofsData, 0, fSetOfsDataLenLod);
            }
            catch (Exception)
            {
                Debug.LogError("[VFDataReader] Failed to read " + fileName);
                fileData.fs = null;
            }
#else
		    string fileName = GameConfig.MapDataPath_Raw + "/map_x" + fileIndex.x + "y" + fileIndex.z + "h" + fileIndex.y + "l0.voxelform";
		    fileData.fs = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		    fileData.len = (int)fileSet.fs.Length;
#endif
        }
		return fileData;
	}
	public static VFPieceDataClone GetPieceDataSub(IntVector4 piecePos, VFFileDataClone fileSet)
	{
		VFPieceDataClone pieceData = new VFPieceDataClone();
		pieceData._pos.x = piecePos.x;
		pieceData._pos.y = piecePos.y;
		pieceData._pos.z = piecePos.z;
		pieceData._lod = fileSet._lod;
		
		if(fileSet.fs == null)
		{
			pieceData._data = new byte[VFVoxel.c_VTSize];
		}
		else
		{
			int ofs, len;
			fileSet.GetOfs(piecePos, out ofs, out len);
			pieceData._data = new byte[len];
			fileSet.fs.Seek(ofs, SeekOrigin.Begin);
			fileSet.fs.Read(pieceData._data, 0, len);
			fileSet.fs.Close();
		}
		return pieceData;
	}
	public static bool GetPieceData(out VFPieceDataClone pieceData, IntVector4 piecePos)	// ret if cache hit
	{
		IntVector4 fileIndex;
		VFFileDataClone.PiecePos2FileIndex(piecePos, out fileIndex);
		VFFileDataClone fileSet = null;//fileSetCache.Find(iter=>VFFileDataClone.Match(iter, fileIndex));
		//if(fileSet == null)
		{
			fileSet = GetFileSetSub(fileIndex);
			//fileSetCache.Add(fileSet);
		}
		
		pieceData = GetPieceDataSub(piecePos, fileSet);
		fileSet = null;
		//fileDataCache.Add(fileData);
		return false;
	}

	public static void AddReadRequest(VFVoxelChunkData chunkData)
	{
		IntVector4 chunkPos = chunkData.ChunkPosLod;
		// Req to chunks out of range will return, these chunks will be null and can not be write according to WriteVoxelAtIdx's code
		if(chunkPos.x < 0 || chunkPos.x >= VoxelTerrainConstants._worldMaxCX)				return;
		if(chunkPos.y < 0 || chunkPos.y >= VoxelTerrainConstants.WorldMaxCY(chunkPos.w))	return;
		if(chunkPos.z < 0 || chunkPos.z >= VoxelTerrainConstants._worldMaxCZ)				return;

		// no caching
		IntVector4 piecePos;
		VFFileDataClone.WorldChunkPosToPiecePos(chunkPos, out piecePos);
		
		IntVector4 fileIndex;
		VFFileDataClone.PiecePos2FileIndex(piecePos, out fileIndex);
		
		VFPieceDataClone pieceData = GetPieceDataSub(piecePos, GetFileSetSub(fileIndex));
		pieceData.Decompress();
		pieceData.SetChunkData(chunkData);
		pieceData._data = null;
		pieceData = null;
	}
}
