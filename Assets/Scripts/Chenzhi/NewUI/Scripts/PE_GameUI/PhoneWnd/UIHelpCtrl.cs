using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;
using System.Linq;

public class TutorialData
{
    public const int BuildingId = 1;
    public const int Building_1Id = 16;
    public const int PlantSolarId = 9;
    public const int RepairMachineId = 8;
	public const int GetOnVehicle = 15;

    public const int ColonyID0 = 20;  //lz-2016.10.24 新加的基地总体介绍的帮助图片
    public const int ColonyID1 = 4;
    public const int ColonyID2 = 5;
    public const int ColonyID3 = 6;
    public const int ColonyID4= 7;
    public const int ColonyID5 = 10;
    public const int ColonyID6 = 17;
    public const int ColonyID7 = 18;
    public const int ColonyID8 = 19;

    //lz-2016.11.04 冒险模式开启了技能树需要添加技能树帮助
    public static int[] SkillIDs = { 21, 22, 23, 24, 25 };  

    public const string HelpTexChinesePath = "Texture2d/HelpTex_Chinese/";
    public const string HelpTexEnglishPath = "Texture2d/HelpTex_English/";


    public int 		mID;
	public int		mType;
	public string mContent { get { return PELocalization.GetString(m_ContentID); } }
	public string	mTexName;

    private int m_ContentID;
	public static Dictionary<int,TutorialData> s_tblTutorialData;
	public static void LoadData()
	{
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("handphone");
		s_tblTutorialData = new Dictionary<int, TutorialData>();
		
		while (reader.Read())
		{
			TutorialData addData = new TutorialData();
			addData.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			addData.mType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
            addData.m_ContentID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Name")));
			addData.mTexName = reader.GetString(reader.GetOrdinal("Picture"));
			s_tblTutorialData[addData.mID] = addData;
		}
	}
	
	public static List<int> m_ActiveIDList = new List<int>();
	public delegate void OnAddActiveIDEvent(int id);
	public static event OnAddActiveIDEvent e_OnAddActiveID = null;

	public static bool AddActiveTutorialID(int _id,bool execEvent=true)
	{
		if(!m_ActiveIDList.Contains(_id) && s_tblTutorialData.ContainsKey(_id))
		{
			m_ActiveIDList.Add(_id);
			if (execEvent&&e_OnAddActiveID != null)
				e_OnAddActiveID(_id);
			return true;
		}
		return false;
	}

	public static byte[] Serialize()
	{
		try
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream(200);
			using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
			{
				bw.Write(m_ActiveIDList.Count);
				for (int i=0;i<m_ActiveIDList.Count;i++)
					bw.Write(m_ActiveIDList[i]);
			}
			return ms.ToArray();
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e);
			return null;
		}
	}

    public static bool Deserialize(byte[] buf)
    {
        m_ActiveIDList.Clear();
        try
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buf, false);
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
            {
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int id = br.ReadInt32();
                    //lz-2016.10.18 保证激活的ID是数据库存在的，有效的
                    if (s_tblTutorialData.ContainsKey(id))
                        m_ActiveIDList.Add(id);
                }

                //lz-2016.10.14 因为后面建造的帮助改成两个图片了，这里检测如果是否缺少，然后补齐
                if (m_ActiveIDList.Contains(BuildingId) && !m_ActiveIDList.Contains(Building_1Id))
                {
                    if (s_tblTutorialData.ContainsKey(Building_1Id))
                    {
                        m_ActiveIDList.Add(Building_1Id);
                    }
                }
                else if (!m_ActiveIDList.Contains(BuildingId) && m_ActiveIDList.Contains(Building_1Id))
                {
                    if (s_tblTutorialData.ContainsKey(BuildingId))
                    {
                        m_ActiveIDList.Add(BuildingId);
                    }
                }
                //lz-2016.10.18 如果以前的存档开启了Id为10的，就开启以下的
                if (m_ActiveIDList.Contains(10))
                {
                    //lz-2016.10.18 后面又新加的一部分基地的帮助图片
                    for (int i  = 17; i <20; i ++)
                    {
                        if (!m_ActiveIDList.Contains(i) && s_tblTutorialData.ContainsKey(i))
                        {
                            m_ActiveIDList.Add(i);
                        }
                    }
                }
                //lz-2016.10.24 新加的基地总体介绍的帮助图片，如果以前的存档第一个开启了ColonyID1，就直接开启这个ColonyID0
                if (m_ActiveIDList.Contains(ColonyID1))
                {
                    if(!m_ActiveIDList.Contains(ColonyID0) && s_tblTutorialData.ContainsKey(ColonyID0))
                        m_ActiveIDList.Add(ColonyID0);
                }
                return true;
            }
        }
        catch (System.Exception e)
        {

            Debug.LogWarning(e);
            return false;
        }
    }

    public static void Clear()
    {
        m_ActiveIDList.Clear();
    }
}

public class UIHelpCtrl : UIBaseWidget 
{
	[SerializeField] UIGrid				mHelpGrid;
	[SerializeField] TutorialItem_N		mPerfab;
	[SerializeField] UITexture			mHelpTex;
	[SerializeField] UILabel			mTitle;
	[SerializeField] UIPhoneWnd         mPhoneWnd;

	private List<TutorialItem_N> 		mTitleList = new List<TutorialItem_N>();
    private TutorialItem_N mBackUpItem;
    int mTabIndex = 0;
	//int mCurrentID = -1;

