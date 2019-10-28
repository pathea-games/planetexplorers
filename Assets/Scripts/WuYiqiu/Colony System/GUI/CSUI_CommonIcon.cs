using UnityEngine;
using System.Collections;

public class CSUI_CommonIcon : MonoBehaviour 
{
	public delegate void  OnClickItemEvent(CSEntity entity);
	public event OnClickItemEvent e_OnClickIco; 

	public UISprite mSpState;
	public UISprite mIcoSpr;
	public UICheckbox mCheckBox;
    public ShowToolTipItem_N mShowToolTip;

    private CSCommon m_Common;
    [HideInInspector]
    public CSCommon Common { get { return m_Common; } set { m_Common = value; if (null != m_Common) { IcoType = m_Common.m_Type; } } }
	
	private int m_Type=0;
    private int m_TipID = 0;
	private int IcoType
	{
		get{ return m_Type;}
		set{
			m_Type = value;
            
			switch (m_Type)
			{
			    case CSConst.dtppCoal:
				    mIcoSpr.spriteName = "building_powerplant_coal";
                    m_TipID = 82210007;
                    break;
			    case CSConst.dtStorage:
				    mIcoSpr.spriteName = "element_building_020201";
                    m_TipID = 82210002;
                    break;
                case CSConst.dtppFusion:
                    mIcoSpr.spriteName = "fusion_plant";
                    m_TipID = 82210017;
                    break;
                case CSConst.dtEnhance:
                    mIcoSpr.spriteName = "element_building_030301";
                    m_TipID = 82210017;
                    break;
                case CSConst.dtRepair:
                    mIcoSpr.spriteName = "element_building_030101";
                    m_TipID = 82210017;  
                    break;
                case CSConst.dtRecyle:
                    mIcoSpr.spriteName = "element_building_030201";
                    m_TipID = 82210017;
                    break;
                default:
                    m_TipID = 0;
                    break;
            }
            if (null != mShowToolTip && m_TipID != 0)
            {
                mShowToolTip.mStrID = m_TipID;
            }
		}
	}

	void OnClickIco()
	{
		if (e_OnClickIco != null && Common != null)
			e_OnClickIco (Common as CSEntity);
	}

	void Start()
	{
		mSpState.enabled =false;
		mCheckBox.radioButtonRoot = transform.parent;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Common == null)
			return;
		if (Common.Assembly == null)
		{
			mSpState.color = new Color(1,1,1,0.3f);
			mSpState.enabled = true;
		}
		else if (!Common.IsRunning)
		{
			mSpState.color = new Color(1,0.3f,0.3f,0.3f);
			mSpState.enabled = true;
		}
		else
			mSpState.enabled = false;
	}
}
