using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using PETools;
using Pathea;

public class PEAnimatorAim : StateMachineBehaviour
{
    public string[] ikBones;
    public AnimationCurve weightCurve;

    AimIK[] m_IKs;
    TargetCmpt m_Target;

    Vector3 m_Pos;

    void GetIKs(Transform root)
    {
        if(m_IKs == null)
        {
            m_IKs = new AimIK[0];

            if(root != null && root.parent != null)
            {
                Transform ikTrans = PEUtil.GetChild(root.parent, "GrounderIK");
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
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (m_IKs == null)
            GetIKs(animator.transform);

        if (m_Target == null)
            m_Target = animator.GetComponentInParent<TargetCmpt>();

        if (m_Target == null || m_Target.GetAttackEnemy() == null)
            return;

        if (m_IKs == null || m_IKs.Length <= 0)
            return;

        m_Pos = m_Target.GetAttackEnemy().centerPos;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        for (int i = 0; i < m_IKs.Length; i++)
        {
            m_IKs[i].solver.IKPosition = m_Pos;
            m_IKs[i].solver.IKPositionWeight = weightCurve.Evaluate(stateInfo.normalizedTime);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        for (int i = 0; i < m_IKs.Length; i++)
        {
            m_IKs[i].solver.IKPosition = Vector3.zero;
        }
    }
}
