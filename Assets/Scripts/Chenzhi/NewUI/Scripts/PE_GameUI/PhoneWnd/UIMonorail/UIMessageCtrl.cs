using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mono.Data.SqliteClient;
using System.Linq;

//lz-2016.07.12 每一条Message的数据结构,以及对应MessagesData数据库表的数据映射，以及用户数据的存储和读取
public class MessageData
{
    private int m_ID;
    private int[] m_MissionIDs;
    private string m_Topic; 
    private string m_Form; 
    private string m_To;
    private string m_Title;
    private string m_Content;
    private string m_End;
    private string m_Date;

    private const int m_MsgVersion = 1607160;  //lz-2016.07.16 增加版本号，避免更改存储数据结构读取报错

    public int ID { get { return m_ID; } }
    public int[] MissionIDs { get { return m_MissionIDs; } }
    public string Topic { get { return m_Topic; } }
    public string Form { get { return m_Form; } }
    public string To { get { return m_To; } }
    public string Title { get { return m_Title; } }
    public string Content { get { return m_Content; } }
    public string End { get { return m_End; } }
    public string Date { get { return m_Date; } }

    //static field
    public static Dictionary<int, MessageData> AllMsgDataDic = new Dictionary<int, MessageData>(); // key=MsgID,value=Data
    public static Dictionary<int, int[]> AllMissionIDDic = new Dictionary<int, int[]>();  //key=MsgID,value=MissionIDs
    public static List<int> ActiveMsgDataIDs = new List<int>();  //ActiveMsgID
    public static Action<int> AddMsgEvent; //MsgID

    public MessageData(int id,int[] missionIDs,string topic,string form,string to,string date,string title,string content,string end)
    {
        this.m_ID = id;
        this.m_MissionIDs = missionIDs;
        this.m_Topic = topic;
        this.m_Form = form;
        this.m_To = to;
        this.m_Title = title;
        this.m_Content = content;
        this.m_End = end;
        this.m_Date = date;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MessagesData");
        MessageData info = null;
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            string missionIDsStr = reader.GetString(reader.GetOrdinal("MissionID"));
            int topicID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Topic")));
            string fromAddress = reader.GetString(reader.GetOrdinal("FromAddress"));
            string toAddress = reader.GetString(reader.GetOrdinal("ToAddress"));
            int dateID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Date")));
            int titleID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Title")));
            int contentID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Content")));
            int endID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("End")));
            string[] strMissionIDs = missionIDsStr.Split(new char[] { ',' });
            int[] missionIDs = new int[strMissionIDs.Length];
            for (int i = 0; i < strMissionIDs.Length; i++)
                missionIDs[i] = Convert.ToInt32(strMissionIDs[i]);
            info = new MessageData(id,
                    missionIDs,
                    PELocalization.GetString(topicID),
                    fromAddress,
                    toAddress,
                    PELocalization.GetString(dateID),
                    PELocalization.GetString(titleID),
                    PELocalization.GetString(contentID),
                    PELocalization.GetString(endID));
            AllMsgDataDic.Add(id,info);
            AllMissionIDDic.Add(id, missionIDs);
        }
    }

    public static void AddMsgByCompletedMissionID(int missionID)
    {
        if (null == AllMsgDataDic || AllMsgDataDic.Count <= 0)
            return;

        Dictionary<int,int[]> allMsgByMissionID = AllMissionIDDic.Where(a => a.Value.Contains(missionID)).ToDictionary(k => k.Key, v => v.Value);
        foreach (KeyValuePair<int,int[]> kv in allMsgByMissionID)
        {
            if (!ActiveMsgDataIDs.Contains(kv.Key))
            {
                if (kv.Value.Length == kv.Value.Count(id => MissionManager.Instance.HadCompleteMissionAnyNum(id)))
                {
                    ActiveMsgDataIDs.Add(kv.Key);
                    if (null != AddMsgEvent)
                        AddMsgEvent(kv.Key);
                }
            }
        }
    }

    public static bool Deserialize(byte[] data)
    {
        ActiveMsgDataIDs.Clear();
        try
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(data,false);
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
            {
                if (reader.ReadInt32() != m_MsgVersion)
                {
                    Debug.LogWarning("MessageData version valid! ");
                    return false;
                }
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    ActiveMsgDataIDs.Add(reader.ReadInt32());
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("MessageData deserialize error: " + e);
            return false;
        }
    }

    public static byte[] Serialize()
    {
        try
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(200);
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
            {
                writer.Write(m_MsgVersion);
                writer.Write(ActiveMsgDataIDs.Count);
                for (int i = 0; i < ActiveMsgDataIDs.Count; i++)
                    writer.Write(ActiveMsgDataIDs[i]);
            }
            return stream.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }

    public static void Clear()
    {
        ActiveMsgDataIDs.Clear();
    }
}

