using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// cpu implementation of indexed marching cubes.
public partial class MarchingCubesSWIndexed
{
	
	// Lists used for rebuilding the mesh.
	List<Vector3> _vertexList = new List<Vector3>();
	List<Vector2> _norm01 = new List<Vector2>();
	List<Vector2> _norm2t = new List<Vector2>();
	
	//List<Color> _colorList = new List<Color>();
	
	List<int> _indexList = new List<int>();
	
	
	// pointer to the input chunkData
	byte[] chunkData;
	
	ushort _baseIndex = 0;
	int tempIndexBufferPingpongIdx;
	
	// Used and reused by GetTriangles to build a group of triangles.

	static int IndicesPerCell = 4;
	ushort[] indexTempBuffer = new ushort[ IndicesPerCell * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE 
		* VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE * 2];
	
	public MarchingCubesSWIndexed()
	{
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
	public Mesh RebuildMesh()
	{
		Mesh _mesh;
		_mesh = new Mesh();

		int numSubmeshes = 1;

		_mesh.Clear();
		
		_vertexList.Clear();
		_norm01.Clear();
		_norm2t.Clear();
		//_colorList.Clear();

		_baseIndex = 0;
		tempIndexBufferPingpongIdx = 0;
		
		List<int[]> subindices = new List<int[]>();

		for (int pz = 0; pz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 1; ++pz)
		{
			for (int py = 0; py < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 1; ++py)
			{
				for (int px = 0; px < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 1; ++px)
				{
//					if(px == 0 && py == 0 && pz == 0)
//						continue;
					BuildRegularVoxelTriangles(px, py, pz);

				}
			}
			tempIndexBufferPingpongIdx = (tempIndexBufferPingpongIdx + 1) % 2;
		}

		subindices.Add(_indexList.ToArray());
		
	
		_mesh.vertices = _vertexList.ToArray();
		_mesh.uv = _norm01.ToArray();
		_mesh.uv2 = _norm2t.ToArray();

		_mesh.subMeshCount = numSubmeshes;
		_mesh.SetTriangles(_indexList.ToArray(), 0);
//		for (int i = 0; i < subindices.Count; ++i)
//		{
//			_mesh.SetTriangles(subindices[i], i);
//		}
		_indexList.Clear();
#if false
		_mesh.RecalculateNormals();
		
		var normals = _mesh.normals;
		int numNormals = _mesh.normals.Length;

		for (int i = 0; i < numNormals; ++i)
		{
			Vector3 n1 = _normalList[i].normalized;
			Vector3 n2 = normals[i].normalized;
			Vector3 n3 = n1 - n2;

			float dx = Mathf.Abs(n3.x);
			float dy = Mathf.Abs(n3.y);
			float dz = Mathf.Abs(n3.z);

			Vector3 result;

			result.x = Mathf.Lerp(n1.x, n2.x, Mathf.Max(0, dx * dx - .1f));
			result.y = Mathf.Lerp(n1.y, n2.y, Mathf.Max(0, dy * dy - .1f));
			result.z = Mathf.Lerp(n1.z, n2.z, Mathf.Max(0, dz * dz - .1f));

			normals[i] = result;

		}

		_mesh.normals = normals;
#endif
		
		return _mesh;
		
	}
	
	Vector3 VertexInterp(Vector3 p1, Vector3 p2, VoxelInterpolationInfo val1, VoxelInterpolationInfo val2, int index)
	{
		float mu = (float)(VoxelTerrainConstants._isolevelInByte - val1.Volume) / (float)(val2.Volume - val1.Volume);
		Vector3 p = Vector3.Lerp(p1, p2, mu);
		return p;
		
	}
	
	int tvTempIndexBufferPingpongIdx;
	
	VoxelInterpolationInfo[] val = new VoxelInterpolationInfo[8];
	
	byte[] low8_array = new byte[3];
	void BuildRegularVoxelTriangles(int px, int py, int pz)
	{

		val[0].XYZ.x = px;
		val[0].XYZ.y = py;
		val[0].XYZ.z = pz;
		val[0].Volume = chunkData[OneIndex(px, py, pz) << 1];
		val[0].VType = chunkData[(OneIndex(px, py, pz) << 1) + 1];
		
		val[1].XYZ.x = px + 1;
		val[1].XYZ.y = py;
		val[1].XYZ.z = pz;
		val[1].Volume = chunkData[OneIndex(px + 1, py, pz) << 1];
		val[1].VType = chunkData[(OneIndex(px + 1, py, pz) << 1) + 1];
		
		val[2].XYZ.x = px;
		val[2].XYZ.y = py + 1;
		val[2].XYZ.z = pz;
		val[2].Volume = chunkData[OneIndex(px, py + 1, pz) << 1];
		val[2].VType = chunkData[(OneIndex(px, py + 1, pz) << 1) + 1];
		
		val[3].XYZ.x = px + 1;
		val[3].XYZ.y = py + 1;
		val[3].XYZ.z = pz;
		val[3].Volume = chunkData[OneIndex(px + 1, py + 1, pz) << 1];
		val[3].VType = chunkData[(OneIndex(px + 1, py + 1, pz) << 1) + 1];
		

		val[4].XYZ.x = px;
		val[4].XYZ.y = py;
		val[4].XYZ.z = pz + 1;
		val[4].Volume = chunkData[OneIndex(px, py, pz + 1) << 1];
		val[4].VType = chunkData[(OneIndex(px, py, pz + 1) << 1) + 1];
		
		val[5].XYZ.x = px + 1;
		val[5].XYZ.y = py;
		val[5].XYZ.z = pz + 1;
		val[5].Volume = chunkData[OneIndex(px + 1, py, pz + 1) << 1];
		val[5].VType = chunkData[(OneIndex(px + 1, py, pz + 1) << 1) + 1];
		
		val[6].XYZ.x = px;
		val[6].XYZ.y = py + 1;
		val[6].XYZ.z = pz + 1;
		val[6].Volume = chunkData[OneIndex(px, py + 1, pz + 1) << 1];
		val[6].VType = chunkData[(OneIndex(px, py + 1, pz + 1) << 1) + 1];
		
		val[7].XYZ.x = px + 1;
		val[7].XYZ.y = py + 1;
		val[7].XYZ.z = pz + 1;
		val[7].Volume = chunkData[OneIndex(px + 1, py + 1, pz + 1) << 1];
		val[7].VType = chunkData[(OneIndex(px + 1, py + 1, pz + 1) << 1) + 1];
		
		byte cubeIndex = 0;
		if(val[0].Volume > 127)
			cubeIndex |= 1;
		if(val[1].Volume > 127)
			cubeIndex |= 2;
		if(val[2].Volume > 127)
			cubeIndex |= 4;
		if(val[3].Volume > 127)
			cubeIndex |= 8;
		if(val[4].Volume > 127)
			cubeIndex |= 16;
		if(val[5].Volume > 127)
			cubeIndex |= 32;
		if(val[6].Volume > 127)
			cubeIndex |= 64;
		if(val[7].Volume > 127)
			cubeIndex |= 128;
		
		if(cubeIndex == 0)
			return;
		
		int directionCode = 0;
		directionCode = (px == 0) ? 1 : 0;
		directionCode |= ((py == 0) ? 1 : 0) << 1;
		directionCode |= ((pz == 0) ? 1 : 0) << 2;
		int cellClass = regularCellClass[cubeIndex];
		
		int triCount = regularCellData[cellClass, 0] & 15;
		//int uniquevertCount = regularCellData[cellClass, 0] >> 4; working
		int vertCount = triCount * 3;		
		//byte[] matIdx = new byte[3];		
		for(int i = 0; i < vertCount; i ++ )
		{
			int id2 = regularCellData[cellClass, i + 1];
			ushort ushrt = regularVertexData[(int)cubeIndex, id2];
			byte hiNibble = (byte)(ushrt >> 12);		// if it is 8, a new vertex is to be created. otherwise, it contains the bits to locate the reuseable vertex.
			byte loNibble = (byte)((ushrt >> 8 )& 15); // index of the vertex index in each cell. each cell contains 4 slots for indices.
			int vertPos0 = (int)(ushrt & 15); // vert position 0 - 7
			int vertPos1 = (int)((ushrt >> 4) & 15);		
			
			// left 8 is a debug variable that should be one of the edge numbers listed in Figure 3.8 (b)
			
			// calculate the offset to the current cell's position
			IntVector3 ofs = new IntVector3();
			ofs.x = px - (hiNibble & 1);
			ofs.y = py - ((hiNibble >> 1) & 1);
			ofs.z = (hiNibble >> 2) & 1;
			
			int sliceId = Mathf.Abs(tempIndexBufferPingpongIdx - ofs.z);
			int idxTmpBfrIdx = IndicesPerCell * (ofs.x + 
					ofs.y * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE + 
					sliceId * VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED )
					+ loNibble;
			
			byte low8 = (byte)(ushrt & 255);
		
			if((hiNibble & 8) > 0)
			{
				
				// calculate the position of the vertex.
				Vector3 newVertPos;
				// use vertPos0, 1 to determine the two positions to interpolate between.
				newVertPos = VertexInterp(val[vertPos0].XYZ, val[vertPos1].XYZ, val[vertPos0], val[vertPos1], 0);
				
				if( directionCode == 0)
				{
					
					// a new vertex is to be created on one of the maximum edges. also write the index to the indextempbuffer
					
					_vertexList.Add(newVertPos);
					low8_array[0] = low8;
					addMatIndex(cubeIndex);
					// write the vertex index into indextempbuffer
					indexTempBuffer[idxTmpBfrIdx] = _baseIndex;
					_indexList.Add(_baseIndex);
					_baseIndex++;
					
				}
				else 
				{
					if( (loNibble == 1 && 
						py > 0)
						||
						(loNibble == 2 && 
						px > 0)
						||
						(loNibble == 3 && 
						pz > 0)
						)
					{
						
						_vertexList.Add(newVertPos);
						low8_array[0] = low8;
						addMatIndex(cubeIndex);
						// write the vertex index into indextempbuffer
						indexTempBuffer[idxTmpBfrIdx] = _baseIndex;
						// the following line is commented to fix a bug.
						// when a new vertex is created on one of the 3 minimal edges,
						// it should not add the index to the index array.
						//_indexList.Add(_baseIndex);
						_baseIndex++;
						
					}
				}
			}
			else
			{
				if( directionCode == 0)
				{
					ushort vert_idx;
					// only reuse vertex from the preceding cell, and read it from the indextempbuffer.
					vert_idx = indexTempBuffer[idxTmpBfrIdx];
					
					_indexList.Add(vert_idx);
				}
			}
		}
	}
	// inline the following function
	void addMatIndex(byte cubeIndex)
	{
		byte[] matIdx = new byte[3];
		// calculate material info
		int low8_st = low8_array[0] & 15;
		int low8_end = (low8_array[0] >> 4) & 15;
		matIdx[0] = val[indexConvert[((cubeIndex & (1 << low8_st)) > 0)? low8_st:low8_end]].VType;
		
//		low8_st = low8_array[1] & 15;
//		low8_end = (low8_array[1] >> 4) & 15;
//		matIdx[1] = val[indexConvert[((cubeIndex & (1 << low8_st)) > 0)? low8_st:low8_end]].VType;
//		
//		low8_st = low8_array[2] & 15;
//		low8_end = (low8_array[2] >> 4) & 15;
//		matIdx[2] = val[indexConvert[((cubeIndex & (1 << low8_st)) > 0)? low8_st:low8_end]].VType;
		
//		uint mat0  = (uint)matIdx[0]*4 + 2; 
//		uint mat12 = (uint)matIdx[1]*256 + matIdx[2];
		uint mat0  = (uint)matIdx[0]*4 + 2; 
		uint mat12 = (uint)matIdx[0]*256 + matIdx[0];
		
		_norm2t.Add(new Vector2(mat0,mat12+0.0f));
//		_norm2t.Add(new Vector2(mat0,mat12+0.1f));
//		_norm2t.Add(new Vector2(mat0,mat12+0.2f));
		_norm01.Add(new Vector2(0.1f, 0.2f));
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