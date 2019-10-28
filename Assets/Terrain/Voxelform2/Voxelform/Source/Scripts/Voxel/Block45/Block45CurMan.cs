using UnityEngine;
//using UnityEditor;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class Block45CurMan : MonoBehaviour
{
	public static Block45CurMan self = null;
	public Material[] _b45Materials;
	private Block45OctDataSource _dataSource;
	private List<Block45OctNode> _octNodesToBuild = new List<Block45OctNode>();
	public Block45OctDataSource DataSource{ get{ return _dataSource; } }
	public bool IsInRebuilding{	get{ return SurfExtractorsMan.B45BuildSurfExtractor != null && !SurfExtractorsMan.B45BuildSurfExtractor.IsAllClear; }	}

	void Awake()
	{
		if(self == null)
		{
			self = this;
		}
		_dataSource = new Block45OctDataSource(AddNodeToBuildList);
	}
	void AddNodeToBuildList(Block45OctNode node)
	{
		if(node.LOD == 0)
		{
			_octNodesToBuild.Add(node);
		}
	}
	public void RebuildMesh()
	{
		if(SurfExtractorsMan.B45BuildSurfExtractor != null)
		{
			foreach(Block45OctNode octNode in _octNodesToBuild)
			{
				SurfExtractorsMan.B45BuildSurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(octNode.GetStamp(), octNode, ChunkProcPostGenMesh));
			}
		}
	}
	public void AlterBlockInBuild(int vx, int vy, int vz, B45Block blk)
	{
		DataSource.SafeWrite(blk, vx, vy, vz, 0);
	}
	void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqB45 req = ireq as SurfExtractReqB45;
		Block45ChunkGo b45Go = Block45ChunkGo.CreateChunkGo(req, transform);
		req._chunkData.AttachChunkGo(b45Go);
	}
}
