using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public struct BlockVector
{
	private int xyz;
	public byte byte0, byte1;
	public int x {get{return xyz & 0xFF;}}
	public int y {get{return (xyz>>8) & 0xFF;}}
	public int z {get{return (xyz>>16) & 0xFF;}}
	public BlockVector(int x, int y, int z, byte b0, byte b1)
	{
		xyz = 	(x & 0xFF) + 
				((y & 0xFF) << 8) +
				((z & 0xFF) << 16);

		byte0 = b0;
		byte1 = b1;
	}
}
public class BlockVectorNode : B45LODNode{
	public List<BlockVector> blockVectors;
	public B45ChunkData chunk;
	public bool isByteArrayMode;
	public BlockVectorNode(IntVector4 _pos, B45LODNode _parent, int _octant):base(_pos, _parent, _octant)
	{
		isByteArrayMode = false;
	}
	public static List<BlockVector> ChunkData2BlockVectors(byte[] chunkData)
	{
		//if(chunkData.Length == 0){
		//	int sdg = 0;
		//}
		List<BlockVector> blvec = new List<BlockVector>();
		for(int z = 0; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; z++){
			for(int y = 0; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; y++){
				for(int x = 0; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; x++){
					int idx = Block45Kernel.OneIndexNoPrefix(x, y, z);
					if(chunkData[idx] != 0)
					{
						blvec.Add(new BlockVector(x,y,z, chunkData[idx], chunkData[idx+1]));
					}
				}
			}
		}
		return blvec;
	}
	public static void Clear(BlockVectorNode node)
	{
		if(node.blockVectors != null)
		{
			node.blockVectors.Clear();
			node.blockVectors = null;
		}
		if(node.chunk != null){
			node.chunk.DestroyGameObject();
			node.chunk = null;
		}
		if(!node.isLeaf){
			for(int i = 0; i < 8; i++){
				Clear (node.children[i] as BlockVectorNode);
			}
		}
	}
	public static void BlockVectors2ChunkData(List<BlockVector> list, byte[] chunkData)
	{
		Array.Clear(chunkData, 0, chunkData.Length);
		for(int i = 0; i < list.Count; i++ ){
			BlockVector bv = list[i];
			int idx = Block45Kernel.OneIndexNoPrefix(bv.x, bv.y, bv.z);
			chunkData[idx] = bv.byte0;
			chunkData[idx + 1] = bv.byte1;
		}
	}
	public static void BlockVectors2ChunkData(List<BlockVector> list, B45ChunkData chunkData)
	{
		
		for(int i = 0; i < list.Count; i++ ){
			BlockVector bv = list[i];
			chunkData.WriteVoxelAtIdx(bv.x, bv.y, bv.z, new B45Block(bv.byte0, bv.byte1));
		}
	}

	// count recursively

