using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
public class ChunkMeshMerger {
	List<MeshStruct> reorganizedMeshes;
	int curMeshIdx;
	int destBufferIdx;
	public ChunkMeshMerger()
	{
	}
	public struct MeshStruct{
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector2[] uv;
		public int[] triangles;
	};
	public void Init(){
		curMeshIdx = 0;
		destBufferIdx = 0;
		reorganizedMeshes = new List<MeshStruct>();
		addNewMesh();
	}
	public List<MeshStruct> GetReorganizedMeshes(){
		return reorganizedMeshes;
	}
	void addNewMesh(){
		MeshStruct newMesh = new MeshStruct();
		
		newMesh.vertices = new Vector3[SurfExtractorsMan.c_vertsCntMax];
		newMesh.uv = new Vector2[SurfExtractorsMan.c_vertsCntMax];
		newMesh.triangles = new int[SurfExtractorsMan.c_vertsCntMax];
		newMesh.normals = new Vector3[SurfExtractorsMan.c_vertsCntMax];
		Array.Copy(Block45Kernel.indicesConst, 0, newMesh.triangles, 0, SurfExtractorsMan.c_vertsCntMax);
		
		reorganizedMeshes.Add(newMesh);
	}
	public void Merge(List<IntVector4> chunkPosList, List<B45ChunkData> chunks, uint numChunks, List<Mesh> meshList)
	{
//		int leftover = 0;
//		int leftat = 0;
//		int n;
//		for(n = 0; n < chunks.Count; n++)
//		{
//			if(chunks[n].IsChunkInReq || !chunkPosList[n].Equals(chunks[n].ChunkPosLod ))
//				continue;
//			
//			if(chunks[n].IsHollow){	
//				//chunks[n].FinHollowUpdate();
//				continue;		}
//			//
//			if(meshList[n].vertices.Length == 0)continue;
//			MeshStruct inputMesh;
//			inputMesh.vertices = meshList[n].vertices;
//			inputMesh.uv = meshList[n].uv;
//			inputMesh.triangles = meshList[n].triangles;
//			inputMesh.normals = meshList[n].normals;
//			if(leftover > 0){
//				addNewMesh();
//				curMeshIdx++;
//				
//				MeshStruct prevMesh;
//				prevMesh.vertices = meshList[n-1].vertices;
//				prevMesh.uv = meshList[n-1].uv;
//				prevMesh.triangles = meshList[n-1].triangles;
//				prevMesh.normals = meshList[n-1].normals;
//				
//				
//				copy (prevMesh, leftat, reorganizedMeshes[curMeshIdx], destBufferIdx, leftover, chunks[n]._chunkPos);
//				destBufferIdx = leftover;
//			}
//			int numVertToCopy;
//			if(destBufferIdx + inputMesh.vertices.Length > VertsLimit)
//			{
//				numVertToCopy = VertsLimit - destBufferIdx;
//				
//				copy (inputMesh, 0, reorganizedMeshes[curMeshIdx], destBufferIdx, numVertToCopy, chunks[n]._chunkPos);
//				
//				leftover = destBufferIdx + inputMesh.vertices.Length - VertsLimit;
//				leftat = inputMesh.vertices.Length - leftover;
//				destBufferIdx += numVertToCopy;
//			}
//			else
//			{
//				numVertToCopy = inputMesh.vertices.Length;
//				copy (inputMesh, 0, reorganizedMeshes[curMeshIdx], destBufferIdx, inputMesh.vertices.Length, chunks[n]._chunkPos);
//				destBufferIdx += inputMesh.vertices.Length;
//			}
//			
//		}
		// fix the last mesh.
		
	}
	public void truncateLastMesh()
	{
		MeshStruct lastMesh = reorganizedMeshes[curMeshIdx];
		Vector3[] truncatedVB = new Vector3[destBufferIdx];
		Vector2[] truncatedUV = new Vector2[destBufferIdx];
		Vector3[] truncatedNRM = new Vector3[destBufferIdx];
		int[] truncatedIB = new int[destBufferIdx];
		
		
		Array.Copy(lastMesh.vertices, 0, truncatedVB, 0, destBufferIdx);
		Array.Copy(lastMesh.uv, 0, truncatedUV, 0, destBufferIdx);
		Array.Copy(lastMesh.triangles, 0, truncatedIB, 0, destBufferIdx);
		
		Array.Copy(lastMesh.normals, 0, truncatedNRM, 0, destBufferIdx);
		
		lastMesh.vertices = truncatedVB;
		lastMesh.uv = truncatedUV;
		lastMesh.triangles = truncatedIB;
		lastMesh.normals = truncatedNRM;
		
		reorganizedMeshes[curMeshIdx] = lastMesh;
	}
	void copy(MeshStruct src, int srcOfs, MeshStruct dest, int destOfs, int length, IntVector3 chunkpos){
		Vector3 cp = new Vector3(chunkpos.x << Block45Constants._shift, chunkpos.y << Block45Constants._shift, chunkpos.z << Block45Constants._shift);
		for(int i = 0; i < length; i++){
			dest.vertices[destOfs + i] = src.vertices[srcOfs + i] + cp;
		}
		//Array.Copy(src.vertices,	srcOfs, dest.vertices,	destOfs, length);
		Array.Copy(src.uv,				srcOfs, dest.uv,			destOfs, length);
		Array.Copy(src.normals,			srcOfs, dest.normals,		destOfs, length);
	}
}
