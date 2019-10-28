using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace VoxelMap{
public class VoxelType
{
    public byte m_vol;
    public byte m_type;
}

public class ChunkIdx
{
    public IntVector3 m_gobalIdx;
    public IntVector3 m_localIdx;
};

public class VoxelChunkData
{
    public int m_iOffset = 0; //chunk偏移位置
    public VoxelType m_zipVoxel; //如果存在对象，则表示为压缩格式的
    public VoxelType[] m_voxels; //保存非压缩格式的voxels

    public void LoadChunkData(BinaryReader _brMS, int _chunkSize)
    {
        if (_chunkSize == 2)
		{
            m_zipVoxel = new VoxelType();
            m_zipVoxel.m_vol = _brMS.ReadByte();
            m_zipVoxel.m_type = _brMS.ReadByte();
		}
        else
		{
            m_voxels = new VoxelType[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH];

            for (int _i = 0; _i != VoxelTerrainConstants.VOXEL_ARRAY_LENGTH; ++_i)
            {
                VoxelType _vt = new VoxelType();
                _vt.m_vol = _brMS.ReadByte();
                _vt.m_type = _brMS.ReadByte();
                m_voxels[_i] = _vt;  
            }
		}
    }

    public void ReplaceChunkData(ChunkIdx _chunkIdx, Dictionary<IntVector4, long> chunkModifyInfos)
    {
        //清空已存在的数据
        Clear();
        //if (info.DefChunkStreamInMenu == null)
        //    return;

        
		foreach (KeyValuePair<IntVector4, long> ite in chunkModifyInfos)
        {
			IntVector4 cpos = ite.Key;
			if(cpos.XYZ.Equals(_chunkIdx.m_gobalIdx))
			{
				bool breplace = false;
			#if false
				for (int _j = 0; _j <= LODOctreeMan.MaxLod; _j++)
			#endif
				{
					byte[] bytes = VFVoxelTerrain.self.SaveLoad.TryGetChunkData(cpos, false);
					m_voxels = new VoxelType[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH];
						
					int m = 0;
					for (int _i = 0; _i != VoxelTerrainConstants.VOXEL_ARRAY_LENGTH; ++_i)
					{
						VoxelType _vt = new VoxelType();
						_vt.m_vol = bytes[m++];
						_vt.m_type = bytes[m++];
						m_voxels[_i] = _vt;
					}
					breplace = true;
				}
				
				if (!breplace)
				{
					Debug.LogError("ReplaceChunkData() is error!");
					return;
				}
				
				Debug.Log("ReplaceChunkData() - done!!!");
				return;
			}
        }
    }

    public int SaveChunkData(BinaryWriter _bw)
    {
        int _startPos = (int)_bw.BaseStream.Position;
        if (m_zipVoxel != null)
        {
            _bw.Write(m_zipVoxel.m_vol);
            _bw.Write(m_zipVoxel.m_type);
        }
        else
        {
            foreach (VoxelType _vt in m_voxels)
            {
                _bw.Write(_vt.m_vol);
                _bw.Write(_vt.m_type);
            }
        }

        int _endPos = (int)_bw.BaseStream.Position;
        int _bytes = _endPos - _startPos;
        return _bytes;
    }

    public void Clear()
    {
        m_iOffset = 0;
        m_zipVoxel = null;
        m_voxels = null;
    }
};

//128x128的piece数据
public class VoxelPieceData
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

    public VoxelChunkData[] m_chunkData;
    public int m_iOffsetPiece;
    public byte[] m_zipBytes;

    public void LoadPieceData(FileStream _fs, BinaryReader _br, int _chunkCount, int _iPieceSize)
    {
        int _totalChunkCount = _chunkCount * _chunkCount * _chunkCount;
        m_chunkData = new VoxelChunkData[_totalChunkCount];
			
        byte[] _zipBytes = _br.ReadBytes(_iPieceSize);
		MemoryStream _ms;
		BinaryReader _brMS;
		if(_iPieceSize == 2)
		{
			_ms = new MemoryStream(_zipBytes);
        	_brMS = new BinaryReader(_ms);
	        for (int _i = 0; _i != m_chunkData.Length; ++_i)
	        {
	            m_chunkData[_i] = new VoxelChunkData();
	            m_chunkData[_i].m_iOffset = 0;
				_ms.Seek(m_chunkData[_i].m_iOffset, SeekOrigin.Begin);
				m_chunkData[_i].LoadChunkData(_brMS, 2);
	        }
		}
		else
		{
	        //偏移地址 + voxel数据
	        int _iMaxSize = _totalChunkCount * 4 + _totalChunkCount * (4 * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE); 
	        byte[] _maxUnzipBytes = new byte[_iMaxSize];
	        int _unzipSize = LZ4_uncompress_unknownOutputSize(_zipBytes, _maxUnzipBytes, _zipBytes.Length, _maxUnzipBytes.Length);
	        byte[] _unzipBytes = new byte[_unzipSize];
	        Array.Copy(_maxUnzipBytes, _unzipBytes, _unzipSize);
	
	        _ms = new MemoryStream(_unzipBytes);
	        _brMS = new BinaryReader(_ms);
	        //读取偏移区域数据
	        for (int _i = 0; _i != m_chunkData.Length; ++_i)
	        {
	            m_chunkData[_i] = new VoxelChunkData();
	            m_chunkData[_i].m_iOffset = _brMS.ReadInt32();
	        }
	
	        //读取每chunk数据
	        for (int _i = 0; _i != m_chunkData.Length; ++_i)
	        {
	            int _chunkSize = 0;
	            if (_i != (m_chunkData.Length - 1))
	            {
	                _chunkSize = m_chunkData[_i + 1].m_iOffset - m_chunkData[_i].m_iOffset;
	            }
	            else
	            {
	                _chunkSize = _unzipSize - m_chunkData[_i].m_iOffset;
	            }
	
	            _ms.Seek(m_chunkData[_i].m_iOffset, SeekOrigin.Begin);
	            m_chunkData[_i].LoadChunkData(_brMS, _chunkSize);
	        }
		}
    }

    public void LoadZipPieceData(BinaryReader _br, int _iPieceSize)
    {
        m_zipBytes = _br.ReadBytes(_iPieceSize);
    }

    public void SavePieceData(BinaryWriter _out)
    {
        _out.Write(m_zipBytes);
    }

    public void ReplaceChunkData(List<ChunkIdx> _ltChunk, int _iChunkCount, Dictionary<IntVector4, long> chunkModifyInfos)
    {
        foreach (ChunkIdx _chunkIdx in _ltChunk)
        {
            //确定替换的chunk编号
            int _idx = _chunkIdx.m_localIdx.x * _iChunkCount * _iChunkCount + _chunkIdx.m_localIdx.y * _iChunkCount + _chunkIdx.m_localIdx.z;
			m_chunkData[_idx].ReplaceChunkData(_chunkIdx, chunkModifyInfos);
        }
    }

    //压缩和保存pieceData
    public void CompressAndSave(BinaryWriter _bw, int _chunkCount)
    {
        //压缩pieceData
        int _totalChunkCount = _chunkCount * _chunkCount * _chunkCount;
        int _bufSize = _totalChunkCount * 4 + _totalChunkCount * (4 * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE);
        byte[] _buf = new byte[_bufSize];
        MemoryStream _ms = new MemoryStream(_buf);
        BinaryWriter _bwMS = new BinaryWriter(_ms);
        _ms.Seek(_totalChunkCount * 4, SeekOrigin.Begin);

        //先写入chunkData
        for(int _i = 0; _i != m_chunkData.Length; ++_i)
        {
            m_chunkData[_i].m_iOffset = (int)_ms.Position;
            m_chunkData[_i].SaveChunkData(_bwMS);
        }
        int _endPos = (int)_ms.Position;

        //后写入chunkData偏移位置
        _ms.Seek(0, SeekOrigin.Begin);
        foreach(VoxelChunkData _vcd in m_chunkData)
        {
            _bwMS.Write(_vcd.m_iOffset);
        }

        //压缩数据
        byte[] _zipBuf = new byte[_endPos];
        int _zipSize = LZ4_compress(_buf, _zipBuf, _endPos);
        _bw.Write(_zipBuf, 0, _zipSize);
    }

    public void Clear()
    {
        m_chunkData = null;
        m_zipBytes = null;
    }
}

