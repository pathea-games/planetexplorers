using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
//TODO: use memory mapped file
//TODO: avoid switching from one file to another and vice versa

public delegate void ChunkDataLoadedProcessor(VFVoxelChunkData chunkData, byte[] chunkDataVT, bool fromPool);

public static class LZ4
{
	[DllImport("lz4_dll")]
	public static extern int LZ4_DllLoad();
	[DllImport("lz4_dll")]
	public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);
	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);
	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress_unknownOutputSize(byte[] source, byte[] dest, int isize, int maxOutputSize);
}

public class VFPieceUnzipBuffer
{
	// worst size 100.4%, with "0.4%" being at least 8 bytes.
	public byte[] zippedDataBuffer = new byte[(int)((VoxelTerrainConstants.VOXEL_NUM_PER_PIECE*VFVoxel.c_VTSize+256)*1.004f)]; 
	public byte[] unzippedDataBuffer = new byte[VoxelTerrainConstants.VOXEL_NUM_PER_PIECE*VFVoxel.c_VTSize + 256]; // Plus offset data
	public int zippedDataLen = 0;
	public int unzippedDataLen = 0;
	public IntVector4 unzippedDataDesc = new IntVector4(-1,-1,-1,-1);
	public bool IsHollow(){
		return zippedDataLen == VFVoxel.c_VTSize;
	}
	public bool IsHitCache(int px, int py, int pz, int lod)
	{
		return (unzippedDataDesc.x == px && unzippedDataDesc.y == py && 
		        unzippedDataDesc.z == pz && unzippedDataDesc.w == lod);
	}
	public void Decompress(int px, int py, int pz, int lod)
	{
		if(IsHollow() || IsHitCache(px, py, pz, lod))
			return;

		unzippedDataLen = LZ4.LZ4_uncompress_unknownOutputSize(zippedDataBuffer, unzippedDataBuffer, zippedDataLen, unzippedDataBuffer.Length);
		if(unzippedDataLen < 0)	{
			Debug.LogError("[VFDATAReader]Failed to decompress vfdata." + "@"+px + py + pz + lod);
		} else {
			unzippedDataDesc.x = px;
			unzippedDataDesc.y = py;
			unzippedDataDesc.z = pz;
			unzippedDataDesc.w = lod;
		}
	}
	
