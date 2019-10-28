#define PLANET_EXPLORERS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUIStatisticsPanel : MonoBehaviour
{
	public bool m_IsEditor = true;
	public string m_NonEditorRemark = "";
	[HideInInspector] public string m_NonEditorError = "";
	[HideInInspector] public byte[] m_NonEditorIcon = null;
	public string m_NonEditorISOName = "";
	public string m_NonEditorISODesc = "";
    public string m_NonEditorISOVersion = "";

	// ISO INFO
	public UITable m_UITable;
	public BoxCollider m_DragContentCollider;
	public UILabel m_ISOName;
	public UILabel m_ISOCategory;
	public UILabel m_ISOSize;
	public UILabel m_ISOStat;
	public UILabel m_ISODesc;
    public UILabel ISOVersionLabel;  //log:lz-2016.05.13 show iso version
	public UITexture m_ISOIcon64;
	public UITexture m_ISOIcon32;
	public Material m_IconMatRes;
	private Material m_IconMat;
	private Texture2D m_IconTex;

    //lz-2018.01.05 track iso cost
    public UICheckbox ckItemTrack;

    // CREATION INFO
    public UILabel m_AttrNames;
	public UILabel m_AttrValues;
	
	// COST
	public VCEUICostList m_CostList;
	
	public bool m_RefreshNow = false;

	// Use this for initialization
	public void Init ()
	{
		m_IconMat = Material.Instantiate(m_IconMatRes) as Material;
		m_ISOIcon64.material = m_IconMat;
		if ( m_ISOIcon32 != null )
			m_ISOIcon32.material = m_IconMat;
		m_CostList.Init();
    }
	
	void OnDestroy ()
	{
		if ( m_IconMat != null )
		{
			Material.Destroy(m_IconMat);
			m_IconMat = null;
		}
		if ( m_IconTex != null )
		{
			Texture2D.Destroy(m_IconTex);
			m_IconTex = null;
		}
	}
	
	public void SetIsoIcon ()
	{
		if ( m_IconTex != null )
		{
			Texture2D.Destroy(m_IconTex);
			m_IconMat.mainTexture = null;
		}
		byte[] pngdata = m_IsEditor ? VCEditor.s_Scene.m_IsoData.m_HeadInfo.IconTex : m_NonEditorIcon;
		if ( pngdata != null )
		{
			m_IconTex = new Texture2D (2,2);
			m_IconTex.LoadImage(pngdata);
			m_IconMat.mainTexture = m_IconTex;
			m_ISOIcon64.gameObject.SetActive(false);
			m_ISOIcon64.gameObject.SetActive(true);
			if ( m_ISOIcon32 != null )
			{
				m_ISOIcon32.gameObject.SetActive(false);
				m_ISOIcon32.gameObject.SetActive(true);
			}
		}
	}

	void Awake ()
	{
		if ( !m_IsEditor ) Init();
	}

	// Update is called once per frame
	string last_desc = "";
	void Update ()
	{
		if ( m_RefreshNow )
		{
			m_RefreshNow = false;
			OnCreationInfoRefresh();
		}
		if ( m_IsEditor )   // Editor
		{
			VCIsoData iso = VCEditor.s_Scene.m_IsoData;
			if ( iso.m_HeadInfo.Name.Trim().Length > 0 )
			{
				m_ISOName.text = iso.m_HeadInfo.Name;
				m_ISOName.color = new Color (1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISOName.text = "< Untitled >".ToLocalizationString();
				m_ISOName.color = new Color (0.6f, 0.6f, 0.6f, 0.5f);
			}
			if ( iso.m_HeadInfo.Desc.Trim().Length > 0 )
			{
				m_ISODesc.text = iso.m_HeadInfo.Desc + "\r\n";
				m_ISODesc.color = new Color (1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISODesc.text = "< No description >".ToLocalizationString() + "\r\n";
				m_ISODesc.color = new Color (0.6f, 0.6f, 0.6f, 0.5f);
			}
			m_ISOCategory.text = VCConfig.s_Categories[iso.m_HeadInfo.Category].m_Name.ToLocalizationString();
			m_ISOSize.text = "Width".ToLocalizationString() + ": " + iso.m_HeadInfo.xSize.ToString() +
				"\r\n" + "Depth".ToLocalizationString() + ": " + iso.m_HeadInfo.zSize.ToString() +
				"\r\n" + "Height".ToLocalizationString() + ": " + iso.m_HeadInfo.ySize.ToString();
			m_ISOStat.text = iso.m_Components.Count.ToString("#,##0") + " " + "component(s)".ToLocalizationString() + "\r\n" +
				iso.m_Voxels.Count.ToString("#,##0") + " " + "voxel(s)".ToLocalizationString() + "\r\n" +
					iso.MaterialUsedCount().ToString() + " " + "material(s)".ToLocalizationString() + "\r\n" +
					iso.m_Colors.Count.ToString("#,##0") + " " + "color unit(s)".ToLocalizationString();
			if ( m_ISODesc.text != last_desc )
			{
				m_UITable.Reposition();
				last_desc = m_ISODesc.text;
			}
		}
		else    // Non-editor
		{
			if ( m_NonEditorISOName.Trim().Length > 0 )
			{
				m_ISOName.text = m_NonEditorISOName;
				m_ISOName.color = new Color (1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISOName.text = "< Untitled >".ToLocalizationString();
				m_ISOName.color = new Color (0.6f, 0.6f, 0.6f, 0.5f);
			}
			if ( m_NonEditorISODesc.Trim().Length > 0 )
			{
				m_ISODesc.text = m_NonEditorISODesc + "\r\n";
				m_ISODesc.color = new Color (1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISODesc.text = "< No description >".ToLocalizationString() + "\r\n";
				m_ISODesc.color = new Color (0.6f, 0.6f, 0.6f, 0.5f);
			}
			m_ISOCategory.text = "";
			m_ISOSize.text = "";
			m_ISOStat.text = "";
			if ( m_ISODesc.text != last_desc )
			{
				m_UITable.Reposition();
				last_desc = m_ISODesc.text;
			}

            if (null != this.m_NonEditorISOVersion)
            {
                float curVersion = 0f;
                float newVersion = float.Parse(SteamWorkShop.NewVersionTag);
                if (float.TryParse(this.m_NonEditorISOVersion, out curVersion) && curVersion >= newVersion)
                {
					this.ISOVersionLabel.text = "ISO " + "Version".ToLocalizationString() + ": [FF0000]" + this.m_NonEditorISOVersion + "[-]";
                }
                else
                {
					this.ISOVersionLabel.text = "ISO " + "Version".ToLocalizationString() + ": [00FFFF]<" + SteamWorkShop.NewVersionTag + "[-]";
                }
            }
		}
		
		// Drag Content
		Vector3 size = m_DragContentCollider.size;
		Vector3 center = m_DragContentCollider.center;
		size.y = m_UITable.mVariableHeight;
		center.y = - (size.y)/2;
		m_DragContentCollider.size = size;
		m_DragContentCollider.center = center;

		// Lock table pos
		Vector3 lpos = m_UITable.transform.localPosition;
		if ( Mathf.Abs(lpos.x) > 2 )
			lpos.x = 0;
		m_UITable.transform.localPosition = lpos;
	}
	
	public void OnCreationInfoRefresh ()
	{
		CreationAttr attr = null;
		if ( m_IsEditor )
		{
			CreationData.CalcCreationAttr(VCEditor.s_Scene.m_IsoData, 0, ref VCEditor.s_Scene.m_CreationAttr);
			attr = VCEditor.s_Scene.m_CreationAttr;
		}
		else
		{
			VCIsoRemark remark = new VCIsoRemark ();
			remark.xml = m_NonEditorRemark;
			attr = remark.m_Attribute;

			// 0.9 ID改变处理
			if (attr != null)
				attr.CheckCostId();
			m_CostList.m_NonEditorAttr = attr;
			if ( remark.m_Error != null && remark.m_Error.Length > 1 )
				m_NonEditorError = "ISO version is obsolete".ToLocalizationString();
			else
				m_NonEditorError = "";

		}
            
        UpdateItemsTrackState(attr);

		if ( attr != null )
		{
            // 上面是否调用了 CalcCreationAttr ？ 为什么属性会不同？？？

            // [VCCase] - Creation attribute info. UI
            if (attr.m_Type == ECreation.Sword || attr.m_Type == ECreation.SwordLarge || attr.m_Type == ECreation.SwordDouble || attr.m_Type == ECreation.Axe)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					"Weight".ToLocalizationString() + ":\r\n" +
					               //"Volume".ToLocalizationString() + ":\r\n" +
						"Sell Price".ToLocalizationString() + ":\r\n\r\n" +
						"Attack".ToLocalizationString() + ":\r\n" +
						"Durability".ToLocalizationString() + ":\r\n\r\n";

                m_AttrValues.text = (attr.m_Type == ECreation.Sword || attr.m_Type == ECreation.SwordLarge || attr.m_Type == ECreation.SwordDouble? "Sword".ToLocalizationString() + "\r\n" : "Axe".ToLocalizationString() + "\r\n") +
					                VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
					                //VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
									(attr.m_SellPrice).ToString("#,##0") + " " + "Meat".ToLocalizationString() +
									"\r\n\r\n" + (attr.m_Attack).ToString("0.0") + "\r\n" +
									(Mathf.CeilToInt(attr.m_Durability * WhiteCat.PEVCConfig.equipDurabilityShowScale)).ToString("0.0") + "\r\n";
			}

			else if (attr.m_Type == ECreation.Bow)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
									"Weight".ToLocalizationString() + ":\r\n" +
									"Volume".ToLocalizationString() + ":\r\n" +
									"Sell Price".ToLocalizationString() + ":\r\n\r\n" +
									"Attack".ToLocalizationString() + ":\r\n" +
									"Durability".ToLocalizationString() + ":\r\n\r\n";

				m_AttrValues.text = "Bow".ToLocalizationString() + "\r\n" +
									VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
									VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
									(attr.m_SellPrice).ToString("#,##0") +
#if PLANET_EXPLORERS
									" " + "Meat".ToLocalizationString() +
#endif
									"\r\n\r\n" + (attr.m_Attack).ToString("0.0") + "\r\n" +
									(Mathf.CeilToInt(attr.m_Durability * WhiteCat.PEVCConfig.equipDurabilityShowScale)).ToString("0.0") + "\r\n";
			}

			else if ( attr.m_Type == ECreation.Shield )
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
									"Weight".ToLocalizationString() + ":\r\n" +
									"Volume".ToLocalizationString() + ":\r\n" +
									"Sell Price".ToLocalizationString() + ":\r\n\r\n" +
									"Defense".ToLocalizationString() + ":\r\n" +
									"Durability".ToLocalizationString() + ":\r\n\r\n";
				
				m_AttrValues.text = "Shield".ToLocalizationString() + "\r\n" +
					                VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
					                VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
									(attr.m_SellPrice).ToString("#,##0") +
	#if PLANET_EXPLORERS
									" " + "Meat".ToLocalizationString() +
	#endif
									"\r\n\r\n" + (attr.m_Defense).ToString("0.0") + "\r\n" +
									(Mathf.CeilToInt(attr.m_Durability * WhiteCat.PEVCConfig.equipDurabilityShowScale)).ToString("0.0") + "\r\n";
			}
			else if ( attr.m_Type == ECreation.HandGun || attr.m_Type == ECreation.Rifle )
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					               "Weight".ToLocalizationString() + ":\r\n" +
					               "Volume".ToLocalizationString() + ":\r\n" +
					               "Sell Price".ToLocalizationString() + ":\r\n\r\n" +
								   "Increase".ToLocalizationString() + ":\r\n" +
					               "Final Attack".ToLocalizationString() + ":\r\n" +
								   "Firing Rate".ToLocalizationString() + ":\r\n" +
								   "Accuracy".ToLocalizationString() + ":\r\n" +
								   "Durability".ToLocalizationString() + ":\r\n\r\n";
				
				m_AttrValues.text = ((attr.m_Type == ECreation.HandGun) ? ("Hand Gun".ToLocalizationString() + "\r\n") : ("Rifle".ToLocalizationString() + "\r\n")) +
					                VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
					                VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
									(attr.m_SellPrice).ToString("#,##0") +
	#if PLANET_EXPLORERS
									 	" " + "Meat".ToLocalizationString() +
	#endif
									"\r\n\r\n" + ((attr.m_MuzzleAtkInc == 0) ? ("-") : (attr.m_MuzzleAtkInc*100.0f - 100.0f).ToString("0.0")) + " %\r\n" +
									(attr.m_Attack).ToString("0.0") + "\r\n" +
									(attr.m_FireSpeed).ToString("0.0") + "\r\n" +
									(1.0f/attr.m_Accuracy*100).ToString("0.0") + " %\r\n" +
									(Mathf.CeilToInt(attr.m_Durability * WhiteCat.PEVCConfig.equipDurabilityShowScale)).ToString("0.0") + "\r\n";
			}
			else if ( attr.m_Type == ECreation.Vehicle )
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					               "Weight".ToLocalizationString() + ":\r\n" +
					               "Volume".ToLocalizationString() + ":\r\n" +
					               "Sell Price".ToLocalizationString() + ":\r\n\r\n" +
								    "Attack".ToLocalizationString() + ":\r\n" +
					               "Durability".ToLocalizationString() + ":\r\n" +
                                   "Fuel".ToLocalizationString() + ":\r\n\r\n";
				
				m_AttrValues.text = "Vehicle".ToLocalizationString() + "\r\n" +
					                VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
					                VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
									(attr.m_SellPrice).ToString("#,##0") +
	#if PLANET_EXPLORERS
									 	" " + "Meat".ToLocalizationString() +
	#endif
									"\r\n\r\n" +
                                    (attr.m_Attack).ToString("#,##0") + " /s\r\n" +
                                    (attr.m_Durability).ToString("#,##0") + "\r\n" +
                                    (attr.m_MaxFuel).ToString("#,##0") + "\r\n";			
			}
			else if ( attr.m_Type == ECreation.Aircraft )
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					    "Weight".ToLocalizationString() + ":\r\n" +
						"Volume".ToLocalizationString() + ":\r\n" +
						"Sell Price".ToLocalizationString() + ":\r\n\r\n" +
						"Attack".ToLocalizationString() + ":\r\n" +
						"Durability".ToLocalizationString() + ":\r\n" +
                        "Fuel".ToLocalizationString() + ":\r\n\r\n";
				
				m_AttrValues.text = "Aircraft".ToLocalizationString() + "\r\n" +
					    VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
						VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
						(attr.m_SellPrice).ToString("#,##0") +
						#if PLANET_EXPLORERS
						" " + "Meat".ToLocalizationString() +
						#endif
						"\r\n\r\n" + 
                        (attr.m_Attack).ToString("#,##0") + " /s\r\n" +
                        (attr.m_Durability).ToString("#,##0") + "\r\n" +
                        (attr.m_MaxFuel).ToString("#,##0") + "\r\n";			
			}
			else if ( attr.m_Type == ECreation.Boat )
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					    "Weight".ToLocalizationString() + ":\r\n" +
						"Volume".ToLocalizationString() + ":\r\n" +
						"Sell Price".ToLocalizationString() + ":\r\n\r\n" +
						"Attack".ToLocalizationString() + ":\r\n" +
						"Durability".ToLocalizationString() + ":\r\n" +
                        "Fuel".ToLocalizationString() + ":\r\n\r\n";
				
				m_AttrValues.text = "Boat".ToLocalizationString() + "\r\n" +
					    VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
						VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
						(attr.m_SellPrice).ToString("#,##0") +
						#if PLANET_EXPLORERS
						" " + "Meat".ToLocalizationString() +
						#endif
						"\r\n\r\n" + 
                        (attr.m_Attack).ToString("#,##0") + " /s\r\n" +
                        (attr.m_Durability).ToString("#,##0") + "\r\n" +
                        (attr.m_MaxFuel).ToString("#,##0") + "\r\n";			
			}
			else if ( attr.m_Type == ECreation.SimpleObject )
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					               "Weight".ToLocalizationString() + ":\r\n" +
						           "Volume".ToLocalizationString() + ":\r\n" +
						           "Sell Price".ToLocalizationString() + ":\r\n\r\n" +
						           "Durability".ToLocalizationString() + ":\r\n\r\n";
				
				m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" +
					    VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
						VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
						(attr.m_SellPrice).ToString("#,##0") +
						#if PLANET_EXPLORERS
						" " + "Meat".ToLocalizationString() +
						#endif
						"\r\n\r\n" + (attr.m_Durability).ToString("#,##0") + "\r\n";			
			}
			else if (attr.m_Type == ECreation.Robot)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
				   "Weight".ToLocalizationString() + ":\r\n" +
				   "Volume".ToLocalizationString() + ":\r\n" +
				   "Sell Price".ToLocalizationString() + ":\r\n\r\n" +
						"Attack".ToLocalizationString() + ":\r\n" +
				   "Durability".ToLocalizationString() + ":\r\n\r\n";

                m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" +
						VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
						VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
						(attr.m_SellPrice).ToString("#,##0") +
