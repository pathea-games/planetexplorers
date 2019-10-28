using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using PETools;
using Pathea;

public class PEAnimatorAimHead : PEAnimatorState
{
    public AnimationCurve weightCurve;

    IKAimCtrl m_AimCtrl;

    internal override void Init(Animator animator)
    {
        base.Init(animator);

        if(m_AimCtrl == null && Entity != null)
        {
            m_AimCtrl = Entity.GetComponentInChildren<IKAimCtrl>();
        }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if(m_AimCtrl != null && Entity.attackEnemy != null)
        {
            m_AimCtrl.SetActive(true);
            m_AimCtrl.SetTarget(Entity.attackEnemy.CenterBone);
        }
    }

    //public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    base.OnStateUpdate(animator, stateInfo, layerIndex);
    //}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if(m_AimCtrl != null)
        {
            m_AimCtrl.SetActive(false);
            m_AimCtrl.SetTarget(null);
        }
    }
}
