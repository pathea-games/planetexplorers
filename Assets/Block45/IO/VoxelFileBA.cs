using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class VoxelFileBA {
	string fileName;
	FileStream fs;
	byte[] binaryBuffer;
	
	public FileHeader fileHeader;
	
	// inferred variables
	bool chkOfsInVectorForm; // are the chunk offsets in vector form
	//int chkOfsUnitLen;
	
	int chunkCount;
	
	int chunkRawDataLength; // length of an uncompressed raw chunk data
	
	int phase2StartOfs = 0;
#region chunk offsets
	public IntVector3[] chunkCoords;
	int[] chunkOffsets;
	
	// for byte array
	public int[] svn_keys_ba;
	
#endregion
	public VoxelFileBA(string _fileName)
	{
		fileName = _fileName + "_ba.bin";
	}

	public int GetChunkCount()
	{
		return chunkCount;
	}
	public bool ReadHeader()
	{
		try
		{
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		catch(Exception)
		{
			return false;
		}
		if(fs.Length == 0)
		{
			fs.Close();
			return false;
		}
		byte[] tmp = new byte[2];
		fs.Read(tmp,0, 2);
		if((tmp[1] & 1) > 0){
			// compressed
		}
		else{
			binaryBuffer = new byte[fs.Length - 2];
			fs.Read(binaryBuffer,0, (int)fs.Length - 2);
		}
		fs.Close();
		
		fileHeader.cellLength = binaryBuffer[FileHeaderOffsets.cellLength];
		
		fileHeader.chunkType = binaryBuffer[FileHeaderOffsets.chunkType];
		fileHeader.chunkSize = binaryBuffer[FileHeaderOffsets.chunkSize];
		
		fileHeader.chunkPrefix = binaryBuffer[FileHeaderOffsets.chunkPrefix];
		fileHeader.chunkPostfix = binaryBuffer[FileHeaderOffsets.chunkPostfix];
		
		fileHeader.chunkCountX = ByteArrayHelper.to_ushort(binaryBuffer, FileHeaderOffsets.chunkCountX);
		fileHeader.chunkCountY = ByteArrayHelper.to_ushort(binaryBuffer, FileHeaderOffsets.chunkCountY);
		fileHeader.chunkCountZ = ByteArrayHelper.to_ushort(binaryBuffer, FileHeaderOffsets.chunkCountZ);
		
		fileHeader.voxelRes = ByteArrayHelper.to_ushort(binaryBuffer, FileHeaderOffsets.voxelRes);
		
		fileHeader.chunkOffsetDesc = binaryBuffer[FileHeaderOffsets.chunkOffsetDesc];
		
		//chkOfsUnitLen = fileHeader.chunkOffsetDesc & 0x7;
		chkOfsInVectorForm = ((fileHeader.chunkOffsetDesc >> 4) > 0);
		
		chunkRawDataLength = fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.cellLength;
		
		int phase1StartOfs = 0;
			
		if(chkOfsInVectorForm){
			// read the chunk count
			
			chunkCount = ByteArrayHelper.to_int(binaryBuffer, FileHeaderOffsets.headerLength);
			
			phase1StartOfs = FileHeaderOffsets.headerLength + 4;
//			chkOfsLen = chunkCount * (chkOfsUnitLen * 3 + 4);
			phase2StartOfs = phase1StartOfs + chunkCount * ChunkOffsetStructOffsets.length;
			
			chunkCoords = new IntVector3[chunkCount];
			chunkOffsets = new int[chunkCount];
			svn_keys_ba = new int[chunkCount];
			for(int i = 0; i < chunkCount; i++ )
			{
				int chunkOffsetUnitStart = phase1StartOfs + i * ChunkOffsetStructOffsets.length;
			
				chunkCoords[i] = ByteArrayHelper.to_IntVector3(binaryBuffer, chunkOffsetUnitStart);
				chunkOffsets[i] = ByteArrayHelper.to_int(binaryBuffer, chunkOffsetUnitStart + ChunkOffsetStructOffsets.Offset);
				svn_keys_ba[i] = ByteArrayHelper.to_int(binaryBuffer, chunkOffsetUnitStart + ChunkOffsetStructOffsets.SVN_key_ba);
			}
			
		}
		else
		{
			// it's the chunk offset full listing mode
			
		}
		// xyz units	3
		// offset		4
		// svn			4
		// svn_ba		4
		
		/* chunkOffsetDesc tells the length of each vector element.
		 * 
		 * use voxelRes to determine the vector format, or the number of chunks in the full indices mode
		 * voxelRes possible values
		 * 2 : 0.125m
		 * 4 : 0.25m
		 * 8 : 0.5m
		 * 16 : 1m
		 * 32 : 2m
		 * 64 : 4m
		 * 128 : 8m
		 * 256 : 16m
		 * 
		 */
		return true;
		
	}
	
	byte makeChunkOffsetDesc(bool isInOfsMode, int unitLen)
	{
		byte desc;
		desc = (byte)unitLen;
		desc |= isInOfsMode ? (byte)0x10 : (byte)0x0;
		return desc;
	}
	
	public B45ChunkDataBase ReadChunkData(int nth){
		int thisChunkDataStartOfs = phase2StartOfs + chunkOffsets[nth];
		
		byte RLE = binaryBuffer[thisChunkDataStartOfs + ChunkBAHeaderOffsets.RLE];
		byte[] rawBuffer;
		B45ChunkDataBase cd;
		B45Block blk;
		
		if(RLE == 0){
			rawBuffer = new byte[chunkRawDataLength];
			Array.Copy(binaryBuffer, thisChunkDataStartOfs + ChunkBAHeaderOffsets.headerLength, rawBuffer, 0, chunkRawDataLength);
			cd = new B45ChunkDataBase(rawBuffer);
		}
		else
		{
			blk.blockType = binaryBuffer[thisChunkDataStartOfs + ChunkBAHeaderOffsets.headerLength];
			blk.materialType = binaryBuffer[thisChunkDataStartOfs + ChunkBAHeaderOffsets.headerLength + 1];
			// TODO
			cd = new B45ChunkDataBase(blk);
			
		}
		
		cd._chunkPos = new IntVector3(chunkCoords[nth]);
		cd.svn_key_ba = svn_keys_ba[nth];
		
		
		return cd;
		
	}
	
	public void InitHeader()
	{
		fileHeader.version = 0;
		fileHeader.cellLength = 2;
		fileHeader.chunkType = 1;
		fileHeader.chunkSize = 10;
		fileHeader.chunkPrefix = 1;
		fileHeader.chunkPostfix = 1;
		fileHeader.chunkCountX = 32;
		fileHeader.chunkCountY = 8;
		fileHeader.chunkCountZ = 32;
		fileHeader.voxelRes = 1;
		fileHeader.chunkOffsetDesc = makeChunkOffsetDesc(true, 1);
		
		
	}
	public void WriteBAHeader(bool compressed, List<B45ChunkDataBase> chunkDataList)
	{
		
		fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
		byte[] tmp = new byte[2];
		
		if(compressed){
			// compressed
			tmp[1] = (byte)(tmp[1] | 0x1);
		}
		else{
			
		}
		fs.Write(tmp,0, 2);
		byte[] headerBuffer = new byte[FileHeaderOffsets.headerLength];
		
		headerBuffer[FileHeaderOffsets.cellLength] = fileHeader.cellLength;
		
		headerBuffer[FileHeaderOffsets.chunkType] = fileHeader.chunkType;
		headerBuffer[FileHeaderOffsets.chunkSize] = fileHeader.chunkSize;
		
		headerBuffer[FileHeaderOffsets.chunkPrefix] = fileHeader.chunkPrefix;
		headerBuffer[FileHeaderOffsets.chunkPostfix] = fileHeader.chunkPostfix;
		
		ByteArrayHelper.ushort_to(headerBuffer, FileHeaderOffsets.chunkCountX, fileHeader.chunkCountX);
		ByteArrayHelper.ushort_to(headerBuffer, FileHeaderOffsets.chunkCountY, fileHeader.chunkCountY);
		ByteArrayHelper.ushort_to(headerBuffer, FileHeaderOffsets.chunkCountZ, fileHeader.chunkCountZ);
		
		ByteArrayHelper.ushort_to(headerBuffer, FileHeaderOffsets.voxelRes, fileHeader.voxelRes);
		
		headerBuffer[FileHeaderOffsets.chunkOffsetDesc] = fileHeader.chunkOffsetDesc;
		
		fs.Write(headerBuffer,0, (int)FileHeaderOffsets.headerLength);
		
		//chkOfsUnitLen = fileHeader.chunkOffsetDesc & 0x7;
		chkOfsInVectorForm = ((fileHeader.chunkOffsetDesc >> 4) > 0);
		
		int chkOfsLen = 4 + chunkDataList.Count * ChunkOffsetStructOffsets.length ;
		
		byte[] chunkOffsetBinaryBuffer = new byte[chkOfsLen];
		
		chunkRawDataLength = fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.cellLength;
		if(chkOfsInVectorForm){
			// write the chunk count
			ByteArrayHelper.int_to(chunkOffsetBinaryBuffer, 0, chunkDataList.Count);
			
			chunkOffsets = new int[chunkDataList.Count + 1];
			
			// calculate the chunk offsets first
			chunkOffsets[0] = 0;
			int i;
			int lastChunkLength;
			for(i = 1; i <= chunkDataList.Count; i++ )
			{
				lastChunkLength = chunkDataList[i - 1].IsHollow ? ChunkBAHeaderOffsets.headerLength + fileHeader.cellLength : ChunkBAHeaderOffsets.headerLength + chunkRawDataLength;
				
				chunkOffsets[i] = chunkOffsets[i - 1] + lastChunkLength;
			}
			// write the last length to the extra offset slot for write chunk data reference.
			
			// write the offsets into the buffer. phase 1
			for(i = 0; i < chunkDataList.Count; i++ )
			{
				B45ChunkDataBase cd = chunkDataList[i];
				// write the chunk pos
				// in "4 + i * (chkOfsUnitLen * 3 + 12)" the 4 is the length of the chunkcount int at the beginning
				ByteArrayHelper.IntVector3_to(chunkOffsetBinaryBuffer, 4 + i * ChunkOffsetStructOffsets.length, cd._chunkPos);
				
				// write the chunk offset relative to the end of the offset buffer
				ByteArrayHelper.int_to(chunkOffsetBinaryBuffer, 4 + i * ChunkOffsetStructOffsets.length + ChunkOffsetStructOffsets.Offset, chunkOffsets[i]);
				
				// write the svn number
				ByteArrayHelper.int_to(chunkOffsetBinaryBuffer, 4 + i * ChunkOffsetStructOffsets.length + ChunkOffsetStructOffsets.SVN_key_ba, cd.svn_key_ba);
				
			}
			fs.Write(chunkOffsetBinaryBuffer, 0, chkOfsLen);
			
		}
		else
		{
			// it's the chunk offset full listing mode
			
		}
		// phase 2
		WriteChunkData(chunkDataList);
		fs.Close();
	}
	// write chunk data
	void WriteChunkData(List<B45ChunkDataBase> chunkDataList){
		byte[] chunkDataBinaryBuffer = new byte[chunkOffsets[chunkOffsets.Length - 1]];
		int updateVectorStOfs;
		for(int i = 0; i < chunkDataList.Count; i++)
		{
			B45ChunkDataBase cd = chunkDataList[i];
			chunkDataBinaryBuffer[chunkOffsets[i]] = cd.IsHollow ? (byte)1:(byte)0;
			
			updateVectorStOfs = chunkOffsets[i] + ChunkBAHeaderOffsets.headerLength;
			
			if(cd.IsHollow)
			{
				Array.Copy(cd._chunkData, 0, chunkDataBinaryBuffer, chunkOffsets[i] + ChunkBAHeaderOffsets.headerLength, B45Block.Block45Size);
				updateVectorStOfs += B45Block.Block45Size;
			}
			else
			{
				Array.Copy(cd._chunkData, 0, chunkDataBinaryBuffer, chunkOffsets[i] + ChunkBAHeaderOffsets.headerLength, chunkRawDataLength);
				updateVectorStOfs += chunkRawDataLength;
			}

			
		}
		fs.Write(chunkDataBinaryBuffer, 0, chunkDataBinaryBuffer.Length);
	}
}
public struct FileHeader{
	public byte version;
	public byte cellLength; // usually 2 for block and marching cubes terrain
	
	public byte chunkType; // 0: terrain chunk, 1: building chunk
	
	public byte chunkSize; // inside a chunk number of voxels in one direction, including paddings
	public byte chunkPrefix; // padding sizes
	public byte chunkPostfix;
	
	public ushort chunkCountX; // number of chunks in one direction in this file
	public ushort chunkCountY;
	public ushort chunkCountZ;
	
	public ushort voxelRes; // voxel resolution
	public byte chunkOffsetDesc; // using full indices or vector collection.
	
	
}
public class FileHeaderOffsets{
	public const int reserved = 0;
	
	public const int cellLength = 1; // usually 2 for block and marching cubes terrain
	
	public const int chunkType = 2; // 0: terrain chunk, 1: building chunk
	
	public const int chunkSize = 3; // inside a chunk number of voxels in one direction, including paddings
	public const int chunkPrefix = 4; // padding sizes
	public const int chunkPostfix = 5;
	
	public const int chunkCountX = 6; // number of chunks in one direction in this file
	public const int chunkCountY = 8;
	public const int chunkCountZ = 10;
	
	public const int voxelRes = 12; // voxel resolution
	public const int chunkOffsetDesc = 14; // using full indices or vector collection.
	public const int headerLength = 15; // header length
	
};


public class ChunkBAHeaderOffsets{
	public const int RLE = 0; // 1: running length encoding enabled
	public const int headerLength = 1;
};
