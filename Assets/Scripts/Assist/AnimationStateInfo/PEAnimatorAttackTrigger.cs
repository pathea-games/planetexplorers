using UnityEngine;
using System.Collections;
using PETools;
using Pathea;

public class PEAnimatorAttackTrigger : PEAnimatorAttack 
{
    public float startTime;
    public float endTime;
    public string[] bones;

    //float m_StartTime = 0.0f;

    PEAttackTrigger[] m_Triggers;

    internal override void Init(Animator animator)
    {
        base.Init(animator);

        m_Triggers = new PEAttackTrigger[0];

        foreach (string bone in bones)
        {
            Transform tr = PEUtil.GetChild(animator.transform, bone);
            if (tr != null)
            {
                PEAttackTrigger attackTrigger = tr.GetComponent<PEAttackTrigger>();
				if(null != attackTrigger)
                {
                    System.Array.Resize(ref m_Triggers, m_Triggers.Length+1);
                    m_Triggers[m_Triggers.Length - 1] = attackTrigger;
                }
            }
        }
    }

    void ClearHitInfo()
    {
        for (int i = 0; i < m_Triggers.Length; i++)
        {
            if(m_Triggers[i] != null)
            {
                m_Triggers[i].ClearHitInfo();
            }
        }
    }

    void ResetHitInfo()
    {
        for (int i = 0; i < m_Triggers.Length; i++)
        {
            if(m_Triggers[i] != null)
            {
                m_Triggers[i].ClearHitInfo();
            }
        }
    }

    void ActivateTrigger(bool value)
    {
        for (int i = 0; i < m_Triggers.Length; i++)
        {
            if(m_Triggers[i] != null)
            {
                m_Triggers[i].active = value;
            }
        }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        ClearHitInfo();
        ActivateTrigger(false);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if(stateInfo.normalizedTime > endTime || stateInfo.normalizedTime < startTime)
        {
            ResetHitInfo();
            ActivateTrigger(false);
        }
        else
        {
            ActivateTrigger(true);
        }
    }

	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit (animator, stateInfo, layerIndex);

        ClearHitInfo();
        ActivateTrigger(false);
	}
}
