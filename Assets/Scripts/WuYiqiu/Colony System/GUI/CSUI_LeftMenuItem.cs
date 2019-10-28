using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_LeftMenuItem : MonoBehaviour
{
    [HideInInspector]
    public List<CSEntity> m_EntityList = new List<CSEntity>();
    [HideInInspector]
    public int m_Type;

    private bool m_IsSelected=false;
    public bool IsSelected
    {
        get
        {
            return m_IsSelected;
        }
        set
        {
            m_IsSelected = value;
            Select(m_IsSelected);
        }
    }

    public UISprite mSpDisabled;
    public UISprite mSpDumped;

    public UISlicedSprite mBakgroundSp;
    public UISprite mForGroundSp;
    public BoxCollider mBoxCollider;

    string bgSprNameWhite;
    string bgSprNameRed;
    string fgSprNameWhite;
    string fgSprNameRed;

    private bool m_NotHaveElectricity = false;
    [HideInInspector]
    public bool NotHaveElectricity { get { if (!m_InitState)this.initInfo(); return m_NotHaveElectricity; } } // 没电

    private bool m_NotHaveAssembly = false;
    [HideInInspector]
    public bool NotHaveAssembly { get { if (!m_InitState)this.initInfo(); return m_NotHaveAssembly; } } // 没核心

    private bool m_AssemblyLevelInsufficient = false;
    [HideInInspector]
    public bool AssemblyLevelInsufficient { get { if (!m_InitState)this.initInfo(); return m_AssemblyLevelInsufficient; } } // 核心等级不足

    //log: lz-2016.05.26 避免第一次拿TagIsDisabled状态的时候，状态没有更新过
    private bool m_InitState = false;
    private int m_NotHaveAssemblyCount;
    private int m_AssemblyLevelInsufficientCount;
    private int m_NotHaveElectricityCount;


    #region mono methods
    void Start()
    {
        this.initInfo();
    }

    void Update()
    {
        UpdateState();
    }
    #endregion

    #region public methods
    public bool IsShow
    {
        get { return gameObject.activeSelf; }
    }

    //lz-2016.06.08 获取没有电的所有名字
    public List<string> GetNamesByNotHaveElectricity()
    {
        List<string> names = new List<string>();
        for (int i = 0; i < m_EntityList.Count; i++)
        {
            CSCommon common = m_EntityList[i] as CSCommon;
            //lz-2016.08.21 有核心没有运行说明没有电
            if(common!=null&&common.Assembly!=null&&!m_EntityList[i].IsRunning)
                names.Add(CSUtils.GetEntityName(m_EntityList[i].m_Type));
        }
        return names;
    }

    //lz-2016.06.08 获取核心等级不足的所有名字
    public List<string> GetNamesByAssemblyLevelInsufficient()
    {
        List<string> names = new List<string>();
        for (int i = 0; i < m_EntityList.Count; i++)
        {
            CSCommon common = m_EntityList[i] as CSCommon;
            //lz-2016.08.21 在核心范围内，但是没有核心，说明核心等级不足
            if (common != null && common.Assembly == null
                &&null != CSUI_MainWndCtrl.Instance.Creator && null != CSUI_MainWndCtrl.Instance.Creator.Assembly && CSUI_MainWndCtrl.Instance.Creator.Assembly.InRange(m_EntityList[i].Position))
                names.Add(CSUtils.GetEntityName(m_EntityList[i].m_Type));
        }
        return names;
    }

    //lz-2016.06.08 获取没有核心的所有名字
    public List<string> GetNamesByNotHaveAssembly()
    {
        List<string> names = new List<string>();
        for (int i = 0; i < m_EntityList.Count; i++)
        {
            CSCommon common = m_EntityList[i] as CSCommon;
            if (common != null&&common.Assembly == null)
                names.Add(CSUtils.GetEntityName(m_EntityList[i].m_Type));
        }
        return names;
    }

    #endregion

    #region private methods

    void initInfo()
    {
        if (!m_InitState)
        {
            UIEventListener.Get(mBoxCollider.gameObject).onClick = OnClickEvent;
            mSpDisabled.enabled = false;
            mSpDumped.enabled = false;

            bgSprNameWhite = mBakgroundSp.spriteName;
            bgSprNameRed = bgSprNameWhite + "_red";
            fgSprNameWhite = mForGroundSp.spriteName;
            fgSprNameRed = fgSprNameWhite + "_red";
            this.UpdateState();
            m_InitState = true;
        }
    }

    void OnClickEvent(GameObject go)
    {
        if (!IsSelected)
        {
            IsSelected = true;
        }
    }

    void Select(bool isSelected)
    {
        SelectSprite(isSelected);
        if (isSelected)
        {
            CSUI_MainWndCtrl.Instance.ShowWndPart(this, m_Type);
        }
        else
        {
            CSUI_MainWndCtrl.Instance.HideWndByType(m_Type);
        }
    }

    public void SelectSprite(bool isSelect)
    {
        mForGroundSp.gameObject.SetActive(isSelect);
    }

    void UpdateState()
    {
        m_NotHaveAssemblyCount = 0;
        m_AssemblyLevelInsufficientCount = 0;
        m_NotHaveElectricityCount = 0;
        for (int i = 0; i < m_EntityList.Count;i++ )
        {
            CSCommon common = m_EntityList[i] as CSCommon;
            if (common != null && common.Assembly == null)
            {
                m_NotHaveAssemblyCount++;
                //lz-2016.06.08 蒲及告诉我如果没有核心，但是在核心的范围内，就说明核心等级不足
                if (null != CSUI_MainWndCtrl.Instance.Creator && null != CSUI_MainWndCtrl.Instance.Creator.Assembly && CSUI_MainWndCtrl.Instance.Creator.Assembly.InRange(m_EntityList[i].Position))
                    m_AssemblyLevelInsufficientCount++;
            }
            if (common != null && common.Assembly!=null&&!m_EntityList[i].IsRunning)
                m_NotHaveElectricityCount++;
        }
        m_NotHaveAssembly = m_NotHaveAssemblyCount > 0;
        m_AssemblyLevelInsufficient = m_AssemblyLevelInsufficientCount > 0;
        m_NotHaveElectricity = m_NotHaveElectricityCount > 0;

        mSpDisabled.enabled = m_NotHaveElectricity;
        mSpDumped.enabled = m_NotHaveAssembly;

        mForGroundSp.spriteName = m_NotHaveElectricity ? fgSprNameRed : fgSprNameWhite;
        mBakgroundSp.spriteName = m_NotHaveElectricity ? bgSprNameRed : bgSprNameWhite;
    }
    #endregion
}
