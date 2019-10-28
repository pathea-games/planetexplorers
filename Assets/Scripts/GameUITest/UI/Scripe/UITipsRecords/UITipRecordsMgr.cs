using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITipRecordsMgr : UIBaseWnd
{

    [SerializeField]
    GameObject msgPrefab;
    [SerializeField]
    UITable mTable;
    [SerializeField]
    GoPool itemGoPool;
    [SerializeField]
    UIScrollBar mScrollBar;

    //bool mRePos = false;

    public int mTipRecordsCountMax = 50;//提示记录最大数量

    List<GameObject> m_WaitList = new List<GameObject>();

    public void AddMsg(PeTipMsg peTipMsg)
    {
        GameObject go = CreateGo();
        UITipMsg uiTipMsg = go.GetComponent<UITipMsg>();
        uiTipMsg.content.text = peTipMsg.GetContent();
        uiTipMsg.content.color = peTipMsg.GetColor();
        uiTipMsg.musicID = peTipMsg.GetMusicID();
        switch (peTipMsg.GetEStyle())
        {
            case PeTipMsg.EStyle.Text:
                uiTipMsg.tex.mainTexture = null;
                uiTipMsg.icon.spriteName = "";
                break;
            case PeTipMsg.EStyle.Icon:
                uiTipMsg.icon.spriteName = peTipMsg.GetIconName();
                uiTipMsg.tex.mainTexture = null;
                break;
            case PeTipMsg.EStyle.Texture:
                uiTipMsg.icon.spriteName = "";
                uiTipMsg.tex.mainTexture = peTipMsg.GetIconTex();
                break;
        }
        uiTipMsg.SetStyle(peTipMsg.GetEStyle());
        uiTipMsg.gameObject.SetActive(true);

        m_WaitList.Add(go);
        CheckTipsCount();
        Reposition();
        if (m_WaitList.Count > 7 && mScrollBar != null)
            mScrollBar.scrollValue = 1f;

    }

    public override void Show()
    {
        base.Show();
        Reposition();
    }

    private void Reposition()
    {
        mTable.Reposition();
    }

    GameObject CreateGo()
    {
        if (itemGoPool != null)
        {
            return itemGoPool.GetGo(mTable.transform, false);
        }

        GameObject go = Instantiate(msgPrefab) as GameObject;
        go.transform.parent = mTable.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    void DestroyGo(GameObject go)
    {
        if (go)
        {
            if (itemGoPool != null)
            {
                itemGoPool.GiveBackGo(go, true);
                return;
            }
            GameObject.Destroy(go);
        }
    }

    void CheckTipsCount()
    {
        if (m_WaitList.Count <= mTipRecordsCountMax)
            return;

        DestroyGo(m_WaitList[0]);
        m_WaitList.RemoveAt(0);

    }

    void OnClearBtn()
    {
        for (int i = 0; i < m_WaitList.Count; i++)
        {
            DestroyGo(m_WaitList[i]);
        }
        m_WaitList.Clear();
        if (mScrollBar != null)
            mScrollBar.scrollValue = 0f;
    }

    public override void ChangeWindowShowState()
    {
        base.ChangeWindowShowState();

        if (mTable != null)
            mTable.Reposition();
        if (m_WaitList.Count > 7 && mScrollBar != null)
            mScrollBar.scrollValue = 1f;
    }

}
