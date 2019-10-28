using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Collections.Generic;
using Pathea.Maths;

namespace RedGrass
{
	public class RGSimpleEditor : MonoBehaviour 
	{
		#region INSPECTOR_VALS
		public bool  isAdd = true;
		public float radius = 20;
		public int deleteHeight = 10;
		public float density = 1f;
		public Texture2D pattern;
		public MapProjectorAnyAxis mapProjector;

		public string prototypes = "0";

		public RedGrass.RGScene scene;

		#endregion

		Dictionary<INTVECTOR3, RedGrassInstance> mAddGrasses;
			
		public bool isEmpty { get { return mAddGrasses.Count == 0;} }

		public RedGrassInstance[] addGrasses
		{
			get
			{
				RedGrassInstance[] _grasses = new RedGrassInstance[mAddGrasses.Count];
				int i = 0;
				foreach (RedGrassInstance rgi in mAddGrasses.Values)
				{
					_grasses[i] = rgi;
					i ++;
				}

				return _grasses;
			}
		}

		public void Clear()
		{
			mAddGrasses.Clear();
		}

		void Awake()
		{
			mAddGrasses = new Dictionary<INTVECTOR3, RedGrassInstance>();
		}

		void Start()
		{

		}

		void Update ()
		{

			int tex_h = pattern.height;
			int tex_w = pattern.width;

			if (tex_h != tex_w)
			{
				return;
			}

			int[] protos = GetPrototypes(prototypes);

			if (isAdd && protos.Length == 0)
				return;

			// Set attribute to Projector
			mapProjector.MapTex = pattern;
			mapProjector.Size = radius;
			mapProjector.NearClip = -50;

			mapProjector.ColorIndex = isAdd? 0 : 1;

			RaycastHit rch;
			Ray pick_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(pick_ray, out rch, 1000, 1 << Pathea.Layer.VFVoxelTerrain))
			{
				mapProjector.transform.position = rch.point;


				if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
				{
					if (isAdd)
						DrawGrass(rch.point, rch.normal, protos);
					else
						DeleteGrass(rch.point, rch.normal);
				}
			}

		}

