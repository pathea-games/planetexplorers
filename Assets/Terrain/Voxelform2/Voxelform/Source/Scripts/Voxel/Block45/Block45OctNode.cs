using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public struct BlockVec
{
	public int _xyz;
	public byte _byte0, _byte1;
	public int x {get{return _xyz & 0xFF;}}
	public int y {get{return (_xyz>>8) & 0xFF;}}
	public int z {get{return (_xyz>>16) & 0xFF;}}
	public BlockVec(int x, int y, int z, byte b0, byte b1)
	{
		_xyz =	ToXYZ(x, y, z);		
		_byte0 = b0;
		_byte1 = b1;
	}
	public BlockVec(int xyz, byte b0, byte b1)
	{
		_xyz =	xyz;		
		_byte0 = b0;
		_byte1 = b1;
	}
	public static int ToXYZ(int x, int y, int z)
	{
		return 	(x & 0xFF) + 
				((y & 0xFF) << 8) +
				((z & 0xFF) << 16);	
	}
}

public partial class Block45OctNode
{
	public readonly static int[,] S_NearNodeOfs = new int[8,3]{ {0,0,0},{1,0,0},{0,1,0},{1,1,0},{0,0,1},{1,0,1},{0,1,1},{1,1,1}, };
	public readonly static int S_MinNoDirtyIdx = Block45Constants._numVoxelsPostfix;
	public readonly static int S_MaxNoDirtyIdx = Block45Constants._numVoxelsPerAxis - Block45Constants._numVoxelsPrefix;

	// the xyz components of pos stores the left-bottom-most point in the cubic area.
	// the w component is the lod level.
	// in logical space
	public IntVector4 		_pos;
	public Block45OctNode 	_parent;
	public Block45OctNode[] _children;

	private List<BlockVec> 	_vecData;
	public List<BlockVec> VecData{ 	get{ return _children == null ? _vecData : GetVecDataFromChildren();	}}
	public Action<Block45OctNode> OnCreateNode{ get; set; }
//	public IntVector4 Pos{			get{	return _pos;					}}
	public bool IsLeaf{				get{	return _children == null;		}}
	public int ScaledSize{			get{	return (1<<(_pos.w + Block45Constants._scaledShift));		}}

	// Node : should keep atpos stepped by 1<<Block45Constants._shift
	private Block45OctNode(){}
	public static Block45OctNode CreateNode(IntVector4 atpos, Block45OctNode parent)
	{
		Block45OctNode node = new Block45OctNode();
		node._pos = atpos;
		node._parent = parent;
		if(parent != null)				node.OnCreateNode = parent.OnCreateNode;
		if(node.OnCreateNode != null)	node.OnCreateNode(node);
		return node;
	}
	public static Block45OctNode CreateNode(IntVector4 atpos, Action<Block45OctNode> onCreateNode)	// root node
	{
		Block45OctNode node = new Block45OctNode();
		node._pos = atpos;
		node._parent = null;
		node.OnCreateNode = onCreateNode;
		if(node.OnCreateNode != null)	node.OnCreateNode(node);
		return node;
	}

