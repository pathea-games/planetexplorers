using UnityEngine;
using System.Collections;
using SkillSystem;
using Pathea.Effect;

public class UIShakeEffct : MonoBehaviour , ISkEffectEntity
{
	SkInst m_Inst;
	public SkInst Inst { set{m_Inst = value;} }
	public AnimationCurve mForceToWeight;

	void Start()
	{
		float force = m_Inst._target.GetAttribute((int)Pathea.AttribType.ForceScale);
		Play(mForceToWeight.Evaluate(force));
	}

	public void Play(float value)
	{
		if (UIEffctMgr.Instance != null)
			UIEffctMgr.Instance.mShakeEffect.Play(value);
		GameObject.Destroy(this.gameObject);
	}
}
