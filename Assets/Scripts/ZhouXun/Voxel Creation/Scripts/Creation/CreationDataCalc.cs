#define PLANET_EXPLORERS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;

// Calculate attribute for CreationData
// 
public partial class CreationData
{
	public void GenCreationAttr()
	{
		CreationData.CalcCreationAttr(m_IsoData, m_RandomSeed, ref m_Attribute);
	}

	//
	// Main Calculator
	//
	public static void CalcCreationAttr(VCIsoData iso, float random_seed, ref CreationAttr attr)
	{
		attr = new CreationAttr();
		CalcCreationType(iso, attr);
		CalcCommonAttr(iso, attr);

		// [VCCase] - Calculate attributes
		switch (attr.m_Type)
		{
			case ECreation.Sword:
            case ECreation.SwordLarge:
            case ECreation.SwordDouble:
				CalcSwordAttr(iso, random_seed, attr);
				break;

			case ECreation.Bow:
				CalcBowAttr(iso, random_seed, attr);
				break;

			case ECreation.Axe:
				CalcSwordAttr(iso, random_seed, attr);
				attr.m_Durability *= PEVCConfig.instance.axeDurabilityScale;
				break;

			case ECreation.Shield:
				CalcShieldAttr(iso, random_seed, attr);
				break;

			case ECreation.HandGun:
			case ECreation.Rifle:
				CalcGunAttr(iso, random_seed, attr);
				break;

			case ECreation.Vehicle:
				CalcVehicleAttr(iso, random_seed, attr);
				break;

			case ECreation.Aircraft:
				CalcAircraftAttr(iso, random_seed, attr);
				break;

			case ECreation.Boat:
				CalcBoatAttr(iso, random_seed, attr);
				break;

			case ECreation.AITurret:
				CalcAITurretAttr(iso, random_seed, attr);
				break;

			case ECreation.SimpleObject:
				CalcSimpleObjectAttr(iso, random_seed, attr);
				break;

			case ECreation.Robot:
				CalcRobotAttr(iso, random_seed, attr);
				break;

			case ECreation.ArmorHead:
			case ECreation.ArmorBody:
			case ECreation.ArmorArmAndLeg:
			case ECreation.ArmorHandAndFoot:
			case ECreation.ArmorDecoration:
				CalcArmorAttr(iso, random_seed, attr);
				break;

			default:
				break;
		}
	}

	public static void CalcCreationType(VCIsoData iso, CreationAttr attr)
	{
		attr.m_Type = ECreation.Null;

		bool has_visible_voxel = false;
		foreach (KeyValuePair<int, VCVoxel> kvp in iso.m_Voxels)
		{
			if (kvp.Value.Volume >= VCEMath.MC_ISO_VALUE)
			{
				has_visible_voxel = true;
				break;
			}
		}
		if (!has_visible_voxel)
		{
			attr.m_Errors.Add("No visible material voxels".ToLocalizationString());
		}

		// [VCCase] - Determine the creation type
		switch (iso.m_HeadInfo.Category)
		{
			case EVCCategory.cgSword:
				{
					int hilt_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpSwordHilt)
							hilt_cnt++;
					}

					if (hilt_cnt == 0)
					{
						attr.m_Errors.Add("No hilt".ToLocalizationString());
					}
					if (hilt_cnt > 1)
					{
						attr.m_Errors.Add("Too many hilts, max is 1".ToLocalizationString());
					}

					if (attr.m_Errors.Count == 0)
						attr.m_Type = ECreation.Sword;
				}
				break;
            case EVCCategory.cgLgSword:
                {
                    int hilt_cnt = 0;
                    foreach (VCComponentData cdata in iso.m_Components)
                    {
                        if (cdata.m_Type == EVCComponent.cpLgSwordHilt)
                            hilt_cnt++;
                    }

                    if (hilt_cnt == 0)
                    {
                        attr.m_Errors.Add("No hilt".ToLocalizationString());
                    }
                    if (hilt_cnt > 1)
                    {
                        attr.m_Errors.Add("Too many hilts, max is 1".ToLocalizationString());
                    }

                    if (attr.m_Errors.Count == 0)
                        attr.m_Type = ECreation.SwordLarge;
                }
                break;

