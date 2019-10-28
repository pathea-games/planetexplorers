using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mono.Data.SqliteClient;
using System.Linq;
using WhiteCat;
using Pathea;

//lz-2016.07.12 每一条怪物图鉴的的数据结构,以及对应数据库表的数据映射，以及用户数据的存储和读取
public class MonsterHandbookData
{
    private int m_ID;
    private int[] m_MonsterIDs;
    private int m_ModelID; 
    private string m_Name; 
    private string m_Description;

    private const int m_MhVersion = 1607200;  //lz-2016.07.20 版本号，避免更改存储数据结构读取报错

    public int ID { get { return m_ID; } }
    public int[] MonsterIDs { get { return m_MonsterIDs; } }
    public int ModelID { get { return m_ModelID; } }
    public string Name { get { return m_Name; } }
    public string Description { get { return m_Description; } }

    //static field
    public static Dictionary<int, MonsterHandbookData> AllMonsterHandbookDataDic = new Dictionary<int, MonsterHandbookData>(); // key=MhID,value=Data
    public static Dictionary<int, int[]> AllMonsterIDDic = new Dictionary<int, int[]>();  //key=MhID,value=MonsterIDs
    public static List<int> ActiveMhDataID = new List<int>();  //ActiveMsgID
    public static Action<int> AddMhEvent; //MhID
    public static Action GetAllMonsterEvent; //lz-2016.07.23 收集完成所有的类型怪物的事件

    public MonsterHandbookData(int id,int[] monsterIDs,int modelID,string name,string description)
    {
        this.m_ID = id;
        this.m_MonsterIDs = monsterIDs;
        this.m_ModelID = modelID;
        this.m_Name = name;
        this.m_Description = description;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("book");
        MonsterHandbookData info = null;
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            string monsterIDsStr = reader.GetString(reader.GetOrdinal("MonsterID"));
            int modelID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ModelID")));
            int nameID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Name")));
            int descriptionID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Content")));

            string[] monsterIDsStrs=monsterIDsStr.Split(new char[] { ',' });
            int[] monsterIDs=new int[monsterIDsStrs.Length];
            for (int i = 0; i < monsterIDsStrs.Length; i++)
                monsterIDs[i] = Convert.ToInt32(monsterIDsStrs[i]);

            info = new MonsterHandbookData(id,
                    monsterIDs,
                    modelID,
                    PELocalization.GetString(nameID),
                    PELocalization.GetString(descriptionID));
            AllMonsterHandbookDataDic.Add(id,info);
            AllMonsterIDDic.Add(id, monsterIDs);
        }
    }

    public static void AddMhByKilledMonsterID(int monsterID)
    {
        if (null == AllMonsterHandbookDataDic || AllMonsterHandbookDataDic.Count <= 0)
            return;

        int[] allActiveMhID = AllMonsterIDDic.Where(a => a.Value.Contains(monsterID)).ToDictionary(k => k.Key, v => v.Value).Keys.ToArray();
        for (int i = 0; i < allActiveMhID.Length; i++)
        {
            if (!ActiveMhDataID.Contains(allActiveMhID[i]))
            {
                ActiveMhDataID.Add(allActiveMhID[i]);
                if (null != AddMhEvent)
                    AddMhEvent(allActiveMhID[i]);
                if (ActiveMhDataID.Count == AllMonsterIDDic.Count)
                {
                    if (null != GetAllMonsterEvent)
                        GetAllMonsterEvent();
                }
            }
        }
    }

    public static bool Deserialize(byte[] data)
    {
        ActiveMhDataID.Clear();
        try
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(data,false);
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
            {
                int version=reader.ReadInt32();
                if (version!= m_MhVersion && PeGameMgr.IsSingle)
                {
                    Debug.LogWarning(string.Format("MonsterHandbookData version valid! CurVersion:{0} ErrorVersion:{1}",m_MhVersion,version));
                    return false;
                }
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    ActiveMhDataID.Add(reader.ReadInt32());
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("MonsterHandbookData deserialize error: " + e);
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
                writer.Write(m_MhVersion);
                writer.Write(ActiveMhDataID.Count);
                for (int i = 0; i < ActiveMhDataID.Count; i++)
                    writer.Write(ActiveMhDataID[i]);
            }
            return stream.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogWarning("MonsterHandbookData serialize error: "+e);
            return null;
        }
    }

    public static void Clear()
    {
        ActiveMhDataID.Clear();
    }
}

