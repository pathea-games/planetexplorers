using UnityEngine;
using System.Collections;

public enum EVCEBrushType : int
{
	Select = 0,
	General = 1
}

public abstract class VCEBrush : MonoBehaviour
{
	public EVCEBrushType m_Type = EVCEBrushType.General;
	protected VCEAction m_Action = null;
	protected abstract void Do ();
	public virtual void Cancel () {}
}
