using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using ItemAsset;

public enum ShootState
{
    Null,
    PutOn,
    Aim,
    Fire,
    Reload,
    PutOff
}

public enum ShootMode
{
    SingleShoot,
    MultiShoot,
    ChargeShoot
}

public enum AmmoType
{
    Bullet,
    Energy
}

[Serializable]
public class Magazine
{
	public float m_Size;
	public float m_Value;

	public Magazine(float max = 30, float current = 30)
	{
		m_Size = max;
		m_Value = Mathf.Clamp(current, 0, m_Size);
	}

	public Magazine(Magazine other)
	{
		if(null != other)
		{
			m_Size = other.m_Size;
			m_Value = other.m_Value;
		}
	}
}

public interface IPEgun
{
	bool NeedReload();
	bool CanShoot();
	void ApplyAmmoCost();
}

public interface IRechargeableEquipment
{
	float enMax{ get; }
	float enCurrent{ get; set; }
	float rechargeSpeed { get; }
	float lastUsedTime{ get; set; }
	float rechargeDelay { get; }
}

public class PEGun : PEAimAbleEquip, IWeapon, IAimWeapon
{
	public AttackMode[] 	m_AttackMode;
    public string[]         m_Idles;

	//Anim
	[Header("Reload")]
	public string 			m_ReloadAnim;
	public GameObject 		m_ChargeEffectGo;
	public int				m_ShellCaseEffectID;
	[HideInInspector]
	public Transform		m_ShellCaseTrans;
	public GameObject		m_MagazineObj;
	public Transform		m_MagazinePos;
	public int				m_MagazineEffectID;
	
	#region itemAttr
	//GunAttr
	public ShootMode 	m_ShootMode = ShootMode.SingleShoot;
	public AmmoType		m_AmmoType = AmmoType.Bullet;
	public Magazine 	m_Magazine;
	//For ammo gun
	[Header("Ammo gun")]
	public int[]		m_AmmoItemIDList = new int[]{11000001};
	int					m_CurAmmoItemIndex = 0;
	//For energy gun
	[Header("Energy gun")]
	public float		m_ChargeEnergySpeed = 0.5f;
	public float		m_RechargeEnergySpeed = 3f;
	public float		m_RechargeDelay = 1.5f;
	public float[]		m_ChargeTime = new float[]{0.8f, 1.5f};
	public float		m_EnergyPerShoot = 1f;

	//Common
	public int[]		m_SkillIDList = new int[]{20219924};
	public int			m_MeleeSkill;
	public float		m_FireRate = 0.3f;

	public virtual float 	magazineCost{ get { return 1f; } }
	public virtual float	magazineSize{ get{ return m_Magazine.m_Size; } }
	public virtual float	magazineValue
	{
		get
		{
			if(null != m_ItemAmmoAttr && Mathf.Abs(m_ItemAmmoAttr.count - m_Magazine.m_Value) > 0.8f)
				m_Magazine.m_Value = m_ItemAmmoAttr.count;
			return m_Magazine.m_Value; 
		}
		set{ m_Magazine.m_Value = value; if(null != m_ItemAmmoAttr) m_ItemAmmoAttr.count = Mathf.RoundToInt(value); } 
	}
	public int			curItemID{ get{ return (m_AmmoItemIDList.Length > 0) ? m_AmmoItemIDList[m_CurAmmoItemIndex] : 0; } }
	public int			curAmmoItemIndex 
	{
		get { return m_CurAmmoItemIndex; }
		set 
		{
			m_CurAmmoItemIndex = Mathf.Min(value, m_AmmoItemIDList.Length - 1);
			if(null != m_ItemAmmoAttr) 
				m_ItemAmmoAttr.index = m_CurAmmoItemIndex; 
		} 
	}
	#endregion

	#region SEID
	[Header("SEID")]
	public int m_ChargeSoundID;
	public int m_ChargeLevelUpSoundID;
	public int m_ChargeLevelUpEffectID;
	public int m_DryFireSoundID;
	public int m_ShootSoundID;
	public int m_ReloadSoundID;
	#endregion

	ItemAsset.GunAmmo 		m_ItemAmmoAttr;

