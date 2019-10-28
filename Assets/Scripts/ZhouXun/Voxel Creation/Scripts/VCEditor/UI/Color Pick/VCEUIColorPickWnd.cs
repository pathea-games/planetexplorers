using UnityEngine;
using System.Collections;

public class VCEUIColorPickWnd : MonoBehaviour
{
	public TweenScale m_ScaleTweener;
	public VCEUIColorPick m_ColorPick;
	
	// Use this for initialization
	void Start ()
	{
		m_ScaleTweener.Play(true);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Input.GetMouseButtonDown(0) && transform.localScale.sqrMagnitude > 2.99f )
		{
			bool cast = false;
			RaycastHit rch;
			if ( Physics.Raycast(VCEInput.s_UIRay, out rch, 100, VCConfig.s_UILayerMask) )
			{
				Transform t = rch.collider.transform;
				while ( t != null )
				{
					if ( t.gameObject == this.gameObject )
					{
						cast = true;
						break;
					}
					t = t.parent;
				}
			}
			if ( !cast )
			{
				m_ScaleTweener.Play(false);
				Invoke("SelfDestroy", 0.5f);
			}
		}
	}
	
	void SelfDestroy ()
	{
		GameObject.Destroy(this.gameObject);
	}
}
