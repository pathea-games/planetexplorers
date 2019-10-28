using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Extendable Block format
/// </summary>
/* Normal Block
 		 * Block0
 		 *  Byte0: (primitiveType<<2)|rotationId
 		 *  Byte1: matType
 		 */ 
/* Extendable Block
 		 * Block0
		 *  Byte0: (63<<2)|rotationId 					;rotation occupies 2 bits
		 *  Byte1: ((length-2)<<2)|extentionId 			;extentionId occupies 2 bits
		 * Block1:
		 *  Byte0: 0x80|(primitiveType<<2)|rotationId	; primitiveType<31 in extendable block in order to differ from root block
		 *  Byte1: (matType<<2)|extentionId 			;Here extentionId for facility of find the first block data
		 * Dumb block: now same as Block1
		 *  Byte0: 0x80|(primitiveType<<2)|rotationId
		 *  Byte1: (matType<<2)|extentionId
		 */
/* RotationId
 		 * 0
 		 * 1
 		 * 2
 		 * 3
 		 */
/* ExtentionId
		 * the low 3 bit indicates one of the 6 directions that it is extending:
		 * 0 -> +x
		 * 1 -> +z
		 * 2 -> +y
		 */

public partial class Block45Kernel{
	byte[] chunkData;
	public static int OneIndexNoPrefix(int x, int y, int z)
	{
		return (x + y * Block45Constants.VOXEL_ARRAY_AXIS_SIZE + z * Block45Constants.VOXEL_ARRAY_AXIS_SQUARED) * B45Block.Block45Size;
	}
	public static int OneIndex4Flags(int x, int y, int z)
	{
		return x + (y << Block45Constants._shift) + (z << (Block45Constants._shift * 2));
	}
	public static int OneIndex(int x, int y, int z)
	{
		return (x + Block45Constants._numVoxelsPrefix + (y + Block45Constants._numVoxelsPrefix) * Block45Constants.VOXEL_ARRAY_AXIS_SIZE + (z + Block45Constants._numVoxelsPrefix) * Block45Constants.VOXEL_ARRAY_AXIS_SQUARED) * B45Block.Block45Size;
	}
	public static int[] indicesConst;
	public Block45Kernel(){
		
		indicesConst = new int[64998];
		for(int i = 0; i < indicesConst.Length; i++){
			indicesConst[i] = i;
		}
		int tmp = Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis;
		for(int i = 0; i < tmp; i++){
			BlockExtendFlagsResetBuffer[i] = 0;
		}
	}
	public void setInputChunkData(byte[] _input)
	{
		chunkData = _input;
	}
	public List<Vector3> verts = new List<Vector3>();
	public List<Vector2> uvs = new List<Vector2>();
	public int matCnt = 0;
	public int[] materialMap;
	public List<List<int>> subMeshIndices = new List<List<int>>();
	int _baseIndex;
	List<List<Vector3>> vertsByMaterial;	// Now not used
	List<List<Vector2>> uvsByMaterial;		// Now not used

	public int[] getMaterialMap()
	{
		return materialMap;
	}

	public void Rebuild()
	{
		verts.Clear();
		uvs.Clear();
		subMeshIndices.Clear();
		for(int i = 0; i < Block45Constants.MaxMaterialCount; i++)
		{
			subMeshIndices.Add(new List<int>());
		}
		matCnt = 0;
		_baseIndex = 0;
		Array.Copy(BlockExtendFlagsResetBuffer, BlockExtendFlags, Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis);
		for(int z = 1; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++z){
			for(int y = 1; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++y){
				for(int x = 1; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++x){
					
					processBlockSM(x,y,z);
				}
			}
		}
		// Mat mapping
		int matIdx = 0;
		materialMap = new int[Block45Constants.MaxMaterialCount];
		for(int i = 0; i < Block45Constants.MaxMaterialCount; i++)
		{
			if(subMeshIndices[matIdx].Count <= 0)
			{
				subMeshIndices.RemoveAt(matIdx);
			}
			else
			{
				materialMap[matIdx++] = i;
				matCnt++;
			}
		}
	}

	public Mesh RebuildMeshSM()
	{
		//int before = Environment.TickCount;
		Rebuild();

		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.subMeshCount = matCnt;

		for(int i = 0; i < matCnt; i++)
		{
			mesh.SetTriangles(subMeshIndices[i].ToArray(), i);
		}
		
		mesh.RecalculateNormals();
		//MonoBehaviour.print("rebuild mesh took " + (Environment.TickCount - before) + " ms.");
		TangentSolver(mesh);
		return mesh;
	}
	
	public static void TangentSolver(Mesh theMesh)
    {
        int vertexCount = theMesh.vertexCount;
        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
        Vector2[] texcoords = theMesh.uv;
        int[] triangles = theMesh.triangles;
        int triangleCount = triangles.Length / 3;
        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];
        int tri = 0;
        for (int i = 0; i < (triangleCount); i++)
        {
            int i1 = triangles[tri];
            int i2 = triangles[tri + 1];
            int i3 = triangles[tri + 2];
 
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
 
            Vector2 w1 = texcoords[i1];
            Vector2 w2 = texcoords[i2];
            Vector2 w3 = texcoords[i3];
 
            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;
 
            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;
 
            float r = 1.0f / (s1 * t2 - s2 * t1);
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
 
            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;
 
            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
 
            tri += 3;
        }
 
        for (int i = 0; i < (vertexCount); i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];
 
            // Gram-Schmidt orthogonalize
            Vector3.OrthoNormalize(ref n, ref t);
 
            tangents[i].x = t.x;
            tangents[i].y = t.y;
            tangents[i].z = t.z;
 
