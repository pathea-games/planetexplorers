using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomChat_N:MonoBehaviour
{
    public CommonChatItem_N m_ChatItemPrefab;
    public UITable ChatListTable;
    public UIPanel ClipPanel;
    public UIScrollBar ChatScrollBar;
    public UIInput ChatInput;
    public UIButton SendBtn;
    public BoxCollider DragContentCollider;
    public Action<string> SendMsgEvent;
    public int LineWidth = 584;

    private int mMaxMsgCount = 300;
    private int mDeleteMsgCount = 100;
    private List<CommonChatItem_N> m_CurChatItems = new List<CommonChatItem_N>();
    private Queue<CommonChatItem_N> m_ChatItemsPool = new Queue<CommonChatItem_N>();
    private int m_RepositionCount = 0;

    public const string LANGE_CN = "[lang:cn]";
    public const string LANGE_OTHER = "[lang:other]";


    #region mon methods

    void OnEnable()
    {
        UIEventListener.Get(this.SendBtn.gameObject).onClick = (go) => this.SeedMsg();
    }

    void OnDisable()
    {
        RecoveryItems(m_CurChatItems.Count);
    }

    void Update()
    {
        if(m_RepositionCount>0)
        { 
            ChatListTable.Reposition();
            ChatScrollBar.scrollValue = 1;
            --m_RepositionCount;
        }
    }

    #endregion

    #region public methods
    public void AddMsg(string userName, string strMsg, string strColor)
    {
        //lz-2016.12.12 去除语言标记符号
        bool isChinese = strMsg.Contains(LANGE_CN);
        strMsg = strMsg.Replace(isChinese ? LANGE_CN : LANGE_OTHER, string.Empty);

        string text = "[" + strColor + "]" + userName + "[-]" + ":" + strMsg;

        CommonChatItem_N item = GetNewChatItem();
        if (null != item)
        {
            item.SetLineWidth(LineWidth);
            item.UpdateText(isChinese, text);
            m_CurChatItems.Add(item);
        }

        if (m_CurChatItems.Count > mMaxMsgCount)
        {
            RecoveryItems(mDeleteMsgCount);
        }

        m_RepositionCount = 3;
    }
    #endregion

    #region private methods

    void GetInputFocus()
    {
        ChatInput.selected = true;
    }

    private void SeedMsg()
    {
        if (null != this.SendMsgEvent)
        {
            string msg = this.ChatInput.text;
            this.ChatInput.text = "";
            msg = msg.Trim();
            if (string.IsNullOrEmpty(msg))
                return;
            //lz-2016.12.12 发消息的时候加入语言识别，方便显示处理
            msg += SystemSettingData.Instance.IsChinese ? UITalkBoxCtrl.LANGE_CN : UITalkBoxCtrl.LANGE_OTHER;
            this.SendMsgEvent(msg);
            Invoke("GetInputFocus", 0.1f);
        }
    }

    CommonChatItem_N GetNewChatItem()
    {
        CommonChatItem_N item;
        if (m_ChatItemsPool.Count > 0)
        {
            item = m_ChatItemsPool.Dequeue();
            item.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = Instantiate(m_ChatItemPrefab.gameObject);
            go.transform.parent = ChatListTable.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            item = go.GetComponent<CommonChatItem_N>();
        }
        return item;
    }

    void RecoveryItems(int count)
    {
        if (null != m_CurChatItems && m_CurChatItems.Count >= count)
        {
            CommonChatItem_N recoveryItem;
            for (int i = 0; i < count; i++)
            {
                recoveryItem = m_CurChatItems[i];
                recoveryItem.ResetItem();
                recoveryItem.gameObject.SetActive(false);
                m_ChatItemsPool.Enqueue(recoveryItem);
            }
            m_CurChatItems.RemoveRange(0, count);
        }
    }
    #endregion
}
