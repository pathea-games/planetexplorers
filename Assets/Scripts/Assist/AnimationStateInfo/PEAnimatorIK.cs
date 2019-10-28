using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using PETools;
using Pathea;

public class PEAnimatorIK : StateMachineBehaviour
{
    public string[] ikBones;
    public AnimationCurve weightCurve;

    AimIK[] m_IKs;
    TargetCmpt m_Target;

    void GetIKs(Transform root)
    {
		if(m_IKs == null && root != null)
        {
            m_IKs = new AimIK[0];

            Transform ikTrans = PEUtil.GetChild(root, "GrounderIK");
            if(ikTrans != null)
            {
                for (int i = 0; i < ikBones.Length; i++)
                {
                    Transform tr = PEUtil.GetChild(ikTrans, ikBones[i]);
                    if (tr != null)
                    {
                        AimIK ik = tr.GetComponent<AimIK>();
                        if (ik != null)
                        {
                            System.Array.Resize(ref m_IKs, m_IKs.Length + 1);
                            m_IKs[m_IKs.Length - 1] = ik;
                        }
                    }
                }
            }
        }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

		if (m_Target == null)
			m_Target = animator.GetComponentInParent<TargetCmpt>();

        if (m_Target == null || m_Target.GetAttackEnemy() == null)
            return;

		if (m_IKs == null)
			GetIKs(m_Target.transform);

        if (m_IKs == null || m_IKs.Length <= 0)
            return;

        for (int i = 0; i < m_IKs.Length; i++)
        {
            m_IKs[i].solver.target = m_Target.GetAttackEnemy().CenterBone;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (m_IKs != null && m_IKs.Length > 0)
        {
            for (int i = 0; i < m_IKs.Length; i++)
            {
                if(m_IKs[i] != null)
                    m_IKs[i].solver.IKPositionWeight = weightCurve.Evaluate(stateInfo.normalizedTime);
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (m_IKs != null && m_IKs.Length > 0)
        {
            for (int i = 0; i < m_IKs.Length; i++)
            {
                if(m_IKs[i] != null)
				{
					m_IKs[i].solver.target = null;
					m_IKs[i].solver.IKPositionWeight = 0;
				}
            }
        }
    }
}