	private bool UpdateHelpGrid = false;

	public override void OnCreate()
	{
		TutorialData.e_OnAddActiveID += OnAddActiveID;
		base.OnCreate();
	}

	public override void OnDelete ()
	{
		TutorialData.e_OnAddActiveID -= OnAddActiveID;
		base.OnDelete ();
	}

	void AddAllTutorIDByGameMode() 
	{
        //lz-2016.07.05 功能 #2615 除故事模式以外的其他模式，进入后显示所有帮助信息
        if (Pathea.PeGameMgr.sceneMode != Pathea.PeGameMgr.ESceneMode.Story)
        {
            if (TutorialData.s_tblTutorialData.Count > 0)
            {
                int[] ids = TutorialData.s_tblTutorialData.Keys.ToArray();
                for (int i = 0; i < ids.Length; i++)
                {
                    //lz-2016.11.04 开启所有存档不包含技能树的
                    if (!TutorialData.m_ActiveIDList.Contains(ids[i])&&!TutorialData.SkillIDs.Contains(ids[i]))
                    {
                        TutorialData.m_ActiveIDList.Add(ids[i]);
                    }
                }
            }
        }

        //lz-2016.11.04 如果是Adventure并且开启了技能树，就添加所有的技能树帮助ID
        if (Pathea.PeGameMgr.IsAdventure && RandomMapConfig.useSkillTree)
        {
            for (int i = 0; i < TutorialData.SkillIDs.Length; i++)
            {
                if (!TutorialData.m_ActiveIDList.Contains(TutorialData.SkillIDs[i]))
                {
                    TutorialData.m_ActiveIDList.Add(TutorialData.SkillIDs[i]);
                }
            }
        }
	}

	void LateUpdate ()
	{
		if (UpdateHelpGrid)
			mHelpGrid.repositionNow = true;
	}

	public override void Show ()
	{
        base.Show();
		ResetTutorial();
	}

	protected override void InitWindow ()
	{
        this.AddAllTutorIDByGameMode();
		base.InitWindow ();
	}

	void ResetTutorial()
	{
		List<int> showIDList = new List<int>();
		
		foreach(TutorialItem_N item in mTitleList)
		{
			item.transform.parent = null;
			Destroy(item.gameObject);
		}
		mTitleList.Clear();
		
		foreach(int id in TutorialData.m_ActiveIDList)
		{
			if(TutorialData.s_tblTutorialData.ContainsKey(id)&&TutorialData.s_tblTutorialData[id].mType == mTabIndex + 1)
				showIDList.Add(id);
		}

        //lz-2016.10.18 显示排序顺序修改为根据数据库填写数据的顺序
        List<int> allId = TutorialData.s_tblTutorialData.Keys.ToList();
        showIDList = showIDList.OrderBy(a =>(allId.FindIndex(b=>b==a))).ToList();

        for (int i = 0; i < showIDList.Count; i++)
		{
			TutorialItem_N addItem = Instantiate(mPerfab) as TutorialItem_N;
			addItem.transform.parent = mHelpGrid.transform;
			addItem.transform.localScale = Vector3.one;
			addItem.transform.localPosition = Vector3.zero;
            addItem.mCheckBox.radioButtonRoot = null;
            addItem.mCheckBox.startsChecked = false;
            addItem.mCheckBox.isChecked = false;
            addItem.SetItem(showIDList[i],TutorialData.s_tblTutorialData[showIDList[i]].mContent);
            addItem.e_OnClick += OnClickGridItem;
			mTitleList.Add(addItem);
		}
        //lz-2016.07.12 唐小力说打开的时候默认选择第一个
        if (mTitleList.Count > 0)
        {
            mTitleList[0].mCheckBox.isChecked = true;
            OnClickGridItem(mTitleList[0]);
        }
		UpdateHelpGrid = true;
	}

	void OnClickGridItem(object sender)
	{
		TutorialItem_N item = sender as TutorialItem_N;
		if (item == null)
			return;
        ChangeSelect(item.mID);
	}

	public void ChangeSelect(int ID)
	{
        if (!TutorialData.s_tblTutorialData.ContainsKey(ID))
		{
			mHelpTex.enabled = false;
			return;
		}

        if (null != mBackUpItem)
            mBackUpItem.mCheckBox.isChecked = false;
        TutorialItem_N curClickItem = GetItemByID(ID);
        if(null!=curClickItem)
            curClickItem.mCheckBox.isChecked = true;
        mBackUpItem = curClickItem;

        mHelpTex.enabled = true;
		//mCurrentID = ID;
		mTitle.text = TutorialData.s_tblTutorialData[ID].mContent;
		
		if(null != mHelpTex.mainTexture)
			Destroy(mHelpTex.mainTexture);
        //lz-2016.10.12 帮助图片按语言加载
		string path = SystemSettingData.Instance.IsChinese ? TutorialData.HelpTexChinesePath : TutorialData.HelpTexEnglishPath;
        path+= TutorialData.s_tblTutorialData[ID].mTexName;
        Texture tex = Resources.Load(path) as Texture;

        mHelpTex.mainTexture = Instantiate(tex) as Texture;
		Resources.UnloadAsset(tex);
	}

    TutorialItem_N GetItemByID(int id)
    {
        return mTitleList.Find(a => a.mID == id);
    }

    void OnAddActiveID(int id)
	{
        if (mPhoneWnd == null)
            return;
        mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
		ChangeSelect(id);
	}
}