    public override void InitEquipment(PeEntity entity, ItemAsset.ItemObject itemObj)
    {
        base.InitEquipment(entity, itemObj);

		m_ItemAmmoAttr = itemObj.GetCmpt<ItemAsset.GunAmmo>();
		if (null != m_ItemAmmoAttr)
        {
			if (null != m_AmmoItemIDList && m_AmmoItemIDList.Length > m_ItemAmmoAttr.index)
            {
				m_CurAmmoItemIndex = m_ItemAmmoAttr.index;
            }

            if (null != m_Magazine)
            {
                if (m_ItemAmmoAttr.count < 0)
                {
                    m_ItemAmmoAttr.count = (int)magazineSize;
                }

				m_Magazine.m_Value = m_ItemAmmoAttr.count;
            }
        }

		m_ShellCaseTrans = PETools.PEUtil.GetChild(transform, "ShellCase");

		m_View = entity.biologyViewCmpt;

		if(null != m_View && null != m_MagazineObj)
		{
			if(null != m_View)
			{
				m_View.AttachObject(m_MagazineObj, "mountOff");
				m_MagazineObj.transform.localPosition = Vector3.zero;
				m_MagazineObj.transform.localRotation = Quaternion.identity;
			}
		}

		if(null != m_ChargeEffectGo)
		{
			EffectLateupdateHelper effect = m_ChargeEffectGo.AddComponent<EffectLateupdateHelper>();
			effect.Init(m_ChargeEffectGo.transform.parent);
			m_ChargeEffectGo.transform.parent = Pathea.Effect.EffectBuilder.Instance.transform;
		}
    }

    public override void RemoveEquipment()
    {
        base.RemoveEquipment();
		
		if(null != m_View && null != m_MagazineObj)
		{
			if(null != m_View)
			{
				m_View.DetachObject(m_MagazineObj);
				GameObject.Destroy(m_MagazineObj);
			}
		}

		if(null != m_ChargeEffectGo)
			GameObject.Destroy(m_ChargeEffectGo);
    }

	public override void ResetView ()
	{
		base.ResetView ();
		if(null != m_View)
			GameObject.Destroy(m_MagazineObj);
	}

	public int GetSkillID(int chargeLevel = 0)
	{
		if(0 == m_SkillIDList.Length || chargeLevel >= m_SkillIDList.Length)
			return 0;
		return m_SkillIDList[chargeLevel];
	}

	#region IWeapon implementation

	public ItemAsset.ItemObject ItemObj{ get{ return m_ItemObj; } }
	
	public virtual void HoldWeapon(bool hold)
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
		return m_MotionMgr.CanDoAction(PEActionType.GunFire);
	}
	
	public void Attack(int index = 0, SkillSystem.SkEntity targetEntity = null)
	{
		if(null == m_MotionMgr)
			return;
		
		if(null != m_MotionEquip)
			m_MotionEquip.SetTarget(targetEntity);

		if(index > 0)
		{
			PEActionParamN param = PEActionParamN.param;
			param.n = index;
			m_MotionMgr.DoAction(PEActionType.GunMelee, param);
		}
		else
		{
			PEActionParamB param = PEActionParamB.param;
			param.b = true;
			m_MotionMgr.DoAction(PEActionType.GunFire, param);
		}
		if(null != m_AttackMode && m_AttackMode.Length > index)
			m_AttackMode[index].ResetCD();
	}

	public bool AttackEnd(int index = 0)
	{
		if(null == m_MotionMgr)
			return true;

		if(index > 0)
			return !m_MotionMgr.IsActionRunning(PEActionType.GunMelee);
		else
			return !m_MotionMgr.IsActionRunning(PEActionType.GunFire);
	}

	public virtual bool IsInCD(int index = 0)
	{
		if(null != m_AttackMode && m_AttackMode.Length > index)
			return m_AttackMode[index].IsInCD();
		return false;
	}

	#endregion

	public virtual void SetAimState(bool aimState)
	{
		if(aimState)
			m_MotionMgr.ContinueAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.GunFire);
		else
		{
			m_MotionMgr.PauseAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.GunFire);
		}
	}

	public void SetTarget (Vector3 aimPos)
	{
		if(null != m_IKCmpt)
			m_IKCmpt.aimTargetPos = aimPos;
	}

	public void SetTarget(Transform trans)
	{
		if(null != m_IKCmpt)
			m_IKCmpt.aimTargetTrans = trans;
	}

    public virtual bool Aimed
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
