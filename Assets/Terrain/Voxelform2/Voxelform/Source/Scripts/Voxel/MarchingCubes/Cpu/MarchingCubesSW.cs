using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// cpu implementation of non-indexed marching cubes.
public partial class MarchingCubesSW
{
	// Lists used for rebuilding the mesh.
	List<Vector3> _vertexList = new List<Vector3>();
	List<Vector2> _norm01 = new List<Vector2>();
	List<Vector2> _norm2t = new List<Vector2>();

	// pointer to the chunkData to be computed.
	byte[] chunkData;

	int[] indicesConst;
	
	public List<Vector3> VertexList{	get{ return _vertexList;	} }
	public List<Vector2> Norm01List{	get{ return _norm01;		} }
	public List<Vector2> Norm2tList{	get{ return _norm2t;		} }
	
	// Used and reused by GetTriangles to build a group of triangles.

	public MarchingCubesSW()
	{
		indicesConst = new int[64999];
		for(int i = 0; i < indicesConst.Length; i++){
			indicesConst[i] = i;
		}
	}
	public static int OneIndex(int x, int y, int z)
	{
		int ret = z * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED +
				y * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE +
				x;

		return ret;
	}
	public static int OneIndexPrefixed(int x, int y, int z)
	{
		return	(z + VoxelTerrainConstants._numVoxelsPrefix) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED +
				(y + VoxelTerrainConstants._numVoxelsPrefix) * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE +
				(x + VoxelTerrainConstants._numVoxelsPrefix);
	}
	public void setInputChunkData(byte[] _input)
	{
		chunkData = _input;
	}
	public void Rebuild()
	{
		_vertexList.Clear();
		_norm01.Clear();
		_norm2t.Clear();

		for (int pz = 1; pz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 2; ++pz)
		{
			for (int py = 1; py < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 2; ++py)
			{
				for (int px = 1; px < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 2; ++px)
				{
					BuildRegularVoxelTriangles(px, py, pz);
				}
			}
		}
	}
	public Mesh RebuildMesh()
	{
		Rebuild();

		Mesh _mesh;
		_mesh = new Mesh();
		_mesh.Clear();
		_mesh.vertices = _vertexList.ToArray();
		_mesh.uv = _norm01.ToArray();
		_mesh.uv2 = _norm2t.ToArray();
		_mesh.subMeshCount = 1;
		
		int[] indices = new int[_vertexList.Count];
		Array.Copy(indicesConst, indices, _vertexList.Count);
		_mesh.SetTriangles(indices, 0);

		return _mesh;
	}

	Vector3 VertexInterp(VoxelInterpolationInfo val1, VoxelInterpolationInfo val2)
	{
		float mu = (float)(VoxelTerrainConstants._isolevelInByte - val1.Volume) / (float)(val2.Volume - val1.Volume);
		Vector3 p = Vector3.Lerp(val1.XYZ, val2.XYZ, mu);
		return p;
	}
	Vector3 normalInterp(Vector3 n1, Vector3 n2, VoxelInterpolationInfo val1, VoxelInterpolationInfo val2)
	{
		float mu = (float)(VoxelTerrainConstants._isolevelInByte - val1.Volume) / (float)(val2.Volume - val1.Volume);
		Vector3 p = Vector3.Lerp(n1, n2, mu);
		return p;
	}

	VoxelInterpolationInfo[] val = new VoxelInterpolationInfo[32];