	public void SetChunkData(VFVoxelChunkData chunkData, ChunkDataLoadedProcessor chunkDataProc)
	{
		byte[] chunkDataVT;
		//bool bFromPool;
		byte vol, typ;
		if(IsHollow()){
			vol = zippedDataBuffer[0];
			typ = zippedDataBuffer[1];
			if(vol == 0)					chunkDataVT = VFVoxelChunkData.S_ChunkDataAir;
			else if(vol == 128)				chunkDataVT = VFVoxelChunkData.S_ChunkDataWaterPlane;
			else if(vol == 255)				chunkDataVT = VFVoxelChunkData.S_ChunkDataSolid[typ];
			else  							chunkDataVT = new byte[VFVoxel.c_VTSize]{vol, typ};

			if(chunkDataProc == null)		chunkData.OnDataLoaded(chunkDataVT, false);
			else 							chunkDataProc(chunkData, chunkDataVT, false);
			return;
		}
		
		byte[] chunkDataSet = unzippedDataBuffer;
		int lenChunkDataSet = unzippedDataLen;
		int ofsDataAddr = 0;
		int idxChunk = VFFileUtil.ChunkPos2IndexInPiece(chunkData.ChunkPosLod);
		int idChunkMax = VoxelTerrainConstants._ChunksPerPiece - 1;
		ofsDataAddr += 4*idxChunk;
		
		try{
			int vxDataAddr = (chunkDataSet[ofsDataAddr])+(chunkDataSet[ofsDataAddr+1]<<8)+(chunkDataSet[ofsDataAddr+2]<<16)+(chunkDataSet[ofsDataAddr+3]<<24);
			int vxDataNextAddr = (idxChunk < idChunkMax) ? (chunkDataSet[ofsDataAddr+4])+(chunkDataSet[ofsDataAddr+5]<<8)+(chunkDataSet[ofsDataAddr+6]<<16)+(chunkDataSet[ofsDataAddr+7]<<24)
				: lenChunkDataSet;
			int dataLen = vxDataNextAddr - vxDataAddr;
			if(dataLen == VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT){
				chunkDataVT = VFVoxelChunkData.s_ChunkDataPool.Get();
				Array.Copy(chunkDataSet, vxDataAddr, chunkDataVT, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
				if(chunkDataProc == null)	chunkData.OnDataLoaded(chunkDataVT, true);
				else 						chunkDataProc(chunkData, chunkDataVT, true);
			} else if(dataLen == VFVoxel.c_VTSize) {
				vol = chunkDataSet[vxDataAddr];
				typ = chunkDataSet[vxDataAddr+1];
				if(vol == 0)				chunkDataVT = VFVoxelChunkData.S_ChunkDataAir;
				else if(vol == 128)			chunkDataVT = VFVoxelChunkData.S_ChunkDataWaterPlane;
				else if(vol == 255)			chunkDataVT = VFVoxelChunkData.S_ChunkDataSolid[typ];
				else 						chunkDataVT = new byte[VFVoxel.c_VTSize]{vol, typ};

				if(chunkDataProc == null)	chunkData.OnDataLoaded(chunkDataVT, false);
				else 						chunkDataProc(chunkData, chunkDataVT, false);
			} else {
				Debug.LogWarning("[VFDATAReader]Unsupported data length("+dataLen+")@"+chunkData.ChunkPosLod);
			}
		} catch {
			Debug.LogWarning("[VFDATAReader]Failed to read Chunk"+chunkData.ChunkPosLod);
		}
	}
}

class VFFileUtil
{
	public const int OfsDataLen = VoxelTerrainConstants._mapPieceCountXYZ*4;
	public const int MaxFileIndex = VoxelTerrainConstants._mapFileCountX * VoxelTerrainConstants._mapFileCountZ * (LODOctreeMan.MaxLod+1);
	public static void PreOpenFile(string strDataFilePrefix, int x, int z, int lod, out FileStream file, out long len)
	{
		file = null;
		len = 0;
		string fileName = strDataFilePrefix + "_x" + x + "_y" + z + (lod != 0 ? ("_" + lod + ".voxelform") : ".voxelform");
		try	{
			file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			len = file.Length;
		} catch (Exception)	{
			Debug.LogWarning("[VFDataReader] Failed to read " + fileName);
		}
	}
	public static void PreOpenAllFiles(string strDataFilePrefix, out FileStream[] fileStreams, out long[] fileLens)
	{
		fileLens = new long[MaxFileIndex];
		fileStreams = new FileStream[MaxFileIndex];

		if (string.IsNullOrEmpty (strDataFilePrefix)) {
			Debug.LogError ("[VFDataReader]Failed to open file because DataFilePrefix is null");
		} else {
			int idx = 0;
			for (int lod = 0; lod <= LODOctreeMan.MaxLod; lod++) {
				for (int z = 0; z < VoxelTerrainConstants._mapFileCountZ; z++) {
					for (int x = 0; x < VoxelTerrainConstants._mapFileCountX; x++) {
						PreOpenFile (strDataFilePrefix, x, z, lod, out fileStreams [idx], out fileLens [idx]);
						idx++;
					}
				}
			}
		}
	}
	public static void CloseAllFiles(ref FileStream curFileStream, ref FileStream[] fileStreams, ref long[] fileLens)
	{
		if (fileStreams != null) {
			if(fileStreams.Contains(curFileStream)){
				curFileStream = null;
			}
			foreach (FileStream fs in fileStreams) {
				if (fs != null) {
					fs.Close ();
				}
			}
			fileStreams = null;
		}
		if(curFileStream != null){
			curFileStream.Close ();
			curFileStream = null;
		}
		fileLens = null;		
	}
	public static void PiecePos2FilePos(int px, int py, int pz, int lod, out int fx, out int fz)
	{
		fx = px / VoxelTerrainConstants._mapPieceCountXorZ;
		fz = pz / VoxelTerrainConstants._mapPieceCountXorZ;
	}	
	public static void WorldChunkPosToPiecePos(IntVector4 chunkPos,	out int px, out int py, out int pz)
    {
		int shift = chunkPos.w + VoxelTerrainConstants._mapPieceChunkShift;
		px = (chunkPos.x >> shift) << chunkPos.w;
		py = (chunkPos.y >> shift) << chunkPos.w;
		pz = (chunkPos.z >> shift) << chunkPos.w;
    }
	public static int ChunkPos2IndexInPiece(IntVector4 chunkPos)
	{
		int lod = chunkPos.w;
		int mask = (1<<VoxelTerrainConstants._mapPieceChunkShift)-1;
		int xIndexLocal = (chunkPos.x>>lod)&mask;
		int yIndexLocal = (chunkPos.y>>lod)&mask;
		int zIndexLocal = (chunkPos.z>>lod)&mask;
		return (VoxelTerrainConstants._zChunkPerPiece * VoxelTerrainConstants._yChunkPerPiece * xIndexLocal +
		        VoxelTerrainConstants._zChunkPerPiece * yIndexLocal + zIndexLocal);
	}
	public static int PiecePos2IndexInFile(int px, int py, int pz, int lod)
	{
		int ofsx = (px%VoxelTerrainConstants._mapPieceCountXorZ)>>lod;
		int ofsz = (pz%VoxelTerrainConstants._mapPieceCountXorZ)>>lod;
		int ofsy = (py)>>lod;
		// X,Z,then Y ----> correspongding to data making
		return (ofsx*(VoxelTerrainConstants._mapPieceCountXorZ>>lod) + ofsz)*(VoxelTerrainConstants._mapPieceCountY>>lod) + ofsy;
	}
}

// Now all lod data is loaded with this reader, in future this reader can be devided to corresponding each lod.
public class VFDataReader : IVxDataLoader
{
	struct ReqValue
	{
		public int px, py, pz, pw;//piecePos
		public int stamp;
		public bool PosEqual(int x, int y, int z, int w){
			return x == px && y == py && z == pz && w == pw;
		}
	}

