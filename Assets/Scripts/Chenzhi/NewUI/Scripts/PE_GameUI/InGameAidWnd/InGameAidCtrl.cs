using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;
using System.Linq;
using Pathea;

public class InGameAidData
{
    public enum InGameAidType
    {
        None = 0,
        JoinMission,         //接任务
        CompleteTask,        //完成任务
        PutOnEquip,          //穿装备
        OpenUI,              //打开UI
        ClickBtn,            //点击按钮Button
        InBuff,              //获得异常状态
        UseItem,             //使用物品
        ShowInGameAid,       //显示引导
        GetItem,             //获得某物品
        GetServant,          //招募仆从
        PlayTalk,            //播放对话
    }

    public int ID { get; private set; }
    public InGameAidType StartType { get; private set; }
    public List<int> StartValue { get; private set; }
    public InGameAidType ShowType { get; private set; }
    public List<int> ShowValue { get; private set; }
    public InGameAidType ComplateType { get; private set; }
    public List<int> ComplateValue { get; private set; }
    public int CountentID { get; private set; }

    public static Dictionary<int, InGameAidData> AllData = new Dictionary<int, InGameAidData>();    //key=id,value=data
    public static Dictionary<InGameAidType, List<int>> AllStartTypeMap = new Dictionary<InGameAidType, List<int>>();   //key=Type,value=list<id>
    public static Dictionary<InGameAidType, List<int>> AllShowTypeMap = new Dictionary<InGameAidType, List<int>>();   //key=Type,value=list<id>
    public static Dictionary<InGameAidType, List<int>> AllCompleteTypeMap = new Dictionary<InGameAidType, List<int>>();   //key=Type,value=list<id>

    #region needArchive field
    public const int Version = 1608210;  //lz-2016.08.21 版本号，避免更改存储数据结构读取报错
    public static bool ShowInGameAidCtrl = true; //lz-2016.10.10 记录引导的位置，（显示在屏幕中，还是隐藏在屏幕外面，并不是开始和禁用的意思，开启和禁用由系统设置决定）
    public static List<int> CurCheckShowIDs = new List<int>(); //需要进行显示检测的ID
    public static List<int> CurShowIDs = new List<int>(); //需要进行显示的ID
    public static List<int> CompleteIDs = new List<int>(); //完成的引导ID
    #endregion

    public static event Action<int> AddEvent;


    public InGameAidData(int id, InGameAidType startType, List<int> startValue, InGameAidType showType, List<int> showValue, InGameAidType complateType, List<int> complateValue, int contentID)
    {
        this.ID = id;
        this.StartType = startType;
        this.StartValue = startValue;
        this.ShowType = showType;
        this.ShowValue = showValue;
        this.ComplateType = complateType;
        this.ComplateValue = complateValue;
        this.CountentID = contentID;
    }


    #region private methods
    private static void AddCompleteID(int completeID)
    {
        if (null == AllData || AllData.Count <= 0)
            return;

        if (!CompleteIDs.Contains(completeID))
        {
            CompleteIDs.Add(completeID);
            if (CurCheckShowIDs.Contains(completeID))
                CurCheckShowIDs.Remove(completeID);
        }
    }

    private static void AddCurCheckShowList(int id)
    {
        if (!CurCheckShowIDs.Contains(id))
        {
            CurCheckShowIDs.Add(id);
        }
    }

    private static void AddShowID(int id)
    {
        if (!CurShowIDs.Contains(id)&& !CompleteIDs.Contains(id))
        {
            CurCheckShowIDs.Remove(id);
            CurShowIDs.Add(id);
            if (null != AddEvent)
                AddEvent(id);

            //lz-2016.08.22 检测显示引导
            CheckShowInGameAid(id);

            //lz-2016.08.22 如果没有完成条件，加入到显示列表就完成了
            if (AllCompleteTypeMap.ContainsKey(InGameAidType.None) && AllCompleteTypeMap[InGameAidType.None].Contains(id))
            {
                AddCompleteID(id);
            }
        }
    }

