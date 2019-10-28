using UnityEngine;
using System.Collections;
using PETools;

public class PEAnimatorVision : StateMachineBehaviour
{
    public float radius;

    PEHearing[] hears;
    PEVision[] visions;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (hears == null)
            hears = animator.GetComponentsInChildren<PEHearing>();

        if (visions == null)
            visions = animator.GetComponentsInChildren<PEVision>();

        for (int i = 0; i < hears.Length; i++)
        {
            if (hears[i] != null)
                hears[i].AddBuff(radius, stateInfo.length);
        }

        for (int i = 0; i < visions.Length; i++)
        {
            if (visions[i] != null)
                visions[i].AddBuff(radius, stateInfo.length);
        }
    }
}
