using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

// All computation referring to node uses SCALED coord
// All computation referring to voxel uses UnSCALED coord
public class Block45OctDataSource
{
	// chunk size 8*8*8
	private Block45OctNode _octRoot;
	private Action<Block45OctNode> _onCreateNode;

	public Block45OctNode RootNode{	get{ return _octRoot;	}	}
	public Block45OctDataSource(Action<Block45OctNode> onCreateNode = null)
	{
		_onCreateNode = onCreateNode;
	}

#region Save Load and Utils
	// append all children(inclusive) with lod recursively for savedata
	private static int AppendToWrite(Block45OctNode node, BinaryWriter bw, int lod)
	{	
		int count = 0;
		if(node._pos.w == lod){
			List<BlockVec> vecData = node.VecData;
			if( vecData != null){	
				IntVector4 blockUnitPos = Block45Constants.ToBlockUnitPos(node._pos.x, node._pos.y, node._pos.z, node._pos.w);
				int n = vecData.Count;
				for(int i = 0; i < n; i++){
					BlockVec bv = vecData[i];
					int x = bv.x - Block45Constants._numVoxelsPrefix;
					int y = bv.y - Block45Constants._numVoxelsPrefix;
					int z = bv.z - Block45Constants._numVoxelsPrefix;
					if(x < 0 || x >= Block45Constants._numVoxelsPerAxis ||
					   y < 0 || y >= Block45Constants._numVoxelsPerAxis ||
					   z < 0 || z >= Block45Constants._numVoxelsPerAxis){
						continue;
					}
					bw.Write(blockUnitPos.x + x);
					bw.Write(blockUnitPos.y + y);
					bw.Write(blockUnitPos.z + z);
					bw.Write(bv._byte0);
					bw.Write(bv._byte1);
					count++;
				}
			}
		}
		else 
		if(node._pos.w > lod && node._children != null){
			for(int i = 0; i < 8; i++){
				count += AppendToWrite(node._children[i], bw, lod);
			}
		}
		return count;
	}
	public void Import(BinaryReader br)
	{
		int dataVer = br.ReadInt32();
		switch(dataVer)
		{
		case 2:
			int cnt = br.ReadInt32();
			int x, y, z;
			B45Block voxel;
			for(int i = 0; i < cnt; i++)
			{
				x = br.ReadInt32();
				y = br.ReadInt32();
				z = br.ReadInt32();
				voxel.blockType = br.ReadByte();
				voxel.materialType = br.ReadByte();
				Write(voxel, x, y, z);
			}
			break;
		}
	}
	public void Export(BinaryWriter bw)
	{
		int dataVer = 2;
		bw.Write(dataVer);
		long addr = bw.BaseStream.Position;
		bw.Write(0);	// cnt placeholder
		if(_octRoot != null)
		{
			switch(dataVer)
			{
			case 2:
				int elementCount = AppendToWrite(_octRoot, bw, 0);
				bw.Seek((int)addr, SeekOrigin.Begin);
				bw.Write(elementCount);
				break;
			}
		}
	}
	public void ImportData(byte[] b45Data)
	{
		MemoryStream ms = new MemoryStream(b45Data);
		BinaryReader br = new BinaryReader(ms);
		Import(br);		
		br.Close();
		ms.Close();
	}
	public byte[] ExportData()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(ms);
		Export(bw);
		bw.Close();
		ms.Close();
		return ms.ToArray();
	}
#endregion

