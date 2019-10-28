using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public delegate void DelegateB45ColliderCreated(B45ChunkGo b45Chunk);
public class Block45Building : MonoBehaviour {

    public event DelegateB45ColliderCreated OnColliderCreated;
    public event DelegateB45ColliderCreated OnColliderDestroy;

	public Material[] blockMaterials;
	public static Block45Building self;
	bool CreateColliderAtMeshGen = false;
	public Transform _observer;
	private IB45DataSource _blockDS;
	private BiLookup<int, B45ChunkData> chunkRebuildList;
	public LODDataUpdate lodDataUpdate;
	public int RebuildListCount{ get{ return chunkRebuildList.Count;}}
	public IB45DataSource Voxels{	get{	return _blockDS;	}	}
	public BiLookup<int, B45ChunkData> ChunkRebuildList{	get{	return chunkRebuildList;	}	}
	public bool TestMode = false;
	public int MeshMode;
	public bool DebugMode = false;
	
	public bool GlobalInstance = false;
	
	int mVersion = 2;
	public GameObject VizCube;
	
	//Dictionary<IntVector3, B45Block> mInitChunkData = new Dictionary<IntVector3, B45Block>();
	
	bool colliderBuilding;
	public bool isColliderBuilding {get{return colliderBuilding;}}
	cpuBlock45 b45proc;

    public void AttachEvents(DelegateB45ColliderCreated created = null,
                                DelegateB45ColliderCreated destroy = null)
    {
        OnColliderCreated += created;
        OnColliderDestroy += destroy;
    }

    public void DetachEvents(DelegateB45ColliderCreated created = null,
                                    DelegateB45ColliderCreated destroy = null)
    {
        OnColliderCreated -= created;
        OnColliderDestroy -= destroy;
    }

	void Awake()
	{
		if(GlobalInstance)
			self = this;
		bBuildColliderAsync = false;
		colliderBuilding = true;
		b45proc = new cpuBlock45();
		b45proc.init();
		
		chunkRebuildList = new BiLookup<int, B45ChunkData>();
		
		_blockDS = new B45OctreeDataSource(chunkRebuildList, this);
	}
	public void SetMeshMode(int mode)
	{
		MeshMode = mode;
	}
	public void Start()
	{
//		if(CreateColliderAtMeshGen == false)
//			StartCoroutine(SetChunksCollider());
//		
//		B45ChunkGoCreator.Start(this, chunkRebuildList, SetChunksMeshSM, true);

        //if (GlobalInstance && !GameConfig.IsMultiMode && Record.m_RecordBuffer.ContainsKey((int)Record.RecordIndex.RECORD_BLOCK45))
        //    Import(Record.m_RecordBuffer[(int)Record.RecordIndex.RECORD_BLOCK45]);
		
		if(TestMode)
			onPlayerPosReady(_observer);
//		testLOD();
//		testbv();
	}
	void testbv()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter _out = new BinaryWriter(ms);
		
		_out.Write(mVersion);
		switch(mVersion)
		{
		case 2:
			int elementCount = 1;
			_out.Write(elementCount);
			_out.Write(1);
			_out.Write(2);
			_out.Write(3);
			_out.Write((byte)11);
			_out.Write((byte)1);
//
//			_out.Write(1);
//			_out.Write(2);
//			_out.Write(30);
//			_out.Write((byte)12);
//			_out.Write((byte)1);
//
//			_out.Write(1);
//			_out.Write(2);
//			_out.Write(60);
//			_out.Write((byte)13);
//			_out.Write((byte)1);

			//BlockVectorNode.rec_append(bvRoot, _out);
			break;
		}
		_out.Close();
		ms.Close();
		byte[] inpba = ms.ToArray();
		Import(ms.ToArray());

