using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public class BSIsoData 
{
	// Version control
	public const int ISO_VERSION = 0x01010001;

	// Head info
	public BSIsoHeadData m_HeadInfo;
	

	// Block
	public Dictionary<int, BSVoxel> m_Voxels;

	#region STATIC_FUNC

	// Voxel int position to an int key
	public static int IPosToKey(IntVector3 pos)
	{
		return (pos.x) | ((pos.z) << 10) | ((pos.y ) << 20);
	}
	public static int IPosToKey(int x, int y, int z)
	{
		return (x) | ((z) << 10) | ((y) << 20);
	}
	// Voxel key to an int position
	public static IntVector3 KeyToIPos(int key)
	{
		return new IntVector3 ( (key & 0x3ff) , (key >> 20), ((key >> 10) & 0x3ff));
	}


	#endregion

	public BSIsoData()
	{

	}

	public void Init(EBSVoxelType voxel_type)
	{
		Destroy();

		m_Voxels = new Dictionary<int, BSVoxel>();

		// Init header
		m_HeadInfo = new BSIsoHeadData();
		m_HeadInfo.Version = ISO_VERSION;
		m_HeadInfo.Mode = voxel_type;
		m_HeadInfo.Author = "";
		m_HeadInfo.Name = "";
		m_HeadInfo.Desc = "";
		m_HeadInfo.Remarks = "<REMARKS />";
		m_HeadInfo.xSize = 256;
		m_HeadInfo.ySize = 256;
		m_HeadInfo.zSize = 256;
		m_HeadInfo.IconTex = new byte [0];
		m_HeadInfo.costs = new Dictionary<byte, UInt32>();
		m_HeadInfo.EnsureIconTexValid();
	}

	public void Destroy()
	{
		if (m_Voxels != null)
			m_Voxels.Clear();

	}

	public BSVoxel GetVoxel (int pos)
	{
		BSVoxel retval;
		m_Voxels.TryGetValue(pos, out retval);
		return retval;
	}

	// Set voxel
	public void SetVoxel(int pos, BSVoxel voxel)
	{
		if (m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			if (voxel.type == 0)
				m_Voxels.Remove(pos);
			else
				m_Voxels[pos] = voxel;
		}
		else if (m_HeadInfo.Mode == EBSVoxelType.Voxel)
		{
			if ( voxel.volmue == 0 )
				m_Voxels.Remove(pos);
			else
				m_Voxels[pos] = voxel;
		}
		else
		{
			Debug.LogError("Unknow voxel type");
		}

	}

	public void CaclCosts ()
	{
		m_HeadInfo.costs.Clear();

		foreach (KeyValuePair<int, BSVoxel> kvp in m_Voxels)
		{
			if (m_HeadInfo.costs.ContainsKey(kvp.Value.materialType))
				m_HeadInfo.costs[kvp.Value.materialType] ++;
			else
				m_HeadInfo.costs.Add(kvp.Value.materialType, 1);
		}
	}
	

	#region EXPOR & IMPORT

	public byte[] Export()
	{
		using ( MemoryStream ms_iso = new MemoryStream () )
		{
			BinaryWriter w = new BinaryWriter (ms_iso);

			w.Write("BSISO");	// w, string
			// Header
			m_HeadInfo.EnsureIconTexValid();
			w.Write(ISO_VERSION);
			w.Write((int)(m_HeadInfo.Mode));
			w.Write(m_HeadInfo.Author);
			w.Write(m_HeadInfo.Name);	
			w.Write(m_HeadInfo.Desc);	
			w.Write(m_HeadInfo.Remarks);
			w.Write(m_HeadInfo.xSize);	
			w.Write(m_HeadInfo.ySize);	
			w.Write(m_HeadInfo.zSize);	
			w.Write(m_HeadInfo.IconTex.Length);	
			w.Write(m_HeadInfo.IconTex, 0, m_HeadInfo.IconTex.Length);
			w.Write(m_HeadInfo.costs.Count);
			foreach (KeyValuePair<byte, UInt32> kvp in m_HeadInfo.costs)
			{
				w.Write(kvp.Key);
				w.Write(kvp.Value);
			}

			// Counts
			w.Write(m_Voxels.Count);

			foreach (KeyValuePair<int, BSVoxel> kvp in m_Voxels)
			{
				w.Write(kvp.Key);
				w.Write(kvp.Value.value0);
				w.Write(kvp.Value.value1);
			}

			w.Close();
			byte [] retval = ms_iso.ToArray();
			return retval;
		}
	}

	public bool Import(byte[] buffer)
	{
		if ( buffer == null )
			return false;

		Init(EBSVoxelType.Block);

		try
		{
			using ( MemoryStream ms_iso = new MemoryStream (buffer) )
			{
				BinaryReader r = new BinaryReader (ms_iso);

				// Header
				string check_str = r.ReadString();	// r, string
				if ( check_str != "BSISO" )
				{
					/************
					 *  Older Building System
					 * *********/

					r.BaseStream.Seek(0, SeekOrigin.Begin);

					int version = r.ReadInt32();
					int count = r.ReadInt32();

					switch (version)
					{
					case 1:

						Dictionary<IntVector3, B45Block> old_blocks = new Dictionary<IntVector3, B45Block>();
						for(int i = 0; i < count; i++)
						{
							IntVector3 index = new IntVector3(r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
							old_blocks[index] = new B45Block(r.ReadByte(), r.ReadByte());
						}
						int texDataSize = r.ReadInt32();
						byte[] tex_bytes = r.ReadBytes(texDataSize);
							
						m_HeadInfo.IconTex = tex_bytes;
						
						// Calculate Bound
						IntVector3 min = new IntVector3(10000, 10000, 10000);
						IntVector3 max = new IntVector3(-10000, -10000, -10000);
						
						foreach (IntVector3 index in old_blocks.Keys)
						{
							if (min.x > index.x)
								min.x = index.x;
							else if (max.x < index.x)
								max.x = index.x;
							
							if (min.y > index.y)
								min.y = index.y;
							else if (max.y < index.y)
								max.y = index.y;
							
							if (min.z > index.z)
								min.z = index.z;
							else if (max.z < index.z)
								max.z = index.z;
						}
						
						IntVector3 size = new IntVector3(max.x - min.x + 1, max.y - min.y + 1, max.z - min.z + 1);
						
						m_HeadInfo.xSize = size.x;
						m_HeadInfo.ySize = size.y;
						m_HeadInfo.zSize = size.z;
						
						foreach (KeyValuePair<IntVector3, B45Block> kvp in old_blocks)
						{
							IntVector3 ipos = new IntVector3(kvp.Key.x +  Mathf.CeilToInt ((float)size.x / 2), kvp.Key.y , kvp.Key.z + Mathf.CeilToInt ((float)size.z / 2));
//							Debug.Log(ipos);
							int key = IPosToKey(ipos);
//							m_Voxels.Add(key, new BSVoxel( kvp.Value));
							m_Voxels[key] = new BSVoxel( kvp.Value);
						}
						
						CaclCosts();
						break;
					default:
						break;
					}
					r.Close();
					return true;
				}

				m_HeadInfo.Version = r.ReadInt32();	// r, int

				switch (m_HeadInfo.Version)
				{
				case 0x01010001:
//					int l = 0;
					m_HeadInfo.Mode    = (EBSVoxelType) r.ReadInt32();
					m_HeadInfo.Author  = r.ReadString();
					m_HeadInfo.Name	   = r.ReadString();
					m_HeadInfo.Desc	   = r.ReadString();
					m_HeadInfo.Remarks = r.ReadString();
					m_HeadInfo.xSize   = r.ReadInt32();
					m_HeadInfo.ySize   = r.ReadInt32();
					m_HeadInfo.zSize   = r.ReadInt32();
					int length = r.ReadInt32();
					m_HeadInfo.IconTex = r.ReadBytes(length);
					length = r.ReadInt32();
					for (int i= 0; i < length; i++)
					{
						m_HeadInfo.costs.Add(r.ReadByte(), r.ReadUInt32());
					}

					length = r.ReadInt32();
					for (int i= 0; i < length; i++)
					{
						BSVoxel voxel = new BSVoxel();
						int key = r.ReadInt32();
						voxel.value0 = r.ReadByte();
						voxel.value1 = r.ReadByte();

						m_Voxels[key] = voxel;

					}
					break;
				default:
					r.Close();
					return false;
				}

				r.Close();
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("Importing ISO Failed :" + e.ToString());
			return false;
		}

		return true;
	}

	// Extract Header
	private static int ExtractHeader(Stream stream, out BSIsoHeadData iso_header)
	{
		iso_header = new BSIsoHeadData ();
		int len = 0;

		iso_header.Init();
		try
		{
			BinaryReader r = new BinaryReader (stream);

			// Header
			string check_str = r.ReadString();	// r, string
			if ( check_str != "BSISO" )
			{
				/************
				*  Older Building System
				* *********/

				r.BaseStream.Seek(0, SeekOrigin.Begin);
				
				int version = r.ReadInt32();
				int count = r.ReadInt32();
				
				switch (version)
				{
				case 1:

					Dictionary<IntVector3, B45Block> old_blocks = new Dictionary<IntVector3, B45Block>();
					for(int i = 0; i < count; i++)
					{
						IntVector3 index = new IntVector3(r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
						old_blocks[index] = new B45Block(r.ReadByte(), r.ReadByte());
					}
					int texDataSize = r.ReadInt32();
					byte[] tex_bytes = r.ReadBytes(texDataSize);
					
					iso_header.IconTex = tex_bytes;
					
					// Calculate Bound
					IntVector3 min = new IntVector3(10000, 10000, 10000);
					IntVector3 max = new IntVector3(-10000, -10000, -10000);
					
					foreach (IntVector3 index in old_blocks.Keys)
					{
						if (min.x > index.x)
							min.x = index.x;
						else if (max.x < index.x)
							max.x = index.x;
						
						if (min.y > index.y)
							min.y = index.y;
						else if (max.y < index.y)
							max.y = index.y;
						
						if (min.z > index.z)
							min.z = index.z;
						else if (max.z < index.z)
							max.z = index.z;
					}
					
					IntVector3 size = new IntVector3(max.x - min.x + 1, max.y - min.y + 1, max.z - min.z + 1);
					
					iso_header.xSize = size.x;
					iso_header.ySize = size.y;
					iso_header.zSize = size.z;
					
					foreach (KeyValuePair<IntVector3, B45Block> kvp in old_blocks)
					{
						//IntVector3 ipos = new IntVector3(kvp.Key.x + size.x / 2, kvp.Key.y , kvp.Key.z + size.z / 2);
//						int key = IPosToKey(ipos);
//						m_Voxels.Add(key, new BSVoxel( kvp.Value));
						if (iso_header.costs.ContainsKey(kvp.Value.materialType))
							iso_header.costs[kvp.Value.materialType] ++;
						else
							iso_header.costs.Add(kvp.Value.materialType, 1);
					}

					break;
				default:
					break;
				}
				r.Close();
				len = (int)stream.Length;
				return len;

			}

//			int l = 0;
			iso_header.Version = r.ReadInt32();	// r, int
			iso_header.Mode    = (EBSVoxelType) r.ReadInt32();
			iso_header.Author  = r.ReadString();
			iso_header.Name	   = r.ReadString();
			iso_header.Desc	   = r.ReadString();
			iso_header.Remarks = r.ReadString();
			iso_header.xSize   = r.ReadInt32();
			iso_header.ySize   = r.ReadInt32();
			iso_header.zSize   = r.ReadInt32();

			int length = r.ReadInt32();
			iso_header.IconTex = r.ReadBytes(length);
			length = r.ReadInt32();
			for (int i= 0; i < length; i++)
			{
				iso_header.costs.Add(r.ReadByte(), r.ReadUInt32());
			}

			len = (int)(stream.Length);
			stream.Close();

		}
		catch (System.Exception)
		{
			return 0;
		}

		iso_header.EnsureIconTexValid();

		return len;
	}

	// file
	public static int ExtractHeader(string filename, out BSIsoHeadData iso_header)
	{
		iso_header = new BSIsoHeadData ();
		if ( !File.Exists(filename) )
			return 0;
		try
		{
            iso_header.Name = filename;
			using ( FileStream fs = File.Open(filename, FileMode.Open) )
			{
				return ExtractHeader(fs, out iso_header);
			}
		}
		catch (System.Exception)
		{
			return 0;
		}
	}

	// byte buffer
	public static int ExtractHeader(byte[] buffer, out BSIsoHeadData iso_header)
	{
		iso_header = new BSIsoHeadData ();
		if ( buffer == null )
			return 0;
		using ( MemoryStream ms = new MemoryStream (buffer) )
		{
			return ExtractHeader(ms, out iso_header);
		}
	}
	#endregion
}
