using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using RedGrass;
using Pathea.Maths;

public class PeGrassExport 
{
	public static string s_PrePath 
	{
		get { return  GameConfig.PEDataPath + "VoxelData/SubTerrains/"; }
	}

	public static string[] s_mergedFilePath =  new string[9]
	{
		s_PrePath+ "subTerG_x0_y0.~dat",
		s_PrePath + "subTerG_x1_y0.~dat",
		s_PrePath + "subTerG_x2_y0.~dat",
		s_PrePath + "subTerG_x0_y1.~dat",
		s_PrePath + "subTerG_x1_y1.~dat",
		s_PrePath + "subTerG_x2_y1.~dat",
		s_PrePath + "subTerG_x0_y2.~dat",
		s_PrePath + "subTerG_x1_y2.~dat",
		s_PrePath + "subTerG_x2_y2.~dat"
	};
	
	public static string[] s_orgnFilePath = new string[9]
	{
		s_PrePath + "subTerG_x0_y0.dat",
		s_PrePath + "subTerG_x1_y0.dat",
		s_PrePath + "subTerG_x2_y0.dat",
		s_PrePath + "subTerG_x0_y1.dat",
		s_PrePath + "subTerG_x1_y1.dat",
		s_PrePath + "subTerG_x2_y1.dat",
		s_PrePath + "subTerG_x0_y2.dat",
		s_PrePath + "subTerG_x1_y2.dat",
		s_PrePath + "subTerG_x2_y2.dat"
	};

	public static string s_EvniPath = "Evni/PeStoryEvni";