    private static void LoadAllMap()
    {
        AllStartTypeMap.Clear();
        AllShowTypeMap.Clear();
        AllCompleteTypeMap.Clear();
        InGameAidData info = null;
        foreach (var kv in AllData)
        {
            info = kv.Value;
            if (!AllStartTypeMap.ContainsKey(info.StartType))
                AllStartTypeMap.Add(info.StartType, new List<int>());
            AllStartTypeMap[info.StartType].Add(info.ID);

            if (info.ShowType != InGameAidType.None)
            {
                if (!AllShowTypeMap.ContainsKey(info.ShowType))
                    AllShowTypeMap.Add(info.ShowType, new List<int>());
                AllShowTypeMap[info.ShowType].Add(info.ID);
            }

            if (!AllCompleteTypeMap.ContainsKey(info.ComplateType))
                AllCompleteTypeMap.Add(info.ComplateType, new List<int>());
            AllCompleteTypeMap[info.ComplateType].Add(info.ID);
        }
        LoadNoneStartType();
    }

    public static void RemoveCompleteIDs()
    {
        if (null == CompleteIDs || CompleteIDs.Count <= 0)
            return;

        int id;
        foreach (InGameAidType type in Enum.GetValues(typeof(InGameAidType)))
        {
            for (int i = 0; i < CompleteIDs.Count; i++)
            {
                id = CompleteIDs[i];
                if (AllStartTypeMap.ContainsKey(type) && !AllStartTypeMap[type].Contains(id))
                {
                    AllStartTypeMap[type].Add(id);
                }
                if (AllShowTypeMap.ContainsKey(type) && !AllShowTypeMap[type].Contains(id))
                {
                    AllShowTypeMap[type].Add(id);
                }
                if (AllCompleteTypeMap.ContainsKey(type) && !AllCompleteTypeMap[type].Contains(id))
                {
                    AllCompleteTypeMap[type].Add(id);
                }
            }
        }
    }
    private static void LoadNoneStartType()
    {
        //如果存在没有开始条件的引导，就直接加入显示检测
        if (AllStartTypeMap.ContainsKey(InGameAidType.None) && AllStartTypeMap[InGameAidType.None].Count > 0)
        {
            List<int> notNeedStartCheck = AllStartTypeMap[InGameAidType.None];
            for (int i = 0; i < notNeedStartCheck.Count; i++)
            {
                int id = notNeedStartCheck[i];
                if (!CompleteIDs.Contains(id) && !CurCheckShowIDs.Contains(id))
                {
                    CurCheckShowIDs.Add(id);
                }
            }
            AllStartTypeMap.Remove(InGameAidType.None);
        }
    }
    #endregion

