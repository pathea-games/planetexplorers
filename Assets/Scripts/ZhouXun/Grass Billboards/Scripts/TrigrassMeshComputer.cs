using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Mesh computer for [Billboard-set Tri-grass System].
public static class TrigrassMeshComputer
{
	public static int s_GrassCountPerMesh = 3000;

	/// <summary>
	/// Computes the grass mesh for [Billboard-set Tri-grass System].
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
        //Win32.HiPerfTimer hitimer = new Win32.HiPerfTimer ();
        //hitimer.Start();
		if ( grass_list == null ) return 0;
		offset = Mathf.Clamp(offset, 0, grass_list.Count);
		int grasspermesh = Mathf.Clamp(s_GrassCountPerMesh, 128, 5400);
		int grasscount = Mathf.Min(grasspermesh, grass_list.Count - offset);
		if ( grasscount < 1 ) return grass_list.Count;
		if ( mf_inout == null ) return grass_list.Count;
		
		Vector3[] verts = new Vector3 [grasscount*12];
		Vector3[] norms = new Vector3 [grasscount*12];
		Vector2[] uvs = new Vector2 [grasscount*12];
		Vector2[] uv2s = new Vector2 [grasscount*12];
		Color32[] colors32 = new Color32 [grasscount*12];
		int[] indices = new int [grasscount*18];
		
		Vector3 A = new Vector3 (-1, -1, 0);
		Vector3 B = new Vector3 (-1,  1, 0);
		Vector3 C = new Vector3 ( 1,  1, 0);
		Vector3 D = new Vector3 ( 1, -1, 0);
		Vector3 up = Vector3.up;
		
		float _2pi = Mathf.PI * 2;
		float _page_interval = _2pi / 3;
		
		float angle_randomness = 0.3f;
//		float size_randomness = 0.3f;
//		float min_size = 0.85f;
//		float bias_randomness = 0.3f;
//		float min_bias = 0.1f;
//		float side_randomness = 2f;
		
		for ( int i = 0; i < grasscount; ++i )
		{
			float phase = Random.value * _2pi;
			
			for ( int p = 0; p < 3; ++p )
			{
//				float angle = angle_randomness * (Random.value - 0.5f) + p*_page_interval + phase;
//				float height = min_size + Random.value * size_randomness;
//				float width = min_size + Random.value * size_randomness;
//				float bias = Random.value * bias_randomness + min_bias;
//				float side = (Random.value - 0.5f) * side_randomness;
//				
//				float cos = width*Mathf.Cos(angle);
//				float sin = width*Mathf.Sin(angle);
//				float bias_x_up = (-sin*bias) * (1+side);
//				float bias_z_up = (cos*bias) * (1+side);
//				float bias_x_dn = (-sin*bias) * (1-side);
//				float bias_z_dn = (cos*bias) * (1-side);
//				
//				Vector3 A = new Vector3 (-cos + bias_x_dn, -height, -sin + bias_z_dn);
//				Vector3 B = new Vector3 (-cos + bias_x_up,  height, -sin + bias_z_up);
//				Vector3 C = new Vector3 ( cos + bias_x_up,  height,  sin + bias_z_up);
//				Vector3 D = new Vector3 ( cos + bias_x_dn, -height,  sin + bias_z_dn);
				
				int a = i*12 + p*4;
				int b = a+1;
				int c = a+2;
				int d = a+3;
				
				int idx = i*18 + p*6;
				
				indices[idx+0] = a;
				indices[idx+1] = b;
				indices[idx+2] = c;
				indices[idx+3] = c;
				indices[idx+4] = d;
				indices[idx+5] = a;
				
				verts[a] = A;
				verts[b] = B;
				verts[c] = C;
				verts[d] = D;
				
				GrassInstance gi = grass_list[offset + i];
				
				norms[d] = norms[c] = norms[b] = norms[a] = gi.Position;
				
				Vector3 n = gi.Normal;
				Vector3 n1 = (n * 1.1f - up * 0.1f).normalized;
				Vector3 n2 = (n * 0.5f + up * 0.5f).normalized;
				
				uvs[a] = new Vector2(n1.x, n1.z);
				uvs[c] = uvs[b] = new Vector2(n2.x, n2.z);
				uvs[d] = uvs[a];
				
				uv2s[d] = uv2s[c] = uv2s[b] = uv2s[a] = new Vector2((float)(gi.Prototype) / (float)(GrassPrototypeMgr.s_PrototypeCount), 
					                                                p * _page_interval + phase + (Random.value - 0.5f) * angle_randomness);
				
				colors32[d] = colors32[c] = colors32[b] = colors32[a] = gi.ColorDw;
			}
		}
		
		Mesh mesh = mf_inout.mesh;
		mesh.Clear();
		mesh.vertices = verts;
		mesh.triangles = indices;
		mesh.normals = norms;
		mesh.uv = uvs;
		mesh.uv2 = uv2s;
		mesh.colors32 = colors32;
		
        //hitimer.Stop();
        //hitimer.PrintDuration("Compute Mesh");
		
		return offset + grasscount;
	}
}
