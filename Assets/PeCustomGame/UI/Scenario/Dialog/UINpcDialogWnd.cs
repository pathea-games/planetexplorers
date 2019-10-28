using UnityEngine;
using System.Collections.Generic;
using System;

public class UINpcDialogWnd : UIBaseWnd
{
    [SerializeField] UILabel npcNameLabel;
    [SerializeField] UISprite npcHeadSprite;
    [SerializeField] UITexture npcHeadTex;

    [SerializeField] UITable  questTable; 
    [SerializeField] UINpcQuestItem questItemPrefab;
    [SerializeField] Transform defaultQuest;

    private List<UINpcQuestItem> m_QuestItems = new List<UINpcQuestItem>();

    public Action<UINpcQuestItem> onSetItemContent;
    public Action<UINpcQuestItem> onQuestItemClick;
    public Action onBeforeShow;

    public void UpdateTable (int count)
    {
        count = count < 0 ? 0 : count;

        if (count > m_QuestItems.Count)
        {
            for (int i = 0; i < m_QuestItems.Count; i++)
            {
                m_QuestItems[i].index = i;
                if (onSetItemContent != null)
                {
                    onSetItemContent(m_QuestItems[i]);
                }
            }

            int cnt = count;
            for (int i = m_QuestItems.Count; i < cnt; i++)
            {
                UINpcQuestItem item = CreateQuestItem();
                item.index = i;
                m_QuestItems.Add(item);
                item.onClick += OnQuestItemClick;

                if (onSetItemContent != null)
                {
                    onSetItemContent(m_QuestItems[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                m_QuestItems[i].index = i;
                if (onSetItemContent != null)
                {
                    onSetItemContent(m_QuestItems[i]);
                }
            }

            for (int i = m_QuestItems.Count - 1; i >= count; i--)
            {
                m_QuestItems[i].onClick -= OnQuestItemClick;
                Destroy(m_QuestItems[i].gameObject);
                m_QuestItems[i].transform.parent = null;
                m_QuestItems.RemoveAt(i);
            }
        }

        defaultQuest.transform.SetAsLastSibling();

        questTable.repositionNow = true;
    }

    public void SetNPCInfo (string _name, string _icon)
    {
        npcNameLabel.text = _name;
        npcHeadTex.enabled = false;
        npcHeadSprite.enabled = true;
        npcHeadSprite.spriteName = _icon;
    }

    public void SetNPCInfo (string _name, Texture _icon)
    {
        npcNameLabel.text = _name;
        npcHeadTex.enabled = true;
        npcHeadSprite.enabled = false;
        npcHeadTex.mainTexture = _icon;
    }

    UINpcQuestItem CreateQuestItem ()
    {
        UINpcQuestItem item = Instantiate<UINpcQuestItem>(questItemPrefab);
        Transform trans = item.transform;
        item.transform.parent = questTable.transform;
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
        trans.gameObject.SetActive(true);

        return item;
    }

    void OnQuestItemClick (UINpcQuestItem item)
    {
        if (onQuestItemClick != null)
            onQuestItemClick(item);
    }

    void OnDefaultQuestClick()
    {
        Hide();
    }

    #region UIBASEWND
    public override void Show()
    {
        if (onBeforeShow != null)
            onBeforeShow();

        base.Show();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnClose()
    {
        base.OnClose();
    }
    #endregion
}
