using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using PETools;
using Pathea;

public class PEAnimatorLaser : StateMachineBehaviour
{
    public float length;
    public string[] ikBones;
    public AnimationCurve weightCurve;
    public AnimationCurve posCurve;

    AimIK[] m_IKs;
    TargetCmpt m_Target;
    PeTrans m_Trans;

    Vector3 m_Start;
    Vector3 m_End;

    void GetPoint(Transform tr, Vector3 target)
    {
        Vector3 tarDir = Vector3.ProjectOnPlane(target - tr.position, Vector3.up);
        Vector3 selDir = Vector3.ProjectOnPlane(tr.forward, Vector3.up);
        //Vector3 right = Vector3.ProjectOnPlane(tr.right, Vector3.up);

        Vector3 v1 = Vector3.Project(tarDir, selDir);
        Vector3 v2 = tarDir - v1;

		m_Start = target + v2.normalized * length;
		m_End = target - v2.normalized * length;
    }

    void GetIKs(Transform root)
    {
        if (m_IKs == null)
        {
            m_IKs = new AimIK[0];

            if (root != null && root.parent != null)
            {
                Transform ikTrans = PEUtil.GetChild(root.parent, "GrounderIK");
                if (ikTrans != null)
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

        if (m_Trans == null)
            m_Trans = animator.GetComponentInParent<PeTrans>();

        if (m_Target == null || m_Target.GetAttackEnemy() == null)
            return;

        if (m_IKs == null || m_IKs.Length <= 0)
            return;

        if(m_Trans != null)
            GetPoint(m_Trans.trans, m_Target.GetAttackEnemy().position);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        float normalizedTime = stateInfo.normalizedTime;
        float weightValue = Mathf.Clamp01(weightCurve.Evaluate(normalizedTime));
		float posValue = Mathf.Clamp01(posCurve.Evaluate(normalizedTime));
        Vector3 ikPos = Vector3.Lerp(m_Start, m_End, posValue);

        for (int i = 0; i < m_IKs.Length; i++)
        {
			m_IKs[i].solver.IKPosition = ikPos;
			m_IKs[i].solver.IKPositionWeight = weightValue;
        }

		//Debug.DrawLine (m_Start, m_End, Color.cyan);
		Debug.DrawLine (m_Start, ikPos, Color.cyan);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        for (int i = 0; i < m_IKs.Length; i++)
        {
            m_IKs[i].solver.target = null;
        }
    }
}
