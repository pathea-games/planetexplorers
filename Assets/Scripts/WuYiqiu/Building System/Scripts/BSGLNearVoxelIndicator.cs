using UnityEngine;
using System.Collections;

public class BSGLNearVoxelIndicator : GLBehaviour 
{

	public static IntVector3 InvalidPos { get { return new IntVector3 (-100,-100,-100); } }
	public IntVector3 m_Center = InvalidPos;
	public int m_Expand = 4;
	public Gradient m_BoxColors;

//	Material _LineMaterial = null;

	public bool ShowBlock = true;

	public int minVol = BSMath.MC_ISO_VALUE;


	void Start ()
	{
		GlobalGLs.AddGL(this);


	}

	void Update ()
	{
		BSMath.DrawTarget dtar;

		if (ShowBlock)
		{
			if ( BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay( Input.mousePosition), BuildingMan.Voxels,  out dtar,  minVol, true, BuildingMan.Datas))
			{
				m_Center = dtar.snapto;

			}
			else
			{
				m_Center = InvalidPos;
			}
		}
		else
		{
			if ( BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay( Input.mousePosition), BuildingMan.Voxels,  out dtar,  minVol))
			{
				m_Center = dtar.snapto;
				
			}
			else
			{
				m_Center = InvalidPos;
			}
		}
	}
	
	void OnDisable()
	{
		m_Center = InvalidPos;
	}
	
	public override void OnGL ()
	{
		if (!enabled || !gameObject.activeInHierarchy)
			return;

		if (BSInput.s_MouseOnUI)
			return;

//		if (_LineMaterial == null)
//		{
//			_LineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
//			_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
//			_LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
//		}

		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Unlit/Transparent Colored"));
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}

		GL.PushMatrix();

//		_LineMaterial.SetPass(0);
		m_Material.SetPass(0);

		for ( int x = -m_Expand; x <= m_Expand; ++x )
		{
			for ( int y = -m_Expand; y <= m_Expand; ++y )
			{
				for ( int z = -m_Expand; z <= m_Expand; ++z )
				{
					IntVector3 pos = new IntVector3 (x+m_Center.x, y+m_Center.y, z+m_Center.z);

					VFVoxel voxel =  VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y, pos.z);

					// contain_block ? 
					bool contain_block = false;
					if (ShowBlock)
					{
						for (float _x = pos.x - 0.5f; _x < pos.x + 0.1f; _x += 0.5f)
						{
							for (float _y = pos.y - 0.5f; _y < pos.y + 0.1f; _y += 0.5f)
							{
								for (float _z = pos.z - 0.5f; _z < pos.z + 0.1f; _z += 0.5f)
								{
									B45Block block = Block45Man.self.DataSource.SafeRead(Mathf.FloorToInt(_x * Block45Constants._scaleInverted), 
						            													Mathf.FloorToInt(_y * Block45Constants._scaleInverted),
						            													Mathf.FloorToInt(_z	* Block45Constants._scaleInverted));

									if (block.blockType != 0 || block.materialType != 0)
									{
										contain_block = true;
										break;
									}
								}
							}
						}
					}

					Bounds box = new Bounds (Vector3.zero, Vector3.zero);
					box.SetMinMax(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f), 
					              new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f));




					float sv = (float)(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z))) / (float)(m_Expand);
					Color lc = m_BoxColors.Evaluate(sv);

					bool draw = false;
					if (voxel.Volume > 1)
					{
						Color vc = m_BoxColors.Evaluate((float)voxel.Volume / 255.0f);
						lc.r = vc.r;
						lc.g = vc.g;
						lc.b = vc.b;
						draw = true;
					}
					else if (contain_block)
					{
						draw = true;
					}

					if (draw)
					{
						Color bc = lc;
						lc.a *= 1.5f;

						float shrink = 0.04f;
						if (Camera.main != null)
						{
							float dist_min = (Camera.main.transform.position - box.min).magnitude;
							float dist_max = (Camera.main.transform.position - box.max).magnitude;
							
							dist_max = dist_max > dist_min ? dist_max : dist_min;
							
							shrink = Mathf.Clamp(dist_max * 0.001f, 0.04f, 0.2f);
						}
						
						box.min -= new Vector3(shrink, shrink, shrink);
						box.max += new Vector3(shrink, shrink, shrink);

						// Edge
						GL.Begin(GL.LINES);
						GL.Color(lc);
						GL.Vertex3( box.min.x, box.min.y, box.min.z );
						GL.Vertex3( box.max.x, box.min.y, box.min.z );
						GL.Vertex3( box.min.x, box.min.y, box.max.z );
						GL.Vertex3( box.max.x, box.min.y, box.max.z );
						GL.Vertex3( box.min.x, box.max.y, box.min.z );
						GL.Vertex3( box.max.x, box.max.y, box.min.z );
						GL.Vertex3( box.min.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.max.z );
						
						GL.Vertex3( box.min.x, box.min.y, box.min.z );
						GL.Vertex3( box.min.x, box.max.y, box.min.z );
						GL.Vertex3( box.min.x, box.min.y, box.max.z );
						GL.Vertex3( box.min.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.min.y, box.min.z );
						GL.Vertex3( box.max.x, box.max.y, box.min.z );
						GL.Vertex3( box.max.x, box.min.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.max.z );
						
						GL.Vertex3( box.min.x, box.min.y, box.min.z );
						GL.Vertex3( box.min.x, box.min.y, box.max.z );
						GL.Vertex3( box.min.x, box.max.y, box.min.z );
						GL.Vertex3( box.min.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.min.y, box.min.z );
						GL.Vertex3( box.max.x, box.min.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.min.z );
						GL.Vertex3( box.max.x, box.max.y, box.max.z );
						GL.End();
						
						// Face
						GL.Begin(GL.QUADS);
						GL.Color(bc);
						GL.Vertex3( box.min.x, box.min.y, box.min.z );
						GL.Vertex3( box.min.x, box.min.y, box.max.z );
						GL.Vertex3( box.min.x, box.max.y, box.max.z );
						GL.Vertex3( box.min.x, box.max.y, box.min.z );
						
						GL.Vertex3( box.max.x, box.min.y, box.min.z );
						GL.Vertex3( box.max.x, box.min.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.min.z );
						
						GL.Vertex3( box.min.x, box.min.y, box.min.z );
						GL.Vertex3( box.min.x, box.min.y, box.max.z );
						GL.Vertex3( box.max.x, box.min.y, box.max.z );
						GL.Vertex3( box.max.x, box.min.y, box.min.z );
						
						GL.Vertex3( box.min.x, box.max.y, box.min.z );
						GL.Vertex3( box.min.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.min.z );
						
						GL.Vertex3( box.min.x, box.min.y, box.min.z );
						GL.Vertex3( box.min.x, box.max.y, box.min.z );
						GL.Vertex3( box.max.x, box.max.y, box.min.z );
						GL.Vertex3( box.max.x, box.min.y, box.min.z );
						
						GL.Vertex3( box.min.x, box.min.y, box.max.z );
						GL.Vertex3( box.min.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.max.y, box.max.z );
						GL.Vertex3( box.max.x, box.min.y, box.max.z );
						GL.End();
					}
				}
			}
		}

		GL.PopMatrix();
	}

}
