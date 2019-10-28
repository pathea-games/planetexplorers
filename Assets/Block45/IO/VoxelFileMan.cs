using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class VoxelFileMan {
	VoxelFileBA fileBA;
	VoxelFileUV fileUV;
	
	public VoxelFileMan(string _fileName)
	{
		fileBA = new VoxelFileBA(_fileName);
		fileUV = new VoxelFileUV(_fileName);
		fileBA.InitHeader();
		fileUV.InitHeader();
	}

	public List<B45ChunkDataBase> ReadAllChunkData(out int out_svn_key, int read_ref_svnkey)
	{
		int highest_svn_key = -1;
		List<B45ChunkDataBase> ret = new List<B45ChunkDataBase>();
		fileBA.ReadHeader();
		fileUV.ReadHeader();
		
		int chunkCount = fileBA.GetChunkCount();
		for(int i = 0; i < chunkCount; i++){
			
			B45ChunkDataBase cd = fileBA.ReadChunkData(i);
			
			if(cd.svn_key_ba <= read_ref_svnkey)continue;
			
			fileUV.AttachUVData(i, cd);
			
			ret.Add(cd);
			if(highest_svn_key < cd.svn_key_ba)
			{
				highest_svn_key = cd.svn_key_ba;
			}
		}
		out_svn_key = highest_svn_key;
		return ret;
	}
	public List<B45ChunkDataHeader> ReadAllChunkHeader(out int out_svn_key, int read_ref_svnkey)
	{
		int highest_svn_key = -1;
		List<B45ChunkDataHeader> ret = new List<B45ChunkDataHeader>();
		fileBA.ReadHeader();
		fileUV.ReadHeader();
		
		int chunkCount = fileBA.GetChunkCount();
		for(int i = 0; i < chunkCount; i++){

			B45ChunkDataHeader cd = new B45ChunkDataHeader();
			cd._chunkPos = fileBA.chunkCoords[i];
			cd.svn_key_ba = fileBA.svn_keys_ba[i];
			cd.svn_key = fileUV.svn_keys_uv[i];
			
			if(cd.svn_key_ba <= read_ref_svnkey)continue;
			
//			fileUV.AttachUVData(i, cd);
			
			ret.Add(cd);
			if(highest_svn_key < cd.svn_key_ba)
			{
				highest_svn_key = cd.svn_key_ba;
			}
		}
		out_svn_key = highest_svn_key;
		return ret;
	}

	public bool MergeChunkData(List<B45ChunkDataBase> cdl)
	{
		if(fileBA.ReadHeader() == false)
		{
			// init cdl's uv data
			for(int i = 0; i < cdl.Count; i++){
				cdl[i].InitUpdateVectors();
				cdl[i].svn_key_ba = 1;
				
//				MonoBehaviour.print("first write  " + cdl[i].OccupiedVecsStr());
			}
			
			fileBA.WriteBAHeader(false, cdl);
			
			fileUV.WriteUVHeader(false, cdl);
			return true;
		}
		
		int tmp_svnkey = 0;
		//List<B45ChunkDataBase> disk_cdl = fileBA.ReadAllChunkData(out tmp_svnkey, 0);
		List<B45ChunkDataBase> disk_cdl = ReadAllChunkData(out tmp_svnkey, 0);
		
		tmp_svnkey++;
		bool addedNewBA = false;
		for(int i = 0; i < cdl.Count; i++){
			B45ChunkDataBase cd = cdl[i];
			bool found = false;
			for(int j = 0; j < disk_cdl.Count; j++){
				B45ChunkDataBase disk_cd = disk_cdl[j];
				if(cd._chunkPos.Equals(disk_cd._chunkPos))
				{
//					MonoBehaviour.print("(svn) replace " + disk_cd._chunkPos.toDBStr() + " rev " + disk_cd.svn_key + " with " + svn_key);
//					MonoBehaviour.print("disk "+disk_cd.OccupiedVecsStr());
//					MonoBehaviour.print("mem  " + cd.OccupiedVecsStr());
					
					List<UpdateVector> uvs = computeDifferences(disk_cd, cd);
					if(disk_cd.updateVectors == null){
						disk_cd.updateVectors = new List<UpdateVector>();
						
					}
					if(disk_cd.uvVersionKeys == null){
						
						disk_cd.uvVersionKeys = new List<UVKeyCount>();
						UVKeyCount uvkc = new UVKeyCount();
						uvkc.svn_key = 0;
						uvkc.count = 0;
						disk_cd.uvVersionKeys.Add(uvkc);
					}
					
					int newkey = MergeUV(uvs, disk_cd.updateVectors, disk_cd.uvVersionKeys);
					//disk_cd.svn_key_ba = 
					disk_cd.svn_key = newkey;
					found = true;
				}
			}
			if(found == false){
//				MonoBehaviour.print("(svn) add " + cd._chunkPos.toDBStr() + " rev " + svn_key);
				addedNewBA = true;
				cd.InitUpdateVectors();
				cd.svn_key = tmp_svnkey;
				disk_cdl.Add(cd);
			}
		}
		if(addedNewBA)
			fileBA.WriteBAHeader(false, disk_cdl);
		fileUV.WriteUVHeader(false, disk_cdl);
		
		return true;
	}
	// uvs1 are from the chunks in memory.
	// uvs2, kcs2 are from the disk.
	public static int MergeUV(List<UpdateVector> uvs1, List<UpdateVector> uvs2, List<UVKeyCount> kcs2)
    {
        int latest_svn_key = kcs2[kcs2.Count - 1].svn_key;
        UVKeyCount newKC = new UVKeyCount();
        newKC.svn_key = latest_svn_key + 1;
        newKC.count = 0;

        int kc2Ptr = 0;
        int currentKeyCount = kcs2[kc2Ptr].count;
        int currentKeyCountI = 0;
        byte[] found = new byte[uvs1.Count];
        int olduvs2Count = uvs2.Count;
        for (int j = 0; j < olduvs2Count; j++)
        {

            int idxFrom2 = uvs2[j].xyz0 + (uvs2[j].xyz1 << 8);
            for (int i = 0; i < uvs1.Count; i++)
            {
                int idxFrom1 = uvs1[i].xyz0 + (uvs1[i].xyz1 << 8);
                if (idxFrom1 == idxFrom2)
                {
                    found[i] = 1;
                    // further compare the voxel bytes.
                    // if they are different, it means one is a newer update. it should supercede the other.
                    if (uvs1[i].voxelData0 != uvs2[j].voxelData0 ||
                        uvs1[i].voxelData1 != uvs2[j].voxelData1)
                    {
                        uvs2[j].voxelData0 = uvs1[i].voxelData0;
                        uvs2[j].voxelData1 = uvs1[i].voxelData1;
                        uvs2.Add(uvs2[j]);

                        uvs2[j] = null;

                        kcs2[kc2Ptr].count--;

                        newKC.count++;
                    }
                }
            }
            currentKeyCountI++;
            if (currentKeyCountI >= currentKeyCount)
            {
                currentKeyCountI = 0;
                kc2Ptr++;
                if(kc2Ptr < kcs2.Count)
                    currentKeyCount = kcs2[kc2Ptr].count;
            }

        }

        // condense uvs2
        int adjustment = 0;
        for (int i = 0; i < uvs2.Count; i++)
        {
            if (uvs2[i] != null )
            {
                if (adjustment != 0)
                {
                    uvs2[i - adjustment] = uvs2[i];
                    uvs2[i] = null;
                }
            }
            else
                adjustment++;


        }
        // take out the nulls in uvs2
        for (int i = uvs2.Count - 1; i >= 0; i--)
        {
            if (uvs2[i] == null)
                uvs2.RemoveAt(i);
            else
                break;
        }
        // add the new uvs.
        for (int i = 0; i < uvs1.Count; i++)
        {
            if (found[i] == 0)
            {
                UpdateVector newUV = new UpdateVector();
                newUV.xyz0 = uvs1[i].xyz0;
                newUV.xyz1 = uvs1[i].xyz1;
                newUV.voxelData0 = uvs1[i].voxelData0;
                newUV.voxelData1 = uvs1[i].voxelData1;
                uvs2.Add(newUV);

                newKC.count++;
            }
        }

        // take out the 0 in kcs2
        for (int i = kcs2.Count - 1; i >= 0; i--)
        {
            if (kcs2[i].count == 0)
                kcs2.RemoveAt(i);
            else
                break;
        }
        kcs2.Add(newKC);
		return latest_svn_key + 1;
    }
   

	// yield to cd2
	List<UpdateVector> computeDifferences(B45ChunkDataBase cd1, B45ChunkDataBase cd2)
	{
		List<UpdateVector> ret = new List<UpdateVector>();
		for(int idx = 0; idx < Block45Constants.VOXEL_ARRAY_LENGTH; idx++)
		{
			if(cd1._chunkData[idx*2] != cd2._chunkData[idx*2] || 
				cd1._chunkData[idx*2 + 1] != cd2._chunkData[idx*2 + 1]){
				UpdateVector vu = new UpdateVector();
				vu.xyz0 = (byte) (idx & 0xFF);
				vu.xyz1 = (byte) ((idx >> 8) & 0xFF);
				vu.voxelData0 = cd2._chunkData[idx * 2];
				vu.voxelData1 = cd2._chunkData[idx * 2 + 1];
				ret.Add(vu);
			}

		}
		return ret;
	}
}