		// Drass Grass 
		Vector3[,] _normals = null;
		void DrawGrass (Vector3 point, Vector3 nml, int[] protos)
		{
			float size = radius * 2;
			int step = Mathf.Clamp( Mathf.RoundToInt( size / 30) + 1, 1, 4);
			
			float begin_x = Mathf.Max(0, point.x - radius - step);
			float end_x =  point.x + radius + step * 2;
			float begin_z = Mathf.Max(0, point.z - radius - step);
			float end_z = point.z + radius + step * 2;
			
			bool up = (point.y >= -0.1f && Camera.main.transform.forward.y < 0.7f);


			// Calc heights matrixs
			int count = Mathf.CeilToInt( size / step ) + 1;
			float[,] heights = new float[count + 2, count + 2];

			Vector3 dir = up ? Vector3.down : Vector3.up;
			float  h = up ? point.y + 100 : point.y - 100;

			{
				int i = 0;
				for (float x = begin_x; x < end_x; x += step, i++)
				{
					int j = 0;
					for (float z = begin_z; z < end_z; z += step, j++)
					{
						Vector3 p = new Vector3(x, h, z);

						RaycastHit rch;
						Ray ray = new Ray(p, dir);
						if (Physics.Raycast(ray, out rch, 1000, 1 << Pathea.Layer.VFVoxelTerrain))
						{
							heights[i, j] = rch.point.y;
						}
					}
				}
			}

			// Calc normal matrixs
			_normals = new Vector3[count, count];
			for (int i = 0; i < count; ++i)
			{
				for (int j = 0; j < count; ++j)
				{
					
					_normals[i, j] = CalculateNormal( heights[i + 2, j + 1], heights[i, j + 1] , heights[i + 1, j + 2],  heights[i + 1, j], step * 2);
					
				}
			}

			// readly draw
			int bound = Mathf.CeilToInt (radius);

			Color[] pixels = mapProjector.MapTex.GetPixels();
			int tex_h = mapProjector.MapTex.height;
			int tex_w = mapProjector.MapTex.width;

			for (int x = -bound; x <= bound; ++x)
			{
				for (int z = -bound; z <= bound; ++z)
				{
					Vector3  p = point + new Vector3(x, h, z);

					Vector2 tex_pos = new Vector2((float)(x + radius)/(radius * 2) * tex_w, (float)(z + radius) / (radius * 2) * tex_h );
					float pic_density = CalcHeight(tex_pos, pixels, tex_w, tex_h);
					float den = Mathf.Clamp01( pic_density * density);

					if (pic_density < 0.02f || den < 0.002f)
						continue;

					RaycastHit rch;
					if (Physics.Raycast(p, dir, out rch, 1000, (1 << Pathea.Layer.VFVoxelTerrain)))
					{
						INTVECTOR3 ipos = new INTVECTOR3((int)rch.point.x, (int)rch.point.y, (int)rch.point.z);
						RedGrassInstance old_rgi = scene.data.Read(ipos.x, ipos.y, ipos.z);

						if (old_rgi.Density < 0.001f)
						{
							RedGrassInstance rgi = new RedGrassInstance();
							rgi.Density  = den;
							rgi.Position = rch.point;

							rgi.Prototype = protos[Random.Range(0, protos.Length) % protos.Length];
							rgi.ColorF 	  = Color.white;

							if (scene.data.Write(rgi))
							{
								mAddGrasses[ipos] = rgi;
							}
						}
						else
						{
							RedGrassInstance rgi = new RedGrassInstance();
							rgi.Density =  Mathf.Clamp01(den + old_rgi.Density);
							rgi.Position = old_rgi.Position;

							if (Random.value < density)
							{
								rgi.Prototype = protos[Random.Range(0, protos.Length) % protos.Length];
							}
							else
								rgi.Prototype = old_rgi.Prototype;

							rgi.ColorF = old_rgi.ColorF;
							rgi.Normal = old_rgi.Normal;

							if (scene.data.Write(rgi))
							{
								mAddGrasses[ipos] = rgi;
							}
						}



					}
				}
			}

		}

