using UnityEngine;
using System.Collections;

public class AnimatorEventSender : StateMachineBehaviour 
{
	public string m_EnterEvent;
	public string m_EixtEvent;

	[System.Serializable]
	public class AnimEvent
	{
		public float eventTime;
		public string eventStr;
		[HideInInspector]
		public bool eventSended;
	}

	AnimatorCtrl m_AnimCtrl;

	public AnimEvent[] events;


	public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter (animator, stateInfo, layerIndex);
		if(null == m_AnimCtrl)
			m_AnimCtrl = animator.GetComponent<AnimatorCtrl>();

		SendMsg(animator, m_EnterEvent);

		for(int i = 0; i < events.Length; i++)
			events[i].eventSended = false;
	}

	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit (animator, stateInfo, layerIndex);

		if(null == m_AnimCtrl)
			m_AnimCtrl = animator.GetComponent<AnimatorCtrl>();

		SendMsg(animator, m_EixtEvent);

		for(int i = 0; i < events.Length; i++)
			if(!events[i].eventSended)
				SendMsg(animator, events[i].eventStr);
    }

	public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for(int i = 0; i < events.Length; i++)
		{
			if(!events[i].eventSended)
			{
				if(events[i].eventTime <= stateInfo.normalizedTime)
				{
					events[i].eventSended = true;
					SendMsg(animator, events[i].eventStr);
				}
			}
		}
	}

	void SendMsg(Animator animator, string eventStr)
	{
		if("" == eventStr)
			return;
		if(null != m_AnimCtrl)
			m_AnimCtrl.AnimEvent(eventStr);
		else
			animator.gameObject.SendMessage("AnimatorEvent", eventStr, SendMessageOptions.DontRequireReceiver);
	}
}
