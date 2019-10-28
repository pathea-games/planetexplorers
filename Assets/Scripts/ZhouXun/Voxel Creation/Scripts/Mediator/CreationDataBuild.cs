#define PLANET_EXPLORERS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;

#if PLANET_EXPLORERS
using ItemAsset;
using ItemAsset.PackageHelper;
#endif

// Build creaion GameObject
// 
public partial class CreationData
{
	/// <summary>
	/// This prefab is a disabled real-time GameObject under the creation group, 
	/// we will use it to instantiate real creation.
	/// It's different from 'Unity Engine Prefabs'.
	/// </summary>
	public GameObject m_Prefab = null;
	private GameObject m_Root = null;
	private GameObject m_PartGroup = null;
	private GameObject m_MeshGroup = null;
	private GameObject m_DecalGroup = null;
	private GameObject m_EffectGroup = null;

    private GameObject m_RootL = null;
    private GameObject m_PartGroupL = null;
    private GameObject m_MeshGroupL = null;
    private GameObject m_DecalGroupL = null;
    private GameObject m_EffectGroupL = null;

	public ulong m_MatGUID = 0;
	
	public CreationController creationController;
	
	/// <summary>
	/// The Mesh manager.
	/// </summary>
    public VCMeshMgr m_MeshMgr = null;
    public VCMeshMgr m_MeshMgrL = null;
	
	/// <summary>
	/// Builds the prefab under the creation group at real-time.
	/// </summary>
	public void BuildPrefab ()
	{
		BuildStructure();
		BuildMaterial();
		BuildCreation();
	}