	Vector3[] vertlist = new Vector3[12];
	Vector3[] tmpN = new Vector3[8];
	Vector3[] normlist = new Vector3[12];
	int[] matIdx = new int[3];
	void BuildRegularVoxelTriangles(int vx, int vy, int vz)
	{
		int px = vx - 1;
		int py = vy - 1;
		int pz = vz - 1;
		
		// Setup data for use by VertexInterp.
		
//		val[0] = new VoxelInterpolationInfo(vx, vy, pz, _voxels.Read(px, py, pz).Volume, _voxels.Read(px, py, pz).Type);
//		val[1] = new VoxelInterpolationInfo(px + 1, py, pz, _voxels.Read(px + 1, py, pz).Volume, _voxels.Read(px + 1, py, pz).Type);
//		val[2] = new VoxelInterpolationInfo(px + 1, py + 1, pz, _voxels.Read(px + 1, py + 1, pz).Volume, _voxels.Read(px + 1, py + 1, pz).Type);
//		val[3] = new VoxelInterpolationInfo(px, py + 1, pz, _voxels.Read(px, py + 1, pz).Volume, _voxels.Read(px, py + 1, pz).Type);
//		val[4] = new VoxelInterpolationInfo(px, py, pz + 1, _voxels.Read(px, py, pz + 1).Volume, _voxels.Read(px, py, pz + 1).Type);
//		val[5] = new VoxelInterpolationInfo(px + 1, py, pz + 1, _voxels.Read(px + 1, py, pz + 1).Volume, _voxels.Read(px + 1, py, pz + 1).Type);
//		val[6] = new VoxelInterpolationInfo(px + 1, py + 1, pz + 1, _voxels.Read(px + 1, py + 1, pz + 1).Volume, _voxels.Read(px + 1, py + 1, pz + 1).Type);
//		val[7]= new VoxelInterpolationInfo(px, py + 1, pz + 1, _voxels.Read(px, py + 1, pz + 1).Volume, _voxels.Read(px, py + 1, pz + 1).Type);
		
		val[0].Volume = chunkData[OneIndex(vx, vy, vz) << 1];
		val[1].Volume = chunkData[OneIndex(vx + 1, vy, vz) << 1];
		val[2].Volume = chunkData[OneIndex(vx + 1, vy + 1, vz) << 1];
		val[3].Volume = chunkData[OneIndex(vx, vy + 1, vz) << 1];
		val[4].Volume = chunkData[OneIndex(vx, vy, vz + 1) << 1];
		val[5].Volume = chunkData[OneIndex(vx + 1, vy, vz + 1) << 1];
		val[6].Volume = chunkData[OneIndex(vx + 1, vy + 1, vz + 1) << 1];
		val[7].Volume = chunkData[OneIndex(vx, vy + 1, vz + 1) << 1];
		
		
		int cubeindex = 0;	//Note: using < instead of >=, to use case = instead of case ][ 
		if (val[0].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 1;
		if (val[1].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 2;
		if (val[2].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 4;
		if (val[3].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 8;
		if (val[4].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 16;
		if (val[5].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 32;
		if (val[6].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 64;
		if (val[7].Volume < VoxelTerrainConstants._isolevelInByte) cubeindex |= 128;
		// If there's nothing to build, then exit.
		if (edgeTable[cubeindex] == 0)return;
		
		val[0].XYZ.x = px;
		val[0].XYZ.y = py;
		val[0].XYZ.z = pz;
		val[0].VType = chunkData[(OneIndex(vx, vy, vz) << 1) + 1];

		val[1].XYZ.x = px + 1;
		val[1].XYZ.y = py;
		val[1].XYZ.z = pz;
		val[1].VType = chunkData[(OneIndex(vx + 1, vy, vz) << 1) + 1];

		val[2].XYZ.x = px + 1;
		val[2].XYZ.y = py + 1;
		val[2].XYZ.z = pz;
		val[2].VType = chunkData[(OneIndex(vx + 1, vy + 1, vz) << 1) + 1];

		val[3].XYZ.x = px;
		val[3].XYZ.y = py + 1;
		val[3].XYZ.z = pz;
		val[3].VType = chunkData[(OneIndex(vx, vy + 1, vz) << 1) + 1];

		val[4].XYZ.x = px;
		val[4].XYZ.y = py;
		val[4].XYZ.z = pz + 1;
		val[4].VType = chunkData[(OneIndex(vx, vy, vz + 1) << 1) + 1];

		val[5].XYZ.x = px + 1;
		val[5].XYZ.y = py;
		val[5].XYZ.z = pz + 1;
		val[5].VType = chunkData[(OneIndex(vx + 1, vy, vz + 1) << 1) + 1];

		val[6].XYZ.x = px + 1;
		val[6].XYZ.y = py + 1;
		val[6].XYZ.z = pz + 1;
		val[6].VType = chunkData[(OneIndex(vx + 1, vy + 1, vz + 1) << 1) + 1];

		val[7].XYZ.x = px;
		val[7].XYZ.y = py + 1;
		val[7].XYZ.z = pz + 1;
		val[7].VType = chunkData[(OneIndex(vx, vy + 1, vz + 1) << 1) + 1];
		
		
		val[8].Volume = chunkData[OneIndex(vx - 1,	vy,		vz) << 1];
		val[9].Volume = chunkData[OneIndex(vx - 1,	vy + 1,	vz) << 1];
		val[10].Volume = chunkData[OneIndex(vx - 1,	vy,		vz + 1) << 1];
		val[11].Volume = chunkData[OneIndex(vx,		vy - 1, vz) << 1];
		val[12].Volume = chunkData[OneIndex(vx + 1,	vy - 1, vz) << 1];
		val[13].Volume = chunkData[OneIndex(vx,		vy - 1, vz + 1) << 1];
		val[14].Volume = chunkData[OneIndex(vx,		vy,		vz - 1) << 1];
		val[15].Volume = chunkData[OneIndex(vx + 1,	vy,		vz - 1) << 1];
		val[16].Volume = chunkData[OneIndex(vx,		vy + 1, vz - 1) << 1];
		val[17].Volume = chunkData[OneIndex(vx - 1,	vy + 1, vz + 1) << 1];
		val[18].Volume = chunkData[OneIndex(vx + 1,	vy - 1, vz + 1) << 1];
		val[19].Volume = chunkData[OneIndex(vx + 1,	vy + 1, vz - 1) << 1];
		
		
		val[20].Volume = chunkData[OneIndex(vx + 2,	vy,		vz) << 1];
		val[21].Volume = chunkData[OneIndex(vx + 2,	vy + 1,	vz) << 1];
		val[22].Volume = chunkData[OneIndex(vx,		vy + 2,	vz) << 1];
		val[23].Volume = chunkData[OneIndex(vx,		vy,		vz + 2) << 1];
		val[24].Volume = chunkData[OneIndex(vx + 2,	vy,		vz + 1) << 1];
		val[25].Volume = chunkData[OneIndex(vx + 2,	vy + 1, vz + 1) << 1];
		
		val[26].Volume = chunkData[OneIndex(vx,		vy + 2,	vz + 1) << 1];
		val[27].Volume = chunkData[OneIndex(vx + 1,	vy + 2,	vz) << 1];
		val[28].Volume = chunkData[OneIndex(vx + 1,	vy,		vz + 2) << 1];
		val[29].Volume = chunkData[OneIndex(vx + 1,	vy + 2, vz + 1) << 1];
		val[30].Volume = chunkData[OneIndex(vx + 1,	vy + 1, vz + 2) << 1];
		val[31].Volume = chunkData[OneIndex(vx,		vy + 1, vz + 2) << 1];
		
	    tmpN[0] = new Vector3((val[ 8].Volume - val[ 1].Volume) / 255.0f,(val[11].Volume - val[ 3].Volume) / 255.0f,(val[14].Volume - val[ 4].Volume) / 255.0f);
	    tmpN[1] = new Vector3((val[ 0].Volume - val[20].Volume) / 255.0f,(val[12].Volume - val[ 2].Volume) / 255.0f,(val[15].Volume - val[ 5].Volume) / 255.0f);
	    tmpN[3] = new Vector3((val[ 9].Volume - val[ 2].Volume) / 255.0f,(val[ 0].Volume - val[22].Volume) / 255.0f,(val[16].Volume - val[ 7].Volume) / 255.0f);
	    tmpN[4] = new Vector3((val[10].Volume - val[ 5].Volume) / 255.0f,(val[13].Volume - val[ 7].Volume) / 255.0f,(val[ 0].Volume - val[23].Volume) / 255.0f);
	    tmpN[2] = new Vector3((val[ 3].Volume - val[21].Volume) / 255.0f,(val[ 1].Volume - val[27].Volume) / 255.0f,(val[19].Volume - val[ 6].Volume) / 255.0f);
	    tmpN[5] = new Vector3((val[ 4].Volume - val[24].Volume) / 255.0f,(val[18].Volume - val[ 6].Volume) / 255.0f,(val[ 1].Volume - val[28].Volume) / 255.0f);
	    tmpN[6] = new Vector3((val[ 7].Volume - val[25].Volume) / 255.0f,(val[ 5].Volume - val[29].Volume) / 255.0f,(val[ 2].Volume - val[30].Volume) / 255.0f);
	    tmpN[7] = new Vector3((val[17].Volume - val[ 6].Volume) / 255.0f,(val[ 4].Volume - val[26].Volume) / 255.0f,(val[ 3].Volume - val[31].Volume) / 255.0f);
		
		
		
//		field[ 8] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(-1, 0, 0, 0)).x);
//	    field[ 9] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(-1, 1, 0, 0)).x);
//	    field[10] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(-1, 0, 1, 0)).x);
//	    field[11] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 0,-1, 0, 0)).x);
//	    field[12] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 1,-1, 0, 0)).x);
//	    field[13] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 0,-1, 1, 0)).x);
//	    field[14] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 0, 0,-1, 0)).x);
//	    field[15] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 1, 0,-1, 0)).x);
//	    field[16] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 0, 1,-1, 0)).x);
//	    field[17] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(-1, 1, 1, 0)).x);
//	    field[18] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 1,-1, 1, 0)).x);
//	    field[19] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)( 1, 1,-1, 0)).x);
		
//		field[20] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(2, 0, 0, 0)).x);
//	    field[21] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(2, 1, 0, 0)).x);
//	    field[22] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(0, 2, 0, 0)).x);
//	    field[23] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(0, 0, 2, 0)).x);
//	    field[24] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(2, 0, 1, 0)).x);
//	    field[25] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(2, 1, 1, 0)).x);
		
		
//	    field[26] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(0, 2, 1, 0)).x);
//	    field[27] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(1, 2, 0, 0)).x);
//	    field[28] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(1, 0, 2, 0)).x);
//	    field[29] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(1, 2, 1, 0)).x);
//	    field[30] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(1, 1, 2, 0)).x);
//	    field[31] = (float)(read_imagef(volume, volumeSampler, voxelPos + (int4)(0, 1, 2, 0)).x);


		// Begin building the triangles from lookup tables.

		if ((edgeTable[cubeindex] & 1) > 0)
		{
			vertlist[0] = VertexInterp(val[0], val[1]);
			normlist[0] = normalInterp(tmpN[0], tmpN[1], val[0], val[1]);
		
		}
		if ((edgeTable[cubeindex] & 2) > 0)
		{
			vertlist[1] = VertexInterp(val[1], val[2]);
			normlist[1] = normalInterp(tmpN[1], tmpN[2], val[1], val[2]);
		
		}
		if ((edgeTable[cubeindex] & 4) > 0)
		{
			vertlist[2] = VertexInterp(val[2], val[3]);
			normlist[2] = normalInterp(tmpN[2], tmpN[3], val[2], val[3]);
		
		}
		if ((edgeTable[cubeindex] & 8) > 0)
		{
			vertlist[3] = VertexInterp(val[3], val[0]);
			normlist[3] = normalInterp(tmpN[3], tmpN[0], val[3], val[0]);
		
		}
		if ((edgeTable[cubeindex] & 16) > 0)
		{
			vertlist[4] = VertexInterp(val[4], val[5]);
			normlist[4] = normalInterp(tmpN[4], tmpN[5], val[4], val[5]);
		
		}
		if ((edgeTable[cubeindex] & 32) > 0)
		{
			vertlist[5] = VertexInterp(val[5], val[6]);
			normlist[5] = normalInterp(tmpN[5], tmpN[6], val[5], val[6]);
		
		}
		if ((edgeTable[cubeindex] & 64) > 0)
		{
			vertlist[6] = VertexInterp(val[6], val[7]);
			normlist[6] = normalInterp(tmpN[6], tmpN[7], val[6], val[7]);
		
		}
		if ((edgeTable[cubeindex] & 128) > 0)
		{
			vertlist[7] = VertexInterp(val[7], val[4]);
			normlist[7] = normalInterp(tmpN[7], tmpN[4], val[7], val[4]);
		
		}

		if ((edgeTable[cubeindex] & 256) > 0)
		{
			vertlist[8] = VertexInterp(val[0], val[4]);
			normlist[8] = normalInterp(tmpN[0], tmpN[4], val[0], val[4]);
		
		}

		if ((edgeTable[cubeindex] & 512) > 0)
		{
			vertlist[9] = VertexInterp(val[1], val[5]);
			normlist[9] = normalInterp(tmpN[1], tmpN[5], val[1], val[5]);
		
		}
		if ((edgeTable[cubeindex] & 1024) > 0)
		{
			vertlist[10] = VertexInterp(val[2], val[6]);
			normlist[10] = normalInterp(tmpN[2], tmpN[6], val[2], val[6]);
		
		}
		if ((edgeTable[cubeindex] & 2048) > 0)
		{
			vertlist[11] = VertexInterp(val[3], val[7]);
			normlist[11] = normalInterp(tmpN[3], tmpN[7], val[3], val[7]);
		}
		
//		vertlist[0] = VertexInterp(val[0], val[1]);
//		vertlist[1] = VertexInterp(val[1], val[2]);
//		vertlist[2] = VertexInterp(val[2], val[3]);
//		vertlist[3] = VertexInterp(val[3], val[0]);
//		vertlist[4] = VertexInterp(val[4], val[5]);
//		vertlist[5] = VertexInterp(val[5], val[6]);
//		vertlist[6] = VertexInterp(val[6], val[7]);
//		vertlist[7] = VertexInterp(val[7], val[4]);
//		vertlist[8] = VertexInterp(val[0], val[4]);
//		vertlist[9] = VertexInterp(val[1], val[5]);
//		vertlist[10] = VertexInterp(val[2], val[6]);
//		vertlist[11] = VertexInterp(val[3], val[7]);
		
//		normlist[0] = normalInterp(tmpN[0], tmpN[1], val[0], val[1]);
//		normlist[1] = normalInterp(tmpN[1], tmpN[2], val[1], val[2]);
//		normlist[2] = normalInterp(tmpN[2], tmpN[3], val[2], val[3]);
//		normlist[3] = normalInterp(tmpN[3], tmpN[0], val[3], val[0]);
//		normlist[4] = normalInterp(tmpN[4], tmpN[5], val[4], val[5]);
//		normlist[5] = normalInterp(tmpN[5], tmpN[6], val[5], val[6]);
//		normlist[6] = normalInterp(tmpN[6], tmpN[7], val[6], val[7]);
//		normlist[7] = normalInterp(tmpN[7], tmpN[4], val[7], val[4]);
//		normlist[8] = normalInterp(tmpN[0], tmpN[4], val[0], val[4]);
//		normlist[9] = normalInterp(tmpN[1], tmpN[5], val[1], val[5]);
//		normlist[10] = normalInterp(tmpN[2], tmpN[6], val[2], val[6]);
//		normlist[11] = normalInterp(tmpN[3], tmpN[7], val[3], val[7]);

		for (int i = 0; triTable[cubeindex, i] != -1; i += 3)
		{
			int idx0 = triTable[cubeindex, i];
			int idx1 = triTable[cubeindex, i+1];
			int idx2 = triTable[cubeindex, i+2];

			Vector3 v0 = vertlist[idx0];
			Vector3 v1 = vertlist[idx1];
			Vector3 v2 = vertlist[idx2];
			
			Vector3 n0 = normlist[idx0];
			Vector3 n1 = normlist[idx1];
			Vector3 n2 = normlist[idx2];
			
			int one = edgeInfo[idx0 * 2];
        	int two = edgeInfo[idx0 * 2 + 1];
			matIdx[0] = val[((cubeindex & (1 << one)) == 0)? one:two].VType;
			
			one = edgeInfo[idx1 * 2];
        	two = edgeInfo[idx1 * 2 + 1];
			matIdx[1] = val[((cubeindex & (1 << one)) == 0)? one:two].VType;
			
			one = edgeInfo[idx2 * 2];
        	two = edgeInfo[idx2 * 2 + 1];
			matIdx[2] = val[((cubeindex & (1 << one)) == 0)? one:two].VType;

			_vertexList.Add(v0);
			_vertexList.Add(v1);
			_vertexList.Add(v2);
			
        	int mat0  = matIdx[0]*4 + 2; 
			int mat12 = matIdx[1]*256 + matIdx[2];
        	
			
            _norm2t.Add(new Vector2(mat0 + n0.z, mat12 + 0.0f));//matIdx[0];
			_norm2t.Add(new Vector2(mat0 + n1.z, mat12 + 0.1f));//matIdx[1];
			_norm2t.Add(new Vector2(mat0 + n2.z, mat12 + 0.2f));//matIdx[2];
				
			_norm01.Add(new Vector2(n0.x,n0.y));
			_norm01.Add(new Vector2(n1.x,n1.y));
			_norm01.Add(new Vector2(n2.x,n2.y));

		}

	}
	
	struct VoxelInterpolationInfo
	{
		public Vector3 XYZ;
		public byte Volume;
		public byte VType;

		public VoxelInterpolationInfo( float x,  float y,  float z, byte volume, byte vtype)
		{
			XYZ.x = x;
			XYZ.y = y;
			XYZ.z = z;
			Volume = volume;
			VType = vtype;
		}

		public VoxelInterpolationInfo(Vector3 xyz, byte volume, byte vtype)
		{
			XYZ = xyz;
			Volume = volume;
			VType = vtype;
		}
	}
}