	public static int rec_count(BlockVectorNode node)
	{
		int count = 0;
		if(node.blockVectors != null)
			count = node.blockVectors.Count;
		if(node.isLeaf == false)
		{
			for(int i = 0; i < 8; i++){
				count += BlockVectorNode.rec_count(node.children[i] as BlockVectorNode);
			}
		}
		return count;
	}
	public static int rec_count_dbg(BlockVectorNode node)
	{
		int count = 0;
		if(node.chunk != null)
		{
			count = node.chunk.getFillRate();
		}
		if(node.isLeaf == false)
		{
			for(int i = 0; i < 8; i++){
				count += BlockVectorNode.rec_count_dbg(node.children[i] as BlockVectorNode);
			}
		}
		return count;
	}
	// append recursively
	public static void rec_append(BlockVectorNode node, BinaryWriter bw)
	{

		if(node.isLeaf)
		{
			if( node.blockVectors != null){

				for(int i = 0; i < node.blockVectors.Count; i++)
				{
					IntVector3 nodePos = new IntVector3(node.pos.XYZ);
					BlockVector bv = node.blockVectors[i];

					nodePos.x += bv.x - Block45Constants._numVoxelsPrefix;
					nodePos.y += bv.y - Block45Constants._numVoxelsPrefix;
					nodePos.z += bv.z - Block45Constants._numVoxelsPrefix;

					bw.Write(nodePos.x);
					bw.Write(nodePos.y);
					bw.Write(nodePos.z);
					bw.Write(bv.byte0);
					bw.Write(bv.byte1);
				}
				return;
			}
		}
		else{
			for(int i = 0; i < 8; i++)
			{
				rec_append(node.children[i] as BlockVectorNode, bw);
			}
		}

	}
	// atpos should be in logical space NOT in world coordinate.
	public bool isCloseTo(IntVector3 atpos)
	{
		// calculate the cube center.
		int half = logicalSize / 2;
		int ctrx = pos.x + half;
		int ctry = pos.y + half;
		int ctrz = pos.z + half;
		
		if(Mathf.Abs(ctrx - atpos.x) > Block45Constants._MeshDistanceX - half || 
			Mathf.Abs(ctry - atpos.y) > Block45Constants._MeshDistanceY - half || 
			Mathf.Abs(ctrz - atpos.z) > Block45Constants._MeshDistanceZ - half){
			// 
			return false;
			
		}
		
		return true;
	}
	public bool isInLodRange(IntVector3 atpos)
	{
		// calculate the cube center.
		int half = logicalSize / 2;
		int ctrx = pos.x + half;
		int ctry = pos.y + half;
		int ctrz = pos.z + half;
		if(Mathf.Abs(ctrx - atpos.x) > Block45Constants._MeshDistanceX - half || 
			Mathf.Abs(ctry - atpos.y) > Block45Constants._MeshDistanceY - half || 
			Mathf.Abs(ctrz - atpos.z) > Block45Constants._MeshDistanceZ - half){
			
			if(Mathf.Abs(ctrx - atpos.x) < Block45Constants._LodMaxX - half || 
				Mathf.Abs(ctry - atpos.y) < Block45Constants._LodMaxY - half || 
				Mathf.Abs(ctrz - atpos.z) < Block45Constants._LodMaxZ - half){
				// 
				return false;
			}
			
		}
		
		return false;
	}
	public static bool isCloseTo_static(IntVector3 nodePos, IntVector3 atpos)
	{
		// calculate the cube center.
		int half = Block45Constants._numVoxelsPerAxis / 2;
		int ctrx = nodePos.x + half;
		int ctry = nodePos.y + half;
		int ctrz = nodePos.z + half;
		
		if(Mathf.Abs(ctrx - atpos.x) > Block45Constants._MeshDistanceX - half || 
			Mathf.Abs(ctry - atpos.y) > Block45Constants._MeshDistanceY - half || 
			Mathf.Abs(ctrz - atpos.z) > Block45Constants._MeshDistanceZ - half){
			// 
			return false;
			
		}
		
		return true;
	}
	public static void rec_find(IntVector3 atpos, BlockVectorNode node, List<BlockVectorNode> ret)
	{
		
		if(node.isLeaf && node.pos.w == 0){
			if(node.isCloseTo(atpos))
			{
				ret.Add(node);
			}
		}
		if(node.isLeaf == false){
			for(int i = 0; i < 8; i++){
				rec_find(atpos, node.children[i] as BlockVectorNode, ret);
				
			}
		}
		
	}
	public static void rec_findLod(IntVector3 atpos, BlockVectorNode node, List<BlockVectorNode> ret)
	{
		
		if(node.isLeaf == false){
			if(node.isInLodRange(atpos))
			{
				ret.Add(node);
			}
		}
		if(node.isLeaf == false){
			for(int i = 0; i < 8; i++){
				rec_findLod(atpos, node.children[i] as BlockVectorNode, ret);
				
			}
		}
		
	}
	public void Split()
	{
		children = new BlockVectorNode[8];
		
		int lod = pos.w;
		int childLogicalSize = logicalSize >> 1;
		for(int i = 0; i < 8; i++){
			IntVector4 apos = new IntVector4(pos);
			
			apos.w = lod - 1;
			apos.x += (i & 1) * childLogicalSize;
			apos.y += ((i >> 1) & 1) * childLogicalSize;
			apos.z += ((i >> 2) & 1) * childLogicalSize;
			children[i] = new BlockVectorNode(apos, this, i);
			
		}
		
	}
	// use this to determine if a pos is inside the range of the node
	public bool covers(IntVector3 atpos)
	{
		if(atpos.x >= pos.x && atpos.x < pos.x + logicalSize &&
			atpos.y >= pos.y && atpos.y < pos.y + logicalSize &&
			atpos.z >= pos.z && atpos.z < pos.z + logicalSize 
			){
			return true;
			
		}
		return false;
	}
	// this function should only be used by the root node.
	public BlockVectorNode reroot(IntVector3 atpos){
		if(covers(atpos) == false){
			// make a new node that can cover atpos
			IntVector4 newRootPos = new IntVector4(pos.XYZ, pos.w + 1);
			int maskX = 0;
			int maskY = 0;
			int maskZ = 0;
			if(atpos.x < pos.x) maskX = 1;
			if(atpos.y < pos.y) maskY = 1;
			if(atpos.z < pos.z) maskZ = 1;
			newRootPos.x -= maskX * logicalSize;
			newRootPos.y -= maskY * logicalSize;
			newRootPos.z -= maskZ * logicalSize;
			
			BlockVectorNode newRoot = new BlockVectorNode(newRootPos, null, 0);
			this.parent = newRoot;
			newRoot.Split();
			
			int thisOctant = maskX + (maskY<<1) + (maskZ<<2);
			newRoot.children[thisOctant] = null;
			newRoot.children[thisOctant] = this;
			this.octant = thisOctant;
			
			return newRoot.reroot(atpos);
		}
		return this;
	}
	// obtain the level 0 node that associates with the given pos.
	public static BlockVectorNode readNode(IntVector3 atpos, BlockVectorNode root)
	{
		// calculate the index of the child in which the split will happen.
		int ind = 0;
		BlockVectorNode cur = root;
		IntVector3 nodeCenterPos = IntVector3.Zero;
		
		while(cur.pos.w != 0)
		{
			nodeCenterPos.x = cur.pos.x + cur.logicalSize / 2;
			nodeCenterPos.y = cur.pos.y + cur.logicalSize / 2;
			nodeCenterPos.z = cur.pos.z + cur.logicalSize / 2;
			
			ind = ((atpos.x >= nodeCenterPos.x) ? 1 : 0) |
				((atpos.y >= nodeCenterPos.y) ? 2 : 0) |
					((atpos.z >= nodeCenterPos.z) ? 4 : 0);
			
			if(cur.isLeaf)
			{
				cur.Split();
			}
			cur = cur.children[ind] as BlockVectorNode;

		}
		return cur;
	}
	public void write(int x, int y, int z, byte b0, byte b1)
	{
		// find out if it is an update or append.
		if(blockVectors == null){
			blockVectors = new List<BlockVector>();
		}
			
		
		bool found = false;
		for(int i = 0; i < blockVectors.Count;i++){
			BlockVector bv = blockVectors[i];
			if(x == bv.x && 
				y == bv.y && 
				z == bv.z )
			{
				bv.byte0 = b0;
				bv.byte1 = b1;
				found = true;
				break;
			}
			
		}
		if(found == false){
			blockVectors.Add(new BlockVector(x,y,z, b0, b1));
		}
		
	}
};