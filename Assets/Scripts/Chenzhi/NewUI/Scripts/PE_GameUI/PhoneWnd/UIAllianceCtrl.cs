using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathea;

public class UIAllianceCtrl : UIBaseWidget
{
    [SerializeField]
    private UIGrid m_Grid;
    [SerializeField]
    private AllianceItem_N m_AllianceItemPrefab;

    private Queue<AllianceItem_N> m_ItemPools = new Queue<AllianceItem_N>();
    private Dictionary<int,AllianceItem_N> m_CurItemDic = new Dictionary<int,AllianceItem_N>(); //key=playerID,value=item
    private int m_MainPlayerID;

    #region override methods

    protected override void InitWindow()
    {
        base.InitWindow();
    }

    public override void Show()
    {
        base.Show();
        UpdateAllianceInfo();
        VArtifactUtil.RegistTownDestryedEvent(UpdateBuildCountEvent);
        ReputationSystem.Instance.onReputationChange += UpdateReputationEvent;
    }

    protected override void OnHide()
    {
        base.OnHide();
        RecoveryItem();
        VArtifactUtil.UnRegistTownDestryedEvent(UpdateBuildCountEvent);
        ReputationSystem.Instance.onReputationChange -= UpdateReputationEvent;
    }

    #endregion

    #region private methods

    /// <summary>通过阵营建筑改变事件更新建筑数量 </summary>
    private void UpdateBuildCountEvent(int allyID)
    {
        int playerID= VATownGenerator.Instance.GetPlayerId(allyID);
        if (m_CurItemDic.ContainsKey(playerID))
        {
            m_CurItemDic[playerID].UpdateBuildCount();
        }
    }

    /// <summary>通过声望改变事件更新声望值 </summary>
    private void UpdateReputationEvent(int forceID, int targetPlayerID)
    {
        float mForceID=ForceSetting.Instance.GetForceID(m_MainPlayerID);
        if (mForceID == forceID&& m_CurItemDic.ContainsKey(targetPlayerID))
        {
            m_CurItemDic[targetPlayerID].UpdateReputation();
        }
    }

    private AllianceItem_N GetNewItem()
    {
        AllianceItem_N item = null;
        if (m_ItemPools.Count > 0)
            item = m_ItemPools.Dequeue();
        else
        {
            GameObject go = Instantiate(m_AllianceItemPrefab.gameObject);
            go.transform.parent = m_Grid.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            item= go.GetComponent<AllianceItem_N>();
        }
        item.gameObject.SetActive(true);
        return item;
    }


    private void UpdateAllianceInfo()
    {
        m_MainPlayerID = (int)PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID);
        int allianceCount = RandomMapConfig.allyCount;
        //lz-2016.09.22 0是玩家的阵营不显示
        for (int i = 1; i < allianceCount; i++)
        {
            AllianceItem_N item=GetNewItem();
            int playerID =VATownGenerator.Instance.GetPlayerId(i);
            item.UpdateInfo(i, playerID, m_MainPlayerID);
            m_CurItemDic.Add(playerID, item);
        }
        m_Grid.Reposition();
    }

    private void RecoveryItem()
    {
        if (m_CurItemDic.Count > 0)
        {
            AllianceItem_N[] itemArray = m_CurItemDic.Values.ToArray();
            for (int i = 0; i < itemArray.Length; i++)
            {
                itemArray[i].Reset();
                itemArray[i].gameObject.SetActive(false);
                m_ItemPools.Enqueue(itemArray[i]);
            }
            m_CurItemDic.Clear();
        }
    }

    #endregion
}