            // Calculate handedness
            tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0) ? -1.0f : 1.0f;
        }
        theMesh.tangents = tangents;
    }
	

	public static int[] _2BitToExDir = new int[]{
		1,0,0,
		0,0,1,
		0,1,0
	};
	void processExtendedBlockSM(int x, int y, int z, byte byte0, byte byte1)
	{
		/*  Byte0: (63<<2)|rotationId 					;rotation occupies 2 bits
		 *  Byte1: ((length-2)<<2)|extentionId 			;extentionId occupies 2 bits
		 *  Byte0: 0x80|(primitiveType<<2)|rotationId	; primitiveType<31 in extendable block in order to differ from root block
		 *  Byte1: (matType<<2)|extentionId 			;Here extentionId for facility of find the first block data
		 */
		int rotationId = byte0 & 3;
		int low2bits = byte1 & 3;
		int EDx = _2BitToExDir[low2bits * 3];
		int EDy = _2BitToExDir[low2bits * 3 + 1];
		int EDz = _2BitToExDir[low2bits * 3 + 2];
		int extensionLength = (1+((int)byte1>>2));
		Vector3 ExtensionVec = new Vector3(EDx * extensionLength, EDy * extensionLength, EDz * extensionLength);

		byte byte2 = chunkData[OneIndexNoPrefix(x+EDx,y+EDy,z+EDz)];
		byte byte3 = chunkData[OneIndexNoPrefix(x+EDx,y+EDy,z+EDz) + 1];
		int primitiveType = (int)((byte2&0x7f)>>2);
		int materialType = (int)(byte3>>2);
		
		Vector3 xyz = new Vector3(x-1,y-1,z-1);
		appendInteriorVertsExSM(primitiveType, rotationId, xyz, materialType, low2bits, ExtensionVec);
		
		for(int i = 0; i < 6; i++ )
		{			
			appendEdgeVertsExSM(primitiveType, rotationId, 0, 0, i, xyz, materialType, low2bits, ExtensionVec);
		}
	}
	byte[] BlockExtendFlags = new byte[Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis];
	byte[] BlockExtendFlagsResetBuffer = new byte[Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis];
	byte[] t_neighbourPrimitiveTypes = new byte[6];
	byte[] t_neighbourMaterialTypes = new byte[6];
	void processBlockSM(int x, int y, int z)
	{
		byte materialType = chunkData[OneIndexNoPrefix(x,y,z) + 1];
		byte blockType = chunkData[OneIndexNoPrefix(x,y,z)];
		int primitiveType = blockType >> 2;
		if(primitiveType == 0 || BlockExtendFlags[OneIndex4Flags(x-1,y-1,z-1)] != 0)return;
		
		// check for extendable types
		if(blockType >= 0x80)
		{
			if(primitiveType == 63)
			{
				processExtendedBlockSM(x,y,z, blockType, materialType);
			}
			return;
		}

		if (materialType >= MaterialGroups.Length)
			return;

		int mat_slot = materialType;
		int rotationId = blockType & 3;
		Vector3 xyz = new Vector3(x-1,y-1,z-1);		
		addInteriorTrianglesSM_c(primitiveType,rotationId, xyz, mat_slot);
		
		int neighbourPrimitiveType;
		int neighbourRotationId;
		int idxData0 = OneIndexNoPrefix(x-1,y,z);
		int idxData1 = OneIndexNoPrefix(x,y,z-1);
		int idxData2 = OneIndexNoPrefix(x+1,y,z);
		int idxData3 = OneIndexNoPrefix(x,y,z+1);
		int idxData4 = OneIndexNoPrefix(x,y-1,z);
		int idxData5 = OneIndexNoPrefix(x,y+1,z);
		
		t_neighbourPrimitiveTypes[0] = chunkData[idxData0];
		t_neighbourPrimitiveTypes[1] = chunkData[idxData1];
		t_neighbourPrimitiveTypes[2] = chunkData[idxData2];
		t_neighbourPrimitiveTypes[3] = chunkData[idxData3];
		t_neighbourPrimitiveTypes[4] = chunkData[idxData4];
		t_neighbourPrimitiveTypes[5] = chunkData[idxData5];
		
		t_neighbourMaterialTypes[0] = chunkData[idxData0 + 1];
		t_neighbourMaterialTypes[1] = chunkData[idxData1 + 1];
		t_neighbourMaterialTypes[2] = chunkData[idxData2 + 1];
		t_neighbourMaterialTypes[3] = chunkData[idxData3 + 1];
		t_neighbourMaterialTypes[4] = chunkData[idxData4 + 1];
		t_neighbourMaterialTypes[5] = chunkData[idxData5 + 1];

		for(int i = 0; i < 6; i++ ){
			neighbourPrimitiveType = t_neighbourPrimitiveTypes[i] >> 2;
			neighbourRotationId = t_neighbourPrimitiveTypes[i] & 3;
			bool forceCreate = false;
			if(t_neighbourPrimitiveTypes[i] >= 0x80 || 
			   t_neighbourMaterialTypes[i] >= MaterialGroups.Length || 
			   MaterialGroups[materialType] != MaterialGroups[t_neighbourMaterialTypes[i]])
			{
					forceCreate = true;
			}
			addEdgeTrianglesSM_c(primitiveType, rotationId, neighbourPrimitiveType, neighbourRotationId, i, xyz, mat_slot, forceCreate);
		}
	}
	//int[] CornerCoordExtensionFlags = new int[]{
	//	0,0,0,
	//	0,1,0,
	//	1,1,0,
	//	1,0,0,
		
	//	0,0,1,
	//	0,1,1,
	//	1,1,1,
	//	1,0,1,
	//};
	Vector3 ExtendindexedCoords(int vecIdx, int ExDirCode, Vector3 ExtensionVec)
	{
		Vector3 ret = indexedCoords[vecIdx];
		//return ret + CornerCoordExtensionFlags[vecIdx * 3 + ExDirCode] * ExtensionVec;
		//ret * (ExtensionVec + Vector3.one);
		ret.x *= ExtensionVec.x+1.0f;
		ret.y *= ExtensionVec.y+1.0f;
		ret.z *= ExtensionVec.z+1.0f;
		return ret;
	}	
	void appendInteriorVertsExSM(int primitiveType, int rotationId, Vector3 xyz, int output_slot, int ExDirCode, Vector3 ExtensionVec)
	{
		int stIdx = -1;
		int idx = primitiveType * 2 + 1;
		if(idx < BlockTypeTable.Length){
			stIdx = BlockTypeTable[idx];
		}

		// check if this primitive type actually has interior triangles.
		if(stIdx < 0)return;
		
		for(int i = stIdx;i < stIdx + 30;i+=3)
		{
			int vecIdx = InteriorFaceVertTable[i];
			if(vecIdx < 0)break;
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
			uvs.Add(InteriorVertUV[i]);
			subMeshIndices[output_slot].Add(_baseIndex++);
			
			vecIdx = InteriorFaceVertTable[i + 1];
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
			uvs.Add(InteriorVertUV[i+1]);
			subMeshIndices[output_slot].Add(_baseIndex++);
			
			vecIdx = InteriorFaceVertTable[i + 2];
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
			uvs.Add(InteriorVertUV[i+2]);
			subMeshIndices[output_slot].Add(_baseIndex++);
		}
	}

	void addInteriorVert(int vecIdx, int rotationId, Vector3 xyz)
	{
		if(vecIdx > 8)
			verts.Add(rotate90(indexedCoords[vecIdx], rotationId) + xyz);
		else
		{
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(indexedCoords[vecIdx] + xyz);
		}
	}
	void addUV(int uvi)
	{
		if(uvi >= InteriorVertUV.Length)
		{
			uvs.Add(Vector2.zero);
		}
		else
			uvs.Add(InteriorVertUV[uvi]);
	}
	void addUVByIndex(int uvi)
	{
		if(uvi >= InteriorVertUVByIndex.Length)
		{
			uvs.Add(Vector2.zero);
		}
		else
		{
			uvs.Add(InteriorVertUVByIndex[uvi]);
		}
	}
	void addUVByIndexXZ(int uvi)
	{
		if(uvi >= VertUVByIndexXZ.Length)
		{
			uvs.Add(Vector2.zero);
		}
		else if(uvi < 8)
		{
			uvs.Add(VertUVByIndexXZ[uvi]);
		}
	}
	int[][] SpecialUVIndexTable = new int[][]{
		null,
		null,
		new int[]{1,2,0,1,3,2,},// 2
		new int[]{1,2,0,1,3,2,},// 3
		new int[]{0,2,1,},// 4
		new int[]{0,2,1,},// 5
		null,
		null,
		new int[]{0,2,1,},// 8
		new int[]{0,2,1,},// 9
		new int[]{1,2,0,1,3,2,},// 10
		null,
		new int[]{
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,

			-1,0,-1,
			-1,0,-1,
			-1,0,-1,
			-1,0,-1,
			-1,0,-1,
			-1,0,-1,
		},//12
		new int[]{
			-1,3,-1,
			-1,3,-1,
			-1,3,-1,
			-1,3,-1,
			-1,3,-1,
			-1,3,-1,
			
			-1,-1,2,
			-1,-1,2,
			-1,-1,2,
			-1,-1,2,
			-1,-1,2,
			-1,-1,2,
		},//13
		new int[]{
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			-1,1,-1,
			
			-1,-1,0,
			-1,-1,0,
			-1,-1,0,
			-1,-1,0,
			-1,-1,0,
			-1,-1,0,
		},//14
		null,
		null,
		new int[]{
			2,-1,-1,
			2,-1,-1,		
			-1,0,-1,
			-1,-1,0,
		},//17
		new int[]{
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,0,-1,		
		},//18
		new int[]{
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			2,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
			0,-1,-1,
		},//19
		new int[]{
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			3,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
			1,-1,-1,
		},
		null,
		null,
	};
	Vector2[] SpecialUVs = new Vector2[]{
		new Vector2(0,0),
		new Vector2(0,1),
		new Vector2(1,0),
		new Vector2(1,1),
	};
	void addInteriorVertSM_c(int primitiveType, int i, int stIdx, int vecIdx, int rotationId, Vector3 xyz, int output_slot)
	{
		// add vertex UV.
		int idx = i - stIdx;
		int[] spUVIdx = SpecialUVIndexTable [primitiveType];
		if (spUVIdx == null || spUVIdx.Length <= idx || spUVIdx [idx] < 0) {
			addUVByIndex(vecIdx);
		} else {
			uvs.Add(SpecialUVs[spUVIdx [idx]]);
		}

		// add the interior vertex position.
		if(vecIdx > 8)
			verts.Add(rotate90(indexedCoords[vecIdx], rotationId) + xyz);
		else
		{
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);

			verts.Add(indexedCoords[vecIdx] + xyz);
		}

		// add vertex index.
		subMeshIndices[output_slot].Add(_baseIndex++);
	}
	void addInteriorTrianglesSM_c(int primitiveType, int rotationId, Vector3 xyz, int output_slot)
	{
		int stIdx = -1;
		int idx = primitiveType * 2 + 1;
		if(idx < BlockTypeTable.Length){
			stIdx = BlockTypeTable[idx];
		}
		
		// check if this primitive type actually has interior triangles.
		if(stIdx < 0)return;
		
		for(int i = stIdx;i < stIdx + 256;i+=3)
		{
			int vecIdx = InteriorFaceVertTable[i];
			if(vecIdx < 0)break;
			addInteriorVertSM_c(primitiveType, i, stIdx, vecIdx,rotationId, xyz, output_slot);
			
			vecIdx = InteriorFaceVertTable[i + 1];
			addInteriorVertSM_c(primitiveType, i+1, stIdx, vecIdx,rotationId, xyz, output_slot);
			
			vecIdx = InteriorFaceVertTable[i + 2];
			addInteriorVertSM_c(primitiveType, i+2, stIdx, vecIdx,rotationId, xyz, output_slot);
		}
	}

	// append edge vertices for extended block, submesh approach,
	void appendEdgeVertsExSM(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, int ExDirCode, Vector3 ExtensionVec)
	{	
		try{
		if(directionId > 3){
			int neighbourFaceId = faceNeighbourFace[directionId];
			byte rotatedMask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId],rotationId);
			byte rotatedMaskNeighbour = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId],neighbourRotationId);
			if(!compareMaskTopBottom(rotatedMask, rotatedMaskNeighbour)){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
				if(vecIdx < 0)return;
				//rotate vec in the index space, against the Y-Axis
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if(vecIdx < 0)return;
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
			}
		}
		else{
			int thisFaceId = rotateFaceId(directionId, rotationId);
			int neighbourFaceId = faceNeighbourFace[directionId];
			neighbourFaceId = rotateFaceId(neighbourFaceId, neighbourRotationId);
			
			if(!compareMask(faceTriMask[primitiveType * 6 + thisFaceId],
			faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId])
				){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6];
				if(vecIdx < 0)return;
				//rotate vec in the index space, against the Y-Axis
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 1];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 2];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 3];
				if(vecIdx < 0)return;
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 4];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 5];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(ExtendindexedCoords(vecIdx, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);

			}
		}
		}catch
		{
			//int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, int ExDirCode, Vector3 ExtensionVec
		}
	}
	void addEdgeVert_c(int vecIdx, int rotationId, Vector3 xyz, int output_slot)
	{
		addUVByIndex(vecIdx);
		//rotate vec in the index space, against the Y-Axis
		vecIdx = rotateCornerVertId(vecIdx, rotationId);
		verts.Add(indexedCoords[vecIdx] + xyz);
		//uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
		subMeshIndices[output_slot].Add(_baseIndex++);
	}
	void addEdgeVertXZ_c(int vecIdx, int rotationId, Vector3 xyz, int output_slot)
	{
		addUVByIndexXZ(vecIdx);
		//rotate vec in the index space, against the Y-Axis
		vecIdx = rotateCornerVertId(vecIdx, rotationId);
		verts.Add(indexedCoords[vecIdx] + xyz);
		//uvs.Add(EdgeVertUV[vecIdx * 6 + directionId]);
		subMeshIndices[output_slot].Add(_baseIndex++);
	}
	void addEdgeTrianglesSM_c(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, bool forceCreate)
	{
		// determine if it is a perfect match.
		// special case for when faceId = 4,5		
		bool bCreate = forceCreate;
		if(directionId > 3){
			if(!bCreate)
			{
				int neighbourFaceId = faceNeighbourFace[directionId];
				byte rotatedMask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId],rotationId);
				byte rotatedMaskNeighbour = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId],neighbourRotationId);
				bCreate = !compareMaskTopBottom(rotatedMask, rotatedMaskNeighbour);
			}
			if(bCreate){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
				if(vecIdx < 0)return;

				addEdgeVertXZ_c(vecIdx,rotationId, xyz, output_slot);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				addEdgeVertXZ_c(vecIdx,rotationId, xyz, output_slot);

				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				addEdgeVertXZ_c(vecIdx,rotationId, xyz, output_slot);

				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if(vecIdx < 0)return;
				addEdgeVertXZ_c(vecIdx,rotationId, xyz, output_slot);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
				addEdgeVertXZ_c(vecIdx,rotationId, xyz, output_slot);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
				addEdgeVertXZ_c(vecIdx,rotationId, xyz, output_slot);
			}
		}
		else{
			int thisFaceId = rotateFaceId(directionId, rotationId);
			int neighbourFaceId = faceNeighbourFace[directionId];
			neighbourFaceId = rotateFaceId(neighbourFaceId, neighbourRotationId);
			if(!bCreate)
			{
				bCreate = !compareMask(faceTriMask[primitiveType * 6 + thisFaceId],
				                       faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId]);
			}
			if(bCreate){

				int vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6];
				if(vecIdx < 0)return;
				addEdgeVert_c(vecIdx,rotationId, xyz, output_slot);

				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 1];
				addEdgeVert_c(vecIdx,rotationId, xyz, output_slot);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 2];
				addEdgeVert_c(vecIdx,rotationId, xyz, output_slot);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 3];
				if(vecIdx < 0)return;
				addEdgeVert_c(vecIdx,rotationId, xyz, output_slot);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 4];
				addEdgeVert_c(vecIdx,rotationId, xyz, output_slot);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 5];
				addEdgeVert_c(vecIdx,rotationId, xyz, output_slot);
			}
		}
	}


	public void UVGenerator()
	{
//		// type 12
//		uvgen (9,13, new Vector2(1,0), new Vector2(0,0), new Vector2(1,0));
//		uvgen (14,18, new Vector2(1,1), new Vector2(0,1), new Vector2(1,1));
//		
//		// type 13
//		uvgen (19,23, new Vector2(1,0), new Vector2(0,0), new Vector2(1,0));
//		uvgen (24,28, new Vector2(1,1), new Vector2(0,1), new Vector2(1,1));
//		
//		// type 14
//		uvgen (29,33, new Vector2(1,0), new Vector2(0,0), new Vector2(1,0));
//		uvgen (34,38, new Vector2(1,1), new Vector2(0,1), new Vector2(1,1));

		
//		// type 15
//		uvgen (39,43, new Vector2(0,0), new Vector2(0,1), new Vector2(0,0));
//		uvgen (44,48, new Vector2(1,0), new Vector2(1,1), new Vector2(1,0));
//		
//		// type 16
//		uvgen (49,53, new Vector2(0,0), new Vector2(0,1), new Vector2(0,0));
//		uvgen (54,58, new Vector2(1,0), new Vector2(1,1), new Vector2(1,0));
//		
//		// type 17
//		uvgen (59,63, new Vector2(0,0), new Vector2(0,1), new Vector2(0,0));
//		uvgen (64,68, new Vector2(1,0), new Vector2(1,1), new Vector2(1,0));
//		
//		// type 18
//		uvgen (69,73, new Vector2(0,0), new Vector2(0,1), new Vector2(0,0));
//		uvgen (74,78, new Vector2(1,0), new Vector2(1,1), new Vector2(1,0));
		
		// type 19
		uvgenforfullcyl (79,91, new Vector2(0,0), new Vector2(0,1), new Vector2(0,0), 6);
		uvgenforfullcyl (92,104, new Vector2(1,0), new Vector2(1,1), new Vector2(1,0), 6);
		
		// type 20
		uvgenforfullcyl (105,117, new Vector2(0,0), new Vector2(0,1), new Vector2(0,0), 6);
		uvgenforfullcyl (118,130, new Vector2(1,0), new Vector2(1,1), new Vector2(1,0), 6);
	}
	void uvgen(int st, int end, Vector2 vecSt, Vector2 vecMid, Vector2 vecEnd, int stepnum = 3)
	{
		String str = "// " + st + " - " + end + "\n";
		Vector2 diff;
		Vector2 result;
		diff = (vecMid - vecSt)/ stepnum;
		for(int i = 1; i < stepnum; i++){
			result = vecSt + diff * i;
			str += "new Vector2(" + result.x + ", " + result.y + "),\n";
			
		}
		diff = (vecEnd - vecMid)/ stepnum;
		for(int i = 0; i < stepnum; i++){
			result = vecMid + diff * i;
			str += "new Vector2(" + result.x + ", " + result.y + "),\n";
			
		}
		MonoBehaviour.print(str);
		
	}
	void uvgenforfullcyl(int st, int end, Vector2 vecSt, Vector2 vecMid, Vector2 vecEnd, int stepnum = 3)
	{
		String str = "// " + st + " - " + end + "\n";
		Vector2 diff;
		Vector2 result;
		diff = (vecMid - vecSt)/ stepnum;
		for(int i = 0; i <= stepnum; i++){
			result = vecSt + diff * i;
			str += "new Vector2(" + result.x + ", " + result.y + "),\n";
			
		}
		diff = (vecEnd - vecMid)/ stepnum;
		for(int i = 1; i <= stepnum; i++){
			result = vecMid + diff * i;
			str += "new Vector2(" + result.x + ", " + result.y + "),\n";
			
		}
		MonoBehaviour.print(str);
		
	}
	#region rotation related auxilliary helper functions.
	Vector3 rotate90(Vector3 vec, int rot)
	{
		if(rot == 0)
			return vec;
		else if(rot == 1)
		{
			return new Vector3(vec.z, vec.y, 1- vec.x);
		}
		else if(rot == 2)
		{
			return new Vector3(1-vec.x, vec.y, 1-vec.z);
		}
		return new Vector3(1-vec.z, vec.y, vec.x);
	}

	bool compareMask(byte mask0, byte mask1)
	{
		// switch the bit 0,4 of mask 1
		bool bit4_set = (mask1 & 0x4) > 0 ? true : false;
		
		if((mask1 & 0x1) > 0)
			mask1 |= 0x4;
		else
			mask1 &= 0xB;
		
		if(bit4_set)
			mask1 |= 0x1;
		else
			mask1 &= 0xE;
		return (mask0 & mask1) == mask0;
	}
	
	bool compareMaskTopBottom(byte mask0, byte mask1)
	{
		// switch the bit 1,3 of mask 1
		bool bit4_set = (mask1 & 0x8) > 0 ? true : false;
		
		if((mask1 & 0x2) > 0)
			mask1 |= 0x8;
		else
			mask1 &= 0x7;
		if(bit4_set)
			mask1 |= 0x2;
		else
			mask1 &= 0xD;
		return (mask0 & mask1) == mask0;
	}

	int rotateCornerVertId(int faceId, int rotationId)
	{
		int ret = (faceId+rotationId)%4;
		return (faceId>3)?ret+4:ret;
	}
	int rotateInteriorVertId(int faceId, int rotationId)
	{
		if(faceId == 8)
		{
			return faceId;
		}

		int ret = (faceId+rotationId)%4;
		return (faceId>3)?ret+4:ret;
	}
	int rotateFaceId(int faceId, int rotationId)
	{
		return (faceId+rotationId)%4;
	}
	byte rotateTopBottomFaceMask(byte faceId, int rotationId)
	{
		return (byte)((byte)(faceId << rotationId) >> 4);
	}
	#endregion
	// this is independent of the block types.
	byte[] MaterialGroups = new byte[]{
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 1, 0,
	};
	
	// the first column is reserved for now.
	// the second column is the offset of the first element in InteriorFaceVertTable
	int[] BlockTypeTable = new int[]{
		0, -1,
		0, -1,
		0, 0,
		0, 9,
		0, 18,
		0, 24, // type 5
		
		0, 30,
		// new shapes added nov, 18
		
		0, 45, // type 7
		
		0, 60, // type 8
		
		0, 66, // type 9
		
		0, 72, // type 10
		
		0, 81, // type 11
		
		0, 96, // type 12
		
		0, 171, // type 13
		
		0, 246, // type 14
		
		0, 321, // type 15
		
		0, 390, // type 16
		
		0, 459, // type 17
		
		0, 528, // type 18
		
		// type 19, "full cylinder, vertical"
		0, 597,
		
		// type 20, "full cylinder, horizontal"
		0, 738,

		// type 21, "quad + prism"
		0, 879,

		// type 22, "quad + prism inverted"
		0, 888,
	};
	// indices in indexedCoords, telling how to wind triangles for interior faces, terminated by -1.
	short[] InteriorFaceVertTable = new short[]{
		// type 2
		4,2,3,
		4,5,2,
		-1,-1,-1,
		// type 3
		7,1,3,
		7,5,1,
		-1,-1,-1,
		// type 4
		4,1,3,
		-1,-1,-1,
		//type 5
		7,5,2,
		-1,-1,-1,
		// type 6
		0,8,3,
		3,8,2,
		2,8,1,
		1,8,0,
		-1,-1,-1,
		
		// type 7, horizontal inverse of type 6, "diamond on the ceiling"
		4,7,8,
		7,6,8,
		6,5,8,
		5,4,8,
		-1,-1,-1,
		
		// type 8, horizontal inverse of type 5
		6,1,3,
		-1,-1,-1,
		
		// type 9, horizontal inverse of type 4, "ceiling corner"
		7,5,0,
		-1,-1,-1,
		
		// type 10, horizontal inverse of type 2, "reverse ramp"
		7,1,0,
		7,6,1,
		-1,-1,-1,
		
		// type 11, "diamond clinging to a wall"
		4,5,8,
		5,1,8,
		1,0,8,
		4,8,0,
		-1,-1,-1,
		
		// type 12, "quarter cylinder"		
		// bottom
		9,0,3,
		10,0,9,
		11,0,10,
		12,0,11,
		13,0,12,
		1,0,13,		
		// upper
		7,4,14,
		14,4,15,
		15,4,16,
		16,4,17,
		17,4,18,
		18,4,5,		
		// middle		
		7,9,3,
		7,14,9,
		14,10,9,
		14,15,10,
		15,11,10,
		15,16,11,
		16,12,11,
		16,17,12,
		17,13,12,
		17,18,13,
		18,1,13,
		18,5,1,		
		-1,-1,-1,
		
		// type 13, "curved ramp", "quarter cylinder"		
		// left
		3,0,19,
		19,0,20,
		20,0,21,
		21,0,22,
		22,0,23,
		23,0,4,		
		// right
		2,24,1,
		24,25,1,
		25,26,1,
		26,27,1,
		27,28,1,
		28,5,1,		
		// middle		
		3,24,2,
		3,19,24,		
		19,25,24,
		19,20,25,		
		20,26,25,
		20,21,26,		
		21,27,26,
		21,22,27,		
		22,28,27,
		22,23,28,		
		23,5,28,
		23,4,5,				
		-1,-1,-1,
		
		
		// type 14, "inverted curved ramp", "quarter cylinder"		
		// left
		0,4,29,
		29,4,30,
		30,4,31,
		31,4,32,
		32,4,33,
		33,4,7,		
		// right
		1,34,5,
		34,35,5,
		35,36,5,
		36,37,5,
		37,38,5,
		38,6,5,		
		// middle		
		0,34,1,
		0,29,34,		
		29,35,34,
		29,30,35,		
		30,36,35,
		30,31,36,		
		31,37,36,
		31,32,37,		
		32,38,37,
		32,33,38,		
		33,6,38,
		33,7,6,				
		-1,-1,-1,
		
		// type 15, "half cylinder, clinging to the ceiling"		
		// z=0
		7,39,40,
		7,40,41,
		7,41,42,
		7,42,43,
		7,43,4,
		// z=1
		6,45,44,
		6,46,45,
		6,47,46,
		6,48,47,
		6,5,48,		
		// mid
		7,6,44,
		7,44,39,		
		39,44,45,
		39,45,40,		
		40,45,46,
		40,46,41,		
		41,46,47,
		41,47,42,		
		42,47,48,
		42,48,43,		
		43,48,5,
		43,5,4,		
		-1,-1,-1,		
		
		// type 16, "half cylinder, on the floor"		
		// z=0
		3,50,49,
		3,51,50,
		3,52,51,
		3,53,52,
		3,0,53,
		// z=1
		2,58,1,
		2,57,58,
		2,56,57,
		2,55,56,
		2,54,55,
		//mid		
		3,54,2,
		3,49,54,		
		49,55,54,
		49,50,55,		
		50,56,55,
		50,51,56,		
		51,57,56,
		51,52,57,		
		52,58,57,
		52,53,58,		
		53,1,58,
		53,0,1,		
		-1,-1,-1,
		
		// type 17, "half cylinder, vertical"	
		// mid
		4,63,0,
		4,68,63,		
		64,1,59,
		64,5,1,
		68,62,63,
		68,67,62,		
		65,59,60,
		65,64,59,
		67,61,62,
		67,66,61,		
		66,60,61,
		66,65,60,
		// y=0
		0,63,62,
		0,62,61,
		0,61,60,
		0,60,59,
		0,59,1,
		// y=1
		4,67,68,
		4,66,67,
		4,65,66,
		4,64,65,
		4,5,64,
		-1,-1,-1,

		// type 18, "half cylinder, horizontal"		
		// z = 0
		4,69,70,
		4,70,71,
		4,71,72,
		4,72,73,
		4,73,0,
		// z = 1
		5,75,74,
		5,76,75,
		5,77,76,
		5,78,77,
		5,1,78,		
		// mid
		4,5,74,
		4,74,69,		
		69,74,75,
		69,75,70,		
		70,75,76,
		70,76,71,		
		71,76,77,
		71,77,72,		
		72,77,78,
		72,78,73,		
		73,78,1,
		73,1,0,		
		-1,-1,-1,
		
		// type 19, "full cylinder, vertical"		
		// y=0
		79,80,81,
		79,81,82,
		79,82,83,
		79,83,84,
		79,84,85,
		79,85,86,
		79,86,87,
		79,87,88,
		79,88,89,
		79,89,90,
		79,90,91,
		// y=1
		92,104,103,
		92,103,102,
		92,102,101,
		92,101,100,
		92,100,99,
		92,99,98,
		92,98,97,
		92,97,96,
		92,96,95,
		92,95,94,
		92,94,93,		
		// mid
		92,80,79,
		92,93,80,		
		93,81,80,
		93,94,81,		
		94,82,81,
		94,95,82,		
		95,	83,	82,
		95,	96,	83,		
		96,	84,	83,
		96,	97,	84,				
		97,	85,	84,
		97,	98,	85,		
		98,	86,	85,
		98,	99,	86,		
		99,		87,		86,
		99,		100,	87,		
		100,	88,		87,
		100,	101,	88,		
		101,	89,		88,
		101,	102,	89,		
		102,	90,		89,
		102,	103,	90,				
		103,	91,		90,
		103,	104,	91,		
		-1,-1,-1,

		// type 20, "full cylinder, horizontal"		
		// z=0
		105,	107,	106,
		105,	108,	107,
		105,	109,	108,
		105,	110,	109,
		105,	111,	110,
		105,	112,	111,
		105,	113,	112,
		105,	114,	113,
		105,	115,	114,
		105,	116,	115,
		105,	117,	116,
		// z=1
		118,	129,	130,
		118,	128,	129,
		118,	127,	128,
		118,	126,	127,
		118,	125,	126,
		118,	124,	125,
		118,	123,	124,
		118,	122,	123,
		118,	121,	122,
		118,	120,	121,
		118,	119,	120,
		// mid
		118,	105,	106,
		118,	106,	119,				
		119,	106,	107,
		119,	107,	120,				
		120,	107,	108,
		120,	108,	121,				
		121,	108,	109,
		121,	109,	122,
		122,	109,	110,
		122,	110,	123,
		123,	110,	111,
		123,	111,	124,				
		124,	111,	112,
		124,	112,	125,				
		125,	112,	113,
		125,	113,	126,				
		126,	113,	114,
		126,	114,	127,				
		127,	114,	115,
		127,	115,	128,				
		128,	115,	116,
		128,	116,	129,				
		129,	116,	117,
		129,	117,	130,
		-1,-1,-1,

		// type 21, "quad + prism"
		4,2,3,
		4,1,2,
		-1,-1,-1,

		// type 22, "quad + prism, inverted"
		7,6,0,
		6,5,0,
		-1,-1,-1,
	};
	Vector2[] VertUVByIndexXZ = new Vector2[]{
		new Vector2(0,0),		// 0
		new Vector2(1,0),		// 1
		new Vector2(1,1),		// 2
		new Vector2(0,1),		// 3
		
		new Vector2(0,0),		// 4
		new Vector2(1,0),		// 5
		new Vector2(1,1),		// 6
		new Vector2(0,1),		// 7
	
	};
	Vector2[] InteriorVertUVByIndex = new Vector2[]{
		new Vector2(0,0),		// 0
		new Vector2(1,0),		// 1
		new Vector2(0,0),		// 2
		new Vector2(1,0),		// 3
		
		new Vector2(0,1),		// 4
		new Vector2(1,1),		// 5
		new Vector2(0,1),		// 6
		new Vector2(1,1),		// 7
		
		new Vector2(0.5f,0.5f),	// 8
		
		// type 12
		
		// 9 - 13
		new Vector2(0.6666666f, 0),
		new Vector2(0.3333333f, 0),
		new Vector2(0, 0),
		new Vector2(0.3333333f, 0),
		new Vector2(0.6666667f, 0),
		
		// 14 - 18
		new Vector2(0.6666666f, 1),
		new Vector2(0.3333333f, 1),
		new Vector2(0, 1),
		new Vector2(0.3333333f, 1),
		new Vector2(0.6666667f, 1),
		
		// type 13
		
		// 19 - 23
		new Vector2(0.6666666f, 0),
		new Vector2(0.3333333f, 0),
		new Vector2(0, 0),
		new Vector2(0.3333333f, 0),
		new Vector2(0.6666667f, 0),
		
		// 24 - 28
		new Vector2(0.6666666f, 1),
		new Vector2(0.3333333f, 1),
		new Vector2(0, 1),
		new Vector2(0.3333333f, 1),
		new Vector2(0.6666667f, 1),
		
		// type 14
		
		// 29 - 33
		new Vector2(0.6666666f, 0),
		new Vector2(0.3333333f, 0),
		new Vector2(0, 0),
		new Vector2(0.3333333f, 0),
		new Vector2(0.6666667f, 0),
		
		// 34 - 38
		new Vector2(0.6666666f, 1),
		new Vector2(0.3333333f, 1),
		new Vector2(0, 1),
		new Vector2(0.3333333f, 1),
		new Vector2(0.6666667f, 1),
		

		// type 15
		
		// 39 - 43
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.6666667f),
		new Vector2(0, 1),
		new Vector2(0, 0.6666666f),
		new Vector2(0, 0.3333333f),
		
		
		
		// 44 - 48
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.6666667f),
		new Vector2(1, 1),
		new Vector2(1, 0.6666666f),
		new Vector2(1, 0.3333333f),
		
		// type 16
		
		// 49 - 53
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.6666667f),
		new Vector2(0, 1),
		new Vector2(0, 0.6666666f),
		new Vector2(0, 0.3333333f),
		
		// 54 - 58
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.6666667f),
		new Vector2(1, 1),
		new Vector2(1, 0.6666666f),
		new Vector2(1, 0.3333333f),
		
		// type 17
		
		// 59 - 63
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.6666667f),
		new Vector2(0, 1),
		new Vector2(0, 0.6666666f),
		new Vector2(0, 0.3333333f),
		
		// 64 - 68
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.6666667f),
		new Vector2(1, 1),
		new Vector2(1, 0.6666666f),
		new Vector2(1, 0.3333333f),
		
		// type 18
		
		// 69 - 73
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.6666667f),
		new Vector2(0, 1),
		new Vector2(0, 0.6666666f),
		new Vector2(0, 0.3333333f),
		
		
		// 74 - 78
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.6666667f),
		new Vector2(1, 1),
		new Vector2(1, 0.6666666f),
		new Vector2(1, 0.3333333f),

		// type 19 "full cylinder"
		
		// 79 - 91
		new Vector2(0, 0),
		new Vector2(0, 0.1666667f),
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.5f),
		new Vector2(0, 0.6666667f),
		new Vector2(0, 0.8333334f),
		new Vector2(0, 1),
		new Vector2(0, 0.8333333f),
		new Vector2(0, 0.6666666f),
		new Vector2(0, 0.5f),
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.1666666f),
		new Vector2(0, 0),
		
		// 92 - 104
		new Vector2(1, 0),
		new Vector2(1, 0.1666667f),
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.5f),
		new Vector2(1, 0.6666667f),
		new Vector2(1, 0.8333334f),
		new Vector2(1, 1),
		new Vector2(1, 0.8333333f),
		new Vector2(1, 0.6666666f),
		new Vector2(1, 0.5f),
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.1666666f),
		new Vector2(1, 0),
		
		// type 20
		
		// 105 - 117
		new Vector2(0, 0),
		new Vector2(0, 0.1666667f),
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.5f),
		new Vector2(0, 0.6666667f),
		new Vector2(0, 0.8333334f),
		new Vector2(0, 1),
		new Vector2(0, 0.8333333f),
		new Vector2(0, 0.6666666f),
		new Vector2(0, 0.5f),
		new Vector2(0, 0.3333333f),
		new Vector2(0, 0.1666666f),
		new Vector2(0, 0),
		
		// 118 - 130
		new Vector2(1, 0),
		new Vector2(1, 0.1666667f),
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.5f),
		new Vector2(1, 0.6666667f),
		new Vector2(1, 0.8333334f),
		new Vector2(1, 1),
		new Vector2(1, 0.8333333f),
		new Vector2(1, 0.6666666f),
		new Vector2(1, 0.5f),
		new Vector2(1, 0.3333333f),
		new Vector2(1, 0.1666666f),
		new Vector2(1, 0),

		// type 21, "quad + prism"
		new Vector2(0,1),
		new Vector2(1,0),
		new Vector2(0,0),

		new Vector2(1,1),
		new Vector2(1,0),
		new Vector2(0,0),


		// 131 - 137


		// type 21, "quad + prism inverted"

		new Vector2(1,0),
		new Vector2(1,1),
		new Vector2(0,0),

		new Vector2(1,0),
		new Vector2(1,1),
		new Vector2(0,0),


		
	};
	Vector2[] InteriorVertUV = new Vector2[]{
		// type 2
		new Vector2(0,1),new Vector2(1,0),new Vector2(0,0),
		new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		// type 3
		new Vector2(0,1),new Vector2(1,0),new Vector2(0,0),
		new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		// type 4
		new Vector2(0,1),new Vector2(1,0),new Vector2(0,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 5 "cant describe"
		new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 6 "pyramid"
		new Vector2(0,1),new Vector2(0.5f,0.5f),new Vector2(0,0),
		new Vector2(0,0),new Vector2(0.5f,0.5f),new Vector2(1,0),
		new Vector2(1,0),new Vector2(0.5f,0.5f),new Vector2(1,1),
		new Vector2(1,1),new Vector2(0.5f,0.5f),new Vector2(0,1),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// new ***
		
		// type 7 "inverted pyramid"
		new Vector2(0,1),new Vector2(0.5f,0.5f),new Vector2(0,0),
		new Vector2(0,0),new Vector2(0.5f,0.5f),new Vector2(1,0),
		new Vector2(1,0),new Vector2(0.5f,0.5f),new Vector2(1,1),
		new Vector2(1,1),new Vector2(0.5f,0.5f),new Vector2(0,1),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 8 "inverted type 5"
		new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 9 "ceiling corner"
		new Vector2(1,1),new Vector2(0,1),new Vector2(1,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 10
		new Vector2(0,1),new Vector2(1,0),new Vector2(0,0),
		new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 11 "pyramid on a wall"
		new Vector2(0,1),new Vector2(0.5f,0.5f),new Vector2(0,0),
		new Vector2(0,0),new Vector2(0.5f,0.5f),new Vector2(1,0),
		new Vector2(1,0),new Vector2(0.5f,0.5f),new Vector2(1,1),
		new Vector2(1,1),new Vector2(0.5f,0.5f),new Vector2(0,1),
		Vector2.zero,Vector2.zero,Vector2.zero,
		
		// type 12
		
		
		// mid
		
		new Vector2(0,1),new Vector2(0.16667f,0),new Vector2(0,0),
		new Vector2(0,1),new Vector2(0.16667f,1),new Vector2(0.16667f,0),
		
	
		
	};
	Vector2[] EdgeVertUV = new Vector2[]{
		// 0					1					2						3					4				5
		new Vector2(1,0),	new Vector2(0,0),	Vector2.zero,		Vector2.zero,		new Vector2(0,0),	Vector2.zero,
		new Vector2(0,0),	Vector2.zero,		Vector2.zero,		new Vector2(1,0),	new Vector2(1,0),	Vector2.zero,
		
		Vector2.zero,		Vector2.zero,		new Vector2(1,0),	new Vector2(0,0),	new Vector2(1,1),	Vector2.zero,
		Vector2.zero,		new Vector2(1,0),	new Vector2(0,0),	Vector2.zero,		new Vector2(0,1),	Vector2.zero,
		new Vector2(1,1),	new Vector2(0,1),	Vector2.zero,		Vector2.zero,		Vector2.zero,		new Vector2(0,1),
		new Vector2(0,1),	Vector2.zero,		Vector2.zero,		new Vector2(1,1),	Vector2.zero,		new Vector2(1,1),	
		Vector2.zero,		Vector2.zero,		new Vector2(1,1),	new Vector2(0,1),	Vector2.zero,		new Vector2(1,0),
		Vector2.zero,		new Vector2(1,1),	new Vector2(0,1),	Vector2.zero,		Vector2.zero,		new Vector2(0,0),
		
		new Vector2(0.5f,0.5f),	new Vector2(0.5f,0.5f),	new Vector2(0.5f,0.5f),	new Vector2(0.5f,0.5f),		Vector2.zero,		Vector2.zero,
	};
	// indices in indexedCoords, telling how to wind triangles on the faces, terminated by -1.
	short[] EdgeFaceVertTable = new short[]{
		
		// type 0 (ok)
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		
		// type 1 (OK)
		5,0,1,
		5,4,0,
		
		4,3,0,
		4,7,3,
		
		7,2,3,
		7,6,2,
		
		6,1,2,
		6,5,1,
		
		3,1,0,
		3,2,1,
		
		4,6,7,
		4,5,6,
		
		// type 2 (OK)
		5,0,1,
		5,4,0,
		
		4,3,0,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		5,1,2,
		-1,-1,-1,
		
		3,1,0,
		3,2,1,
		
		-1,-1,-1,
		-1,-1,-1,
				
		// type 3 (OK)
		5,0,1,
		5,4,0,
		
		4,3,0,
		4,7,3,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		3,1,0,
		-1,-1,-1,
		
		4,5,7,
		-1,-1,-1,
		
		// type 4
		4,0,1,
		-1,-1,-1,
		
		4,3,0,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		3,1,0,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		// type 5
		5,0,1,
		5,4,0,
		
		0,4,3,
		4,7,3,
		
		7,2,3,
		-1,-1,-1,
		
		5,1,2,
		-1,-1,-1,
		
		0,3,2,
		0,2,1,
		
		4,5,7,
		-1,-1,-1,
		
		
		// type 6 (OK)
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		0,3,2,
		0,2,1,
		-1,-1,-1,
		-1,-1,-1,
		
		// type 7, "diamond on the ceiling"
		
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		4,5,6,
		4,6,7,
		
		
		// type 8
		
		5,0,1,
		5,4,0,
		
		0,4,3,
		4,7,3,
		
		7,6,3,
		-1,-1,-1,
		
		5,1,6,
		-1,-1,-1,
		
		0,3,1,
		-1,-1,-1,
		
		4,5,7,
		7,5,6,
		
		// type 9, "ceiling corner"
		4,0,5,
		-1,-1,-1,
		
		4,7,0,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		4,5,7,
		-1,-1,-1,
		
		
		// type 10, "reverse ramp"
		5,0,1,
		5,4,0,
		
		4,7,0,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		6,5,1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		7,4,5,
		7,5,6,
		
		// type 11, "diamond clinging to a wall"
		5,0,1,
		5,4,0,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		// type 12, "quarter cylinder"
		5,4,0,
		5,0,1,
		
		4,3,0,
		4,7,3,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		-1,-1,-1,
		-1,-1,-1,
		
		// type 13, "curved ramp", "quarter cylinder"
		
		5,0,1,
		5,4,0,		
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		3,1,0,
		3,2,1,
		-1,-1,-1,
		-1,-1,-1,
		
		// type 14, "inverted curved ramp", "quarter cylinder"
		5,0,1,
		5,4,0,	
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		-1,-1,-1,
		-1,-1,-1,
		4,6,7,
		4,5,6,
		
		// type 15, "half cylinder, clinging to the ceiling"
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		-1,-1,-1,
		-1,-1,-1,
		4,6,7,
		4,5,6,
		
		
		// type 16, "half cylinder, on the floor"
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		3,1,0,
		3,2,1,
		-1,-1,-1,
		-1,-1,-1,

		
		// type 17, "half cylinder, vertical"
		5,0,1,
		5,4,0,	
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		
		// type 18, "half cylinder, horizontal"
		5,0,1,
		5,4,0,	
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		
		// type 19, "full cylinder, vertical"
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		
		
		// type 20, "full cylinder, horizontal"
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,		
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,
		-1,-1,-1,

		// type 21, "quad, "
		4,0,1,
		-1,-1,-1,

		4,3,0,
		-1,-1,-1,

		-1,-1,-1,
		-1,-1,-1,		

		-1,-1,-1,
		-1,-1,-1,

		0,2,1,
		0,3,2,

		-1,-1,-1,
		-1,-1,-1,
		
		
		// type 22, "quad, inverted"
		5,4,0,
		-1,-1,-1,

		4,7,0,
		-1,-1,-1,

		-1,-1,-1,
		-1,-1,-1,		

		-1,-1,-1,
		-1,-1,-1,

		-1,-1,-1,
		-1,-1,-1,

		4,6,7,
		4,5,6,
		
	};
	
	byte[] faceTriMask = new byte[]{
		// type 0 "empty block"
		0x0,
		0x0,
		0x0,
		0x0,
		0x0,
		0x0,
		
		// type 1 (OK) "cube"
		0xF,
		0xF,
		0xF,
		0xF,
		0xFF,
		0xFF,
		
		// type 2 "ramp"
		0xF,
		0xC,
		0x0,
		0x9,
		0xFF,
		0x0,
		
		// type 3 (OK)
		0xF,
		0xF,
		0x0,
		0x0,
		0xCC,
		0xCC,
		
		// type 4 (OK) "floor corner"
		0x9,
		0xC,
		0x0,
		0x0,
		0xCC,
		0x0,
		
		
		// type 5 (OK)
		0xF,
		0xF,
		0xC,
		0x9,		
		0xFF,
		0x66,
		
		// type 6 (OK) "pyramid"
		0x0,
		0x0,
		0x0,
		0x0,
		0xFF,
		0x0,
		
		// type 7, "diamond on the ceiling"
		0x0,
		0x0,
		0x0,
		0x0,
		0x0,
		0xFF,		
		
		// need to recal from this point on.
		
		// type 8
		0xF,
		0xF,
		0xC,
		0x9,		
		0x66,
		0xFF,
		
		// type 9, "ceiling corner"
		0x3,
		0x6,
		0x0,
		0x0,
		0x0,
		0x66,
		
		// type 10, "reverse ramp"
		0xF,
		0x6,
		0x0,
		0x3,
		0x0,
		0xFF,
		
		// type 11, "diamond clinging to a wall"
		0xF,
		0x0,
		0x0,
		0x0,
		0x0,
		0x0,
		
		// type 12
		0xF,
		0xF,
		0x0,
		0x0,
		0x00,
		0x00,
		
		// type 13, "curved ramp"
		0xF,
		0x0,
		0x0,
		0x0,
		0xFF,
		0x00,
		
		// type 14, "inverted curved ramp"
		0xF,
		0x0,
		0x0,
		0x0,
		0x00,
		0xFF,
		
		// type 15, "half cylinder, clinging to the ceiling"
		0x0,
		0x0,
		0x0,
		0x0,
		0x00,
		0xFF,
		
		
		// type 16, "half cylinder, on the floor"
		0x0,
		0x0,
		0x0,
		0x0,
		0xFF,
		0x00,
		
		// type 17, "half cylinder, vertical"
		0xF,
		0x0,
		0x0,
		0x0,
		0x00,
		0x00,

		
		// type 18, "half cylinder, horizontal"
		0xF,
		0x0,
		0x0,
		0x0,
		0x00,
		0x00,
		
		// type 19, "full cylinder, vertical"
		0x0,
		0x0,
		0x0,
		0x0,
		0x00,
		0x00,
		
		// type 20, "full cylinder, horizontal"
		0x0,
		0x0,
		0x0,
		0x0,
		0x00,
		0x00,

		// type 21, "quad, "
		0x9,
		0xC,
		0x0,
		0x0,
		0xFF,
		0x00,
		
		// type 22, "quad, "
		0x3,
		0x6,
		0x0,
		0x0,
		0x00,
		0xFF,
		
	};
	
	public static Vector3[] indexedCoords = new Vector3[]{
		new Vector3(0,0,0),
		new Vector3(0,0,1),
		new Vector3(1,0,1),
		new Vector3(1,0,0),
		
		new Vector3(0,1,0),
		new Vector3(0,1,1),
		new Vector3(1,1,1),
		new Vector3(1,1,0),
		// tip of the pyramid, the center of the cube
		new Vector3(0.5f,0.5f,0.5f),
		
		// for type 12
		// 9 - 13
		new Vector3(0.96593f,	0,	0.25882f),
		new Vector3(0.86603f,	0,	0.5f),
		new Vector3(0.70711f,	0,	0.70711f),
		new Vector3(0.5f,	0,	0.86603f),
		new Vector3(0.25882f,	0,	0.96593f),
		
		// 14 - 18
		new Vector3(0.96593f,	1,	0.25882f),
		new Vector3(0.86603f,	1,	0.5f),
		new Vector3(0.70711f,	1,	0.70711f),
		new Vector3(0.5f,		1,	0.86603f),
		new Vector3(0.25882f,	1,	0.96593f),
		
		// for type 13, "curved ramp"
		// 19 - 23
		
		new Vector3(0.96593f,	0.25882f,		0),
		new Vector3(0.86603f,	0.5f,			0),
		new Vector3(0.70711f,	0.70711f,		0),
		new Vector3(0.5f,		0.86603f,		0),
		new Vector3(0.25882f,	0.96593f,		0),
		
		
		// 24 - 28
		new Vector3(0.96593f,	0.25882f,		1),
		new Vector3(0.86603f,	0.5f,			1),
		new Vector3(0.70711f,	0.70711f,		1),		
		new Vector3(0.5f,		0.86603f,		1),
		new Vector3(0.25882f,	0.96593f,		1),
		
		
		// for type 14, "inverted curved ramp"
		// 29 - 33
		new Vector3(0.25882f,	1-0.96593f,		0),
		new Vector3(0.5f,		1-0.86603f,		0),
		new Vector3(0.70711f,	1-0.70711f,		0),
		new Vector3(0.86603f,	0.5f,			0),
		new Vector3(0.96593f,	1-0.25882f,		0),		
		
		// 34 - 38
		new Vector3(0.25882f,	1-0.96593f,		1),
		new Vector3(0.5f,		1-0.86603f,		1),
		new Vector3(0.70711f,	1-0.70711f,		1),		
		new Vector3(0.86603f,	0.5f,			1),
		new Vector3(0.96593f,	1-0.25882f,		1),
		
		// type 15, "half cylinder, clinging to the ceiling"
		
		// 39-43
		
		new Vector3(0.93301f,	0.75000f,	0),
		new Vector3(0.75000f,	0.56699f,	0),
		new Vector3(0.50000f,	0.50000f,	0),
		new Vector3(0.25000f,	0.56699f,	0),
		new Vector3(0.06699f,	0.75000f,	0),
		
		// 44-48
		new Vector3(0.93301f,	0.75000f,	1),
		new Vector3(0.75000f,	0.56699f,	1),
		new Vector3(0.50000f,	0.50000f,	1),
		new Vector3(0.25000f,	0.56699f,	1),
		new Vector3(0.06699f,	0.75000f,	1),

		
		// type 16, "half cylinder, on the floor"
		// 49-53
		new Vector3(0.93301f,	0.25000f,	0),
		new Vector3(0.75000f,	0.43301f,	0),
		new Vector3(0.50000f,	0.50000f,	0),
		new Vector3(0.25000f,	0.43301f,	0),
		new Vector3(0.06699f,	0.25000f,	0),
		// 54-58
		new Vector3(0.93301f,	0.25000f,	1),
		new Vector3(0.75000f,	0.43301f,	1),
		new Vector3(0.50000f,	0.50000f,	1),
		new Vector3(0.25000f,	0.43301f,	1),
		new Vector3(0.06699f,	0.25000f,	1),
		
		// type 17, "half cylinder, vertical"
		// 59-63
		new Vector3(0.25000f,	0,	0.93301f),
		new Vector3(0.43301f,	0,	0.75000f),
		new Vector3(0.50000f,	0,	0.50000f),
		new Vector3(0.43301f,	0,	0.25000f),
		new Vector3(0.25000f,	0,	0.06699f),
		// 64-68
		new Vector3(0.25000f,	1,	0.93301f),
		new Vector3(0.43301f,	1,	0.75000f),
		new Vector3(0.50000f,	1,	0.50000f),
		new Vector3(0.43301f,	1,	0.25000f),
		new Vector3(0.25000f,	1,	0.06699f),
		
		
		// type 18, "half cylinder, horizontal"
		
		// 69-73
		new Vector3(0.25000f,	0.93301f,	0),
		new Vector3(0.43301f,	0.75000f,	0),
		new Vector3(0.50000f,	0.50000f,	0),
		new Vector3(0.43301f,	0.25000f,	0),
		new Vector3(0.25000f,	0.06699f,	0),
		// 74-78
		new Vector3(0.25000f,	0.93301f,	1),
		new Vector3(0.43301f,	0.75000f,	1),
		new Vector3(0.50000f,	0.50000f,	1),
		new Vector3(0.43301f,	0.25000f,	1),
		new Vector3(0.25000f,	0.06699f,	1),
		
		// type 19, "full cylinder, vertical"
		
		// 79-91
		new Vector3(0.93301f,	0,	0.75000f),
		new Vector3(0.75000f,	0,	0.93301f),
		new Vector3(0.50000f,	0,	1.00000f),
		new Vector3(0.25000f,	0,	0.93301f),
		new Vector3(0.06699f,	0,	0.75000f),
		new Vector3(0.00000f,	0,	0.50000f),
		new Vector3(0.06699f,	0,	0.25000f),
		new Vector3(0.25000f,	0,	0.06699f),
		new Vector3(0.50000f,	0,	0.00000f),
		new Vector3(0.75000f,	0,	0.06699f),
		new Vector3(0.93301f,	0,	0.25000f),
		new Vector3(1.00000f,	0,	0.50000f),
		new Vector3(0.93301f,	0,	0.75000f),
		
		// 92-104
		new Vector3(0.93301f,	1,	0.75000f),
		new Vector3(0.75000f,	1,	0.93301f),
		new Vector3(0.50000f,	1,	1.00000f),
		new Vector3(0.25000f,	1,	0.93301f),
		new Vector3(0.06699f,	1,	0.75000f),
		new Vector3(0.00000f,	1,	0.50000f),
		new Vector3(0.06699f,	1,	0.25000f),
		new Vector3(0.25000f,	1,	0.06699f),
		new Vector3(0.50000f,	1,	0.00000f),
		new Vector3(0.75000f,	1,	0.06699f),
		new Vector3(0.93301f,	1,	0.25000f),
		new Vector3(1.00000f,	1,	0.50000f),
		new Vector3(0.93301f,	1,	0.75000f),
		
		// type 20, "full cylinder, horizontal"
		
		// 105-117
		new Vector3(0.93301f,	0.75000f,	0),
		new Vector3(0.75000f,	0.93301f,	0),
		new Vector3(0.50000f,	1.00000f,	0),
		new Vector3(0.25000f,	0.93301f,	0),
		new Vector3(0.06699f,	0.75000f,	0),
		new Vector3(0.00000f,	0.50000f,	0),
		new Vector3(0.06699f,	0.25000f,	0),
		new Vector3(0.25000f,	0.06699f,	0),
		new Vector3(0.50000f,	0.00000f,	0),
		new Vector3(0.75000f,	0.06699f,	0),
		new Vector3(0.93301f,	0.25000f,	0),
		new Vector3(1.00000f,	0.50000f,	0),
		new Vector3(0.93301f,	0.75000f,	0),
		// 118-130
		new Vector3(0.93301f,	0.75000f,	1),
		new Vector3(0.75000f,	0.93301f,	1),
		new Vector3(0.50000f,	1.00000f,	1),
		new Vector3(0.25000f,	0.93301f,	1),
		new Vector3(0.06699f,	0.75000f,	1),
		new Vector3(0.00000f,	0.50000f,	1),
		new Vector3(0.06699f,	0.25000f,	1),
		new Vector3(0.25000f,	0.06699f,	1),
		new Vector3(0.50000f,	0.00000f,	1),
		new Vector3(0.75000f,	0.06699f,	1),
		new Vector3(0.93301f,	0.25000f,	1),
		new Vector3(1.00000f,	0.50000f,	1),
		new Vector3(0.93301f,	0.75000f,	1),
		
	};
	short[] faceNeighbourFace = new short[]{
		2,3,0,1,5,4
	};
	 
}
public struct B45Block{
	// the high 6 bit indicates the primitive type, the low 2 bit indicates the rotationId
	public byte blockType;
	
	public byte materialType;
	public const int Block45Size = 2;
	public B45Block(byte b, byte m)
	{
		blockType = b;
		materialType = m;
	}
	public B45Block(byte b)
	{
		blockType = b;
		materialType = 0;
	}
	public int RotId{  get{return (int)(blockType&3);	} }
	public int PrimId{ get{return (int)(blockType>>2);	} }
	public bool IsExtendable()
	{
		return blockType >= 0x80;
	}
	public bool IsExtendableRoot()
	{
		return (blockType>>2) == 63;
	}
	internal void Update(B45Block block)
	{
		blockType = block.blockType;
		materialType = block.materialType;
	}

	// Static Method
	public static byte MakeBlockType(int primitiveType, int rotation)
	{
		return (byte)((primitiveType << 2) | rotation);
	}	
	public static void MakeExtendableBlock(int primitiveType, int rotation, int extendDir, int length, int materialType, out B45Block block0, out B45Block block1)
	{
		/*Root block
		 *  Byte0: (63<<2)|rotationId 					;rotation occupies 2 bits
		 *  Byte1: ((length-2)<<2)|extentionId 			;extentionId occupies 2 bits
		 *Ext block  
		 *  Byte0: 0x80|(primitiveType<<2)|rotationId	; primitiveType<31 in extendable block in order to differ from root block
		 *  Byte1: (matType<<2)|extentionId 			;Here extentionId for facility of find the first block data
		 */
		block0 = new B45Block((byte)((63<<2) | rotation), 
		                      (byte)(((length-2)<<2) | extendDir));
		block1 = new B45Block((byte)(0x80 | (primitiveType << 2) | rotation), 
		                      (byte)((materialType<<2) | extendDir));
	}
	public static void RepositionBlocks(List<IntVector3> posLst, List<B45Block> blockLst, int rot, Vector3 originalPos)
	{
		int n = posLst.Count;
		// Reposition position
		for(int i = 0; i < n; i++)
		{
			// rote if need
			Vector3 lpos = posLst[i].ToVector3();
			Vector3 offset = Quaternion.Euler(0, 90* rot, 0) * new Vector3(lpos.x + 0.5f, lpos.y + 0.5f, lpos.z + 0.5f);
			posLst[i] = new IntVector3(Mathf.FloorToInt( (originalPos.x ) * Block45Constants._scaleInverted) + Mathf.FloorToInt(offset.x),
			                           Mathf.FloorToInt( (originalPos.y ) * Block45Constants._scaleInverted) + Mathf.FloorToInt(offset.y),
			                           Mathf.FloorToInt( (originalPos.z ) * Block45Constants._scaleInverted) + Mathf.FloorToInt(offset.z) );
		}
		if((rot&3) == 0)	return;

		// Proceed all but ext_roots those need swapping
		List<int> idxSwap = new List<int>();
		for(int i = 0; i < n; i++)
		{
			B45Block ori = blockLst[i];
			int rotId = ((ori.blockType & 3) + rot) & 3;

			if(ori.blockType >= 128)
			{
				int prim = ori.blockType>>2;
				int extId = ori.materialType & 3;
				if(extId != B45Block.EBY)
				{
					if((rot&1) != 0)
					{
						extId = extId == B45Block.EBX ? B45Block.EBZ : B45Block.EBX;
					}
					// only root need swapping, if all other blocks filled with ext block
					if(prim == 63 && (rot==1||rot==2)) //clockwise
					{
						// need swap
						idxSwap.Add(i);
					}
				}
				blockLst[i] = new B45Block((byte)((prim<<2)|rotId), (byte)((ori.materialType&0xfc) | extId));
			}
			else
			{		
				// Normal block
				blockLst[i] = new B45Block((byte)((ori.blockType&0xfc) | rotId), ori.materialType);
			}
		}
		// Proceed swap
		foreach(int idx in idxSwap)
		{
			IntVector3 pos = posLst[idx];
			B45Block ori = blockLst[idx];
			int e = ori.materialType&3;
			int EDx = Block45Kernel._2BitToExDir[e * 3];
			int EDy = Block45Kernel._2BitToExDir[e * 3 + 1];
			int EDz = Block45Kernel._2BitToExDir[e * 3 + 2];
			int len = (ori.materialType>>2)+1;
			int endx = pos.x-EDx*len;
			int endy = pos.y-EDy*len;
			int endz = pos.z-EDz*len;
			int idxEnd = posLst.FindIndex(it => it.x==endx && it.y==endy && it.z==endz);
			if(idxEnd < 0)
			{
				Debug.LogError("[BLOCK]Failed to rotate blocks because len_info not found.");
				continue;
			}
			posLst[idx] = posLst[idxEnd];
			posLst[idxEnd] = pos;
		}
	}
	public const int EBX = 0;
	public const int EBY = 2;
	public const int EBZ = 1;			
};