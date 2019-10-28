using UnityEngine;
using System.Collections;
using Pathea;
using System.Collections.Generic;

public class CSUI_TrainNpcInfCtrl : MonoBehaviour
{



    [SerializeField]
    GameObject m_InfoPage;//学员信息节点
    [SerializeField]
    GameObject m_InventoryPage;//学员存货节点


    private CSPersonnel m_Npc;
    public CSPersonnel Npc
    {
        get { return m_Npc; }
        set
        {
            //lz-2016.10.14 避免上一个m_Npc身上的事件没有移除
            if (null != m_Npc&& null!=m_Npc.m_Npc)
            {
                AbnormalConditionCmpt accOld = m_Npc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
                if (accOld != null)
                {
                    accOld.evtStart -= AddNpcAbnormal;
                    accOld.evtEnd -= RemoveNpcAbnormal;
                }
            }

            m_Npc = value;

            //lz-2016.10.14 重新添加事件
            if (null != m_Npc && null != m_Npc.m_Npc)
            {
                AbnormalConditionCmpt accNew = m_Npc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
                if (accNew != null)
                {
                    accNew.evtStart += AddNpcAbnormal;
                    accNew.evtEnd += RemoveNpcAbnormal;
                }
            }

            RefreshNpcAbnormal();
        }
    }

    private void PageInfoOnActive(bool active)
    {
        m_InfoPage.SetActive(active);
    }
    private void PageInvetoryOnActive(bool active)
    {
        m_InventoryPage.SetActive(active);
    }

    void Update()
    {
        GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.UpdateTraineeSkillsShow(m_Npc);
        GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.Reflashpackage();
        if (m_Npc == null)
        {
            GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetSprSex("null");
        }
        else
        {
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetServantInfo(m_Npc.FullName, m_Npc.Sex, (int)m_Npc.GetAttribute(AttribType.Hp),
                (int)m_Npc.GetAttribute(AttribType.HpMax), (int)m_Npc.GetAttribute(AttribType.Stamina), (int)m_Npc.GetAttribute(AttribType.StaminaMax),
                (int)m_Npc.GetAttribute(AttribType.Hunger), (int)m_Npc.GetAttribute(AttribType.HungerMax), (int)m_Npc.GetAttribute(AttribType.Comfort),
                (int)m_Npc.GetAttribute(AttribType.ComfortMax), (int)m_Npc.GetAttribute(AttribType.Oxygen), (int)m_Npc.GetAttribute(AttribType.OxygenMax),
                (int)m_Npc.GetAttribute(AttribType.Shield), (int)m_Npc.GetAttribute(AttribType.ShieldMax), (int)m_Npc.GetAttribute(AttribType.Energy),
                (int)m_Npc.GetAttribute(AttribType.EnergyMax), (int)m_Npc.GetAttribute(AttribType.Atk), (int)m_Npc.GetAttribute(AttribType.Def));
        }
    }

    #region Abnormal

    [SerializeField]
    UIGrid mAbnormalGrid;
    [SerializeField]
    CSUI_BuffItem mAbnormalPrefab;

    bool mReposition = false;

    private List<CSUI_BuffItem> mAbnormalList = new List<CSUI_BuffItem>(1);

    void RefreshNpcAbnormal()
    {
        RemoveAllAbnormal();
        if (m_Npc == null)
        {
            return;
        }

        List<PEAbnormalType> abList = m_Npc.m_Npc.Alnormal.GetActiveAbnormalList();

        if (abList.Count == 0)
            return;

        for (int i = 0; i < abList.Count; i++)
        {
            AddNpcAbnormal(abList[i]);
        }
    }

    void AddNpcAbnormal(PEAbnormalType type)
    {
        AbnormalData data = AbnormalData.GetData(type);
        //lz-2016.08.26 异常状态是0的图标不显示
        if (null == data || data.iconName == "0")
            return;
        CSUI_BuffItem item = Instantiate(mAbnormalPrefab) as CSUI_BuffItem;
        if (!item.gameObject.activeSelf)
            item.gameObject.SetActive(true);
        item.transform.parent = mAbnormalGrid.transform;
        CSUtils.ResetLoacalTransform(item.transform);
        item.SetInfo(data.iconName, data.description);
        mAbnormalList.Add(item);
        mReposition = true;
    }

    void RemoveNpcAbnormal(PEAbnormalType type)
    {
        AbnormalData data = AbnormalData.GetData(type);
        //lz-2016.08.26 异常状态是0的图标不显示
        if (null == data || data.iconName == "0")
            return;
        CSUI_BuffItem item = mAbnormalList.Find(i => i._icon == data.iconName);
        if (item == null)
            return;
        Destroy(item.gameObject);
        mAbnormalList.Remove(item);
        mReposition = true;
    }

    void RemoveAllAbnormal()
    {
        if (mAbnormalList.Count == 0)
            return;
        for (int i = 0; i < mAbnormalList.Count; i++)
        {
            Destroy(mAbnormalList[i].gameObject);
            mAbnormalList.Remove(mAbnormalList[i]);
        }
    }

    void UpdateReposition()
    {
        if (mReposition)
        {
            mReposition = false;
            mAbnormalGrid.repositionNow = true;
        }
    }

    void LateUpdate()
    {
        UpdateReposition();
    }

    #endregion
}
