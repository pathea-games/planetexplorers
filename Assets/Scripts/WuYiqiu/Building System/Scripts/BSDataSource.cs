using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// voxel terrain data.
/// </summary>
public class BSVoxelData :IBSDataSource
{
	public static int s_ScaleInverted { get { return 1;} }
	public static float s_Scale { get {return 1.0f;} }

	public int ScaleInverted { get{ return s_ScaleInverted;} }

	public float Scale { get { return s_Scale; } }

	public Vector3 Offset { get { return new Vector3(-0.5f, -0.5f, -0.5f);} }

	public float DiagonalOffset {get {return 0.15f;}}

	public int DataType { get { return (int)EBSVoxelType.Voxel;}}

	public Bounds Lod0Bound { get { return VFVoxelTerrain.self.LodMan._Lod0ViewBounds; }}
		
	public BSVoxel Read (int x, int y, int z, int lod = 0)
	{
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.Read(x, y, z, lod);
		return new BSVoxel(voxel);

	}

	public int Write(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		VFVoxelTerrain.self.AlterVoxelInBuild(x, y, z, voxel.ToVoxel());
		return 0;
	}

	public BSVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(x, y, z, lod);
		return new BSVoxel(voxel);
	}

	public bool SafeWrite(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		VFVoxelTerrain.self.AlterVoxelInBuild(x, y, z, voxel.ToVoxel());
		return true;
	}

	public bool VoxelIsZero (BSVoxel voxel, float volume)
	{
		return voxel.value0 < volume;
	}

	public BSVoxel Add(BSVoxel voxel, int x, int y, int z, int lod = 0 )
	{
		VFVoxel old_voxel = VFVoxelTerrain.self.Voxels.Read(x, y, z, lod);
		voxel.volmue = (byte)Mathf.Clamp( old_voxel.Volume + voxel.volmue, 0, 255);
		
		VFVoxelTerrain.self.Voxels.Write(x, y, z, voxel.ToVoxel(), lod);

		return voxel;
	}

	public BSVoxel Subtract(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		VFVoxel old_voxel = VFVoxelTerrain.self.Voxels.Read(x, y, z, lod);
		VFVoxel new_voxel = new VFVoxel(old_voxel.Volume, old_voxel.Type);
		new_voxel.Volume = (byte)Mathf.Clamp( old_voxel.Volume - voxel.volmue, 0, 255);
		
		VFVoxelTerrain.self.AlterVoxelInBuild(x, y, z, new_voxel);

		return voxel;
	}

	public bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<BSVoxel> voxels)
	{
		posList = new List<IntVector4>();
		voxels = new List<BSVoxel>();
		return false;

	}
}

/// <summary>
/// block terrain data.
/// </summary>
public class BSBlock45Data :IBSDataSource
{
    public static System.Action<int[]> voxelWrite;
	public static int s_ScaleInverted { get { return Block45Constants._scaleInverted;} }
	public static float s_Scale { get {return Block45Constants._scale;} }

	public int ScaleInverted { get{ return s_ScaleInverted;} }

	public float Scale { get { return s_Scale; } }

	public Vector3 Offset { get { return Vector3.zero;} }

	public float DiagonalOffset {get {return 0.25f;}}

	public Bounds Lod0Bound { get { return Block45Man.self.LodMan._Lod0ViewBounds; }}

	public int DataType { get { return (int)EBSVoxelType.Block;}}

	public BSVoxel Read (int x, int y, int z, int lod = 0)
	{
		B45Block block = Block45Man.self.DataSource.Read(x, y, z, lod);
		return new BSVoxel(block);
		
	}
	
	public int Write(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
        if (voxelWrite != null)
            voxelWrite.Invoke(new int[] { x, y, z});

		return Block45Man.self.DataSource.Write(voxel.ToBlock(), x, y, z, lod);
	}
	
	public BSVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		B45Block block = Block45Man.self.DataSource.SafeRead(x, y, z, lod);
		return new BSVoxel(block);
	}
	
	public bool SafeWrite(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
        if (voxelWrite != null)
            voxelWrite.Invoke(new int[] { x, y, z});

		return Block45Man.self.DataSource.SafeWrite(voxel.ToBlock(), x, y, z, lod);
	}

	public bool VoxelIsZero (BSVoxel voxel, float volume)
	{
		return (voxel.value0 >> 2) == 0;
	}

	public BSVoxel Add(BSVoxel voxel, int x, int y, int z, int lod = 0 )
	{
		if ((voxel.value0 >> 2) == 0)
			return new BSVoxel(Block45Man.self.DataSource.Read(x, y, z));
		Block45Man.self.DataSource.Write(voxel.ToBlock(), x, y, z, lod);
		
		return voxel;
	}
	
	public BSVoxel Subtract(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		B45Block block = new B45Block();
		Block45Man.self.DataSource.Write(block, x, y, z, lod);
		
		return new BSVoxel(block);
	}

	public bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<BSVoxel> voxels)
	{
		List<B45Block> blocks = null;
		if ( Block45Man.self.DataSource.ReadExtendableBlock(pos, out posList, out blocks))
		{
			voxels = new List<BSVoxel>();
			foreach (B45Block b in blocks)
			{
				voxels.Add(new BSVoxel(b));
			}
			return true;
		}
		else
		{
			posList = null;
			voxels = null;
			return false;
		}
		
	}
}

 