//lz-2016.06.12 这个脚本是Phone界面新加的Message页面的控制脚本
public class UIMessageCtrl : UIBaseWidget
{
    private Queue<UIListItem> m_MsgListItemPool = new Queue<UIListItem>();
    private List<UIListItem> m_CurListItems = new List<UIListItem>();


    [SerializeField]
    private UIListItem m_ListItemPrefab;
    [SerializeField]
    private UIGrid m_ListGrid;
    [SerializeField]
    private UIScrollBar m_ListScrollBar;
    [SerializeField]
    private UILabel m_FormLabel;
    [SerializeField]
    private UILabel m_ToLabel;
    [SerializeField]
    private UILabel m_TopicLabel;
    [SerializeField]
    private UILabel m_TitleLabel;
    [SerializeField]
    private UILabel m_ContentLabel;
    [SerializeField]
    private UILabel m_EndLabel;
    [SerializeField]
    private UITable m_ContentTable;
    [SerializeField]
    private UIScrollBar m_ContentScrollBar;
    [SerializeField]
    private UILabel m_DateLabel;
    [SerializeField]
    private UIInput m_InfoInput;
    [SerializeField]
    private N_ImageButton m_SeedBtn;
    private UIListItem m_BackupSelectItem;

    //lz-2016.06.12 发送信息实现这个事件
    public Action<string> SeedMsgEvent;

    #region mono methods
    void Awake()
    {
        UIEventListener.Get(this.m_SeedBtn.gameObject).onClick+=(go)=>this.SeedBtnEvent();
    }

    #endregion

    #region override methods

    public override void OnCreate()
    {
        this.m_MsgListItemPool.Clear();
        this.m_CurListItems.Clear();
        MessageData.AddMsgEvent += this.AddMsgByData;
    }

    protected override void InitWindow()
    {
        base.InitWindow();
    }

    public override void Show()
    {
        base.Show();
        this.ResetWnd();
        this.UpdateMsgList();
    }

    protected override void OnHide()
    {
        base.OnHide();
        this.RecycleMsgListItem();
    }

    protected override void OnClose()
    {
        base.OnClose();
        this.RecycleMsgListItem();
    }
   
    #endregion

    #region private methods

    private void AddMsgByData(int msgID)
    {
        if (null!=this&&this.gameObject.activeInHierarchy)
        {
            this.AddNewListItem(msgID);
            this.m_ListGrid.repositionNow = true;
        }
    }

    private void UpdateMsgList()
    {
        if (null == MessageData.ActiveMsgDataIDs || MessageData.ActiveMsgDataIDs.Count <= 0)
            return;
        for (int i = 0; i < MessageData.ActiveMsgDataIDs.Count; i++)
		{
            if (!this.m_CurListItems.Any(a => a.ID == MessageData.ActiveMsgDataIDs[i]))
            {
                this.AddNewListItem(MessageData.ActiveMsgDataIDs[i]);
            }
		}
        this.m_ListGrid.repositionNow = true;

        //lz-2016.07.23 选中第一个
        if (null != this.m_CurListItems && this.m_CurListItems.Count > 0)
        {
            this.m_CurListItems[0].Select();
        }
    }

    private void AddNewListItem(int msgID)
    {
        UIListItem item = this.GetNewMsgListItem();
        item.UpdateInfo(msgID, MessageData.AllMsgDataDic[msgID].Topic);
        item.SelectEvent = this.MegListItemSelectEvent;
        this.m_CurListItems.Add(item);
    }

    private void MegListItemSelectEvent(UIListItem item)
    {
        if (item == this.m_BackupSelectItem)
            return;
        if (null != this.m_BackupSelectItem)
        {
            this.m_BackupSelectItem.CancelSelect();
        }
        this.m_BackupSelectItem = item;

        if (MessageData.ActiveMsgDataIDs.Contains(item.ID))
        {
            MessageData info = MessageData.AllMsgDataDic[item.ID];
            this.UpdateForm(info.Form);
            this.UpdateTo(info.To);
            this.UpdateTopic(info.Topic);
            this.UpdateContent(info.Title, info.Content,info.End);
            this.UpdateDate(info.Date);
        }
        else
        {
            this.ResetAllLabel();
        }
    }

    private void ResetAllLabel()
    {
        this.UpdateForm("");
        this.UpdateTo("");
        this.UpdateTopic("");
        this.UpdateContent("", "", "");
        this.UpdateDate("");
    }

    private void UpdateForm(string from)
    {
        this.m_FormLabel.text = from;
    }

    private void UpdateTo(string to)
    {
        this.m_ToLabel.text = to;
    }

