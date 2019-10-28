using UnityEngine;
using System.Collections;

public abstract class VCESelect : VCEBrush
{
	public GameObject m_MainInspectorRes;
	public GameObject m_MainInspector;
	
	protected override void Do () {}
	public override void Cancel () { ClearSelection(); }
	public abstract void ClearSelection();
}
