using UnityEngine;
using System.Collections;
using SkillSystem;
using Pathea;

public class ShootWeaponEffect : MonoBehaviour, Pathea.Effect.ISkEffectEntity
{
	SkInst m_Inst;

	// Use this for initialization
	void Start () 
	{
		if(null == m_Inst || null == m_Inst._caster)
		{
			Destroy(gameObject);
			return;
		}

		PeEntity mono = m_Inst._caster.GetComponent<PeEntity>();
		if(null != mono)
			mono.SendMsg(EMsg.Battle_OnShoot);
		Destroy(gameObject);
	}

	#region ISkEffectEntity implementation

	SkillSystem.SkInst Pathea.Effect.ISkEffectEntity.Inst 
	{
		set { m_Inst = value; }
	}
	#endregion
}
