using UnityEngine;
using System.Collections.Generic;
using System;

// no lod 
public class B45OctreeDataSource : IB45DataSource
{
	// chunk size 8*8*8
	public readonly int[,] nearChunkOfs = new int[8,3]{
		{0,0,0},
		{1,0,0},
		{0,1,0},
		{1,1,0},
		{0,0,1},
		{1,0,1},
		{0,1,1},
		{1,1,1},
	};
	Block45Building b45Building;
	
	Dictionary<IntVector4, B45ChunkData> _chunks;
	public Dictionary<IntVector4, B45ChunkData> ChunksDictionary {get{return _chunks;}}
	BiLookup<int, B45ChunkData> _chunkRebuildList;
	ChunkColliderMan colliderMan = new ChunkColliderMan();
	// block vector tree root
	public BlockVectorNode bvtRoot;
	
	public B45OctreeDataSource(BiLookup<int, B45ChunkData> chunkRebuildList, Block45Building _b45Building){
		_chunkRebuildList = chunkRebuildList;
		_chunks = new Dictionary<IntVector4, B45ChunkData>();

		bvtRoot = new BlockVectorNode(new IntVector4(0,0,0,1), null, 0);
		
		b45Building = _b45Building;
	}
	public B45ChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		IntVector4 index = new IntVector4(x,y,z, lod);
		if(!_chunks.ContainsKey(index))
			_chunks[index] = CreateChunk(index);
		
