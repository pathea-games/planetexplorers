/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/
#define ThreePlaneRiver	//Plane_River,	Plane_Left,	Plane_Right

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]
public class AttachedRiverScript : MonoBehaviour 
{
	// input
	public GameObject parentTerrain;
	
	public GameObject riverObject;
	public LayerMask terrainLayer;
	public float defRiverWidth;
	public float defRiverDepth;
	public int riverSmooth;
	public int curRiverNodeToPosite;
	public bool showHandles;
	public bool finalized;
	public int seaHeight;
	public List<RiverNodeObject> nodeObjects;
	public Vector3[] nodeObjectVerts;
	// Used to force the river downhill
	public float lowestHeight; 
	
	// Used to reference terrain cells
	public ArrayList riverCells;
	
	//public WaterToolScript waterScript;
	
	public AttachedRiverScript()	// constructor will exec before data serialization
	{
		defRiverWidth = 4f;
		defRiverDepth = 3f;
		riverSmooth = 15;
		seaHeight = 0;
	}
	
	public void Start()
	{
		//waterScript = (WaterToolScript)parentTerrain.GetComponent("WaterToolScript");
		
		curRiverNodeToPosite = -1;
		showHandles = true;
		finalized = false;
		
		CreateMesh(riverSmooth);
	}
	