	private Dictionary<VFVoxelChunkData, ReqValue> _chunkReqList = new Dictionary<VFVoxelChunkData, ReqValue>();						// value is stamp+piecePos, key is chunk
	private bool _bImmMode = true;	// default true because AddRequest already in thread
	private string _dataFilePrefix;
	private long[] _fileLens = null;
	private FileStream[] _fileStreams = null;
	private ChunkDataLoadedProcessor _chunkDataProc = null;
	public VFDataReader(string dataFilePrefix, ChunkDataLoadedProcessor chunkDataProc = null, bool bPreopenFiles = true){
		_dataFilePrefix = dataFilePrefix;
		_chunkDataProc  = chunkDataProc;
		if (bPreopenFiles) {
			VFFileUtil.PreOpenAllFiles (_dataFilePrefix, out _fileStreams, out _fileLens);
		}
	}

	public VFPieceUnzipBuffer _buff = new VFPieceUnzipBuffer();

	private long _curFileLen = 0;
	private FileStream _curFileStream = null;
	private int _curFileX, _curFileZ, _curFileLod = -1;
	private byte[] _ofsData = new byte[VFFileUtil.OfsDataLen];	// use the max of all lod's ofsdata len
	private void GetPieceOfsNLen(long fileLen, int px, int py, int pz, int lod, out int pieceDataOfs, out int pieceDataLen)
	{
		int idx = VFFileUtil.PiecePos2IndexInFile(px, py, pz, lod)*4; //in byte
		pieceDataOfs = _ofsData[idx] + (_ofsData[idx+1]<<8) + (_ofsData[idx+2]<<16) + (_ofsData[idx+3]<<24);
		int fSetOfsDataLenLod = (VoxelTerrainConstants._mapPieceCountXorZ>>lod)*(VoxelTerrainConstants._mapPieceCountXorZ>>lod)*(VoxelTerrainConstants._mapPieceCountY>>lod)*4;
		if(idx >= fSetOfsDataLenLod-4) {
			pieceDataLen = (int)(fileLen - pieceDataOfs);
		} else {
			pieceDataLen = _ofsData[idx+4] + (_ofsData[idx+5]<<8) + (_ofsData[idx+6]<<16) + (_ofsData[idx+7]<<24) - pieceDataOfs;
		}
	}
	private void FetchFile(int fx, int fz, int lod)
	{
		if (fx != _curFileX || fz != _curFileZ || lod != _curFileLod) {
			if (_fileStreams != null) {
				int idx = lod * VoxelTerrainConstants._mapFileCountX * VoxelTerrainConstants._mapFileCountZ + fz * VoxelTerrainConstants._mapFileCountX + fx;
				_curFileStream = _fileStreams [idx];
				_curFileLen = _fileLens [idx];
			} else {
				VFFileUtil.PreOpenFile(_dataFilePrefix, fx, fz, lod, out _curFileStream, out _curFileLen);
			}
			
			if (_curFileStream != null) {
				int ofsDataLenLod = (VoxelTerrainConstants._mapPieceCountXorZ >> lod) * (VoxelTerrainConstants._mapPieceCountXorZ >> lod) * (VoxelTerrainConstants._mapPieceCountY >> lod) * 4;
				_curFileStream.Seek(0, SeekOrigin.Begin);
				_curFileStream.Read(_ofsData, 0, ofsDataLenLod);
			}
			_curFileX = fx;
			_curFileZ = fz;
			_curFileLod = lod;
		}
	}
	private void ReadPieceDataToBuff(int px, int py, int pz, int lod, int fx, int fz)
	{
		FetchFile (fx, fz, lod);
		if(_curFileStream == null){
			_buff.zippedDataLen = VFVoxel.c_VTSize;
			_buff.zippedDataBuffer[0] = 0;
			_buff.zippedDataBuffer[1] = 0;
		} else {
			int ofs, len;
			GetPieceOfsNLen(_curFileLen, px, py, pz, lod, out ofs, out len);
			_buff.zippedDataLen = len;
			_curFileStream.Seek(ofs, SeekOrigin.Begin);
			_curFileStream.Read(_buff.zippedDataBuffer, 0, _buff.zippedDataLen);
		}
	}
	public void ReadPieceDataToBuff(int px, int py, int pz, int lod)	// ret if cache hit
	{
		int fx, fz;
		VFFileUtil.PiecePos2FilePos(px, py, pz, lod, out fx, out fz);
		ReadPieceDataToBuff(px, py, pz, lod, fx, fz);
	}

