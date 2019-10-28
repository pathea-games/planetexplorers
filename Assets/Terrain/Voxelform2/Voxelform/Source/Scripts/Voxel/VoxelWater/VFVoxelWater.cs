//#define RIVER_DATA_FILE_TST
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class VFVoxelWater : MonoBehaviour, IVxChunkHelperProc, ILODNodeDataMan
{
	public const string ArchiveKey = "ArchiveKeyVoxelWater";

	public const byte c_iSeaWaterType = (byte)VFVoxel.EType.WaterSourceBeg;
	public const byte c_iSurfaceVol = 128;
	public const int  c_Sig = 1;

	public static VFVoxelWater self;
	public static float c_fWaterLvl = 97.0f;
	public static byte[][] s_surfaceChunkData = null;
	public static VFVoxel c_WaterSource = new VFVoxel(255,128);	
	public static bool s_bSeaInSight;	// For refelction setting
	public static int s_layer;

	public VFVoxelSave SaveLoad;

	[SerializeField]
	private Material _waterMat;
	public Material WaterMat
	{
		get{ return _waterMat;	}
		set{
			if(_waterMat != value)
			{
				_waterMat = value;
				MeshRenderer[] mrs = gameObject.GetComponentsInChildren<MeshRenderer>();
				int n = mrs.Length;
				for(int i = 0; i < n; i++)
				{
					mrs[i].sharedMaterial = _waterMat;
				}
			}
		}
	}

#if RIVER_DATA_FILE_TST
	private Dictionary<IntVector4, string> _riverChunkFileList = new Dictionary<IntVector4, string>();
#endif
	private byte[] _tmpSurfChunkData4Req = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
	private MCOutputData _tmpSeaSurfaceVerts = new MCOutputData(null, null, null, null);
	private MCOutputData[] _seaSurfaceVerts = new MCOutputData[LODOctreeMan.MaxLod+1];
	private List<SurfExtractReqMC> _seaSurfaceChunkReqs = new List<SurfExtractReqMC>();
	private List<IntVector4> _dirtyChunkPosList = new List<IntVector4>();
	private EulerianFluidProcessor _fluidProcessor;
	private IVxDataSource _voxels;
	private IVxDataLoader _dataLoader;
	public  IVxDataSource Voxels{	get{	return _voxels;					}	}

#if UNITY_EDITOR
	public int wx = 0, wy = 0, wz = 0;
	public int wsize = 2;
	public bool bWriteWaterAndFile = false;
	public bool bWriteWater = false;
	public bool bGenFiles = false;
	public int Viscosity = 4;
	public int Iteration = 8;
	private List<VFVoxelChunkData> dirtyWaterChunks = new List<VFVoxelChunkData>();
#endif

#region SpecialReq4SeaSurface
	void ProcGenSeaSurface(SurfExtractReqMC req)
	{
		_tmpSeaSurfaceVerts.Clone (_seaSurfaceVerts [req._chunk.LOD]);	// To avoid outputdata(_seaSurfaceVerts) from clearing
		req.AddOutData(_tmpSeaSurfaceVerts);

		VFVoxelChunkGo vfGo = VFVoxelChunkGo.CreateChunkGo(req, _waterMat, s_layer);
		vfGo.transform.parent = gameObject.transform;
		req._chunkData = _tmpSurfChunkData4Req;
		req._chunk.AttachChunkGo(vfGo, req);
	}
#endregion


	void Awake()
	{
		self = this;
		s_bSeaInSight = false;
		s_layer = Pathea.Layer.Water;
    }
    public void Import(Pathea.PeRecordReader r)
	{	
		ApplyQuality(SystemSettingData.Instance.WaterRefraction, SystemSettingData.Instance.WaterDepth);

		SaveLoad = new VFVoxelSave(ArchiveKey, AddtionalReader, AddtionalWriter);
        SaveLoad.Import(r);
		if(s_surfaceChunkData == null)
		{
			InitSufaceChunkData();
		}

#if RIVER_DATA_FILE_TST
		// Load river chunk pos
		River2Voxel.ReadRiverChunksList(ref _riverChunkFileList);
#endif
	}
	void LateUpdate()
	{
		s_bSeaInSight = false;
	}
	void OnDestroy()
	{
		if(SurfExtractor != null)
		{
			SurfExtractor.Reset();
		}
		if(_dataLoader != null){
			_dataLoader.Close();
		}
		c_fWaterLvl = 97.0f; // reset to default
		Debug.Log("Mem size before vfTerrain destroyed all chunks :"+GC.GetTotalMemory(true));
		// If something is left over... (in the editor)
		if (gameObject.transform.childCount > 0)
		{
			// Clean up chunks
			for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
			{
				var child = gameObject.transform.GetChild(i).gameObject;
				DestroyImmediate(child);
			}
		}
		Resources.UnloadUnusedAssets();
		self = null;
		VFVoxelSave.Clean();
	}

	void StartFluidProcessor(string para)
	{
		StartCoroutine(CoFuildProcess());
	}
	IEnumerator CoFuildProcess()
	{
		while(true)
		{
#if UNITY_EDITOR
			_fluidProcessor.Viscosity = Viscosity;
			if(bWriteWaterAndFile)
			{
				int x = 0, y = 0, z = 0;
				for(z = -wsize; z <= wsize; z++)
				{
					//for(y = -1; y <= 1; y++)
					{
						for(x = -wsize; x <= wsize; x++)
						{
							Voxels.Write(wx+x,wy+y,wz+z,c_WaterSource);
						}
					}
				}
				int n = Voxels.DirtyChunkList.Count;
				for(int i = 0; i < n; i++)
				{
					VFVoxelChunkData chunkData = Voxels.DirtyChunkList[i];
					// Write to file
					string fileName = "/water_x"+chunkData.ChunkPosLod.x+"_y"+chunkData.ChunkPosLod.y+"_z"+chunkData.ChunkPosLod.z+"_"+chunkData.ChunkPosLod.w+".chnk";
					using(FileStream fs = new FileStream(River2Voxel.outputDir+"/"+fileName,FileMode.Create))
					{
						fs.Write(chunkData.DataVT, 0, chunkData.DataVT.Length);
					}
				}
				bWriteWaterAndFile = false;
			}
			if(bWriteWater)
			{
				int x = 0, y = 0, z = 0;
				for(z = -wsize; z <= wsize; z++)
				{
					//for(y = -1; y <= 1; y++)
					{
						for(x = -wsize; x <= wsize; x++)
						{
							Voxels.Write(wx+x,wy+y,wz+z,c_WaterSource);
						}
					}
				}
				int n = Voxels.DirtyChunkList.Count;
				for(int i = 0; i < n; i++)
				{
					VFVoxelChunkData chunk = Voxels.DirtyChunkList[i];
					if(!_fluidProcessor.DirtyChunkPosList.Contains(chunk.ChunkPosLod))
					{
						_fluidProcessor.DirtyChunkPosList.Add(chunk.ChunkPosLod);
					}
				}
				bWriteWater = false;
			}
			if(bGenFiles)
			{
				if(SurfExtractor.IsAllClear)
				{
					_fluidProcessor.UpdateFluidConstWater(true);
				}
				int n = _fluidProcessor.DirtyChunkList.Count;
				for(int i = 0; i < n; i++)
				{
					VFVoxelChunkData chunkData = _fluidProcessor.DirtyChunkList[i];
					if(!dirtyWaterChunks.Contains(chunkData))
					{
						dirtyWaterChunks.Add(chunkData);
					}
				}
				n = dirtyWaterChunks.Count;
				for(int i = n-1; i >= 0; i--)
				{
					VFVoxelChunkData chunkData = dirtyWaterChunks[i];
					if(chunkData.DataVT.Length > VFVoxel.c_VTSize && !_fluidProcessor.DirtyChunkList.Contains(chunkData))
					{
						// Write to file
						string fileName = "/water_x"+chunkData.ChunkPosLod.x+"_y"+chunkData.ChunkPosLod.y+"_z"+chunkData.ChunkPosLod.z+"_"+chunkData.ChunkPosLod.w+".chnk";
						using(FileStream fs = new FileStream(River2Voxel.outputDir+"/"+fileName,FileMode.Create))
						{
							fs.Write(chunkData.DataVT, 0, chunkData.DataVT.Length);
						}
						dirtyWaterChunks.RemoveAt(i);
					}
				}
				yield return 0;
				continue;
			}
#endif
			if(SurfExtractor.IsAllClear && VFVoxelTerrain.TerrainVoxelComplete)
			{
				_fluidProcessor.UpdateFluid(true);
				int n = _fluidProcessor.DirtyChunkList.Count;
				for(int i = 0; i < n; i++)
				{
					SaveLoad.AddChunkToSaveList(_fluidProcessor.DirtyChunkList[i]);
				}
			}
			yield return 0;
		}
	}
	public void OnWriteVoxelOfTerrain(LODOctreeNode node, byte oldVol, byte newVol, int idxVol)
	{
		if(newVol < oldVol)
		{
			IntVector4 chunkPos = new IntVector4(node.CX, node.CY, node.CZ, node.Lod);
			if(!_fluidProcessor.DirtyChunkPosList.Contains(chunkPos))
			{
				_fluidProcessor.DirtyChunkPosList.Add(chunkPos);
			}
		}
	}
	private void AddtionalReader(BinaryReader br)
	{
		_dirtyChunkPosList = new List<IntVector4>();
		int n = br.ReadInt32();
		for(int i = 0; i < n; i++)
		{
			int x = br.ReadInt32();
			int y = br.ReadInt32();
			int z = br.ReadInt32();
			int w = br.ReadInt32();
			_dirtyChunkPosList.Add(new IntVector4(x,y,z,w));
		}
	}
	private void AddtionalWriter(BinaryWriter bw)
	{
		int n = _fluidProcessor.DirtyChunkPosList.Count;
		bw.Write(n);
		for(int i = 0; i < n; i++)
		{
			IntVector4 chunkPos = _fluidProcessor.DirtyChunkPosList[i];
			bw.Write(chunkPos.x);
			bw.Write(chunkPos.y);
			bw.Write(chunkPos.z);
			bw.Write(chunkPos.w);
		}
	}
	public void ApplyQuality(bool opt0, bool opt1)
	{
//		_waterMat = (opt0&&opt1) ? _waterMatHQ : 
//					(opt0||opt1) ? _waterMatMQ : _waterMatLQ;
	}

	public bool IsInWater(Vector3 pos)
	{
		return IsInWater(pos.x, pos.y, pos.z);
	}

	public bool IsInWater(float fx, float fy, float fz)
	{
		try{
			if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.PajaShip||RandomDunGenUtil.IsDungeonPosY(fy))
            {
                float rayLen = 50;
                return Physics.Raycast(new Vector3(fx, fy + rayLen, fz), Vector3.down, rayLen, 1 << Pathea.Layer.Water);
            }
			if(Pathea.PeGameMgr.IsMulti)
			{
				if(PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand)// PlayerNetwork.MainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.PajaShip)
				{
					float rayLen = 50;
					return Physics.Raycast(new Vector3(fx, fy + rayLen, fz), Vector3.down, rayLen, 1 << Pathea.Layer.Water);
				}
			}
			if(_voxels == null)
				return false;

			int x = Mathf.RoundToInt(fx);
			int y = Mathf.RoundToInt(fy);
			int z = Mathf.RoundToInt(fz);
			int cx = x >> VoxelTerrainConstants._shift;
			int cy = y >> VoxelTerrainConstants._shift;
			int cz = z >> VoxelTerrainConstants._shift;
			VFVoxelChunkData chunk = _voxels.readChunk(cx, cy, cz);
			if(chunk == null || chunk.DataVT.Length < VFVoxel.c_VTSize){
				return c_fWaterLvl > fy;
				//return false;
			}
			byte cVol;
			if(chunk.IsHollow)
			{
				cVol = chunk.DataVT[0];
				if(cVol == 0)	return false;
				if(cVol == 255)	return true;
				return c_fWaterLvl > fy;
			}
			int idx = VFVoxelChunkData.OneIndex(x&VoxelTerrainConstants._mask,
			                                    y&VoxelTerrainConstants._mask,
			                                    z&VoxelTerrainConstants._mask);
			int idxVT = idx<<VFVoxel.c_shift;
			cVol = chunk.DataVT[idxVT];
			if(cVol == 0)	return false;
			if(cVol == 255)	return true;
			byte oVol;
			if(cVol < 128)
			{
				oVol = chunk.DataVT[idxVT-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
				if(oVol >= 128) return ((128-oVol)/(float)(cVol-oVol)+y-1) > fy;
				oVol = chunk.DataVT[idxVT+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
				if(oVol >= 128) return ((128-cVol)/(float)(oVol-cVol)+y) < fy;
				return false;
			}
			else
			{
				oVol = chunk.DataVT[idxVT-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
				if(oVol < 128) return ((128-oVol)/(float)(cVol-oVol)+y-1) < fy;
				oVol = chunk.DataVT[idxVT+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
				if(oVol < 128) return ((128-cVol)/(float)(oVol-cVol)+y) > fy;
				return true;
			}
		}
		catch
		{
			//Debug.LogError("[WATER]:error on testing IsInWater:"+fx+","+fy+","+fz);
		}
		return false;
	}
	public float DownToWaterSurface(float fx, float fy, float fz)
	{
		int x = Mathf.RoundToInt(fx);
		int y = Mathf.RoundToInt(fy);
		int z = Mathf.RoundToInt(fz);
		int cx = x >> VoxelTerrainConstants._shift;
		int cy = y >> VoxelTerrainConstants._shift;
		int cz = z >> VoxelTerrainConstants._shift;
		int yInChunk = y&VoxelTerrainConstants._mask;
		int idxBase = VFVoxelChunkData.OneIndex(x&VoxelTerrainConstants._mask, 31, z&VoxelTerrainConstants._mask)<<VFVoxel.c_shift;
		int idx = idxBase - (31-yInChunk)*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
		while(cy >= 0)
		{
			VFVoxelChunkData chunk = _voxels.readChunk(cx, cy, cz);
			if(chunk == null || chunk.DataVT.Length < VFVoxel.c_VTSize){
				return fy - c_fWaterLvl;
				//return -1;
			}
			if(chunk.IsHollow)
			{
				byte vol = chunk.DataVT[0];
				if(vol == 0){ cy--; yInChunk = 31; idx = idxBase;		continue;	}
				if(vol == 255){											return -1;	}
				return fy - c_fWaterLvl;
			}
			byte[] data = chunk.DataVT;
			byte cVol, uVol;
			while(yInChunk >= 0)
			{
				cVol = data[idx];
				if(cVol>=128)
				{
					uVol = data[idx+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
					if(uVol >= 128){									return -1;	}
					float fHInChunk = (128-cVol)/(float)(uVol-cVol)+yInChunk;
					return fy -((cy<<VoxelTerrainConstants._shift)+fHInChunk);
				}
				yInChunk--;
				idx -= VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
			}
			cy--; yInChunk = 31; idx = idxBase;
		}
		return -1;
	}
	public float UpToWaterSurface(float fx, float fy, float fz)
	{
		if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.PajaShip||RandomDunGenUtil.IsDungeonPosY(fy))
        {
            float rayLen = 20;
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(fx, fy + rayLen, fz), Vector3.down, out hit, rayLen, 1 << Pathea.Layer.Water))
                return Vector3.Distance(new Vector3(fx, fy, fz), hit.point);
			return -1;
        }
		if(Pathea.PeGameMgr.IsMulti)
		{
			if(PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand)// PlayerNetwork.MainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.PajaShip)
            {
				float rayLen = 20;
				RaycastHit hit;
				if (Physics.Raycast(new Vector3(fx, fy + rayLen, fz), Vector3.down, out hit, rayLen, 1 << Pathea.Layer.Water))
					return Vector3.Distance(new Vector3(fx, fy, fz), hit.point);
				return -1;
			}
		}
		int x = Mathf.RoundToInt(fx);
		int y = Mathf.RoundToInt(fy);
		int z = Mathf.RoundToInt(fz);
		int cx = x >> VoxelTerrainConstants._shift;
		int cy = y >> VoxelTerrainConstants._shift;
		int cz = z >> VoxelTerrainConstants._shift;
		int yInChunk = y&VoxelTerrainConstants._mask;
		int idxBase = VFVoxelChunkData.OneIndex(x&VoxelTerrainConstants._mask, 0, z&VoxelTerrainConstants._mask)<<VFVoxel.c_shift;
		int idx = idxBase + yInChunk*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
		while(cy < 32)	//tmp use 32 insread of 92
		{
			VFVoxelChunkData chunk = _voxels.readChunk(cx, cy, cz);
			if(chunk == null || chunk.DataVT.Length < VFVoxel.c_VTSize){
				return c_fWaterLvl - fy;
				//return -1;
			}
			if(chunk.IsHollow)
			{
				byte vol = chunk.DataVT[0];
				if(vol == 255){	cy++; yInChunk = 0; idx = idxBase;		continue;	}
				if(vol == 0){											return -1;	}
				return c_fWaterLvl - fy;
			}
			byte[] data = chunk.DataVT;
			byte cVol, dVol;
			while(yInChunk < VoxelTerrainConstants._numVoxelsPerAxis)
			{
				cVol = data[idx];
				if(cVol<128)
				{
					dVol = data[idx-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT];
					if(dVol < 128){										return -1;	}
					float fHInChunk = (128-dVol)/(float)(cVol-dVol)+yInChunk-1;
					return ((cy<<VoxelTerrainConstants._shift)+fHInChunk) - fy;
				}
				yInChunk++;
				idx += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
			}
			cy++; yInChunk = 0; idx = idxBase;
		}
		return -1;
	}
	private void SetSurfaceMeshes()
	{	
		lock(_seaSurfaceChunkReqs)
		{
			int n = _seaSurfaceChunkReqs.Count;
			for(int i = 0; i < n; i++)
			{
				_seaSurfaceChunkReqs[i].OnReqFinished();
			}
			_seaSurfaceChunkReqs.Clear();
		}
	}
	// Delegates
	public void OnWaterDataLoad(VFVoxelChunkData chunkData, byte[] chunkDataVT, bool bFromPool)
	{
		if(chunkDataVT.Length != VFVoxel.c_VTSize || chunkDataVT[0] != c_iSurfaceVol){
			// not surface chunk
			chunkData.OnDataLoaded(chunkDataVT, bFromPool);
			return;
		}

		// surface chunk
		int step = 1<<chunkData.LOD;
		if(c_fWaterLvl >= ((chunkData.ChunkPosLod.y+step)<<VoxelTerrainConstants._shift)){
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataWaterSolid);
			return;
		}

		if(_seaSurfaceVerts[chunkData.LOD] == null){
			float fy = (c_fWaterLvl - (chunkData.ChunkPosLod.y << VoxelTerrainConstants._shift))/step;
			Vector3[] pos    = new Vector3[4]{
				new Vector3( 0.0f, fy,  0.0f),
				new Vector3( 0.0f, fy, 32.0f),
				new Vector3(32.0f, fy,  0.0f),
				new Vector3(32.0f, fy, 32.0f),
			};
			Vector2[] norm01 = new Vector2[4]{
				new Vector2(),new Vector2(),new Vector2(),new Vector2(),
			};
			Vector2[] norm2t = new Vector2[4]{
				new Vector2(),new Vector2(),new Vector2(),new Vector2(),
			};
			int[] indice = new int[6]{0,1,2,1,3,2};
			MCOutputData chunkMeshData = new MCOutputData(pos, norm01, norm2t, indice);
			_seaSurfaceVerts[chunkData.LOD] = chunkMeshData;
		}
		lock(_seaSurfaceChunkReqs)
		{
			_seaSurfaceChunkReqs.Add(SurfExtractReqMC.Get(chunkData, ProcGenSeaSurface));
		}
		// if surface chunk data not the following format, we should set it to this
		//chunkDataVT[0] = c_iSurfaceVol;
		//chunkDataVT[1] = c_iSeaWaterType;
		chunkData.SetDataVT(chunkDataVT);
	}

#region IVxChunkHelperProc_ILODNodeDataMan
	private int _idxInLODNodeData = 0;
	public int IdxInLODNodeData{	get{return _idxInLODNodeData;} set{_idxInLODNodeData = value;}	}
	public LODOctreeMan LodMan{	get;set;}
	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		VFVoxelChunkData data = new VFVoxelChunkData(node);
		data.HelperProc = this;
		return data;
	}
	public void ProcPostLodInit()
	{
		_dataLoader = VFVoxelTerrain.RandomMap ? VFVoxelTerrain.self.DataLoader : new VFDataReader(string.IsNullOrEmpty(VFVoxelTerrain.MapDataPath_Zip) ? string.Empty : (VFVoxelTerrain.MapDataPath_Zip + "/water"), OnWaterDataLoad);
		_voxels = new VFLODDataSource(LodMan.LodTreeNodes, IdxInLODNodeData);
		_fluidProcessor = new EulerianFluidProcessor();
		_fluidProcessor.DirtyChunkPosList.AddRange(_dirtyChunkPosList);
		
		StartCoroutine(CoFuildProcess());
	}
	public void ProcPostLodUpdate()
	{
		s_bSeaInSight = false;
		SetSurfaceMeshes();
	}
	public void ProcPostLodRefresh()
	{
		VFVoxelChunkData.EndAllReqs();
		_dataLoader.ProcessReqs();
	}

	public IVxSurfExtractor SurfExtractor{ get{	return SurfExtractorsMan.VxSurfExtractor;	} }
	public int  ChunkSig{ get{ return c_Sig;	}	}
	public void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool)
	{
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		if((!GameConfig.IsMultiMode) && data == VFVoxelChunkData.S_ChunkDataNull && SaveLoad.ChunkSaveList.Contains(cdata))
		{
			SaveLoad.SaveChunksInListToTmpFile();
		}
	}
	public void ChunkProcPreLoadData(ILODNodeData ndata)
	{
		VFVoxelChunkData chunk = ndata as VFVoxelChunkData;
#if RIVER_DATA_FILE_TST
		if(_riverChunkFileList.ContainsKey(chunk.ChunkPos))
		{
			Debug.Log("RiverChunk:"+chunk.ChunkPos);
			chunk.OnDataLoaded(File.ReadAllBytes(_riverChunkFileList[chunk.ChunkPos]), false);
			return;
		}
#endif
		byte[] data = SaveLoad.TryGetChunkData(chunk.ChunkPosLod);
		if(data != null)
		{
			chunk.OnDataLoaded(data, true);
		}
		else
		{
			_dataLoader.AddRequest(chunk);
		}
	}
	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		byte volume = cdata.DataVT[0];
		if(volume == c_iSurfaceVol)		return false;
		//Array.Copy(s_surfaceChunkData[cdata.LOD], data, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);

		VFVoxelChunkData.ExpandHollowChunkData(cdata);
		return true;
	}
	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
		byte volume = cdata.DataVT[0];
		byte type = cdata.DataVT[1];
		if(volume != c_iSurfaceVol)	return new VFVoxel(volume, type);
		// Process water level
		int posy = y + (cdata.ChunkPosLod.y << VoxelTerrainConstants._shift);
		float fWaterLvl = cdata.LOD == 0 ? c_fWaterLvl : c_fWaterLvl/(1<<cdata.LOD);
		if(fWaterLvl >= posy+0.5f)				return new VFVoxel(255, c_iSeaWaterType);
		if(fWaterLvl <= posy-0.5f)				return new VFVoxel(0, 0);
		if(Mathf.Abs(fWaterLvl - posy) < 0.1f)	return new VFVoxel(128, c_iSeaWaterType);
		if(fWaterLvl > posy)					return new VFVoxel((byte)(128+(fWaterLvl-posy)*128), c_iSeaWaterType);
		else     								return new VFVoxel((byte)(255-128/(fWaterLvl-posy+1)), c_iSeaWaterType);
	}
	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqMC req = ireq as SurfExtractReqMC;
		VFVoxelChunkGo vfGo = VFVoxelChunkGo.CreateChunkGo(req, _waterMat, s_layer);
		if(vfGo != null){
			vfGo.transform.parent = gameObject.transform;
		}
		req._chunk.AttachChunkGo(vfGo, req);
	}
	public void OnBegUpdateNodeData(ILODNodeData ndata){}
	public void OnEndUpdateNodeData(ILODNodeData ndata){}
	public void OnDestroyNodeData(ILODNodeData ndata){}
