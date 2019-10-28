using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public class Block45Lab : MonoBehaviour {
	GameObject primitiveGO;
	List<GameObject> chunkGOList = new List<GameObject>();
	public GameObject[] primitiveTypeList;
	public Material[] blockMaterials;
	byte[] chunkData = new byte[Block45Constants.VOXEL_ARRAY_LENGTH_VT];
	//byte[] chunkData2 = new byte[Block45Constants.VOXEL_ARRAY_LENGTH_VT];
	B45LODCompute lodCompute;
	public void Start()
	{
		
		cpuBlock45.Inst.init();
//		testextendedblock();
//		getResult(chunkData);
//		makeType(1);
		test4();
//		getResult(chunkData);
		
		lodCompute = new B45LODCompute();
		lodCompute.Init();
		List<byte[]> testChunkDataList = new List<byte[]>();
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		testChunkDataList.Add(chunkData);
		//chunkData2 = lodCompute.Compute(testChunkDataList);
		//B45ChunkData.OccupiedVecsStr(chunkData);
		getResult(chunkData);
		
	}

	void makeType(int pType)
	{
		GameObject go = GameObject.Instantiate(primitiveTypeList[pType]) as GameObject;
		primitiveGO = go;
		
	}
	
	void CreatePrimitive(Vector3[] slopeVerts, int[] slopeVertIndices, Vector2[] uv, int material_index = 0)
	{
		GameObject thisgo = new GameObject("slope"+material_index);
		MeshRenderer mr = thisgo.AddComponent<MeshRenderer>();
		MeshFilter mf = thisgo.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		
		mesh.vertices = slopeVerts;
		mesh.SetTriangles(slopeVertIndices, 0);
		mesh.uv = uv;
		mesh.RecalculateNormals();
		mf.sharedMesh = mesh;
		MeshCollider mc = thisgo.AddComponent<MeshCollider>();
		mc.sharedMesh = mesh;
		
		mr.sharedMaterial = blockMaterials[material_index];
		
		primitiveGO = thisgo;
	}
	void CreateB45ChunkGO(Mesh meshList, int[] usedMatIndices)
	{
		GameObject thisgo = new GameObject("b45mesh");
		MeshRenderer mr = thisgo.AddComponent<MeshRenderer>();
		MeshFilter mf = thisgo.AddComponent<MeshFilter>();
		mf.sharedMesh = meshList;
		MeshCollider mc = thisgo.AddComponent<MeshCollider>();
		mc.sharedMesh = meshList;
		
		List<Material> tmpMatList = new List<Material>();
			
		for(int i =0; i < meshList.subMeshCount; i++)
		{
			tmpMatList.Add(blockMaterials[usedMatIndices[i]]);
		}
		
		mr.sharedMaterials = tmpMatList.ToArray();
		//thisgo.transform.position = new Vector3(10,10,10);
		chunkGOList.Add(thisgo);
	}
	void getResult(byte[] inputChunkData)
	{
		cpuBlock45.Inst.AddChunkVolumeData(inputChunkData);
		cpuBlock45.Inst.computeIsosurface();
		
		List<Mesh> retMeshList = cpuBlock45.Inst.getOutputMesh();
		List<int[]> usedMatIndices = cpuBlock45.Inst.usedMaterialIndicesList;
		CreateB45ChunkGO(retMeshList[0], usedMatIndices[0]);
		cpuBlock45.Inst.clearOutputMesh();
	}
	void getResult(List<byte[]> inputChunkData)
	{
		for(int i = 0; i < inputChunkData.Count; i++)
			cpuBlock45.Inst.AddChunkVolumeData(inputChunkData[i]);
		cpuBlock45.Inst.computeIsosurface();
		
		List<Mesh> retMeshList = cpuBlock45.Inst.getOutputMesh();
		List<int[]> usedMatIndices = cpuBlock45.Inst.usedMaterialIndicesList;
		
		for(int i = 0; i < inputChunkData.Count; i++)
			CreateB45ChunkGO(retMeshList[i], usedMatIndices[i]);
			
		cpuBlock45.Inst.clearOutputMesh();
	}
	void writeExtendedBlock(int x, int y, int z, int primitiveType, int rotation, int extendDir, int length, byte materialType)
	{
		B45Block b0, b1;
		B45Block.MakeExtendableBlock(primitiveType, rotation, extendDir, length, (int)materialType, out b0, out b1);
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,y,z)] = b0.blockType;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,y,z)+1] = b0.materialType;
		
		int idx = B45Block.Block45Size*B45ChunkData.OneIndex(
			x+Block45Kernel._2BitToExDir[extendDir*3],
			y+Block45Kernel._2BitToExDir[extendDir*3+1],
			z+Block45Kernel._2BitToExDir[extendDir*3+2]);
		chunkData[idx] = b1.blockType;
		chunkData[1+idx] = b1.materialType;
		
		for(int i = 2; i < length; i++){
			idx = B45Block.Block45Size*B45ChunkData.OneIndex(
				x+Block45Kernel._2BitToExDir[extendDir*3] * i,
				y+Block45Kernel._2BitToExDir[extendDir*3+1] * i,
				z+Block45Kernel._2BitToExDir[extendDir*3+2] * i);
			
			chunkData[idx] = (byte)extendDir;
			chunkData[1+idx] = (byte)(length-2);
		}
	}
	void testextendedblock()
	{
		writeExtendedBlock(0,0,0,
			2,0,B45Block.EBX,3,1);
	}
	void test1()
	{
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,0)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,0)+1] = 0;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,1)] = B45Block.MakeBlockType(2,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,1)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,2)] = B45Block.MakeBlockType(4,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,2)+1] = 2;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,3)] = B45Block.MakeBlockType(3,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,3)+1] = 3;
		
	}
	void test2()
	{
		UnityEngine.Random.seed = 1223;
		for(int z =0; z < Block45Constants._numVoxelsPerAxis;z++)
		{
//			for(int y =0; y < Block45Constants._numVoxelsPerAxis;y++)
			{
				for(int x =0; x < Block45Constants._numVoxelsPerAxis;x++)
				{
					int rot = (Mathf.RoundToInt(UnityEngine.Random.value *256)) % 4;
					chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,0,z)] = 
						B45Block.MakeBlockType(((x + z * Block45Constants._numVoxelsPerAxis + 0 
						* Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis)%6) + 1, rot);
					
					rot = (Mathf.RoundToInt(UnityEngine.Random.value *256)) % 4;
					chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,1,z)] = 
						B45Block.MakeBlockType(((x + z * Block45Constants._numVoxelsPerAxis + 0 
						* Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis)%6) + 1, rot);
				}
			}
		}
	}
	void test3()
	{
		UnityEngine.Random.seed = 1223;
		for(int z =0; z < Block45Constants._numVoxelsPerAxis;z++)
		{
//			for(int y =0; y < Block45Constants._numVoxelsPerAxis;y++)
			{
				for(int x =0; x < Block45Constants._numVoxelsPerAxis;x++)
				{
					int rot = (Mathf.RoundToInt(UnityEngine.Random.value *256)) % 4;
					chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,0,z)] = 
						B45Block.MakeBlockType(((x + z * Block45Constants._numVoxelsPerAxis + 0 
						* Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis)%6) + 1, rot);
					
					chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,0,z) + 1] = (byte)(x % 3);
					
					rot = (Mathf.RoundToInt(UnityEngine.Random.value *256)) % 4;
					chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,1,z)] = 
						B45Block.MakeBlockType(((x + z * Block45Constants._numVoxelsPerAxis + 0 
						* Block45Constants._numVoxelsPerAxis * Block45Constants._numVoxelsPerAxis)%6) + 1, rot);
					chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(x,1,z) + 1] = (byte)(z % 3);
				}
			}
		}
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,0)] = B45Block.MakeBlockType(1,0);
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,0)+1] = 0;
//		
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,1)] = B45Block.MakeBlockType(1,0);
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,1)+1] = 1;
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,2)] = B45Block.MakeBlockType(1,0);
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,2)+1] = 2;
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,3)] = B45Block.MakeBlockType(1,0);
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,3)+1] = 0;
//		
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,3)] = B45Block.MakeBlockType(1,0);
//		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(0,0,3)+1] = 3;
		
	}
	void test4()
	{
		IntVector3 bPos = new IntVector3(0,0,0);
		int rot = 0;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 0)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 0)+1] = 0;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 1)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 1)+1] = 0;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 1)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 1)+1] = 0;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 0)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 0)+1] = 0;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 0)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 0)+1] = 0;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 1)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 1)+1] = 0;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 1)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 1)+1] = 0;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 0)] = B45Block.MakeBlockType(1,0);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 0)+1] = 0;
		
		////
		bPos = new IntVector3(2,0,0);
		rot = 3;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 0)] = B45Block.MakeBlockType(1,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 0)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 1)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 1)+1] = 1;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 1)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 1)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 0)] = B45Block.MakeBlockType(1,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 0)+1] = 1;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 0)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 0)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 1)] = B45Block.MakeBlockType(0,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 1)+1] = 1;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 1)] = B45Block.MakeBlockType(0,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 1)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 0)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 0)+1] = 1;
		
		/////////////////
		bPos = new IntVector3(4,0,0);
		rot = 0;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 0)] = B45Block.MakeBlockType(1,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 0)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 1)] = B45Block.MakeBlockType(1,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 0, bPos.z + 1)+1] = 1;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 1)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 1)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 0)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 0, bPos.z + 0)+1] = 1;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 0)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 0)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 1)] = B45Block.MakeBlockType(2,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 0, bPos.y + 1, bPos.z + 1)+1] = 1;
		
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 1)] = B45Block.MakeBlockType(0,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 1)+1] = 1;
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 0)] = B45Block.MakeBlockType(0,rot);
		chunkData[B45Block.Block45Size*B45ChunkData.OneIndex(bPos.x + 1, bPos.y + 1, bPos.z + 0)+1] = 1;
	}
	
	float angle = 90;
	
	void Update () {
		
		
			
		Debug.DrawLine(new Vector3(0,0,0), new Vector3(10,0,0), Color.red);
		Debug.DrawLine(new Vector3(0,0,0), new Vector3(0,10,0), Color.green);
		Debug.DrawLine(new Vector3(0,0,0), new Vector3(0,0,10), Color.blue);
		if(Input.GetKeyUp(KeyCode.T))
		{
			genList();
		}
		if(Input.GetKeyUp(KeyCode.R))
		{

			Vector3 against = new Vector3(0.5f,0,0.5f);

			primitiveGO.transform.RotateAround(against, Vector3.up,angle);
		}
		
		for(int i = 1; i <= 6; i++)
		{
			if(Input.GetKeyUp(KeyCode.Alpha1 + i - 1))
			{
				GameObject.DestroyImmediate(primitiveGO);
				makeType(i);
				
			}
		}
	
	}
	float[] fourptsX = new float[4]{
		0.5f, 0.25f, 0.5f, 0.75f,
		
	};
	float[] fourptsY = new float[4]{
		0.25f,0.5f,0.75f,0.5f,
	};
	void genList()
	{
		string str = "";
		int hexa = 0;
		RaycastHit hitInfo;
		for(int i = 0; i < 4; i++){
			Debug.DrawRay(new Vector3(-0.1f,fourptsY[i], 1-fourptsX[i]), new Vector3(1,0,0), Color.white);
			if(Physics.Raycast(new Vector3(-0.1f,fourptsY[i], 1-fourptsX[i]), new Vector3(1,0,0), out hitInfo, 0.11f) )
			{
				str += "1, ";
				hexa += (1 << (3-i));
				
			}
			else
				str += "0, ";
			
		}
		print(str + " 0x" + hexa.ToString("X"));
	}
	void genListTopBottom()
	{
		string str = "";
		RaycastHit hitInfo;
		for(int i = 0; i < 4; i++){
			Debug.DrawRay(new Vector3(fourptsX[i],fourptsY[i], -0.1f), new Vector3(0,0,0.11f), Color.white);
			if(Physics.Raycast(new Vector3(fourptsX[i],fourptsY[i], -0.1f), new Vector3(0,0,1), out hitInfo, 0.11f) )
			{
				str += "1, ";
				
			}
			else
				str += "0, ";
			
		}
		print(str);
	}
	void OnDrawGizmos()
	{
		for(int i =0 ; i < 8;i++)
			Gizmos.DrawIcon(Block45Kernel.indexedCoords[i], ""+i+".png", false);
	}
	
}