#if PLANET_EXPLORERS
						" " + "Meat".ToLocalizationString() +
#endif
						"\r\n\r\n" + 
                        (attr.m_Attack).ToString("#,##0") + " /s\r\n" +
                        (attr.m_Durability).ToString("#,##0") + "\r\n";
            }
			else if (attr.m_Type == ECreation.AITurret)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
				   "Weight".ToLocalizationString() + ":\r\n" +
				   "Volume".ToLocalizationString() + ":\r\n" +
				   "Sell Price".ToLocalizationString() + ":\r\n\r\n" +
					"Attack".ToLocalizationString() + ":\r\n" +
				   "Durability".ToLocalizationString() + ":\r\n\r\n";

                m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" +
						VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
						VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
						(attr.m_SellPrice).ToString("#,##0") +
#if PLANET_EXPLORERS
						" " + "Meat".ToLocalizationString() +
#endif
						"\r\n\r\n" +
                        (attr.m_Attack).ToString("#,##0") + " /s\r\n" +
                        (attr.m_Durability).ToString("#,##0") + "\r\n";
            }
			else if (attr.m_Type == ECreation.ArmorHead
				|| attr.m_Type == ECreation.ArmorBody
				|| attr.m_Type == ECreation.ArmorArmAndLeg
				|| attr.m_Type == ECreation.ArmorHandAndFoot
				|| attr.m_Type == ECreation.ArmorDecoration)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
				   "Weight".ToLocalizationString() + ":\r\n" +
				   "Volume".ToLocalizationString() + ":\r\n" +
				   "Sell Price".ToLocalizationString() + ":\r\n\r\n" +
					"Defense".ToLocalizationString() + ":\r\n" +
				   "Durability".ToLocalizationString() + ":\r\n\r\n";

                m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" +
						VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
						VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
						(attr.m_SellPrice).ToString("#,##0") +