#endregion
#region Collider
	static int _frameCnt;
	void FixedUpdate()
	{
		if (LodMan != null && LodMan.Observer != null && _frameCnt != Time.frameCount && VFVoxelTerrain.bChunkColliderRebuilding)	//water is low prior
		{
			_frameCnt = Time.frameCount;
			Vector3 posCurrent = LodMan.Observer.position;
			int cx=((int)posCurrent.x)>>VoxelTerrainConstants._shift;
			int cy=((int)posCurrent.y)>>VoxelTerrainConstants._shift;
			int cz=((int)posCurrent.z)>>VoxelTerrainConstants._shift;
			int nOffset = VFVoxelTerrain.OrderedOffsetList.Count;
			for(int n = 0; n < nOffset; n++)
			{
				IntVector3 vecOffset = VFVoxelTerrain.OrderedOffsetList[n];
				VFVoxelChunkData chunk = _voxels.readChunk(vecOffset.x+cx, vecOffset.y+cy, vecOffset.z+cz );
				VFVoxelChunkGo vfgo = VFVoxelTerrain.GenOneCollider(chunk);
				if(null != vfgo)
				{
					vfgo.OnColliderReady();
					break;
				}
			}
		}
	}
#endregion

	public static void InitSufaceChunkData()
	{
		s_surfaceChunkData = new byte[LODOctreeMan.MaxLod+1][];
		for(int i = 0; i <= LODOctreeMan.MaxLod; i++)
		{
			float fWaterLvlLod = c_fWaterLvl/(1<<i);
			int iWaterLvlLod = (int)(fWaterLvlLod+0.5f);
			float fWaterLvlLodDec = fWaterLvlLod - (int)fWaterLvlLod;
			int iy = (iWaterLvlLod&VoxelTerrainConstants._mask) + VoxelTerrainConstants._numVoxelsPrefix;
			byte vol = fWaterLvlLodDec < 0.5f ? (byte)(256.0f*0.5f/(1-fWaterLvlLodDec)) : (byte)(255.999f*(1-0.5f/fWaterLvlLodDec));
			s_surfaceChunkData[i] = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
			byte[] curChunkData = s_surfaceChunkData[i];
			int idx = 0;
			int above = (VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - iy - 1) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
			for(int z = 0; z < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; z++)
			{
				// Fill data below surface
				for(int y = 0; y < iy; y++)
				{
					for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++)
					{
						curChunkData[idx++] = 255;
						curChunkData[idx++] = c_iSeaWaterType;
					}
				}
				// Fill data of surface
				for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++)
				{
					curChunkData[idx++] = vol;
					curChunkData[idx++] = c_iSeaWaterType;
				}
				// default value(0) used for those above surface
				idx += above;
			}
		}
	}
	public static void ExpandSurfaceChunkData(VFVoxelChunkData cdata)
	{
		cdata.DataVT[0] = cdata.DataVT[1] = 0;
		VFVoxelChunkData.ExpandHollowChunkData(cdata);
		Array.Copy(s_surfaceChunkData[cdata.LOD], cdata.DataVT, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
	}
}
