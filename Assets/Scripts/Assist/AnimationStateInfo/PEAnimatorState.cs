using UnityEngine;
using System.Collections;
using Pathea;

public class PEAnimatorState : StateMachineBehaviour
{
    bool m_Init;
    PeEntity m_Entity;

    public PeEntity Entity { get { return m_Entity; } }

    internal virtual void Init(Animator animator) { }

    void InitAnimator(Animator animator)
    {
        if(!m_Init)
        {
            m_Entity = animator.GetComponentInParent<PeEntity>();

            Init(animator);
            m_Init = true;
        }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        InitAnimator(animator);
    }
}
