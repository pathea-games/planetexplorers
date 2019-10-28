using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEVoxelSelection : MonoBehaviour
{
	public Dictionary<int, byte> m_Selection;
	public GLSelectionBoxes m_GL;

	public void ClearSelection ()
	{
		if ( m_Selection != null )
		{
			m_Selection.Clear();
			m_GL.m_Boxes.Clear();
		}
	}
	
	// Use this for initialization
	void Start ()
	{
		m_Selection = new Dictionary<int, byte> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void RebuildSelectionBoxes()
	{
		LeastBox.Calculate(m_Selection, ref m_GL.m_Boxes);
	}
}
