using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class TrigrassGenTest : MonoBehaviour 
{
	public int m_GrassCount = 4000;
	
	public Color m_GrassColor = Color.white;
	
	public int m_ProtoType0 = 14;
	public int m_ProtoType1 = 13;
	public int m_ProtoType2 = 15;
	
	public int m_RandSeed = 0;
	
	private MeshFilter m_Mesh = null;
	public Vector3 m_StartCoord = Vector3.zero;
	public float m_GenAreaSize = 128f;
	
	private List<GrassInstance>	m_Grasses = new List<GrassInstance>();
//	private static Dictionary<int, List<GrassInstance>> s_map = new Dictionary<int, List<GrassInstance>> ();
	
	public bool m_RegenerateNow = false;
	
	void ReGen ()
	{
		m_Grasses.Clear();
		SimplexNoise noise = new SimplexNoise ();
		//Random.seed = Time.frameCount + Mathf.RoundToInt(Random.value*1000000);
		RaycastHit rch;
		for (int i = 0; i < m_GrassCount; i++)
		{
			Vector3 origin = new Vector3(Random.value, 1, Random.value) * m_GenAreaSize + m_StartCoord;
//			IntVector3 origin_i = new IntVector3(origin);
//			int hash = origin_i.GetHashCode();
//			
//			if ( !m_map.ContainsKey(hash) )
//				m_map.Add(hash, new List<GrassInstance>());
//			if ( m_map[hash].Count > 1 )
//			{
//				float p = Mathf.Pow(0.7f, (float)(m_map[hash].Count));
//				if ( Random.value > p )
//					continue;
//			}
			
			origin.y = 512f;
			
			if ( Physics.Raycast(origin, Vector3.down, out rch, 1024, 1 << Pathea.Layer.VFVoxelTerrain) ) 
			{
				float nx = (float)(noise.Noise( rch.point.x/32, rch.point.y/32, rch.point.z/32 )) + 1;
				float ny = (float)(noise.Noise( (rch.point.x+m_GenAreaSize)/32, (rch.point.y+m_GenAreaSize)/32, (rch.point.z+m_GenAreaSize)/32 )) + 1;
				float nz = (float)(noise.Noise( rch.point.y/16, rch.point.z/16, rch.point.x/16 )) + 1;

				nx = Mathf.Pow(nx, 15);
				ny = Mathf.Pow(ny, 15);
				nz = Mathf.Pow(nz, 15);
				
				float sum = nx + ny + nz;
				nx /= sum;
				ny /= sum; ny += nx;
				nz /= sum; nz += ny;
				
				GrassInstance gi = new GrassInstance();
				gi.Position = rch.point;
				gi.Normal = rch.normal;
				gi.ColorF = m_GrassColor;
				
				float r = Random.value;
				if ( r < nx )
					gi.Prototype = m_ProtoType0;
				else if ( r < ny )
					gi.Prototype = m_ProtoType1;
				else if ( r < nz )
					gi.Prototype = m_ProtoType2;
				
				m_Grasses.Add(gi);
//				m_map[hash].Add(gi);
			}
		}
		
		TrigrassMeshComputer.ComputeMesh(m_Grasses, 0, m_Mesh);
		m_Grasses.Clear();
	}
	
	void OnEnable ()
	{
		ReGen ();
	}

	// Use this for initialization
	void Start () 
	{
		m_Mesh = GetComponent<MeshFilter>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_RegenerateNow )
		{
			ReGen();
			m_RegenerateNow = false;
		}
	}
}
