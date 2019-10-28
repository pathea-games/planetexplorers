using UnityEngine;
using System.Collections;

public class IKGroundHelper : MonoBehaviour 
{
	[SerializeField]Transform m_ModeTrans;
	void Start()
	{
		if (null != m_ModeTrans)
			return;
		ModeTrans ();
	}

	void Update()
	{
		if(null != m_ModeTrans)
		{
			transform.position = m_ModeTrans.position;
			transform.rotation = m_ModeTrans.rotation;
		}
	}

	public void ModeTrans()
	{
		PEModelController mc = PETools.PEUtil.GetCmpt<PEModelController>(transform.parent);
		if(null != mc)
			m_ModeTrans = mc.transform;
	}
}
