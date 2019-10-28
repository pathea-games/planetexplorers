using UnityEngine;
using System.Collections;

public class EffectLateupdateHelperEX_Part2 : MonoBehaviour
{
	EffectLateupdateHelperEX_Part1 m_Part1;

	void Awake()
	{
		m_Part1 = GetComponent<EffectLateupdateHelperEX_Part1>();
	}

	protected void LateUpdate ()
	{
		if(null != m_Part1.parentTrans && null != m_Part1.centerBone)
		{
			m_Part1.centerToParentLocal = m_Part1.centerBone.InverseTransformDirection(m_Part1.parentTrans.position - m_Part1.centerBone.position);
			m_Part1.parentForwardLocal = m_Part1.centerBone.InverseTransformDirection(m_Part1.parentTrans.forward);
		}
	}
}