		byte[] testba = Export();
		String str = "";
		for(int i = 0; i < inpba.Length; i++){
			str += inpba[i] + ", ";
		}
//		print (str);
		str = "";
		for(int i = 0; i < inpba.Length; i++){
			str += testba[i] + ", ";
		}
//		print (str);
	}
	B45LODNode root;
	void testLOD()
	{
		root = new B45LODNode(new IntVector4(0,0,0, 5), null, 0);
		B45LODNode.splitAt(root, new IntVector3(10,20,30), 3);
		B45LODNode.makeCubeRec(root);
	}

	ChunkMeshMerger cmm = new ChunkMeshMerger();
	void SetChunksMesh(List<IntVector4> chunkPosList, List<B45ChunkData> chunks, uint numChunks)
	{
		
		List<Mesh> meshListFromKernel = b45proc.getOutputMesh();
		
		
		cmm.Merge(chunkPosList, chunks, numChunks, meshListFromKernel);
	}
	
	
	public void FinishMerge()
	{
		cmm.truncateLastMesh();
		List<ChunkMeshMerger.MeshStruct> meshList = cmm.GetReorganizedMeshes();
		for(int n = 0; n < meshList.Count; n++)
		{
			
			GameObject go = new GameObject();
			go.isStatic = true;
			MeshFilter mf = go.AddComponent<MeshFilter>();
	        MeshCollider mc = go.AddComponent<MeshCollider>();
	        MeshRenderer mr = go.AddComponent<MeshRenderer>();
			mc.sharedMesh = null;
			mr.sharedMaterial = blockMaterials[0];
			
			Mesh _mesh = new Mesh();
			_mesh.vertices = meshList[n].vertices;
			_mesh.uv = meshList[n].uv;
			_mesh.SetTriangles(meshList[n].triangles, 0);
			_mesh.normals = meshList[n].normals;
			_mesh.name = "b45mergedmesh_"+n;

			mf.sharedMesh = _mesh;
			
			B45ChunkGo vfGo = go.AddComponent<B45ChunkGo>();
			vfGo._mesh = _mesh;
			vfGo._meshCollider = mc;

            vfGo.OnColliderCreated += OnColliderCreatedFunc;
            vfGo.OnColliderDestroy += OnColliderDestroyFunc;
			
			if(CreateColliderAtMeshGen)
			{
				SetChunkCollider(vfGo);
				
			}

			go.transform.parent = transform;	
//			chunks[n].AttachChunkGo(vfGo);

			go.layer = Pathea.Layer.VFVoxelTerrain;
			
		}
		b45proc.clearOutputMesh();
	}

    void OnColliderCreatedFunc(B45ChunkGo b45Chunk)
    {
        if (null != OnColliderCreated)
            OnColliderCreated(b45Chunk);
    }

    void OnColliderDestroyFunc(B45ChunkGo b45Chunk)
    {
        if (null != OnColliderDestroy)
            OnColliderDestroy(b45Chunk);
    }

	void SetChunksMeshByMaterial(List<IntVector4> chunkPosList, List<B45ChunkData> chunks, uint numChunks)
	{
		
		List<List<Mesh>> meshList = b45proc.getOutputMeshByMaterial();
		for(int n = 0; n < chunks.Count; n++)
		{
			if(chunks[n].IsChunkInReq || !chunkPosList[n].Equals(chunks[n].ChunkPosLod))
				continue;
			
//			if(chunks[n].IsHollow){		chunks[n].FinHollowUpdate();
//				continue;		}
			for(int mat_i = 0; mat_i < Block45Constants.MaxMaterialCount; ++mat_i)
			{

				Mesh _mesh = meshList[n][mat_i];
				
				if(_mesh == null || _mesh.vertexCount == 0)continue;
				
				//int tmpVertsLimit = 64998;
				
				GameObject go = new GameObject();
				
				MeshFilter mf = go.AddComponent<MeshFilter>();
		        MeshCollider mc = go.AddComponent<MeshCollider>();
		        MeshRenderer mr = go.AddComponent<MeshRenderer>();
				mc.sharedMesh = null;
				
				_mesh.name = "b45mesh_"+mat_i+"_"+chunks[n].GenChunkIdentifier();
				mf.sharedMesh = _mesh;
				mr.sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterials[mat_i];
				
				B45ChunkGo vfGo = go.AddComponent<B45ChunkGo>();
				vfGo._mesh = _mesh;
				vfGo._meshCollider = mc;
				
	            vfGo.OnColliderCreated += OnColliderCreatedFunc;
	            vfGo.OnColliderDestroy += OnColliderDestroyFunc;
				
				if(CreateColliderAtMeshGen)
				{
					SetChunkCollider(vfGo);
				
				}
	
				go.transform.parent = transform;	
				chunks[n].AttachChunkGo(vfGo, mat_i);
	
				go.layer = Pathea.Layer.VFVoxelTerrain;
			}
			
		}
		b45proc.clearOutputMesh();
	}
	// using submesh
	void SetChunksMeshSM(List<int> chunkStampsList, List<B45ChunkData> chunks, uint numChunks)
	{
		
		List<Mesh> meshList = b45proc.getOutputMesh();
		List<int[]> usedMatIndices = b45proc.usedMaterialIndicesList;
		for(int n = 0; n < chunks.Count; n++)
		{
			int stamp = chunkStampsList[n];
			
			if(!chunks[n].IsStampIdentical(stamp))
			{
				//Debug.Log("RemoveChunkInSet"+chunkData.ChunkPosLod+":"+stamp+"|"+chunkData.StampOfChnkUpdating);
				continue;
			}
			
			
//			if(chunks[n].IsHollow){		chunks[n].FinHollowUpdate();
//				continue;		}
			if(n >= meshList.Count)
			{
				continue;
			}
			
			Mesh _mesh = meshList[n];
			
			if(_mesh == null)
			{
				continue;
			}
			
//			int tmpVertsLimit = 64998;
			
			GameObject go = new GameObject();
			
			MeshFilter mf = go.AddComponent<MeshFilter>();
	        MeshCollider mc = go.AddComponent<MeshCollider>();
	        MeshRenderer mr = go.AddComponent<MeshRenderer>();
			mc.sharedMesh = null;
			
			_mesh.name = "b45mesh_"+chunks[n].GenChunkIdentifier();
			mf.sharedMesh = _mesh;
			
			List<Material> tmpMatList = new List<Material>();
			
			for(int i =0; i < _mesh.subMeshCount; i++)
			{
				tmpMatList.Add(blockMaterials[usedMatIndices[n][i]]);
//				tmpMatList.Add(blockMaterials[i]);
			}
			
			mr.sharedMaterials = tmpMatList.ToArray();
			
			B45ChunkGo vfGo = go.AddComponent<B45ChunkGo>();
			vfGo._mesh = _mesh;
			vfGo._meshCollider = mc;
			
            vfGo.OnColliderCreated += OnColliderCreatedFunc;
            vfGo.OnColliderDestroy += OnColliderDestroyFunc;
			
			if(CreateColliderAtMeshGen)
			{
				SetChunkCollider(vfGo);
				
			}
			
			chunks[n].safeToRemoveCollider();
			go.transform.parent = transform;	
			chunks[n].AttachChunkGo(vfGo);

			go.layer = Pathea.Layer.VFVoxelTerrain;
			//chunks[n].setNotInQueue();
		}
		b45proc.clearOutputMesh();
	}

	void SetChunkCollider(B45ChunkGo goChunk)
	{
		goChunk.SetCollider();
		
		B45ChunkGo[] children = goChunk.GetComponentsInChildren<B45ChunkGo>();
		for(int i = 0; i < children.Length; i++)
		{
			children[i].SetCollider();
		}
	}

	bool bBuildColliderAsync;
	IEnumerator SetChunksCollider()
	{
		Dictionary<IntVector4, B45ChunkData> chunks = ((B45OctreeDataSource)_blockDS).ChunksDictionary;
        
		while(true)
		{
            if (_observer == null)
			{
				yield return 0;
				continue;
			}

			//int collidersCreated = 0;
			foreach(KeyValuePair<IntVector4, B45ChunkData> kvp in chunks)
			{
				
				B45ChunkData chunk = kvp.Value;
				if(chunk == null || kvp.Key.w != 0)continue;
				
				B45ChunkGo goChunk = chunk.ChunkGo;
				if(goChunk != null && goChunk._meshCollider.sharedMesh == null)
				{
					colliderBuilding = true;
					SetChunkCollider(goChunk);
					chunk.safeToRemoveCollider();
					chunk.setNotInQueue();
					
					if(bBuildColliderAsync)
						goto ColliderSetFin;
				}
			}
			colliderBuilding = false;
			
			ColliderSetFin:
			yield return 0;
		}
	}
	B45ChunkGoCreator b45creator;
	public void onPlayerPosReady(Transform trans){
		_observer = trans;
		((B45OctreeDataSource)_blockDS).OctreeUpdate(_observer.transform.position*Block45Constants._scaleInverted);
		b45creator = new B45ChunkGoCreator();
		
		b45creator.Start(this, chunkRebuildList, SetChunksMeshSM, b45proc, false);
//		if(null == GetComponent<BuildOpBlock>()) // Use sync mode for opblock to avoid bugs on switch brush while building
//		{
//			b45creator._bAsyncBuildMode = true;
//		}
		
		StartCoroutine(SetChunksCollider());
		bBuildColliderAsync = true;
		
		
	}
	void Update()
	{
		if(_observer == null){
            //if(PlayerFactory.mMainPlayer != null){
            //    //_observer = PlayerFactory.mMainPlayer.transform;
            //    onPlayerPosReady(PlayerFactory.mMainPlayer.transform);
            //}
            //else
                if(null != Camera.main)
			{
				onPlayerPosReady(Camera.main.transform);
			}

		}
		else{
			
			((B45OctreeDataSource)_blockDS).OctreeUpdate((_observer.transform.position - transform.position)*Block45Constants._scaleInverted);
		}
		
		if(TestMode)
			DebugSaveLoad();
		//StartCoroutine(saveChunksInList);
//		randombuild();
		
	}
	void OnDestroy()
	{
		if(lodDataUpdate != null)
			lodDataUpdate.Stop();
		Debug.Log("Mem size before vfTerrain destroyed all chunks :"+GC.GetTotalMemory(true));
		if (gameObject.transform.childCount > 0)
		{
			// Clean up chunks
			for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
			{
				var child = gameObject.transform.GetChild(i).gameObject;
				DestroyImmediate(child);
			}
		}
	}
	public void AlterBlockInBuild(int vx, int vy, int vz, B45Block blk)
	{
		int nOldCnt = chunkRebuildList.Count;
		_blockDS.SafeWrite(vx, vy, vz, blk, 0);
		int nNewCnt = chunkRebuildList.Count;
		for(int i = nOldCnt; i < nNewCnt; i++)
		{
			AddChunkToSaveList(chunkRebuildList[i]);
		}
	}
	#region Voxel_SaveModified
	List<B45ChunkData> chunkSaveList = new List<B45ChunkData>();
	//byte[] chunkModifyFlags;
    //IntVector2 x-Ô­chunkModifyFlagsµÄidx£¬y-Ô­chunkModifyFlagsµÄvalue¡£IntVector3žúSubterÒ»Ñù¡£
    Dictionary<IntVector3, byte[]> m_SaveBuffer;// = new Dictionary<IntVector3, byte[]>();
 
	public void AddChunkToSaveList(B45ChunkData vc)
	{
		if(!chunkSaveList.Contains(vc))
		{
			chunkSaveList.Add(vc);
		}
	}
	VoxelFileMan vfile;