	// Use to check if parent is dirty which means when their children change they do not update these changes yet.
	public int GetChildStamp()
	{
		if(_children != null)
		{
			int sumStamp = 0;
			for( int i = 0; i < 8; i++)
			{
				sumStamp += _children[i].GetChildStamp();
			}
			return sumStamp;
		}

		return _stamp;
	}
	public List<BlockVec> GetVecDataFromChildren()
	{
		int sumStamp = GetChildStamp();
		if(sumStamp != _stamp)
		{
			GenVecDataFromChildren(this);
			_stamp = sumStamp;
		}
		return  _vecData;
	}
	// use this to determine if a pos is inside the range of the node
	public bool Covers(IntVector4 pos)
	{
		int size = Block45Constants.Size(_pos.w);
		if(_pos.x <= pos.x && _pos.x + size > pos.x &&
		   _pos.y <= pos.y && _pos.y + size > pos.y &&
		   _pos.z <= pos.z && _pos.z + size > pos.z 
		   ){
			return true;			
		}
		return false;
	}
	public bool IsCenterInside(IntVector3 boundPos, int boundSize) //Is Node center inside of bound?
	{
		int centerOfs = Block45Constants.CenterOfs(_pos.w);
		if(boundPos.x <= _pos.x + centerOfs && boundPos.x + boundSize >= _pos.x + centerOfs &&
		   boundPos.y <= _pos.y + centerOfs && boundPos.y + boundSize >= _pos.y + centerOfs &&
		   boundPos.z <= _pos.z + centerOfs && boundPos.z + boundSize >= _pos.z + centerOfs 
		   ){
			return true;			
		}
		return false;
	}
	public bool IsCenterInside(IntVector3 boundPos, IntVector3 boundSize) //Is Node center inside of bound?
	{
		int centerOfs = Block45Constants.CenterOfs(_pos.w);
		if(boundPos.x <= _pos.x + centerOfs && boundPos.x + boundSize.x >= _pos.x + centerOfs &&
		   boundPos.y <= _pos.y + centerOfs && boundPos.y + boundSize.y >= _pos.y + centerOfs &&
		   boundPos.z <= _pos.z + centerOfs && boundPos.z + boundSize.z >= _pos.z + centerOfs 
		   ){
			return true;			
		}
		return false;
	}
	public bool IsWholeInside(IntVector3 boundPos, IntVector3 boundSize) //Is Node whole inside of bound?
	{
		int size = Block45Constants.Size(_pos.w);
		if(boundPos.x <= _pos.x && boundPos.x + boundSize.x >= _pos.x + size &&
		   boundPos.y <= _pos.y && boundPos.y + boundSize.y >= _pos.y + size &&
		   boundPos.z <= _pos.z && boundPos.z + boundSize.z >= _pos.z + size 
		   ) {
			return true;
		}
		return false;
	}
	public bool IsOverlapped(IntVector3 boundPos, IntVector3 boundSize) //Is Node overlapped with bound?
	{
		int size = Block45Constants.Size(_pos.w);
		if(boundPos.x > _pos.x + size || boundPos.x + boundSize.x < _pos.x ||
		   boundPos.y > _pos.y + size || boundPos.y + boundSize.y < _pos.y ||
		   boundPos.z > _pos.z + size || boundPos.z + boundSize.z < _pos.z 
		   ) {
			return false;
		}
		return true;
	}
	private void DoSplit(int octant, Block45OctNode node)
	{
		_children = new Block45OctNode[8];
		int lod = _pos.w;
		int childScaledSize = ScaledSize >> 1;
		for (int i = 0; i < 8; i++) {
			if (i == octant) {
				_children [octant] = node;
				continue;
			}
			
			IntVector4 apos = new IntVector4 (_pos);
			apos.w = lod - 1;
			apos.x += (i & 1) * childScaledSize;
			apos.y += ((i >> 1) & 1) * childScaledSize;
			apos.z += ((i >> 2) & 1) * childScaledSize;
			_children [i] = Block45OctNode.CreateNode (apos, this);
		}
	}
	public void Split(int octant = -1, Block45OctNode node = null)
	{
		if (NodeData != null) {
			lock (this.NodeData) {
				DoSplit(octant, node);
			}
		} else {
			DoSplit(octant, node);
		}
	}