    private void UpdateTopic(string topic)
    {
        this.m_TopicLabel.text = topic;
    }

    private void UpdateContent(string title,string content,string end)
    {
        this.UpdateContentTitle(title);
        if (content == "0" || string.IsNullOrEmpty(content))
        {
            this.m_ContentLabel.gameObject.SetActive(false);
        }
        else
        {
            this.m_ContentLabel.gameObject.SetActive(true);
            content = content.Replace("<\\n>", "\n");
            this.m_ContentLabel.text = content;
        }
        this.UpdateContentEnd(end);
        this.m_ContentTable.repositionNow = true;
        this.m_ContentScrollBar.scrollValue = 0;
    }

    private void UpdateContentTitle(string title)
    {
        if (title == "0"||string.IsNullOrEmpty(title))
        {
            this.m_TitleLabel.gameObject.SetActive(false);
            return;
        }
        this.m_TitleLabel.gameObject.SetActive(true);
        title = title.Replace("<\\n>", "\n");
        this.m_TitleLabel.text = title;
    }

    private void UpdateContentEnd(string end)
    {
        if (end == "0" || string.IsNullOrEmpty(end))
        {
            this.m_EndLabel.gameObject.SetActive(false);
            return;
        }

        this.m_EndLabel.gameObject.SetActive(true);
        if (end.Contains("<\\right>"))
        {
            this.m_EndLabel.pivot=UIWidget.Pivot.TopRight;
            this.m_EndLabel.transform.localPosition=new Vector3(550,0,0);
            end=end.Replace("<\\right>", "");
                
        }
        else //默认居左边 
        {
            this.m_EndLabel.pivot=UIWidget.Pivot.TopLeft;
            this.m_EndLabel.transform.localPosition=new Vector3(0,0,0);
            if(end.Contains("<\\left>"))
                end=end.Replace("<\\left>", "");
        }
        end = end.Replace("<\\n>", "\n");
        this.m_EndLabel.text = end;
    }

    private void UpdateDate(string date)
    {
        this.m_DateLabel.text = date;
    }

    private UIListItem GetNewMsgListItem()
    {
        UIListItem item=null;
        if (this.m_MsgListItemPool.Count > 0)
        {
            item = this.m_MsgListItemPool.Dequeue();
            item.ResetItem();
            item.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = GameObject.Instantiate(this.m_ListItemPrefab.gameObject) as GameObject;
            go.transform.parent = this.m_ListGrid.gameObject.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(0, 0, 0);
            item = go.GetComponent<UIListItem>();
        }
        return item;
    }

    private void ResetWnd()
    {
        this.m_ListScrollBar.scrollValue = 0;
        this.m_ContentScrollBar.scrollValue = 0;
        this.ResetAllLabel();
    }

    private void RecycleMsgListItem()
    {
        if (null == this.m_CurListItems || this.m_CurListItems.Count <= 0)
            return;
        for (int i = 0; i < this.m_CurListItems.Count; i++)
		{
            this.m_CurListItems[i].gameObject.SetActive(false);
            this.m_MsgListItemPool.Enqueue(this.m_CurListItems[i]);
		}
        this.m_CurListItems.Clear();
    }

    private void SeedBtnEvent()
    {
        string msg=this.m_InfoInput.text;
        msg=msg.Trim();
        if (string.IsNullOrEmpty(msg))
            return;

        if (null != SeedMsgEvent)
        {
            SeedMsgEvent(msg);
        }

        //lz-2016.12.06 开启音乐播放器命令，发布版测试用
        if (msg == string.Format("OpenRadio{0}{1}",System.DateTime.Now.Month, System.DateTime.Now.Day))
        {
            if (null != GameUI.Instance && null != GameUI.Instance.mPhoneWnd)
            {
                GameUI.Instance.mPhoneWnd.OpenRadio = true;
            }
        }
        this.m_InfoInput.text = "";
    }

    #endregion

    #region test methods

    //lz-2016.06.12 测试数据，测试结束删除
    private void TestInfo()
    {
        if (Application.isEditor)
        {
            MessageData.AllMsgDataDic.Add(1000,new MessageData(1000, new int[] { 501 }, "TestTopic0", "star", "sun", "2016.06.12", "This is a title", "This is a Content", "<\\right>This is a end!<\\n> This is a second line"));
            MessageData.AllMsgDataDic.Add(1001,new MessageData(1001, new int[] { 502 }, "TestTopic1", "star", "sun", "2016.06.12", "This is a title", "This is a Content", "This is a end! <\\n>Is a Two Lines"));
            MessageData.ActiveMsgDataIDs.Add(1000);
            MessageData.ActiveMsgDataIDs.Add(1001);
        }
    }
    #endregion
}
