using UnityEngine;
using System.Collections.Generic;
using PETools;
using Pathea;

public class PEAnimatorAttackTriggerEX : PEAnimatorAttack 
{
	[System.Serializable]
	public class Attack
	{
		public float startTime;
		public float endTime;
		public string[] bones;
		[HideInInspector]
		public PEAttackTrigger[] triggers;
		[HideInInspector]
		public bool active;
		[HideInInspector]
		public bool playAttack;
	}
	
	AnimatorCtrl m_AnimCtrl;
	[SerializeField] Attack[] m_Attacks;

    internal override void Init(Animator animator)
    {
        base.Init(animator);

		for(int i = 0; i < m_Attacks.Length; i++)
		{
			m_Attacks[i].triggers = new PEAttackTrigger[m_Attacks[i].bones.Length];
			for(int j = 0; j < m_Attacks[i].bones.Length; j++)
			{
				Transform tr = PEUtil.GetChild(animator.transform, m_Attacks[i].bones[j]);
		        if (tr != null)
		        {
		            PEAttackTrigger attackTrigger = tr.GetComponent<PEAttackTrigger>();
					if(null != attackTrigger)
						m_Attacks[i].triggers[j] = attackTrigger;
					else
						Debug.LogError("Can't find PEAttackTrigger:" + m_Attacks[i].bones[j]);
		        }
				else
					Debug.LogError("Can't find bone:" + m_Attacks[i].bones[j]);
			}
		}
    }

    void ClearTrigger(bool value)
    {
		if (m_Attacks == null)
			return;

        for (int i = 0; i < m_Attacks.Length; i++)
		{
			m_Attacks[i].active = value;
			m_Attacks[i].playAttack = false;
			if (m_Attacks[i].triggers == null)
				continue;

			for(int j = 0; j < m_Attacks[i].triggers.Length; j++)
			{
				if(null == m_Attacks[i].triggers[j])
					continue;
				m_Attacks[i].triggers[j].active = false;
				m_Attacks[i].triggers[j].ClearHitInfo();
			}
		}
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
		ClearTrigger(true);
		if(null == m_AnimCtrl)
			m_AnimCtrl = animator.GetComponent<AnimatorCtrl>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

		for (int i = 0; i < m_Attacks.Length; i++)
		{
			if(m_Attacks[i].active)
			{
				if(stateInfo.normalizedTime >= m_Attacks[i].endTime)
				{
					m_Attacks[i].active = false;
					for(int j = 0; j < m_Attacks[i].triggers.Length; j++)
					{
						if(null == m_Attacks[i].triggers[j])
							continue;
						m_Attacks[i].triggers[j].active = false;
						m_Attacks[i].triggers[j].ResetHitInfo();
					}

					if(null != m_AnimCtrl)
						m_AnimCtrl.AnimEvent("MonsterEndAttack");
					else
						animator.gameObject.SendMessage("AnimatorEvent", "MonsterEndAttack", SendMessageOptions.DontRequireReceiver);
				}
				else if(!m_Attacks[i].playAttack && stateInfo.normalizedTime >= m_Attacks[i].startTime)
				{
					m_Attacks[i].playAttack = true;
					for(int j = 0; j < m_Attacks[i].triggers.Length; j++)
					{
						if(null == m_Attacks[i].triggers[j])
							continue;
						m_Attacks[i].triggers[j].active = true;
					}
				}
			}
		}
    }

	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit (animator, stateInfo, layerIndex);
        ClearTrigger(false);
	}
}
