using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RedGrass
{
	public class RedGrassMeshComputer
	{
		public const int MAX_QUAD = 65536;
		public const int MAX_VERT = MAX_QUAD << 2;
		public const int MAX_IDX = MAX_QUAD * 6;
		
		public static float s_FullDensity = 2f;
		
		public class OutputStruct
		{
			// Array buffers
			public Vector3[] Verts;
			public Vector3[] Norms;
			public Vector2[] UVs;
			public Vector2[] UV2s;
			public Color32[] Color32s;
			public int[] Indices;
			
			// lens
			public int BillboardCount;
			public int TriquadCount;
			public int TotalVertCount;
			
			public void Init ()
			{
				Verts = new Vector3 [MAX_VERT];
				Norms = new Vector3 [MAX_VERT];
				UVs = new Vector2 [MAX_VERT];
				UV2s = new Vector2 [MAX_VERT];
				Color32s = new Color32 [MAX_VERT];
				Indices = new int [MAX_IDX];
				Reset();
				
				// Fixed arrays
				Vector3 A = new Vector3 (-1, -1, 0);
				Vector3 B = new Vector3 (-1,  1, 0);
				Vector3 C = new Vector3 ( 1,  1, 0);
				Vector3 D = new Vector3 ( 1, -1, 0);
				for ( int i = 0; i < MAX_QUAD; ++i )
				{
					Verts[i*4+0] = A;
					Verts[i*4+1] = B;
					Verts[i*4+2] = C;
					Verts[i*4+3] = D;
				}
				for ( int i = 0; i < MAX_QUAD; ++i )
				{
					Indices[i*6+0] = i*4+0;
					Indices[i*6+1] = i*4+1;
					Indices[i*6+2] = i*4+2;
					Indices[i*6+3] = i*4+2;
					Indices[i*6+4] = i*4+3;
					Indices[i*6+5] = i*4+0;
				}
			}
			public void Reset ()
			{
				BillboardCount = 0;
				TriquadCount = 0;
				TotalVertCount = 0;
			}
		}
		
		public static OutputStruct s_Output;
		
		public static void Init ()
		{
			s_Output = new OutputStruct ();
			s_Output.Init();
		}
		
		private static int RandomCount (float expectation, RedGrassInstance rgi)
		{
			float min = (float)((int)(expectation));
			float p = expectation - min;
			return (int)((p == 0f) ? min : ((rgi.RandAttr.x < p) ? min + 1 : min));
		}
		
		/// <summary>
		/// Computes the mesh elements info the output
		/// </summary>
		/// <param name='grass_list'>
		/// Grass list.
		/// </param>
		/// <param name='density'>
		/// Global Density.
		/// </param>
		public static void ComputeMesh ( List<RedGrassInstance> grass_list, List<RedGrassInstance> tri_grass_list, float density )
		{
			try
			{
				s_Output.Reset();
				int grass_cnt = 0;
				int tri_grass_cnt = 0;
				if ( grass_list != null )
					grass_cnt = grass_list.Count;
				if ( tri_grass_list != null )
					tri_grass_cnt = tri_grass_list.Count;
				
				Vector3 up = Vector3.up;
				int quad = 0;
				float _2pi = Mathf.PI * 2;
				float _page_interval = _2pi / 3;
				float angle_randomness = 0.3f;
				
				// Billboard grasses
				for ( int i = 0; i < grass_cnt; ++i )
				{
					RedGrassInstance vgi = grass_list[i];
					int randcnt = RandomCount(s_FullDensity * vgi.Density * density, vgi);
					for ( int q = 0; q < randcnt; ++q )
					{
						int a = quad*4;
						int b = a+1;
						int c = a+2;
						int d = a+3;
						
						s_Output.Norms[d] = 
							s_Output.Norms[c] = 
								s_Output.Norms[b] = 
								s_Output.Norms[a] = vgi.RandPos(q);
						
						Vector3 n = vgi.Normal;
						Vector3 n1 = (n * 1.25f - up * 0.25f).normalized;
						Vector3 n2 = (n * 0.5f + up * 0.5f).normalized;
						
						s_Output.UVs[a] = new Vector2(n1.x, n1.z);
						s_Output.UVs[c] = 
							s_Output.UVs[b] = new Vector2(n2.x, n2.z);
						s_Output.UVs[d] = s_Output.UVs[a];
						
						s_Output.UV2s[d] = 
							s_Output.UV2s[c] = 
								s_Output.UV2s[b] = 
								s_Output.UV2s[a] = new Vector2((float)(vgi.Prototype) / (float)(RGPrototypeMgr.s_PrototypeCount), 0);
						
						s_Output.Color32s[d] = 
							s_Output.Color32s[c] = 
								s_Output.Color32s[b] = 
								s_Output.Color32s[a] = vgi.ColorDw;
						
						quad++;
					}
				}
				s_Output.BillboardCount = quad;
				
				// Tri-quad grasses
				for ( int i = 0; i < tri_grass_cnt; ++i )
				{
					RedGrassInstance vgi = tri_grass_list[i];
					int randcnt = RandomCount(s_FullDensity * vgi.Density * density * 0.333f, vgi);
					for ( int t = 0; t < randcnt; ++t )
					{
						float phase = (float)(vgi.RandAttr.x) * _2pi;
						Vector3 randpos = vgi.RandPos(t);
						for ( int page = 0; page < 3; ++page )
						{
							int a = quad*4;
							int b = a+1;
							int c = a+2;
							int d = a+3;
							
							s_Output.Norms[d] = 
								s_Output.Norms[c] = 
									s_Output.Norms[b] = 
									s_Output.Norms[a] = randpos;
							
							Vector3 n = vgi.Normal;
							Vector3 n1 = (n * 1.1f - up * 0.1f).normalized;
							Vector3 n2 = (n * 0.5f + up * 0.5f).normalized;
							
							s_Output.UVs[a] = new Vector2(n1.x, n1.z);
							s_Output.UVs[c] = 
								s_Output.UVs[b] = new Vector2(n2.x, n2.z);
							s_Output.UVs[d] = s_Output.UVs[a];
							
							s_Output.UV2s[d] =
								s_Output.UV2s[c] =
									s_Output.UV2s[b] =
									s_Output.UV2s[a] = new Vector2((float)(vgi.Prototype-64) / (float)(RGPrototypeMgr.s_PrototypeCount),
									                               page * _page_interval + phase + ((float)(vgi.RandAttrs(page+1).x) - 0.5f) * angle_randomness);
							
							s_Output.Color32s[d] =
								s_Output.Color32s[c] =
									s_Output.Color32s[b] =
									s_Output.Color32s[a] = vgi.ColorDw;
							
							quad++;
						}
					}
				}
				s_Output.TriquadCount = quad - s_Output.BillboardCount;
				s_Output.TotalVertCount = quad*4;
			}
			catch
			{
				Debug.Log("-----------------------------Grass Thread error");
			}
		}
		
		public static void ComputeParticleMesh ( RedGrassInstance[] grass_array, int count )
		{
			s_Output.Reset();
			
			// Billboard grasses
			for ( int i = 0; i < count; ++i )
			{
				RedGrassInstance vgi = grass_array[i];
				
				int a = i*4;
				int b = a+1;
				int c = a+2;
				int d = a+3;
				
				s_Output.Norms[d] = 
					s_Output.Norms[c] = 
						s_Output.Norms[b] = 
						s_Output.Norms[a] = vgi.RandPos(0);
				
				Vector3 n = vgi.Normal;
				
				s_Output.UVs[d] = 
					s_Output.UVs[c] = 
						s_Output.UVs[b] = 
						s_Output.UVs[a] = new Vector2(n.x, n.z);
				
				s_Output.UV2s[d] = 
					s_Output.UV2s[c] = 
						s_Output.UV2s[b] = 
						s_Output.UV2s[a] = new Vector2((float)(vgi.Prototype) / (float)(RGPrototypeMgr.s_PrototypeCount), 0);
				
				s_Output.Color32s[d] = 
					s_Output.Color32s[c] = 
						s_Output.Color32s[b] = 
						s_Output.Color32s[a] = Color.white;
			}
			s_Output.BillboardCount = count;
			s_Output.TriquadCount = 0;
			s_Output.TotalVertCount = count*4;
		}

	}
}
