using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using RedGrass;
using Pathea.Maths;

public class RGDemoExport  
{
	public static string s_mergedFilePath =  "D:/grassinst_test.~dat";

	public static string s_orgnFilePath = "D:/grassinst_test.dat";

	public static string s_EvniPath = "Evni/Demo Evni";

	#region EDITOR_USE

	[MenuItem("RedGrassMenu/DataMerge/Merge Modified Grass")]
	public static void SaveToOriginalFile()
	{
		RedGrass.EvniAsset evni = Resources.Load(s_EvniPath) as RedGrass.EvniAsset; 
		if (evni == null)
			Debug.LogError("EvniAsset is missiing");

		// Open or Create original subterrain data file
		FileStream orgnGrassFile = null;
		int[]  orgnOfsData = new int[evni.XZTileCount];
		int[]  orgnLenData = new int[evni.XZTileCount];
		if (File.Exists(s_orgnFilePath))
		{
			orgnGrassFile =  new FileStream(s_orgnFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			{
				BinaryReader _in = new BinaryReader(orgnGrassFile);
				
				for (int i = 0; i < evni.XZTileCount; ++i)
				{
					orgnOfsData[i]  = _in.ReadInt32();
					orgnLenData[i]  = _in.ReadInt32(); 
				}
			}
		}
		else
		{
			orgnGrassFile =  new FileStream(s_orgnFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		}

		// Merge the cache
		string[] raw_file_path = Directory.GetFiles(RGDemoEditorSaver.s_FileDirectory);
		Dictionary<int, Dictionary<INTVECTOR3, RedGrassInstance>> raw_file_add = new Dictionary<int, Dictionary<INTVECTOR3, RedGrassInstance>>();
		List<int> version_list = new List<int>();
		List<string> tempfile_to_del = new List<string>();

		for (int i = 0; i < raw_file_path.Length; i++)
		{
			EditorUtility.DisplayProgressBar("Merging Grasses...", "Read raw data.. ("+(i+1).ToString()+" of "+raw_file_path.Length.ToString()+")", 0.2f*((float)(i)/(float)(raw_file_path.Length)));

			// the expanded-name must be "gs"
			if ( raw_file_path[i].Substring(raw_file_path[i].LastIndexOf(".") + 1) != "gs")
				continue;

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

		List<RedGrassInstance> file_add = new List<RedGrassInstance>();

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

				file_add.Add(kvp.Value);
				
			}
		}

		#region READ_ORIGINAL_FILE & REFRESH_DATA

		// Read old original file data
		Dictionary<INTVECTOR3, RedGrassInstance>[] old_datas = new Dictionary<INTVECTOR3, RedGrassInstance>[evni.XZTileCount];
		{
			string fileName = s_orgnFilePath.Substring( s_orgnFilePath.LastIndexOf("/") + 1);
			// Read now
			BinaryReader _in = new BinaryReader(orgnGrassFile);
			for (int j = 0; j < evni.XZTileCount; ++j)
			{
				if (j % 100 == 0)
					EditorUtility.DisplayProgressBar("Merging Grasses...", "Read file " + fileName +" data.." + (j+1).ToString()+" of "+ evni.XZTileCount.ToString()+")",
					                                 ((float)(j)/(float)evni.XZTileCount));

				old_datas[j] = new Dictionary<INTVECTOR3, RedGrassInstance>();

				if (orgnLenData[j] == 0)
					continue;

				_in.BaseStream.Seek(orgnOfsData[j], SeekOrigin.Begin);
				int count = orgnLenData[j];
				for (int k = 0; k < count; ++k)
				{
					RedGrassInstance rgi = new RedGrassInstance();
					rgi.ReadFromStream(_in);
					INTVECTOR3 index = Utils.WorldPosToVoxelPos(rgi.Position);
					old_datas[j][index] = rgi;
				}
			}
		}

		// Add

		int _per = 0;
		foreach (RedGrassInstance rgi in file_add)
		{
			INTVECTOR3 w_i = Utils.WorldPosToVoxelPos(rgi.Position);
			int f_startX = evni.XStart;
			int f_startZ = evni.ZStart;

			if (_per % 100 == 0)
				EditorUtility.DisplayProgressBar("Merging Grasses...", "Calculate the Grasses...",
				                                 ((float)(_per)/(float)file_add.Count));
            
//            for (int i = 0; i < evni.XZTileCount; ++i)
//			{
//				if (old_datas[i].ContainsKey(w_i))
//				{
//					old_datas[i][w_i] = rgi;
//					break;
//				}
//				else
//				{
//
//					int x = i % evni.XTileCount;
//					int z = i / evni.ZTileCount;
//					int startX = f_startX + x * evni.Tile;
//					int startZ = f_startZ + z * evni.Tile;
//					if (w_i.x >= startX && w_i.x < startX + evni.Tile
//					    && w_i.z >= startZ && w_i.z < startZ + evni.Tile)
//					{
//						old_datas[i].Add(w_i, rgi);
//						break;
//					}
//
//				}
//			}

			int chunk_x = ((int)w_i.x  - f_startX) / evni.Tile;
			int chunk_z = ((int)w_i.z  - f_startZ) / evni.Tile;
			int key = chunk_x + chunk_z * evni.XTileCount;


			old_datas[key][w_i] = rgi;
//			RGScene
			_per ++;
		}

		file_add.Clear();

		#endregion



		// Save to a series of Temp file
		string file_name = s_mergedFilePath;

		
		using (FileStream fs = new FileStream(file_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			BinaryWriter _w = new BinaryWriter(fs);
			
			int[] offsets = new int[evni.XZTileCount];
			int[] lens  = new int[evni.XZTileCount];
			_w.Seek(evni.XZTileCount * 4 * 2, SeekOrigin.Begin);
			// Write Grass data
			for (int i = 0; i < evni.XZTileCount; i++)
			{
				if (i % 100 == 0)
					EditorUtility.DisplayProgressBar("Merging Grasses...", "Merge file data.. ("+(i+1).ToString()+" of "+ evni.XZTileCount.ToString()+")",
					                                 ((float)(i)/(float) evni.XZTileCount));
				offsets[i] = (int)fs.Position;
				
				if (old_datas[i].Count == 0)
				{
					offsets[i] = 0;
					continue;
				}
				
//				_w.Write(old_datas[i].Count);
				lens[i] = old_datas[i].Count;
				foreach(RedGrassInstance rgi in old_datas[i].Values)
					rgi.WriteToStream(_w);
			}
			// Write Offset
			_w.Seek(0, SeekOrigin.Begin);
			for (int i = 0; i < evni.XZTileCount; i++)
			{
				_w.Write(offsets[i]);
				_w.Write(lens[i]);
			}

			
			fs.Close();
		}

		// Merge complete
		EditorUtility.ClearProgressBar();
		if ( EditorUtility.DisplayDialog("Merge complete!", "Do you want to make it work ?", "Yes", "No") )
		{
			// Make it work
			
			File.Copy(s_mergedFilePath, s_orgnFilePath, true);
			
			// delete temp file
			File.Delete(s_mergedFilePath);
			if (File.Exists(s_mergedFilePath + ".meta"))
				File.Delete(s_mergedFilePath + ".meta");
			
			foreach (string tmpfile in tempfile_to_del)
			{
				if ( File.Exists(tmpfile) )
				{
					File.Copy(tmpfile, tmpfile + ".bak", true);
					File.Delete(tmpfile);
				}
			}
		}
		else
		{
			// Don't make it work
			File.Delete(s_mergedFilePath);
			if (File.Exists(s_mergedFilePath + ".meta"))
				File.Delete(s_mergedFilePath + ".meta");
		}
	}
	#endregion
}
