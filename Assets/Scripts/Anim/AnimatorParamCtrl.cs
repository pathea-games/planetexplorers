using UnityEngine;
using System.Collections;

public class AnimatorParamCtrl : StateMachineBehaviour
{
	public enum ParamType
	{
		Boolean,
		Trigger,
		Integer,
		Floating
	}

	[System.Serializable]
	public class CtrlAction
	{

		public string paramName;
		public ParamType type;
		public bool targetBool;
		public float targetValue;
	}

	public CtrlAction[] startActions;

	public CtrlAction[] endActions;

	public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for(int i = 0; i < startActions.Length; i++)
			ChangeParam(startActions[i], animator);
	}
	
	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for(int i = 0; i < endActions.Length; i++)
			ChangeParam(endActions[i], animator);
	}

	void ChangeParam(CtrlAction action, Animator animator)
	{
		switch(action.type)
		{
		case ParamType.Boolean:
			animator.SetBool(action.paramName, action.targetBool);
			break;
		case ParamType.Trigger:
			if(action.targetBool)
				animator.SetTrigger(action.paramName);
			else
				animator.ResetTrigger(action.paramName);
			break;
		case ParamType.Integer:
			animator.SetInteger(action.paramName, Mathf.RoundToInt(action.targetValue));
			break;
		case ParamType.Floating:
			animator.SetFloat(action.paramName, action.targetValue);
			break;
		}
	}
}