            case EVCCategory.cgDbSword:
                {
                    int hilt_cnt = 0;
                    int l_hilt_cnt =0;
                    int r_hilt_cnt =0;
                    int lscnt = 0;
                    int rscnt = 0;
                    foreach (VCComponentData cdata in iso.m_Components)
                    {
                        if (cdata.m_Type == EVCComponent.cpDbSwordHilt)
                            hilt_cnt++;

                        if(cdata is VCFixedHandPartData)
                        {
                            if ((cdata as VCFixedHandPartData).m_LeftHand) l_hilt_cnt++;
                            else r_hilt_cnt++;

                            if (cdata.m_Position.x < (0.5f * iso.m_HeadInfo.xSize + 5) * iso.m_HeadInfo.FindSceneSetting().m_VoxelSize) lscnt++;
                            if (cdata.m_Position.x > (0.5f * iso.m_HeadInfo.xSize - 5) * iso.m_HeadInfo.FindSceneSetting().m_VoxelSize) rscnt++;
                        }
                       
                    }


                    switch (hilt_cnt)
                    {
                        case 0: attr.m_Errors.Add(PELocalization.GetString(9500449)); break;
                        case 1: attr.m_Errors.Add(PELocalization.GetString(9500449)); break;
                        case 2:
                            {
                                if(l_hilt_cnt == 0)
                                {
                                    attr.m_Errors.Add(PELocalization.GetString(9500447));
                                }

                                if(r_hilt_cnt == 0)
                                {
                                    attr.m_Errors.Add(PELocalization.GetString(9500448));
                                }
                            }
                            break;
                        default: break;

                    }

                    if (lscnt >1 || rscnt > 1)
                        attr.m_Errors.Add(PELocalization.GetString(9500446)); //场景一侧只能摆放一个剑柄

                    if (attr.m_Errors.Count == 0)
                        attr.m_Type = ECreation.SwordDouble;
                }
                break;
			case EVCCategory.cgBow:
				{
					int bow_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpBowGrip)
							bow_cnt++;
					}

					if (bow_cnt == 0)
					{
						attr.m_Errors.Add("No bow grip".ToLocalizationString());
					}
					if (bow_cnt > 1)
					{
						attr.m_Errors.Add("Too many bow grips, max is 1".ToLocalizationString());
					}

					if (attr.m_Errors.Count == 0)
						attr.m_Type = ECreation.Bow;
				}
				break;

			case EVCCategory.cgAxe:
				{
					int axe_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpAxeHilt)
							axe_cnt++;
					}

					if (axe_cnt == 0)
					{
						attr.m_Errors.Add("No hilt".ToLocalizationString());
					}
					if (axe_cnt > 1)
					{
					attr.m_Errors.Add("Too many hilts, max is 1".ToLocalizationString());
					}

					if (attr.m_Errors.Count == 0)
						attr.m_Type = ECreation.Axe;
				}
				break;

			case EVCCategory.cgShield:
				{
					int handle_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpShieldHandle)
							handle_cnt++;
					}

					if (handle_cnt == 0)
					{
						attr.m_Errors.Add("No handle".ToLocalizationString());
					}
					if (handle_cnt > 1)
					{
					attr.m_Errors.Add("Too many handles, max is 1".ToLocalizationString());
					}

					if (attr.m_Errors.Count == 0)
						attr.m_Type = ECreation.Shield;
				}
				break;
			case EVCCategory.cgGun:
				{
					int handle_cnt = 0;
					int muzzle_cnt = 0;
					VCPGunHandle handle_prop = null;
					VCPGunMuzzle muzzle_prop = null;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpGunHandle)
						{
							handle_prop = VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPGunHandle>();
							handle_cnt++;
						}
						else if (cdata.m_Type == EVCComponent.cpGunMuzzle)
						{
							muzzle_prop = VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPGunMuzzle>();
							muzzle_cnt++;
						}
					}

					if (handle_cnt == 0)
					{
						attr.m_Errors.Add("No handle".ToLocalizationString());
					}
					if (handle_cnt > 1)
					{
						attr.m_Errors.Add("Too many handles, max is 1".ToLocalizationString());
					}

					bool dual_hand = false;
					int handle_type = 0;
					int muzzle_type = 0;
					if (handle_prop != null)
					{
						dual_hand = handle_prop.DualHand;
						handle_type = handle_prop.GunType;
					}
					if (muzzle_prop != null)
					{
						muzzle_type = muzzle_prop.GunType;
					}
					if (muzzle_cnt == 0)
					{
						attr.m_Errors.Add("No muzzle".ToLocalizationString());
					}
					if (muzzle_cnt > 1)
					{
						attr.m_Errors.Add("Too many muzzles, max is 1".ToLocalizationString());
					}
					if (handle_type != muzzle_type)
					{
						attr.m_Errors.Add("Gun type mismatch".ToLocalizationString());
					}
					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = dual_hand ? ECreation.Rifle : ECreation.HandGun;
					}
				}
				break;
			case EVCCategory.cgVehicle:
				{
					int cockpit_cnt = 0;
					int wheel_cnt = 0;
					int steering_tire_cnt = 0;
					int motor_tire_cnt = 0;
					int fuel_cnt = 0;
					int engine_cnt = 0;
					int jet_cnt = 0;

					int ct_cnt = 0;
					int fc_cnt = 0;
					int ml_cnt = 0;
					int ai_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpVehicleCockpit)
							cockpit_cnt++;
						else if (cdata.m_Type == EVCComponent.cpVehicleWheel)
						{
							wheel_cnt++;
							if ((cdata as VCQuadphaseFixedPartData).isSteerWheel) steering_tire_cnt++;
							if ((cdata as VCQuadphaseFixedPartData).isMotorWheel) motor_tire_cnt++;
						}
						else if (cdata.m_Type == EVCComponent.cpVehicleFuelCell)
							fuel_cnt++;
						else if (cdata.m_Type == EVCComponent.cpVehicleEngine)
							engine_cnt++;
						else if (cdata.m_Type == EVCComponent.cpJetExhaust)
							jet_cnt++;
						else if (cdata.m_Type == EVCComponent.cpCtrlTurret)
							ct_cnt++;
						else if (cdata.m_Type == EVCComponent.cpFrontCannon)
							fc_cnt++;
						else if (cdata.m_Type == EVCComponent.cpMissileLauncher)
							ml_cnt++;
					}
					if (cockpit_cnt != 1)
						attr.m_Errors.Add("Zero or more than one cockpit".ToLocalizationString());

					if (wheel_cnt == 0)
						attr.m_Errors.Add("No wheel".ToLocalizationString());
					else if (wheel_cnt == 1)
						attr.m_Warnings.Add("Need more wheel(s) to keep balance".ToLocalizationString());
					else if (wheel_cnt > 16)
						attr.m_Errors.Add("Too many wheels, max is 16".ToLocalizationString());

					if (motor_tire_cnt == 0)
						attr.m_Errors.Add("No motor wheel".ToLocalizationString());

					if (steering_tire_cnt == 0)
						attr.m_Errors.Add("No steering wheel".ToLocalizationString());
					else if (steering_tire_cnt == 1)
						attr.m_Warnings.Add("More than one steering wheel needed".ToLocalizationString());

					if (fuel_cnt == 0)
						attr.m_Errors.Add("No fuel cell".ToLocalizationString());
					else if (fuel_cnt > 8)
						attr.m_Errors.Add("Too many fuel cells, max is".ToLocalizationString() + " 8");

					if (engine_cnt != 1)
						attr.m_Errors.Add("Zero or more than one engine".ToLocalizationString());

					if (jet_cnt > 4)
						attr.m_Errors.Add("Too many jet exhausts, max is".ToLocalizationString() + " 4");
					if (ct_cnt > 20)
						attr.m_Errors.Add("Too many turrets, max is".ToLocalizationString() + " 20");
					if (fc_cnt > 10)
						attr.m_Errors.Add("Too many canons, max is".ToLocalizationString() + " 10");
					if (ml_cnt > 2)
						attr.m_Errors.Add("Too many missiles, max is".ToLocalizationString() + " 2");
					if (ai_cnt > 8)
						attr.m_Errors.Add("Too many ai-towers, max is".ToLocalizationString() + " 8");

					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = ECreation.Vehicle;
					}
				}
				break;
			case EVCCategory.cgAircraft:
				{
					int cockpit_cnt = 0;
					int propellor_cnt = 0;
					int fuel_cnt = 0;
					int jet_cnt = 0;
					int thruster_cnt = 0;

					int ct_cnt = 0;
					int fc_cnt = 0;
					int ml_cnt = 0;
					int ai_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpVtolCockpit)
							cockpit_cnt++;
						else if (cdata.m_Type == EVCComponent.cpVtolRotor)
							propellor_cnt++;
						else if (cdata.m_Type == EVCComponent.cpVtolFuelCell || cdata.m_Type == EVCComponent.cpVehicleFuelCell)
							fuel_cnt++;
						else if (cdata.m_Type == EVCComponent.cpJetExhaust)
							jet_cnt++;
						else if (cdata.m_Type == EVCComponent.cpAirshipThruster)
							thruster_cnt++;
						else if (cdata.m_Type == EVCComponent.cpCtrlTurret)
							ct_cnt++;
						else if (cdata.m_Type == EVCComponent.cpFrontCannon)
							fc_cnt++;
						else if (cdata.m_Type == EVCComponent.cpMissileLauncher)
							ml_cnt++;
					}
					VCESceneSetting ss = iso.m_HeadInfo.FindSceneSetting();
					bool battle_ship = ss.m_EditorSize.z > 200;

					if (cockpit_cnt == 0)
						attr.m_Errors.Add("No cockpit".ToLocalizationString());
					if (cockpit_cnt > 1)
					attr.m_Errors.Add("Too many cockpits, max is".ToLocalizationString() + " 1");
					if (propellor_cnt + thruster_cnt == 0)
						attr.m_Errors.Add("No rotor or thruster".ToLocalizationString());
					else if (propellor_cnt + thruster_cnt > 32)
						attr.m_Errors.Add("Too many rotors, max is".ToLocalizationString() + " 32");
					if (fuel_cnt == 0)
						attr.m_Errors.Add("No fuel cell".ToLocalizationString());
					else if (fuel_cnt > (battle_ship ? 32 : 8))
						attr.m_Errors.Add("Too many fuel cells, max is".ToLocalizationString() + " " + (battle_ship ? 32 : 8).ToString());
					if (jet_cnt > (battle_ship ? 32 : 16))
						attr.m_Errors.Add("Too many jet exhausts, max is".ToLocalizationString() + " " +  (battle_ship ? 32 : 16).ToString());
					if (thruster_cnt > (battle_ship ? 16 : 2))
						attr.m_Errors.Add("Too many thrusters, max is".ToLocalizationString() + " " +  (battle_ship ? 16 : 2).ToString());
					if (ct_cnt > 20)
						attr.m_Errors.Add("Too many turrets, max is".ToLocalizationString() + " 20");
					if (fc_cnt > 10)
						attr.m_Errors.Add("Too many canons, max is".ToLocalizationString() + " 10");
					if (ml_cnt > 2)
						attr.m_Errors.Add("Too many missiles, max is".ToLocalizationString() + " 2");
					if (ai_cnt > 8)
						attr.m_Errors.Add("Too many ai-towers, max is".ToLocalizationString() + " 8");
					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = ECreation.Aircraft;
					}

				}
				break;
			case EVCCategory.cgBoat:
				{
					int cockpit_cnt = 0;
					int propellor_cnt = 0;
					int fuel_cnt = 0;
					int jet_cnt = 0;
					int rudder_cnt = 0;
					int tank_cnt = 0;

					int ct_cnt = 0;
					int fc_cnt = 0;
					int ml_cnt = 0;
					int ai_cnt = 0;

					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpShipCockpit)
							cockpit_cnt++;
						else if (cdata.m_Type == EVCComponent.cpShipPropeller)
							propellor_cnt++;
						else if (cdata.m_Type == EVCComponent.cpVehicleFuelCell || cdata.m_Type == EVCComponent.cpVtolFuelCell)
							fuel_cnt++;
						else if (cdata.m_Type == EVCComponent.cpJetExhaust)
							jet_cnt++;
						else if (cdata.m_Type == EVCComponent.cpShipRudder)
							rudder_cnt++;
						else if (cdata.m_Type == EVCComponent.cpCtrlTurret)
							ct_cnt++;
						else if (cdata.m_Type == EVCComponent.cpFrontCannon)
							fc_cnt++;
						else if (cdata.m_Type == EVCComponent.cpMissileLauncher)
							ml_cnt++;
						else if (cdata.m_Type == EVCComponent.cpSubmarineBallastTank)
							tank_cnt++;
					}
					if (cockpit_cnt == 0)
						attr.m_Errors.Add("No cockpit".ToLocalizationString());
					if (cockpit_cnt > 1)
						attr.m_Errors.Add("Too many cockpits, max is".ToLocalizationString() + " 1");
					if (propellor_cnt == 0)
						attr.m_Errors.Add("No propeller".ToLocalizationString());