//	IEnumerator	saveChunksInList()
//	{
//		//int actualSaves = 0;
//		for(int i = 0; i < chunkSaveList.Count; i++ )
//		{
//			if(chunkSaveList[i] != null)
//			{
//				saveChunk(chunkSaveList[i]);
//				yield return 0;
//				//actualSaves++;
//			}
//		}
//        //if(actualSaves > 0 )
//        //{
//        //    print("saving modified chunk data: " + actualSaves + " chunks saved");
//        //    saveModifyFlagData();
//        //}
//		chunkSaveList.Clear();
//	}

#region Network
	public byte[][] GetChunkData()
	{
		IEnumerable<byte[]> data = chunkSaveList.Select(iter => iter.DataVT);
		return data.ToArray();
	}

	public int GetChunkDataCount()
	{
		return chunkSaveList.Count;
	}

	public void ResetSaveList()
	{
		chunkSaveList.Clear();
	}
#endregion

	#endregion
	
	public byte[] Export()
	{
		BlockVectorNode root = ((B45OctreeDataSource)_blockDS).bvtRoot;
        MemoryStream ms = new MemoryStream();
        BinaryWriter _out = new BinaryWriter(ms);
		_out.Write(mVersion);
		switch(mVersion)
		{
		case 2:
			((B45OctreeDataSource)_blockDS).ConvertAllToBlockVectors();
			
			int elementCount = BlockVectorNode.rec_count(root);
			_out.Write(elementCount);
			BlockVectorNode.rec_append(root, _out);
			break;
		}
        _out.Close();
        ms.Close();
        return ms.ToArray();
	}
	void DebugSaveLoad()
	{
		String tmpFile = "tmp.bin";
		// load
		if(Input.GetKeyUp(KeyCode.F9)){
			byte[] saveData = null;
			
			using(FileStream fileStream = new FileStream(tmpFile, FileMode.Open, FileAccess.Read))
	        {
	            BinaryReader _in = new BinaryReader(fileStream);
				saveData = new byte[fileStream.Length];
				print("" + _in.Read(saveData, 0, (int)fileStream.Length) + " bytes read.");
				_in.Close();
				fileStream.Close();
					
			}
			Import(saveData);
			
		}
		// save
		if(Input.GetKeyUp(KeyCode.F10)){
			byte[] saveData = Export ();
			using(FileStream fileStream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write))
	        {
	            BinaryWriter _out = new BinaryWriter(fileStream);
				_out.Write(saveData);
				_out.Close();
				fileStream.Close();
			}
			
		}
		if(Input.GetKeyUp(KeyCode.O)){
			//_blockDS.SafeWrite(
			
		}
	}
	bool compareSeg(byte[] arr, int ind0, int ind1)
	{
		for(int i = 0; i < 14; i++){
			if(arr[ind0 + i] != arr[ind1 + i])
			{
				return false;
			}
		}
		return true;
	}
	int inc = 0;
	void randombuild(){
//		if(DebugMode == false)
//			return;
		int stX = 1;
		int stY = 1;
		int stZ = 62;
		
		int size = 16;
		if(Input.GetKeyUp(KeyCode.Alpha5))
		{
			_blockDS.SafeWrite(1,63,63, new B45Block(B45Block.MakeBlockType(1,0),4), 0);
			_blockDS.SafeWrite(1,64,64, new B45Block(B45Block.MakeBlockType(1,0),4), 0);
			
			_blockDS.SafeWrite(1,63,64, new B45Block(B45Block.MakeBlockType(1,0),4), 0);
			_blockDS.SafeWrite(1,64,63, new B45Block(B45Block.MakeBlockType(1,0),4), 0);
		}
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			stX += inc;
			stY += inc;
			stZ += inc;
			for(int z = 0; z <1;z++){
				for(int y = 0; y <size;y++){
					for(int x = 0; x <size;x++){
						_blockDS.SafeWrite(x + stX,y + stY,z + stZ, new B45Block(B45Block.MakeBlockType(1,0),2), 0);
					}
				}
			}
			inc++;
		}
		if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			stX += inc;
			stY += inc;
			stZ += inc;
			for(int z = 0; z <size;z++){
				for(int y = 0; y <1;y++){
					for(int x = 0; x <size;x++){
						_blockDS.SafeWrite(x + stX,y + stY,z + stZ, new B45Block(B45Block.MakeBlockType(1,0),3), 0);
					}
				}
			}
			inc++;
		}
		if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			stX += inc;
			stY += inc;
			stZ += inc;
			for(int z = 0; z <size;z++){
				for(int y = 0; y <size;y++){
					for(int x = 0; x <1;x++){
						_blockDS.SafeWrite(x + stX,y + stY,z + stZ, new B45Block(B45Block.MakeBlockType(1,0),4), 0);
					}
				}
			}
			inc++;
		}
		
	}
		
	
	public void Import(byte[] buffer)
	{
		// vector octree root
		BlockVectorNode root = ((B45OctreeDataSource)_blockDS).bvtRoot;
		((B45OctreeDataSource)_blockDS).Clear();
		
		
		
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);
		
		int readVersion = _in.ReadInt32();
		switch(readVersion)
		{
		case 2:
			int Size = _in.ReadInt32();
			for(int i = 0; i < Size; i++)
			{
				int x = _in.ReadInt32();
				int y = _in.ReadInt32();
				int z = _in.ReadInt32();
				IntVector3 index = new IntVector3(x,y,z);
				try{
					root = ((B45OctreeDataSource)_blockDS).bvtRoot.reroot(index);
				}
				catch(Exception e)
				{
					Debug.LogWarning("Unexpected exception while importing"+index+e);
					break;
				}
				
				((B45OctreeDataSource)_blockDS).bvtRoot = root;
				
				BlockVectorNode bvnode = BlockVectorNode.readNode(new IntVector3(x,y,z), root);
				//BlockVectorNode bvnode = node;

				if(bvnode.blockVectors == null)
				{
					bvnode.blockVectors = new List<BlockVector>() as List<BlockVector>;
				}
				// calculate the position relative to the block chunk's position.
				x = x & Block45Constants._mask;
				y = y & Block45Constants._mask;
				z = z & Block45Constants._mask;
				bvnode.blockVectors.Add(new BlockVector(
					x + Block45Constants._numVoxelsPrefix,
					y + Block45Constants._numVoxelsPrefix,
					z + Block45Constants._numVoxelsPrefix,
					_in.ReadByte(),_in.ReadByte()));
			}
			break;
		}
        _in.Close();
        ms.Close();
	}
}
public class ChunkColliderMan{
	Dictionary<IntVector3, int> colliderPos;
	public ChunkColliderMan()
	{
		colliderPos = new Dictionary<IntVector3, int>();
	}
	public void addRebuildChunk(IntVector3 chunkIdx)
	{
		if(!colliderPos.ContainsKey(chunkIdx))
			colliderPos.Add(chunkIdx, 0);
	}
	public bool isColliderBeingRebuilt(IntVector3 chunkIdx)
	{
		return colliderPos.ContainsKey(chunkIdx);
	}
	public void colliderBuilt(IntVector3 chunkIdx)
	{
		colliderPos.Remove(chunkIdx);
	}
}