	static void AlignPivotInChild(Transform root, Transform pivot)
	{
		var parent = root.parent;
		root.SetParent(null, false);

		parent.position = pivot.position;
		parent.rotation = pivot.rotation;

		root.SetParent(parent, true);

		parent.localPosition = Vector3.zero;
		parent.localRotation = Quaternion.identity;
    }

	
	private void BuildStructure ()
	{
		m_Prefab = new GameObject ("Creation_" + m_ObjectID.ToString() + " (" + m_IsoData.m_HeadInfo.Name + ")");
		m_Prefab.transform.parent = VCEditor.Instance.m_CreationGroup.transform;
		m_Prefab.transform.localPosition = Vector3.zero;
		m_Prefab.layer = VCConfig.s_ProductLayer;
		m_Prefab.SetActive(false);

		m_Root = new GameObject ("Root");
		m_Root.transform.parent = m_Prefab.transform;
		m_Root.transform.localPosition = Vector3.zero;
		m_Root.layer = VCConfig.s_ProductLayer;
		m_PartGroup = new GameObject ("Parts");
		m_PartGroup.transform.parent = m_Root.transform;
		m_PartGroup.transform.localPosition = Vector3.zero;
		m_PartGroup.layer = VCConfig.s_ProductLayer;
		m_MeshGroup = new GameObject ("Meshes");
		m_MeshGroup.transform.parent = m_Root.transform;
		m_MeshGroup.transform.localPosition = Vector3.zero;
		m_MeshGroup.layer = VCConfig.s_ProductLayer;
		m_DecalGroup = new GameObject ("Decals");
		m_DecalGroup.transform.parent = m_Root.transform;
		m_DecalGroup.transform.localPosition = Vector3.zero;
		m_DecalGroup.layer = VCConfig.s_ProductLayer;
		m_EffectGroup = new GameObject ("Effects");
		m_EffectGroup.transform.parent = m_Root.transform;
		m_EffectGroup.transform.localPosition = Vector3.zero;
		m_EffectGroup.layer = VCConfig.s_ProductLayer;

        m_RootL = new GameObject("Root_L");
        m_RootL.transform.parent = m_Prefab.transform;
        m_RootL.transform.localPosition = Vector3.zero;
        m_RootL.layer = VCConfig.s_ProductLayer;
        m_PartGroupL = new GameObject("Parts_L");
        m_PartGroupL.transform.parent = m_RootL.transform;
        m_PartGroupL.transform.localPosition = Vector3.zero;
        m_PartGroupL.layer = VCConfig.s_ProductLayer;
        m_MeshGroupL = new GameObject("Meshes_L");
        m_MeshGroupL.transform.parent = m_RootL.transform;
        m_MeshGroupL.transform.localPosition = Vector3.zero;
        m_MeshGroupL.layer = VCConfig.s_ProductLayer;
        m_DecalGroupL = new GameObject("Decals_L");
        m_DecalGroupL.transform.parent = m_RootL.transform;
        m_DecalGroupL.transform.localPosition = Vector3.zero;
        m_DecalGroupL.layer = VCConfig.s_ProductLayer;
        m_EffectGroupL = new GameObject("Effects_L");
        m_EffectGroupL.transform.parent = m_RootL.transform;
        m_EffectGroupL.transform.localPosition = Vector3.zero;
        m_EffectGroupL.layer = VCConfig.s_ProductLayer;

        bool dbRoot = m_IsoData.m_HeadInfo.Category == EVCCategory.cgDbSword;
        VCESceneSetting sceneSetting = m_IsoData.m_HeadInfo.FindSceneSetting();
        foreach ( VCComponentData cdata in m_IsoData.m_Components )
		{
			if ( cdata.m_Type == EVCComponent.cpDecal )
			{
               if(dbRoot)
               {
                   bool isleft = cdata.m_Position.x < 0.5f * m_IsoData.m_HeadInfo.xSize * sceneSetting.m_VoxelSize;
                   if (isleft) cdata.CreateEntity(false, m_DecalGroupL.transform);
                   else cdata.CreateEntity(false, m_DecalGroup.transform);
               }
                else
               {
                   cdata.CreateEntity(false, m_DecalGroup.transform);
               }
               
			}
			else if ( cdata.m_Type == EVCComponent.cpEffect )
			{
                if(dbRoot)
                {
                    bool isleft = cdata.m_Position.x < 0.5f * m_IsoData.m_HeadInfo.xSize * sceneSetting.m_VoxelSize;
                    if (isleft) cdata.CreateEntity(false, m_EffectGroupL.transform);
                    else cdata.CreateEntity(false, m_EffectGroup.transform);
                }
                else
                {
                    cdata.CreateEntity(false, m_EffectGroup.transform);
                }
				
			}
			else if(cdata.m_Type == EVCComponent.cpDbSwordHilt)
            {
                if((cdata as VCFixedHandPartData).m_LeftHand)
                {
                    cdata.CreateEntity(false, m_PartGroupL.transform);
                    VCParticlePlayer pp = cdata.m_Entity.AddComponent<VCParticlePlayer>();
                    pp.FunctionTag = VCParticlePlayer.ftDamaged;
                    pp.LocalPosition = cdata.m_Entity.GetComponent<VCEComponentTool>().m_SelBound.transform.localPosition;

                    m_MeshMgrL = m_MeshGroupL.AddComponent<VCMeshMgr>();
                    m_MeshMgrL.m_VoxelSize = sceneSetting.m_VoxelSize;
                    m_MeshMgrL.m_ColorMap = m_IsoData.m_Colors;
                    m_MeshMgrL.m_ColliderDirty = false;
                    m_MeshMgrL.m_MeshMat = null;
                    m_MeshMgrL.m_DaggerMesh = true;
                    m_MeshMgrL.m_LeftSidePos = cdata.m_Position.x < 0.5f * m_IsoData.m_HeadInfo.xSize * sceneSetting.m_VoxelSize;
                    m_MeshMgrL.Init();
                }
                else
                {
                    cdata.CreateEntity(false, m_PartGroup.transform);
                    VCParticlePlayer pp = cdata.m_Entity.AddComponent<VCParticlePlayer>();
                    pp.FunctionTag = VCParticlePlayer.ftDamaged;
                    pp.LocalPosition = cdata.m_Entity.GetComponent<VCEComponentTool>().m_SelBound.transform.localPosition;

                    m_MeshMgr = m_MeshGroup.AddComponent<VCMeshMgr>();
                    m_MeshMgr.m_VoxelSize = sceneSetting.m_VoxelSize;
                    m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
                    m_MeshMgr.m_ColliderDirty = false;
                    m_MeshMgr.m_MeshMat = null;
                    m_MeshMgr.m_DaggerMesh = true;
                    m_MeshMgr.m_LeftSidePos = cdata.m_Position.x < 0.5f * m_IsoData.m_HeadInfo.xSize * sceneSetting.m_VoxelSize;
                    m_MeshMgr.Init();

                }
            }
            else
			{
				cdata.CreateEntity(false, m_PartGroup.transform);
				VCParticlePlayer pp = cdata.m_Entity.AddComponent<VCParticlePlayer>();
				pp.FunctionTag = VCParticlePlayer.ftDamaged;
				pp.LocalPosition = cdata.m_Entity.GetComponent<VCEComponentTool>().m_SelBound.transform.localPosition;

                m_MeshMgr = m_MeshGroup.AddComponent<VCMeshMgr>();
                m_MeshMgr.m_VoxelSize = sceneSetting.m_VoxelSize;
                m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
                m_MeshMgr.m_ColliderDirty = false;
                m_MeshMgr.m_MeshMat = null;
                m_MeshMgr.m_DaggerMesh = false;
                m_MeshMgr.m_LeftSidePos = false;
                m_MeshMgr.Init();
			}
           
		}

        if(m_MeshMgr == null)
        {
            m_MeshMgr = m_MeshGroup.AddComponent<VCMeshMgr>();
            m_MeshMgr.m_VoxelSize = sceneSetting.m_VoxelSize;
            m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
            m_MeshMgr.m_ColliderDirty = false;
            m_MeshMgr.m_MeshMat = null;
            m_MeshMgr.m_DaggerMesh = false;
            m_MeshMgr.m_LeftSidePos = false;
            m_MeshMgr.Init();
        }
	}