//					else if (propellor_cnt < 2)
//					attr.m_Warnings.Add("More propeller(s) to keep thrust and balance, min is 2");
					else if (propellor_cnt > 32)
						attr.m_Errors.Add("Too many propellers, max is".ToLocalizationString() + " 32");
					if (fuel_cnt == 0)
						attr.m_Errors.Add("No fuel cell".ToLocalizationString());
					else if (fuel_cnt > 16)
						attr.m_Errors.Add("Too many fuel cells, max is".ToLocalizationString() + " 16");
					if (jet_cnt > 4)
						attr.m_Errors.Add("Too many jet exhausts, max is".ToLocalizationString() + " 4");
					if (ct_cnt > 20)
						attr.m_Errors.Add("Too many turrets, max is".ToLocalizationString() + " 20");
					if (fc_cnt > 10)
						attr.m_Errors.Add("Too many canons, max is".ToLocalizationString() + " 10");
					if (ml_cnt > 2)
						attr.m_Errors.Add("Too many missiles, max is".ToLocalizationString() + " 2");
					if (ai_cnt > 8)
						attr.m_Errors.Add("Too many ai-towers, max is".ToLocalizationString() + " 8");

					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = ECreation.Boat;
					}

				}
				break;

			case EVCCategory.cgObject:
				{
					int bed_cnt = 0;
					int light_cnt = 0;
					int pivot_cnt = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpBed)
							bed_cnt++;
						if (cdata.m_Type == EVCComponent.cpLight)
							light_cnt++;
						if (cdata.m_Type == EVCComponent.cpPivot)
							pivot_cnt++;
					}

					if (bed_cnt > 4)
						attr.m_Errors.Add("Too many beds, max is".ToLocalizationString() + " 4");
					if (light_cnt > 4)
						attr.m_Errors.Add("Too many lights, max is".ToLocalizationString() + " 4");
					if (pivot_cnt > 1)
						attr.m_Errors.Add("Too many pivots, max is".ToLocalizationString() + " 1");

					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = ECreation.SimpleObject;
					}
					else
					{
						attr.m_Type = ECreation.Null;
					}
				}
				break;

			case EVCCategory.cgRobot:
				{
					int controller_cnt = 0;
					int battery_cnt = 0;
					int weapon_cnt = 0;

					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpRobotController)
							controller_cnt++;
						else if (cdata.m_Type == EVCComponent.cpRobotBattery)
							battery_cnt++;
						else if (cdata.m_Type == EVCComponent.cpRobotWeapon)
							weapon_cnt++;
					}

					if (controller_cnt != 1)
						attr.m_Errors.Add("Zero or more than 1 controller".ToLocalizationString());
					if (battery_cnt != 1)
						attr.m_Errors.Add("Zero or more than 1 battery".ToLocalizationString());
					if (weapon_cnt > 2)
						attr.m_Errors.Add("More than 2 weapons".ToLocalizationString());

					//////////////////////////////////
#if !UNITY_EDITOR
					//attr.m_Errors.Add("Coming soon...");