	// Interface implementation
	public bool IsIdle{			get{	return _chunkReqList.Count == 0;				}	}
	public bool ImmMode{		get{	return _bImmMode;	} set{ _bImmMode = value; 	}	}
	public void Close()	{		VFFileUtil.CloseAllFiles (ref _curFileStream, ref _fileStreams, ref _fileLens);	}
	public void AddRequest(VFVoxelChunkData chunkData)
	{
		IntVector4 chunkPos = chunkData.ChunkPosLod;
		// Req to chunks out of range will return, these chunks will be null and can not be write according to WriteVoxelAtIdx's code
		if(	chunkPos.x < 0 || chunkPos.x >= VoxelTerrainConstants._worldMaxCX ||
			chunkPos.y < 0 || chunkPos.y >= VoxelTerrainConstants.WorldMaxCY(chunkPos.w) ||
			chunkPos.z < 0 || chunkPos.z >= VoxelTerrainConstants._worldMaxCZ)
		{
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataAir);
			return;
		}

		if (_bImmMode) {// no caching
			int px, py, pz;
			VFFileUtil.WorldChunkPosToPiecePos (chunkPos, out px, out py, out pz);
			ReadPieceDataToBuff (px, py, pz, chunkPos.w);
			_buff.Decompress (px, py, pz, chunkPos.w);
			_buff.SetChunkData (chunkData, _chunkDataProc);
		} else {
			ReqValue reqValue;
			if(!_chunkReqList.TryGetValue(chunkData, out reqValue) || !chunkData.IsStampIdentical(reqValue.stamp)){

				VFFileUtil.WorldChunkPosToPiecePos(chunkPos, out reqValue.px, out reqValue.py, out reqValue.pz);
				reqValue.pw = chunkPos.w;
				reqValue.stamp = chunkData.StampOfUpdating;
				_chunkReqList[chunkData] = reqValue;
			}
		} 
	}	
	public void ProcessReqs()
	{
		if (_chunkReqList.Count == 0)
			return;

		List<VFVoxelChunkData> keys = _chunkReqList.Keys.Cast<VFVoxelChunkData>().ToList();
		VFVoxelChunkData chnk;
		ReqValue reqv;
		int px;
		int py;
		int pz;
		int pw;
		int stamp;
		int n;
		do {
			n = keys.Count;
			chnk = keys [n - 1];
			keys.RemoveAt (n - 1);

			reqv = _chunkReqList [chnk];
			px = reqv.px;
			py = reqv.py;
			pz = reqv.pz;
			pw = reqv.pw;
			stamp = reqv.stamp;
			if (chnk.IsStampIdentical (stamp)) {
				ReadPieceDataToBuff (px, py, pz, pw);
				_buff.Decompress (px, py, pz, pw);

				_buff.SetChunkData (chnk, _chunkDataProc);
				for (int i = n-2; i >= 0; i--) {
					chnk = keys [i];
					reqv = _chunkReqList [chnk];
					if (reqv.PosEqual(px, py, pz, pw)) {
						keys.RemoveAt (i);
						if (chnk.IsStampIdentical (reqv.stamp)) {
							_buff.SetChunkData (chnk, _chunkDataProc);
						}
					}
				}
			}
		} while(keys.Count > 0);
		_chunkReqList.Clear();
	}