	[MenuItem("PeCustomMenu/DataMerge/Merge Modified Grass (wuyiqiu, new)")]
	public static void SaveToOriginalFile()
	{
		RedGrass.EvniAsset evni = Resources.Load(s_EvniPath) as RedGrass.EvniAsset; 
		if (evni == null)
			Debug.LogError("EvniAsset is missiing");

		// Open original subterrain data file
		FileStream[] orgnGrassFiles = new FileStream[9];
		int[,]  orgnOfsData = new int[9, evni.XZTileCount];
		
		for (int i = 0; i < 9; ++i)
		{
			orgnGrassFiles[i] = new FileStream(s_orgnFilePath[i], FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			BinaryReader _in = new BinaryReader(orgnGrassFiles[i]);
			for (int j = 0; j < evni.XZTileCount; ++j)
			{
				orgnOfsData[i,j]  = _in.ReadInt32();
			}
		}

		#region MERGE_CACHES
		// Merge the cache
		Dictionary<int, List<RedGrassInstance>> file_add_dic = new Dictionary<int, List<RedGrassInstance>>();

		string[] raw_file_path = Directory.GetFiles(GameConfig.GetUserDataPath() + GameConfig.CreateSystemData + "/Grasses/");
		Dictionary<int, Dictionary<INTVECTOR3, RedGrassInstance>> raw_file_add = new Dictionary<int, Dictionary<INTVECTOR3, RedGrassInstance>>();
		List<int> version_list = new List<int>();
		Dictionary<int,int> modify_file_index = new Dictionary<int, int>();
		List<string> tempfile_to_del = new List<string>();

		for (int i = 0; i < raw_file_path.Length; i++)
		{
			EditorUtility.DisplayProgressBar("Merging Grasses...", "Read raw data.. ("+(i+1).ToString()+" of "+raw_file_path.Length.ToString()+")", 0.2f*((float)(i)/(float)(raw_file_path.Length)));

			// the expanded-name must be "gs"
			if ( raw_file_path[i].Substring(raw_file_path[i].LastIndexOf(".") + 1) != "gs")
				continue;

			// Read the caches
			// Read the caches
			using( FileStream raw_files = new FileStream(raw_file_path[i], FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				BinaryReader _in = new BinaryReader(raw_files);
				int version = _in.ReadInt32();
				version_list.Add(version);
				int cnt = _in.ReadInt32();
				
				raw_file_add.Add(version, new Dictionary<INTVECTOR3, RedGrassInstance>());
				for (int j = 0; j < cnt; ++j)
				{
					RedGrassInstance rgi = new RedGrassInstance();
					rgi.ReadFromStream(_in);
					INTVECTOR3 index =  Utils.WorldPosToVoxelPos(rgi.Position);
					raw_file_add[version][index] = rgi;
				}
			}
			
			tempfile_to_del.Add(raw_file_path[i]);
		}

		if (version_list.Count == 0)
		{
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Merging Grasses", "Nothing changed !", "OK");
			return;
		}

		// Descending order the version_list
		version_list.Sort(delegate(int small, int big) { 
			if (small > big)
				return -1;
			else if (small == big)
				return 0;
			else
				return 1;
		} );

		// Merge cache to a list
		for (int i = 0; i < version_list.Count; ++i)
		{
			int version = version_list[i];
			
			
			// Add to list
			foreach (KeyValuePair<INTVECTOR3, RedGrassInstance> kvp in raw_file_add[version])
			{
				for (int j = i + 1; j < version_list.Count; ++j)
				{
					int l_version = version_list[j];
					if (raw_file_add[l_version].ContainsKey(kvp.Key))
						raw_file_add[l_version].Remove(kvp.Key);
				}
				
				// which the orgin file the vgi in
				Vector3 pos = kvp.Value.Position;
				INTVECTOR3 chunkPos = new INTVECTOR3((int)pos.x >> evni.SHIFT, 0 , (int)pos.z >> evni.SHIFT);
				INTVECTOR3 chunk32Pos = ChunkPosToPos32(chunkPos.x, chunkPos.z, evni);
				int f_index = FindOrginFileIndex(chunk32Pos, evni);
				if (f_index != -1)
				{
					if (file_add_dic.ContainsKey(f_index))
						file_add_dic[f_index].Add(kvp.Value);
					else
					{
						file_add_dic.Add(f_index, new List<RedGrassInstance>());
						file_add_dic[f_index].Add(kvp.Value);
					}
					
					if (!modify_file_index.ContainsKey(f_index))
						modify_file_index.Add(f_index, f_index);
				}
				
			}
		}
		#endregion

		#region READ_ORIGINAL_FILE & REFRESH_DATA

		// Read old original file data
		Dictionary<int, Dictionary<INTVECTOR3, RedGrassInstance>[]> old_datas = new Dictionary<int, Dictionary<INTVECTOR3, RedGrassInstance>[]>();

		foreach (int f_i in modify_file_index.Keys)
		{
			string fileName = s_orgnFilePath[f_i].Substring( s_orgnFilePath[f_i].LastIndexOf("/") + 1);

			old_datas.Add(f_i, new Dictionary<INTVECTOR3, RedGrassInstance>[evni.XZTileCount]);
			//Read now
			BinaryReader _in = new BinaryReader(orgnGrassFiles[f_i]);
			for (int j = 0; j < evni.XZTileCount; ++j)
			{
				if (j % 100 == 0)
					EditorUtility.DisplayProgressBar("Merging Grasses...", "Read file " + fileName +" data.." + (j+1).ToString()+" of "+ evni.XZTileCount.ToString()+")",
					                                 ((float)(j)/(float) evni.XZTileCount));

				old_datas[f_i][j] = new Dictionary<INTVECTOR3, RedGrassInstance>();

				if (orgnOfsData[f_i,j] == 0)
					continue;

				_in.BaseStream.Seek(orgnOfsData[f_i,j], SeekOrigin.Begin);
				int count = _in.ReadInt32();
				for (int k = 0; k < count; ++k)
				{
					RedGrassInstance vgi = new RedGrassInstance();
					vgi.ReadFromStream(_in);
					INTVECTOR3 index = Utils.WorldPosToVoxelPos(vgi.Position);
					old_datas[f_i][j][index] = vgi;
				}

			}
		}

		// Add

		foreach (int f_i in file_add_dic.Keys)
		{
			int _per = 0;
			foreach (RedGrassInstance rgi in file_add_dic[f_i])
			{
				INTVECTOR3 w_i = Utils.WorldPosToVoxelPos(rgi.Position);
				int fileX = f_i % evni.FileXCount;
				int fileZ = f_i / evni.FlieZCount;
				int f_startX = evni.XStart + fileX * evni.XTileCount * evni.Tile;
				int f_startZ = evni.ZStart + fileZ * evni.ZTileCount * evni.Tile;

				if (_per % 100 == 0)
					EditorUtility.DisplayProgressBar("Merging Grasses...", "Calculate the Grasses of file " + f_i.ToString() + "...",
					                                 ((float)(_per)/(float)file_add_dic[f_i].Count));

				int chunk_x = ((int)w_i.x  - f_startX) / evni.Tile;
				int chunk_z = ((int)w_i.z  - f_startZ) / evni.Tile;
				int key = chunk_x + chunk_z * evni.XTileCount;

				old_datas[f_i][key][w_i] = rgi;

				_per ++;
			}
		}

		file_add_dic.Clear();
		#endregion


		// Save to a series of Temp file
		foreach (int f_i in modify_file_index.Keys)
		{
			string file_name = s_mergedFilePath[f_i];

			using (FileStream fs = new FileStream(file_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				BinaryWriter _w = new BinaryWriter(fs);

				int[] offsets = new int[evni.XZTileCount];
				_w.Seek(evni.XZTileCount * 4, SeekOrigin.Begin);
				// Write Grass data
				for (int i = 0; i < evni.XZTileCount; i++)
				{
					if (i % 100 == 0)
						EditorUtility.DisplayProgressBar("Merging Grasses...", "Merge file data.. ("+(i+1).ToString()+" of "+ evni.XZTileCount.ToString()+")",
						                                 ((float)(i)/(float) evni.XZTileCount));

					offsets[i] = (int)fs.Position;

					if (old_datas[f_i][i].Count == 0)
					{
						offsets[i] = 0;
						continue;
					}

					_w.Write(old_datas[f_i][i].Count);
					foreach(RedGrassInstance rgi in old_datas[f_i][i].Values)
						rgi.WriteToStream(_w);
				}

				// write offset
				_w.Seek(0, SeekOrigin.Begin);
				for (int i = 0; i < evni.XZTileCount; i++)
					_w.Write(offsets[i]);

				fs.Close();
			}
		}

		// Merge complete
		EditorUtility.ClearProgressBar();
		if ( EditorUtility.DisplayDialog("Merge complete!", "Do you want to make it work ?", "Yes", "No") )
		{
			// Make it work 
			foreach (int f_i in modify_file_index.Keys)
			{
				File.Copy(s_mergedFilePath[f_i], s_orgnFilePath[f_i], true);
				
				// delete temp file
				File.Delete(s_mergedFilePath[f_i]);
				if (File.Exists(s_mergedFilePath[f_i] + ".meta"))
					File.Delete(s_mergedFilePath[f_i] + ".meta");

				foreach (string tmpfile in tempfile_to_del)
				{
					if ( File.Exists(tmpfile) )
					{
						File.Copy(tmpfile, tmpfile + ".bak", true);
						File.Delete(tmpfile);
					}
				}
			}
		}
		else
		{
			// Don't make it work 
			foreach (int f_i in modify_file_index.Keys)
			{
				File.Delete(s_mergedFilePath[f_i]);
				if (File.Exists(s_mergedFilePath[f_i] + ".meta"))
					File.Delete(s_mergedFilePath[f_i] + ".meta");
			}
		}

		// Close the file
		for (int i = 0; i < 9; ++i)
		{
			orgnGrassFiles[i].Close();
		}
	}

	#region HELP_FUNC
	public static INTVECTOR3 ChunkPosToPos32 (int x_index, int z_index, EvniAsset evni)
	{
		return new INTVECTOR3(x_index * (evni.CHUNKSIZE >> 5), 0, z_index * (evni.CHUNKSIZE >> 5));
	}
	
	public static int Pos32ToIndex32 (int x_32, int z_32, EvniAsset evni)
	{
		return (x_32 - evni.XStart / 32) + (z_32 - evni.ZStart /32) * evni.XTileCount;
	}

	public static int FindOrginFileIndex (Vector3 chunk32Pos, EvniAsset evni)
	{
		int file_index = -1;
		for (int i = 0; i < evni.FileXCount; ++i)
		{
			int zStart = (evni.ZStart >> evni.Tile) + (evni.ZTileCount * i);
			int zEnd = zStart + evni.ZTileCount;
			for (int j = 0; j <  evni.FlieZCount; ++j)
			{
				int xStart = (evni.XStart >> evni.Tile) + (evni.XTileCount * j);
				int xEnd = xStart + evni.XTileCount;
				
				if ( xStart <= chunk32Pos.x && xEnd > chunk32Pos.x
				    && zStart <= chunk32Pos.z && zEnd > chunk32Pos.z)
				{
					file_index = i * evni.FlieZCount + j;
					break;
				}
			}
		}
		return file_index;
	}

	#endregion

}