//lz-2016.06.12 这个脚本是Phone界面新加的MonsterHandbook页面的控制脚本
public class UIMonsterHandbookCtrl : UIBaseWidget
{
    [SerializeField]
    private UIListItem m_ListItemPrefab;
    [SerializeField]
    private UIGrid m_ListGrid;
    [SerializeField]
    private UIScrollBar m_ListScrollBar;
    [SerializeField]
    private UILabel m_NameLabel;
    [SerializeField]
    private UILabel m_SizeInfoLabel;
    [SerializeField]
    private UILabel m_DescriptionLabel;
    [SerializeField]
    private UIScrollBar m_DescriptionScrollBar;
    [SerializeField]
    private UIViewController m_UIViewCtrl;
    [SerializeField]
    private float m_CameraScaleFactor = 0.4f; //lz-2016.07.21 相机的缩放，根据最佳距离进行偏移
    [SerializeField]
    private UITexture m_CameraTxuture;

    private Queue<UIListItem> m_MsgListItemPool = new Queue<UIListItem>();
    private List<UIListItem> m_CurListItems = new List<UIListItem>();
    private UIListItem m_BackupSelectItem;
    private PeViewController m_PeViewCtrl;
    private int m_OpCameraStep=0; //lz-2016.07.23  0：关闭相机，加载模型 1：计算Bounds，设置相机参数  2：开启相机
    private Vector2 m_ViewWndSize;
    private Animator m_CurAnimator;
    private MovementField m_MovementField;
    private string m_CurIdleAnimName;
    private int m_CurIdleIndex;

    #region mono methods

    void LateUpdate()
    {
        if (this.m_OpCameraStep==1)
        {
            Bounds bounds = SpeciesViewStudio.GetModelBounds(this.m_CurMonsterGo);
            this.UpdateSizeInfo(bounds);
            float distance, yaw;
            SpeciesViewStudio.GetCanViewModelFullDistance(this.m_CurMonsterGo, this.m_PeViewCtrl.viewCam, this.m_ViewWndSize, out distance, out yaw);
            this.m_UIViewCtrl.SetCameraToBastView(bounds.center, distance, yaw, this.m_CameraScaleFactor);
            this.PlayAnimator();
            this.m_OpCameraStep = 2;
        }
        else if (this.m_OpCameraStep == 2)
        {
            this.m_PeViewCtrl.viewCam.enabled = true;
            this.m_OpCameraStep = 0;
        }
    }
    #endregion

    #region override methods

    public override void OnCreate()
    {
        this.m_MsgListItemPool.Clear();
        this.m_CurListItems.Clear();
        MonsterHandbookData.AddMhEvent += this.AddMhByData;
        this.m_PeViewCtrl = SpeciesViewStudio.CreateViewController(ViewControllerParam.DefaultSpicies);
        this.m_UIViewCtrl.Init(this.m_PeViewCtrl);
        this.m_CameraTxuture.mainTexture = this.m_PeViewCtrl.RenderTex;
        this.m_ViewWndSize =new Vector2(this.m_CameraTxuture.mainTexture.width, this.m_CameraTxuture.mainTexture.height);
//		SpeciesViewStudio.Instance.gameObject.SetActive(false);
    }

    protected override void InitWindow()
    {
        base.InitWindow();
        this.CheckGameMode();
    }