	public delegate byte[] ProcToMergeChunkAtPos(IntVector4 chunkPos, byte[] baseChunkData);
	public void ReplaceChunkDatas(List<IntVector4> newChunkPosList, ProcToMergeChunkAtPos mergeChunkDataAtPos)
	{
		Dictionary<IntVector4, Dictionary<IntVector4, List<IntVector4>>> file2piece2chunkList = new Dictionary<IntVector4, Dictionary<IntVector4, List<IntVector4>>>();
		int n = newChunkPosList.Count;
		int px, py, pz;
		int fx, fz;
		for(int i = 0; i < n; i++)
		{
			IntVector4 chunkPos = newChunkPosList[i];
			VFFileUtil.WorldChunkPosToPiecePos(chunkPos, out px, out py, out pz);			
			VFFileUtil.PiecePos2FilePos(px, py, pz, chunkPos.w, out fx, out fz);

			IntVector4 filePos = new IntVector4(fx, 0, fz, chunkPos.w);
			IntVector4 piecePos = new IntVector4(px, py, pz, chunkPos.w);
			if(!file2piece2chunkList.ContainsKey(filePos)) 
				file2piece2chunkList.Add(filePos, new Dictionary<IntVector4,List<IntVector4>>());
			if(!file2piece2chunkList[filePos].ContainsKey(piecePos))
				file2piece2chunkList[filePos].Add(piecePos, new List<IntVector4>());
			file2piece2chunkList[filePos][piecePos].Add(chunkPos);
		}
		List<IntVector4> fileIndexList = file2piece2chunkList.Keys.Cast<IntVector4>().ToList();
		int nFileIndexList = fileIndexList.Count;
		for(int i = 0; i < nFileIndexList; i++)
		{
			IntVector4 fileIndex = fileIndexList[i];
			ReplacePiecesInFile(file2piece2chunkList[fileIndex], mergeChunkDataAtPos, fileIndex);
		}
	}
	public void ReplacePiecesInFile(Dictionary<IntVector4,List<IntVector4>> piece2chunkList, ProcToMergeChunkAtPos mergeChunkDataAtPos, IntVector4 fileIndex)
	{
		int lod = fileIndex.w;
		string newFileName = _dataFilePrefix + "_x" + fileIndex.x + "_y" + fileIndex.z + (lod != 0 ? ("_" + lod + ".tmp") : ".tmp");
		string fileName = _dataFilePrefix + "_x" + fileIndex.x + "_y" + fileIndex.z + (lod != 0 ? ("_" + lod + ".voxelform") : ".voxelform");
		using(FileStream  fsOld = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using(FileStream  fsNew = new FileStream(newFileName, FileMode.Create))
			{
				int nMaxPieceCount = (VoxelTerrainConstants._mapPieceCountXorZ>>lod)*(VoxelTerrainConstants._mapPieceCountXorZ>>lod)*(VoxelTerrainConstants._mapPieceCountY>>lod);
				int fSetOfsDataLenLod = nMaxPieceCount*4;
				byte[] ofsDataOld = new byte[fSetOfsDataLenLod];
				fsOld.Read(ofsDataOld, 0, fSetOfsDataLenLod);
				byte[] ofsDataNew = new byte[fSetOfsDataLenLod];
				fsNew.Seek(fSetOfsDataLenLod, SeekOrigin.Begin);
				List<IntVector4> piecePosList = piece2chunkList.Keys.Cast<IntVector4>().ToList();
				List<IntVector4> sortedPiecePosList = piecePosList.OrderBy(p=>VFFileUtil.PiecePos2IndexInFile(p.x, p.y, p.z, p.w)).ToList();
				int idx = 0;
				int idxInFile = 0;
				int pieceDataOfs = 0;
				int pieceDataLen = 0;
				int curFilePos = 0;
				byte[] tmpBuff = new byte[(int)((VoxelTerrainConstants.VOXEL_NUM_PER_PIECE*VFVoxel.c_VTSize+256)*1.004f)];
				int n = sortedPiecePosList.Count;
				for(int i = 0; i < n; i++)
				{
					IntVector4 piecePos = sortedPiecePosList[i];
					int idxPiece = VFFileUtil.PiecePos2IndexInFile(piecePos.x, piecePos.y, piecePos.z, piecePos.w);
					while(idxInFile < idxPiece)
					{
						idx = idxInFile*4;
						pieceDataOfs = ofsDataOld[idx] + (ofsDataOld[idx+1]<<8) + (ofsDataOld[idx+2]<<16) + (ofsDataOld[idx+3]<<24);
						if(idx >= fSetOfsDataLenLod-4)
						{
							pieceDataLen = (int)fsOld.Length - pieceDataOfs;
						}
						else
						{
							pieceDataLen = ofsDataOld[idx+4] + (ofsDataOld[idx+5]<<8) + (ofsDataOld[idx+6]<<16) + (ofsDataOld[idx+7]<<24) - pieceDataOfs;
						}
						fsOld.Seek(pieceDataOfs, SeekOrigin.Begin);
						fsOld.Read(tmpBuff, 0, pieceDataLen);
						curFilePos = (int)fsNew.Position;
						ofsDataNew[idx] = (byte)(curFilePos&0xff);
						ofsDataNew[idx+1] = (byte)((curFilePos>>8)&0xff);
						ofsDataNew[idx+2] = (byte)((curFilePos>>16)&0xff);
						ofsDataNew[idx+3] = (byte)((curFilePos>>24)&0xff);
						fsNew.Write(tmpBuff, 0, pieceDataLen);
						idxInFile++;
					}
					ReplaceChunksInPiece(piece2chunkList[piecePos], mergeChunkDataAtPos, piecePos);
					curFilePos = (int)fsNew.Position;
					idx = idxPiece*4;
					ofsDataNew[idx] = (byte)(curFilePos&0xff);
					ofsDataNew[idx+1] = (byte)((curFilePos>>8)&0xff);
					ofsDataNew[idx+2] = (byte)((curFilePos>>16)&0xff);
					ofsDataNew[idx+3] = (byte)((curFilePos>>24)&0xff);
					fsNew.Write(_buff.zippedDataBuffer, 0, _buff.zippedDataLen);
					idxInFile++;
				}
				while(idxInFile < nMaxPieceCount)
				{
					idx = idxInFile*4;
					pieceDataOfs = ofsDataOld[idx] + (ofsDataOld[idx+1]<<8) + (ofsDataOld[idx+2]<<16) + (ofsDataOld[idx+3]<<24);
					if(idx >= fSetOfsDataLenLod-4)
					{
						pieceDataLen = (int)fsOld.Length - pieceDataOfs;
					}
					else
					{
						pieceDataLen = ofsDataOld[idx+4] + (ofsDataOld[idx+5]<<8) + (ofsDataOld[idx+6]<<16) + (ofsDataOld[idx+7]<<24) - pieceDataOfs;
					}
					fsOld.Seek(pieceDataOfs, SeekOrigin.Begin);
					fsOld.Read(tmpBuff, 0, pieceDataLen);
					curFilePos = (int)fsNew.Position;
					ofsDataNew[idx] = (byte)(curFilePos&0xff);
					ofsDataNew[idx+1] = (byte)((curFilePos>>8)&0xff);
					ofsDataNew[idx+2] = (byte)((curFilePos>>16)&0xff);
					ofsDataNew[idx+3] = (byte)((curFilePos>>24)&0xff);
					fsNew.Write(tmpBuff, 0, pieceDataLen);
					idxInFile++;
				}
				fsNew.Seek(0,SeekOrigin.Begin);
				fsNew.Write(ofsDataNew, 0, fSetOfsDataLenLod);
			}
		}
		VFFileUtil.CloseAllFiles (ref _curFileStream, ref _fileStreams, ref _fileLens);

		string oldFileName = string.Format(fileName+".{0:MMdd_hhmmss}", DateTime.Now);
		File.Move(fileName, oldFileName);
		File.Move(newFileName, fileName);
	}
	// Output : _buff.zippedDataBuffer, _buff.zippedDataLen
	public void ReplaceChunksInPiece(List<IntVector4> chunkPosList, ProcToMergeChunkAtPos mergeChunkDataAtPos, IntVector4 piecePos)
	{
		ReadPieceDataToBuff(piecePos.x, piecePos.y, piecePos.z, piecePos.w);
		_buff.Decompress(piecePos.x, piecePos.y, piecePos.z, piecePos.w);

		List<IntVector4> sortedChunkPosList = chunkPosList.OrderBy(pos=>VFFileUtil.ChunkPos2IndexInPiece(pos)).ToList();
		byte[] chunkDataSet = _buff.unzippedDataBuffer;
		int lenChunkDataSet = _buff.unzippedDataLen;
		byte[] hollowData = null;
		if(_buff.IsHollow())
		{
			hollowData = new byte[_buff.zippedDataLen];
			Array.Copy(_buff.zippedDataBuffer, hollowData, _buff.zippedDataLen);
		}
		int idxChunkMax = VoxelTerrainConstants._ChunksPerPiece - 1;
		int idxInPiece = 0;
		List<byte[]> chunkDataList = new List<byte[]>();
		byte[] chunkData = null;
		int sumDataLen = 0;
		int n = sortedChunkPosList.Count;
		for(int i = 0; i < n; i++)
		{
			IntVector4 chunkPos = sortedChunkPosList[i];
			int idxChunk = VFFileUtil.ChunkPos2IndexInPiece(chunkPos);
			while(idxInPiece <= idxChunk)
			{
				if(_buff.IsHollow()){ 
					chunkDataList.Add(hollowData);	
					sumDataLen+=hollowData.Length; 
				}
				else
				{
					int ofsDataAddr = 4*idxInPiece;
					int vxDataAddr = (chunkDataSet[ofsDataAddr])+(chunkDataSet[ofsDataAddr+1]<<8)+(chunkDataSet[ofsDataAddr+2]<<16)+(chunkDataSet[ofsDataAddr+3]<<24);
					int vxDataNextAddr = (idxInPiece < idxChunkMax) ? (chunkDataSet[ofsDataAddr+4])+(chunkDataSet[ofsDataAddr+5]<<8)+(chunkDataSet[ofsDataAddr+6]<<16)+(chunkDataSet[ofsDataAddr+7]<<24)
						: lenChunkDataSet;
					int len = vxDataNextAddr-vxDataAddr;
					chunkData = new byte[len];
					Array.Copy(chunkDataSet, vxDataAddr, chunkData, 0, len);
					chunkDataList.Add(chunkData);
					sumDataLen += len;
				}
				idxInPiece++;
			}
			// merge chunk
			sumDataLen -= chunkDataList[idxChunk].Length;
			chunkDataList[idxChunk] = mergeChunkDataAtPos(chunkPos, chunkDataList[idxChunk]);
			sumDataLen += chunkDataList[idxChunk].Length;
		}
		while(idxInPiece <= idxChunkMax)
		{
			if(_buff.IsHollow()){
				chunkDataList.Add(hollowData);	
				sumDataLen+=hollowData.Length; 
			}
			else
			{
				int ofsDataAddr = 4*idxInPiece;
				int vxDataAddr = (chunkDataSet[ofsDataAddr])+(chunkDataSet[ofsDataAddr+1]<<8)+(chunkDataSet[ofsDataAddr+2]<<16)+(chunkDataSet[ofsDataAddr+3]<<24);
				int vxDataNextAddr = (idxInPiece < idxChunkMax) ? (chunkDataSet[ofsDataAddr+4])+(chunkDataSet[ofsDataAddr+5]<<8)+(chunkDataSet[ofsDataAddr+6]<<16)+(chunkDataSet[ofsDataAddr+7]<<24)
					: lenChunkDataSet;
				int len = vxDataNextAddr-vxDataAddr;
				chunkData = new byte[len];
				Array.Copy(chunkDataSet, vxDataAddr, chunkData, 0, len);
				chunkDataList.Add(chunkData);
				sumDataLen += len;
			}
			idxInPiece++;
		}

		int ofsDataLen = 4*VoxelTerrainConstants._ChunksPerPiece;
		_buff.unzippedDataLen = ofsDataLen+sumDataLen;
		byte[] pieceData = _buff.unzippedDataBuffer;
		int ofs = ofsDataLen;
		for(int i = 0; i < VoxelTerrainConstants._ChunksPerPiece; i++)
		{
			byte[] data = chunkDataList[i];
			Array.Copy(data, 0, pieceData, ofs, data.Length);
			pieceData[i*4] = (byte)(ofs&0xff);
			pieceData[i*4+1] = (byte)((ofs>>8)&0xff);
			pieceData[i*4+2] = (byte)((ofs>>16)&0xff);
			pieceData[i*4+3] = (byte)((ofs>>24)&0xff);
			ofs += data.Length;
		}
		//Compress
		_buff.zippedDataLen = LZ4.LZ4_compress(_buff.unzippedDataBuffer, _buff.zippedDataBuffer, _buff.unzippedDataLen);
	}
	public VFVoxelChunkData ReadChunkImm(IntVector4 cpos) // Read Chunk Data immediately
	{
		VFVoxelChunkData chunkData = new VFVoxelChunkData(null);
		chunkData.ChunkPosLod_w = cpos;
		// Req to chunks out of range will return, these chunks will be null and can not be write according to WriteVoxelAtIdx's code
		if(	cpos.x < 0 || cpos.x >= VoxelTerrainConstants._worldMaxCX ||
		   cpos.y < 0 || cpos.y >= VoxelTerrainConstants.WorldMaxCY(cpos.w) ||
		   cpos.z < 0 || cpos.z >= VoxelTerrainConstants._worldMaxCZ)
		{
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataAir);
			return chunkData;
		}
		int px, py, pz;
		VFFileUtil.WorldChunkPosToPiecePos (cpos, out px, out py, out pz);
		ReadPieceDataToBuff (px, py, pz, cpos.w);
		_buff.Decompress (px, py, pz, cpos.w);
		_buff.SetChunkData (chunkData, _chunkDataProc);
		return chunkData;
	}
	// chunk1 - chunk2
	public static byte[] MinusChunkData(byte[] chunk1, byte[] chunk2, int lod)
	{
		if (chunk1.Length == VFVoxel.c_VTSize) {
			if (chunk1 [0] == 0)	return chunk1;
		}
		if (chunk2.Length == VFVoxel.c_VTSize) {
			if(chunk2[0] == 0)		return chunk1;
			if(chunk2[0] == 255)	return new byte[VFVoxel.c_VTSize];
			Debug.LogWarning("Unrecognized chunk:"+chunk2[0]+","+chunk2[1]);
			return new byte[VFVoxel.c_VTSize];
		}

		byte[] outChunkData = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT]; // default is 0
		if (chunk1.Length != VFVoxel.c_VTSize) {
			int j;
			for (int i = 0; i < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT; i+=VFVoxel.c_VTSize) {
				if(chunk2[i] < 255){
					outChunkData [i] = chunk1[i];
					outChunkData [i+1] = chunk1[i+1];
					if(chunk1[i] < 192) {	// Avoid too thin mesh
						j = i-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
						if(j >= 0){
							outChunkData [j] = chunk1[j];
							outChunkData [j+1] = chunk1[j+1];
						}
					}
				}/* else {	
					j = i+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
					if(j < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT && chunk1[i] != 0 && chunk1[j] == 0){	// surface
						if(chunk2[j] < 128){
							outChunkData [i] = chunk1[i];
							outChunkData [i+1] = chunk1[i+1];
						}
					}
				}*/
			}
		} else if (chunk1 [0] == 255) {
			byte type = chunk1 [1];
			for (int i = 0; i < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT; i+=VFVoxel.c_VTSize) {
				if(chunk2[i] < 255){
					outChunkData [i] = 255;
					outChunkData [i+1] = type;
				}
			}
		} else if (chunk1 [0] == VFVoxelWater.c_iSurfaceVol) {
			float fWaterLvlLod = VFVoxelWater.c_fWaterLvl/(1<<lod);
			int iWaterLvlLod = (int)(fWaterLvlLod+0.5f);
			float fWaterLvlLodDec = fWaterLvlLod - (int)fWaterLvlLod;
			int iy = (iWaterLvlLod&VoxelTerrainConstants._mask) + VoxelTerrainConstants._numVoxelsPrefix;
			byte vol = fWaterLvlLodDec < 0.5f ? (byte)(256.0f*0.5f/(1-fWaterLvlLodDec)) : (byte)(255.999f*(1-0.5f/fWaterLvlLodDec));
			int i = 0;
			int above = (VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - iy - 1) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
			for(int z = 0; z < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; z++)
			{
				// Fill data below surface
				for(int y = 0; y < iy; y++)
				{
					for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++)
					{
						if(chunk2[i] < 255){
							outChunkData[i] = 255;
							outChunkData[i+1] = VFVoxelWater.c_iSeaWaterType;
						}
						i += 2;
					}
				}
				// Fill data of surface
				for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++)
				{
					if(chunk2[i] < 255){
						outChunkData[i] = vol;
						outChunkData[i+1] = VFVoxelWater.c_iSeaWaterType;
					}
					i += 2;
				}
				// default value(0) used for those above surface
				i += above;
			}
		} else {
			Debug.LogWarning ("Unrecognized chunk:" + chunk1 [0] + "," + chunk1 [1]);
			return chunk1;
		}
		return outChunkData;
	}
	// chunk1 + chunk2
	public static byte[] MergeChunkData(byte[] chunk1, byte[] chunk2, int lod)
	{
		if(chunk1.Length != chunk2.Length)
		{
			byte[] chunkShort;
			byte[] chunkLong;
			if(chunk1.Length < chunk2.Length)
			{
				chunkShort = chunk1;
				chunkLong = chunk2;
			}
			else
			{
				chunkShort = chunk2;
				chunkLong = chunk1;
			}
			if(chunkShort[0] == 0)		return chunkLong;
			if(chunkShort[0] == 255)	return chunkShort;
			if(chunkShort[0] != VFVoxelWater.c_iSurfaceVol){
				Debug.LogWarning("Unrecognized chunk:"+chunkShort[0]+","+chunkShort[1]);
				return chunkShort;
			}

			//(volume == VFVoxelWater.c_iSurfaceVol)
			float fWaterLvlLod = VFVoxelWater.c_fWaterLvl/(1<<lod);
			int iWaterLvlLod = (int)(fWaterLvlLod+0.5f);
			float fWaterLvlLodDec = fWaterLvlLod - (int)fWaterLvlLod;
			int iy = (iWaterLvlLod&VoxelTerrainConstants._mask) + VoxelTerrainConstants._numVoxelsPrefix;
			byte vol = fWaterLvlLodDec < 0.5f ? (byte)(256.0f*0.5f/(1-fWaterLvlLodDec)) : (byte)(255.999f*(1-0.5f/fWaterLvlLodDec));
			byte[] outChunkData = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
			Array.Copy(chunkLong, outChunkData, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
			int idx = 0;
			int above = (VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - iy - 1) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
			for(int z = 0; z < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; z++)
			{
				// Fill data below surface
				for(int y = 0; y < iy; y++)
				{
					for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++)
					{
						outChunkData[idx++] = 255;
						outChunkData[idx++] = VFVoxelWater.c_iSeaWaterType;
					}
				}
				// Fill data of surface
				for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++)
				{
					if(chunkLong[idx] > vol)
					{
						idx++;
						idx++;
					}
					else
					{
						outChunkData[idx++] = vol;
						outChunkData[idx++] = VFVoxelWater.c_iSeaWaterType;
					}
				}
				// default value(0) used for those above surface
				idx += above;
			}
			return outChunkData;
		}
		else
		{
			int n = chunk1.Length;
			byte[] outChunkData = new byte[n];
			Array.Copy(chunk1, outChunkData, n);
			for(int i = 0; i < n; i += VFVoxel.c_VTSize)
			{
				if(outChunkData[i] < chunk2[i])
				{
					outChunkData[i] = chunk2[i];
					outChunkData[i+1] = chunk2[i+1];
				}
			}
			return outChunkData;
		}
	}
}
