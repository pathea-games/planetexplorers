using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EffectLateupdateHelperEX_Part2))]
public class EffectLateupdateHelperEX_Part1 : EffectLateupdateHelper 
{
	[SerializeField]string m_CenterBoneName = "Bip01 Spine3";
	public ParticleSystem[] particleSystems;
	[HideInInspector]
	public Transform centerBone;
	[HideInInspector]
	public Vector3 centerToParentLocal;
	[HideInInspector]
	public Vector3 parentForwardLocal;

	public Transform parentTrans{ get { return m_ParentTrans; } }

	void Reset()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	public override void Init(Transform parentTrans)
	{
		base.Init(parentTrans);
		centerBone = PETools.PEUtil.GetChild(parentTrans, m_CenterBoneName);
	}

	protected override void Update ()
	{
	}

	protected override void LateUpdate ()
	{
		if(null == m_ParentTrans || null == centerBone)
		{
			GameObject.Destroy(gameObject);
			return;
		}
//		m_LateUpdatePos = Vector3.ProjectOnPlane(m_CenterBone.position, m_ParentTrans.right) 
//			+ Quaternion.AngleAxis(Vector3.Angle(m_ParentTrans.up, forwardDir) - 90f, m_ParentTrans.right) * Vector3.ProjectOnPlane(m_CenterToParentLocal, m_ParentTrans.right);
		transform.position = centerBone.position + centerBone.TransformDirection(centerToParentLocal);

		parentForwardLocal = centerBone.TransformDirection(parentForwardLocal);

		transform.rotation = Quaternion.LookRotation(parentForwardLocal, Vector3.up);
	}
}