	// this function should only be used by the root node.
	public Block45OctNode RerootToContainPos(IntVector4 pos)
	{
		if(!Covers(pos)){
			// make a new node that can cover atpos and pos is ALIGNED with LOD Node width
			// B45Node width should be equal to or smaller than LODNode width
			int maskX, maskY, maskZ;
			if(_pos.w < Block45Constants.MAX_LOD)
			{
				int posMask = (1<<(Block45Constants._scaledShift+_pos.w + 1)) - 1;
				maskX = ((_pos.x & posMask) != 0 && pos.x < _pos.x) ? 1 : 0;
				maskY = ((_pos.y & posMask) != 0 && pos.y < _pos.y) ? 1 : 0;
				maskZ = ((_pos.z & posMask) != 0 && pos.z < _pos.z) ? 1 : 0;
			}
			else
			{
				maskX = (pos.x < _pos.x) ? 1 : 0;
				maskY = (pos.y < _pos.y) ? 1 : 0;
				maskZ = (pos.z < _pos.z) ? 1 : 0;
			}
			int thisOctant = maskX + (maskY<<1) + (maskZ<<2);	// righttop to leftbottom
			IntVector4 newRootPos = new IntVector4(_pos.x - maskX*ScaledSize,
			                                       _pos.y - maskY*ScaledSize,
			                                       _pos.z - maskZ*ScaledSize,
			                                       _pos.w + 1);			
			Block45OctNode newRoot = Block45OctNode.CreateNode(newRootPos, OnCreateNode);
			this._parent = newRoot;
			newRoot.Split(thisOctant, this);
			
			return newRoot.RerootToContainPos(pos);
		}
		return this;
	}
	public Block45OctNode RerootToLOD(int lod)
	{
		if(LOD < lod)
		{
			int posMask = (1<<(Block45Constants._scaledShift+_pos.w + 1)) - 1;
			int maskX = (_pos.x & posMask) != 0 ? 1 : 0;
			int maskY = (_pos.y & posMask) != 0 ? 1 : 0;
			int maskZ = (_pos.z & posMask) != 0 ? 1 : 0;
			int thisOctant = maskX + (maskY<<1) + (maskZ<<2);	// righttop to leftbottom
			IntVector4 newRootPos = new IntVector4(_pos.x - maskX*ScaledSize,
			                                       _pos.y - maskY*ScaledSize,
			                                       _pos.z - maskZ*ScaledSize,
			                                       _pos.w + 1);
			Block45OctNode newRoot = Block45OctNode.CreateNode(newRootPos, OnCreateNode);
			this._parent = newRoot;
			newRoot.Split(thisOctant, this);

			return newRoot.RerootToLOD(lod);
		}
		return this;
	}
	public void Write(int x, int y, int z, byte b0, byte b1)
	{
		x += Block45Constants._numVoxelsPrefix; 
		y += Block45Constants._numVoxelsPrefix;
		z += Block45Constants._numVoxelsPrefix;
		if(_vecData != null)
		{
			int i = 0;
			for( ; i < _vecData.Count;i++){
				BlockVec bv = _vecData[i];
				if(x == bv.x && y == bv.y && z == bv.z){
					if(b0 == 0)
					{
						_vecData.RemoveAt(i);
					}
					else
					{
						bv._byte0 = b0;
						bv._byte1 = b1;
						_vecData[i] = bv;
					}
					break;
				}			
			}
			if(i >= _vecData.Count && b0 != 0)
			{
				_vecData.Add(new BlockVec(x,y,z, b0, b1));
			}
			_stamp++;
			WriteToByteArray(x, y, z, b0, b1);
		}
		else if(b0 != 0)
		{
			_vecData = new List<BlockVec>();
			_vecData.Add(new BlockVec(x,y,z, b0, b1));
			_stamp++;
			WriteToByteArray(x, y, z, b0, b1);
		}		
	}
	public B45Block Read(int x, int y, int z)
	{
		if(_vecData == null)	return new B45Block();

		x += Block45Constants._numVoxelsPrefix; 
		y += Block45Constants._numVoxelsPrefix;
		z += Block45Constants._numVoxelsPrefix;
		for(int i = 0; i < _vecData.Count;i++){
			BlockVec bv = _vecData[i];
			if(x == bv.x && y == bv.y && z == bv.z )
			{
				return new B45Block(bv._byte0, bv._byte1);
			}			
		}
		return new B45Block();
	}