	public void CreateMesh(int smoothLevel) 
	{
		lowestHeight = 9999999f;
		MeshFilter meshFilter = (MeshFilter)riverObject.GetComponent(typeof(MeshFilter));
		Mesh newMesh = meshFilter.sharedMesh;
	 
		if (newMesh == null) 
		{
			newMesh = new Mesh();
			newMesh.name = "Generated River Mesh";
			meshFilter.sharedMesh = newMesh;
		} 
	  
		else 
			newMesh.Clear();
		
		if (nodeObjects == null || nodeObjects.Count < 2) 
		{
			return;
		}
		
		int n = nodeObjects.Count;

		int verticesPerNode = 2 * (smoothLevel + 1) * 2;
		int trianglesPerNode = 6 * (smoothLevel + 1);
#if ThreePlaneRiver
		verticesPerNode *= 2;
		trianglesPerNode *= 3;
#endif
		int[] newTriangles = new int[(trianglesPerNode * (n - 1))];
		Vector3[] newVertices = new Vector3[(verticesPerNode * (n - 1))];
		Vector2[] uvs = new Vector2[(verticesPerNode * (n - 1))];
		nodeObjectVerts = new Vector3[(verticesPerNode * (n - 1))];
		
		int nextVertex  = 0;
		int nextTriangle = 0;
		int nextUV = 0;

		float[] cubicX = new float[n];
		float[] cubicY = new float[n];
		float[] cubicZ = new float[n];
		float[] cubicW = new float[n];
		
		Vector3 handle1Tween = new Vector3();
		
		Vector3[] g1 = new Vector3[smoothLevel+1];
		Vector3[] g2 = new Vector3[smoothLevel+1];
		Vector3[] g3 = new Vector3[smoothLevel+1];
		Vector3 oldG2 = new Vector3();
		Vector3 normExtrudedPointL = new Vector3();
		Vector3 normExtrudedPointR = new Vector3();
		
		for(int i = 0; i < n; i++)
		{
			cubicX[i] = nodeObjects[i].position.x;
			cubicY[i] = nodeObjects[i].position.y;
			cubicZ[i] = nodeObjects[i].position.z;
			cubicW[i] = nodeObjects[i].width;
		}
	
		Cubic[] X = calcNaturalCubic(n-1, cubicX);
		Cubic[] Y = calcNaturalCubic(n-1, cubicY);
		Cubic[] Z = calcNaturalCubic(n-1, cubicZ);
		Cubic[] W = calcNaturalCubic(n-1, cubicW);
		for (int i = 0; i < n; i++) 
		{
			g1 = new Vector3[smoothLevel+1];
			g2 = new Vector3[smoothLevel+1];
			g3 = new Vector3[smoothLevel+1];
			
			normExtrudedPointL = new Vector3();
			normExtrudedPointR = new Vector3();
			
			if (i == 0)
			{
#if ThreePlaneRiver	// RightEnd- RightOfRiver- LeftOfRiver- LeftEnd
				newVertices[nextVertex] = nodeObjects[0].position;			nextVertex++;
				newVertices[nextVertex] = nodeObjects[0].position;			nextVertex++;
				newVertices[nextVertex] = nodeObjects[0].position;			nextVertex++;
				newVertices[nextVertex] = nodeObjects[0].position;			nextVertex++;
				uvs[nextUV] = new Vector2(1f, 1f);								nextUV++;
				uvs[nextUV] = new Vector2(0f, 1f);								nextUV++;
				uvs[nextUV] = new Vector2(1f, 1f);								nextUV++;
				uvs[nextUV] = new Vector2(0f, 1f);								nextUV++;
#else
				newVertices[nextVertex] = nodeObjects[0].position;			nextVertex++;
				newVertices[nextVertex] = nodeObjects[0].position;			nextVertex++;
				uvs[nextUV] = new Vector2(0f, 1f);								nextUV++;
				uvs[nextUV] = new Vector2(1f, 1f);								nextUV++;
#endif
				continue;
			}
			
			// Interpolate the points on the path using natural cubic splines
			for (int j = 0; j < smoothLevel + 1; j++) 
			{
				// clone the vertex for uvs
				if(i == 1 && j == 0)
				{
					oldG2 = nodeObjects[0].position;
				}
				else
				{
#if ThreePlaneRiver	// RightEnd- RightOfRiver- LeftOfRiver- LeftEnd
					newVertices[nextVertex] = newVertices[nextVertex-4];	nextVertex++;
					newVertices[nextVertex] = newVertices[nextVertex-4];	nextVertex++;
					newVertices[nextVertex] = newVertices[nextVertex-4];	nextVertex++;
					newVertices[nextVertex] = newVertices[nextVertex-4];	nextVertex++;
					uvs[nextUV] = new Vector2(1f, 1f);						nextUV++;
					uvs[nextUV] = new Vector2(0f, 1f);						nextUV++;
					uvs[nextUV] = new Vector2(1f, 1f);						nextUV++;
					uvs[nextUV] = new Vector2(0f, 1f);						nextUV++;
#else
					newVertices[nextVertex] = newVertices[nextVertex-2];	nextVertex++;
					newVertices[nextVertex] = newVertices[nextVertex-2];	nextVertex++;
					uvs[nextUV] = new Vector2(0f, 1f);						nextUV++;
					uvs[nextUV] = new Vector2(1f, 1f);						nextUV++;
#endif
				}
				
				float u = (float)(j+1)/(float)(smoothLevel+1f);
				Vector3 tweenPoint = new Vector3(X[i-1].eval(u), Y[i-1].eval(u), Z[i-1].eval(u));
				float nw = W[i-1].eval(u);

				g2[j] = tweenPoint;
				g1[j] = oldG2;
				g3[j] = g2[j] - g1[j];
				oldG2 = g2[j];
				// xz axis exchange in order to get a vertical vector to g3
				normExtrudedPointL = new Vector3(-g3[j].z, 0, g3[j].x);
				normExtrudedPointR = new Vector3(g3[j].z, 0, -g3[j].x);
				normExtrudedPointL.Normalize();
				normExtrudedPointR.Normalize();

				// used to update the handles
				if(i == 1 && j == 0)
				{
					handle1Tween = tweenPoint;
				}
#if ThreePlaneRiver	// RightEnd- RightOfRiver- LeftOfRiver- LeftEnd
				float ew = 2*nw;
				int curVertex = nextVertex+1;
				newVertices[nextVertex] = tweenPoint + normExtrudedPointR * ew;	newVertices[nextVertex].y = 0;				nextVertex++;
				newVertices[nextVertex] = tweenPoint + normExtrudedPointR * nw;	newVertices[nextVertex].y = tweenPoint.y;	nextVertex++;
				newVertices[nextVertex] = tweenPoint + normExtrudedPointL * nw;	newVertices[nextVertex].y = tweenPoint.y;	nextVertex++;
				newVertices[nextVertex] = tweenPoint + normExtrudedPointL * ew;	newVertices[nextVertex].y = 0;				nextVertex++;
				uvs[nextUV] = new Vector2(1f, 0f);				nextUV++;				
				uvs[nextUV] = new Vector2(0f, 0f);				nextUV++;
				uvs[nextUV] = new Vector2(1f, 0f);				nextUV++;				
				uvs[nextUV] = new Vector2(0f, 0f);				nextUV++;
#else
				int curVertex = nextVertex;
				newVertices[nextVertex] = tweenPoint + normExtrudedPointR * nw;	newVertices[nextVertex].y = tweenPoint.y;	nextVertex++;
				newVertices[nextVertex] = tweenPoint + normExtrudedPointL * nw;	newVertices[nextVertex].y = tweenPoint.y;	nextVertex++;
				uvs[nextUV] = new Vector2(0f, 0f);				nextUV++;
				uvs[nextUV] = new Vector2(1f, 0f);				nextUV++;
#endif
				// flatten LR and force downhill
				float vy = newVertices[curVertex+1].y > (newVertices[curVertex].y-0.2f) ? newVertices[curVertex].y : newVertices[curVertex+1].y;
				newVertices[curVertex+1].y = newVertices[curVertex].y = vy;
				if(newVertices[curVertex+1].y > lowestHeight)
				{
					//newVertices[curVertex+1].y = lowestHeight;
					//newVertices[curVertex].y = lowestHeight;
				}
				else
					lowestHeight = newVertices[curVertex+1].y;
				
				// Create triangles...
#if ThreePlaneRiver	// RightEnd- RightOfRiver- LeftOfRiver- LeftEnd
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  0;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  1;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  4;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  1;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  5;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  4;	nextTriangle++;
				// River
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  1;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  2;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  5;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  2;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  6;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  5;	nextTriangle++;
				// LeftPlane
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  2;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  3;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  6;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  3;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  7;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (8 * j) +  6;	nextTriangle++;
#else
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) +  0;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) +  1;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) +  2;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) +  1;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) +  3;	nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) +  2;	nextTriangle++;				
#endif				
			}
		}

		// update handles
		g2[0] = handle1Tween;
		g1[0] = nodeObjects[0].position;
		g3[0] = g2[0] - g1[0];
	
		normExtrudedPointL = new Vector3(-g3[0].z, 0, g3[0].x);
		normExtrudedPointR = new Vector3(g3[0].z, 0, -g3[0].x);
		normExtrudedPointL.Normalize();
		normExtrudedPointR.Normalize();
