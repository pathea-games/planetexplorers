using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class River2Voxel
{
	private VFDataReader terraReader;
	private VFDataReader waterReader;
	public  static string outputDir = Application.dataPath+"/../Water";
	private string curOutputDir = "";
	private const int VolPlus = 32;
	public River2Voxel()
	{
        terraReader = new VFDataReader(VFVoxelTerrain.MapDataPath_Zip + "/map", OnChunkDataLoad);
		waterReader = new VFDataReader(VFVoxelTerrain.MapDataPath_Zip + "/water", OnChunkDataLoad);
	}
	public void GenRiverVoxels(AttachedRiverScript riverScript)
	{
		MeshCollider mc = riverScript.GetComponent<MeshCollider>();
		if(mc == null)
		{
			riverScript.CreateMesh(riverScript.riverSmooth);
			mc = riverScript.GetComponent<MeshCollider>();
		}
		if(mc == null || mc.sharedMesh == null)
		{
			Debug.LogError("Can not find mesh collider");
			return;
		}
		VFVoxelWater.InitSufaceChunkData();

		Bounds meshBound = mc.sharedMesh.bounds;
	#if UNITY_EDITOR
		EditorUtility.DisplayProgressBar("RiverVoxels...", "Starting gen...", 0);
	#endif
		Vector3 boundMin = meshBound.min;
		Vector3 boundMax = meshBound.max;
		IntVector4 cpos = new IntVector4();

		if(!System.IO.Directory.Exists(outputDir))
		{
			System.IO.Directory.CreateDirectory(outputDir);
		}
		curOutputDir = outputDir+"/"+riverScript.name;
		if(!System.IO.Directory.Exists(curOutputDir))
		{
			System.IO.Directory.CreateDirectory(curOutputDir);
		}
		string filePrefix = curOutputDir+"/water";

#if false //test
		GenARiverChunk(ref boundMin, ref boundMax, mc, new IntVector4(310,2,221,0), filePrefix);
#else
		//float prev = VoxelTerrainConstants._numVoxelsPrefix /(float)VoxelTerrainConstants._numVoxelsPerAxis;
		//float post = VoxelTerrainConstants._numVoxelsPostfix/(float)VoxelTerrainConstants._numVoxelsPerAxis;
		for(cpos.w = 0; cpos.w <= LODOctreeMan.MaxLod; cpos.w++)
		{
			int step = 1<<cpos.w;
			int lodPrefix = VoxelTerrainConstants._numVoxelsPrefix<<cpos.w;
			int lodpostfix = VoxelTerrainConstants._numVoxelsPostfix<<cpos.w;
			int sx = (((int)(boundMin.x)-lodPrefix )>>(VoxelTerrainConstants._shift+cpos.w))<<cpos.w;
			int sy = (((int)(boundMin.y)-lodPrefix )>>(VoxelTerrainConstants._shift+cpos.w))<<cpos.w;
			int sz = (((int)(boundMin.z)-lodPrefix )>>(VoxelTerrainConstants._shift+cpos.w))<<cpos.w;
			int ex = (((int)(boundMax.x)+lodpostfix)>>(VoxelTerrainConstants._shift+cpos.w))<<cpos.w;
			int ey = (((int)(boundMax.y)+lodpostfix)>>(VoxelTerrainConstants._shift+cpos.w))<<cpos.w;
			int ez = (((int)(boundMax.z)+lodpostfix)>>(VoxelTerrainConstants._shift+cpos.w))<<cpos.w;
			if(sy < 0)	sy = 0;
			int n = (((ex-sx)>>cpos.w)+1)*(((ey-sy)>>cpos.w)+1)*(((ez-sz)>>cpos.w)+1);
			float progressStep = 1.0f/n;
			float progress = 0.0f;

			for(cpos.x = sx; cpos.x <= ex; cpos.x+=step)
			{
				for(cpos.z = sz; cpos.z <= ez; cpos.z+=step)
				{
					for(cpos.y = sy; cpos.y <= ey; cpos.y+=step)
					{
					#if UNITY_EDITOR
						EditorUtility.DisplayProgressBar("RiverVoxels..."+cpos.w, "Gen "+cpos+"...", progress);
						progress += progressStep;
					#endif
						GenARiverChunk(ref boundMin, ref boundMax, mc, new IntVector4(cpos), filePrefix);
					}
				}
			}
		}
#endif
	#if UNITY_EDITOR
		EditorUtility.ClearProgressBar();
	#endif
	}

	private void GenARiverChunk(ref Vector3 boundMin, ref Vector3 boundMax, MeshCollider mc, IntVector4 chunkPos, string filePrefix)
	{
		int lodPrefix = VoxelTerrainConstants._numVoxelsPrefix<<chunkPos.w;
		int lodAxisSize = VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE<<chunkPos.w;
		int sx = (chunkPos.x<<VoxelTerrainConstants._shift)-lodPrefix;
		int sy = (chunkPos.y<<VoxelTerrainConstants._shift)-lodPrefix;
		int sz = (chunkPos.z<<VoxelTerrainConstants._shift)-lodPrefix;
		int ex = sx+lodAxisSize;
		int ey = sy+lodAxisSize;
		int ez = sz+lodAxisSize;
		int minx = Mathf.Max((int)boundMin.x, sx);
		int miny = Mathf.Max((int)boundMin.y, sy);
		int minz = Mathf.Max((int)boundMin.z, sz);
		int maxx = Mathf.Min((int)boundMax.x, ex);
		int maxy = Mathf.Min((int)boundMax.y, ey);
		int maxz = Mathf.Min((int)boundMax.z, ez);
		if(minx >= maxx || miny >= maxy || minz >= maxz)	return;
		//if(miny < VFVoxelWater.c_fWaterLvl)	miny = (int)VFVoxelWater.c_fWaterLvl;

		VFVoxelChunkData terraChunkData = terraReader.ReadChunkImm(chunkPos);
		byte[] terraData = terraChunkData.DataVT;

		VFVoxelChunkData waterChunkData0 = waterReader.ReadChunkImm(chunkPos);
		byte[] waterData0 = waterChunkData0.DataVT;

		//int idxOfs = 32*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
		//int yOverlap = VoxelTerrainConstants._numVoxelsPrefix + VoxelTerrainConstants._numVoxelsPostfix;

		Ray ray = new Ray();
		ray.direction = Vector3.down;
		Vector3 origin = new Vector3(0, boundMax.y + 0.5f, 0);
		float distance = boundMax.y-miny+1;
		RaycastHit hitInfo;
		int step = 1<<chunkPos.w;
		int mask = (-1)<<chunkPos.w;
		minx &= mask;
		miny &= mask;
		minz &= mask;
		int minYIdx = sy < 0 ? 1 : 0;	// ignore pos -1;
		int start = VFVoxelChunkData.OneIndexNoPrefix((minx-sx)>>chunkPos.w, 0, (minz-sz)>>chunkPos.w);
		bool bChunkDirty0 = false;
		bool bColDirty;
		int cur, y, idx, leftVol, tmpVol;
		for(int z = minz; z < maxz; z+=step,start+=VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED)
		{
			cur = start;
			origin.z = z;
			for(int x = minx; x < maxx; x+=step,cur++)
			{
				origin.x = x;
				ray.origin = origin;
				if(mc.Raycast(ray, out hitInfo, distance)
				   && hitInfo.point.y >= VFVoxelWater.c_fWaterLvl) // this check of y can be removed if neccessary
				{
					float fHitY = hitInfo.point.y;
					int iHitY = (int)(fHitY+0.5f*step);
					int iy = (iHitY - sy)>>chunkPos.w;
					if(iy >= 0)
					{
						bColDirty = false;
						if(iy >= VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE)
						{
							iy = VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE;
							y = (VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-1);
							idx = (cur<<1)+y*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
						}
						else
						{
							idx = (cur<<1)+iy*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
							leftVol = 255 - terraData[idx];
							byte surfVol = 0;
							if(leftVol > 0)
							{
								float fLodHitY = fHitY/step;
								float fLodHitYDec = fLodHitY - (int)fLodHitY;
								surfVol = fLodHitYDec < 0.5f ? (byte)(256.0f*0.5f/(1-fLodHitYDec)) : (byte)(255.999f*(1-0.5f/fLodHitYDec));
								if(surfVol > waterData0[idx])
								{ 
									waterData0[idx] = surfVol;
									waterData0[idx+1] = (byte)VFVoxel.EType.WaterSourceBeg;
									bColDirty = true;
								}
							}
							idx -= VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
							y = iy-1;
							if(bColDirty && y >= 0) //for seamlessness with terrain
							{
								if(surfVol < 128)
								{
									waterData0[idx] = 255;
									waterData0[idx+1] = (byte)VFVoxel.EType.WaterSourceBeg;
									idx -= VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
									y--;
								}
								else
								{
									waterData0[idx] = 128;
									waterData0[idx+1] = (byte)VFVoxel.EType.WaterSourceBeg;
								}
							}
						}
						bool bOverChanged = false, bTmpOverChange = false;
						for(; y >=minYIdx; y--, idx-=VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT)
						{
							bTmpOverChange = false;
							leftVol = 255 - terraData[idx];
							if(bOverChanged)		leftVol += VolPlus;
							if(leftVol > waterData0[idx])
							{
								if(!bOverChanged)	leftVol += VolPlus;
								if(leftVol > 255)
								{
									if(y+1 < iy)
									{
										tmpVol = waterData0[idx+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
										if(tmpVol < 255)
										{
											tmpVol += VolPlus;
											if(tmpVol > 255) tmpVol = 255;
											waterData0[idx+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT] = (byte)tmpVol;
											waterData0[idx+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT+1] = (byte)VFVoxel.EType.WaterSourceBeg;
										}
									}
									bTmpOverChange = true;
									leftVol = 255;
								}
								waterData0[idx] = (byte)(leftVol);
								waterData0[idx+1] = (byte)VFVoxel.EType.WaterSourceBeg;
								bColDirty = true;
							}
							bOverChanged = bTmpOverChange;
						}

						if(bColDirty)
						{
							bChunkDirty0 = true;
						}
					}
				}
			}
		}
		if(bChunkDirty0) WriteChunkToFile(filePrefix, waterChunkData0);

		waterChunkData0.ClearMem();
		terraChunkData.ClearMem();
	}
	private void WriteChunkToFile(string filePrefix, VFVoxelChunkData chunk)
	{
		byte[] data = chunk.DataVT;
		int len = data.Length;
		string filePathName = filePrefix+"_x"+chunk.ChunkPosLod.x+"_y"+chunk.ChunkPosLod.y+"_z"+chunk.ChunkPosLod.z+"_"+chunk.ChunkPosLod.w+".chnk";
		FileStream fs = File.Create(filePathName);
		fs.Write(data, 0, len);
		fs.Close();
	}
	private void OnChunkDataLoad(VFVoxelChunkData chunkData, byte[] chunkDataVT, bool bFromPool)
	{
		chunkData.SetDataVT(chunkDataVT, bFromPool);
		if(chunkData.IsHollow)
		{
			if(chunkData.DataVT[0] == 128)	VFVoxelWater.ExpandSurfaceChunkData(chunkData);
			else    						VFVoxelChunkData.ExpandHollowChunkData(chunkData);
		}
	}
	public static void ReadRiverChunksList(ref Dictionary<IntVector4, List<string>> riverChunkFileList, string fileFilter = "water_x*.chnk", int fileX = -1, int fileZ = -1)
	{
		if(riverChunkFileList == null) riverChunkFileList = new Dictionary<IntVector4, List<string>>();

		string dataDir = outputDir;
		if(Directory.Exists(dataDir))
		{
			string[] files = Directory.GetFiles(dataDir, fileFilter, SearchOption.AllDirectories);
			string[] delimiters = new string[] {"_x", "_y", "_z", "_", ".chnk"};
			int minX = fileX < 0 ? 0 : fileX*VoxelTerrainConstants._mapChunkCountXorZ;
			int maxX = fileX < 0 ? VoxelTerrainConstants._worldMaxCX : (fileX+1)*VoxelTerrainConstants._mapChunkCountXorZ;
			int minZ = fileZ < 0 ? 0 : fileZ*VoxelTerrainConstants._mapChunkCountXorZ;
			int maxZ = fileZ < 0 ? VoxelTerrainConstants._worldMaxCZ : (fileZ+1)*VoxelTerrainConstants._mapChunkCountXorZ;
			foreach(string file in files)
			{
				string[] strPos = Path.GetFileName(file).Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				try{
					int x = Convert.ToInt32(strPos[1]);
                    int y = Convert.ToInt32(strPos[2]);
                    int z = Convert.ToInt32(strPos[3]);
					int w = Convert.ToInt32(strPos[4]);
					if(x < minX || x >= maxX || z < minZ || z >= maxZ)
						continue;

					IntVector4 pos = new IntVector4(x,y,z,w);
					if(riverChunkFileList.ContainsKey(pos))
					{
						Debug.LogWarning("Warning in ReadChunkFileList: multiple same chunk at"+pos);
						riverChunkFileList[pos].Add(file);
					}
					else
					{
						riverChunkFileList[pos] = new List<string>();
						riverChunkFileList[pos].Add(file);
					}
				}
				catch
				{
					Debug.LogWarning("Warning in reading river chunk files : Unresolved chunk file name"+file);
				}
			}
		}
		return;
	}
}
