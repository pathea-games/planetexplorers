using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

// Iso options
public struct VAOption
{
    public VAOption(bool editor)
	{
		ForEditor = editor;
	}
	public bool ForEditor;
}

public class VArtifactData
{
	// Version control
	public const int ISO_VERSION = 0x02020001;
	
	// Head info
	public VCIsoHeadData m_HeadInfo;
	
	// Material info
	public const int MAT_ROW_CNT = 8;
	public const int MAT_COL_CNT = 8;
	public const int MAT_ARR_CNT = MAT_ROW_CNT * MAT_COL_CNT;
	public const int DECAL_ARR_CNT = 4;
	public VAMaterial[] m_Materials = null;
	public VCDecalAsset[] m_DecalAssets = null;
	
	// Voxel info
	public Dictionary<int, VCVoxel> m_Voxels = null;
	
	// Components
	public List<VCComponentData> m_Components = null;
	
	// Colors
	public Dictionary<int, Color32> m_Colors = null;
	public static Color32 BLANK_COLOR = new Color32 (255,255,255,0);
	
	// Options ( will not save to stream )
	private VAOption m_Options;
	
	//
	// Static functions
	//
	
	// Voxel int position to an int key
	public static int IPosToKey(IntVector3 pos)
	{
		return (pos.x) | (pos.z << 10) | (pos.y << 20);
	}
	public static int IPosToKey(int x, int y, int z)
	{
		return (x) | (z << 10) | (y << 20);
	}
	// Voxel key to an int position
	public static IntVector3 KeyToIPos(int key)
	{
		return new IntVector3 ( key & 0x3ff, key >> 20, (key >> 10) & 0x3ff );
	}
	public static void GetIPosFromKey(int key, IntVector3 output)
	{
		output.x = key & 0x3ff;
		output.y = key >> 20;
		output.z = (key >> 10) & 0x3ff;
	}
	// World position to an int key for coloring
	public static IntVector3 IPosToColorPos(Vector3 iso_pos)
	{
		// iso_pos = worldpos / voxel_size
		return new IntVector3( (int)((iso_pos.x + 0.75f) * 2.0f),
		                       (int)((iso_pos.y + 0.75f) * 2.0f),
		                       (int)((iso_pos.z + 0.75f) * 2.0f) );
	}
	public static int ColorPosToColorKey(IntVector3 color_pos)
	{
		return (color_pos.x) | (color_pos.z << 10) | (color_pos.y << 20);
	}
	public static int IPosToColorKey(Vector3 iso_pos)
	{
		// iso_pos = worldpos / voxel_size
		return ((int)((iso_pos.x + 0.75f) * 2.0f)) | (((int)((iso_pos.z + 0.75f) * 2.0f)) << 10) | (((int)((iso_pos.y + 0.75f) * 2.0f)) << 20);
	}
	
	//
	// Member functions
	//
	