		void DeleteGrass(Vector3 point, Vector3 nml)
		{
			int step = Mathf.CeilToInt( radius / 30) * 2;

			bool up = (nml.y >= -0.1f && Camera.main.transform.forward.y < 0.7f);
			Vector3 dir = up ? Vector3.down : Vector3.up;
			float  h = up ? point.y + radius : point.y - radius;

			int half_step = step / 2;
			Color[] pixels = mapProjector.MapTex.GetPixels();
			int tex_h = mapProjector.MapTex.height;
			int tex_w = mapProjector.MapTex.width;


			for (float x = -radius; x < radius; x += 1)
			{
				for (float z = -radius; z < radius; z += 1)
				{
					Vector3 tar_pos = point + new Vector3(x, h, z);
					Ray ray = new Ray(tar_pos, dir);

					RaycastHit rch;
					if (Physics.Raycast(ray, out rch, 1000, (1 << Pathea.Layer.VFVoxelTerrain)))
					{
						for (int dx = -half_step; dx <= half_step; dx++)
						{
							for (int dz = -half_step; dz <= half_step; dz++)
							{
								Vector2 tex_pos = new Vector2( ((x + dx) / radius + 1) * 0.5f * tex_w, ((z + dz) / radius + 1) * 0.5f * tex_h );

								float fd = CalcHeight(tex_pos, pixels, tex_w, tex_h) * density;

								List<RedGrassInstance> old_grasses = scene.data.Read((int)tar_pos.x + dx, (int)tar_pos.z + dz, 
								                                                     (int)rch.point.y - half_step -1, (int)rch.point.y + half_step + 1 + Mathf.Max(0, deleteHeight));

								if (fd < 0.0001f)
									continue;

								foreach (RedGrassInstance old_vgi in old_grasses)
								{
									RedGrassInstance new_vgi = new RedGrassInstance();
									new_vgi.Position 	= old_vgi.Position;
									new_vgi.Normal	 	= old_vgi.Normal;
									new_vgi.Density  	= Mathf.Max(0, old_vgi.Density - fd);
									new_vgi.ColorF	 	= old_vgi.ColorF;
									new_vgi.Prototype	= old_vgi.Prototype;
									
									// Modidy
									if (scene.data.Write(new_vgi))
									{
										Vector3 _pos = new_vgi.Position;
										mAddGrasses[new INTVECTOR3((int)_pos.x, (int)_pos.y, (int)_pos.z)] = new_vgi; 
									}
									
								}
							}
						}

	//					Vector2 tex_pos = new Vector2( (x / radius + 1) * 0.5f * tex_w, (z / radius + 1) * 0.5f * tex_h );
	//					float fd = CalcHeight(tex_pos, pixels, tex_w, tex_h) * density;
	//
	//					List<RedGrassInstance> old_grasses = scene.data.Read((int)tar_pos.x, (int)tar_pos.z, 
	//							                                                     (int)rch.point.y - half_step -1, (int)rch.point.y + half_step + 1);
	//
	//					if (fd < 0.0001f)
	//						continue;
	//
	//					foreach (RedGrassInstance old_vgi in old_grasses)
	//					{
	//						RedGrassInstance new_vgi = new RedGrassInstance();
	//						new_vgi.Position 	= old_vgi.Position;
	//						new_vgi.Normal	 	= old_vgi.Normal;
	//						new_vgi.Density  	= Mathf.Max(0, old_vgi.Density - fd);
	//						new_vgi.ColorF	 	= old_vgi.ColorF;
	//						new_vgi.Prototype	= old_vgi.Prototype;
	//						
	//						// Modidy
	//						if (scene.data.Write(new_vgi))
	//						{
	//							Vector3 _pos = new_vgi.Position;
	//							mAddGrasses[new INTVECTOR3((int)_pos.x, (int)_pos.y, (int)_pos.z)] = new_vgi;
	//						}
	//						
	//					}
					}
				}
			}
		}

		Vector3 CalculateNormal (float xForwad, float xBack, float zForwad, float zBack, int detla)
		{
			float x = (xBack - xForwad) / detla;
			float z = (zBack - zForwad) / detla;
			Vector3 n = new Vector3(Mathf.Clamp(x, -0.6f,  0.6f), 1, Mathf.Clamp(z, -0.6f,  0.6f)).normalized;
			return n;
		}

		// high-efficiency 
		float CalcHeight (Vector2 pos, Color[] pixels, int tex_w, int tex_h)
		{
			if (pos.x < 0 || pos.y < 0)
				return 0;
			
			
			float x_frac = pos.x - (int)pos.x;
			float y_frac = pos.y - (int)pos.y;
			
			int x_ceil  = Mathf.Min( Mathf.CeilToInt(pos.x), tex_w - 1);
			int x_floor = Mathf.Min( Mathf.FloorToInt(pos.x), tex_w - 1);
			int y_ceil  = Mathf.Min( Mathf.CeilToInt(pos.y), tex_h - 1);
			int y_floor = Mathf.Min( Mathf.FloorToInt(pos.y), tex_h - 1);
			
			float h1 = pixels[tex_w * y_floor + x_floor].a;
			float h2 = pixels[tex_w * y_floor + x_ceil].a;
			float h3 = pixels[tex_w * y_ceil + x_floor].a;
			float h4 = pixels[tex_w * y_ceil + x_ceil].a;
			
			
			float height = Mathf.Lerp( Mathf.Lerp(h1, h2, x_frac), Mathf.Lerp(h3, h4, x_frac), y_frac);
			
			return height;
		}
		

		int[] GetPrototypes(string str)
		{
			try
			{
				string[] sqlits = str.Split(',');
				int[] protos = new int[sqlits.Length];
				for (int i = 0; i < sqlits.Length; i++)
				{
					protos[i] = int.Parse(sqlits[i]);
				}

				return protos;
			}
			catch 
			{
				return new int[0];
			}
		}
	}
}
