using UnityEngine;
using System.Collections;
using Pathea;
using System;
using ItemAsset;

public class PEBow : PEAimAbleEquip, IWeapon, IAimWeapon
{
	public AttackMode[]	m_AttackMode;

	public int[] 	m_CostItemID;
	public int[] 	m_SkillID;
    public string[] m_Idles;


	int				m_CurIndex;
	public int		curItemIndex
	{
		get{ return m_CurIndex; }
		set
		{
			m_CurIndex = Mathf.Min(value, m_CostItemID.Length - 1);
			if(null != m_ItemAmmoAttr)
				m_ItemAmmoAttr.index = m_CurIndex;
		}
	}
	public int		curItemID { get { return m_CostItemID[curItemIndex]; } }
	public int		skillID { get { return m_SkillID[curItemIndex]; } }

	public string 		m_ReloadAnim = "BowReload";
	const string		m_ArrowBagBone = "Bow_box";
	const string		m_ArrowFinger = "Bip01 R Finger21";

	public Transform	m_ArrowBagTrans;
	public Transform 	m_LineBone;
	Vector3				m_LineBoneDefaultPos;
	public GameObject[]	m_ArrowModel;

	ItemAsset.Arrow 	m_ItemAmmoAttr;
	Animator  			m_Anim;
	GameObject			m_ShowArrow;
	Transform			m_FingerBone;

	float				m_AimTime = 0.3f;
	float 				m_StartTime;
	bool				m_BowOpen;

	public override void InitEquipment(PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Anim = GetComponentInChildren<Animator>();
		m_MotionEquip = m_Entity.GetCmpt<Motion_Equip>();
		m_ItemAmmoAttr = itemObj.GetCmpt<ItemAsset.Arrow>();
		if (null != m_ItemAmmoAttr)
		{
			m_CurIndex = m_ItemAmmoAttr.index;
		}

		if(null != m_ArrowBagTrans)
		{
			m_ArrowBagTrans.gameObject.SetActive(true);
			m_View.AttachObject(m_ArrowBagTrans.gameObject, m_ArrowBagBone);
			m_ArrowBagTrans.localPosition = Vector3.zero;
			m_ArrowBagTrans.localRotation = Quaternion.identity;
			m_ArrowBagTrans.localScale = Vector3.one;
		}

		m_FingerBone = PETools.PEUtil.GetChild(m_View.modelTrans, m_ArrowFinger);
		if(null != m_LineBone)
			m_LineBoneDefaultPos = m_LineBone.localPosition;

		if(m_ArrowModel.Length != m_CostItemID.Length || m_SkillID.Length != m_CostItemID.Length)
			Debug.LogError("ArrowModelNum, ItemNum, SkillNum not match");

		m_StartTime = 0f;
		m_BowOpen = false;
	}
	
	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if(null != m_ArrowBagTrans)
		{
			m_View.DetachObject(m_ArrowBagTrans.gameObject);
			Destroy(m_ArrowBagTrans.gameObject);
		}
		if(null != m_ShowArrow)
		{
			m_View.DetachObject(m_ShowArrow);
			Destroy(m_ShowArrow.gameObject);
		}
	}

	public override void ResetView ()
	{
		base.ResetView ();
		if(null != m_ShowArrow)
			GameObject.Destroy(m_ShowArrow);
	}

	public void OnShoot()
	{
		if(null != m_Anim)
			m_Anim.SetTrigger("Shoot");
	}

	public void SetBowOpenState(bool openBow)
	{
		if(null != m_Anim)
			m_Anim.SetBool("Open", openBow);
		m_BowOpen = openBow;
	}

	public void SetArrowShowState(bool show)
	{
		if(null != m_ArrowModel && m_ArrowModel.Length > 0)
		{
			if(null != m_ShowArrow)
			{
				m_View.DetachObject(m_ShowArrow);
				Destroy(m_ShowArrow.gameObject);
			}
			if(show && null != m_ArrowModel[curItemIndex])
			{
				m_ShowArrow = Instantiate(m_ArrowModel[curItemIndex]) as GameObject;
				if(null != m_ShowArrow)
				{
					m_View.AttachObject(m_ShowArrow, m_ArrowFinger);
					m_StartTime = Time.time;
				}
			}
		}
	}

	void LateUpdate()
	{
		if(null != m_ShowArrow)
		{
			m_ShowArrow.transform.rotation = Quaternion.LookRotation(Vector3.Slerp(m_AimAttr.m_AimTrans.right, (m_AimAttr.m_AimTrans.position - m_ShowArrow.transform.position).normalized,
			                                                                       Mathf.Clamp01((Time.time - m_StartTime) / m_AimTime)));
		}

		if(null != m_LineBone)
		{
			if(m_BowOpen && null != m_FingerBone)
			{
				m_LineBone.position = m_FingerBone.position;
			}
			else
			{
				m_LineBone.localPosition = m_LineBoneDefaultPos;
			}
		}
	}

	public ItemAsset.ItemObject ItemObj{ get{ return m_ItemObj; } }

	public void HoldWeapon(bool hold)
	{
//		if(hold)
			m_MotionEquip.ActiveWeapon(this, hold);
//		else
//			m_MotionEquip.ActiveWeapon(this, hold, true);
	}

	public bool HoldReady { get { return m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask); } }
	
	public bool UnHoldReady { get { return !m_MotionMgr.IsActionRunning(m_HandChangeAttr.m_ActiveActionType); } }

	public AttackMode[] GetAttackMode ()
	{
		return m_AttackMode;
	}

	public bool CanAttack(int index = 0)
	{
		return m_MotionMgr.CanDoAction(PEActionType.BowShoot);
	}

	public void Attack(int index = 0, SkillSystem.SkEntity targetEntity = null)
	{
		if(null != m_MotionEquip)
			m_MotionEquip.SetTarget(targetEntity);
		if(null != m_MotionMgr)
			m_MotionMgr.DoAction(PEActionType.BowShoot);
		if(null != m_AttackMode && m_AttackMode.Length > index)
			m_AttackMode[index].ResetCD();
	}

	public bool AttackEnd(int index = 0)
	{
		if(null != m_MotionMgr)
			return !m_MotionMgr.IsActionRunning(PEActionType.BowShoot) 
				&& !m_MotionMgr.IsActionRunning(PEActionType.BowReload);

		return true;
	}

	public virtual bool IsInCD(int index = 0)
	{
		if(null != m_AttackMode && m_AttackMode.Length > index)
			return m_AttackMode[index].IsInCD();
		return false;
	}

	
	public virtual void SetAimState(bool aimState)
	{
		if(aimState)
			m_MotionMgr.ContinueAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.BowShoot);
		else
			m_MotionMgr.PauseAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.BowShoot);
	}

	public void SetTarget (Vector3 aimPos)
	{
		if(null != m_IKCmpt)
			m_IKCmpt.aimTargetPos = aimPos;
	}

	public void SetTarget (Transform trans)
	{
		if(null != m_IKCmpt)
			m_IKCmpt.aimTargetTrans = trans;
	}

    public bool Aimed
	{
		get
		{
			if(null != m_IKCmpt)
				return m_IKCmpt.aimed;
			return false;
		}
	}

    public string[] leisures
    {
        get
        {
            return m_Idles;
        }
    }
}
