using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class do all the GL works on a camera
public class GlobalGLs : MonoBehaviour
{
	private static List<GLBehaviour> s_listGLs = new List<GLBehaviour> ();
	public static void AddGL (GLBehaviour gl)
	{
		if ( s_listGLs != null )
			s_listGLs.Add(gl);
		else
			Debug.LogError("Add GL Failed");
	}
	public static void RemoveGL (GLBehaviour gl)
	{
		if ( s_listGLs != null )
			s_listGLs.Remove(gl);
	}

	void OnPostRender()
	{
		if ( LSubTerrainMgr.Instance != null )
		{
			LSubTerrainMgr.Instance.m_Editor.DoGL();
		}
        //if ( ArtifactMgr.Instance != null )
        //{
        //    ArtifactMgr.Instance.DrawGL();
        //}
		foreach ( GLBehaviour gl in s_listGLs )
		{
			if ( gl != null )
			{
				gl.OnGL();
			}
		}
	}
}
