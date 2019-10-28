using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class BuildingEditOps : MonoBehaviour {
	GameObject cursorCubeGo = null;
	GameObject RefPlaneManGo;
	Block45CurMan b45Building = null;
	//RefPlaneManager refPlaneMan;
	private Block45OctDataSource _voxels = null;
	private 
		int currentShape;
		int currentRotation;
		int currentMat;
	// extended
	void Awake()
	{
//		cursorCubeGo = GameObject.Find("CursorCube");
//		cursorCubeGo.transform.localScale = new Vector3(Block45Constants._scale,Block45Constants._scale,Block45Constants._scale);
		
		//RefPlaneManGo = GameObject.Find("RefPlaneMan");
		//refPlaneMan = RefPlaneManGo.GetComponent<RefPlaneManager>();
		
	}
	void Start () {
	
	}
	public void setShape(int val)
	{
		currentShape = val;
	}
	public void setMat(int val)
	{
		currentMat = val;
	}
	public void setRotation(int val)
	{
		currentRotation = val;
	}
	
	IntVector3 buildCursorPos;
	
	
	void Update () {
		if(_voxels == null && b45Building != null){
			_voxels = b45Building.DataSource;
		}
		RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
        if (Physics.Raycast(ray, out raycastHit, 100.0f)
			//&& Physics.Raycast(ray, out raycastHitUnused, 100.0f, uiLayerMask) == false
			)
        {
			if(cursorCubeGo != null){
				cursorCubeGo.GetComponent<MeshRenderer>().enabled = true;
				Vector3 snapVec = raycastHit.point;
				//snapVec += raycastHit.normal/2.0f;
				snapVec *= Block45Constants._scaleInverted;
				// round to the nearest constants._scale
				
				snapVec.x = Mathf.FloorToInt(snapVec.x);
				snapVec.y = Mathf.FloorToInt(snapVec.y);
				snapVec.z = Mathf.FloorToInt(snapVec.z);
				
				snapVec.x += Block45Constants._scale / 2.0f;
				snapVec.y += Block45Constants._scale / 2.0f;
				snapVec.z += Block45Constants._scale / 2.0f;
				buildCursorPos = new IntVector3(snapVec);
				snapVec /= Block45Constants._scaleInverted;
				
				cursorCubeGo.transform.position = snapVec;
				
				if(snapVec.x >= 0 && snapVec.y >= 0 && snapVec.z >= 0){
					
					
					buildCursorPos.x = Mathf.FloorToInt(buildCursorPos.x);
					buildCursorPos.y = Mathf.FloorToInt(buildCursorPos.y);
					buildCursorPos.z = Mathf.FloorToInt(buildCursorPos.z);
					
				}
				else{
					cursorCubeGo.GetComponent<MeshRenderer>().enabled = false;
					buildCursorPos = null;
				}
			}
		}
		else
		{
//			if(Physics.Raycast(ray, out raycastHitUnused, 100.0f, uiLayerMask))
//			{
//				print (raycastHitUnused.collider.name);
//			}
//			cursorCubeGo.GetComponent<MeshRenderer>().enabled = false;
//			buildCursorPos = null;
		}
		if(Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftAlt) && buildCursorPos != null)
		{
			B45Block block;
			block.blockType = B45Block.MakeBlockType(currentShape, currentRotation);
			block.materialType = (byte)currentMat;
//			_voxels.Write(buildCursorPos.x, buildCursorPos.y, buildCursorPos.z, block, 0);
			b45Building.AlterBlockInBuild(buildCursorPos.x, buildCursorPos.y, buildCursorPos.z, block);
		}
//		if(Input.GetKey(KeyCode.P))
//		{
//			float lv = refPlaneMan.getXYRefPlaneLevel();
//			refPlaneMan.setXYRefPlane(lv + 1);
//		}
//		if(Input.GetKey(KeyCode.O))
//		{
//			float lv = refPlaneMan.getXYRefPlaneLevel();
//			refPlaneMan.setXYRefPlane(lv - 1);
//		}

		testRoutine();
	
	}
	#region simulated testing
	int lastInvocation = -1;
	int opFrequency = 50;
	List<IntVector3> spots;
	bool randomized;
	
	void testRoutine(){
		if(Environment.TickCount - lastInvocation > opFrequency && curClick != -1)
		{
			
			B45Block blk;
			blk.blockType = B45Block.MakeBlockType(1, 0);
			blk.materialType = (byte)(Mathf.RoundToInt(UnityEngine.Random.value * 65535) % 16);
			int x, y, z;
			if(randomized)
			{
				x = Mathf.RoundToInt(UnityEngine.Random.value * 65535) % Block45Constants._worldSideLenXInVoxels;
				y = (Mathf.RoundToInt(UnityEngine.Random.value * 65535) % (Block45Constants._worldSideLenYInVoxels - 1)) + 1;
				z = Mathf.RoundToInt(UnityEngine.Random.value * 65535) % Block45Constants._worldSideLenZInVoxels;
				
			}else{
				x = spots[curClick].x;
				y = spots[curClick].y;
				z = spots[curClick].z;
			}
			
			b45Building.AlterBlockInBuild(x, y, z, blk);
			
			OpInfo opInfo;
			opInfo.x = (byte)x;
			opInfo.y = (byte)y;
			opInfo.z = (byte)z;
			opInfo.voxelByte0 = blk.blockType;
			opInfo.voxelByte1 = blk.materialType;
			
			opList.Add(opInfo);
			
			curClick++;
			
//			if((curClick % 4) == 0)
//				b45Building.SaveChunks();
			if(curClick >= numClicks)
			{
				
				//b45Building.SaveChunks();
				print ("simulated test finished.");
				
				// write to file.
				FileStream fs = new FileStream("test_simulation.bin", FileMode.Append, FileAccess.Write, FileShare.Read);
				byte[] tmpBuf = new byte[5];
				for(int i = 0; i < opList.Count; i++ ){
					tmpBuf[0] = opList[i].x;
					tmpBuf[1] = opList[i].y;
					tmpBuf[2] = opList[i].z;
					tmpBuf[3] = opList[i].voxelByte0;
					tmpBuf[4] = opList[i].voxelByte1;
					
					fs.Write(tmpBuf,0,5);
				}
				
				fs.Close();
				opList.Clear();
				curClick = -1;
			}
			
			lastInvocation = Environment.TickCount;
		}
		
		
	}
	int numClicks = 100;
	int curClick = -1;
	public struct OpInfo
	{
		public byte x,y,z;
		public byte voxelByte0, voxelByte1;
		public bool Equals(OpInfo val)
		{
			return x == val.x && y == val.y && z == val.z && voxelByte0 == val.voxelByte0 && voxelByte1 == val.voxelByte1;
		}
	}
	List<OpInfo> opList;
	void OnGUI(){
		if(GUI.Button(new Rect(10,10,140,45), "randomized plots"))
		{
			randomized = true;
			if(opList == null)
				opList = new List<OpInfo>();
			opList.Clear();
			curClick = 0;
			UnityEngine.Random.seed = Environment.TickCount % 65536;
		}
		if(GUI.Button(new Rect(10,70,140,45), "sequential plots"))
		{
			randomized = false;
			spots = new List<IntVector3>();
			for(int i = 0; i < 10; i++)
			{
				spots.Add(new IntVector3(1,1 + i,1));
			}
			
			numClicks = spots.Count;
			if(opList == null)
				opList = new List<OpInfo>();
			opList.Clear();
			curClick = 0;
		}
		
		if(GUI.Button(new Rect(10,130,80,45), "verify"))
		{
			if(opList == null)
				opList = new List<OpInfo>();
			opList.Clear();
			loadPreviousSimulatedTestData();
			verify();
		}
		
		if(GUI.Button(new Rect(10,190,80,45), "merge"))
		{

		}
	}
	void loadPreviousSimulatedTestData()
	{
		FileStream fs = new FileStream("test_simulation.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
		byte[] tmpBuf = new byte[5];
		for(int i = 0; i < fs.Length / 5; i++ ){
			fs.Read(tmpBuf,0,5);
			OpInfo op;
			op.x = tmpBuf[0];
			op.y = tmpBuf[1];
			op.z = tmpBuf[2];
			op.voxelByte0 = tmpBuf[3];
			op.voxelByte1 = tmpBuf[4];
			opList.Add(op);
		}
		fs.Close();
	}
	void verify()
	{
		B45ChunkGo[] cdgos = GameObject.Find("Block45").GetComponentsInChildren<B45ChunkGo>();
		for(int i = 0; i < cdgos.Length; i++){
			B45ChunkGo cgo = cdgos[i];
			if(cgo._data == null) continue;
			List<OpInfo> chunkOV = cgo._data.OccupiedVecs();
			for(int j = chunkOV.Count - 1; j >= 0; j--){
				
				for(int k = opList.Count - 1; k >= 0; k--){
					if(chunkOV[j].Equals(opList[k]))
					{
						opList.RemoveAt(k);
						break;
					}
				}
			}
		}
		print ("leftovers: " + opList.Count);
		for(int i = 0; i < opList.Count;i++){
			print (":(" + opList[i].x + "," + opList[i].y + "," + opList[i].z + ") " + opList[i].voxelByte0 + "-" + opList[i].voxelByte1+";");
		}
		
		
	}
	
	#endregion
}