public class VoxelMapData
{
	public VoxelPieceData[] m_pieceData;
	public void ReplaceModifyChunk(string _strVoxelmap, int _pieceCount, int _iMaxTier, int _chunkCount, List<IntVector4> _ltChunk, Dictionary<IntVector4, long> chunkModifyInfos)
    {
        string _strSrcVoxelMap = VFVoxelTerrain.MapDataPath_Zip + _strVoxelmap;

        using (FileStream _fsSrcVM = new FileStream(_strSrcVoxelMap, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (BinaryReader _brSrcVM = new BinaryReader(_fsSrcVM))
            {
                int _fileSize = (int)_fsSrcVM.Length;
                //int _iMaxSize = _pieceCount * _iMaxTier * _pieceCount;
                int _iSize = _pieceCount * _iMaxTier;
                int _jSize = _iMaxTier;

				// Here Ignore lod() use 0
                //计算并保存修改的chunk在当前voxelMap中的对应的piece和chunk位置
                Dictionary<IntVector4, List<ChunkIdx>> _mapChunk = new Dictionary<IntVector4, List<ChunkIdx>>();
                foreach (IntVector4 _idx in _ltChunk)
                {		
					int _offsetX = _idx.x % (VoxelTerrainConstants._mapFileWidth>>VoxelTerrainConstants._shift);
					int _offsetZ = _idx.z % (VoxelTerrainConstants._mapFileWidth>>VoxelTerrainConstants._shift);

                    IntVector4 _pieceIdx = new IntVector4();
                    _pieceIdx.x = _offsetX / VoxelTerrainConstants._ChunksPerPiecePerAxis;
                    _pieceIdx.y = _idx.y / VoxelTerrainConstants._ChunksPerPiecePerAxis;
                    _pieceIdx.z = _offsetZ / VoxelTerrainConstants._ChunksPerPiecePerAxis;

                    ChunkIdx _chunkIdx = new ChunkIdx();
					_chunkIdx.m_gobalIdx = _idx.XYZ;	// Here Ignore lod() use 0
                    _chunkIdx.m_localIdx = new IntVector3();
                    _chunkIdx.m_localIdx.x = _offsetX % VoxelTerrainConstants._ChunksPerPiecePerAxis;
                    _chunkIdx.m_localIdx.z = _offsetZ % VoxelTerrainConstants._ChunksPerPiecePerAxis;
                    _chunkIdx.m_localIdx.y = _idx.y % VoxelTerrainConstants._ChunksPerPiecePerAxis;
						
                    List<ChunkIdx> _chunkList;
                    if (_mapChunk.TryGetValue(_pieceIdx, out _chunkList))
                        _chunkList.Add(_chunkIdx);
                    else
                    {
                        _chunkList = new List<ChunkIdx>();
                        _chunkList.Add(_chunkIdx);
                        _mapChunk.Add(_pieceIdx, _chunkList);
                    }
                }

                //读取每个piece的offset并且初始化
                int _iPieceSize = _pieceCount * _iMaxTier * _pieceCount;
                m_pieceData = new VoxelPieceData[_iPieceSize];
                for (int _i = 0; _i != _iPieceSize; ++_i)
                {
                    m_pieceData[_i] = new VoxelPieceData();
                    //依次读取每个piece的chunk数据
                    m_pieceData[_i].m_iOffsetPiece = _brSrcVM.ReadInt32();
                }

                //拷贝和整合修改后的数据
                if (!Directory.Exists(GameConfig.MergedUserDataPath))
                    Directory.CreateDirectory(GameConfig.MergedUserDataPath);
					
                string _strDstVoxelMap = GameConfig.MergedUserDataPath + _strVoxelmap;
                using (FileStream _fsDstVM = new FileStream(_strDstVoxelMap, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (BinaryWriter _bwDstVM = new BinaryWriter(_fsDstVM))
                    {
                        //偏移_fsDstVM中的piece数据区
                        _fsDstVM.Seek(_iPieceSize * 4, SeekOrigin.Begin);

                        //写入pieceData
                        for (int _i = 0; _i != _iPieceSize; ++_i)
                        {
                            IntVector4 _pieceIdx = new IntVector4();
							int _temp = _i % _iSize;
                            _pieceIdx.x = _i / _iSize;
                            _pieceIdx.z = _temp / _jSize;
                            _pieceIdx.y = _temp % _jSize;

                            //确定当前piece的bytes大小
                            int _iPieceBytes = 0;
                            if (_i != (m_pieceData.Length - 1))
                            {
                                _iPieceBytes = m_pieceData[_i + 1].m_iOffsetPiece - m_pieceData[_i].m_iOffsetPiece;
                            }
                            else
                                _iPieceBytes = _fileSize - m_pieceData[_i].m_iOffsetPiece;

                            //设置源文件pieceData偏移位置
                            _fsSrcVM.Seek(m_pieceData[_i].m_iOffsetPiece, SeekOrigin.Begin);
                            //记录目标文件pieceData当前位置
                            m_pieceData[_i].m_iOffsetPiece = (int)_fsDstVM.Position;

                            //载入pieceData数据
                            List<ChunkIdx> _chunkList;
                            if (_mapChunk.TryGetValue(_pieceIdx, out _chunkList))
                            {
								string _strLastFileMerged = "";
						        foreach (ChunkIdx _chunkIdx in _chunkList)
						        {
									_strLastFileMerged += "(" + _chunkIdx.m_gobalIdx.x + "," + _chunkIdx.m_gobalIdx.z + "," + _chunkIdx.m_gobalIdx.y + ")";
						        }
								EditorUtility.DisplayProgressBar("Merging files...", _strVoxelmap+_strLastFileMerged, _i/(float)_iPieceSize);
							
                                //载入解压后的pieceData                         
                                m_pieceData[_i].LoadPieceData(_fsSrcVM, _brSrcVM, _chunkCount, _iPieceBytes);
                                //替换其中修改后的chunkData
								m_pieceData[_i].ReplaceChunkData(_chunkList, _chunkCount, chunkModifyInfos);
                                m_pieceData[_i].CompressAndSave(_bwDstVM, _chunkCount);
                            }
                            else
                            {
								EditorUtility.DisplayProgressBar("Merging files...", _strVoxelmap + ":" + _i , _i/(float)_iPieceSize);
							
                                //直接读取压缩pieceData并存入_fsDstVM
                                m_pieceData[_i].LoadZipPieceData(_brSrcVM, _iPieceBytes);
                                m_pieceData[_i].SavePieceData(_bwDstVM);
                            }

                            //清理pieceData
                            m_pieceData[_i].Clear();
                        }

                        //写入pieceData偏移数据
                        _fsDstVM.Seek(0, SeekOrigin.Begin);
                        foreach (VoxelPieceData _vpd in m_pieceData)
                        {
                            _bwDstVM.Write(_vpd.m_iOffsetPiece);
                        }
                    }
                }
            }
        }
    }
}
}
