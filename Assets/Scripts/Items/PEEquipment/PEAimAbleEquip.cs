using UnityEngine;
using System;
using Pathea;

[Serializable]
public class PEAimAttr
{
	public const float	AccuracyDis = 100f;
	// 枪头（枪管）
	public Transform 	m_AimTrans;

	// 准星类型（枪管）
	public UISightingTelescope.SightingType m_AimPointType;

	// 开枪稳定性（枪管）
	[Range(0.1f,1f)]
	public float		m_FireStability = 0.5f;

	// 精确度范围（枪托）
	public float 		m_AccuracyMin = 1f;
	public float		m_AccuracyMax = 5f;

	// 晃动周期（枪托）
	public float		m_AccuracyPeriod = 5f;

	// 精确度范围增加率（枪管）
	public float		m_AccuracyDiffusionRate = 1f;

	// 精确度回收速度（枪托）
	public float		m_AccuracyShrinkSpeed = 3f;

	// 枪口上扬最大值（枪托）
	public float		m_CenterUpDisMax = 10f;

	// 开枪上扬增加量（枪管）
	public float		m_CenterUpDisPerShoot = 3f;

	// 枪口恢复速度（枪托）
	public float		m_CenterUpShrinkSpeed = 3f;

	public bool			m_ApplyAimIK = true;

	public bool 		m_SyncIKWhenAnim = true;
}

public class PEAimAbleEquip : PEHoldAbleEquipment
{
	public PEAimAttr	m_AimAttr = new PEAimAttr();
	protected IKCmpt	m_IKCmpt;

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_IKCmpt = m_Entity.GetCmpt<IKCmpt>();
	}

}