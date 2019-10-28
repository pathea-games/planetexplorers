using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;
public class BoundingCubeScale : MonoBehaviour{

	public Material mat;
	GameObject go;
	
	void Start()
	{
	}

	public void MakeCube(int maxX, int maxY, int maxZ)
	{
		go = new GameObject();
		go.name = "trepassing is naughty";
		go.transform.parent = this.transform;
		go.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
		go.layer = Pathea.Layer.VFVoxelTerrain;
		
		MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshCollider mc = go.AddComponent<MeshCollider>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
		
		mr.sharedMaterial = mat;
		
		Mesh _mesh = mf.mesh;
		_mesh.name = "bounding cube mesh";
		Vector3[] vert = new Vector3[8];
		vert[0].x = 0; vert[0].y = 0; vert[0].z = 0;
		vert[1].x = 1; vert[1].y = 0; vert[1].z = 0;
		vert[2].x = 0; vert[2].y = 1; vert[2].z = 0;
		vert[3].x = 1; vert[3].y = 1; vert[3].z = 0;
		
		vert[4].x = 0; vert[4].y = 0; vert[4].z = 1;
		vert[5].x = 1; vert[5].y = 0; vert[5].z = 1;
		vert[6].x = 0; vert[6].y = 1; vert[6].z = 1;
		vert[7].x = 1; vert[7].y = 1; vert[7].z = 1;
		
		_mesh.vertices = vert;
		int[] triIndices = new int[30]
		{
			0, 1, 2,	3, 2, 1,
			2, 4, 0,	6, 4, 2,
			7, 5, 6,	6, 5, 4,
			3, 1, 5,	7, 3, 5,
			
			5, 1, 4,	4, 1, 0,
		};
		
		_mesh.SetTriangles(triIndices,0);
		Vector2[] uv = new Vector2[8];
		
		uv[0].x = 0; uv[0].y = 0;
		uv[1].x = 1; uv[1].y = 0;
		uv[2].x = 0; uv[2].y = 1;
		uv[3].x = 1; uv[3].y = 1;
		
		uv[4].x = 0; uv[4].y = 0;
		uv[5].x = 1; uv[5].y = 0; 
		uv[6].x = 0; uv[6].y = 1; 
		uv[7].x = 1; uv[7].y = 1; 		
		
		_mesh.uv = uv;
		mc.sharedMesh = _mesh;
		
		Transform goTransform = go.GetComponent<Transform>();
		goTransform.localScale = new Vector3(maxX - 2, maxY + 10, maxZ - 2);
		//goTransform.localPosition = new Vector3((maxX - 2) / 2, maxY / 2, (maxZ - 2) / 2);

	}
	


}
