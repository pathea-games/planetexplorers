using UnityEngine;
using System;
using WhiteCat;

[Serializable]
public abstract class VCComponentData
{
	#region DATA_BLOCK

	public int m_ComponentId;	// 有效位数为 16 位
	public int m_ExtendData;    // 有效位数为 16 位

	public EVCComponent m_Type;
	
	public Vector3 m_Position = Vector3.zero;
	public Vector3 m_Rotation = Vector3.zero;
	public Vector3 m_Scale = Vector3.one;
	public bool m_Visible = true;
	#endregion
	
	public abstract void Validate();
	public abstract void Import(byte[] buffer);
	public abstract byte[] Export();
	protected void PositionValidate()
	{
		if ( m_CurrIso != null )
		{
			m_Position = m_CurrIso.ClampPointWorldCoord(m_Position);
		}
	}
	public virtual VCComponentData Copy()
	{
		VCComponentData copydata = Create(m_Type, Export());
		copydata.m_Entity = m_Entity;
		copydata.m_CurrIso = m_CurrIso;
		return copydata;
	}
	
	
	
	#region LINKS

	// The entity of this data block
	public GameObject m_Entity;
	public abstract void UpdateEntity(bool for_editor);
	public abstract GameObject CreateEntity(bool for_editor, Transform parent);
	public void DestroyEntity()
	{
		if ( m_Entity != null )
		{
			GameObject.Destroy(m_Entity);
			m_Entity = null;
		}
	}
	
	// Pointer to current iso
	public VCIsoData m_CurrIso;

	// 在 UpdateEntity 后刷新组件的参数
	public void UpdateComponent()
	{
		var part = m_Entity.GetComponent<VCPart>();
		if (part) UpdateComponent(part);
	}

	protected virtual void UpdateComponent(VCPart part)
	{
		part.hiddenModel = !m_Visible;

		if (part is VCPWeapon)
		{
			(part as VCPWeapon).groupIndex = m_ExtendData;
		}
	}

	#endregion



	#region STATIC FUNCTIONS
	public static VCComponentData Create(VCPartInfo part)
	{
		if ( part == null )
			return null;
		VCComponentData data = Create(part.m_Type);
		data.m_ComponentId = part.m_ID;
		data.m_Type = part.m_Type;
		data.m_Position = Vector3.zero;
		data.m_Rotation = Vector3.zero;
		data.m_Scale = Vector3.one;
		data.m_Visible = true;
		return data;
	}
	public static VCComponentData CreateDecal ()
	{
		return Create(EVCComponent.cpDecal);
	}
	public static VCComponentData CreateEffect ()
	{
		return Create(EVCComponent.cpEffect);
	}
	public static VCComponentData Create(EVCComponent type)
	{
		// [VCCase] - How to create ComponentData with component type ?
		switch ( type )
		{
		default:
		case EVCComponent.cpAbstract:
			return null;
			
		case EVCComponent.cpVehicleFuelCell:
		case EVCComponent.cpVtolFuelCell:
		case EVCComponent.cpVtolRotor:
		case EVCComponent.cpHeadLight:
		case EVCComponent.cpCtrlTurret:
		case EVCComponent.cpMissileLauncher:
		case EVCComponent.cpShipPropeller:
		case EVCComponent.cpShipRudder:
		case EVCComponent.cpAirshipThruster:
		case EVCComponent.cpSubmarineBallastTank:
		case EVCComponent.cpRobotBattery:
		case EVCComponent.cpRobotController:
		case EVCComponent.cpRobotWeapon:
		case EVCComponent.cpAITurretWeapon:
				return new VCFreePartData ();
			
		case EVCComponent.cpSwordHilt:
        case EVCComponent.cpLgSwordHilt:
		case EVCComponent.cpBowGrip:
		case EVCComponent.cpAxeHilt:
		case EVCComponent.cpShieldHandle:
		case EVCComponent.cpGunMuzzle:
		case EVCComponent.cpVehicleCockpit:
		case EVCComponent.cpVehicleEngine:
		case EVCComponent.cpVtolCockpit:
		case EVCComponent.cpSideSeat:
		case EVCComponent.cpJetExhaust:
		case EVCComponent.cpShipCockpit:
		case EVCComponent.cpBed:
		case EVCComponent.cpHeadPivot:
		case EVCComponent.cpBodyPivot:
		case EVCComponent.cpArmAndLegPivot:
		case EVCComponent.cpHandAndFootPivot:
		case EVCComponent.cpDecorationPivot:
			return new VCGeneralPartData ();

        case EVCComponent.cpDbSwordHilt:
            return new VCFixedHandPartData();
			
		case EVCComponent.cpGunHandle:
			return new VCFixedPartData ();
			
		case EVCComponent.cpFrontCannon:
			return new VCAsymmetricGeneralPartData ();
						
		case EVCComponent.cpLandingGear:
			return new VCTriphaseFixedPartData ();
			
		case EVCComponent.cpVehicleWheel:
			return new VCQuadphaseFixedPartData ();

		case EVCComponent.cpDecal:
			return new VCDecalData ();

		case EVCComponent.cpLight:
			return new VCObjectLightData();

		case EVCComponent.cpPivot:
			return new VCObjectPivotData();
		
		}
	}
	public static VCComponentData Create(EVCComponent type, byte[] buffer)
	{
		VCComponentData cdata = Create(type);
		if ( cdata != null )
			cdata.Import(buffer);
		return cdata;
	}
	#endregion
}
