using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class VFDataRTGenFileCache
{
	public const int MagicCode = 16102000;//16093001;	// If file cache structure changes, the ver also should be changed/increased.
    public const int MagicCodeLen = sizeof(int);
	public const int MaxCacheFiles = 10;


    public static string GetCacheFilePathName(string strSeed)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] hash_byte = md5.ComputeHash(Encoding.UTF8.GetBytes(strSeed));
        string md5str = System.BitConverter.ToString(hash_byte);
        md5str = md5str.Replace("-", "");
        return GameConfig.VoxelCacheDataPath + md5str;
    }
    string _cacheFilePathName;
    // Key[x,z,l,h], Value[VFTerTileCacheDesc]
    Dictionary<IntVector4, VFTerTileCacheDesc> _voxelTileCacheDescsList = new Dictionary<IntVector4, VFTerTileCacheDesc>();
    FileStream _voxelTileCachesFS = null;
    BinaryReader _br; // Using/Close will close the underlying stream.
    BinaryWriter _bw; // Using/Close will close the underlying stream.

	public static void ClearAllCache()
	{
		if (!Directory.Exists(GameConfig.VoxelCacheDataPath)) return;

		string[] files = Directory.GetFiles(GameConfig.VoxelCacheDataPath);
		int n = files.Length;
		for (int i = 0; i < n; i++) {
			try
			{
				File.Delete(files[i]);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[VoxelCache]Failed to delete an invalid cache file:" + files[i] + e);
			}
		}
	}

    void ClearInvalidCache()
    {
        if (!Directory.Exists(GameConfig.VoxelCacheDataPath)) return;

        string[] files = Directory.GetFiles(GameConfig.VoxelCacheDataPath);
		int n = files.Length;
		for(int i = 0; i < n; i++)
        {
			string file = files[i];
            try
            {
                bool invalidFile = false;
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    BinaryReader br = new BinaryReader(fs);
                    if (fs.Length < MagicCodeLen || br.ReadInt32() != MagicCode)
                    {
                        invalidFile = true;
                    }
                }
                if (invalidFile)
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[VoxelCache]Failed to delete an invalid cache file:" + file + e);
            }
        }
#if false
		files = Directory.GetFiles(GameConfig.VoxelCacheDataPath);
		n = files.Length;
		if(n > MaxCacheFiles){
			try
			{
				int idxOld = 0;
				FileInfo fi = new FileInfo(files[0]);
				DateTime old = fi.LastWriteTime;
				for(int i = 1; i < n; i++)
				{
					DateTime t = new FileInfo(files[i]).LastWriteTime;
					if(t < old){
						idxOld = i;
						t= old;
					}
				}
				File.Delete(files[idxOld]);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[VoxelCache]Failed to delete an old cache file:"  + e);
			}
		}
#endif
	}
	void LoadVoxelTileCacheDescs()
    {
        if (_voxelTileCachesFS != null) _voxelTileCachesFS.Close();

        try
        {
            // Open or create
            if (!Directory.Exists(GameConfig.VoxelCacheDataPath)) Directory.CreateDirectory(GameConfig.VoxelCacheDataPath);
            _voxelTileCachesFS = new FileStream(_cacheFilePathName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _br = new BinaryReader(_voxelTileCachesFS);
            _bw = new BinaryWriter(_voxelTileCachesFS);
            long endPos = _voxelTileCachesFS.Length;
            //Test magic code
            if (endPos < MagicCodeLen || _br.ReadInt32() != MagicCode)
            {
                _voxelTileCachesFS.SetLength(0);
                _bw.Write(MagicCode);
                _bw.Flush();
                ClearInvalidCache();
                endPos = _voxelTileCachesFS.Length;
            }

            long curPos = _voxelTileCachesFS.Position;
            while (curPos < endPos)
            {
                _voxelTileCachesFS.Seek(curPos, SeekOrigin.Begin);
                VFTerTileCacheDesc desc = VFTerTileCacheDesc.ReadDescFromCache(_br);
				if (desc.xzlh.w == VFDataRTGen.s_noiseHeight && desc.dataLen > 0)
				{
					_voxelTileCacheDescsList.Add(desc.xzlh, desc);
				}
				else
                {
					Debug.LogWarning("[VFDataRTGen]Error: Unrecognized voxel tile,discard the following data." + desc.xzlh + ":" + desc.dataLen);
                    _voxelTileCachesFS.SetLength(curPos);
                    endPos = _voxelTileCachesFS.Length;
                }
                curPos += desc.dataLen + VFTerTileCacheDesc.DataOfs;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[VFDataRTGen]Error:Failed to open/create voxel cache file:" + e);
            Close();
        }
    }
    public void SaveDataToFileCaches(int bitMask, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
    {
        if (_voxelTileCachesFS == null) return;

        IntVector4 xzlh = new IntVector4(tile.tileX, tile.tileZ, tile.tileL, tile.tileH);
        if (_voxelTileCacheDescsList.ContainsKey(xzlh))
        {
            Debug.LogWarning("[VFDataRTGen]:Try to append a existing voxel tile to cache file." + xzlh);
            return;
        }
        try
        {
            _voxelTileCachesFS.Seek(0, SeekOrigin.End);	//Append
            VFTerTileCacheDesc desc = VFTerTileCacheDesc.WriteDataToCache(_bw, bitMask, tile, nData, hData, gData, tData);
            _voxelTileCacheDescsList.Add(xzlh, desc);
        }
        catch (Exception e)
        {
			string strE = e.ToString();
			Debug.LogWarning("[VFDataRTGen]:Failed to append voxel tile to cache file" + xzlh + e);
			if(strE.Contains("IOException: Win32 IO returned 112.")){
				GameLog.HandleExceptionInThread(e);
			}
        }
    }

    public VFDataRTGenFileCache(string cacheFilePathName)
    {
        _cacheFilePathName = cacheFilePathName;
        if (SystemSettingData.Instance.VoxelCacheEnabled)
        {
            LoadVoxelTileCacheDescs();
        }
    }
    public void Close()
    {
        if (_voxelTileCachesFS == null) return;
        try
        {
            _br.Close();
            _bw.Close();
            _voxelTileCachesFS.Close();
        }
        catch
        {
            Debug.LogWarning("[VFDataRTGen]Error:Failed to close the opening voxel cache file.");
        }
        _voxelTileCachesFS = null;
    }
    public VFTerTileCacheDesc FillTileDataWithFileCache(IntVector4 xzlh, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
    {
        VFTerTileCacheDesc desc = null;
        if (_voxelTileCacheDescsList.TryGetValue(xzlh, out desc))
        {
            try
            {
                desc.ReadDataFromCache(_br, tile, nData, hData, gData, tData);
            }
            catch
            {
                Debug.LogWarning("[VFDataRTGen]Error:Failed to read data from cache " + xzlh);
                return null;
            }
        }
        return desc;
    }
}