		return _chunks[index];
	}
	public void writeChunk(int x, int y , int z, B45ChunkData vc, int lod = 0){
		try
		{
			IntVector4 index = new IntVector4(x,y,z, lod);
			_chunks[index] = vc;
		}
		catch(Exception)
		{
			Debug.LogError(string.Format("writeB45Chunk Exception. Max Length:({0}, {1}, {2}}), CurPos({3}, {4}, {5})"));
		}
	}
	public byte this[IntVector3 idx, int lod]{
		get{	return Read(idx.x, idx.y, idx.z, lod).blockType;	}
	}
	public B45Block Read(int x, int y, int z, int lod = 0){
		int _shift = Block45Constants._shift;
		int cx = (x>>_shift);
		int cy = (y>>_shift);
		int cz = (z>>_shift);
		int vx = (x)&Block45Constants._mask;
		int vy = (y)&Block45Constants._mask;
		int vz = (z)&Block45Constants._mask;
		IntVector4 index = new IntVector4(cx,cy,cz, lod);
		if(!_chunks.ContainsKey(index))
			return new B45Block(0, 0);
		
		return _chunks[index].ReadVoxelAtIdx(vx,vy,vz);
	}
	bool isOutOfMeshDistance(IntVector3 index)
	{
		IntVector3 shiftVec = IntVector3.Zero;
		shiftVec.x = index.x << Block45Constants._shift;
		shiftVec.y = index.y << Block45Constants._shift;
		shiftVec.z = index.z << Block45Constants._shift;
		if(b45Building._observer == null)
			return true;
		return !BlockVectorNode.isCloseTo_static(shiftVec, b45Building._observer.position*Block45Constants._scaleInverted);
		
	}
	public int Write(int x, int y, int z, B45Block voxel, int lod = 0){
		int _shift = Block45Constants._shift;
		int cx = (x>>_shift);
		int cy = (y>>_shift);
		int cz = (z>>_shift);
		int cxround = cx;
		int cyround = cy;
		int czround = cz;
		int vx = (x)&Block45Constants._mask;
		int vy = (y)&Block45Constants._mask;
		int vz = (z)&Block45Constants._mask;
		
		IntVector4 index = new IntVector4(cxround,cyround,czround, lod);
		
		// determine if this write is out of the mesh distance
		if(isOutOfMeshDistance(index.XYZ))
		{
			writeToBlockVectors(index.XYZ, new IntVector3(vx, vy, vz), voxel.blockType, voxel.materialType);
		}
		else
		{
			if(!_chunks.ContainsKey(index))
				_chunks[index] = CreateChunk(index);
			
			if(!_chunks[index].WriteVoxelAtIdx(vx,vy,vz,voxel))
				return 0;
		}
		
		//_SaveData[new IntVector3(x,y,z)] = voxel;

		int minIdx = Block45Constants._numVoxelsPostfix;
		int maxIdx = Block45Constants._numVoxelsPerAxis - Block45Constants._numVoxelsPrefix;
		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// 0,1,2 bit for xyz dirty mask;4,5,6 bit for sign(neg->1);7 bit for current pos(now not used)
		
		// If write one edge's voxel may cause the other edge being modified
		if( vx< minIdx)		{fx = -1;dirtyMask|=0x11;}else
		if( vx>=maxIdx)		{fx =  1;dirtyMask|=0x01;}
		if( vy< minIdx)		{fy = -1;dirtyMask|=0x22;}else
		if( vy>=maxIdx)		{fy =  1;dirtyMask|=0x02;}
		if( vz< minIdx)		{fz = -1;dirtyMask|=0x44;}else
		if( vz>=maxIdx)		{fz =  1;dirtyMask|=0x04;}
		
//		if( vx< minIdx&&cx>0)									{fx = -1;dirtyMask|=0x11;}else
//		if( vx>=maxIdx&&cx<Block45Constants._worldMaxCX-1)		{fx =  1;dirtyMask|=0x01;}
//		if( vy< minIdx&&cy>0)									{fy = -1;dirtyMask|=0x22;}else
//		if( vy>=maxIdx&&cy<Block45Constants._worldMaxCY-1)		{fy =  1;dirtyMask|=0x02;}
//		if( vz< minIdx&&cz>0)									{fz = -1;dirtyMask|=0x44;}else
//		if( vz>=maxIdx&&cz<Block45Constants._worldMaxCZ-1)		{fz =  1;dirtyMask|=0x04;}
//		
		
		if(dirtyMask != 0x80)
		{
			for(int i = 1; i < 8; i++)
			{
				if((dirtyMask&i)==i)
				{
					int dx = fx*nearChunkOfs[i,0], dy = fy*nearChunkOfs[i,1], dz = fz*nearChunkOfs[i,2];
					cxround = (cx+dx);
					cyround = (cy+dy);
					czround = (cz+dz);
					
					index = new IntVector4(cxround,cyround,czround, lod);
					
					if(isOutOfMeshDistance(index.XYZ))
					{
						writeToBlockVectors(index.XYZ, new IntVector3(
							vx-dx*Block45Constants._numVoxelsPerAxis,
							vy-dy*Block45Constants._numVoxelsPerAxis,
							vz-dz*Block45Constants._numVoxelsPerAxis), voxel.blockType, voxel.materialType);
					}
					else
					{
						if(!_chunks.ContainsKey(index))
							_chunks[index] = CreateChunk(index);
						
						try
						{
							if(!_chunks[index].WriteVoxelAtIdx(
									vx-dx*Block45Constants._numVoxelsPerAxis,
									vy-dy*Block45Constants._numVoxelsPerAxis, 
									vz-dz*Block45Constants._numVoxelsPerAxis, voxel))
							{
								dirtyMask |= i<<8;	// flag for unsuccessful write
							}
						}catch(Exception)
						{
							Debug.LogError("Unexpected block45 write at("+cxround+","+cyround+","+czround+")");
						}
					}
				}
			}
		}
		return dirtyMask;
	}
	void writeToBlockVectors(IntVector3 index, IntVector3 localIndex, byte b0, byte b1)
	{
		IntVector3 shiftVec = IntVector3.Zero;
		shiftVec.x = index.x << Block45Constants._shift;
		shiftVec.y = index.y << Block45Constants._shift;
		shiftVec.z = index.z << Block45Constants._shift;

		try{	// Add try for StackOverflowException
			BlockVectorNode newRootNode = bvtRoot.reroot(shiftVec);
			bvtRoot = newRootNode;
		}
		catch(Exception e)
		{
			Debug.LogWarning("Unexpected exception while writing block to"+index+e);
			return;
		}
		
		BlockVectorNode bvNode = BlockVectorNode.readNode(shiftVec, bvtRoot);
		
		bvNode.write(localIndex.x + Block45Constants._numVoxelsPrefix, localIndex.y + Block45Constants._numVoxelsPrefix, localIndex.z + Block45Constants._numVoxelsPrefix, b0, b1);
	}
	public B45Block SafeRead(int x, int y, int z, int lod = 0){
		return Read(x,y,z,lod);
	}
	public bool SafeWrite(int x, int y, int z, B45Block voxel, int lod = 0){
		Write(x,y,z,voxel,lod);
		return true;
	}
	
	// clear
	public void Clear()
	{
		BlockVectorNode.Clear(bvtRoot);
		B45LODNode.merge((B45LODNode)bvtRoot);
		_chunks.Clear();
		
	}
	int countpoints()
	{
		int sum = 0;
		foreach(KeyValuePair<IntVector4, B45ChunkData> kvp in _chunks)
		{
			sum += kvp.Value.getFillRate();
		}
		return sum;
	}
	public void OctreeUpdate(IntVector3 cursorPos)
	{
		List<BlockVectorNode> blkNodeToConv = new List<BlockVectorNode>();
		
		// check if there are block vector nodes that should be converted into lod chunks.
//		BlockVectorNode.rec_findLod(cursorPos, bvtRoot, blkNodeToConv);
//		for(int i = 0; i < blkNodeToConv.Count; i++){
//			BlockVectorNode bvNode = blkNodeToConv[i];
//			
//			// shift xyz
//			IntVector4 shiftXYZ = IntVector4.Zero;
//			shiftXYZ.x = bvNode.Pos.x >> Block45Constants._shift;
//			shiftXYZ.y = bvNode.Pos.y >> Block45Constants._shift;
//			shiftXYZ.z = bvNode.Pos.z >> Block45Constants._shift;
//			shiftXYZ.w = bvNode.Pos.w;
//
//			
//			if(!_chunks.ContainsKey(shiftXYZ) )
//			{
//				
//				B45ChunkData newChunk = CreateChunk(shiftXYZ);
//				
//				bvNode.chunk = newChunk;
//				newChunk._bvNode = bvNode;
//				
//				_chunks[shiftXYZ] = newChunk;
//				newChunk.bp();
//			}
//			if(bvNode.blockVectors != null && bvNode.chunk != null){
//				bvNode.chunk.bp();
//				try
//				{
//				BlockVectorNode.BlockVectors2ChunkData(bvNode.blockVectors, bvNode.chunk.DataVT);
//				}
//				catch(Exception ex){
//					int sdkf = 0;
//				}
//				bvNode.blockVectors.Clear();
//				bvNode.blockVectors = null;
//			}
//		}
		
		// check if there are block vector nodes that should be converted into real chunks(byte array mode).
		BlockVectorNode.rec_find(cursorPos, bvtRoot, blkNodeToConv);
		
//		for(int i = 0; i < blkNodeToConv.Count; i++){
////			blkNodeToConv[i].makeCube();
//		}
////		B45LODNode.makeCubeRec(bvtRoot as B45LODNode);
		
		int elementCount1=0;
		int fills1=0;
		int fillsIn_Chunks1=0;
		
		if(b45Building.DebugMode)
		{
			elementCount1 = BlockVectorNode.rec_count(bvtRoot);
			fills1 = BlockVectorNode.rec_count_dbg(bvtRoot);
			fillsIn_Chunks1 = countpoints();
		}
		for(int i = 0; i < blkNodeToConv.Count; i++){
			BlockVectorNode bvNode = blkNodeToConv[i];
			
			// shift xyz
			IntVector4 shiftXYZ = IntVector4.Zero;
			shiftXYZ.x = bvNode.Pos.x >> Block45Constants._shift;
			shiftXYZ.y = bvNode.Pos.y >> Block45Constants._shift;
			shiftXYZ.z = bvNode.Pos.z >> Block45Constants._shift;

			
			if(!_chunks.ContainsKey(shiftXYZ) )
			{
				
				B45ChunkData newChunk = CreateChunk(shiftXYZ);
				
				bvNode.chunk = newChunk;
				newChunk._bvNode = bvNode;
				
				_chunks[shiftXYZ] = newChunk;
				newChunk.bp();
			}
			if(bvNode.blockVectors != null && bvNode.chunk != null){
				bvNode.chunk.bp();
				try
				{
				BlockVectorNode.BlockVectors2ChunkData(bvNode.blockVectors, bvNode.chunk.DataVT);
				}
				catch{
					//int sdkf = 0;
				}
				bvNode.blockVectors.Clear();
				bvNode.blockVectors = null;
			}
		}
		
		if(b45Building.DebugMode)
		{
			int elementCount2 = BlockVectorNode.rec_count(bvtRoot);
			
			int fills2 = BlockVectorNode.rec_count_dbg(bvtRoot);
			int fillsIn_Chunks2 = countpoints();
			Debug.LogError("B45 Octree State: " + elementCount1 + " / " + fills1 + " / " + fillsIn_Chunks1 + " ------------- " + elementCount2 + " / " + fills2 + " / " + fillsIn_Chunks2);		// check for chunks that are out of view
		}
		//List<BlockVectorNode> bvToDispose = new List<BlockVectorNode>();
		List<B45ChunkData> cdToDispose = new List<B45ChunkData>();
		foreach(KeyValuePair<IntVector4, B45ChunkData> kvp in _chunks)
		{
			BlockVectorNode bvNode = kvp.Value._bvNode;
			B45ChunkData cd = kvp.Value;
			if(bvNode == null)
			{
				Debug.LogError("node null!");
			}
			if(!bvNode.isCloseTo(cursorPos)
				&&
				cd != null
				)
			{
				// this node is too far from the camera, put it in the dispose list
				cd.bp();
				cdToDispose.Add(cd);
//				_chunks.Remove(kvp.Key);
			}
		}
		for(int i = 0; i < cdToDispose.Count; i++){
			B45ChunkData cd = cdToDispose[i];
			// convert the chunk data from byte array mode to block vector mode
			
			if(cd != null && cd.isBeingDestroyed == false)
			{
				cd.setBeingDestroyed();
				cd.bp ();
				
				cd._bvNode.blockVectors = BlockVectorNode.ChunkData2BlockVectors(cd.DataVT);
				cd._bvNode.isByteArrayMode = false;
				cd._bvNode.removeCube();
				// remove this chunk from memory
				cd._bvNode.chunk = null;
				deferredRemoveList.Add(cd);

			}
			else{
				//int sdgf = 0;
			}
		}
		deferredRemoval();
	}
	
	void deferredRemoval(){
		for(int i = deferredRemoveList.Count - 1; i >= 0; i--){
			B45ChunkData cd = deferredRemoveList[i];
			cd.bp ();
//			if(colliderMan.isColliderBeingRebuilt(cd.ChunkPos.XYZ)){
//				Debug.LogError("the replacement collider is not done rebuilding.");
//			}
			if(cd!= null && cd.isInQueue == false && colliderMan.isColliderBeingRebuilt(cd.ChunkPos.XYZ) == false)
			{
				cd.bp ();
				if(_chunks.Remove(new IntVector4(cd.ChunkPos.XYZ,0)) == false)
				{
					Debug.LogError("a chunk can not be removed from the dictionary");
				}
				cd.ClearMem();
				cd.DestroyChunkGO();
				deferredRemoveList.RemoveAt(i);
			}
		}
		
	}
	List<B45ChunkData> deferredRemoveList = new List<B45ChunkData>();
	public void ConvertAllToBlockVectors()
	{
		List<BlockVectorNode> chunksToConvert = new List<BlockVectorNode>();
		foreach(KeyValuePair<IntVector4, B45ChunkData> kvp in _chunks)
		{
			BlockVectorNode bvNode = kvp.Value._bvNode;
			if(bvNode == null)
			{
				Debug.LogError("node null!");
			}
			chunksToConvert.Add(bvNode);
		}
		for(int i = 0; i < chunksToConvert.Count; i++){
			BlockVectorNode bvNode = chunksToConvert[i];
			B45ChunkData cd = bvNode.chunk;
			// convert the chunk data from byte array mode to block vector mode
			//if((cd.d_state & B45ChunkData.DS_BEING_DESTROYED) > 0)
			if(cd != null){
				cd.bp();
				bvNode.blockVectors = BlockVectorNode.ChunkData2BlockVectors(cd.DataVT);
			}
		}
	}
	B45ChunkData CreateChunk(IntVector4 index)
	{
		B45ChunkData chunk = new B45ChunkData(colliderMan);
		
		chunk.BuildList = _chunkRebuildList;
        writeChunk( index.x, index.y, index.z, chunk );
		
		chunk.ChunkPosLod_w = new IntVector4(
			(index.x), 
			(index.y), 
			(index.z), 0);
		chunk.bp();
		chunk.AddToBuildList();
		
		// make the bv node.
		if(true){
			IntVector3 shiftVec = IntVector3.Zero;
			shiftVec.x = index.x << Block45Constants._shift;
			shiftVec.y = index.y << Block45Constants._shift;
			shiftVec.z = index.z << Block45Constants._shift;

			try{	// Add try for StackOverflowException
				BlockVectorNode newRootNode = bvtRoot.reroot(shiftVec);
				bvtRoot = newRootNode;
			}
			catch(Exception e)
			{
				Debug.LogWarning("Unexpected exception while creating chunk to"+index+e);
				return chunk;
			}

			BlockVectorNode bvNode = BlockVectorNode.readNode(shiftVec, bvtRoot);
			if(bvNode.chunk != null)
			{
				// already a chunk has been assigned to this node. something is wrong here.
				return chunk;
				
			}
			bvNode.chunk = chunk;
			chunk._bvNode = bvNode;
			bvNode.isByteArrayMode = true;
		}
		
		
		return chunk;
	}
}

