using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class VoxelGrassGenTest : MonoBehaviour 
{
	public Color m_GrassColor = Color.white;
	
	public int m_ProtoType = 0;
	
	public int m_RandSeed = 0;
	
	private MeshFilter m_Mesh = null;
	public Vector3 m_StartCoord = Vector3.zero;
	public float m_GenAreaSize = 32f;
	public float m_Density = 1f;
	
	private List<VoxelGrassInstance> m_Grasses = new List<VoxelGrassInstance>();
	
	public bool m_RegenerateNow = false;
	
	void ReGen ()
	{
		VoxelGrassMeshComputer.Init();
		m_Grasses.Clear();
		RaycastHit rch;
		
		for ( float x = m_StartCoord.x + 0.5f; x < m_StartCoord.x + m_GenAreaSize; ++x )
		{
			for ( float z = m_StartCoord.z + 0.5f; z < m_StartCoord.z + m_GenAreaSize; ++z )
			{
				Vector3 origin = new Vector3(x, 512f, z);
				
				if ( Physics.Raycast(origin, Vector3.down, out rch, 1024, 1 << Pathea.Layer.VFVoxelTerrain) ) 
				{
					VoxelGrassInstance gi = new VoxelGrassInstance();
					gi.Position = rch.point;
					gi.Density = 1;
					gi.Normal = rch.normal;
					gi.ColorF = m_GrassColor;
					gi.Prototype = m_ProtoType;
					
					m_Grasses.Add(gi);
				}
			}
		}
		
		VoxelGrassMeshComputer.ComputeMesh(m_Grasses, 0, m_Mesh, m_Density);
		m_Grasses.Clear();
	}
	
	void OnEnable ()
	{
		m_Mesh = GetComponent<MeshFilter>();
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
