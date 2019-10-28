using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.Effect;

public class PEEquipmentEffect : MonoBehaviour 
{
	public int effectID = 97;

	public bool hideInFirstPerson = false;

	MainPlayerCmpt m_MainPlayer;

	ControllableEffect m_Effect;

	// Use this for initialization
	void Start () 
	{
		m_MainPlayer = GetComponentInParent<MainPlayerCmpt>();
		m_Effect = new ControllableEffect(effectID, transform);
	}

	void Update()
	{
		if(hideInFirstPerson && null != m_MainPlayer && null != m_Effect)
		{
			if(m_MainPlayer.firstPersonCtrl == m_Effect.active)
				m_Effect.active = !m_MainPlayer.firstPersonCtrl;
		}
	}

	void OnDestroy()
	{
		if(null != m_Effect)
		{
			m_Effect.Destory();
			m_Effect = null;
		}
	}
}