#if ThreePlaneRiver	// RightEnd- RightOfRiver- LeftOfRiver- LeftEnd
		//float nodeW = nodeObjects[0].width;
		//float sideW = nodeW*2;
		//newVertices[0] = nodeObjects[0].position + normExtrudedPointR * sideW;		newVertices[0].y = newVertices[4].y; 
		//newVertices[1] = nodeObjects[0].position + normExtrudedPointR * nodeW;		newVertices[1].y = newVertices[5].y; 
		//newVertices[2] = nodeObjects[0].position + normExtrudedPointL * nodeW;		newVertices[2].y = newVertices[6].y;
		//newVertices[3] = nodeObjects[0].position + normExtrudedPointL * sideW;		newVertices[3].y = newVertices[7].y;
		newVertices[0].y = newVertices[4].y;
		newVertices[1].y = newVertices[5].y;
		newVertices[2].y = newVertices[6].y;
		newVertices[3].y = newVertices[7].y;
#else		
		newVertices[0] = nodeObjects[0].position + normExtrudedPointR * nodeObjects[0].width;		newVertices[0].y = newVertices[2].y; 
		newVertices[1] = nodeObjects[0].position + normExtrudedPointL * nodeObjects[0].width;		newVertices[1].y = newVertices[3].y;
#endif	
		for(int i = 0; i <newVertices.Length; i++)
		{
			nodeObjectVerts[i] = newVertices[i];
		}
		
		newMesh.vertices = newVertices;
		newMesh.triangles = newTriangles;
		newMesh.uv =  uvs;
		
		Vector3[] myNormals = new Vector3[newMesh.vertexCount];
		for(int p = 0; p < newMesh.vertexCount; p++)
		{
			myNormals[p] = Vector3.up;
		}
		
		newMesh.normals = myNormals;
		
		TangentSolver(newMesh);

