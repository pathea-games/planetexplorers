using UnityEngine;
using System.Collections;
using System;

public class UIMsgListItem : MonoBehaviour
{
    [SerializeField]
    private UILabel m_TopicLabel;
    [SerializeField]
    private GameObject m_SelectGo;
    [SerializeField]
    private GameObject m_HoverGo;
    private int m_MsgID;
    private bool m_Selected;

    public int MsgID { get { return this.m_MsgID; } }
    public Action<UIMsgListItem> SelectEvent;
    

    #region mono methods

    void Awake()
    {
        this.ResetItem();
        UIEventListener.Get(this.gameObject).onClick += (go) => this.Select();
        UIEventListener.Get(this.gameObject).onHover += (go, isHover) => this.Hover(isHover);
    }

    #endregion

    #region private methods

    private void UpdateTotpic(string topic)
    {
        this.m_TopicLabel.text = topic;
    }

    private void Select()
    {
        this.m_SelectGo.SetActive(true);
        this.Hover(false);
        if (null != SelectEvent)
        {
            this.SelectEvent(this);
        }
        this.m_Selected = true;
    }

    private void Hover(bool isHover)
    {
        if (this.m_Selected)
            this.m_HoverGo.SetActive(false);
        else
           this.m_HoverGo.SetActive(isHover);
    }
    
    #endregion

    #region public methods

    public void ResetItem()
    {
        this.CancelSelect();
        this.Hover(false);
        this.m_TopicLabel.text = "";
    }

    public void UpdateInfo(int msgID, string topic)
    {
        this.m_MsgID = msgID;
        this.UpdateTotpic(topic);
    }

    public void CancelSelect()
    {
        this.m_SelectGo.SetActive(false);
        this.m_Selected = false;
    }

    #endregion
}
