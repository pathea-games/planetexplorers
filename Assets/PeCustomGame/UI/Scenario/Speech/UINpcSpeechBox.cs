using UnityEngine;
using System.Collections.Generic;
using System;

public class UINpcSpeechBox : UIBaseWidget
{
    [SerializeField] UILabel    contentLabel;
    [SerializeField] UITable    chooseTable;

    [SerializeField] UILabel    npcNameLabel;   
    [SerializeField] UISprite   npcHeadSprite;
    [SerializeField] UITexture  npcHeadTex;

    [SerializeField] UINpcQuestItem questItemPrefab;

    private List<UINpcQuestItem> m_QuestItems = new List<UINpcQuestItem>();

    public bool IsChoice { get { return !contentLabel.gameObject.activeSelf;} }

    // Event
    public Action<UINpcQuestItem> onSetItemContent;
    public Action<UINpcQuestItem> onQuestItemClick;
    public Action onUIClick;

    public void SetContent (string content)
    {
        if (!contentLabel.gameObject.activeSelf)
            contentLabel.gameObject.SetActive(true);

        contentLabel.text = content;

        if (chooseTable.gameObject.activeSelf)
            chooseTable.gameObject.SetActive(false);
    }

    public void SetContent(int choose_num)
    {
        int count = choose_num < 0 ? 0 : choose_num;

        if (!chooseTable.gameObject.activeSelf)
            chooseTable.gameObject.SetActive(true);

        if (contentLabel.gameObject.activeSelf)
            contentLabel.gameObject.SetActive(false);

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

        chooseTable.repositionNow = true;
    }

    public void SetNpcInfo(string _name, string _icon)
    {
        npcNameLabel.text = _name;
        npcHeadTex.enabled = false;
        npcHeadSprite.enabled = true;
        npcHeadSprite.spriteName = _icon;
    }

    public void SetNpcInfo(string _name, Texture _icon)
    {
        npcNameLabel.text = _name;
        npcHeadTex.enabled = true;
        npcHeadSprite.enabled = false;
        npcHeadTex.mainTexture = _icon;
    }

    UINpcQuestItem CreateQuestItem()
    {
        UINpcQuestItem item = Instantiate<UINpcQuestItem>(questItemPrefab);
        Transform trans = item.transform;
        item.transform.parent = chooseTable.transform;
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
        trans.gameObject.SetActive(true);

        return item;
    }

    void OnQuestItemClick(UINpcQuestItem item)
    {
        if (onQuestItemClick != null)
            onQuestItemClick(item);
    }

    public void OnUIClick()
    {
        if (onUIClick != null)
            onUIClick ();
    }

    #region UIBASEWidget
    public override void Show()
    {
        base.Show();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnClose()
    {
        //base.OnClose();
        if (onUIClick != null)
            onUIClick();
    }
    #endregion
}

