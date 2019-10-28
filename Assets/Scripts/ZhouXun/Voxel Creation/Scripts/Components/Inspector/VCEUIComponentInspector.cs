using UnityEngine;
using System.Collections;

public abstract class VCEUIComponentInspector : VCEUIInspector
{
	public VCESelectComponent m_SelectBrush;
	public GameObject m_ApplyButton;
	public abstract void Set (VCComponentData data);
	public abstract VCComponentData Get ();
	protected abstract bool Changed ();
	protected void Update () { m_ApplyButton.SetActive(Changed()); }
}
