using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// FileFormat:
// Version: 4bytes
// ChunkCount: 4bytes
// ChunkDatas: 35*35*35*2*ChunkCount
// OffsetDatas:(IntVector4(16bytes)+offsetInFile(8bytes))*ChunkCount

public class VFVoxelSave
{
	static readonly int s_ver = 1606230;
    static List<VFVoxelSave> s_VoxelSaveList = new List<VFVoxelSave>();

	string _archiveKey = "";
	Action<BinaryReader> _addtionalReader = null;
	Action<BinaryWriter> _addtionalWriter = null;
	List<VFVoxelChunkData> _chunkListToSave = new List<VFVoxelChunkData>();
	Dictionary<IntVector4, long> _modifiedChunksInfo = new Dictionary<IntVector4, long>();
	FileStream _tmpVoxelFileStream = null;//临时保存chunk文件

	public Dictionary<IntVector4, long> modifiedChunksInfoDic
    {
        get
        {
			return _modifiedChunksInfo;
        }
    }
	public List<VFVoxelChunkData> ChunkSaveList
    {
        get
        {
            return _chunkListToSave;
        }
    }

	public VFVoxelSave(string archiveKey, Action<BinaryReader> reader = null, Action<BinaryWriter> writer = null)
	{		
		int index = s_VoxelSaveList.Count;
		_tmpVoxelFileStream = OpenTmpVoxelFileStream(index);
        s_VoxelSaveList.Add(this);

		_archiveKey = archiveKey;
		_addtionalReader = reader;
		_addtionalWriter = writer;
	}
	FileStream OpenTmpVoxelFileStream(int index)
    {
        string fPathTmp = System.IO.Path.GetTempPath();
        try
        {
            return new FileStream(fPathTmp+"/voxel"+index+".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
        catch (System.Exception ex)
        {
        	Debug.LogError("Create tmp file error!"+ex);
            return null;
        }
    }
	void CloseTmpVoxelFileStream()
	{
		if (null != _tmpVoxelFileStream)
		{
			_tmpVoxelFileStream.Close();
		}
	}

	public void Import(Pathea.PeRecordReader r)
	{
		if (null == r)           return;
		if (_tmpVoxelFileStream == null)	return;

        if (!r.Open())
        {
            return;
        }

		BinaryReader br = r.binaryReader;

		// Version check
		int ver = br.ReadInt32();
		if(ver != s_ver)
		{
			Debug.LogError("[VoxelSave]:Error version:" + ver + "|" + s_ver);
			r.Close();
			return;
		}

		int cnt = br.ReadInt32();
		byte[] buff = VFVoxelChunkData.s_ChunkDataPool.Get();
		BinaryWriter tmpbw = new BinaryWriter(_tmpVoxelFileStream);
		_tmpVoxelFileStream.Seek(0, SeekOrigin.Begin);
		tmpbw.Write(ver);
		tmpbw.Write(cnt);
		for (int i = 0; i < cnt; i++)
		{
			br.Read(buff, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
			_tmpVoxelFileStream.Write(buff, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
		}
		VFVoxelChunkData.s_ChunkDataPool.Free(buff);

		_modifiedChunksInfo.Clear();
		//long chnkDataPos = br.BaseStream.Position;
		//long infoDataPos = chnkDataPos + cnt*VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT;
		//br.BaseStream.Seek(infoDataPos);
		for (int i = 0; i < cnt; i++)
		{
			int x = br.ReadInt32();
			int y = br.ReadInt32();
			int z = br.ReadInt32();
			int w = br.ReadInt32();
			long pos = br.ReadInt64();
			_modifiedChunksInfo.Add(new IntVector4(x, y, z, w), pos);
		}
		
		if(_addtionalReader != null)
		{
			_addtionalReader(br);
		}
		r.Close();
	}
	public void Export(Pathea.PeRecordWriter w)
	{
		if (_tmpVoxelFileStream == null)	return;

		if (!GameConfig.IsMultiMode)
			SaveChunksInListToTmpFile();
		
		BinaryWriter bw = w.binaryWriter;
		if (bw == null)
		{
			Debug.LogError("On WriteRecord FileStream is null!");
			return;
		}

		byte[] buff = VFVoxelChunkData.s_ChunkDataPool.Get();
		int cnt = _modifiedChunksInfo.Count;
		bw.Write(s_ver);
		bw.Write(cnt);	
		_tmpVoxelFileStream.Seek(2*sizeof(int), SeekOrigin.Begin);
		for (int i = 0; i < cnt; i++)
		{
			_tmpVoxelFileStream.Read(buff, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
			bw.Write(buff, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
		}
		VFVoxelChunkData.s_ChunkDataPool.Free(buff);

		foreach (KeyValuePair<IntVector4, long> pair in _modifiedChunksInfo)
		{
			IntVector4 key = pair.Key;

			bw.Write(key.x);
			bw.Write(key.y);
			bw.Write(key.z);
			bw.Write(key.w);
			bw.Write(pair.Value);
		}

		if(_addtionalWriter != null)
		{
			_addtionalWriter(bw);
		}
	}

	public void SaveChunkListImmediately(List<VFVoxelChunkData> vcds)
	{
		int n = vcds.Count;
		for(int i = 0; i < n; i++)
		{
			AddChunkToSaveList(vcds[i]);
		}
		if (!GameConfig.IsMultiMode)
		{
			for(int i = 0; i < _chunkListToSave.Count; i++ )
			{
				if(_chunkListToSave[i] != null)
				{
					SaveChunkToTmpFile(_chunkListToSave[i]);
				}
			}
			_chunkListToSave.Clear();
		}
	}

	public byte[] TryGetChunkData(IntVector4 cpos, bool useChunkDataPool = true)
	{
		if (_tmpVoxelFileStream == null)	return null;

		long pos = 0;
		if(_modifiedChunksInfo.TryGetValue(cpos, out pos))
		{
			_tmpVoxelFileStream.Seek(pos, SeekOrigin.Begin);

			byte[] buff = useChunkDataPool ? VFVoxelChunkData.s_ChunkDataPool.Get() : new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
			_tmpVoxelFileStream.Read(buff, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
			return buff;
		}
		return null;
	}
	public void AddChunkToSaveList(VFVoxelChunkData vc)
	{
		if(!_chunkListToSave.Contains(vc))
		{
			_chunkListToSave.Add(vc);
		}
		Pathea.ArchiveMgr.Instance.SaveMe(_archiveKey);
	}	
	public bool SaveChunkToTmpFile(VFVoxelChunkData chunk)	// Save To _tmpVoxelFileStream and _modifiedChunksInfo
	{
		if(chunk.DataVT.Length == 0)
		{
			Debug.LogError("[SaveChunk]Data is null:"+chunk.ChunkPosLod);
			return false;
		}

		if(chunk.IsHollow)
		{
			Debug.LogError("[SaveChunk]Data is Hollow:"+chunk.ChunkPosLod);
			return false;
		}

		long pos = 0;
		if(!_modifiedChunksInfo.TryGetValue(chunk.ChunkPosLod, out pos))
		{
			_modifiedChunksInfo[new IntVector4(chunk.ChunkPosLod)] = pos = _tmpVoxelFileStream.Length;
		}
		_tmpVoxelFileStream.Seek(pos, SeekOrigin.Begin);
		_tmpVoxelFileStream.Write(chunk.DataVT, 0, chunk.DataVT.Length);
		return true;
	}
	public void SaveChunksInListToTmpFile()
	{
		for (int i = 0; i < _chunkListToSave.Count; i++)
		{
			if (_chunkListToSave[i] != null)
			{
				SaveChunkToTmpFile(_chunkListToSave[i]);
			}
		}
		_chunkListToSave.Clear();
	}

	// Static methods
	public static IEnumerator CoSaveAllChunksInList()
	{
		int n = s_VoxelSaveList.Count;
		for(int i = 0; i < n; i++)
		{
			s_VoxelSaveList[i].SaveChunksInListToTmpFile();
		}
		yield return 0;
	}
	public static void SaveAllChunksInList()
	{
		int n = s_VoxelSaveList.Count;
		for(int i = 0; i < n; i++)
		{
			s_VoxelSaveList[i].SaveChunksInListToTmpFile();
		}
	}
	public static void Clean()
	{
		s_VoxelSaveList.Clear();
	}
}