	private void BuildMaterial ()
	{
		if ( VCMatGenerator.Instance != null && VCMatManager.Instance != null )
		{
			m_MatGUID = VCMatGenerator.Instance.GenMeshMaterial(m_IsoData.m_Materials, false);
			if ( VCMatManager.Instance.m_mapMaterials.ContainsKey(m_MatGUID) )
				m_MeshMgr.m_MeshMat = VCMatManager.Instance.m_mapMaterials[m_MatGUID];
		}
	}


	T FindComponent<T>(EVCComponent evcComponent) where T : MonoBehaviour
	{
		foreach (VCComponentData cdata in m_IsoData.m_Components)
		{
			if (cdata.m_Type == evcComponent)
			{
				return cdata.m_Entity.GetComponent<T>();
			}
		}
		return null;
	}


	private void BuildCreation ()
	{
		#region Root Transform
		// [VCCase] - Set creation root Transform

		if ( m_Attribute.m_Type == ECreation.Sword)
		{
			Transform pivot = null;
			foreach ( VCComponentData cdata in m_IsoData.m_Components )
			{
                if (cdata.m_Type == EVCComponent.cpSwordHilt || cdata.m_Type == EVCComponent.cpLgSwordHilt)
				{
					pivot = cdata.m_Entity.transform;
					break;
				}
			}

			m_Root.transform.localRotation = Quaternion.Inverse(pivot.rotation);
			m_Root.transform.Rotate(Vector3.up, -90, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90, Space.World);
			m_Root.transform.localPosition = -pivot.position;
		}
        else if(m_Attribute.m_Type == ECreation.SwordLarge)
        {
            Transform pivot = null;
            foreach (VCComponentData cdata in m_IsoData.m_Components)
            {
                if (cdata.m_Type == EVCComponent.cpLgSwordHilt)
                {
                    pivot = cdata.m_Entity.transform;
                    break;
                }
            }

            m_Root.transform.localRotation = Quaternion.Inverse(pivot.rotation);
            m_Root.transform.Rotate(Vector3.up, -90, Space.World);
            m_Root.transform.Rotate(Vector3.right, 90, Space.World);
            m_Root.transform.localPosition = -pivot.position;


        }
        else if (m_Attribute.m_Type == ECreation.SwordDouble)
        {
            Transform pivot = null;
            int cdcnt = 0;
            foreach (VCComponentData cdata in m_IsoData.m_Components)
            {
                if (cdata.m_Type == EVCComponent.cpDbSwordHilt && (cdata as VCFixedHandPartData).m_LeftHand)
                {
                    pivot = cdata.m_Entity.transform;
                    //SingleHandSword
                    Transform go = cdata.m_Entity.transform.FindChild("SingleHandSword");//VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj  
                    go.gameObject.name = "SingleHandSwordL";
                    m_RootL.transform.localRotation = Quaternion.Inverse(pivot.rotation);
                    m_RootL.transform.Rotate(Vector3.up, -90, Space.World);
                    m_RootL.transform.Rotate(Vector3.right, 90, Space.World);
                    m_RootL.transform.Rotate(Vector3.up, 180, Space.World);
                    m_RootL.transform.Rotate(Vector3.forward, 180, Space.World);
                    m_RootL.transform.localPosition = -pivot.position;
                    cdcnt ++;
                    
                }

                if(cdata.m_Type == EVCComponent.cpDbSwordHilt && !(cdata as VCFixedHandPartData).m_LeftHand)
                {
                    pivot = cdata.m_Entity.transform;

                    Transform go = cdata.m_Entity.transform.FindChild("SingleHandSword");
                    go.gameObject.name = "SingleHandSwordR";


                    //m_Root.transform.localRotation = Quaternion.Inverse(pivot.rotation);
                    //m_Root.transform.Rotate(Vector3.up, -90, Space.World);
                    //m_Root.transform.Rotate(Vector3.right, 90, Space.World);
                    //m_Root.transform.localPosition = pivot.position;
                    m_Root.transform.localRotation = Quaternion.Inverse(pivot.rotation);
                    m_Root.transform.Rotate(Vector3.up, -90, Space.World);
                    m_Root.transform.Rotate(Vector3.right, 90, Space.World);
                    m_Root.transform.Rotate(Vector3.up, 180, Space.World);
                    m_Root.transform.Rotate(Vector3.forward, 180, Space.World);
                    m_Root.transform.localPosition = -pivot.position;
                    cdcnt++;
                }

                if (cdcnt == 2) break;

            }

           

           
        }

		else if (m_Attribute.m_Type == ECreation.Bow)
		{
			Transform pivot = null;
			foreach (VCComponentData cdata in m_IsoData.m_Components)
			{
				if (cdata.m_Type == EVCComponent.cpBowGrip)
				{
					pivot = cdata.m_Entity.transform;
					break;
				}
			}

			AlignPivotInChild(m_Root.transform, pivot);
		}
		else if (m_Attribute.m_Type == ECreation.Axe)
		{
			Transform pivot = null;
			foreach (VCComponentData cdata in m_IsoData.m_Components)
			{
				if (cdata.m_Type == EVCComponent.cpAxeHilt)
				{
					pivot = cdata.m_Entity.transform;
					break;
				}
			}

			m_Root.transform.localRotation = Quaternion.Inverse(pivot.rotation);
			m_Root.transform.Rotate(Vector3.up, -90, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90, Space.World);
			m_Root.transform.localPosition = -pivot.position;
		}
		else if ( m_Attribute.m_Type == ECreation.Shield )
		{
			Transform pivot = null;
			foreach ( VCComponentData cdata in m_IsoData.m_Components )
			{
				if ( cdata.m_Type == EVCComponent.cpShieldHandle )
				{
					pivot = cdata.m_Entity.GetComponent<VCPShieldHandle>().m_PivotPoint;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse( pivot.rotation );
			m_Root.transform.Rotate(Vector3.right, 90, Space.World);
			m_Root.transform.localPosition = -pivot.position;
		}
		else if ( m_Attribute.m_Type == ECreation.HandGun || m_Attribute.m_Type == ECreation.Rifle )
		{
			Transform pivot = null;
			foreach ( VCComponentData cdata in m_IsoData.m_Components )
			{
				if ( cdata.m_Type == EVCComponent.cpGunHandle )
				{
					pivot = cdata.m_Entity.GetComponent<VCPGunHandle>().m_FirstHandPoint;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse( pivot.rotation );
			m_Root.transform.Rotate(Vector3.up, -90, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90, Space.World);
			m_Root.transform.localPosition = -pivot.position;
		}
		else if ( m_Attribute.m_Type == ECreation.Vehicle )
		{
			Vector3 pivot = m_Attribute.m_CenterOfMass;
			float sum_y = 0;
			int wheel_cnt = 0;
			foreach ( VCComponentData cdata in m_IsoData.m_Components )
			{
				if ( cdata.m_Type == EVCComponent.cpVehicleWheel )
				{
					sum_y += cdata.m_Entity.GetComponent<VCEComponentTool>().m_DrawPivot.position.y;
					wheel_cnt++;

				}
			}
			pivot.y = sum_y / (float)wheel_cnt - 0.1f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}
		else if ( m_Attribute.m_Type == ECreation.Aircraft )
		{
			Vector3 pivot = m_Attribute.m_CenterOfMass;
			pivot.y = 0;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}
		else if ( m_Attribute.m_Type == ECreation.Boat )
		{
			Vector3 pivot = m_Attribute.m_CenterOfMass;
			pivot.y = 0;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}
		else if ( m_Attribute.m_Type == ECreation.SimpleObject )
		{
			VCComponentData pivotCmpt = null;
			foreach (VCComponentData cdata in m_IsoData.m_Components)
			{
				if (cdata.m_Type == EVCComponent.cpPivot)
				{
					pivotCmpt = cdata;
					break;
				}
			}

			Vector3 pivot;
			if (pivotCmpt != null)
			{
				pivot = pivotCmpt.m_Position;
			}
			else
			{
				pivot = m_Attribute.m_CenterOfMass;
			}
			pivot.y = 0;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}
		else if (m_Attribute.m_Type == ECreation.Robot)
		{
			Vector3 pivot = m_Attribute.m_CenterOfMass;
			pivot.y = 0;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}
		else if (m_Attribute.m_Type == ECreation.AITurret)
		{
			Vector3 pivot = m_Attribute.m_CenterOfMass;
			pivot.y = 0;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}
		else if (m_Attribute.m_Type == ECreation.ArmorHead
			|| m_Attribute.m_Type == ECreation.ArmorBody
			|| m_Attribute.m_Type == ECreation.ArmorArmAndLeg
			|| m_Attribute.m_Type == ECreation.ArmorHandAndFoot
			|| m_Attribute.m_Type == ECreation.ArmorDecoration
			)
		{
			Vector3 pivot = m_Attribute.m_CenterOfMass;
			pivot.y = 0;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -pivot;
		}

		#endregion

		#region Top Script

		// [VCCase] - Create top scripts
		creationController = m_Prefab.AddComponent<CreationController>();
		creationController.enabled = false;
		creationController.Init(
			m_PartGroup.transform,
			m_MeshGroup.transform,
			m_DecalGroup.transform,
			m_EffectGroup.transform,
			this);

		switch (m_Attribute.m_Type)
		{
			case ECreation.Sword:
				{
					PeSword sword = m_Prefab.AddComponent<PeSword>();
					VCPSwordHilt properties = FindComponent<VCPSwordHilt>(EVCComponent.cpSwordHilt);
					properties.CopyTo(sword, this);
					float weight = creationController.creationData.m_Attribute.m_Weight;
					sword.m_AnimSpeed = VCUtility.GetSwordAnimSpeed(weight);

                    //set attacktrigger height
                    if (properties.Attacktrigger != null)
                    {
                        for (int i = 0; i < properties.Attacktrigger.attackParts.Length; i++)
                        {
                            properties.Attacktrigger.attackParts[i].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.x;
                            properties.Attacktrigger.attackParts[i].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.z;
                        }
                           

                    }
				}
				break;

            case ECreation.SwordLarge:
                {
                    PeSword sword = m_Prefab.AddComponent<PeSword>();
                    VCPSwordHilt properties = FindComponent<VCPSwordHilt>(EVCComponent.cpLgSwordHilt);
                    properties.CopyTo(sword, this);
                    float weight = creationController.creationData.m_Attribute.m_Weight;
                    sword.m_AnimSpeed = VCUtility.GetSwordAnimSpeed(weight);

                    //set attacktrigger height
                    if (properties.Attacktrigger != null)
                    {
                        for (int i = 0; i < properties.Attacktrigger.attackParts.Length; i++)
                        {
                            properties.Attacktrigger.attackParts[i].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.x;
                            properties.Attacktrigger.attackParts[i].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.z;
                        }
                           
                    }
                   
                }
                break;

            case ECreation.SwordDouble:
                {
                    PETwoHandWeapon sword = m_Prefab.AddComponent<PETwoHandWeapon>();
                    VCPSwordHilt properties = FindComponent<VCPSwordHilt>(EVCComponent.cpDbSwordHilt);
                    properties.CopyTo(sword, this);
                    sword.m_LHandWeapon = m_RootL;
                    float weight = creationController.creationData.m_Attribute.m_Weight;
                    sword.m_AnimSpeed = VCUtility.GetSwordAnimSpeed(weight);

                    //set attacktrigger height
                    int cnt = 0;
                    foreach (VCComponentData cdata in m_IsoData.m_Components)
                    {
                        if (cdata.m_Type == EVCComponent.cpDbSwordHilt && (cdata as VCFixedHandPartData).m_LeftHand && properties.Attacktrigger != null)
                        {
                            cnt++;
                            VCPSwordHilt hiltL = cdata.m_Entity.GetComponent<VCPSwordHilt>();
                            if (hiltL == null)
                                continue;

                            for (int i = 0; i < properties.Attacktrigger.attackParts.Length; i++)
                            {
                                hiltL.Attacktrigger.attackParts[i].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.x;
                                hiltL.Attacktrigger.attackParts[i].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.z;
                            }
                               
                           
                        }

                        if (cdata.m_Type == EVCComponent.cpDbSwordHilt && !(cdata as VCFixedHandPartData).m_LeftHand && properties.Attacktrigger != null)
                        {
                            cnt++;
                            VCPSwordHilt hiltR = cdata.m_Entity.GetComponent<VCPSwordHilt>();
                            if (hiltR == null)
                                continue;

                            for (int i = 0; i < properties.Attacktrigger.attackParts.Length; i++)
                            {
                                hiltR.Attacktrigger.attackParts[i].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.y;
                                hiltR.Attacktrigger.attackParts[i].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.w;

                            }
                               
                           
                        }
                        if (cnt == 2) break;
                    }

                }
                break;


			case ECreation.Bow:
				{
					PEBow bow = m_Prefab.AddComponent<PEBow>();
					VCPBowGrip bowProperties = FindComponent<VCPBowGrip>(EVCComponent.cpBowGrip);
					bowProperties.CopyTo(bow, this);
				}
				break;

			case ECreation.Axe:
				{
					PEAxe axe = m_Prefab.AddComponent<PEAxe>();
					VCPAxeHilt axeProperties = FindComponent<VCPAxeHilt>(EVCComponent.cpAxeHilt);
					axeProperties.CopyTo(axe, this);
					float weight = creationController.creationData.m_Attribute.m_Weight;
					axe.m_AnimSpeed = VCUtility.GetAxeAnimSpeed(weight);
				}
				break;

			case ECreation.Shield:
				{
					PESheild sheild = m_Prefab.AddComponent<PESheild>();
					sheild.showOnVehicle = false;
				}
				break;

			case ECreation.Rifle:
			case ECreation.HandGun:
				{
					PEGun gun = m_Prefab.AddComponent<PEGun>();
					VCPGunHandle handleProperties = FindComponent<VCPGunHandle>(EVCComponent.cpGunHandle);
					VCPGunMuzzle muzzleProperties = FindComponent<VCPGunMuzzle>(EVCComponent.cpGunMuzzle);
					handleProperties.CopyTo(gun);
					muzzleProperties.CopyTo(gun);
				}
				break;

			case ECreation.Vehicle:
				{
					VCParticlePlayer pp = m_Prefab.AddComponent<VCParticlePlayer>();
					pp.FunctionTag = VCParticlePlayer.ftExplode;
					pp.LocalPosition = creationController.bounds.center;
					break;
				}
				
			case ECreation.Aircraft:
				{
					VCParticlePlayer pp = m_Prefab.AddComponent<VCParticlePlayer>();
					pp.FunctionTag = VCParticlePlayer.ftExplode;
					pp.LocalPosition = creationController.bounds.center;
					break;
				}

			case ECreation.Boat:
				{
					VCParticlePlayer pp = m_Prefab.AddComponent<VCParticlePlayer>();
					pp.FunctionTag = VCParticlePlayer.ftExplode;
					pp.LocalPosition = creationController.bounds.center;
					break;
				}

			case ECreation.Robot:
				{
					VCParticlePlayer pp = m_Prefab.AddComponent<VCParticlePlayer>();
					pp.FunctionTag = VCParticlePlayer.ftExplode;
					pp.LocalPosition = creationController.bounds.center;
					break;
				}

			case ECreation.AITurret:
				{
					VCParticlePlayer pp = m_Prefab.AddComponent<VCParticlePlayer>();
					pp.FunctionTag = VCParticlePlayer.ftExplode;
					pp.LocalPosition = creationController.bounds.center;
					break;
				}

			case ECreation.SimpleObject:
				break;

			case ECreation.ArmorHead:
			case ECreation.ArmorBody:
			case ECreation.ArmorArmAndLeg:
			case ECreation.ArmorHandAndFoot:
			case ECreation.ArmorDecoration:
				break;

			default:
				break;
		}

		#endregion
	}
	

	/// <summary>
	/// Sends this creation to player (Create ItemObject).
	/// A creation can be send only once!
	/// </summary>
	public int SendToPlayer (out ItemObject item,bool pay = true)
	{
        // Check
        item = null;

        if ( m_Attribute.m_Type == ECreation.Null )
			return 0;
		if ( m_Prefab == null )
			return 0;
		
#if PLANET_EXPLORERS
        if (Pathea.PeCreature.Instance.mainPlayer == null)
			return 0;

        ItemProto itemProto = ItemProto.Mgr.Instance.Get(m_ObjectID);

		// Sending item to player.
		int retval = 1;
        if(!pay)
        {
            item = ItemMgr.Instance.CreateItem(m_ObjectID);
            return retval;
        }
        Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
        
		SlotList slotList = pkg.package.GetSlotList((ItemAsset.ItemPackage.ESlotType)itemProto.tabIndex);

		if (slotList.GetVacancyCount() > 0)
		{
            item = ItemMgr.Instance.CreateItem(m_ObjectID); // single
			if (slotList.Add(item, true) )
			{
				if ( !VCEditor.Instance.m_CheatWhenMakeCreation && !Pathea.PeGameMgr.IsSingleBuild)
				{
					// Player must pay for it !
					foreach ( KeyValuePair<int, int> kvp in m_Attribute.m_Cost )
					{
                        if (kvp.Value > 0)
                        {
                            pkg.package.Destroy(kvp.Key, kvp.Value);
                        }
					}
				}
			}
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_FULL);
			retval = -1;
		}
		// GUI Synchronization
		GameUI.Instance.mItemPackageCtrl.ResetItem();
#endif
		
		// Instance Data
		//m_Hp = m_Attribute.m_Durability;
		//m_Fuel = m_Attribute.m_MaxFuel;
		return retval;
	}

	public void DestroyPrefab ()
	{
		if ( m_Prefab != null )
			GameObject.Destroy(m_Prefab);
		m_Prefab = null;
		m_Root = null;
		m_PartGroup = null;
		m_MeshGroup = null;
		m_DecalGroup = null;
		m_EffectGroup = null;
	}
}