#region Read/Write : use scaled coordination(lod0 block unit)
	public B45Block Read(int x, int y, int z, int lod = 0)
	{
		IntVector4 poslod = Block45Constants.ToWorldUnitPos(x,y,z, lod);
		Block45OctNode curNode = Block45OctNode.GetNodeRO(poslod, _octRoot);
		if(curNode == null)
			return new B45Block(0, 0);

		int vx = (x>>lod)&Block45Constants._mask;
		int vy = (y>>lod)&Block45Constants._mask;
		int vz = (z>>lod)&Block45Constants._mask;
		return curNode.Read(vx,vy,vz);
	}
	public int Write(B45Block voxel, int x, int y, int z, int lod = 0)				// logic pos
	{
		int vx = (x>>lod)&Block45Constants._mask;
		int vy = (y>>lod)&Block45Constants._mask;
		int vz = (z>>lod)&Block45Constants._mask;
		//Debug.LogWarning("[BlockWrite]:"+x+","+y+","+z+","+voxel.blockType+","+voxel.materialType);
		
		IntVector4 poslod;
		Block45OctNode curNode;
		if(_octRoot == null)
		{
			poslod = Block45Constants.ToWorldUnitPos(x&~Block45Constants._mask,
			                                      y&~Block45Constants._mask,
			                                      z&~Block45Constants._mask,
			                                      lod);
			IntVector4 rootPosLod = new IntVector4(poslod);
			curNode = _octRoot = Block45OctNode.CreateNode(rootPosLod, _onCreateNode);
			//Extend root node if root.lod < VoxelTerrainConstans.MaxLOD
			_octRoot = _octRoot.RerootToLOD(LODOctreeMan._maxLod);
		}
		else
		{
			poslod = Block45Constants.ToWorldUnitPos(x,y,z, lod);
			curNode = Block45OctNode.GetNodeRW(poslod, ref _octRoot);
		}
		curNode.Write(vx, vy, vz, voxel.blockType, voxel.materialType);

		// Write neighbour
		int fx = 0, fy = 0, fz = 0;
		int dirtyMask = 0x80;	// 0,1,2 bit for xyz dirty mask;4,5,6 bit for sign(neg->1);7 bit for current pos(now not used)
		
		// If write one edge's voxel may cause the other edge being modified
		if( vx< Block45OctNode.S_MinNoDirtyIdx)	{fx = -1;dirtyMask|=0x11;}else
		if( vx>=Block45OctNode.S_MaxNoDirtyIdx)	{fx =  1;dirtyMask|=0x01;}
		if( vy< Block45OctNode.S_MinNoDirtyIdx)	{fy = -1;dirtyMask|=0x22;}else
		if( vy>=Block45OctNode.S_MaxNoDirtyIdx)	{fy =  1;dirtyMask|=0x02;}
		if( vz< Block45OctNode.S_MinNoDirtyIdx)	{fz = -1;dirtyMask|=0x44;}else
		if( vz>=Block45OctNode.S_MaxNoDirtyIdx)	{fz =  1;dirtyMask|=0x04;}

		if(dirtyMask != 0x80)
		{
			int _shift = Block45Constants._shift;
			int cxlod = (x>>(lod+_shift));
			int cylod = (y>>(lod+_shift));
			int czlod = (z>>(lod+_shift));
			int cxround;
			int cyround;
			int czround;
			for(int i = 1; i < 8; i++)
			{
				if((dirtyMask&i)==i)
				{
					int dx = fx*Block45OctNode.S_NearNodeOfs[i,0], dy = fy*Block45OctNode.S_NearNodeOfs[i,1], dz = fz*Block45OctNode.S_NearNodeOfs[i,2];
					cxround = (cxlod+dx);
					cyround = (cylod+dy);
					czround = (czlod+dz);

					poslod.x = cxround<<(Block45Constants._scaledShift+lod);
					poslod.y = cyround<<(Block45Constants._scaledShift+lod);
					poslod.z = czround<<(Block45Constants._scaledShift+lod);
					Block45OctNode nearNode = Block45OctNode.GetNodeRW(poslod, ref _octRoot);
					nearNode.Write(vx-dx*Block45Constants._numVoxelsPerAxis,
					               vy-dy*Block45Constants._numVoxelsPerAxis,
					               vz-dz*Block45Constants._numVoxelsPerAxis,
					               voxel.blockType, voxel.materialType);
				}
			}
		}
		return dirtyMask;
	}
	public B45Block SafeRead(int x, int y, int z, int lod = 0){
		return Read(x,y,z,lod);
	}
	public bool SafeWrite(B45Block voxel, int x, int y, int z, int lod = 0){
		Write(voxel, x,y,z,lod);
		return true;
	}
#endregion

#region Helper
	public bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<B45Block> blocks)
	{
		B45Block ori = Read(pos.x, pos.y, pos.z, pos.w);
		if(!ori.IsExtendable())
		{
			posList = null;
			blocks = null;
			return false;
		}

		int e = ori.materialType&3;
		int EDx = Block45Kernel._2BitToExDir[e * 3];
		int EDy = Block45Kernel._2BitToExDir[e * 3 + 1];
		int EDz = Block45Kernel._2BitToExDir[e * 3 + 2];
		int posx = pos.x;
		int posy = pos.y;
		int posz = pos.z;
		int posw = pos.w;
		if(!ori.IsExtendableRoot())
		{
			while(true)
			{
				posx -= EDx;
				posy -= EDy;
				posz -= EDz;
				ori = Read(posx, posy, posz, posw);
				if(!ori.IsExtendable())
				{
					Debug.LogError("[Block]Get root data error in ReadExtendableBlock:"+pos);
					posList = null;
					blocks = null;
					return false;
				}
				else if(ori.IsExtendableRoot())
				{
					break;
				}
			}
		}

		int len = (ori.materialType>>2) + 2;
		posList = new List<IntVector4>(len);
		blocks = new List<B45Block>(len);
		for(int i = 0; i < len; i++)
		{
			IntVector4 curPos = new IntVector4(posx+i*EDx, posy+i*EDy, posz+i*EDz, posw);
			posList.Add(curPos);
			blocks.Add(Read(curPos.x, curPos.y, curPos.z, curPos.w));
		}
		return true;
	}
#endregion

	// clear
	public void Clear()
	{
		if(_octRoot != null)
		{
			Block45OctNode.Clear(_octRoot);
			Block45OctNode.Merge(_octRoot);
		}
	}
}


