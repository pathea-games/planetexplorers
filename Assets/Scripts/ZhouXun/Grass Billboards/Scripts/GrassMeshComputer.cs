using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Mesh computer for [Billboard-set Grass System].
public static class GrassMeshComputer
{
	public static int s_GrassCountPerMesh = 15000;

	/// <summary>
	/// Computes the grass mesh for [Billboard-set Grass System].
	/// </summary>
	/// <returns>
	/// Offset in 'grass_list' from where you can compute the next mesh.
	/// </returns>
	/// <param name='grass_list'>
	/// The grass instance list.
	/// </param>
	/// <param name='offset'>
	/// Offset in 'grass_list' from where you compute the mesh.
	/// </param>
	/// <param name='mf_inout'>
	/// [inout] The MeshFilter component receives the mesh.
	/// </param>
	public static int ComputeMesh (List<GrassInstance> grass_list, int offset, MeshFilter mf_inout)
	{
		if ( grass_list == null ) return 0;
		offset = Mathf.Clamp(offset, 0, grass_list.Count);
		int grasspermesh = Mathf.Clamp(s_GrassCountPerMesh, 128, 16240);
		int grasscount = Mathf.Min(grasspermesh, grass_list.Count - offset);
		if ( grasscount < 1 ) return grass_list.Count;
		if ( mf_inout == null ) return grass_list.Count;
		
		Vector3[] verts = new Vector3 [grasscount<<2];
		Vector3[] norms = new Vector3 [grasscount<<2];
		Vector2[] uvs = new Vector2 [grasscount<<2];
		Vector2[] uv2s = new Vector2 [grasscount<<2];
		Color32[] colors32 = new Color32 [grasscount<<2];
		int[] indices = new int [grasscount*6];
		
		Vector3 A = new Vector3 (-1, -1, 0);
		Vector3 B = new Vector3 (-1,  1, 0);
		Vector3 C = new Vector3 ( 1,  1, 0);
		Vector3 D = new Vector3 ( 1, -1, 0);
		Vector3 up = Vector3.up;
		
		for ( int i = 0; i < grasscount; ++i )
		{
			int a = i*4;
			int b = a+1;
			int c = a+2;
			int d = a+3;
			
			indices[i*6+0] = a;
			indices[i*6+1] = b;
			indices[i*6+2] = c;
			indices[i*6+3] = c;
			indices[i*6+4] = d;
			indices[i*6+5] = a;
			
			verts[a] = A;
			verts[b] = B;
			verts[c] = C;
			verts[d] = D;
			
			GrassInstance gi = grass_list[offset + i];
			
			norms[d] = norms[c] = norms[b] = norms[a] = gi.Position;
			
			Vector3 n = gi.Normal;
			Vector3 n1 = (n * 1.25f - up * 0.25f).normalized;
			Vector3 n2 = (n * 0.5f + up * 0.5f).normalized;
			
			uvs[a] = new Vector2(n1.x, n1.z);
			uvs[c] = uvs[b] = new Vector2(n2.x, n2.z);
			uvs[d] = uvs[a];
			
			uv2s[d] = uv2s[c] = uv2s[b] = uv2s[a] = new Vector2((float)(gi.Prototype) / (float)(GrassPrototypeMgr.s_PrototypeCount), 0);
			
			colors32[d] = colors32[c] = colors32[b] = colors32[a] = gi.ColorDw;
		}
		
		Mesh mesh = mf_inout.mesh;
		mesh.Clear();
		mesh.vertices = verts;
		mesh.triangles = indices;
		mesh.normals = norms;
		mesh.uv = uvs;
		mesh.uv2 = uv2s;
		mesh.colors32 = colors32;
		
		return offset + grasscount;
	}
}