    #region archivemgr methods
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("IngameAid");
        InGameAidData info = null;
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            int startType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("S_Type")));
            string startValueStr = reader.GetString(reader.GetOrdinal("S_Value"));
            int showType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
            string showValueStr = reader.GetString(reader.GetOrdinal("Value"));
            int completeType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("C_Type")));
            string completeValueStr = reader.GetString(reader.GetOrdinal("C_Value"));
            int contentID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Content")));

            string[] startValueStrArray = startValueStr.Split(',');
            int[] startValueArray = new int[startValueStrArray.Length];
            for (int i = 0; i < startValueArray.Length; i++)
            {
                int value = 0;
                if (int.TryParse(startValueStrArray[i], out value))
                {
                    startValueArray[i] = value;
                }
                else
                {
                    Debug.LogError("Field [S_Value] has errory! and ID is " + id + " by table [IngameAid]");
                    return;
                }
            }


            string[] showValueStrArray = showValueStr.Split(',');

            int[] showValueArray = new int[showValueStrArray.Length];
            for (int i = 0; i < showValueArray.Length; i++)
            {
                int value = 0;
                if (int.TryParse(showValueStrArray[i], out value))
                {
                    showValueArray[i] = value;
                }
                else
                {
                    Debug.LogError("Field [Value] has errory! and ID is " + id + " by table [IngameAid]");
                    return;
                }
            }

            string[] completeValueStrArray = completeValueStr.Split(',');
            int[] completeValueArray = new int[completeValueStrArray.Length];
            for (int i = 0; i < completeValueArray.Length; i++)
            {
                int value = 0;
                if (int.TryParse(completeValueStrArray[i], out value))
                {
                    completeValueArray[i] = value;
                }
                else
                {
                    Debug.LogError("Field [C_Value] has errory! and ID is " + id + " by table [IngameAid]");
                    return;
                }
            }
            info = new InGameAidData(id, (InGameAidType)startType, startValueArray.ToList(), (InGameAidType)showType, showValueArray.ToList(), (InGameAidType)completeType, completeValueArray.ToList(), contentID);
            AllData.Add(id, info);
        }
    }
    public static bool Deserialize(byte[] data)
    {
        Clear();
        try
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(data, false);
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
            {
                int version = reader.ReadInt32();
                if (version != Version && PeGameMgr.IsSingle)
                {
                    Debug.LogWarning(string.Format("InGameAidData version valid! CurVersion:{0} ErrorVersion:{1}", Version, version));
                    return false;
                }

                ShowInGameAidCtrl = reader.ReadBoolean();

                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    CurCheckShowIDs.Add(reader.ReadInt32());

                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    CurShowIDs.Add(reader.ReadInt32());

                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    CompleteIDs.Add(reader.ReadInt32());
            }
            LoadAllMap();
            RemoveCompleteIDs();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("InGameAidData deserialize error: " + e);
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
                writer.Write(Version);

                writer.Write(ShowInGameAidCtrl);

                writer.Write(CurCheckShowIDs.Count);
                for (int i = 0; i < CurCheckShowIDs.Count; i++)
                    writer.Write(CurCheckShowIDs[i]);

                writer.Write(CurShowIDs.Count);
                for (int i = 0; i < CurShowIDs.Count; i++)
                    writer.Write(CurShowIDs[i]);

                writer.Write(CompleteIDs.Count);
                for (int i = 0; i < CompleteIDs.Count; i++)
                    writer.Write(CompleteIDs[i]);
            }
            return stream.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogWarning("InGameAidData serialize error: " + e);
            return null;
        }
    }

    public static void Clear()
    {
        CurCheckShowIDs.Clear();
        CurShowIDs.Clear();
        CompleteIDs.Clear();
        LoadAllMap();
    }

    
    #endregion

    #region Check methods

    /// <summary>
    /// 检测接任务
    /// </summary>
    /// <param name="missionID"></param>
    public static void CheckJoinMission(int missionID){ CheckValue( InGameAidType.JoinMission,missionID); }

    /// <summary>
    /// 检测完成任务
    /// </summary>
    /// <param name="missionID"></param>
    public static void CheckCompleteTask(int missionID) { CheckValue(InGameAidType.CompleteTask, missionID); }

    /// <summary>
    /// 检测穿装备
    /// </summary>
    /// <param name="equipID"></param>
    public static void CheckPutOnEquip(int equipID) { CheckValue(InGameAidType.PutOnEquip, equipID); }

    /// <summary>
    /// 检测打开UI
    /// </summary>
    /// <param name="type"></param>
    public static void CheckOpenUI(UIEnum.WndType type) { CheckValue(InGameAidType.OpenUI, (int)type); }

    /// <summary>
    /// 检测点击按钮Button
    /// </summary>
    /// <param name="btnID"></param>
    public static void CheckClickBtn(int btnID) { CheckValue(InGameAidType.ClickBtn, btnID); }

    /// <summary>
    /// 检测获得buff
    /// </summary>
    /// <param name="buffID"></param>
    public static void CheckInBuff(int buffID) { CheckValue(InGameAidType.InBuff, buffID); }

    /// <summary>
    /// 检测使用物品
    /// </summary>
    /// <param name="itemID"></param>
    public static void CheckUseItem(int itemID) { CheckValue(InGameAidType.UseItem, itemID); }

    /// <summary>
    /// 检测显示引导
    /// </summary>
    /// <param name="igaID"></param>
    public static void CheckShowInGameAid(int igaID) { CheckValue(InGameAidType.ShowInGameAid, igaID); }

    /// <summary>
    /// 检测背包中有某物品
    /// </summary>
    /// <param name="itemID"></param>
    public static void CheckGetItem(int itemID) { CheckValue(InGameAidType.GetItem, itemID); }

    /// <summary>
    /// 检测招募仆从
    /// </summary>
    /// <param name="servantID"></param>
    public static void CheckGetServant(int servantID) { CheckValue(InGameAidType.GetServant, servantID); }


    /// <summary>
    /// 检测NPC对话
    /// </summary>
    /// <param name="talkID"></param>
    public static void CheckNpcTalk(int talkID) { CheckValue(InGameAidType.PlayTalk, talkID); }

    public static void CheckValue(InGameAidType type, int value)
    {
        if (PeGameMgr.sceneMode != PeGameMgr.ESceneMode.Story)
            return;

        //先检测完成，如果完成满足，就不用检测开始地图、检测显示地图、当前检测显示列表中移除
        if (AllCompleteTypeMap.ContainsKey(type))
        {
            List<int> checkCompIDs = AllCompleteTypeMap[type];
            if (null == checkCompIDs || checkCompIDs.Count <= 0)
            {
                AllCompleteTypeMap.Remove(type);
            }
            else
            {
                List<int> checkCompValueIds = new List<int>();
                for (int i = 0; i < checkCompIDs.Count; i++)
                {
                    int id = checkCompIDs[i];
                    checkCompValueIds = AllData[id].ComplateValue;
                    //lz-2016.08.22 如果只有一个值是-1,表示任意id都可以
                    if ((checkCompValueIds.Count==1&& checkCompValueIds[0]==-1)||checkCompValueIds.Contains(value))
                    {
                        AddCompleteID(id);
                        checkCompIDs.RemoveAt(i);
                        i--;
                        if (AllStartTypeMap.ContainsKey(type) && AllStartTypeMap[type].Contains(id))
                            AllStartTypeMap[type].Remove(id);
                        if (AllShowTypeMap.ContainsKey(type) && AllShowTypeMap[type].Contains(id))
                            AllShowTypeMap[type].Remove(id);
                    }
                }
            }
        }

        //检测开始，如果开始满足加入到显示检测列表，并从全部的开始检测地图中移除
        if (AllStartTypeMap.ContainsKey(type))
        {
            List<int> checkStartIDs = AllStartTypeMap[type];
            if (null == checkStartIDs || checkStartIDs.Count <= 0)
            {
                AllStartTypeMap.Remove(type);
            }
            else
            {
                List<int> checkStartValuses = new List<int>();
                for (int i = 0; i < checkStartIDs.Count; i++)
                {
                    int id = checkStartIDs[i];
                    checkStartValuses = AllData[id].StartValue;
                    //lz-2016.08.22 如果只有一个值是-1,表示任意id都可以
                    if ((checkStartValuses.Count == 1 && checkStartValuses[0] == -1)||checkStartValuses.Contains(value))
                    {
                        AddCurCheckShowList(id);
                        checkStartIDs.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        //检测显示，如果显示满足,就添加到显示队列，并从检测显示列表和全部的检测显示地图中移除
        if (AllShowTypeMap.ContainsKey(type))
        {
            List<int> checkShowIDs = AllShowTypeMap[type];
            if (null == checkShowIDs || checkShowIDs.Count <= 0)
            {
                AllShowTypeMap.Remove(type);
            }
            else
            {
                List<int> checkShowValues = new List<int>();
                for (int i = 0; i < checkShowIDs.Count; i++)
                {
                    int id = checkShowIDs[i];
                    checkShowValues = AllData[id].ShowValue;
                    //lz-2016.08.22 如果只有一个值是-1,表示任意id都可以
                    if ((checkShowValues.Count==1&& checkShowValues[0]==-1|| checkShowValues.Contains(value)))
                    {
                        if (CurCheckShowIDs.Contains(id))
                        {
                            AddShowID(id);
                            checkShowIDs.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }

    #endregion
}

public class InGameAidCtrl: MonoBehaviour
{
    [SerializeField]
    private UIButton m_HideAndShowBtn;
    [SerializeField]
    private Transform m_ContentParent;
    [SerializeField]
    private UIPanel m_Panel;
    [SerializeField]
    private UIScrollBar m_ScrollBar;
    [SerializeField]
    private GameObject m_LabelPrefab;
    [SerializeField]
    private TweenPosition m_Tween;
    [SerializeField]
    private BoxCollider m_MouseHoverCollider;
    [SerializeField]
    private int m_ShowCount=2;
    [SerializeField]
    private float m_PaddingY=10f;
    [SerializeField]
    private float m_ItemShowTime = 10f;
    [SerializeField]
    private float m_HideUITime = 10f;
    [SerializeField]
    private Color m_ReadColor = new Color(0.5f, 0.5f, 0.5f);
    
    

    private bool m_FirstShow = false;
    private bool m_Show = false;
    private bool m_ShowFinish = false;
    //private bool m_NeedReposition = false;
    private List<int> m_CurShow = new List<int>();
    private List<GameObject> m_CurShowLabelGos = new List<GameObject>();
    private Queue<int> m_WaitShowQueue = new Queue<int>();
    private Queue<GameObject> m_PrefabPools = new Queue<GameObject>();
    private bool m_EnableUI;
    private bool m_PlayQueueing = false;
    private float m_StartWaitHideTime;
    private bool m_MouseHover = false;


    #region mono methods
    void Start()
    {
        Init();
        if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story&& m_EnableUI)
        {
            EnableUI(true);
            FirstShow();
        }
        else
        {
            EnableUI(false);
        }
    }

    void Init()
    {
        m_EnableUI = SystemSettingData.Instance.AndyGuidance;
        m_FirstShow = false;
        m_Show = InGameAidData.ShowInGameAidCtrl;
        m_ShowFinish = false;
        //m_NeedReposition = false;
        m_CurShow.Clear();
        m_CurShowLabelGos.Clear();
        m_WaitShowQueue.Clear();
        m_PrefabPools.Clear();
        m_PlayQueueing = false;
        m_StartWaitHideTime = 0f;
        m_MouseHover = false;
        for (int i = 0; i < m_ContentParent.childCount; i++)
        {
            Transform trans=m_ContentParent.GetChild(i);
            if (null != trans && trans.gameObject)
            {
                trans.gameObject.SetActive(false);
                Destroy(trans.gameObject);
            }
        }
        m_Tween.onFinished = (tween) => { TweenFinish(); };
        UIEventListener.Get(m_HideAndShowBtn.gameObject).onClick = (btn) => { PlayTween(!m_Show); };
        UIEventListener.Get(m_MouseHoverCollider.gameObject).onHover = (go, isHover) => { m_MouseHover = isHover; };
    }
    void Update()
    {
        if (!m_FirstShow)
        {
            FirstShow();
        }

        if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story&&SystemSettingData.Instance.AndyGuidance != m_EnableUI)
        {
            m_EnableUI = SystemSettingData.Instance.AndyGuidance;
            EnableUI(m_EnableUI);
        }
    }

    #endregion

    #region private methods

    ///<summary>显示和隐藏整个UI</summary>
    private void EnableUI(bool show)
    {
        if (null != m_Tween)
        {
            m_Tween.gameObject.SetActive(show);
            m_Tween.enabled = false;
        }
        m_HideAndShowBtn.gameObject.SetActive(show);
        if (show)
        {
            m_HideAndShowBtn.isEnabled = true;
            RepositionVertical();
        }
    }

    ///<summary>新引导消息事件，添加到显示队列</summary>
    private void AddNewMsg(int id)
    {
        if (!m_WaitShowQueue.Contains(id))
        {
            m_WaitShowQueue.Enqueue(id);
            if (!m_Show)
                PlayTween(true);
            if (!m_PlayQueueing)
                StartCoroutine("ShowTipsIterator");
        }
    }

    ///<summary>更新每条显示消息的状态</summary>
    private void SetTipIsNew(GameObject go,bool isNew,bool showLine)
    {
        UILabel[] label = go.GetComponentsInChildren<UILabel>(true);
        if (null!=label&& label.Count()>0) label[0].color = isNew?Color.white: m_ReadColor;
        UISprite[] lineSprite = go.GetComponentsInChildren<UISprite>(true);
        if (null!= lineSprite&&lineSprite.Count()>0) lineSprite[0].gameObject.SetActive(showLine);
    }

    ///<summary>显示所有消息，第一次读档用</summary>
    private void ShowAllMsg()
    {
        if (InGameAidData.CurShowIDs.Count > m_ShowCount)
            InGameAidData.CurShowIDs.RemoveRange(0, InGameAidData.CurShowIDs.Count - m_ShowCount);

        for (int i = 0; i < InGameAidData.CurShowIDs.Count; i++)
        {
            AddItemByID(InGameAidData.CurShowIDs[i]);
        }
        if (!m_PlayQueueing)
            StartCoroutine("ShowTipsIterator");
    }

    ///<summary>控制显示消息事件和隐藏事件的的协程</summary>
    private IEnumerator ShowTipsIterator()
    {
        m_PlayQueueing = true;
        float startTime = 0; //第一条消息0延迟
        bool inWaitHide = false;
        while (true)
        {
            //等待一条消息的显示时间
            if (Time.realtimeSinceStartup - startTime >= m_ItemShowTime)
            {
                //消息队列有消息就显示消息
                if (m_WaitShowQueue.Count > 0)
                {
                    AddItemByID(m_WaitShowQueue.Dequeue());
                    startTime = Time.realtimeSinceStartup;
                    inWaitHide = false;
                }
                else
                {
                    //鼠标悬浮不进入隐藏计时
                    if (m_MouseHover)
                    {
                        if(inWaitHide)
                            inWaitHide = false;
                    }
                    else
                    {
                        if (!inWaitHide)
                        {
                            inWaitHide = true;
                            m_StartWaitHideTime = Time.realtimeSinceStartup;
                        }
                    }
                }
                
            }
            if(inWaitHide&& m_ShowFinish && Time.realtimeSinceStartup - m_StartWaitHideTime >= m_HideUITime)
            {
                if (m_Show)
                {
                    PlayTween(false);
                }
                inWaitHide = false;
            }
            yield return null;
        }
    }

    ///<summary>刷新所有Item的状态和位置</summary>
    void RepositionVertical()
    {
        if (null == m_CurShowLabelGos || m_CurShowLabelGos.Count <= 0)
            return;
        Bounds lastBounds=new Bounds();
        Vector3 preEndPos = Vector3.zero;
        Transform curTrans;
        Vector3 padingY = Vector3.zero;

        //改变颜色
        SetTipIsNew(m_CurShowLabelGos[0], m_CurShowLabelGos.Count == 1, false);
        if (m_CurShowLabelGos.Count > 1)
        {
            for (int i = 1; i < m_CurShowLabelGos.Count; i++)
            {
                SetTipIsNew(m_CurShowLabelGos[i], i == m_CurShowLabelGos.Count-1, true);
            }
        }


        //计算位置
        for (int i = 0; i < m_CurShowLabelGos.Count; i++)
        {
            curTrans = m_CurShowLabelGos[i].transform;
            curTrans.localPosition = preEndPos - padingY;
            padingY.y = m_PaddingY;
            lastBounds = NGUIMath.CalculateRelativeWidgetBounds(curTrans);
            preEndPos.y = curTrans.localPosition.y - lastBounds.size.y;
        }
        Bounds contentBounds = NGUIMath.CalculateRelativeWidgetBounds(m_ContentParent);
        Vector3 contentPos = m_ContentParent.transform.localPosition;
        float showHeight = m_Panel.clipRange.w - m_Panel.clipSoftness.y*2;
        m_Panel.transform.localPosition = Vector3.zero;
        Vector4 v4= m_Panel.clipRange;
        v4.x = 0;
        v4.y=-((m_Panel.clipRange.w - m_Panel.clipSoftness.y) * 0.5f);
        m_Panel.clipRange = v4;
        //超过显示区域
        if (contentBounds.size.y - showHeight > 0)
        {
            //最后一条顶端对齐
            contentPos.y = contentBounds.size.y - lastBounds.size.y;
        }
        else
        {
            //整体顶端对齐
            contentPos.y = 0;
        }
        m_ContentParent.transform.localPosition = contentPos;
    }

    ///<summary>通过引导ID添加一条Item</summary>
    private void AddItemByID(int id)
    {
        //超过限制移除消息，回收Item
        if (InGameAidData.CurShowIDs.Count > m_ShowCount && m_ShowCount > 0)
        {
            if (InGameAidData.CurShowIDs.Count > 0&& m_CurShowLabelGos.Count>0)
            {
                InGameAidData.CurShowIDs.RemoveAt(0);
                GameObject go = m_CurShowLabelGos[0];
                SetTipIsNew(go, true, true);
                go.SetActive(false);
                m_CurShowLabelGos.RemoveAt(0);
                m_PrefabPools.Enqueue(go);
            }
        }
        if (InGameAidData.AllData.ContainsKey(id)&&!m_CurShow.Contains(id))
        {
            int contentID = InGameAidData.AllData[id].CountentID;
            GameObject go = m_PrefabPools.Count > 0 ? m_PrefabPools.Dequeue() : GetNewLabel();
            if (null == go) return;
            go.SetActive(true);
            UILabel[] tempLabel = go.GetComponentsInChildren<UILabel>(true);
            if (tempLabel == null && tempLabel.Count()<= 0) return;
            tempLabel[0].text = PELocalization.GetString(contentID);
            tempLabel[0].MakePixelPerfect();
            m_CurShowLabelGos.Add(go);
            RepositionVertical();
            m_CurShow.Add(id);
        }   
    }

    ///<summary>检测第一次显示全部</summary>
    private void FirstShow()
    {
        if (!m_FirstShow && PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story && HasContent()&& m_EnableUI)
        {
            ShowAllMsg();
            PlayTween(m_Show);
            InGameAidData.AddEvent += AddNewMsg;
            m_FirstShow = true;
        }
    }

    ///<summary>播放收起和显示动画</summary>
    private void PlayTween(bool show)
    {
        if (null != m_Tween)
        {
            InGameAidData.ShowInGameAidCtrl = show;
            m_Show = show;
            m_Tween.Play(show);
            m_HideAndShowBtn.isEnabled = false;
            if (show) StartCoroutine(PlayOpenAudio(Time.realtimeSinceStartup, m_Tween.duration * 0.5f));
        }
    }

    ///<summary>动画播放完成事件</summary>
    private void TweenFinish()
    {
        m_ShowFinish = m_Show;
        if (m_ShowFinish) m_StartWaitHideTime = Time.realtimeSinceStartup;
        m_HideAndShowBtn.isEnabled = true;
        m_HideAndShowBtn.transform.rotation = Quaternion.Euler(m_Show ? Vector3.zero : new Vector3(0, 0, 180));
    }

    ///<summary>获取一条新的Item</summary>
    private GameObject GetNewLabel()
    {
        GameObject go = Instantiate(m_LabelPrefab);
        if (null != go)
        {
            go.transform.parent = m_ContentParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }
        return null;
    }

    ///<summary>判断数据层是否有要显示的引导数据</summary>
    private bool HasContent()
    {
        return InGameAidData.CurShowIDs.Count > 0;
    }

    ///<summary> 播放打开引导音效</summary>
    private IEnumerator PlayOpenAudio(float startTime,float waitTime)
    {
        while (Time.realtimeSinceStartup- startTime< waitTime)
            yield return null;
        GameUI.Instance.PlayWndOpenAudioEffect();
    }

    #endregion
}
