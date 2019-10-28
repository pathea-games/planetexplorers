using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using VoxelMap;

public partial class PeCustomMenuc : EditorWindow
{
	//static float _progress = 0.1f;
	static int _nLeft = 0;
	[MenuItem("PeCustomMenu/DataMerge/Merge River VoxelData")]
	static void MergeRiverVoxelData()
	{
		//_progress = 0.0f;
		if(VFVoxelTerrain.MapDataPath_Zip == null){
			VFVoxelTerrain.MapDataPath_Zip = GameConfig.PEDataPath + GameConfig.MapDataDir_Zip + "/";
		}
		//bool bFrom0 = EditorUtility.DisplayDialog ("MergeRiver", "Merge 0-maxlod data or 1-maxlod data?", "From 0", "From 1");
		//int lodStart = bFrom0 ? 0 : 1;
		//EditorUtility.DisplayProgressBar("Start replacing river data ", "...", _progress);
		Debug.LogError("<color=green>Start merging river data at " + DateTime.Now.ToString("G") + " </color>");
		int lod0 = 0;
		int lod1 = LODOctreeMan.MaxLod+1; // include LODOctreeMan.MaxLod
		int x0 = 0;
		int x1 = VoxelTerrainConstants._mapFileCountX;
		int z0 = 0;
		int z1 = VoxelTerrainConstants._mapFileCountZ;
		//x0 = 1; x1 = 2;
		//z0 = 1; z1 = 2;
		_nLeft = (x1 - x0) * (z1 - z0) * (lod1 - lod0);
		for(int lod = lod0; lod < lod1; lod++)
		{
			for(int x = x0; x < x1; x++){
				for(int z = x0; z < z1; z++){
					WaterChunksMerger riverChunksGetter = new WaterChunksMerger("water_x*_"+lod+".chnk", x, z);
					riverChunksGetter.StartMerge();
				}
			}
		}
		//EditorUtility.ClearProgressBar();
	}
	public class WaterChunksMerger
	{
		private Thread _thread;
		private string _fileLog;
		private VFDataReader _waterReader;
		private VFDataReader _terraReader;
		private List<IntVector4> _chunkPosList;
		private Dictionary<IntVector4, List<string>> _riverChunkFileList;
		public WaterChunksMerger(string fileFilter, int fileX = -1, int fileZ = -1)
		{
			_fileLog = fileFilter+"["+fileX+","+fileZ+"]";
			_waterReader = new VFDataReader(VFVoxelTerrain.MapDataPath_Zip + "/water", OnChunkDataLoad, false);
			_terraReader = new VFDataReader(VFVoxelTerrain.MapDataPath_Zip + "/map", OnChunkDataLoad, false);
			River2Voxel.ReadRiverChunksList(ref _riverChunkFileList, fileFilter, fileX, fileZ);
			_chunkPosList = _riverChunkFileList.Keys.Cast<IntVector4>().ToList();
		}
		public void StartMerge()
		{
			_thread = new Thread(new ThreadStart(ExecMerge));
			_thread.Start();
		}
		void ExecMerge()
		{
			_waterReader.ReplaceChunkDatas(_chunkPosList, MergeWaterData);			
			Debug.LogError("<color=green>Succeed in merging "+_fileLog + " at " + DateTime.Now.ToString("G") + " </color>");
			_nLeft--;
			if (_nLeft == 0) {
				Debug.LogError("<color=green>All merging work finished!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</color>");
			}
		}
		byte[] MergeWaterData(IntVector4 chunkPos, byte[] baseChunkData)
		{
			List<string> files = _riverChunkFileList[chunkPos];
			int n = files.Count;
			if(n > 0)
			{
				byte[] chunkData = baseChunkData;
				for(int i = 0; i < n; i++)
				{
					chunkData = VFDataReader.MergeChunkData(chunkData, File.ReadAllBytes(files[i]), chunkPos.w);
				}

				if(chunkPos.w == 0){// || chunkData.Length > VFVoxel.c_VTSize){  //let waterchunk_lod(not full/empty) do minus
					VFVoxelChunkData tChunk = _terraReader.ReadChunkImm(chunkPos);
					chunkData = VFDataReader.MinusChunkData(chunkData, tChunk.DataVT, 0);
				}
				return chunkData;
			}
			return baseChunkData;
		}
	}
	private static void OnChunkDataLoad(VFVoxelChunkData chunkData, byte[] chunkDataVT, bool bFromPool) // do not add to buildLiat
	{
		chunkData.SetDataVT(chunkDataVT, bFromPool);
	}
}