#if PLANET_EXPLORERS
						" " + "Meat".ToLocalizationString() +
#endif
						"\r\n\r\n" +
                        (attr.m_Defense).ToString("#,##0") + "\r\n" +
                        (attr.m_Durability).ToString("#,##0") + "\r\n";
            }
			else
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" +
					               "Weight".ToLocalizationString() + ":\r\n" +
					               "Volume".ToLocalizationString() + ":\r\n" +
					               "Sell Price".ToLocalizationString() + ":\r\n";
				
				m_AttrValues.text = "[FF0000]???[-]\r\n" +
					                VCUtils.WeightToString(attr.m_Weight) + "\r\n" +
					                VCUtils.VolumeToString(attr.m_Volume) + "\r\n" +
									(attr.m_SellPrice).ToString("#,##0") +
	#if PLANET_EXPLORERS
									 	" " + "Meat".ToLocalizationString() +
	#endif
									"\r\n";			
			}
			
			if ( attr.m_Errors.Count > 0 )
			{
				m_AttrNames.text += "\r\n[FF0000]" + "Errors".ToLocalizationString() + ":\r\n";
				foreach ( string e in attr.m_Errors )
				{
					m_AttrNames.text += ("> " + e.ToLocalizationString() + "\r\n");
				}
				m_AttrNames.text += "[-]";
			}
			if ( attr.m_Warnings.Count > 0 )
			{
				m_AttrNames.text += "\r\n[FFFF00]" + "Warnings".ToLocalizationString() + ":\r\n";
				foreach ( string w in attr.m_Warnings )
				{
					m_AttrNames.text += ("> " + w.ToLocalizationString() + "\r\n");
				}
				m_AttrNames.text += "[-]";
			}
			m_CostList.RefreshCostList();
			m_UITable.Reposition();
		}
		else    // attr == null
		{
			m_AttrNames.text = "[FF0000]" + m_NonEditorError + "[-]";
			m_AttrValues.text = "";
			m_CostList.RefreshCostList();
			m_UITable.Reposition();
		}

		// Status
		if ( m_IsEditor )
		{
			if ( VCEditor.s_Scene.m_CreationAttr.m_Errors.Count > 0 )
				VCEStatusBar.ShowText("Your creation has some errors".ToLocalizationString(), Color.red, 10);
			else if ( VCEditor.s_Scene.m_CreationAttr.m_Warnings.Count > 0 )
				VCEStatusBar.ShowText("Your creation has some warnings".ToLocalizationString(), Color.yellow, 10);

			if ( attr.m_Weight > 0.0001f )
			{
				VCEditor.Instance.m_MassCenterTrans.localPosition = attr.m_CenterOfMass;
				VCEditor.Instance.m_MassCenterTrans.gameObject.SetActive(true);
			}
			else
			{
				VCEditor.Instance.m_MassCenterTrans.gameObject.SetActive(false);
			}

			VCIsoRemark remark = new VCIsoRemark ();
			remark.m_Attribute = attr;
			VCEditor.s_Scene.m_IsoData.m_HeadInfo.Remarks = remark.xml;
		}
	}
	
	public void OnCostRefresh ()
	{
		OnCreationInfoRefresh();
	}

    void OnDisable ()
	{
		if ( m_IsEditor )
		{
			VCEditor.Instance.m_MassCenterTrans.gameObject.SetActive(false);
		}
	}

    #region Item Track

    CreationAttr _curAttr;
    void UpdateItemsTrackState(CreationAttr attr)
    {
        if (ckItemTrack)
        {
            _curAttr = attr;
            string isoName = VCEditor.s_Scene.m_IsoData.m_HeadInfo.Name;
            bool show = !string.IsNullOrEmpty(isoName) && _curAttr != null && GameUI.Instance && GameUI.Instance.mItemsTrackWnd;
            ckItemTrack.gameObject.SetActive(show);
            if(show) ckItemTrack.isChecked = GameUI.Instance.mItemsTrackWnd.ContainsIso(isoName);
        }
    }

    void OnItemTrackCk(bool isChecked)
    {
        if (ckItemTrack && _curAttr != null && GameUI.Instance && GameUI.Instance.mItemsTrackWnd)
        {
            string isoName = VCEditor.s_Scene.m_IsoData.m_HeadInfo.Name;
            if (!string.IsNullOrEmpty(isoName))
            {
                if (isChecked)
                {
                    GameUI.Instance.mItemsTrackWnd.AddIso(isoName, _curAttr.m_Cost);
                }
                else
                {
                    GameUI.Instance.mItemsTrackWnd.RemoveIso(isoName);
                }
            }
        }
    }

    #endregion
}