	// Reset iso, destroy first, collections are ready.
	public void Reset(VAOption options)
	{
		// Destroy old data
		Destroy();
		
		// Set default header
		m_HeadInfo = new VCIsoHeadData ();
		m_HeadInfo.Author = "";
		m_HeadInfo.Name = "";
		m_HeadInfo.Desc = "";
		m_HeadInfo.Remarks = "<REMARKS />";
		m_HeadInfo.IconTex = new byte [0];
		
		// Allocate material, voxel, component, color.
		m_Materials = new VAMaterial [MAT_ARR_CNT];
		m_DecalAssets = new VCDecalAsset[DECAL_ARR_CNT];
		m_Voxels = new Dictionary<int, VCVoxel> ();
		m_Components = new List<VCComponentData> ();
		m_Colors = new Dictionary<int, Color32> ();
		
		// Set options
		m_Options = options;
	}
	// Init iso, destroy first, collections, header are ready.
	public void Init( int version, VCESceneSetting setting, VAOption options )
	{
		// Destroy old data
		Destroy();
		
		// Init header data
		m_HeadInfo = new VCIsoHeadData ();
		m_HeadInfo.Version = version;
		m_HeadInfo.Category = setting.m_Category;
		m_HeadInfo.Author = "";
		m_HeadInfo.Name = "";
		m_HeadInfo.Desc = "";
		m_HeadInfo.Remarks = "<REMARKS />";
		m_HeadInfo.xSize = setting.m_EditorSize.x;
		m_HeadInfo.ySize = setting.m_EditorSize.y;
		m_HeadInfo.zSize = setting.m_EditorSize.z;
		m_HeadInfo.IconTex = new byte [0];
		m_HeadInfo.EnsureIconTexValid();
		
		// Allocate material, voxel, component, color.
		m_Materials = new VAMaterial [MAT_ARR_CNT];
		m_DecalAssets = new VCDecalAsset[DECAL_ARR_CNT];
		m_Voxels = new Dictionary<int, VCVoxel> ();
		m_Components = new List<VCComponentData> ();
		m_Colors = new Dictionary<int, Color32> ();
		
		// Option
		m_Options = options;
	}
	// Destroy iso, free memory.
	public void Destroy()
	{
		// Destroy header
		m_HeadInfo = new VCIsoHeadData ();
		
		// Material, voxel, component, color.
		DestroyMaterials();
		m_Materials = null;
		DestroyDecalAssets();
		m_DecalAssets = null;
		if ( m_Voxels != null )
		{
			m_Voxels.Clear();
			m_Voxels = null;
		}
		if ( m_Components != null )
		{
			if ( m_Options.ForEditor )
			{
				foreach ( VCComponentData cdata in m_Components )
				{
					cdata.DestroyEntity();
				}
			}
			m_Components.Clear();
			m_Components = null;
		}
		if ( m_Colors != null )
		{
			m_Colors.Clear();
			m_Colors = null;
		}
	}
	// Free VAMaterial objects
	public void DestroyMaterials()
	{
		if ( m_Materials != null )
		{
			if ( !m_Options.ForEditor )
			{
				foreach ( VAMaterial vcm in m_Materials )
				{
					if ( vcm != null )
					{
						// Free material's Texture2D interfaces and byte buffers. (F12)
						vcm.Destroy();
					}
				}
			}
			// Set null m_Materials array elements
			// The m_Materials array will not destroy now.
			for ( int i = 0; i < MAT_ARR_CNT; ++i )
				m_Materials[i] = null;
		}
	}
	// Free VCDecalAsset objects
	public void DestroyDecalAssets()
	{
		if ( m_DecalAssets != null )
		{
			if ( !m_Options.ForEditor )
			{
				foreach ( VCDecalAsset vcd in m_DecalAssets )
				{
					if ( vcd != null )
					{
						// Free decal's Texture2D interfaces and byte buffers. (F12)
						vcd.Destroy();
					}
				}
			}
			// Set null m_DecalAssets array elements
			// The m_DecalAssets array will not destroy now.
			for ( int i = 0; i < DECAL_ARR_CNT; ++i )
				m_DecalAssets[i] = null;
		}
	}
	// Clear iso, include materials, voxels, components and colors, iso header will not change.
	public void Clear()
	{
		// Header will not change !
		
		// Clear materials, decals, voxels, components, color collections.
		DestroyMaterials();
		DestroyDecalAssets();
		m_Voxels.Clear();
		if ( m_Options.ForEditor )
		{
			foreach ( VCComponentData cdata in m_Components )
			{
				cdata.DestroyEntity();
			}
		}
		m_Components.Clear();
		m_Colors.Clear();
	}

