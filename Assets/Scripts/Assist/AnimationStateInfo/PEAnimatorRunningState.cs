using UnityEngine;
using System.Collections;

public class PEAnimatorRunningState : StateMachineBehaviour
{
    public string running;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        base.OnStateMachineEnter(animator, stateMachinePathHash);

        animator.SetBool(running, true);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        base.OnStateMachineExit(animator, stateMachinePathHash);

        animator.SetBool(running, false);
    }
}
