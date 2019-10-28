using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The Select method of selecting voxels
public enum EVCESelectMethod
{
	None = 0,
	Box = 1
}

// Select method base class
public abstract class VCESelectMethod : GLBehaviour
{
	public VCESelectVoxel m_Parent;
	public bool m_Selecting;	// is during select action ?
	protected Dictionary<int, byte> m_Selection;	// pointer of selection ( bitmap )
	public bool m_NeedUpdate;	// use this to notice selection system
	protected VCIsoData m_Iso;	// pointer of current editing iso
	
	// Functions
	public virtual void Init ( VCESelectVoxel parent )
	{
		m_Parent = parent;
		m_Selecting = false;
		m_Selection = parent.m_SelectionMgr.m_Selection;
		m_Iso = VCEditor.s_Scene.m_IsoData;
	}
	public abstract void MainMethod ();
	protected abstract void Submit ();
}
