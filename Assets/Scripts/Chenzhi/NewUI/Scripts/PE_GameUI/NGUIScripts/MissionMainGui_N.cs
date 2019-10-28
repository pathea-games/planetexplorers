/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AiAsset;
using ItemAsset;
using Pathea;


public class MissionMainGui_N : UIBaseWnd
{
	public MissionSelItem_N mPrefab;
	
	public Grid_N			mPrefabGrid_N;
	
	public MissionTargetItem_N mPrefabTarget;
	
	public UITable	mUITable;
	
	public UITable	mMainMissionTable;
	
	public UITable	mSubMissionTable;
	
	List<MissionSelItem_N>  mMissionList = new List<MissionSelItem_N>();
	
	List<MissionTargetItem_N>	mTargetList = new List<MissionTargetItem_N>();
	List<Grid_N>			mRewardsList = new List<Grid_N>();
	
	public UILabel			mDesLabel;
	public UITable			mTargetGrid;
	public UIGrid			mRewardsGrid;
	
	
	public UILabel			mGiverName;
	public UISprite			mGiverSpr;
	public UILabel			mSubmitName;
	public UISprite			mSubmitSpr;
	
	public MissionTrackGui_N	mMissiontrack;
	//public MainRightGui_N		mRightWnd;
	public WorldMapGui_N		mWorldMap;
	
	public UICheckbox		mOpenMissionBtn;
	
	public UIButton			mAbortMission;
	
	public UIScrollBar		mDesSB;
	public UIScrollBar		mTargetSB;
	public UIScrollBar		mRewardSB;
	
	int 					mCurrentMissionID = -1;
	
	int 					mReposCount = 2;
	
	public class stMissionView
    {
        public int MissionID;                       
        public MissionType mMissionType;
        public string MissionTitle;		            
        public string MissionExplain;	            
        public string MissionDesc;  	            
        public string MissionFlagName;	            
        public string MissionFlagValue;	            
        public string NPCReplyName;					
        public string MissionTraceText;				
		
		public List<TargetShow> mTargetList;
		public List<ItemSample> mRewardsList;
        public List<ItemSample> mSelRewardsList;
    };
	
	public class TargetShow
	{
        public string       mContent;
		public List<string>	mIconName = new List<string>();
		public int			mNum;
	}

    Dictionary<int, stMissionView> m_HasMissionViewInfo = new Dictionary<int, stMissionView>();   //ÈËÎïÉíÉÏÒÑœÓµÄÈÎÎñÊýŸÝ

    public PeEntity m_Player;
    private bool m_bIsShowMissionMainGUI;
    private bool m_bToggle;
    private int m_ToolBar;
    private string[] m_ToolString;
//    private string m_MissionDescription;
//    private string m_MissionTarget;
//    private string m_MissionRewards;
	
//	private string mNPCReplyName;
	
//	float 			mUpdateTime = 2f;
	
	int mNumCount = 0;
	
//	public List<TargetShow> mTargetList;
//	public List<Grid> mRewardsList;
//	Vector2 mDesPos = Vector2.zero;
	
	class MainMission
	{
		int 			ID;
		public stMissionView   mView;
		public Dictionary<int, stMissionView> mSubMission;
		
		public MainMission(int id,stMissionView   view)
		{
			ID = id;
			mView = view;
			mSubMission = new Dictionary<int, stMissionView>();
		}
		public bool AddSubMission(int id,stMissionView view)
		{
			if(mSubMission.ContainsKey(id))
				return false;
			
			mSubMission[id] = view;
			return true;
		}
		public bool RemoveSubMission(int id)
		{
			if(mSubMission.ContainsKey(id))
			{
				mSubMission.Remove(id);
			}
			
			return false;
		}
	}	
	Dictionary<int, MainMission> mMainMissionData = new Dictionary<int, MainMission>();

