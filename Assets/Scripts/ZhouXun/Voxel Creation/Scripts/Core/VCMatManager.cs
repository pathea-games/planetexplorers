using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// This class manages U3D resources (Materials & Textures) used for VOXEL CREATIONs
// !! NOT for VCEditor !!

public class VCMatManager : MonoBehaviour
{
	private static VCMatManager s_Instance = null;
	public static VCMatManager Instance { get { return s_Instance; } }
	
	// U3D Resource Collections
	public Dictionary<ulong, Material> m_mapMaterials;
	public Dictionary<ulong, RenderTexture> m_mapDiffuseTexs;
	public Dictionary<ulong, RenderTexture> m_mapBumpTexs;
	public Dictionary<ulong, Texture2D> m_mapPropertyTexs;
	
	// Reference Counter
	public Dictionary<ulong, int> m_mapMatRefCounters;
	
	#region U3D_INTERNAL_FUNCS
	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
	}
	
	void Start ()
	{
		// Init. u3d resource collections
		this.m_mapMaterials = new Dictionary<ulong, Material> ();
		this.m_mapDiffuseTexs = new Dictionary<ulong, RenderTexture> ();
		this.m_mapBumpTexs = new Dictionary<ulong, RenderTexture> ();
		this.m_mapPropertyTexs = new Dictionary<ulong, Texture2D> ();
		this.m_mapMatRefCounters = new Dictionary<ulong, int> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	// On destroy
	void OnDestroy ()
	{
		s_Instance = null;
	}
	#endregion
}
