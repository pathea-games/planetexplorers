using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[RequireComponent (typeof (MSMainCamera))]
public class MSScan : MonoBehaviour {
	static MSScan mInstance;
	public static MSScan Instance{get{return mInstance;}}
	
	IVxDataSource _dataSource;
	public IntVector3 centerPos;
	public int radius;
	public Material mat;
	public bool bRefresh = false;
	public bool bInScanning = false;
	List<IntVector3> orderedOffsetList;
	public List<byte> mMatList;

	// Use this for initialization
	void Awake () 
	{
		radius = 80;
		mInstance = this;
		mMatList = new List<byte>();
	}
	
	void Update()
	{
		if(bRefresh)
		{
			bRefresh = false;
			_dataSource = VFVoxelTerrain.self.Voxels;
//			centerPos = GetComponent<MSMainCamera>()._posCenter.transform.position;
			if(null != GameUI.Instance.mMainPlayer)
			{
				centerPos = GameUI.Instance.mMainPlayer.position;
			}
			StartCoroutine(Scan());
		}
	}
	
	void ScanOneChunk(IntVector4 cposlod, out List<Vector3> outputVerts, out List<Color32> outputCols)
	{
		outputVerts = new List<Vector3>(); 
		outputCols = new List<Color32>();		
		VFVoxelChunkData cdata = _dataSource.readChunk(cposlod.x, cposlod.y, cposlod.z, cposlod.w);
		if(cdata == null)	return;
		byte[] data = cdata.DataVT;
		int nLen = data.Length;
		if(nLen == 0)		return;
		if(nLen == VFVoxel.c_VTSize)
		{
			if(mMatList.Contains(data[1]))
			{
				int nVerts = 1<<(VoxelTerrainConstants._shift*3);
				outputVerts = new List<Vector3>(nVerts);
				outputCols = new List<Color32>(nVerts);
				Vector3 point = new Vector3(0, 0, 0);
				Color32 col = MetalScanData.GetColorByType((byte)(data[0]*2));// new Color32((byte)(data[0]*2), 0, 0, 255);
				for(int z = 0; z < VoxelTerrainConstants._numVoxelsPerAxis; z++,point.z++)
				{
					for(int y = 0; y < VoxelTerrainConstants._numVoxelsPerAxis; y++,point.y++)
					{
						for(int x = 0; x < VoxelTerrainConstants._numVoxelsPerAxis; x++,point.x++)
						{
							outputVerts.Add(point);
							outputCols.Add(col);
						}
						point.x = 0;
					}
					point.y = 0;
				}
				return;
			}
			return;
		}
		else
		{
			int idx = (	 VoxelTerrainConstants._numVoxelsPrefix*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE + VoxelTerrainConstants._numVoxelsPrefix)
						*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE + VoxelTerrainConstants._numVoxelsPrefix;
			for(int z = 0; z < VoxelTerrainConstants._numVoxelsPerAxis; z++)
			{
				for(int y = 0; y < VoxelTerrainConstants._numVoxelsPerAxis; y++)
				{
					for(int x = 0; x < VoxelTerrainConstants._numVoxelsPerAxis; x++,idx++)
					{
						int idxBy = idx<<1;
						if(data[idxBy] >= 128)
						{
							byte curType = data[idxBy+1];
							Color col = MetalScanData.GetColorByType(curType);
							if(mMatList.Contains(curType))
							{
								outputVerts.Add(new Vector3(x, y, z)); 
								outputCols.Add(col);
								//outputCols.Add(new Color32(255, 0, 0, 255));
							}
						}
					}
					idx += VoxelTerrainConstants._numVoxelsPrefix + VoxelTerrainConstants._numVoxelsPostfix;
				}
				idx += (VoxelTerrainConstants._numVoxelsPrefix + VoxelTerrainConstants._numVoxelsPostfix)*VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE;
			}
		}
		return;
	}
	
	IEnumerator Scan()
	{
		bInScanning = true;
		Transform tParent = ResetGOs();
		
		IntVector4 vecChunkPosLD = new IntVector4(
										(centerPos.x-radius)>>VoxelTerrainConstants._shift,
										(centerPos.y-radius)>>VoxelTerrainConstants._shift,
										(centerPos.z-radius)>>VoxelTerrainConstants._shift,
										0);
		int nAxis = 1 + (radius*2 >> VoxelTerrainConstants._shift);
		for(int z = 0; z < nAxis; z++)
		{
			for(int y = 0; y < nAxis; y++)
			{
				for(int x = 0; x < nAxis; x++)
				{
					IntVector4 vecChunkPos = new IntVector4(vecChunkPosLD.x+x, vecChunkPosLD.y+y, vecChunkPosLD.z+z, 0);
					List<Vector3> outputVerts;
					List<Color32> outputCols;
					ScanOneChunk(vecChunkPos, out outputVerts, out outputCols);
					
					int nVerts = outputVerts.Count;
					if(nVerts > 0)
					{
						GameObject mineralPointsGo = new GameObject("MP_"+vecChunkPos);
						mineralPointsGo.layer = Pathea.Layer.MetalScan;
						mineralPointsGo.AddComponent<MeshRenderer>().sharedMaterial = mat;
						Mesh mesh = mineralPointsGo.AddComponent<MeshFilter>().mesh;
						mesh.vertices = outputVerts.ToArray();
						mesh.colors32 = outputCols.ToArray();
						
						int[] indices = new int[nVerts];
						Array.Copy(SurfExtractorsMan.s_indiceMax, indices, nVerts);
						mesh.SetIndices(indices,MeshTopology.Points, 0);
						mineralPointsGo.transform.parent = tParent;
						mineralPointsGo.transform.position = new Vector3(
									vecChunkPos.x<<VoxelTerrainConstants._shift,
									vecChunkPos.y<<VoxelTerrainConstants._shift,
									vecChunkPos.z<<VoxelTerrainConstants._shift);
					}
					yield return 0;					
				}
			}
		}
		bInScanning = false;
	}
	Transform ResetGOs()	// return parent's transform
	{
		GameObject mpParent = GameObject.Find("Minerals");
		if(mpParent != null)
		{
			List<Transform> children = new List<Transform>();
			foreach(Transform t in mpParent.transform)
			{
				try{
					children.Add(t);
					Destroy(t.GetComponent<MeshFilter>().mesh);
				}
				catch{}
			}
			for(int i = children.Count-1; i >= 0; i--)
			{
				Destroy(children[i].gameObject);
			}
		}
		else
		{
			mpParent = new GameObject("Minerals");
			mpParent.layer = Pathea.Layer.MetalScan;
		}
		return mpParent.transform;
	}

	public void MakeAScan(Vector3 pos, List<byte> matList,int rad = 80)
	{
		_dataSource = VFVoxelTerrain.self.Voxels;
		centerPos = pos;
		mMatList = matList;
		radius = rad;
		StopAllCoroutines ();
		StartCoroutine(Scan());
	}
}