    public override void Show()
    {
        base.Show();
        this.ResetWnd();
        this.UpdateMhList();		
//		SpeciesViewStudio.Instance.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        base.OnHide();
        this.DestoryCurModel();
        this.RecycleMhListItem();		
//		SpeciesViewStudio.Instance.gameObject.SetActive(false);
    }

    protected override void OnClose()
    {
        base.OnClose();
        this.DestoryCurModel();
        this.RecycleMhListItem();
    }
   
    #endregion

    #region private methods

    private void ResetWnd()
    {
        this.m_OpCameraStep = 0;
        this.m_CurMonsterGo = null;
        this.m_CurMonsterID = -1;
        this.m_ListScrollBar.scrollValue = 0;
        this.m_DescriptionScrollBar.scrollValue = 0;
        this.m_BackupSelectItem = null;
        this.ResetAllLabel();
    }

    private void AddMhByData(int mhID)
    {
        if (null != this&&this.gameObject.activeInHierarchy)
        {
            this.AddNewListItem(mhID);
            this.CurListItemOderByID();
        }
    }

    private void UpdateMhList()
    {
        if (null == MonsterHandbookData.ActiveMhDataID || MonsterHandbookData.ActiveMhDataID.Count <= 0)
            return;
        for (int i = 0; i < MonsterHandbookData.ActiveMhDataID.Count; i++)
		{
            if (!this.m_CurListItems.Any(a => a.ID == MonsterHandbookData.ActiveMhDataID[i]))
            {
                this.AddNewListItem(MonsterHandbookData.ActiveMhDataID[i]);
            }
		}
        this.CurListItemOderByID();
    }

    private void CurListItemOderByID()
    {
        if (null != this.m_CurListItems && this.m_CurListItems.Count > 0)
        {
            //通过ID排序显示
            this.m_CurListItems = this.m_CurListItems.OrderBy(item => item.ID).ToList();
            for (int i = 0; i < this.m_CurListItems.Count; i++)
            {
                this.m_CurListItems[i].gameObject.name = i.ToString("D3") + "_ListItem";
            }
            this.m_ListGrid.repositionNow = true;

            //lz-2016.07.23 选中第一个
            this.m_CurListItems[0].Select();
        }
    }

    private void AddNewListItem(int msgID)
    {
        UIListItem item = this.GetNewMsgListItem();
        item.UpdateInfo(msgID, MonsterHandbookData.AllMonsterHandbookDataDic[msgID].Name);
        item.SelectEvent = this.MhListItemSelectEvent;
        this.m_CurListItems.Add(item);
    }

    private void MhListItemSelectEvent(UIListItem item)
    {
        if (item == this.m_BackupSelectItem)
            return;
        if (null != this.m_BackupSelectItem)
        {
            this.m_BackupSelectItem.CancelSelect();
        }
        this.m_BackupSelectItem = item;

        if (MonsterHandbookData.ActiveMhDataID.Contains(item.ID))
        {
            MonsterHandbookData info = MonsterHandbookData.AllMonsterHandbookDataDic[item.ID];
            this.SetNewModel(info.ModelID);
            this.UpdateDescription(info.Description);
            this.UpdateName(info.Name);
        }
        else
        {
            this.ResetAllLabel();
        }
    }

    private void ResetAllLabel()
    {
        this.UpdateName("");
        this.UpdateDescription("");
    }

    private void UpdateName(string name)
    {
        this.m_NameLabel.text = PELocalization.GetString(10019)+name;
    }

    private void UpdateSizeInfo(Bounds bounds)
    {
        //lz-2016.08.10 长和宽放反了
        this.m_SizeInfoLabel.text = string.Format("{0}{1:F2}\n{2}{3:F2}\n{4}{5:F2}", PELocalization.GetString(8000597), bounds.size.z, PELocalization.GetString(8000598), bounds.size.x, PELocalization.GetString(8000599), bounds.size.y);
    }

