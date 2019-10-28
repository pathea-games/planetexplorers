using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

public static class LSubTerrExport
{
    #region LZ4_EXTERN
    [DllImport("lz4_dll")]
    public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);
    [DllImport("lz4_dll")]
    public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);
    #endregion
	public static readonly string s_orgnFilePath = PeGrassDataIO_Story.originalSubTerrainDir + "subter.dat";
	public static readonly string s_mergedFilePath = PeGrassDataIO_Story.originalSubTerrainDir + "subter.~dat";
	
	[MenuItem("PeCustomMenu/DataMerge/Merge Modified SubTerrain (zhouxun, new)")]
	public static void SaveToOriginalFile()
	{
		// Open original subterrain data file
		FileStream orgnSubTerrFile = new FileStream(s_orgnFilePath, FileMode.Open, FileAccess.Read);
		BinaryReader _in = new BinaryReader(orgnSubTerrFile);
		// Read each subterrain's data offset, len and uncmp-len.
		int [] orgnOfsData = new int [LSubTerrConstant.XZCount];
		int [] orgnLenData = new int [LSubTerrConstant.XZCount];
		int [] orgnUcmpLenData = new int [LSubTerrConstant.XZCount];
        for ( int i = 0; i < LSubTerrConstant.XZCount; ++i )
        {
            orgnOfsData[i] = _in.ReadInt32();
			if ( i > 0 )
				orgnLenData[i-1] = orgnOfsData[i] - orgnOfsData[i-1];
            orgnUcmpLenData[i] = _in.ReadInt32();
        }
		orgnLenData[LSubTerrConstant.XZCount - 1] = (int)orgnSubTerrFile.Length - orgnOfsData[LSubTerrConstant.XZCount - 1];
		
		// Preparing..
		int l = orgnOfsData.Length;
		List<byte[]> data_nodes = new List<byte[]> ();
		List<string> _tmpfile_to_del = new List<string> ();
		string doc_path = GameConfig.GetUserDataPath();
		string cache_path = doc_path + GameConfig.CreateSystemData + "/SubTerrains/cache_";
		
		// Read old data
		for ( int i = 0; i < l; ++i )
		{
			EditorUtility.DisplayProgressBar("Merging subTerrains...", "Read old data.. ("+(i+1).ToString()+" of "+l.ToString()+")", 0.5f*((float)(i)/(float)(l)));
			byte[] node_data = new byte [0];
			
			// Cache exist - modified
		    string filename = cache_path + i.ToString() + ".subter";
			if ( File.Exists(filename) )
			{
				// Read raw data
				byte[] raw_data = new byte [0];
				using ( FileStream fs = new FileStream( filename, FileMode.Open, FileAccess.Read, FileShare.Read ) )
				{
		        	BinaryReader r = new BinaryReader(fs);
					raw_data = r.ReadBytes((int)(fs.Length));
					r.Close();
					fs.Close();
					_tmpfile_to_del.Add(filename);
				}
				// Not Empty
				if ( raw_data.Length > 4 )
				{
					byte[] _buf = new byte [Mathf.CeilToInt((raw_data.Length + 8) * 1.1f)];
					int size = LZ4_compress(raw_data, _buf, raw_data.Length);
					node_data = new byte [size];
					Array.Copy(_buf, node_data, size);
					// Change the uncomp, len
					orgnUcmpLenData[i] = raw_data.Length;
					orgnLenData[i] = node_data.Length;
				}
				// Empty
				else
				{
					// Change the uncomp, len
					orgnUcmpLenData[i] = 0;
					orgnLenData[i] = 0;
				}
			}
			// No modify
			else
			{
				// Read old
		       	orgnSubTerrFile.Seek(orgnOfsData[i], SeekOrigin.Begin);
		        node_data = new byte[orgnLenData[i]];
		        orgnSubTerrFile.Read(node_data, 0, orgnLenData[i]);
			}
			data_nodes.Add(node_data);
		}
		_in.Close();
		orgnSubTerrFile.Close();
		
		// No need to merge, exit
		if ( _tmpfile_to_del.Count < 1 )
		{
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Merging subTerrains", "Nothing changed !", "OK");
			data_nodes.Clear();
			return;
		}
		
		// Assign new offset data
		for ( int i = 0; i < l-1; ++i )
		{
			orgnOfsData[i+1] = orgnOfsData[i] + data_nodes[i].Length;
		}
		
		// Creating new file (it's a temp file)
		using ( FileStream mergedSubTerrFile = new FileStream(s_mergedFilePath, FileMode.Create, FileAccess.Write) )
		{
			BinaryWriter _out = new BinaryWriter (mergedSubTerrFile);
			// Write header
			for ( int i = 0; i < l; ++i )
			{
				_out.Write(orgnOfsData[i]);
				_out.Write(orgnUcmpLenData[i]);
			}
			// Write each subterrain node
			for ( int i = 0; i < l; ++i )
			{
				EditorUtility.DisplayProgressBar("Merging subTerrains...", "Copy new data.. ("+(i+1).ToString()+" of "+l.ToString()+")", 0.5f + 0.5f*((float)(i)/(float)(l)));
				_out.Write(data_nodes[i], 0, data_nodes[i].Length);
			}
			EditorUtility.DisplayProgressBar("Merging subTerrains...", "Done!", 1);
			_out.Close();
			mergedSubTerrFile.Close();
		}
		
		// Merge complete
		EditorUtility.ClearProgressBar();
		if ( EditorUtility.DisplayDialog("Merge complete!", "Do you want to make it work ?", "Yes", "No") )
		{
			// Make it work
			File.Copy(s_mergedFilePath, s_orgnFilePath, true);
			// delete temp file
			File.Delete(s_mergedFilePath);
			if ( File.Exists(s_mergedFilePath + ".meta") )
				File.Delete(s_mergedFilePath + ".meta");
			// delete cache
			foreach ( string tmpfile in _tmpfile_to_del )
			{
				if ( File.Exists(tmpfile) )
				{
					File.Copy(tmpfile, tmpfile + ".bak", true);
					File.Delete(tmpfile);
				}
			}
			data_nodes.Clear();
		}
		else
		{
			// Don't make it work
			File.Delete(s_mergedFilePath);
			if ( File.Exists(s_mergedFilePath + ".meta") )
				File.Delete(s_mergedFilePath + ".meta");
			data_nodes.Clear();
		}
	}
}