	#region MATERIALS_FUNCS
	// Query a voxel type for a specified VAMaterial
	public int QueryVoxelType(VAMaterial vcmat)
	{
		if ( vcmat == null )
			return -1;
		// Find the same material
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			if ( m_Materials[i] != null && vcmat.m_Guid == m_Materials[i].m_Guid )
				return i;
		}
		// Find a null position
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			if ( m_Materials[i] == null )
				return i;
		}
		// Full, find not used
		int[] used_count = new int [MAT_ARR_CNT];
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			used_count[i] = 0;
		}
		foreach ( KeyValuePair<int, VCVoxel> kvp in m_Voxels )
		{
			if (kvp.Value.Type != 0x7f)
			{
				int type = kvp.Value.Type & 0x7f;
				used_count[type]++;
			}
		}
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			if ( used_count[i] == 0 )
				return i;
		}
		// Unfortunately, no material avaliable
		return -1;
	}
	public int QueryMaterialIndex(VAMaterial vcmat)
	{
		if ( vcmat == null )
			return -1;
		// Find the same material
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			if ( m_Materials[i] != null && vcmat.m_Guid == m_Materials[i].m_Guid )
				return i;
		}
		return -1;
	}
	public VAMaterial QueryMaterial(ulong guid)
	{
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			if ( m_Materials[i] != null && guid == m_Materials[i].m_Guid )
				return m_Materials[i];
		}
		return null;
	}
	public int MaterialUsedCount()
	{
		int cnt = 0;
		for ( int i = 0; i < MAT_ARR_CNT; ++i )
		{
			if ( m_Materials[i] != null )
				cnt++;
		}
		return cnt;
	}
	public ulong MaterialGUID(int index)
	{
		if ( index < 0 || index >= MAT_ARR_CNT )
			return 0;
		else if ( m_Materials[index] == null )
			return 0;
		else
			return m_Materials[index].m_Guid;
	}
	#endregion

	#region DECALS_FUNCS
	// Query a decal index for a specified VCDecalAsset
	public int QueryNewDecalIndex(VCDecalAsset vcdcl)
	{
		if ( vcdcl == null )
			return -1;
		// Find the same decal
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			if ( m_DecalAssets[i] != null && vcdcl.m_Guid == m_DecalAssets[i].m_Guid )
				return i;
		}
		// Find a null position
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			if ( m_DecalAssets[i] == null )
				return i;
		}
		// Full, find not used
		int[] used_count = new int [DECAL_ARR_CNT];
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			used_count[i] = 0;
		}
		foreach ( VCComponentData cdata in m_Components )
		{
			if ( cdata.m_Type == EVCComponent.cpDecal )
			{
				VCDecalData ddata = cdata as VCDecalData;
				if ( ddata != null )
				{
					if ( ddata.m_AssetIndex >= 0 && ddata.m_AssetIndex < DECAL_ARR_CNT )
						used_count[ddata.m_AssetIndex]++;
				}
			}
		}
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			if ( used_count[i] == 0 )
				return i;
		}
		// Unfortunately, no decal avaliable
		return -1;
	}
	public int QueryExistDecalIndex(VCDecalAsset vcdcl)
	{
		if ( vcdcl == null )
			return -1;
		// Find the same decal
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			if ( m_DecalAssets[i] != null && vcdcl.m_Guid == m_DecalAssets[i].m_Guid )
				return i;
		}
		return -1;
	}
	public VCDecalAsset QueryExistDecal(ulong guid)
	{
		if ( guid == 0 )
			return null;
		// Find the same decal
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			if ( m_DecalAssets[i] != null && guid == m_DecalAssets[i].m_Guid )
				return m_DecalAssets[i];
		}
		return null;
	}
	public int DecalUsedCount()
	{
		int cnt = 0;
		for ( int i = 0; i < DECAL_ARR_CNT; ++i )
		{
			if ( m_DecalAssets[i] != null )
				cnt++;
		}
		return cnt;
	}
	public ulong DecalGUID(int index)
	{
		if ( index < 0 || index >= DECAL_ARR_CNT )
			return 0;
		else if ( m_DecalAssets[index] == null )
			return 0;
		else
			return m_DecalAssets[index].m_Guid;
	}
	#endregion

	public bool IsColorPosIn ( int x, int y, int z )
	{
		if ( x < 1 ) return false;
		if ( y < 1 ) return false;
		if ( z < 1 ) return false;
		if ( x > m_HeadInfo.xSize * 2 + 1 ) return false;
		if ( y > m_HeadInfo.ySize * 2 + 1 ) return false;
		if ( z > m_HeadInfo.zSize * 2 + 1 ) return false;
		return true;
	}
	public bool IsColorPosIn ( IntVector3 pos )
	{
		if ( pos.x < 1 ) return false;
		if ( pos.y < 1 ) return false;
		if ( pos.z < 1 ) return false;
		if ( pos.x > m_HeadInfo.xSize * 2 + 1 ) return false;
		if ( pos.y > m_HeadInfo.ySize * 2 + 1 ) return false;
		if ( pos.z > m_HeadInfo.zSize * 2 + 1 ) return false;
		return true;
	}
	public bool IsPointIn ( int x, int y, int z )
	{
		if ( x < 0 ) return false;
		if ( y < 0 ) return false;
		if ( z < 0 ) return false;
		if ( x >= m_HeadInfo.xSize ) return false;
		if ( y >= m_HeadInfo.ySize ) return false;
		if ( z >= m_HeadInfo.zSize ) return false;
		return true;
	}
	public bool IsPointIn ( IntVector3 pos )
	{
		if ( pos == null )
			return false;
		if ( pos.x < 0 ) return false;
		if ( pos.y < 0 ) return false;
		if ( pos.z < 0 ) return false;
		if ( pos.x >= m_HeadInfo.xSize ) return false;
		if ( pos.y >= m_HeadInfo.ySize ) return false;
		if ( pos.z >= m_HeadInfo.zSize ) return false;
		return true;
	}
	public bool IsPointIn ( Vector3 pos )
	{
		if ( pos.x < 0 ) return false;
		if ( pos.y < 0 ) return false;
		if ( pos.z < 0 ) return false;
		if ( pos.x >= m_HeadInfo.xSize ) return false;
		if ( pos.y >= m_HeadInfo.ySize ) return false;
		if ( pos.z >= m_HeadInfo.zSize ) return false;
		return true;
	}
	public bool IsComponentIn ( Vector3 pos )
	{
		VCESceneSetting setting = m_HeadInfo.FindSceneSetting();
		if ( setting == null )
			return false;
		float voxelsize = setting.m_VoxelSize;
		if ( pos.x < 0 ) return false;
		if ( pos.y < 0 ) return false;
		if ( pos.z < 0 ) return false;
		if ( pos.x > m_HeadInfo.xSize * voxelsize ) return false;
		if ( pos.y > m_HeadInfo.ySize * voxelsize ) return false;
		if ( pos.z > m_HeadInfo.zSize * voxelsize ) return false;
		return true;
	}
	public Vector3 ClampPointWorldCoord ( Vector3 pos )
	{
		VCESceneSetting setting = m_HeadInfo.FindSceneSetting();
		if ( setting == null )
			return Vector3.zero;
		float voxelsize = setting.m_VoxelSize;
		float sx = m_HeadInfo.xSize * voxelsize;
		float sy = m_HeadInfo.ySize * voxelsize;
		float sz = m_HeadInfo.zSize * voxelsize;
		if ( pos.x < 0 ) pos.x = 0;
		else if ( pos.x > sx ) pos.x = sx;
		if ( pos.y < 0 ) pos.y = 0;
		else if ( pos.y > sy ) pos.y = sy;
		if ( pos.z < 0 ) pos.z = 0;
		else if ( pos.z > sz ) pos.z = sz;
		return pos;
	}
	public Vector3 ClampPointF ( Vector3 pos )
	{
		if ( pos.x < 0 ) pos.x = 0;
		else if ( pos.x >= m_HeadInfo.xSize ) pos.x = m_HeadInfo.xSize - 0.001F;
		if ( pos.y < 0 ) pos.y = 0;
		else if ( pos.y >= m_HeadInfo.ySize ) pos.y = m_HeadInfo.ySize - 0.001F;
		if ( pos.z < 0 ) pos.z = 0;
		else if ( pos.z >= m_HeadInfo.zSize ) pos.z = m_HeadInfo.zSize - 0.001F;
		return pos;
	}
	public IntVector3 ClampPointI ( IntVector3 pos )
	{
		if ( pos.x < 0 ) pos.x = 0;
		else if ( pos.x >= m_HeadInfo.xSize ) pos.x = m_HeadInfo.xSize - 1;
		if ( pos.y < 0 ) pos.y = 0;
		else if ( pos.y >= m_HeadInfo.ySize ) pos.y = m_HeadInfo.ySize - 1;
		if ( pos.z < 0 ) pos.z = 0;
		else if ( pos.z >= m_HeadInfo.zSize ) pos.z = m_HeadInfo.zSize - 1;
		return pos;
	}
	public bool CanSee ( Vector3 iso_pos )
	{
		int x = Mathf.FloorToInt(iso_pos.x);
		int y = Mathf.FloorToInt(iso_pos.y);
		int z = Mathf.FloorToInt(iso_pos.z);
		if ( x <= 0 || y <= 0 || z <= 0 ||
		    x >= m_HeadInfo.xSize - 1 || y >= m_HeadInfo.ySize - 1 || z >= m_HeadInfo.zSize - 1 )
			return true;
		if ( GetVoxel(VArtifactData.IPosToKey(x+1,y,z)).Volume < 128 )
			return true;
		if ( GetVoxel(VArtifactData.IPosToKey(x-1,y,z)).Volume < 128 )
			return true;
		if ( GetVoxel(VArtifactData.IPosToKey(x,y+1,z)).Volume < 128 )
			return true;
		if ( GetVoxel(VArtifactData.IPosToKey(x,y-1,z)).Volume < 128 )
			return true;
		if ( GetVoxel(VArtifactData.IPosToKey(x,y,z+1)).Volume < 128 )
			return true;
		if ( GetVoxel(VArtifactData.IPosToKey(x,y,z-1)).Volume < 128 )
			return true;
		return false;
	}

	// Find Component
	public int GetComponentIndex(VCComponentData cdata)
	{
		if ( cdata != null )
		{
			for ( int i = 0; i < m_Components.Count; ++i )
			{
				if ( m_Components[i] == cdata )
					return i;
			}
		}
		return -1;
	}
	public List<VCComponentData> FindComponentsAtPos(Vector3 wpos, int id = 0)
	{
		List<VCComponentData> list = new List<VCComponentData> ();
		VCESceneSetting setting = m_HeadInfo.FindSceneSetting();
		if ( setting == null )
			return list;
		foreach ( VCComponentData cdata in m_Components )
		{
			if ( Vector3.Distance(cdata.m_Position, wpos) < setting.m_VoxelSize * 0.01f )
			{
				if ( id == 0 || cdata.m_ComponentId == id )
					list.Add(cdata);
			}
		}
		return list;
	}

	// Get voxel
	public VCVoxel GetVoxel(int pos)
	{
		VCVoxel retval;
		m_Voxels.TryGetValue(pos, out retval);
		return retval;
	}
	// Set voxel
	public void SetVoxel(int pos, VCVoxel voxel)
	{
		if ( voxel.Volume == 0 )
		{
			if ( m_Voxels.ContainsKey(pos) )
				m_Voxels.Remove(pos);
		}
		else
		{
			if ( m_Voxels.ContainsKey(pos) )
				m_Voxels[pos] = voxel;
			else
				m_Voxels.Add(pos, voxel);
		}
	}
	// Get color
	public Color32 GetColor(int pos)
	{
		Color32 retval;
		if ( m_Colors.TryGetValue(pos, out retval) )
			return retval;
		return BLANK_COLOR;
	}
	// Set color
	public void SetColor(int pos, Color32 color)
	{
		if ( color.r == BLANK_COLOR.r && 
			 color.g == BLANK_COLOR.g && 
			 color.b == BLANK_COLOR.b &&
			 color.a == BLANK_COLOR.a )
		{
			if ( m_Colors.ContainsKey(pos) )
				m_Colors.Remove(pos);
		}
		else
		{
			if ( m_Colors.ContainsKey(pos) )
				m_Colors[pos] = color;
			else
				m_Colors.Add(pos, color);
		}
	}

	public bool IsGarbageVoxel (int pos)
	{
		byte v = GetVoxel(pos).Volume;
		if ( v < VCEMath.MC_ISO_VALUE )
		{
			int pos0 = pos + 1;
			int pos1 = pos - 1;
			int pos2 = pos + (1 << 10);
			int pos3 = pos - (1 << 10);
			int pos4 = pos + (1 << 20);
			int pos5 = pos - (1 << 20);
			if ( GetVoxel(pos0).Volume < VCEMath.MC_ISO_VALUE 
			    && GetVoxel(pos1).Volume < VCEMath.MC_ISO_VALUE 
			    && GetVoxel(pos2).Volume < VCEMath.MC_ISO_VALUE 
			    && GetVoxel(pos3).Volume < VCEMath.MC_ISO_VALUE 
			    && GetVoxel(pos4).Volume < VCEMath.MC_ISO_VALUE 
			    && GetVoxel(pos5).Volume < VCEMath.MC_ISO_VALUE )
			{
				return true;
			}
		}
		return false;
	}

	public void NormalizeAllVoxels()
	{
		List<int> del_list = new List<int> ();
		List<int> max_list = new List<int> ();
		foreach ( KeyValuePair<int, VCVoxel> kvp in m_Voxels )
		{
			int pos0 = kvp.Key + 1;
			int pos1 = kvp.Key - 1;
			int pos2 = kvp.Key + (1 << 10);
			int pos3 = kvp.Key - (1 << 10);
			int pos4 = kvp.Key + (1 << 20);
			int pos5 = kvp.Key - (1 << 20);
			if ( kvp.Value.Volume >= VCEMath.MC_ISO_VALUE )
			{
				if ( GetVoxel(pos0).Volume >= VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos1).Volume >= VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos2).Volume >= VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos3).Volume >= VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos4).Volume >= VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos5).Volume >= VCEMath.MC_ISO_VALUE )
				{
					max_list.Add(kvp.Key);
				}
			}
			else
			{
				if ( GetVoxel(pos0).Volume < VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos1).Volume < VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos2).Volume < VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos3).Volume < VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos4).Volume < VCEMath.MC_ISO_VALUE 
				  && GetVoxel(pos5).Volume < VCEMath.MC_ISO_VALUE )
				{
					del_list.Add(kvp.Key);
				}
			}
		}
		foreach ( int key in del_list )
		{
			m_Voxels.Remove(key);
		}
		foreach ( int key in max_list )
		{
			VCVoxel v = m_Voxels[key];
			v.Volume = 255;
			m_Voxels[key] = v;
		}
		del_list.Clear();
		max_list.Clear();
	}
	
	public byte[] Export()
	{
		using ( MemoryStream ms_iso = new MemoryStream () )
		{
			BinaryWriter w = new BinaryWriter (ms_iso);
			
			w.Write("VCISO");	// w, string
			
			// Header
			m_HeadInfo.EnsureIconTexValid();
			w.Write(ISO_VERSION);	// w, int
			w.Write((int)(m_HeadInfo.Category));	// w, int
			string author = "";
			for ( int c = 0; c < m_HeadInfo.Author.Length; ++c )
				author += (char)(m_HeadInfo.Author[c] ^ (char)(0xAC));
			w.Write(author);	// w, string (v2.2)
			w.Write(m_HeadInfo.Name);	// w, string
			w.Write(m_HeadInfo.Desc);	// w, string
			string remarks = "";
			for ( int c = 0; c < m_HeadInfo.Remarks.Length; ++c )
				remarks += (char)(m_HeadInfo.Remarks[c] ^ (char)(0xAC));
			w.Write(remarks);	// w, string (v2.2)
			w.Write(m_HeadInfo.xSize);	// w, int
			w.Write(m_HeadInfo.ySize);	// w, int
			w.Write(m_HeadInfo.zSize);	// w, int
			w.Write(m_HeadInfo.IconTex.Length);	// w, int
			w.Write(m_HeadInfo.IconTex, 0, m_HeadInfo.IconTex.Length);	// w, byte[]
			
			// Counts
			w.Write(MAT_ARR_CNT);	// w, int
			w.Write(DECAL_ARR_CNT);	// w, int (v2.1)
			w.Write(m_Components.Count);	// w, int
			w.Write(m_Voxels.Count);	// w, int
			w.Write(m_Colors.Count);	// w, int
			
			// Materials
			for ( int i = 0; i < MAT_ARR_CNT; ++i )
			{
				if ( m_Materials[i] != null )
				{
					byte[] mat_buffer = m_Materials[i].Export();
					w.Write(m_Materials[i].m_Guid);	// w, ulong
					w.Write(mat_buffer.Length);	// w, int
					w.Write(mat_buffer, 0, mat_buffer.Length);	// w, byte[]
				}
				else
				{
					ulong zerolong = (ulong)(0);
					w.Write(zerolong);	// w, ulong
				}
			}

			// Decals (v2.1)
			for ( int i = 0; i < DECAL_ARR_CNT; ++i )
			{
				if ( m_DecalAssets[i] != null )
				{
					byte[] dcl_buffer = m_DecalAssets[i].Export();
					w.Write(m_DecalAssets[i].m_Guid);	// w, ulong
					w.Write(dcl_buffer.Length);	// w, int
					w.Write(dcl_buffer, 0, dcl_buffer.Length);	// w, byte[]
				}
				else
				{
					ulong zerolong = (ulong)(0);
					w.Write(zerolong);	// w, ulong
				}
			}
			
			// Fill data will be Compressed ( Components, Voxels, Colors )
			// 
			using ( MemoryStream ms_zip = new MemoryStream () )
			{
				BinaryWriter w_zip = new BinaryWriter (ms_zip);
				
				// Components
				foreach ( VCComponentData cdata in m_Components )
				{
					if ( cdata != null )
					{
						byte[] com_buffer = cdata.Export();
						w_zip.Write(com_buffer.Length);	// zip, int
						w_zip.Write(com_buffer, 0, com_buffer.Length);	// zip, byte[]
						w_zip.Write((int)(cdata.m_Type));	// zip, int
					}
					else
					{
						int zero = 0;
						w.Write(zero);	// w, int
					}
				}

				w_zip.Write(m_HeadInfo.HeadSignature);
				
				// Voxels
				foreach ( KeyValuePair<int, VCVoxel> kvp in m_Voxels )
				{
					w_zip.Write(kvp.Key);	// zip, int
					w_zip.Write((ushort)(kvp.Value));	// zip, ushort
				}
				
				// Colors
				foreach ( KeyValuePair<int, Color32> kvp in m_Colors )
				{
					w_zip.Write(kvp.Key);	// zip, int
					w_zip.Write(kvp.Value.r);	// zip, byte
					w_zip.Write(kvp.Value.g);	// zip, byte
					w_zip.Write(kvp.Value.b);	// zip, byte
					w_zip.Write(kvp.Value.a);	// zip, byte
				}
				
				// Compress data
				//
				using ( MemoryStream ms_ziped = new MemoryStream () )
				{
					ms_zip.Seek((long)(0), SeekOrigin.Begin);
					IonicZlib.Compress(ms_zip, ms_ziped);
					w.Write((int)(ms_ziped.Length));	// w, int
					w.Write(ms_ziped.GetBuffer(), 0, (int)(ms_ziped.Length));	// w, byte[]
				}
				w_zip.Close();
			}
			w.Close();
			byte [] retval = ms_iso.ToArray();
			return retval;
		}
	}
		
	public bool Import(byte[] buffer, VAOption options)
	{
		if ( buffer == null )
			return false;
		Reset(options);
		int min_iso_size = 48;
		if ( buffer.Length < min_iso_size )
			return false;
		
		using ( MemoryStream ms_iso = new MemoryStream (buffer) )
		{
			BinaryReader r = new BinaryReader (ms_iso);
			
			// Header
			string check_str = r.ReadString();	// r, string
			if ( check_str != "VCISO" )
			{
				r.Close();
				return false;
			}
			int l = 0;
			m_HeadInfo.Version = r.ReadInt32();	// r, int
			m_HeadInfo.Category = (EVCCategory)(r.ReadInt32());	// r, int
			string author = "";
			if ( m_HeadInfo.Version >= 0x02020000 )
				author = r.ReadString();
			m_HeadInfo.Name = r.ReadString();	// r, string
			m_HeadInfo.Desc = r.ReadString();	// r, string
			string remarks = "";
			if ( m_HeadInfo.Version >= 0x02020000 )
				remarks = r.ReadString();
			m_HeadInfo.xSize = r.ReadInt32();	// r, int
			m_HeadInfo.ySize = r.ReadInt32();	// r, int
			m_HeadInfo.zSize = r.ReadInt32();	// r, int
			l = r.ReadInt32();	// r, int
			m_HeadInfo.IconTex = r.ReadBytes(l);	// r, byte[]
			//m_HeadInfo.EnsureIconTexValid();

			m_HeadInfo.Author = "";
			m_HeadInfo.Remarks = "";
			if ( m_HeadInfo.Version >= 0x02020000 )
			{
				for ( int c = 0; c < author.Length; ++c )
					m_HeadInfo.Author += (char)(author[c] ^ (char)(0xAC));
				for ( int c = 0; c < remarks.Length; ++c )
					m_HeadInfo.Remarks += (char)(remarks[c] ^ (char)(0xAC));
			}

			switch ( m_HeadInfo.Version )
			{
			case 0x02000000:
			case 0x02010000:
			case 0x02020000:
			case 0x02020001:
			{
				// Counts
				int mat_cnt = r.ReadInt32();	// r, int
				int dcl_cnt = 0;
				if ( m_HeadInfo.Version >= 0x02010000 )
					dcl_cnt = r.ReadInt32();	// r, int
				int com_cnt = r.ReadInt32();	// r, int
				int vxl_cnt = r.ReadInt32();	// r, int
				int clr_cnt = r.ReadInt32();	// r, int
				
				// Materials
				for ( int i = 0; i < mat_cnt; ++i )
				{
					ulong guid = r.ReadUInt64();	// r, ulong;
					if ( guid != 0 )
					{
						l = r.ReadInt32();	// r, int
						byte[] mat_buffer = r.ReadBytes(l);	// r, byte[]
						
						// Option
						// If for editor, find mats in VCEAssetMgr for the iso, or create mats in VCEAssetMgr.
						if ( m_Options.ForEditor )
						{
                            //if ( VCEAssetMgr.s_Materials.ContainsKey(guid) )
                            //{
                            //    m_Materials[i] = VCEAssetMgr.s_Materials[guid];
                            //}
                            //else
                            //{
                            //    VAMaterial vcmat = new VAMaterial ();
                            //    vcmat.Import(mat_buffer);
                            //    VCEAssetMgr.s_TempMaterials.Add(guid, vcmat);
                            //    m_Materials[i] = vcmat;
                            //}
						}
						// If not for editor, these materials are belong to the iso.
						else
						{
							m_Materials[i] = new VAMaterial ();
							m_Materials[i].Import(mat_buffer);
						}
					}
				}
				// Decals
				for ( int i = 0; i < dcl_cnt; ++i )
				{
					ulong guid = r.ReadUInt64();	// r, ulong;
					if ( guid != 0 )
					{
						l = r.ReadInt32();	// r, int
						byte[] dcl_buffer = r.ReadBytes(l);	// r, byte[]
						
						// Option
						// If for editor, find decals in VCEAssetMgr for the iso, or create decals in VCEAssetMgr.
						if ( m_Options.ForEditor )
						{
							if ( VCEAssetMgr.s_Decals.ContainsKey(guid) )
							{
								m_DecalAssets[i] = VCEAssetMgr.s_Decals[guid];
							}
							else
							{
								VCDecalAsset vcdcl = new VCDecalAsset ();
								vcdcl.Import(dcl_buffer);
								VCEAssetMgr.s_TempDecals.Add(guid, vcdcl);
								m_DecalAssets[i] = vcdcl;
							}
						}
						// If not for editor, these decals are belong to the iso.
						else
						{
							m_DecalAssets[i] = new VCDecalAsset ();
							m_DecalAssets[i].Import(dcl_buffer);
						}
					}
				}

				// Read compressed data
				//
				using ( MemoryStream ms_zip = new MemoryStream () )
				{
					l = r.ReadInt32();	// r, int
					ms_zip.Write(r.ReadBytes(l), 0, l);	// r, byte[]	zip, byte[]
					
					// Decompress data
					//
					using ( MemoryStream ms_unzip = new MemoryStream () )
					{
						ms_zip.Seek((long)(0), SeekOrigin.Begin);
						IonicZlib.Decompress(ms_zip, ms_unzip);
						ms_unzip.Seek((long)(0), SeekOrigin.Begin);
						BinaryReader r_unzip = new BinaryReader (ms_unzip);
						
						// Components
						for ( int i = 0; i < com_cnt; ++i )
						{
							l = r_unzip.ReadInt32();	// unzip, int
							if ( l > 0 )
							{
								byte[] com_buffer = r_unzip.ReadBytes(l);	// unzip, byte[]
								EVCComponent com_type = (EVCComponent)(r_unzip.ReadInt32());	// unzip, int
								VCComponentData cdata = VCComponentData.Create(com_type, com_buffer);
								if ( cdata != null )
								{
									//cdata.m_CurrIso = this;
									m_Components.Add(cdata);
								}
							}
						}

						if ( m_HeadInfo.Version >= 0x02020001 )
						{
							ulong sig = r_unzip.ReadUInt64();
							if ( m_HeadInfo.HeadSignature != sig )
							{
								Debug.LogError("Check sig failed");
								return false;
							}
						}
						
						// Voxels
						for ( int i = 0; i < vxl_cnt; ++i )
						{
							int key = r_unzip.ReadInt32();	// unzip, int
							ushort val = r_unzip.ReadUInt16();	// unzip, ushort
							m_Voxels[key] = (VCVoxel)(val);
						}
						
						// Colors
						for ( int i = 0; i < clr_cnt; ++i )
						{
							int key = r_unzip.ReadInt32();	// unzip, int
							Color32 val;
							val.r = r_unzip.ReadByte();	// unzip, byte
							val.g = r_unzip.ReadByte();	// unzip, byte
							val.b = r_unzip.ReadByte();	// unzip, byte
							val.a = r_unzip.ReadByte();	// unzip, byte
							m_Colors.Add(key, val);
						}
						r_unzip.Close();
					}
				}
				break;
			}
			default: return false;
			}
			r.Close();
			return true;
		}
	}
	
	// Extract Header
	// stream
	private static int ExtractHeader(Stream stream, out VCIsoHeadData iso_header)
	{
		iso_header = new VCIsoHeadData ();
		int size = 0;
		try
		{
			BinaryReader r = new BinaryReader (stream);
			
			// Header
			string check_str = r.ReadString();	// r, string
			if ( check_str != "VCISO" )
			{
				stream.Close();
				return 0;
			}
			int l = 0;
			iso_header.Version = r.ReadInt32();	// r, int
			iso_header.Category = (EVCCategory)(r.ReadInt32());	// r, int
			string author = "";
			if ( iso_header.Version >= 0x02020000 )
				author = r.ReadString();
			iso_header.Name = r.ReadString();	// r, string
			iso_header.Desc = r.ReadString();	// r, string
			string remarks = "";
			if ( iso_header.Version >= 0x02020000 )
				remarks = r.ReadString();
			iso_header.xSize = r.ReadInt32();	// r, int
			iso_header.ySize = r.ReadInt32();	// r, int
			iso_header.zSize = r.ReadInt32();	// r, int
			l = r.ReadInt32();	// r, int
			iso_header.IconTex = r.ReadBytes(l);	// r, byte[]
			size = (int)(stream.Length);
			stream.Close();

			iso_header.Author = "";
			iso_header.Remarks = "";
			if ( iso_header.Version >= 0x02020000 )
			{
				for ( int c = 0; c < author.Length; ++c )
					iso_header.Author += (char)(author[c] ^ (char)(0xAC));
				for ( int c = 0; c < remarks.Length; ++c )
					iso_header.Remarks += (char)(remarks[c] ^ (char)(0xAC));
			}

			if ( iso_header.Name.Length > 256 )
				return 0;
			if ( iso_header.Desc.Length > 2048 )
				return 0;
			if ( iso_header.Author.Length > 256 )
				return 0;
			if ( iso_header.Remarks.Length > 8192 )
				return 0;
			if ( iso_header.xSize < 0 )
				return 0;
			if ( iso_header.xSize >= 512 )
				return 0;
			if ( iso_header.ySize < 0 )
				return 0;
			if ( iso_header.ySize >= 512 )
				return 0;
			if ( iso_header.zSize < 0 )
				return 0;
			if ( iso_header.zSize >= 512 )
				return 0;
		}
		catch (Exception)
		{
			return 0;
		}	
		iso_header.EnsureIconTexValid();
		return size;
	}
	// file
	public static int ExtractHeader(string filename, out VCIsoHeadData iso_header)
	{
		iso_header = new VCIsoHeadData ();
		if ( !File.Exists(filename) )
			return 0;
		try
		{
			using ( FileStream fs = File.Open(filename, FileMode.Open) )
			{
				return ExtractHeader(fs, out iso_header);
			}
		}
		catch (Exception)
		{
			return 0;
		}
	}
	// byte buffer
	public static int ExtractHeader(byte[] buffer, out VCIsoHeadData iso_header)
	{
		iso_header = new VCIsoHeadData ();
		if ( buffer == null )
			return 0;
		using ( MemoryStream ms = new MemoryStream (buffer) )
		{
			return ExtractHeader(ms, out iso_header);
		}
	}
}