	public byte[] BlockVecs2ByteArray()
	{
		byte[] chunkData = s_ChunkDataPool.Get();
		Array.Clear(chunkData, 0, chunkData.Length);
		for(int i = 0; i < _vecData.Count; i++ ){
			BlockVec bv = _vecData[i];
			int idx = Block45Kernel.OneIndexNoPrefix(bv.x, bv.y, bv.z);
			chunkData[idx] = bv._byte0;
			chunkData[idx + 1] = bv._byte1;
		}
		return chunkData;
	}
	public List<BlockVec> ByteArray2BlockVecs()
	{
		List<BlockVec> blvec = new List<BlockVec>();
		if(_chkData != null)
		{
			for(int z = 0; z < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; z++){
				for(int y = 0; y < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; y++){
					for(int x = 0; x < Block45Constants.VOXEL_ARRAY_AXIS_SIZE; x++){
						int idx = Block45Kernel.OneIndexNoPrefix(x, y, z);
						if(_chkData[idx] != 0){
							blvec.Add(new BlockVec(x,y,z, _chkData[idx], _chkData[idx+1]));
						}
					}
				}
			}
		}
		return blvec;
	}

#region StaticUtil
	public static Block45OctNode GetNodeRO(IntVector4 poslod/*w unused*/, Block45OctNode root)	// Read only
	{
		if(root == null || !root.Covers(poslod))	return null;
		if(root._pos.w <  poslod.w)					return null;
		if(root._pos.w == poslod.w)					return root;

		int ind = 0;
		int nodeCenterX = 0;
		int nodeCenterY = 0;
		int nodeCenterZ = 0;
		Block45OctNode cur = root;
		while(cur._pos.w > poslod.w)
		{
			if(cur.IsLeaf)							return null;

			int centerOfs = Block45Constants.CenterOfs(cur._pos.w);
			nodeCenterX = cur._pos.x + centerOfs;
			nodeCenterY = cur._pos.y + centerOfs;
			nodeCenterZ = cur._pos.z + centerOfs;			
			ind = 	((poslod.x >= nodeCenterX) ? 1 : 0) |
					((poslod.y >= nodeCenterY) ? 2 : 0) |
					((poslod.z >= nodeCenterZ) ? 4 : 0);			
			cur = cur._children[ind];
		}
		return cur;
	}
	public static Block45OctNode GetNodeRW(IntVector4 poslod, ref Block45OctNode root)	// Write new if null
	{
		// Note: if req's w > root.w, root would be returned
		root = root.RerootToContainPos(poslod);
		int ind = 0;
		int nodeCenterX = 0;
		int nodeCenterY = 0;
		int nodeCenterZ = 0;
		Block45OctNode cur = root;
		while(cur._pos.w > poslod.w)
		{
			if(cur.IsLeaf)
			{
				cur.Split();
			}

			int centerOfs = Block45Constants.CenterOfs(cur._pos.w);
			nodeCenterX = cur._pos.x + centerOfs;
			nodeCenterY = cur._pos.y + centerOfs;
			nodeCenterZ = cur._pos.z + centerOfs;			
			ind = 	((poslod.x >= nodeCenterX) ? 1 : 0) |
					((poslod.y >= nodeCenterY) ? 2 : 0) |
					((poslod.z >= nodeCenterZ) ? 4 : 0);			
			cur = cur._children[ind];
		}
		return cur;
	}
	public static void SplitAt(Block45OctNode root, IntVector3 atpos, int lod)
	{
		// calculate the index of the child in which the split will happen.
		int ind = 0;
		Block45OctNode cur = root;
		int nodeCenterX = 0;
		int nodeCenterY = 0;
		int nodeCenterZ = 0;
		
		for(int i = 0; i < lod; i++){
			int centerOfs = Block45Constants.CenterOfs(cur._pos.w);
			nodeCenterX = cur._pos.x + centerOfs;
			nodeCenterY = cur._pos.y + centerOfs;
			nodeCenterZ = cur._pos.z + centerOfs;			
			ind = 	((atpos.x > nodeCenterX) ? 1 : 0) |
					((atpos.y > nodeCenterY) ? 2 : 0) |
					((atpos.z > nodeCenterZ) ? 4 : 0);			
			if(cur.IsLeaf){
				cur.Split();
			}
			cur = cur._children[ind];
		}		
	}
	public static void Merge(Block45OctNode node)
	{
		if(!node.IsLeaf){
			for(int i = 0; i < 8; i++){
				if(node._children[i] != null){
					Merge(node._children[i]);
				}
				node._children[i] = null;
			}
		}
		node._children = null;
	}
	public static void FindNodesCenterInside(IntVector3 boundPos, int boundSize, int lod, Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == lod) {
			if (root.IsCenterInside (boundPos, boundSize)) {
				outNodesList.Add (root);
			}
			return;
		} else if (root._pos.w > lod) {
			if(!root.IsLeaf) {
				for (int i = 0; i < 8; i++) {
					FindNodesCenterInside (boundPos, boundSize, lod, root._children [i], ref outNodesList);
				}
			}
		}
	}
	public static void Clear(Block45OctNode node)
	{
		if(node._vecData != null){
			node._vecData = null;
		}
		node.FreeChkData();
		if(node._goChunk != null)
		{
			VFGoPool<Block45ChunkGo>.FreeGo(node._goChunk);
			node._goChunk = null;
		}
		if(!node.IsLeaf){
			for(int i = 0; i < 8; i++){
				Clear (node._children[i]);
			}
		}
	}
	// tmp algo
	public static void GenVecDataFromChildren(Block45OctNode node)
	{
		Dictionary<int, BlockVec> vecData = new Dictionary<int, BlockVec>();
		for(int i = 0; i < 8; i++){
			if(node._children[i]._vecData == null && !node._children[i].IsLeaf)
			{
				GenVecDataFromChildren(node._children[i]);
			}

			List<BlockVec> vecDataChild = node._children[i]._vecData;
			if(vecDataChild == null)	continue;

			int ofsxyz = BlockVec.ToXYZ(S_NearNodeOfs[i,0] << Block45Constants._shift,
			                            S_NearNodeOfs[i,1] << Block45Constants._shift,
			                            S_NearNodeOfs[i,2] << Block45Constants._shift);
			int n = vecDataChild.Count;
			for(int j = 0; j < n; j++)
			{
				int xyz = ((vecDataChild[j]._xyz + ofsxyz) >> 1) & 0x7F7F7F;
				vecData[xyz] = new BlockVec(xyz, vecDataChild[j]._byte0, vecDataChild[j]._byte1);
			}			
		}

		node._vecData = new List<BlockVec>(vecData.Values);
		node.FreeChkData();
	}
#endregion

#region DebugFunc
	// for visualization
	public GameObject cube;
	GameObject cubeGO;
	public void MakeCube()
	{
		Vector3 cubePos;
		int size = ScaledSize;
		int centerOfs = size>>1;
		cubePos = _pos.ToVector3();
		cubePos.x += centerOfs;
		cubePos.y += centerOfs;
		cubePos.z += centerOfs;
		
		if(cubeGO == null ){}
		//cubeGO = GameObject.Instantiate(Block45Man.self.VizCube, cubePos, Quaternion.identity) as GameObject;
		
		cubeGO.transform.localScale = new Vector3(size, size, size);
		
	}
	public void RemoveCube()
	{
		if(cubeGO != null)
		{
			GameObject.DestroyImmediate(cubeGO);
			cubeGO = null;
		}
	}
	public static void MakeCubeRec(Block45OctNode node)
	{
		node.MakeCube();
		if(!node.IsLeaf){
			for(int i = 0; i < 8; i++)
			{
				MakeCubeRec(node._children[i]);
			}
		}
	}
#endregion
}
