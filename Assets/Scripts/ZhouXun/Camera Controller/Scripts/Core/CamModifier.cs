using UnityEngine;
using System.Collections;

public abstract class CamModifier : MonoBehaviour
{
	[HideInInspector] public CamController m_Controller;
	[HideInInspector] public Camera m_TargetCam;
	public int m_Tag = 0;
	public abstract void Do ();
	
	void OnDestroy () { GameObject.Destroy(gameObject); }

	public static bool MatchName (CamModifier iter, string name)
	{
		if ( iter == null ) return false;
		return (iter.name.ToLower() == name.ToLower());
	}
}
