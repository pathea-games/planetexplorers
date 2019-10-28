using UnityEngine;
using System.Collections;

public class PEAnimatorBoneRotate : StateMachineBehaviour
{
    public float delayTime;
    public float endTime;
    public float rotateSpeed;

    bool m_IsActive;
    float m_RotateSpeed;
    PEBoneRotation m_BoneRoattion;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (m_BoneRoattion == null)
            m_BoneRoattion = animator.GetComponentInChildren<PEBoneRotation>();

        if(m_BoneRoattion != null)
        {
            m_RotateSpeed = m_BoneRoattion.rotateSpeed;

            if(rotateSpeed > PETools.PEMath.Epsilon)
            {
                m_BoneRoattion.rotateSpeed = rotateSpeed;
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (m_BoneRoattion != null)
        {
            if (stateInfo.normalizedTime >= endTime)
                m_BoneRoattion.rotateAuto = false;
            else if (stateInfo.normalizedTime >= delayTime)
                m_BoneRoattion.rotateAuto = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (m_BoneRoattion != null)
        {
            m_BoneRoattion.rotateAuto = false;
            m_BoneRoattion.rotateSpeed = m_RotateSpeed;
        }
    }
}