#endif

					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = ECreation.Robot;
					}
					else
					{
						attr.m_Type = ECreation.Null;
					}
				}
				break;

			case EVCCategory.cgAITurret:
				{
					int weapon_cnt = 0;

					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpAITurretWeapon)
							weapon_cnt++;
					}

					if (weapon_cnt != 1)
						attr.m_Errors.Add("Zero or more than 1 weapon".ToLocalizationString());

					if (attr.m_Errors.Count == 0)
					{
						attr.m_Type = ECreation.AITurret;
					}
					else
					{
						attr.m_Type = ECreation.Null;
					}
				}
				break;

			case EVCCategory.cgHeadArmor:
			case EVCCategory.cgBodyArmor:
			case EVCCategory.cgArmAndLegArmor:
			case EVCCategory.cgHandAndFootArmor:
			case EVCCategory.cgDecorationArmor:
				{
					int pivotCount = 0;
					foreach (VCComponentData cdata in iso.m_Components)
					{
						if (cdata.m_Type == EVCComponent.cpHeadPivot
							|| cdata.m_Type == EVCComponent.cpBodyPivot
							|| cdata.m_Type == EVCComponent.cpArmAndLegPivot
							|| cdata.m_Type == EVCComponent.cpHandAndFootPivot
							|| cdata.m_Type == EVCComponent.cpDecorationPivot)
						{
							pivotCount++;
						}
					}

					if (pivotCount != 1)
					{
					attr.m_Errors.Add("Zero or more than one pivot".ToLocalizationString());
						attr.m_Type = ECreation.Null;
					}
					else
					{
						attr.m_Type = (ECreation)((int)iso.m_HeadInfo.Category);
					}

				}
				break;

			default:
				{
					attr.m_Errors.Add("Unknown category".ToLocalizationString());
				}
				break;
		}
	}

	private static void CalcCommonAttr(VCIsoData iso, CreationAttr attr)
	{
		double cWeight = 0;
		double vWeight = 0;
		double cVolume = 0;
		double vVolume = 0;
		double cSumX = 0, cSumY = 0, cSumZ = 0;
		double vSumX = 0, vSumY = 0, vSumZ = 0;


       

		VCESceneSetting scene_setting = iso.m_HeadInfo.FindSceneSetting();

		foreach (KeyValuePair<int, VCMatterInfo> kvp in VCConfig.s_Matters)
			attr.m_Cost.Add(kvp.Value.ItemId, 0);

		Vector3 half = Vector3.one * 0.5f;
		double voxel_size = scene_setting.m_VoxelSize;
		double cell_vol = voxel_size * voxel_size * voxel_size;
		foreach (KeyValuePair<int, VCVoxel> kvp in iso.m_Voxels)
		{
			if (kvp.Value.Volume < VCEMath.MC_ISO_VALUE)
				continue;

			IntVector3 pos = VCIsoData.KeyToIPos(kvp.Key);
			Vector3 wpos = (pos.ToVector3() + half) * (float)(voxel_size);
			double v = 1;
			double w = v;

			VCMaterial vcmat = iso.m_Materials[kvp.Value.Type];
			if (vcmat != null)
			{
				VCMatterInfo matter = VCConfig.s_Matters[vcmat.m_MatterId];
				w = matter.Density * 1000 * v;
				attr.m_Durability += matter.Durability;
				if (v > 0.5f)
					attr.m_Cost[vcmat.ItemId]++;
			}
			vSumX += wpos.x * w;
			vSumY += wpos.y * w;
			vSumZ += wpos.z * w;
			vWeight += w;
			vVolume += v;

           
		}

		if (iso.m_Colors.Count > 0)
		{
			float color_cnt = 0;
			foreach (KeyValuePair<int, Color32> kvp in iso.m_Colors)
			{
				float dr = Mathf.Abs(VCIsoData.BLANK_COLOR.r - kvp.Value.r);
				float dg = Mathf.Abs(VCIsoData.BLANK_COLOR.g - kvp.Value.g);
				float db = Mathf.Abs(VCIsoData.BLANK_COLOR.b - kvp.Value.b);
				float da = Mathf.Abs(VCIsoData.BLANK_COLOR.a - kvp.Value.a);
				float density = Mathf.Clamp01((dr + dg + db + da) * 0.01f);
				IntVector3 cpos = VCIsoData.KeyToIPos(kvp.Key);
				Vector3 ipos = cpos.ToVector3() * 0.5f - 0.5f * Vector3.one;
				if (iso.CanSee(ipos))
					color_cnt += density;
			}
			attr.m_Cost.Add(VCConfig.s_DyeID, Mathf.CeilToInt((color_cnt / scene_setting.m_DyeUnit) - 0.1f));
		}

		foreach (KeyValuePair<int, VCMatterInfo> kvp in VCConfig.s_Matters)
		{
			attr.m_Cost[kvp.Value.ItemId] = Mathf.CeilToInt((float)(attr.m_Cost[kvp.Value.ItemId]) / scene_setting.m_BlockUnit);
			attr.m_SellPrice += (kvp.Value.SellPrice * attr.m_Cost[kvp.Value.ItemId]);
		}

		vVolume *= cell_vol;
		vWeight *= cell_vol;
		vSumX *= cell_vol;
		vSumY *= cell_vol;
		vSumZ *= cell_vol;

		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (VCConfig.s_Parts.ContainsKey(cdata.m_ComponentId))
			{
				VCPartInfo part = VCConfig.s_Parts[cdata.m_ComponentId];
				if (part.m_ResObj == null)
					throw new Exception("Can't load resource of part".ToLocalizationString() + ": [" + part.m_Name + "]");
				VCEComponentTool ctool = part.m_ResObj.GetComponent<VCEComponentTool>();
				if (ctool == null || ctool.m_SelBound == null)
					throw new Exception("Incomplete resource of part".ToLocalizationString() + ": [" + part.m_Name + "]");

				// Calculate mass center
				Vector3 p = cdata.m_Position;
				if (ctool.m_MassCenter != null)
				{
					// Set trans
					part.m_ResObj.transform.position = cdata.m_Position;
					part.m_ResObj.transform.eulerAngles = cdata.m_Rotation;
					part.m_ResObj.transform.localScale = cdata.m_Scale;
					// Fetch position
					p = ctool.m_MassCenter.position;
					// Revert trans
					part.m_ResObj.transform.position = Vector3.zero;
					part.m_ResObj.transform.rotation = Quaternion.identity;
					part.m_ResObj.transform.localScale = Vector3.one;
				}
				// Calculate volume and weight
				Transform boundtr = ctool.m_SelBound.transform;
				double v = boundtr.localScale.x * boundtr.localScale.y * boundtr.localScale.z
						 * cdata.m_Scale.x * cdata.m_Scale.y * cdata.m_Scale.z * part.m_Volume;
				double w = VCConfig.s_Parts[cdata.m_ComponentId].m_Weight * cdata.m_Scale.x * cdata.m_Scale.y * cdata.m_Scale.z;

				cSumX += p.x * w;
				cSumY += p.y * w;
				cSumZ += p.z * w;
				cWeight += w;
				cVolume += v;

				// Cost
				if (part.m_CostCount > 0)
				{
					int iid = part.m_ItemID;
					if (!attr.m_Cost.ContainsKey(iid))
						attr.m_Cost.Add(iid, 0);
					attr.m_Cost[iid] += part.m_CostCount;
					attr.m_SellPrice += part.m_SellPrice * part.m_CostCount;
				}
			}
		}

		attr.m_Weight = (float)(cWeight + vWeight);
		attr.m_Volume = (float)(cVolume + vVolume);
       
		attr.m_CenterOfMass = Vector3.zero;
		if (attr.m_Weight > 0)
		{
			Vector3 sum = new Vector3((float)(cSumX + vSumX), (float)(cSumY + vSumY), (float)(cSumZ + vSumZ));
			attr.m_CenterOfMass = sum / attr.m_Weight;
		}
	}

	#region EACH_CREATION_ATTR

	private static void CalcSwordAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		int dimx = 1 << 10;
		int dimxz = 1 << 20;
        
		int[] shape_cnt = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] cRange0 = new int[iso.m_HeadInfo.ySize];
        int[] cRange1 = new int[iso.m_HeadInfo.ySize];
        VCESceneSetting scene_setting = iso.m_HeadInfo.FindSceneSetting();

		foreach (KeyValuePair<int, VCVoxel> pair in iso.m_Voxels)
		{
			int negative = 0, positive = 0;
			int faces = 0;
			int x, y, z;
			x = pair.Key & 0x3ff;
			y = pair.Key >> 20;
			z = (pair.Key >> 10) & 0x3ff;

			if (x == 0 || iso.GetVoxel(pair.Key - 1).Volume < 128)
			{
				negative = negative | 1;
				faces++;
			}
			if (x == iso.m_HeadInfo.xSize - 1 || iso.GetVoxel(pair.Key + 1).Volume < 128)
			{
				positive = positive | 1;
				faces++;
			}
			if (y == 0 || iso.GetVoxel(pair.Key - dimxz).Volume < 128)
			{
				negative = negative | 2;
				faces++;
			}
			if (y == iso.m_HeadInfo.ySize - 1 || iso.GetVoxel(pair.Key + dimxz).Volume < 128)
			{
				positive = positive | 2;
				faces++;
			}
			if (z == 0 || iso.GetVoxel(pair.Key - dimx).Volume < 128)
			{
				negative = negative | 4;
				faces++;
			}
			if (z == iso.m_HeadInfo.zSize - 1 || iso.GetVoxel(pair.Key + dimx).Volume < 128)
			{
				positive = positive | 4;
				faces++;
			}

			if (faces == 0)
				shape_cnt[0]++;
			else if (faces == 1)
				shape_cnt[1]++;
			else if (faces == 2)
				shape_cnt[((negative & positive) == 0) ? 2 : 3]++;
			else if (faces == 3)
				shape_cnt[((negative & positive) == 0) ? 4 : 5]++;
			else if (faces == 4)
				shape_cnt[((negative | positive) == 7) ? 6 : 7]++;
			else if (faces == 5)
				shape_cnt[8]++;
			else if (faces == 6)
				shape_cnt[9]++;

            //atk range
            IntVector3 pos = VCIsoData.KeyToIPos(pair.Key);
            if(attr.m_Type == ECreation.SwordDouble)
            {
                if(pos.x < 0.5f * iso.m_HeadInfo.xSize) //L
                    cRange0[pos.y] = 1;
                else
                    cRange1[pos.y] = 1;
            }
            else
            {
                cRange0[pos.y] = 1;
            }
            
		}

		// Face count                        0          1          2                 3                 4               5        6
		float[] sharpness = new float[10] { 1.0f, 2.0f, 5.0f, 3.0f, 12.0f, 6.0f, 35.0f, 10.0f, 70.0f, 3.0f };
		float[] limits = new float[10] { 10000.0f, 10000.0f, 1500.0f, 7000.0f, 1200.0f, 1200.0f, 350.0f, 350.0f, 300.0f, 200.0f };
		// Index                             0          1          2       3         4       5         6      7        8        9

		for (int i = 0; i < 10; i++)
		{
			float u = shape_cnt[i] / limits[i];
			float t = Mathf.Pow(0.4f, u);
			sharpness[i] = Mathf.Lerp(0.2f, sharpness[i], t);
		}

		float attack = 0;
		float normal_attack = 0;
		foreach (KeyValuePair<int, VCVoxel> pair in iso.m_Voxels)
		{
			int this_vol = pair.Value.Volume;

			float avevol = 0;
			float volcnt = 0;

			float coef = 0.0f;
			int negative = 0, positive = 0;
			int faces = 0;
			int x, y, z;
			x = pair.Key % dimx;
			y = pair.Key / dimxz;
			z = (pair.Key % dimxz) / dimx;

			int tmpv = 0;

			tmpv = iso.GetVoxel(pair.Key - 1).Volume;
			if (x == 0 || tmpv < 128)
			{
				negative = negative | 1;
				faces++;
			}
			else
			{
				avevol += tmpv;
				volcnt += 1.0f;
			}

			tmpv = iso.GetVoxel(pair.Key + 1).Volume;
			if (x == iso.m_HeadInfo.xSize - 1 || tmpv < 128)
			{
				positive = positive | 1;
				faces++;
			}
			else
			{
				avevol += tmpv;
				volcnt += 1.0f;
			}

			tmpv = iso.GetVoxel(pair.Key - dimxz).Volume;
			if (y == 0 || tmpv < 128)
			{
				negative = negative | 2;
				faces++;
			}
			else
			{
				avevol += tmpv;
				volcnt += 1.0f;
			}

			tmpv = iso.GetVoxel(pair.Key + dimxz).Volume;
			if (y == iso.m_HeadInfo.ySize - 1 || tmpv < 128)
			{
				positive = positive | 2;
				faces++;
			}
			else
			{
				avevol += tmpv;
				volcnt += 1.0f;
			}

			tmpv = iso.GetVoxel(pair.Key - dimx).Volume;
			if (z == 0 || tmpv < 128)
			{
				negative = negative | 4;
				faces++;
			}
			else
			{
				avevol += tmpv;
				volcnt += 1.0f;
			}

			tmpv = iso.GetVoxel(pair.Key + dimx).Volume;
			if (z == iso.m_HeadInfo.zSize - 1 || tmpv < 128)
			{
				positive = positive | 4;
				faces++;
			}
			else
			{
				avevol += tmpv;
				volcnt += 1.0f;
			}

			if (faces == 0)
				coef = sharpness[0];
			else if (faces == 1)
				coef = sharpness[1];
			else if (faces == 2)
				coef = ((negative & positive) == 0) ? sharpness[2] : sharpness[3];
			else if (faces == 3)
				coef = ((negative & positive) == 0) ? sharpness[4] : sharpness[5];
			else if (faces == 4)
				coef = ((negative | positive) == 7) ? sharpness[6] : sharpness[7];
			else if (faces == 5)
				coef = sharpness[8];
			else if (faces == 6)
				coef = sharpness[9];

			// ------------------ calc volume sharpness (v0.75)
			float vsharp = 1;
			if ((faces > 3 || faces == 3 && (negative & positive) != 0) && (this_vol >= 128))
			{
				if (volcnt > 0)
				{
					avevol /= volcnt;
					vsharp = (128.0f / (float)(this_vol)) * (255.0f / avevol);
				}
				else
				{
					vsharp = 255.0f / (float)(this_vol);
				}
			}

			VCMatterInfo mi = VCConfig.s_Matters[iso.m_Materials[pair.Value.Type].m_MatterId];
			attack += mi.Attack * coef * Mathf.Max(1, Mathf.Pow(vsharp, 4));
			normal_attack += mi.Attack;
		}
		float increase_rate = Mathf.Pow(Mathf.Clamp(normal_attack / attack, 0, 1), 0.7f);

		//Debug.Log("attack before increase = " + normal_attack.ToString("0.000"));
		//Debug.Log("durability before decrease = " + attr.m_Durability.ToString("0.000"));

		//Debug.Log("attack coef = " + (attack / normal_attack).ToString("0.000") + "  dur coef = " + increase_rate.ToString("0.000"));
		float durability = attr.m_Durability;

		durability *= increase_rate;
		durability *= 0.08f;
		attack *= 0.027f;

		//Debug.Log("attack (no hilt) = " + attack.ToString("0.000"));
		//Debug.Log("durability (no hilt) = " + durability.ToString("0.000"));

        float hiltLcurY = 0.0f;
        float hiltRcurY = 0.0f;
        int dbcnt = 0;
        bool cw = true;
		foreach (VCComponentData cdata in iso.m_Components)
		{
            if (attr.m_Type == ECreation.SwordDouble)
            {
                if(cdata.m_Type == EVCComponent.cpDbSwordHilt && (cdata as VCFixedHandPartData).m_LeftHand){
                    dbcnt++;
                    cw = cdata.m_Position.x < iso.m_HeadInfo.xSize * scene_setting.m_VoxelSize * 0.5f;
                }

                if (cdata.m_Type == EVCComponent.cpDbSwordHilt && !(cdata as VCFixedHandPartData).m_LeftHand) dbcnt++;
                
                if (cdata.m_Position.x < iso.m_HeadInfo.xSize * scene_setting.m_VoxelSize * 0.5f)
                    hiltLcurY = cdata.m_Position.y;
                else
                    hiltRcurY = cdata.m_Position.y;

                if(dbcnt == 2)
                {
                    attack *= VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackEnh;
                    attack += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackInc;
                    durability *= VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityEnh;
                    durability += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityInc;
                    break;
                }
            }
            else
            {
                if (cdata.m_Type == EVCComponent.cpSwordHilt || cdata.m_Type == EVCComponent.cpLgSwordHilt || cdata.m_Type == EVCComponent.cpDbSwordHilt)
                {
                    attack *= VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackEnh;
                    attack += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackInc;
                    durability *= VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityEnh;
                    durability += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityInc;
                    hiltLcurY = cdata.m_Position.y;
                    hiltRcurY = cdata.m_Position.y;
                    break;
                }
            }
			
		}

		//Debug.Log("attack before constraint = " + attack.ToString("0.000"));
		//Debug.Log("durability before constraint = " + durability.ToString("0.000"));



        //calc attackheight
        float pivotY = attr.m_Type == ECreation.SwordDouble || attr.m_Type == ECreation.SwordLarge ? VoxelTerrainConstants._spHiltPivotY : VoxelTerrainConstants._normalHiltPivotY;
        int hiltyL0 = Mathf.CeilToInt((hiltLcurY - pivotY) / scene_setting.m_VoxelSize);
        int hiltyL1 = Mathf.CeilToInt((hiltLcurY + pivotY) / scene_setting.m_VoxelSize);

        int hiltyR0 = Mathf.CeilToInt((hiltRcurY - pivotY) / scene_setting.m_VoxelSize);
        int hiltyR1 = Mathf.CeilToInt((hiltRcurY + pivotY) / scene_setting.m_VoxelSize);

        int cyr0 = 0;
        int offset0 = 0;
        for(int i=0;i<cRange0.Length;i++)
        {
            cyr0 += cRange0[i];
            if (cRange0[i] != 0 && i >= hiltyL0 && i <= hiltyL1) cyr0--;//voxel在剑柄区域不算入加攻击距离
            if (cRange0[i] != 0 && i < hiltyL0) offset0++;//voxel在剑柄反向位置
        }
       cyr0 += (hiltyL1 - hiltyL0);

        int cyr1 = 0;
        int offset1 = 0; 
        for (int i = 0; i < cRange1.Length; i++)
        {
            cyr1 += cRange1[i];
            if (cRange1[i] != 0 && i >= hiltyR0 && i <= hiltyR1) cyr1--;//voxel在剑柄区域不算入加攻击距离
            if (cRange0[i] != 0 && i < hiltyR0) offset1++;//voxel在剑柄反向位置
        }
         cyr1 += (hiltyR1 - hiltyR0); 
        
        float h0 = (cyr0 +1) * scene_setting.m_VoxelSize;
        float h1 = (cyr1 +1) * scene_setting.m_VoxelSize;

        float offseth0 = offset0 * scene_setting.m_VoxelSize;
        float offseth1 = offset1 * scene_setting.m_VoxelSize;

        if(!cw)//不是顺时摆放（左右颠倒）
        {
            float tmp = h0;
            h0 = h1; h1 = tmp;

            tmp = offseth0;
            offseth0 = offseth1;
            offseth1 = tmp;
        }
		// Constraints
		attr.m_Attack = VCEMath.SmoothConstraint(attack, 210, 1.25f);
		attr.m_Durability = VCEMath.SmoothConstraint(durability, 2000, 0.5f);
        attr.m_AtkHeight = new Vector4(h0, h1, offseth0,offseth1);//new Vector3();
           
	}

	private static void CalcShieldAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCPShieldHandle handle = null;
		Vector3 handle_pos = Vector3.zero;
		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpShieldHandle)
			{
				handle = VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPShieldHandle>();
				handle_pos = cdata.m_Position;
				break;
			}
		}

		float voxel_size = iso.m_HeadInfo.FindSceneSetting().m_VoxelSize;
		float[,] thickness = new float[iso.m_HeadInfo.xSize / 5 + 1, iso.m_HeadInfo.ySize / 5 + 1];
		float[,] stability = new float[iso.m_HeadInfo.xSize / 5 + 1, iso.m_HeadInfo.ySize / 5 + 1];
		int[,] blockcnt = new int[iso.m_HeadInfo.xSize / 5 + 1, iso.m_HeadInfo.ySize / 5 + 1];
		int center_x = (int)(handle_pos.x / voxel_size) / 5;
		int center_y = (int)(handle_pos.y / voxel_size) / 5;

		foreach (KeyValuePair<int, VCVoxel> pair in iso.m_Voxels)
		{
			int x = pair.Key & 0x3ff;
			int y = pair.Key >> 20;
			if (pair.Value.Volume > 127)
			{
				blockcnt[x / 5, y / 5]++;
				VCMatterInfo mi = VCConfig.s_Matters[iso.m_Materials[pair.Value.Type].m_MatterId];
				thickness[x / 5, y / 5] += mi.Defence;
				stability[x / 5, y / 5] += mi.Durability;
			}
		}

		float def_raw = 0;
		float dur_raw = 0;
		for (int i = 0; i < iso.m_HeadInfo.xSize / 5 + 1; i++)
		{
			for (int j = 0; j < iso.m_HeadInfo.ySize / 5 + 1; j++)
			{
				if (blockcnt[i, j] >= 17)
				{
					float dist = Vector2.Distance(new Vector2(i, j), new Vector2(center_x, center_y));
					float dist_coef = Mathf.Clamp01((9 - dist) / 8) + 0.05f;
					if (dist_coef > 0)
					{
						float def_mat = thickness[i, j] / blockcnt[i, j];
						float dur_mat = stability[i, j] / blockcnt[i, j];
						float cnt_coef = Mathf.Pow(blockcnt[i, j], 0.25f);
						float def_inc = def_mat * cnt_coef * dist_coef;
						float dur_inc = dur_mat * cnt_coef * dist_coef;
						def_raw += def_inc;
						dur_raw += dur_inc;
					}
				}
			}
		}

		def_raw *= 2.4f;
		dur_raw *= 0.3f;

		def_raw *= handle.DefenceEnh;
		def_raw += handle.BaseDefence;
		dur_raw *= handle.DurabilityEnh;
		dur_raw += handle.BaseDurability;

		attr.m_Attack = 0;
		attr.m_Defense = VCEMath.SmoothConstraint(def_raw, 232, 0.5f);
		attr.m_Durability = VCEMath.SmoothConstraint(dur_raw, 1500, 0.5f);
	}

	private static void CalcBowAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		attr.m_Durability = PEVCConfig.instance.bowDurabilityScale * Mathf.Sqrt(attr.m_Durability) + PEVCConfig.instance.bowDurabilityBase;
		attr.m_Attack = 0f;

		float elasticity = 0;

		foreach (KeyValuePair<int, VCVoxel> kvp in iso.m_Voxels)
		{
			if (kvp.Value.Volume < VCEMath.MC_ISO_VALUE)
				continue;

			VCMaterial vcmat = iso.m_Materials[kvp.Value.Type];
			if (vcmat != null)
			{
				VCMatterInfo matter = VCConfig.s_Matters[vcmat.m_MatterId];
				elasticity += (matter.Attack * 3.3f * kvp.Value.VolumeF);
			}
		}

		elasticity = Mathf.Clamp(elasticity / 100000f, 0f, 2f);
		elasticity = elasticity * (2f - elasticity);

		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpBowGrip)
			{
				VCPBowGrip bowPart = VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPBowGrip>();
				if (bowPart)
				{
					attr.m_Attack = bowPart.baseAttack + bowPart.maxExtendAttack * elasticity;
					break;
				}
			}
		}
	}

	private static void CalcGunAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		float step0 = attr.m_Weight + 1f;
		float step1 = Mathf.Log10(step0);
		float step2 = Mathf.Pow(step1 - 1.1f, 2f);
		float err_coef = (step2 + 0.05f) * 10;

		float attack = 0;
		float fire_speed = 0;

		float sum_atk = 0;

		foreach (KeyValuePair<int, VCVoxel> kvp in iso.m_Voxels)
		{
			if (kvp.Value.Volume < VCEMath.MC_ISO_VALUE)
				continue;

			VCMaterial vcmat = iso.m_Materials[kvp.Value.Type];
			if (vcmat != null)
			{
				VCMatterInfo matter = VCConfig.s_Matters[vcmat.m_MatterId];
				sum_atk += (matter.Attack * kvp.Value.VolumeF);
			}
		}

		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpGunMuzzle)
			{
				VCPGunMuzzle muzzle = VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<VCPGunMuzzle>();
				attack += muzzle.Attack;
				if (muzzle.Multishot)
					fire_speed += (1.0f / (muzzle.FireInterval + 0.001f));
				else
					fire_speed += 0.75f;
			}
		}

		attr.m_MuzzleAtkInc = VCEMath.SmoothConstraint(sum_atk * 0.002f, 0.724f, 1f);
		attr.m_Attack = attack * attr.m_MuzzleAtkInc;
		attr.m_FireSpeed = fire_speed;
		attr.m_Durability = VCEMath.SmoothConstraint(attr.m_Durability * 0.01f + 200, 1200, 0.5f);
		attr.m_Accuracy = VCEMath.SmoothConstraint(err_coef, 6f, 1f);
	}

	private static void CalcVehicleAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * settings.m_VoxelSize * 500f * PEVCConfig.instance.vehicleDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData cdata in iso.m_Components)
		{
            if (cdata.m_Type == EVCComponent.cpVehicleFuelCell)
				attr.m_MaxFuel += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPFuelCell>().energyCapacity;
		}

        CalcVCPWeaponAttack(iso, attr);
    }

	private static void CalcAircraftAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * Mathf.Pow(settings.m_VoxelSize, 3.0f) * 50000f * PEVCConfig.instance.helicopterDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpVtolFuelCell || cdata.m_Type == EVCComponent.cpVehicleFuelCell)
				attr.m_MaxFuel += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPFuelCell>().energyCapacity;
		}

        CalcVCPWeaponAttack(iso, attr);

    }

    private static void CalcBoatAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * Mathf.Pow(settings.m_VoxelSize, 3.0f) * 50000f * PEVCConfig.instance.boatDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpVehicleFuelCell || cdata.m_Type == EVCComponent.cpVtolFuelCell)
				attr.m_MaxFuel += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPFuelCell>().energyCapacity;
		}
		attr.m_FluidDisplacement = new List<VolumePoint>();
		int step = 20;
		float half_step = step * 0.5f;
		float cell_size = settings.m_VoxelSize * step;
		float cell_volume = cell_size * cell_size * cell_size / 2;
		double voxel_volume = settings.m_VoxelSize * settings.m_VoxelSize * settings.m_VoxelSize;
		Vector3 center = attr.m_CenterOfMass;
		center.y = 0;
		for (int y = 0; y < settings.m_EditorSize.y; y += step / 2)
		{
			for (int z = 0; z < settings.m_EditorSize.z; z += step)
			{
				for (int x = 0; x < settings.m_EditorSize.x; x += step)
				{
					Vector3 lpos = new Vector3(x + half_step, y + half_step, z + half_step) * settings.m_VoxelSize - center;
					double v = 0;
					for (int _y = 0; _y < step; ++_y)
					{
						for (int _z = 0; _z < step; ++_z)
						{
							for (int _x = 0; _x < step; ++_x)
							{
								if (iso.GetVoxel(VCIsoData.IPosToKey(x + _x, y + _y, z + _z)).Volume >= VCEMath.MC_ISO_VALUE)
									v += voxel_volume;
							}
						}
					}
					float disp = Mathf.Min(cell_volume, (float)v * 20);
					if (disp > 0)
						attr.m_FluidDisplacement.Add(new VolumePoint(lpos, disp, disp));
				}
			}
		}

        CalcVCPWeaponAttack(iso, attr);

    }

    private static void CalcSimpleObjectAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * settings.m_VoxelSize * 500f;
	}

	private static void CalcRobotAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		//VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * PEVCConfig.instance.robotDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpRobotBattery)
				attr.m_MaxFuel += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPFuelCell>().energyCapacity;
		}

        CalcVCPWeaponAttack(iso, attr);

    }

    private static void CalcAITurretAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		//VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * PEVCConfig.instance.aiTurretDurabilityScale;

		attr.m_MaxFuel = 0f;
		attr.m_Defense = 1f;
		foreach (VCComponentData cdata in iso.m_Components)
		{
			if (cdata.m_Type == EVCComponent.cpAITurretWeapon)
			{
				attr.m_MaxFuel += VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPWeapon>().bulletCapacity;
				attr.m_Defense = (VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPWeapon>().bulletProtoID == 0) ? 1f : 0f;
			}
		}

        CalcVCPWeaponAttack(iso, attr);

    }


    private static void CalcArmorAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		//VCESceneSetting settings = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * PEVCConfig.instance.armorDurabilityScale;
        attr.m_Defense = VCUtility.GetArmorDefence(attr.m_Durability);
	}


    private static void CalcVCPWeaponAttack(VCIsoData iso, CreationAttr attr)
    {
        attr.m_Attack = 0f;
        foreach (VCComponentData cdata in iso.m_Components)
        {
            switch (cdata.m_Type)
            {
                case EVCComponent.cpAITurretWeapon:
                case EVCComponent.cpCtrlTurret:
                case EVCComponent.cpFrontCannon:
                case EVCComponent.cpMissileLauncher:
                case EVCComponent.cpRobotWeapon:
                    var weapon = VCConfig.s_Parts[cdata.m_ComponentId].m_ResObj.GetComponent<WhiteCat.VCPWeapon>();
                    if (weapon) attr.m_Attack += weapon.attackPerSecond;
                    break;
            }
        }
    }

	#endregion

	public string TypeString()
	{
		// [VCCase] - Creation type string
		switch (m_Attribute.m_Type)
		{
			case ECreation.Null: return "Null".ToLocalizationString();
			case ECreation.Sword:
            case ECreation.SwordDouble:
            case ECreation.SwordLarge: return "Sword".ToLocalizationString();
			case ECreation.Axe: return "Axe".ToLocalizationString();
			case ECreation.Bow: return "Bow".ToLocalizationString();
			case ECreation.HandGun: return "Hand gun".ToLocalizationString();
			case ECreation.Rifle: return "Rifle".ToLocalizationString();
			case ECreation.Shield: return "Shield".ToLocalizationString();
			case ECreation.Vehicle: return "Vehicle".ToLocalizationString();
			case ECreation.Aircraft: return "Aircraft".ToLocalizationString();
			case ECreation.Boat: return "Boat".ToLocalizationString();
			case ECreation.SimpleObject: return "Object".ToLocalizationString();
			case ECreation.ArmorHead: return "Head Armor".ToLocalizationString();
			case ECreation.ArmorBody: return "Body Armor".ToLocalizationString();
			case ECreation.ArmorArmAndLeg: return "Arm And Leg Armor".ToLocalizationString();
			case ECreation.ArmorHandAndFoot: return "Hand And Foot Armor".ToLocalizationString();
			case ECreation.ArmorDecoration: return "Decoration Armor".ToLocalizationString();
			case ECreation.Robot: return "Robot".ToLocalizationString();
			case ECreation.AITurret: return "AI Turret".ToLocalizationString();
			default: return "??????";
		}
	}

	private string DivideValString(string name, float curr, float max, float red = 0.333f)
	{
		if (curr / max < red)
			return name + ": [FF3030]" + curr.ToString("0") + "[-] / [1FD0FF]" + max.ToString("0") + "[-]\r\n";
		else
			return name + ": [1FD0FF]" + curr.ToString("0") + "[-] / [1FD0FF]" + max.ToString("0") + "[-]\r\n";
	}
	private string WeaponDescString()
	{
		VCPWeapon[] weapons = m_Prefab.GetComponentsInChildren<VCPWeapon>(true);
		//VCPFrontCannonProperty [] fcs = m_Prefab.GetComponentsInChildren<VCPFrontCannonProperty>(true);
		//VCPMissileLauncherProperty [] mls = m_Prefab.GetComponentsInChildren<VCPMissileLauncherProperty>(true);
		//VCPAITurretProperty [] ats = m_Prefab.GetComponentsInChildren<VCPAITurretProperty>(true);
		string weapon_str = "";
		weapon_str += "Weapons".ToLocalizationString() + ":\r\n";
		Dictionary<string, int> weapon_cnts = new Dictionary<string, int>();
		foreach (var ct in weapons)
		{
			string weapon_name = ct.gameObject.name;
			if (weapon_cnts.ContainsKey(weapon_name))
				weapon_cnts[weapon_name]++;
			else
				weapon_cnts.Add(weapon_name, 1);
		}
		//foreach ( VCPFrontCannonProperty fc in fcs )
		//{
		//	string weapon_name = fc.gameObject.name;
		//	if ( weapon_cnts.ContainsKey(weapon_name) )
		//		weapon_cnts[weapon_name]++;
		//	else
		//		weapon_cnts.Add(weapon_name,1);
		//}
		//foreach ( VCPMissileLauncherProperty ml in mls )
		//{
		//	string weapon_name = ml.gameObject.name;
		//	if ( weapon_cnts.ContainsKey(weapon_name) )
		//		weapon_cnts[weapon_name]++;
		//	else
		//		weapon_cnts.Add(weapon_name,1);
		//}
		//foreach ( VCPAITurretProperty at in ats )
		//{
		//	string weapon_name = at.gameObject.name;
		//	if ( weapon_cnts.ContainsKey(weapon_name) )
		//		weapon_cnts[weapon_name]++;
		//	else
		//		weapon_cnts.Add(weapon_name,1);
		//}
		if (weapon_cnts.Count > 0)
		{
			foreach (KeyValuePair<string, int> kvp in weapon_cnts)
			{
				weapon_str += "  - [ [C0C0C0]" + kvp.Key + "[-] ]   x  " + kvp.Value.ToString() + "\r\n";
			}
		}
		else
		{
			weapon_str += "    [A0A0A0]" + "No weapon".ToLocalizationString() + "[-]\r\n";
		}

		return weapon_str;
	}

	public string AttrDescString(ItemAsset.ItemObject obj)
	{
		// [VCCase] - Creation attribute desc string
		if (m_Prefab == null || obj == null || obj.instanceId != m_ObjectID)
			return "< No information >".ToLocalizationString();

		string info = "";
		float hp = 0;
		float energy = 0;

		//ItemAsset.Strengthen strengthen = obj.GetCmpt<ItemAsset.Strengthen>();
		ItemAsset.Property property = obj.GetCmpt<ItemAsset.Property>();

		var lifeLimit = obj.GetCmpt<ItemAsset.LifeLimit>();
		if (lifeLimit != null) hp = lifeLimit.floatValue.current;

		ItemAsset.Durability durability = obj.GetCmpt<ItemAsset.Durability>();
		if (durability != null) hp = durability.floatValue.current;

		var energyLimit = obj.GetCmpt<ItemAsset.Energy>();
		if (energyLimit != null) energy = energyLimit.floatValue.current;

		var name = m_IsoData.m_HeadInfo.Name;
		if (string.IsNullOrEmpty(name)) name = PELocalization.GetString(TextID.EmptyName);

		var desc = m_IsoData.m_HeadInfo.Desc;
		if (string.IsNullOrEmpty(desc)) desc = PELocalization.GetString(TextID.EmptyDesc);

		switch (m_Attribute.m_Type)
		{
			case ECreation.Sword:
            case ECreation.SwordLarge:
            case ECreation.SwordDouble:
                {
					info = string.Format(PELocalization.GetString(TextID.SwordTip),
						name,
						property.GetProperty(Pathea.AttribType.Atk),
						PELocalization.GetString(VCUtility.GetSwordAtkSpeedTextID(VCUtility.GetSwordAnimSpeed(m_Attribute.m_Weight))),
	                    Mathf.CeilToInt(hp * PEVCConfig.equipDurabilityShowScale),
	                    Mathf.CeilToInt(durability.valueMax * PEVCConfig.equipDurabilityShowScale),
						obj.GetSellPrice(),
                        desc);
					return info;
                }
			case ECreation.Axe:
				{
					info = string.Format(PELocalization.GetString(TextID.AxeTip),
						name,
						property.GetProperty(Pathea.AttribType.Atk),
						PELocalization.GetString(VCUtility.GetAxeAtkSpeedTextID(VCUtility.GetAxeAnimSpeed(m_Attribute.m_Weight))),
						Mathf.CeilToInt(hp * PEVCConfig.equipDurabilityShowScale),
	                    Mathf.CeilToInt(durability.valueMax * PEVCConfig.equipDurabilityShowScale),
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.Bow:
				{
					info = string.Format(PELocalization.GetString(TextID.BowTip),
						name,
						property.GetProperty(Pathea.AttribType.Atk),
	                    Mathf.CeilToInt(hp * PEVCConfig.equipDurabilityShowScale),
	                    Mathf.CeilToInt(durability.valueMax * PEVCConfig.equipDurabilityShowScale),
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.Shield:
				{
					info = string.Format(PELocalization.GetString(TextID.ShieldTip),
						name,
						property.GetProperty(Pathea.AttribType.Def),
	                    Mathf.CeilToInt(hp * PEVCConfig.equipDurabilityShowScale),
		                Mathf.CeilToInt(durability.valueMax * PEVCConfig.equipDurabilityShowScale),
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.HandGun:
			case ECreation.Rifle:
				{
					info = string.Format(PELocalization.GetString(TextID.GunTip),
						name,
						property.GetProperty(Pathea.AttribType.Atk),
						1f / m_Prefab.GetComponent<PEGun>().m_FireRate,
		                Mathf.CeilToInt(hp * PEVCConfig.equipDurabilityShowScale),
		                Mathf.CeilToInt(durability.valueMax * PEVCConfig.equipDurabilityShowScale),
						obj.GetSellPrice(),
                        desc);
					return info;
				}
			case ECreation.Vehicle:
				{
					info = string.Format(PELocalization.GetString(TextID.VehicleTip),
						name,
						creationController.bounds.size.x,
						creationController.bounds.size.y,
						creationController.bounds.size.z,
                        hp,
						lifeLimit.valueMax,
						energy,
						energyLimit.valueMax,
                        m_Attribute.m_Attack,
                        obj.GetSellPrice(),
                        desc);

					return info;
				}
			case ECreation.Aircraft:
				{
					info = string.Format(PELocalization.GetString(TextID.AircraftTip),
						name,
						creationController.bounds.size.x,
						creationController.bounds.size.y,
						creationController.bounds.size.z,
						hp,
						lifeLimit.valueMax,
						energy,
						energyLimit.valueMax,
                        m_Attribute.m_Attack,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.Boat:
				{
					info = string.Format(PELocalization.GetString(TextID.BoatTip),
						name,
						creationController.bounds.size.x,
						creationController.bounds.size.y,
						creationController.bounds.size.z,
						hp,
						lifeLimit.valueMax,
						energy,
						energyLimit.valueMax,
                        m_Attribute.m_Attack,
						obj.GetSellPrice(),
                        desc);
					return info;
				}
			case ECreation.SimpleObject:
				{
					info = string.Format(PELocalization.GetString(TextID.ObjectTip),
						name,
						creationController.bounds.size.x,
						creationController.bounds.size.y,
						creationController.bounds.size.z,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.ArmorHead:
				{
					info = string.Format(PELocalization.GetString(TextID.ArmorHeadTip),
						name,
						VCUtility.GetArmorDefence(durability.valueMax),
						hp,
						durability.valueMax,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.ArmorBody:
				{
					info = string.Format(PELocalization.GetString(TextID.ArmorBodyTip),
						name,
						VCUtility.GetArmorDefence(durability.valueMax),
						hp,
						durability.valueMax,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.ArmorArmAndLeg:
				{
					info = string.Format(PELocalization.GetString(TextID.ArmorArmAndLegTip),
						name,
						VCUtility.GetArmorDefence(durability.valueMax),
						hp,
						durability.valueMax,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.ArmorHandAndFoot:
				{
					info = string.Format(PELocalization.GetString(TextID.ArmorHandAndFootTip),
						name,
						VCUtility.GetArmorDefence(durability.valueMax),
						hp,
						durability.valueMax,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.ArmorDecoration:
				{
					info = string.Format(PELocalization.GetString(TextID.ArmorDecorationTip),
						name,
						VCUtility.GetArmorDefence(durability.valueMax),
						hp,
						durability.valueMax,
						obj.GetSellPrice(),
						desc);
					return info;
				}
			case ECreation.Robot:
				{
					info = string.Format(PELocalization.GetString(TextID.RobotTip),
						name,
						hp,
						lifeLimit.valueMax,
						energy,
						energyLimit.valueMax,
                        m_Attribute.m_Attack,
						obj.GetSellPrice(),
                        desc);
					return info;
				}
			case ECreation.AITurret:
				{
					info = string.Format(PELocalization.GetString(obj.protoData.unchargeable ? TextID.AmmoTurretTip : TextID.EnergyTurretTip),
						name,
						hp,
						lifeLimit.valueMax,
						energy,
						energyLimit.valueMax,
                        m_Attribute.m_Attack,
						obj.GetSellPrice(),
                        desc);
					return info;
				}
			default:
				return info;
		}
	}
}