//		newMesh.RecalculateNormals();
		newMesh.Optimize();
		MeshCollider mc = GetComponent<MeshCollider>();
		if(mc != null)
		{
			mc.sharedMesh = newMesh;
		}
	}

	public void OnDrawGizmos()
	{
		if(showHandles)
		{
			if(nodeObjectVerts != null)
				if (nodeObjectVerts.Length > 0) 
				{
					int n = nodeObjectVerts.Length;
					for (int i = 0; i < n; i++) 
					{
						// Handles...
						Gizmos.color = Color.white;
						
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(-0.5f, 0, 0)), transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0.5f, 0, 0)));
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, -0.5f, 0)), transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, 0.5f, 0)));
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, 0, -0.5f)), transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, 0, 0.5f)));
					}
				}
		}
	}
	
	/*
	Derived from
	Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh? Terathon Software 3D Graphics Library, 2001.
	http://www.terathon.com/code/tangent.html
	*/

	public void TangentSolver(Mesh theMesh)
    {
        int vertexCount = theMesh.vertexCount;
        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
        Vector2[] texcoords = theMesh.uv;
        int[] triangles = theMesh.triangles;
        int triangleCount = triangles.Length/3;
        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];
        int tri = 0;
		
		int i1, i2, i3;
		Vector3 v1, v2, v3, w1, w2, w3, sdir, tdir;
		float x1, x2, y1, y2, z1, z2, s1, s2, t1, t2, r;
        for (int i = 0; i < (triangleCount); i++)
        {
            i1 = triangles[tri];
            i2 = triangles[tri+1];
            i3 = triangles[tri+2];

            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];

            w1 = texcoords[i1];
            w2 = texcoords[i2];
            w3 = texcoords[i3];

            x1 = v2.x - v1.x;
            x2 = v3.x - v1.x;
            y1 = v2.y - v1.y;
            y2 = v3.y - v1.y;
            z1 = v2.z - v1.z;
            z2 = v3.z - v1.z;

            s1 = w2.x - w1.x;
            s2 = w3.x - w1.x;
            t1 = w2.y - w1.y;
            t2 = w3.y - w1.y;

            r = 1.0f / (s1 * t2 - s2 * t1);
            sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;

            tri += 3;
        }
		
        for (int i = 0; i < (vertexCount); i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            Vector3.OrthoNormalize(ref n, ref t);

            tangents[i].x  = t.x;
            tangents[i].y  = t.y;
            tangents[i].z  = t.z;

            tangents[i].w = ( Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ) ? -1.0f : 1.0f;
        }       
		
        theMesh.tangents = tangents;
    }
	
	/* Derived from original Java source 
	Tim Lambert                                       
	School of Computer Science and Engineering         
	The University of New South Wales                
	*/
	
	public class Cubic
	{
	  float a,b,c,d;        

	  public Cubic(float a, float b, float c, float d){
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	  }
	  
	  public float eval(float u) 
	  {
		return (((d*u) + c)*u + b)*u + a;
	  }
	}
	
	public Cubic[] calcNaturalCubic(int n, float[] x) 
	{
		float[] gamma = new float[n+1];
		float[] delta = new float[n+1];
		float[] D = new float[n+1];
		int i;
    
		gamma[0] = 1.0f/2.0f;
		
		for ( i = 1; i < n; i++) 
		{
		  gamma[i] = 1/(4-gamma[i-1]);
		}
		
		gamma[n] = 1/(2-gamma[n-1]);
		
		delta[0] = 3*(x[1]-x[0])*gamma[0];
		
		for ( i = 1; i < n; i++) 
		{
		  delta[i] = (3*(x[i+1]-x[i-1])-delta[i-1])*gamma[i];
		}
		
		delta[n] = (3*(x[n]-x[n-1])-delta[n-1])*gamma[n];
		
		D[n] = delta[n];
		
		for ( i = n-1; i >= 0; i--) 
		{
		  D[i] = delta[i] - gamma[i]*D[i+1];
		}

		Cubic[] C = new Cubic[n+1];
		for ( i = 0; i < n; i++) {
		  C[i] = new Cubic((float)x[i], D[i], 3*(x[i+1] - x[i]) - 2*D[i] - D[i+1],
				   2*(x[i] - x[i+1]) + D[i] + D[i+1]);
		}
		
		return C;
	}
}