    private void UpdateDescription(string decription)
    {
        if (decription == "0" || string.IsNullOrEmpty(decription))
        {
            this.m_DescriptionLabel.gameObject.SetActive(false);
        }
        else
        {
            this.m_DescriptionLabel.gameObject.SetActive(true);
            decription = decription.Replace("<\\n>", "\n");
            this.m_DescriptionLabel.text = decription;
        }
        this.m_DescriptionScrollBar.scrollValue = 0;
    }

    private int m_CurMonsterID;
    private GameObject m_CurMonsterGo;

    private void SetNewModel(int modelID)
    {
        if (modelID == m_CurMonsterID) return;
        this.m_CurAnimator = null;
        this.m_MovementField = MovementField.None;
        DestoryCurModel();
        this.m_PeViewCtrl.viewCam.enabled = false;
        this.m_CurMonsterGo = SpeciesViewStudio.LoadMonsterModelByID(modelID, ref m_MovementField);
        this.m_CurMonsterID = modelID;
        if (null == m_CurMonsterGo) return;
        this.m_CurAnimator = m_CurMonsterGo.GetComponent<Animator>();
        this.m_PeViewCtrl.SetTarget(this.m_CurMonsterGo.transform);
        this.m_OpCameraStep = 1;
    }

    private void PlayAnimator()
    {
        if (null != this.m_CurAnimator)
        {
            this.m_CurIdleAnimName = m_MovementField == Pathea.MovementField.Sky ? "normalsky_leisure" : "normal_leisure";
            this.m_CurIdleIndex = 0;
            if (m_MovementField == Pathea.MovementField.Sky) this.m_CurAnimator.SetBool("Fly", true);
            this.m_CurAnimator.SetTrigger(this.m_CurIdleAnimName + this.m_CurIdleIndex);
            AnimatorClipInfo[] idleClip = this.m_CurAnimator.GetCurrentAnimatorClipInfo(0);
            if (null != idleClip && idleClip.Length > 0)
            {
                idleClip[0].clip.SampleAnimation(this.m_CurAnimator.gameObject, 0);
            }
            //lz-2016.07.27  随机动画切换有点生硬，暂时不要了
            this.StopCoroutine("PlayIdelAnimaIterator");
            this.StartCoroutine("PlayIdelAnimaIterator");
        }
    }

    private IEnumerator PlayIdelAnimaIterator()
    {
        float startTime = Time.realtimeSinceStartup;
        float waitTime = UnityEngine.Random.Range(6, 12);
        while (null!=this.m_CurAnimator)
        {
            if (Time.realtimeSinceStartup - startTime >= waitTime && !m_CurAnimator.GetBool("Leisureing"))
            {
                if (++this.m_CurIdleIndex > 2)
                {
                    this.m_CurIdleIndex = 0;
                }
                this.m_CurAnimator.SetTrigger(this.m_CurIdleAnimName + (++this.m_CurIdleIndex));
                startTime = Time.realtimeSinceStartup;
            }
            //lz-2016.10.27 等一帧放后边，避免刚好那一帧的时候m_CurAnimator被置空
            yield return null;
        }
    }

    private void DestoryCurModel()
    {
        GameObject.Destroy(m_CurMonsterGo);
        m_CurMonsterID = -1;
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

    private void RecycleMhListItem()
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

    /// <summary> 检测游戏模式，Adventure是默认开启所有的</summary>
    private void CheckGameMode()
    {
        if(PeGameMgr.IsAdventure)
        {
            int[] ids= MonsterHandbookData.AllMonsterHandbookDataDic.Keys.ToArray();
            for (int i = 0; i < ids.Length; i++)
            {
                if (!MonsterHandbookData.ActiveMhDataID.Contains(ids[i]))
                    MonsterHandbookData.ActiveMhDataID.Add(ids[i]);
            }
        }
    }

    #endregion
}