    //»ñÈ¡ÈÎÎñÌáÊŸÊýŸÝ
    stMissionView GetHasMissionView(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)PlayerMission.MissionInfo.MAX_MISSION_COUNT)
            return null;

        return m_HasMissionViewInfo.ContainsKey(MissionID) ? m_HasMissionViewInfo[MissionID] : null;
    }
	
	public override void Show()
	{
		base.Show ();
		mDepth = 60f;
		mOpenMissionBtn.gameObject.SetActive(false);
		mAbortMission.gameObject.SetActive(false);
		ResetMissionList();
		
		ClearMissionInfo();
		if(mMissionList.Count > 0)
			OnMutexBtnClick(mMissionList[0].mMissionId);
		//MainRightGui_N.Instance.NoticeMissionUpdate(false);
		mRewardsGrid.Reposition();
		mTargetGrid.Reposition();
		mSubMissionTable.Reposition();
		mMainMissionTable.Reposition();
		mUITable.Reposition();
		Invoke("UpdateTable", 0.05f);
	}
	
	void UpdateTable()
	{
		mRewardsGrid.Reposition();
		mTargetGrid.Reposition();
		mSubMissionTable.Reposition();
		mMainMissionTable.Reposition();
		mUITable.Reposition();
	}

    public void DeleteHasMissionView(int MissionID)
    {
        stMissionView missionView = GetHasMissionView(MissionID);
        if (missionView != null)
		{
            m_HasMissionViewInfo.Remove(MissionID);
			ResetMissionList();
		}
    }
	
	bool AddMainMission(int id ,stMissionView view)
	{
        Debug.Log("Coming111_111" + id);
		if(mMainMissionData.ContainsKey(id))
			return false;
		mMainMissionData[id] = new MainMission(id,view);
		ResetMissionList();
		return true;
	}
	
	bool RemoveMainMission(int id)
	{
		if(mMainMissionData.ContainsKey(id))
		{
			mMainMissionData.Remove(id);
			ResetMissionList();
			return true;
		}
		return false;
	}
	
	bool AddSubMission(int mainMissionId, int subMissionId, stMissionView view)
	{
		if(mMainMissionData.ContainsKey(mainMissionId))
			return mMainMissionData[mainMissionId].AddSubMission(subMissionId,view);
		
		return false;
	}
	
	bool RemoveSubMisson(int mainMissionId,int subMissionId)
	{
		if(mMainMissionData.ContainsKey(mainMissionId))
			return mMainMissionData[mainMissionId].RemoveSubMission(subMissionId);
		
		return false;
	}

    public void SetCurPlayer(PeEntity player)
    {
        m_Player = player;
    }

    public void InitMissionData()
    {
        foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_Player.m_PlayerMission.m_MissionInfo)
        {
            int id = iter.Key;
            Dictionary<string, string> missionFlagType = iter.Value;

            MissionCommonData data = MissionManager.GetMissionCommonData(id);

            if(data == null)
                continue;

            if (data.IsTalkMission())
                continue;

            GetMissionData(data, missionFlagType);
        }

        UpdateMission(-1);
		ResetMissionList();
    }

    bool GetMissionData(MissionCommonData data, Dictionary<string, string> MissionFlagType)
    {
        stMissionView stMV = new stMissionView();
        stMV.MissionID = data.m_ID;
        stMV.mMissionType = data.m_Type;
        stMV.MissionTitle = data.m_MissionName;
        //stMV.NPCReplyName = data.m_ReplyNpc;
        stMV.mTargetList = new List<TargetShow>();
        stMV.mRewardsList = new List<ItemSample>();
        stMV.mSelRewardsList = new List<ItemSample>();

        ParseMissionFlag(data, MissionFlagType, stMV);

        AddMainMission(data.m_ID, stMV);

        return true;
    }

    void ParseMissionFlag(MissionCommonData data, Dictionary<string, string> MissionFlagType, stMissionView stMV)
    {
        string content = data.m_Description;
        if(content == null)
            return ;
		
        content = GameUI.Instance.mNPCTalk.ParseStrDefine(content, data);

        if(data.m_Type == MissionType.MissionType_Mul)
        {
            TargetShow addTarget = new TargetShow();
            addTarget.mContent = data.m_MulDesc;

            stMV.mTargetList.Add(addTarget);

            stMV.MissionDesc = content;
            return;
        }
		
        string monstername = "\"monsterid%\"";
        string monsternum = "\"monsternum%\"";
        string killmonname = "\"killedmosternum%\"";

        string pos = "\"position%\"";
        string npc1 = "\"npcid1%\"";
        string npc2 = "\"npcid2%\"";
        string npc3 = "\"npcid3%\"";

        string itemname = "\"itemid%\"";
        string itemnum = "\"itemnum%\"";
        string targetitem = "\"targetitemid%\"";

        string givemisnpc = "\"givenpcid%\"";
        string receivenpc = "\"receivenpcid%\"";
        string giveitemname = "\"giveitemid%\"";
        string giveitemnum = "\"giveitemnum%\"";

        string ritemnum = "\"n-ri%\"";
        string ritemname = "\"ri%\"";
        string playername = "\"name%\"";

        AiNpcObject npc;
        int outinfo;

        int num;
        for(int m=0; m<data.m_TargetIDList.Count; m++)
        {
            TypeMonsterData monData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[m]);
            TypeCollectData colData = MissionManager.GetTypeCollectData(data.m_TargetIDList[m]);
            TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[m]);
            TypeSearchData seaData = MissionManager.GetTypeSearchData(data.m_TargetIDList[m]);
            TypeUseItemData useData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[m]);
            TypeMessengerData mesData = MissionManager.GetTypeMessengerData(data.m_TargetIDList[m]);
            TypeTowerDefendsData towData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[m]);
            TargetShow addTarget = new TargetShow();

            if (monData != null)
            {
                addTarget.mContent = monData.m_Desc != "0" ? monData.m_Desc : "";
                string oldcontent = "";

                for(int i=0; i<monData.m_MonsterList.Count; i++)
                {
					addTarget.mIconName.Add(AiDataBlock.GetIconName(monData.m_MonsterList[i].id));
                    num = m_Player.m_PlayerMission.GetQuestVariable(data.m_ID, "monster", monData.m_MonsterList[i].id);
	                if (num >= monData.m_MonsterList[i].num)
	                    addTarget.mContent = oldcontent + "[EEFF11]" + addTarget.mContent;
	                else
	                    addTarget.mContent = oldcontent + "[00BfFF]" + addTarget.mContent;

                    if (addTarget.mContent.Contains(monstername))
                        addTarget.mContent = addTarget.mContent.Replace(monstername, AiDataBlock.GetAIDataName(monData.m_MonsterList[i].id));

                    if (addTarget.mContent.Contains(monsternum))
                        addTarget.mContent = addTarget.mContent.Replace(monsternum, monData.m_MonsterList[i].num.ToString());

                    addTarget.mContent += "[-]";
                    oldcontent += addTarget.mContent;
                }
            }
            else if (colData != null)
            {
                addTarget.mContent = colData.m_Desc != "0" ? colData.m_Desc : "";
                addTarget.mIconName.Add(ItemData.GetIconName(colData.m_ItemID).Split(',')[0]);

                addTarget.mNum = colData.m_ItemNum;
                num = 0;
                //num = m_Player.GetItemNum(colData.m_ItemID);

                int type = m_Player.m_PlayerMission.IsSpecialID(colData.m_ItemID);
                if(type > 0)
                    num = m_Player.m_PlayerMission.GetCollectSpecialItem(type, colData.m_ItemID);

	            if(num >= colData.m_ItemNum)
	                addTarget.mContent = "[EEFF11]" + addTarget.mContent;
	            else
	                addTarget.mContent = "[00BfFF]" + addTarget.mContent;

                if (addTarget.mContent.Contains(itemname))
                    addTarget.mContent = addTarget.mContent.Replace(itemname, ItemAsset.ItemData.GetName(colData.m_ItemID));

                if (addTarget.mContent.Contains(itemnum))
                    addTarget.mContent = addTarget.mContent.Replace(itemnum, colData.m_ItemNum.ToString());

                if (addTarget.mContent.Contains(targetitem))
                    addTarget.mContent = addTarget.mContent.Replace(targetitem, colData.m_TargetItemID.ToString());

                //string strName = data.m_Npc;
                //if (int.TryParse(data.m_Npc, out outinfo))
                //{
                //    npc = NpcManager.Instance.GetNpcRandom(outinfo);
                //    if(npc != null)
                //        strName = npc.NpcName;
                //    else
                //    {
                //        AiAdNpcNetwork adnpc = NpcManager.Instance.GetMutNpcData(outinfo);
                //        if (adnpc != null)
                //            strName = adnpc.mNpcName;
                //    }
                //}

                //if (addTarget.mContent.Contains(npc1))
                //    addTarget.mContent = addTarget.mContent.Replace(npc1, strName);

                addTarget.mContent += "[-]";
            }
            else if(folData != null)
            {
				addTarget.mContent = folData.m_Desc != "0" ? folData.m_Desc : "";
                //for(int i=0; i<folData.m_NpcList.Count; i++)
                //{
                //    if (int.TryParse(folData.m_NpcList[i], out outinfo))
                //        npc = NpcManager.Instance.GetNpcRandom(outinfo);
                //    else
                //        npc = NpcManager.Instance.GetNpc(folData.m_NpcList[i]);

                //    if(npc != null)
                //    {
                //        if(folData.m_NpcList[i] == "AllenCarryingGerdy")
                //            addTarget.mIconName.Add("npc_AllenCarter");
                //        else
                //            addTarget.mIconName.Add(npc.m_NpcIcon);

                //        if(i == 0 && addTarget.mContent.Contains(npc1))
                //            addTarget.mContent = addTarget.mContent.Replace(npc1, folData.m_NpcList[i]);
                //        else if(i == 1 && addTarget.mContent.Contains(npc2))
                //            addTarget.mContent = addTarget.mContent.Replace(npc2, folData.m_NpcList[i]);
                //        else if(i == 2 && addTarget.mContent.Contains(npc3))
                //            addTarget.mContent = addTarget.mContent.Replace(npc3, folData.m_NpcList[i]);
                //    }
                //}

                if (addTarget.mContent.Contains(pos))
                    addTarget.mContent = addTarget.mContent.Replace(pos, folData.m_DistPos.ToString());
            }
            else if(seaData != null)
            {
                addTarget.mContent = seaData.m_Desc != "0" ? seaData.m_Desc : "";

                if (addTarget.mContent.Contains(pos))
                    addTarget.mContent = addTarget.mContent.Replace(pos, seaData.m_DistPos.ToString());

                //if(addTarget.mContent.Contains(npc1))
                //    addTarget.mContent = addTarget.mContent.Replace(npc1, seaData.m_NpcName);
            }
            else if(useData != null)
            {
                addTarget.mContent = useData.m_Desc != "0" ? useData.m_Desc : "";
                addTarget.mIconName.Add(ItemData.GetIconName(useData.m_ItemID).Split(',')[0]);

                addTarget.mNum = useData.m_UseNum;

                if (addTarget.mContent.Contains(pos))
                    addTarget.mContent = addTarget.mContent.Replace(pos, useData.m_Pos.ToString());

                if (addTarget.mContent.Contains(itemname))
                    addTarget.mContent = addTarget.mContent.Replace(itemname, ItemAsset.ItemData.GetName(useData.m_ItemID));

                if (addTarget.mContent.Contains(itemnum))
                    addTarget.mContent = addTarget.mContent.Replace(itemnum, useData.m_UseNum.ToString());

                num = m_Player.m_PlayerMission.GetQuestVariable(data.m_ID, "item",useData.m_ItemID);

                addTarget.mContent = "[00BfFF]" + addTarget.mContent;

                if(num >= useData.m_UseNum && useData.m_TargetID != 5004)   //埋骨灰盒，暂时特殊处理
                    addTarget.mContent = "[EEFF11]" + addTarget.mContent;

                addTarget.mContent += "[-]";

            }
            else if(mesData != null)
            {
                //addTarget.mContent = mesData.m_Desc != "0" ? mesData.m_Desc : "";
                //npc = NpcManager.Instance.GetNpc(mesData.m_ReplyNpc);
                //if(npc != null)
                //{
                //    addTarget.mIconName.Add(npc.m_NpcIcon);
                //}

                //if (addTarget.mContent.Contains(givemisnpc))
                //    addTarget.mContent = addTarget.mContent.Replace(givemisnpc, mesData.m_Npc);

                //if (addTarget.mContent.Contains(receivenpc))
                //    addTarget.mContent = addTarget.mContent.Replace(receivenpc, mesData.m_ReplyNpc);

                //if (addTarget.mContent.Contains(itemname))
                //    addTarget.mContent = addTarget.mContent.Replace(itemname, ItemAsset.ItemData.GetName(mesData.m_ItemID));

                if (addTarget.mContent.Contains(itemnum))
                    addTarget.mContent = addTarget.mContent.Replace(itemnum, mesData.m_ItemNum.ToString());
            }
            else if(towData != null)
            {
                addTarget.mContent = towData.m_Desc != "0" ? towData.m_Desc : "";
            }

            stMV.mTargetList.Add(addTarget);
        }

        stMV.MissionDesc = content;
		
        //REWARD
        for (int i = 0; i < data.m_Com_RewardItem.Count; i++)
        {
            ItemSample itemGrid = new ItemSample(data.m_Com_RewardItem[i].id, data.m_Com_RewardItem[i].num);
            if (null == itemGrid.prototypeData)
            {
                Debug.LogError("xxxxxxxxxxxxxxxxxx========================================" + data.m_Com_RewardItem[i].id);
                continue;
            }
            if (null == itemGrid || itemGrid.prototypeData.mEquiSex != 0/* && itemGrid.prototypeData.mEquiSex != m_Player.GetPlayerSex())
                continue;

            if (data.m_Com_RewardItem[i].id > 0)
            {
                stMV.mRewardsList.Add(itemGrid);
            }
        }
		
        //SELREWARD
        for (int i = 0; i < data.m_Com_SelRewardItem.Count; i++)
        {
            ItemSample itemGrid = new ItemSample(data.m_Com_SelRewardItem[i].id, data.m_Com_SelRewardItem[i].num);

            if (itemGrid.prototypeData.mEquiSex != 0/* && itemGrid.prototypeData.mEquiSex != m_Player.GetPlayerSex())
                continue;

            if (data.m_Com_SelRewardItem[i].id > 0)
            {
                stMV.mSelRewardsList.Add(itemGrid);
            }
        }
    }

    public void UpdateMission(int MissionID, bool bComplete = true)
    {
        if(MissionID == MissionManager.m_SpecialMissionID5)
            return ;

        if (MissionID > 0 && MissionID <= (int)PlayerMission.MissionInfo.MAX_MISSION_COUNT)
        {
            if (bComplete)
            {
                if (mMainMissionData.ContainsKey(MissionID))
                    RemoveMainMission(MissionID);
				ClearMissionInfo();
				mMissiontrack.UpdataMissionInfo(MissionID,true);
                return;
            }

            Dictionary<string, string> missionFlagType = m_Player.m_PlayerMission.GetMissionFlagType(MissionID);
            if (missionFlagType == null)
                return;

            if (mMainMissionData.ContainsKey(MissionID))
                RemoveMainMission(MissionID);
            
            MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
            if(data == null)
                return ;

            if (data.IsTalkMission())
                return;

            GetMissionData(data, missionFlagType);

			mMissiontrack.UpdataMissionInfo(MissionID);
        }
        else if (MissionID == -1)
        {
            mMainMissionData.Clear();
			bool find = false;
            foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_Player.m_PlayerMission.m_MissionInfo)
            {
                int id = iter.Key;

				if(mCurrentMissionID == id)
					find = true;

                Dictionary<string, string> missionFlagType = iter.Value;
                
                MissionCommonData data = MissionManager.GetMissionCommonData(id);
                if(data == null)
                    continue;

                if (data.IsTalkMission())
                    continue;

                GetMissionData(data, missionFlagType);
				
				mMissiontrack.UpdataMissionInfo(id);
            }
			if(!find)
				ClearMissionInfo();
        }
		ResetMissionList();
    }
	
	void AddMission(stMissionView view)//int missionId)
	{
		MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
		AddItem.gameObject.name = "missionlist" + mNumCount;
		mNumCount++;

        if(!MissionManager.HasRandomMission(view.MissionID)
            && view.mMissionType == MissionType.MissionType_Main)
            AddItem.transform.parent = mMainMissionTable.transform;
        else
            AddItem.transform.parent = mSubMissionTable.transform;
		AddItem.transform.localPosition = Vector3.zero;
		AddItem.transform.localRotation = Quaternion.identity;
		AddItem.transform.localScale = Vector3.one;
		AddItem.SetMissionTitle(view,this, m_Player);
		mMissionList.Add(AddItem);
	}
	
	void AddTarget(TargetShow target)
	{

	    MissionTargetItem_N AddItem = Instantiate(mPrefabTarget) as MissionTargetItem_N;
	    AddItem.transform.parent = mTargetGrid.transform;
	    AddItem.transform.localPosition = new Vector3(0,0,-1);
	    AddItem.transform.localRotation = Quaternion.identity;
	    AddItem.transform.localScale = Vector3.one;
	    AddItem.SetTarget(target.mContent, target.mIconName);
	    mTargetList.Add(AddItem);
	}
	
	void AddRewards(ItemSample itemGrid)
	{
		Grid_N AddItem = Instantiate(mPrefabGrid_N) as Grid_N;
		AddItem.transform.parent = mRewardsGrid.transform;
		AddItem.transform.localPosition = new Vector3(0,0,-1);
		AddItem.transform.localRotation = Quaternion.identity;
		AddItem.transform.localScale = Vector3.one;
		AddItem.SetItem(itemGrid);
		mRewardsList.Add(AddItem);
	}
	
	void ResetMissionList()
	{
		for(int i=mMissionList.Count-1;i>=0;i--)
		{
			mMissionList[i].transform.parent = null;
			Destroy(mMissionList[i].gameObject);
		}
		mMissionList.Clear();
		
		mNumCount = 0;
		
		foreach(MainMission mainMission in mMainMissionData.Values)
		{
			AddMission(mainMission.mView);
			foreach(int id in mainMission.mSubMission.Keys)
				AddMission(mainMission.mSubMission[id]);
		}
		
		mSubMissionTable.Reposition();
		mMainMissionTable.Reposition();
		mUITable.Reposition();
		Invoke("UpdateTable", 0.05f);
	}
		
	void ResetMissionInfo(stMissionView view)
	{
		for(int i=mTargetList.Count-1;i>=0;i--)
		{
			mTargetList[i].transform.parent = null;
			Destroy(mTargetList[i].gameObject);
		}
		mTargetList.Clear();
		
		for(int i=mRewardsList.Count-1;i>=0;i--)
		{
			mRewardsList[i].transform.parent = null;
			Destroy(mRewardsList[i].gameObject);
		}
		mRewardsList.Clear();
		
		for(int i=0;i<view.mTargetList.Count;i++)
			AddTarget(view.mTargetList[i]);
		mTargetGrid.Reposition();
		
		for(int i=0;i<view.mRewardsList.Count;i++)
			AddRewards(view.mRewardsList[i]);
		mRewardsGrid.Reposition();
		
		mDesLabel.text = view.MissionDesc;
		
		mTargetGrid.Reposition();
		mRewardsGrid.Reposition();
		mMainMissionTable.Reposition();
		mSubMissionTable.Reposition();
		mUITable.Reposition();
		Invoke("UpdateTable", 0.05f);
		
        MissionCommonData data = MissionManager.GetMissionCommonData(view.MissionID);

        if(data == null)
            return ;

        //string strNpc = data.m_Npc;
        //string strReplyNpc = data.m_ReplyNpc;

        if(MissionManager.HasRandomMission(view.MissionID))
        {
            //int rid = System.Convert.ToInt32(data.m_Npc);
            //AiNpcObject npc = NpcManager.Instance.GetNpcRandom(rid);
            //if(npc != null)
            //    strNpc = npc.NpcName;
            //else if(GameConfig.IsMultiMode)
            //{
            //    AiAdNpcNetwork adNpc = NpcManager.Instance.GetMutNpcData(rid);
            //    if (adNpc != null)
            //        strNpc = adNpc.mNpcName;
            //}

            //rid = System.Convert.ToInt32(data.m_ReplyNpc);
            //npc = NpcManager.Instance.GetNpcRandom(rid);
            //if(npc != null)
            //    strReplyNpc = npc.NpcName;
            //else if (GameConfig.IsMultiMode)
            //{
            //    AiAdNpcNetwork adNpc = NpcManager.Instance.GetMutNpcData(rid);
            //    if (adNpc != null)
            //        strReplyNpc = adNpc.mNpcName;
            //}
        }
        
        //mGiverName.text = strNpc == "0" ? "" : strNpc;
        //mSubmitName.text = strReplyNpc == "0" ? "" : strReplyNpc;
		
        //AiNpcObject npcData = NpcManager.Instance.GetNpc(mGiverName.text);
        //if(npcData != null)
        //    mGiverSpr.spriteName = npcData.m_NpcIcon;
        //else
        //    mGiverSpr.spriteName = "EHead";

        //npcData = NpcManager.Instance.GetNpc(mSubmitName.text);
        //if(npcData != null)
        //    mSubmitSpr.spriteName = npcData.m_NpcIcon;
        //else
        //    mSubmitSpr.spriteName = "EHead";
		mGiverSpr.MakePixelPerfect();
		mSubmitSpr.MakePixelPerfect();
		
		if (data.IsTalkMission())
		{
			mOpenMissionBtn.gameObject.SetActive(false);
			mAbortMission.gameObject.SetActive(false);
		}
		else if(mMissiontrack.RemoveList.Contains(view.MissionID))
		{
			mOpenMissionBtn.gameObject.SetActive(true);
			mOpenMissionBtn.isChecked = false;
			mAbortMission.gameObject.SetActive(true);
		}
		else
		{
			mOpenMissionBtn.gameObject.SetActive(true);
			mOpenMissionBtn.isChecked = true;
			mAbortMission.gameObject.SetActive(true);
		}
		
		mMainMissionTable.Reposition();
		mSubMissionTable.Reposition();
		mUITable.Reposition();
		Invoke("UpdateTable", 0.05f);
	}
	
    void OnAbortMission()
    {
        if(mCurrentMissionID == -1)
            return ;

        MissionCommonData data = MissionManager.GetMissionCommonData(mCurrentMissionID);

        if (data == null)
            return;

        if(!data.m_bGiveUp)
            return ;

		mOpMissionID = mCurrentMissionID;
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000066), AbortMission);
    }

	int mOpMissionID;

	void AbortMission()
	{
		if (PlayerFactory.mMainPlayer != null)
			PlayerFactory.mMainPlayer.AbortMission(mOpMissionID);
		mOpMissionID = 0;
	}
	
	void OnActivateMissionTrack(bool active)
	{
        MissionCommonData data = MissionManager.GetMissionCommonData(mCurrentMissionID);

        if (data == null)
            return ;
		mMissiontrack.ChangeMissionTrackState(data, active);
		
		mMainMissionTable.Reposition();
		mSubMissionTable.Reposition();
		mUITable.Reposition();
		Invoke("UpdateTable", 0.05f);
	}
	
	public void OnMissionTrackClose(int missionId)
	{
		OnMutexBtnClick(missionId);
		mOpenMissionBtn.isChecked = false;
	}
	
	public void OnMutexBtnClick(int missionId)
	{
		ActiveWnd();
		mDesSB.scrollValue = 0;
		mTargetSB.scrollValue = 0;
		mRewardSB.scrollValue = 0;
        //if(mCurrentMissionID == missionId)
        //    return;
		mCurrentMissionID = missionId;
		foreach(MainMission mainMission in mMainMissionData.Values)
		{
			if(missionId == mainMission.mView.MissionID)
			{
				ResetMissionInfo(mainMission.mView);
				return;
			}
			else
			{
				foreach(int id in mainMission.mSubMission.Keys)
				{
					if(id == missionId)
					{
						ResetMissionInfo(mainMission.mSubMission[id]);
						return;
					}
				}
			}
		}
		
	}
	
	void ClearMissionInfo()
	{
		for(int i=mTargetList.Count-1;i>=0;i--)
		{
			mTargetList[i].transform.parent = null;
			Destroy(mTargetList[i].gameObject);
		}
		mTargetList.Clear();
		
		for(int i=mRewardsList.Count-1;i>=0;i--)
		{
			mRewardsList[i].transform.parent = null;
			Destroy(mRewardsList[i].gameObject);
		}
		mRewardsList.Clear();
		
		mDesLabel.text = "";
		mGiverName.text = "";
		mSubmitName.text = "";
		mGiverSpr.spriteName = "Null";
		mSubmitSpr.spriteName = "Null";
	}
	
	void SetGiver(string IconName,string name)
	{
		mGiverSpr.spriteName = IconName;
		mGiverSpr.MakePixelPerfect();
		mGiverName.text = name;
	}
	
	void SetSubmit(string IconName,string name)
	{
		mSubmitSpr.spriteName = IconName;
		mSubmitSpr.MakePixelPerfect();
		mSubmitName.text = name;
	}
}
*/