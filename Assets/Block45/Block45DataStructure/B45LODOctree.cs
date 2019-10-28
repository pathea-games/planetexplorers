using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
public class B45LODOctree{
	public B45LODOctree()
	{
	}
}

public class B45LODNode{
	
	// the xyz components of pos stores the left-bottom-most point in the cubic area.
	// the w component is the lod level.
	// in logical space
	
	protected IntVector4 pos;
	public IntVector4 Pos {get{return pos;}}
	
	protected B45LODNode[] children;
	protected B45LODNode parent;
	protected bool isLeaf
	{
		get
		{
			return children == null;
		}
	}
	protected int childmaxLod;
	protected int octant;

	public B45LODNode(IntVector4 _pos, B45LODNode _parent, int _octant){
		pos = new IntVector4(_pos);
		parent = _parent;
		octant = _octant;

	}

	public void split()
	{
		children = new B45LODNode[8];

		int lod = pos.w;
		int childLogicalSize = logicalSize >> 1;
		for(int i = 0; i < 8; i++){
			IntVector4 apos = new IntVector4(pos);
			
			apos.w = lod - 1;
			apos.x += (i & 1) * childLogicalSize;
			apos.y += ((i >> 1) & 1) * childLogicalSize;
			apos.z += ((i >> 2) & 1) * childLogicalSize;
			children[i] = new B45LODNode(apos, this, i);
			
		}
		
	}
	// obtain the level 0 node that associates with the given pos.
	public static B45LODNode readNode(IntVector3 atpos, B45LODNode root, int lod)
	{
		// calculate the index of the child in which the split will happen.
		int ind = 0;
		B45LODNode cur = root;
		IntVector3 nodeCenterPos = IntVector3.Zero;
		
		while(true)
		{
			nodeCenterPos.x = cur.pos.x + cur.logicalSize / 2;
			nodeCenterPos.y = cur.pos.y + cur.logicalSize / 2;
			nodeCenterPos.z = cur.pos.z + cur.logicalSize / 2;
			
			ind = ((atpos.x >= nodeCenterPos.x) ? 1 : 0) |
				((atpos.y >= nodeCenterPos.y) ? 2 : 0) |
					((atpos.z >= nodeCenterPos.z) ? 4 : 0);
			
			if(cur.isLeaf)
			{
				cur.split();
			}
			cur = cur.children[ind];
			if(cur.pos.w == 0)
				break;
		}
		return cur;
	}
	public static void splitAt(B45LODNode root, IntVector3 atpos, int lod)
	{
		// calculate the index of the child in which the split will happen.
		int ind = 0;
		B45LODNode cur = root;
		IntVector3 nodeCenterPos = IntVector3.Zero;
		
		int i = 0;
		for(; i < lod; i++){
			nodeCenterPos.x = cur.pos.x + cur.logicalSize;
			nodeCenterPos.y = cur.pos.y + cur.logicalSize;
			nodeCenterPos.z = cur.pos.z + cur.logicalSize;
			
			ind = ((atpos.x > nodeCenterPos.x) ? 1 : 0) |
						((atpos.y > nodeCenterPos.y) ? 2 : 0) |
						((atpos.z > nodeCenterPos.z) ? 4 : 0);
			
			if(cur.isLeaf)
			{
				cur.split();
			}
			cur = cur.children[ind];
		}		
	}
	public int physicalSize{	get{	return (1<<(pos.w)) * Block45Constants.ChunkPhysicalSize;	}	}
	public int logicalSize{	get{	return 1<<(pos.w + Block45Constants._shift);	}	}
	public static void merge(B45LODNode node)
	{
		if(!node.isLeaf){
			for(int i = 0; i < 8; i++)
			{
				if(node.children[i] != null)
				{
					merge(node.children[i]);
				}
				node.children[i] = null;
			}
		}
		node.children = null;
	}
	public void findNeighbour(int dirIdx){
		for(int i = 0; i < 8; i++){
			// find out if this node needs to split
			
			
		}
		
		
	}
	
	// for visualization
	public GameObject cube;
	GameObject cubeGO;
	public void makeCube()
	{
		
//		if(Block45Building.self.VizCube == null)
//			return;
		
		Vector3 cubePos;
		cubePos = pos.XYZ.ToVector3() * Block45Constants._scale;
		cubePos.x += physicalSize * Block45Constants._scale;
		cubePos.y += physicalSize * Block45Constants._scale;
		cubePos.z += physicalSize * Block45Constants._scale;
		
		if(cubeGO == null ){}
			//cubeGO = GameObject.Instantiate(Block45Building.self.VizCube, cubePos, Quaternion.identity) as GameObject;
		
		cubeGO.transform.localScale = new Vector3(physicalSize, physicalSize, physicalSize);
		
	}
	public void removeCube()
	{
		if(cubeGO != null)
		{
			GameObject.DestroyImmediate(cubeGO);
			cubeGO = null;
		}
	}
	public static void makeCubeRec(B45LODNode node)
	{
		node.makeCube();
		if(!node.isLeaf){
			for(int i = 0; i < 8; i++)
			{
				makeCubeRec(node.children[i]);
			}
		}
	}
	
}
