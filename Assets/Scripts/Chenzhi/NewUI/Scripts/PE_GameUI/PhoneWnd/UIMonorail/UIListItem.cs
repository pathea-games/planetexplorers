using UnityEngine;
using System.Collections;
using System;

public class UIListItem : MonoBehaviour
{
    [SerializeField]
    private UILabel m_TopicLabel;
    [SerializeField]
    private GameObject m_SelectGo;
    [SerializeField]
    private GameObject m_HoverGo;
    [SerializeField]
    private BoxCollider m_Collider;
    [SerializeField]
    private Color32 m_ErrorCol = new Color32(246,13,13,255);
    private int m_ID;
    private bool m_Selected;
    private UIPanel m_CurPanel;
    

    public int ID { get { return this.m_ID; } }
    public Action<UIListItem> SelectEvent;
    

    #region mono methods

    void Awake()
    {
        this.ResetItem();
        UIEventListener.Get(this.gameObject).onClick += (go) => this.Select();
        UIEventListener.Get(this.gameObject).onHover += (go, isHover) => this.Hover(isHover);
    }

    void Update()
    {
        if (null != this.m_CurPanel)
        {
            this.m_Collider.enabled = this.m_CurPanel.IsVisible(this.transform.position);
        }
    }

    #endregion

    #region private methods

    private void UpdateTotpic(string topic)
    {
        this.m_TopicLabel.text = topic;
    }

    private void Hover(bool isHover)
    {
        if (this.m_Selected)
            this.m_HoverGo.SetActive(false);
        else
           this.m_HoverGo.SetActive(isHover);
    }

    private void GetCompt()
    {
         UIDraggablePanel draggablePanel = NGUITools.FindInParents<UIDraggablePanel>(this.gameObject);
        if (null != draggablePanel)
            this.m_CurPanel = draggablePanel.GetComponent<UIPanel>();
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
        this.m_ID = msgID;
        this.UpdateTotpic(topic);
        this.GetCompt();
    }

    public void UpdateInfo(int msgID, string topic,bool isPlayError)
    {
        this.m_ID = msgID;
        this.UpdateTotpic(topic);
        SetIsPlayError(isPlayError);
        this.GetCompt();
    }

    public void CancelSelect()
    {
        this.m_SelectGo.SetActive(false);
        this.m_Selected = false;
    }

    public void Select()
    {
        this.m_SelectGo.SetActive(true);
        this.Hover(false);
        if (null != SelectEvent)
        {
            this.SelectEvent(this);
        }
        this.m_Selected = true;
    }

    public void SetIsPlayError(bool isPlayError)
    {
        m_TopicLabel.color = isPlayError ?  m_ErrorCol: new Color32(255,255,255,255);
    }
    #endregion
}
