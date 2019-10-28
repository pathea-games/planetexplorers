using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class VoxelFileUV {
	string fileName;
	FileStream fs;
	byte[] binaryBuffer;
	
	byte chunkOffsetDesc;
	
	// inferred variables
	bool chkOfsInVectorForm; // are the chunk offsets in vector form
	//int chkOfsUnitLen;
	
	int chunkCount;
	
	int chunkRawDataLength; // length of an uncompressed raw chunk data
	
	int phase2StartOfs = 0;
#region chunk offsets
	IntVector3[] chunkCoords;
	int[] chunkOffsets;
	// for update vectors
	public int[] svn_keys_uv;
	
#endregion
	public VoxelFileUV(string _fileName)
	{
		fileName = _fileName + "_uv.bin";
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
		if(fs.Length < 5)
		{
			fs.Close();
			return false;
		}
		chunkOffsetDesc = (byte)fs.ReadByte();
		if((chunkOffsetDesc & 32) > 0){
			// compressed
		}
		else{
			binaryBuffer = new byte[fs.Length - 1];
			fs.Read(binaryBuffer,0, (int)fs.Length - 1);
		}
		fs.Close();
		
		//chkOfsUnitLen = chunkOffsetDesc & 0x7;
		chkOfsInVectorForm = ((chunkOffsetDesc >> 4) > 0);
		
		int phase1StartOfs = 0;
			
		if(chkOfsInVectorForm){
			// read the chunk count
			
			chunkCount = ByteArrayHelper.to_int(binaryBuffer, 0);
			
			phase1StartOfs = 4;
//			chkOfsLen = chunkCount * (chkOfsUnitLen * 3 + 4);
			phase2StartOfs = phase1StartOfs + chunkCount * ChunkOffsetStructOffsets.length;
			
			chunkCoords = new IntVector3[chunkCount];
			chunkOffsets = new int[chunkCount];
			svn_keys_uv = new int[chunkCount];
			for(int i = 0; i < chunkCount; i++ )
			{
				int chunkOffsetUnitStart = phase1StartOfs + i * ChunkOffsetStructOffsets.length;
			
				chunkCoords[i] = ByteArrayHelper.to_IntVector3(binaryBuffer, chunkOffsetUnitStart);
				chunkOffsets[i] = ByteArrayHelper.to_int(binaryBuffer, chunkOffsetUnitStart + ChunkOffsetStructOffsets.Offset);
				svn_keys_uv[i] = ByteArrayHelper.to_int(binaryBuffer, chunkOffsetUnitStart + ChunkOffsetStructOffsets.SVN_key);
			}
			
		}
		else
		{
			// it's the chunk offset full listing mode
			
		}

		return true;
		
	}
	byte makeChunkOffsetDesc(bool isInOfsMode, int unitLen)
	{
		byte desc;
		desc = (byte)unitLen;
		desc |= isInOfsMode ? (byte)0x10 : (byte)0x0;
		return desc;
	}
	
	public B45ChunkDataBase AttachUVData(int nth, B45ChunkDataBase cd){
		int thisChunkDataStartOfs = phase2StartOfs + chunkOffsets[nth];
		
		int vectorCount = ByteArrayHelper.to_int(binaryBuffer, thisChunkDataStartOfs + ChunkUVHeaderOffsets.vectorCount);
		int svnkeyCount = ByteArrayHelper.to_int(binaryBuffer, thisChunkDataStartOfs + ChunkUVHeaderOffsets.svnkeyCount);
		
		cd.svn_key = svn_keys_uv[nth];
		cd.uvVersionKeys = new List<UVKeyCount>();
		cd.updateVectors = new List<UpdateVector>();
		
		
		for(int i = 0; i < svnkeyCount; i++)
		{
			int thisVersionKeyCountOfs = thisChunkDataStartOfs + ChunkUVHeaderOffsets.headerLength + i * UVKeyCount.length;
			
			UVKeyCount uvkc = new UVKeyCount();
			uvkc.svn_key = ByteArrayHelper.to_int(binaryBuffer, thisVersionKeyCountOfs);
			uvkc.count = ByteArrayHelper.to_ushort(binaryBuffer, thisVersionKeyCountOfs + 4);
				
			cd.uvVersionKeys.Add(uvkc);
		}
		int svnkeyLength = UVKeyCount.length * svnkeyCount;
		
		for(int i = 0; i < vectorCount; i++)
		{
			int thisUpdateVectorOfs = thisChunkDataStartOfs + ChunkUVHeaderOffsets.headerLength + svnkeyLength + i * UpdateVector.length;
			
			UpdateVector uv = new UpdateVector();
			uv.xyz0 = binaryBuffer[thisUpdateVectorOfs];
			uv.xyz1 = binaryBuffer[thisUpdateVectorOfs + 1];
			uv.voxelData0 = binaryBuffer[thisUpdateVectorOfs + 2];
			uv.voxelData1 = binaryBuffer[thisUpdateVectorOfs + 3];
				
			cd.updateVectors.Add(uv);
		}
		
		return cd;
		
	}
	public void InitHeader()
	{
		chunkOffsetDesc = makeChunkOffsetDesc(true, 1);
	}
	
	public void WriteUVHeader(bool compressed, List<B45ChunkDataBase> chunkDataList)
	{
		
		fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
		byte headerByte = chunkOffsetDesc;
		
		if(compressed){
			// compressed
			headerByte = (byte)(headerByte | 32);
		}
		else{
			
		}
		
		fs.WriteByte(headerByte);
		
		//chkOfsUnitLen = chunkOffsetDesc & 0x7;
		chkOfsInVectorForm = ((chunkOffsetDesc >> 4) > 0);
		
		int chkOfsLen = 4 + chunkDataList.Count * ChunkOffsetStructOffsets.length ;
		
		byte[] chunkOffsetBinaryBuffer = new byte[chkOfsLen];
		
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
				List<UVKeyCount> uvkc = chunkDataList[i - 1].uvVersionKeys;
				int uvkc_len = 0;
				if(uvkc != null)
					uvkc_len = uvkc.Count * UVKeyCount.length;
				
				List<UpdateVector> uvs = chunkDataList[i - 1].updateVectors;
				int uv_len = 0;
				if(uvs != null)
					uv_len = uvs.Count * UpdateVector.length;
				
				lastChunkLength = ChunkUVHeaderOffsets.headerLength + uvkc_len + uv_len;
				
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
				ByteArrayHelper.int_to(chunkOffsetBinaryBuffer, 4 + i * ChunkOffsetStructOffsets.length + ChunkOffsetStructOffsets.SVN_key, cd.svn_key);
			}
			fs.Write(chunkOffsetBinaryBuffer, 0, chkOfsLen);
			
		}
		else
		{
			// it's the chunk offset full listing mode
			
		}
		// phase 2
		WriteUVData(chunkDataList);
		fs.Close();
	}
	
	
	void WriteUVData(List<B45ChunkDataBase> chunkDataList){
		byte[] chunkDataBinaryBuffer = new byte[chunkOffsets[chunkOffsets.Length - 1]];
		int updateVectorStOfs;
		int uvKeyCountStOfs;
		for(int i = 0; i < chunkDataList.Count; i++)
		{
			B45ChunkDataBase cd = chunkDataList[i];
			
			//chunkRawDataOffsetStart + chunkOffsets[nth] + ChunkBAHeaderOffsets.vectorCount
			if(cd.updateVectors == null)
				ByteArrayHelper.int_to(chunkDataBinaryBuffer, chunkOffsets[i] + ChunkUVHeaderOffsets.vectorCount, 0);
			else
				ByteArrayHelper.int_to(chunkDataBinaryBuffer, chunkOffsets[i] + ChunkUVHeaderOffsets.vectorCount, cd.updateVectors.Count);
			
			if(cd.uvVersionKeys == null)
				ByteArrayHelper.int_to(chunkDataBinaryBuffer, chunkOffsets[i] + ChunkUVHeaderOffsets.svnkeyCount, 0);
			else
				ByteArrayHelper.int_to(chunkDataBinaryBuffer, chunkOffsets[i] + ChunkUVHeaderOffsets.svnkeyCount, cd.uvVersionKeys.Count);
			
			
			uvKeyCountStOfs = chunkOffsets[i] + ChunkUVHeaderOffsets.headerLength;
			
			if(cd.uvVersionKeys != null){
				
				for(int j = 0; j < cd.uvVersionKeys.Count;j++)
				{
					ByteArrayHelper.int_to(chunkDataBinaryBuffer, uvKeyCountStOfs + j * UVKeyCount.length, cd.uvVersionKeys[j].svn_key);
					ByteArrayHelper.ushort_to(chunkDataBinaryBuffer, uvKeyCountStOfs + j * UVKeyCount.length + 4, cd.uvVersionKeys[j].count);
				}
				
				updateVectorStOfs = uvKeyCountStOfs + cd.uvVersionKeys.Count * UVKeyCount.length;
				
				if(cd.updateVectors != null){
				
					for(int j = 0; j < cd.updateVectors.Count;j++)
					{
						chunkDataBinaryBuffer[updateVectorStOfs + j * UpdateVector.length] = cd.updateVectors[j].xyz0;
						chunkDataBinaryBuffer[updateVectorStOfs + j * UpdateVector.length + 1] = cd.updateVectors[j].xyz1;
						chunkDataBinaryBuffer[updateVectorStOfs + j * UpdateVector.length + 2] = cd.updateVectors[j].voxelData0;
						chunkDataBinaryBuffer[updateVectorStOfs + j * UpdateVector.length + 3] = cd.updateVectors[j].voxelData1;
						
					}
				}
			}
			
			cd.updateVectors = null;
			
		}
		fs.Write(chunkDataBinaryBuffer, 0, chunkDataBinaryBuffer.Length);
	}
	
}

// the following offsets assume using 1 byte for storing each axis element.
public class ChunkOffsetStructOffsets{
	public const int Offset = 3;
	public const int SVN_key = 7;
	public const int SVN_key_ba = 7;
	
	public const int length = 11;
};
public class ChunkUVHeaderOffsets{
	public const int vectorCount = 0; // 1: running length encoding enabled
	public const int svnkeyCount = 4; // vector count
	public const int headerLength = 8;
};
