using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public partial class Block45Kernel{
	
	Mesh RebuildMesh()
	{
		if(verts == null)
			verts = new List<Vector3>();
		verts.Clear();
		if(uvs == null)
			uvs = new List<Vector2>();
		uvs.Clear();
		for(int z = 1; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++z){
			for(int y = 1; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++y){
				for(int x = 1; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++x){

					processBlock(x,y,z);
				}
			}
		}
		Mesh mesh = new Mesh();
		
		mesh.vertices = verts.ToArray();
		
		int[] indices = new int[verts.Count];
		Array.Copy(indicesConst, indices, verts.Count);
		
		mesh.SetTriangles(indices, 0);
		mesh.uv = uvs.ToArray();
		mesh.RecalculateNormals();
		
		return mesh;
	}
	
	List<Mesh> RebuildMeshByMaterial()
	{
		int before = Environment.TickCount;
		//for(int dumb = 0; dumb < 16 *32; dumb++)
		if(vertsByMaterial == null)
			vertsByMaterial = new List<List<Vector3>>();
		vertsByMaterial.Clear();
			
		if(uvsByMaterial == null)
			uvsByMaterial = new List<List<Vector2>>();
		uvsByMaterial.Clear();
		
		for(int i = 0; i < 256; i++)
		{
			vertsByMaterial.Add(new List<Vector3>());
			uvsByMaterial.Add(new List<Vector2>());
		}
		for(int z = 1; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++z){
			for(int y = 1; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++y){
				for(int x = 1; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE - 1; ++x){

					processBlockByMaterial(x,y,z);
				}
			}
		}
		List<Mesh> retbm = new List<Mesh>();
		for(int i = 0; i < 256; i++)
		{
			Mesh newMesh = new Mesh();
			retbm.Add(newMesh);
			if(vertsByMaterial[i].Count == 0)continue;
			
			//createPrimitive(vertsByMaterial[i].ToArray(), indices, uvsByMaterial[i].ToArray(), i);
			newMesh.vertices = vertsByMaterial[i].ToArray();
		
			int[] indices = new int[vertsByMaterial[i].Count];
			Array.Copy(indicesConst, indices, vertsByMaterial[i].Count);
			
			newMesh.SetTriangles(indices, 0);
			newMesh.uv = uvsByMaterial[i].ToArray();
			newMesh.RecalculateNormals();
		}
		
		MonoBehaviour.print("rebuild mesh bm took " + (Environment.TickCount - before) + " ms.");
		return retbm;
	}
	void processBlock(int x, int y, int z)
	{
		byte blockType = chunkData[OneIndexNoPrefix(x,y,z)];
		int primitiveType = blockType >> 2;
		int rotationId = blockType & 3;
		if(primitiveType == 0)return;
		
		Vector3 xyz = new Vector3(x-1,y-1,z-1);
		
		appendInteriorVerts(primitiveType,rotationId, xyz);
		
		byte[] neighbours = new byte[6];
		int neighbourPrimitiveType;
		int neighbourRotationId;
		
		neighbours[0] = chunkData[OneIndexNoPrefix(x-1,y,z)];
		neighbours[1] = chunkData[OneIndexNoPrefix(x,y,z-1)];
		neighbours[2] = chunkData[OneIndexNoPrefix(x+1,y,z)];
		neighbours[3] = chunkData[OneIndexNoPrefix(x,y,z+1)];
		
		neighbours[4] = chunkData[OneIndexNoPrefix(x,y-1,z)];
		neighbours[5] = chunkData[OneIndexNoPrefix(x,y+1,z)];
		for(int i = 0; i < 6; i++ ){
			neighbourPrimitiveType = neighbours[i] >> 2;
			neighbourRotationId = neighbours[i] & 3;
			//if( i == 3 )//&& chunkData[OneIndexNoPrefix(x,y,z) + 1]== 123)
			{
				//int sdfg = 0;
			}
			appendEdgeVerts(primitiveType, rotationId, neighbourPrimitiveType, neighbourRotationId, i, xyz);
		}
	}
	void processBlockByMaterial(int x, int y, int z)
	{
		byte materialType = chunkData[OneIndexNoPrefix(x,y,z) + 1];
		int mat_slot = materialType;
		
		byte blockType = chunkData[OneIndexNoPrefix(x,y,z)];
		int primitiveType = blockType >> 2;
		int rotationId = blockType & 3;
		if(primitiveType == 0)return;
		
		Vector3 xyz = new Vector3(x-1,y-1,z-1);
		
		appendInteriorVertsByMaterial(primitiveType,rotationId, xyz, mat_slot);
		
		byte[] neighbourPrimitiveTypes = new byte[6];
		byte[] neighbourMaterialTypes = new byte[6];
		int neighbourPrimitiveType;
		int neighbourRotationId;
		
		neighbourPrimitiveTypes[0] = chunkData[OneIndexNoPrefix(x-1,y,z)];
		neighbourPrimitiveTypes[1] = chunkData[OneIndexNoPrefix(x,y,z-1)];
		neighbourPrimitiveTypes[2] = chunkData[OneIndexNoPrefix(x+1,y,z)];
		neighbourPrimitiveTypes[3] = chunkData[OneIndexNoPrefix(x,y,z+1)];
		neighbourPrimitiveTypes[4] = chunkData[OneIndexNoPrefix(x,y-1,z)];
		neighbourPrimitiveTypes[5] = chunkData[OneIndexNoPrefix(x,y+1,z)];
		
		neighbourMaterialTypes[0] = chunkData[OneIndexNoPrefix(x-1,y,z) + 1];
		neighbourMaterialTypes[1] = chunkData[OneIndexNoPrefix(x,y,z-1) + 1];
		neighbourMaterialTypes[2] = chunkData[OneIndexNoPrefix(x+1,y,z) + 1];
		neighbourMaterialTypes[3] = chunkData[OneIndexNoPrefix(x,y,z+1) + 1];
		neighbourMaterialTypes[4] = chunkData[OneIndexNoPrefix(x,y-1,z) + 1];
		neighbourMaterialTypes[5] = chunkData[OneIndexNoPrefix(x,y+1,z) + 1];
		
		
		for(int i = 0; i < 6; i++ ){
			neighbourPrimitiveType = neighbourPrimitiveTypes[i] >> 2;
			neighbourRotationId = neighbourPrimitiveTypes[i] & 3;
			bool forceCreate = false;
			if(MaterialGroups[materialType] != MaterialGroups[neighbourMaterialTypes[i]])
				forceCreate = true;
			appendEdgeVertsByMaterial(primitiveType, rotationId, neighbourPrimitiveType, neighbourRotationId, i, xyz, mat_slot, forceCreate);
		}
	}
	
	
	void appendEdgeVerts(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz)
	{
		// determine if it is a perfect match.
		// special case for when faceId = 4,5
		if(directionId > 3){
			int neighbourFaceId = faceNeighbourFace[directionId];
			byte rotatedMask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId],rotationId);
			byte rotatedMaskNeighbour = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId],neighbourRotationId);
			if(!compareMaskTopBottom(rotatedMask, rotatedMaskNeighbour)){
			//if(rotatedMask != rotatedMaskNeighbour){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
				if(vecIdx < 0)return;
				//rotate vec in the index space, against the Y-Axis
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if(vecIdx < 0)return;
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
			}
		}
		else{
			int thisFaceId = rotateFaceId(directionId, rotationId);
			int neighbourFaceId = faceNeighbourFace[directionId];
			neighbourFaceId = rotateFaceId(neighbourFaceId, neighbourRotationId);
			
			if(!compareMask(faceTriMask[primitiveType * 6 + thisFaceId],
			faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId])){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6];
				if(vecIdx < 0)return;
				//rotate vec in the index space, against the Y-Axis
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 1];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 2];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 3];
				if(vecIdx < 0)return;
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 4];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 5];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				verts.Add(indexedCoords[vecIdx] + xyz);
				addUV(vecIdx, directionId);
				
	//			addVert(directionId * 6, rotationId, xyz);
	//			addVert(directionId * 6 + 1, rotationId, xyz);
	//			addVert(directionId * 6 + 2, rotationId, xyz);
	//			addVert(directionId * 6 + 3, rotationId, xyz);
	//			addVert(directionId * 6 + 4, rotationId, xyz);
	//			addVert(directionId * 6 + 5, rotationId, xyz);
			}
		}
		
	}
	void appendEdgeVertsByMaterial(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, bool forceCreate)
	{
		// determine if it is a perfect match.
		// special case for when faceId = 4,5
		
		if(directionId > 3){
			int neighbourFaceId = faceNeighbourFace[directionId];
			byte rotatedMask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId],rotationId);
			byte rotatedMaskNeighbour = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId],neighbourRotationId);
			if(!compareMaskTopBottom(rotatedMask, rotatedMaskNeighbour) || forceCreate){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
				if(vecIdx < 0)return;
				//rotate vec in the index space, against the Y-Axis
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if(vecIdx < 0)return;
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
			}
		}
		else{
			int thisFaceId = rotateFaceId(directionId, rotationId);
			int neighbourFaceId = faceNeighbourFace[directionId];
			neighbourFaceId = rotateFaceId(neighbourFaceId, neighbourRotationId);
			
			if(!compareMask(faceTriMask[primitiveType * 6 + thisFaceId],
			faceTriMask[neighbourPrimitiveType * 6 + neighbourFaceId])
				|| forceCreate){
				
				int vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6];
				if(vecIdx < 0)return;
				//rotate vec in the index space, against the Y-Axis
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 1];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 2];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				// the second triangle
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 3];
				if(vecIdx < 0)return;
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 4];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
				vecIdx = EdgeFaceVertTable[36 * primitiveType + thisFaceId * 6 + 5];
				vecIdx = rotateCornerVertId(vecIdx, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[vecIdx * 6 + directionId]);
				
	//			addVert(directionId * 6, rotationId, xyz);
	//			addVert(directionId * 6 + 1, rotationId, xyz);
	//			addVert(directionId * 6 + 2, rotationId, xyz);
	//			addVert(directionId * 6 + 3, rotationId, xyz);
	//			addVert(directionId * 6 + 4, rotationId, xyz);
	//			addVert(directionId * 6 + 5, rotationId, xyz);
			}
		}
		
	}
	
	// vertices belong to triangles that are inside the block. these triangles can never be cut to reduce faces.
	void appendInteriorVerts(int primitiveType, int rotationId, Vector3 xyz)
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
			verts.Add(indexedCoords[vecIdx] + xyz);
			addInteriorUV(i);
			
			vecIdx = InteriorFaceVertTable[i + 1];
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(indexedCoords[vecIdx] + xyz);
			addInteriorUV(i+1);
			
			vecIdx = InteriorFaceVertTable[i + 2];
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(indexedCoords[vecIdx] + xyz);
			addInteriorUV(i+2);
		}
		
	}
	
	void appendInteriorVertsByMaterial(int primitiveType, int rotationId, Vector3 xyz, int output_slot)
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
			vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
			uvsByMaterial[output_slot].Add(InteriorVertUV[i]);
			
			vecIdx = InteriorFaceVertTable[i + 1];
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
			uvsByMaterial[output_slot].Add(InteriorVertUV[i+1]);
			
			vecIdx = InteriorFaceVertTable[i + 2];
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[vecIdx] + xyz);
			uvsByMaterial[output_slot].Add(InteriorVertUV[i+2]);
		}
		
	}
		
	void addUV(int vertId, int directionId)
	{
		uvs.Add(EdgeVertUV[vertId * 6 + directionId]);
	}
	void addInteriorUV(int index)
	{
		uvs.Add(InteriorVertUV[index]);
	}

}
