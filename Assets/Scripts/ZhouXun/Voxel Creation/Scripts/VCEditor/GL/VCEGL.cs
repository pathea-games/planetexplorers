using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VCEGL : MonoBehaviour
{
	public GameObject m_ParentObject;
	void Awake()
	{

	}
	void Start()
	{
		CreateLineMaterials();
	}
	static public int SortByOrder (GLBehaviour a, GLBehaviour b) { return a.m_RenderOrder - b.m_RenderOrder; }
	void OnPostRender()
	{
		GLBehaviour[] GLArray = m_ParentObject.GetComponentsInChildren<GLBehaviour>();
		List<GLBehaviour> gl_list = new List<GLBehaviour> ();
		
		foreach ( GLBehaviour gl in GLArray )
		{
			gl_list.Add(gl);
		}
		
		gl_list.Sort(SortByOrder);
		
		foreach ( GLBehaviour gl in gl_list )
		{
			// If the gl is active
			if ( gl != null && gl.gameObject.activeInHierarchy && gl.enabled )
			{
				// Set the current material
				Material mat = gl.m_Material;
				if ( mat == null )
					mat = m_LineMaterial;
				int pcnt = mat.passCount;

				for ( int i = 0; i < pcnt; ++i )
				{
					GL.PushMatrix();
					
					// Call OnGL function
					if ( mat.SetPass(i) )
						gl.OnGL();

					GL.PopMatrix();
				}
			}
		}
	}
	//void OnDestroy()
	//{
	//	Material.Destroy(m_LineMaterial);
	//}
	
	// GL Material
	private Material m_LineMaterial;
	private void CreateLineMaterials()
	{
    	if( !m_LineMaterial ) 
		{
	        m_LineMaterial = WhiteCat.PEVCConfig.instance.handleMaterial;
	        m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
	        m_LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    	}
	}
}
