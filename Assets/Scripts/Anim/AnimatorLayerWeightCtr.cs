using UnityEngine;
using System.Collections;

public class AnimatorLayerWeightCtr : StateMachineBehaviour
{
	public float fadeoutSpeed = 5f;
	public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate (animator, stateInfo, layerIndex);
		animator.SetLayerWeight(layerIndex, Mathf.Lerp(animator.GetLayerWeight(layerIndex), 0, fadeoutSpeed * Time.deltaTime));
	}

	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit (animator, stateInfo, layerIndex);
		animator.SetLayerWeight(layerIndex, 1f);
	}
}
