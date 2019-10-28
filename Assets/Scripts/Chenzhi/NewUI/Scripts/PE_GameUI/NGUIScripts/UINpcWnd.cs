using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtTrans;

public class UINpcWnd : UIBaseWnd
{
    public MissionSelItem_N mPrefab;
    public UISprite mHeadSpr;
    public UILabel mNamelabel;
    public UITable mUITable;

    public UITexture mHeadTex;

    List<MissionSelItem_N> mMissionItemList = new List<MissionSelItem_N>();

    public PeEntity m_CurSelNpc;
    List<int> m_MissionList = new List<int>();
    List<int> m_MissionListReply = new List<int>();

    PeEntity m_Player;

    public PeEntity MPlayer { set { m_Player = value; } }

    bool m_bShowShop = false;
    bool m_bShowStorage = false;
    //	bool mRepos = false;

    int mSelIndex = 0;

	PeEntity 	mSayHalotarget;
	bool 		mSayHalo;

    public delegate void OpenEvent(PeEntity npc);
    public event OpenEvent ReviveOpenHandler;

    public void AddOpenEvent(OpenEvent handler)
    {
        ReviveOpenHandler += handler;
    }

    public void DeletOpenEvent(OpenEvent handler)
    {
        ReviveOpenHandler -= handler;
    }

    public void SetCurSelNpc(PeEntity npc, bool sayHalo = false)
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;

        m_Player = PeCreature.Instance.mainPlayer;

        m_CurSelNpc = npc;// NpcManager.Instance.GetNpcScript(name);
        if (null == m_CurSelNpc)
        {
            return;
        }

		if(sayHalo && m_CurSelNpc != mSayHalotarget)
		{
			mSayHalotarget = m_CurSelNpc;
			mSayHalo = true;
		}

        //NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(npc.Id);
        //m_CurSelNpc.SetUserData(nmd);
        NpcMissionData missionData = m_CurSelNpc.GetUserData() as NpcMissionData;
        if (missionData == null)
            return;

        if (missionData.mInFollowMission)
        {
            return;
        }

        m_CurSelNpc.CmdFaceToPoint(PeCreature.Instance.mainPlayer.ExtGetPos());
        m_CurSelNpc.CmdStartTalk();
        StroyManager.Instance.SetTalking(m_CurSelNpc);
        m_CurSelNpc.SayHiRandom();

        UpdateMission();

        ChangeHeadTex(m_CurSelNpc);

        if (ReviveOpenHandler != null)
            ReviveOpenHandler(m_CurSelNpc);
    }

    public void ChangeHeadTex(PeEntity npc)
    {
		mNamelabel.text = npc.ExtGetName();
		if (EntityCreateMgr.Instance.IsRandomNpc(npc))
        {
            mHeadTex.enabled = true;
            mHeadSpr.enabled = false;

            mHeadTex.mainTexture = npc.ExtGetFaceTex();
            if (mHeadTex.mainTexture == null)
            {
                mHeadTex.enabled = false;
                mHeadSpr.enabled = true;
                mHeadSpr.spriteName = npc.ExtGetFaceIcon();
            }
        }
        else
        {
            mHeadTex.enabled = false;
            mHeadSpr.enabled = true;
            mHeadSpr.spriteName = npc.ExtGetFaceIcon();
        }
    }

    public bool CheckAddMissionListID(int id, NpcMissionData missionData)
    {
        CSCreator creator;

        if (id == 0)
            return false;

        if (id == MissionManager.m_SpecialMissionID9
             || MissionManager.HasRandomMission(id))
        {
            for (int m = 0; m < missionData.m_RecruitMissionList.Count; m++)
            {
                if (!MissionManager.Instance.HadCompleteMission(missionData.m_RecruitMissionList[m]))
                    return false;
            }
            if (RMRepository.m_RandomFieldMap.ContainsKey(id))
            {
                if (missionData.mCurComMisNum >= RMRepository.m_RandomFieldMap[id].TargetIDMap.Count)
                    return false;
            }
        }
        else if (id == MissionManager.m_SpecialMissionID59
            || id == MissionManager.m_SpecialMissionID60)
        {
            if (MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID58)
                 && !MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID63))
            {
                creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
                if (creator == null)
                {
                    if (!MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID61))
                    {
                        if (PeGameMgr.IsMulti)
                            MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID67);
                        else
                            MissionManager.Instance.CompleteMission(MissionManager.m_SpecialMissionID67);
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID61, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }

                    return false;
                }

                if (creator.Assembly == null)
                {
                    if (!MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID61))
                    {
                        if (PeGameMgr.IsMulti)
                            MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID67);
                        else
                            MissionManager.Instance.CompleteMission(MissionManager.m_SpecialMissionID67);
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID61, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }

                    return false;
                }

                int npcnum = StroyManager.Instance.GetMgCampNpcCount();
                int creatornum = CSMain.s_MgCreator.GetEmptyBedCnt();
                //若基地空床位<X，则弹出对话床位不足（任务506）
                if (creatornum < npcnum)
                {
                    if (!MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID62))
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID62, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    return false;
                }
                //若基地床位>=X，则在对话后把地球营地所有NPC招募到基地（任务507）
                else
                {
                    if (!MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID63))
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID63, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    return false;
                }

            }
        }

        if (!MissionManager.Instance.IsGetTakeMission(id))
            return false;

        return true;
    }

    bool CheckAddMissionReplyID(int id)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(id);

        if (data == null)
            return false;

        if (id == 9137 || id == 9138)
        {
            if (data.m_iReplyNpc != m_CurSelNpc.Id)
                return false;
        }

        //如果不是谈话任务，那么未接取或已完成的不能交
        if (!data.IsTalkMission())
        {
            if (!MissionManager.Instance.HasMission(id) || MissionManager.Instance.HadCompleteMission(id))
                return false;
        }

        if (MissionRepository.IsAutoReplyMission(id))
            return false;

        if (MissionRepository.GetMissionNpcListName(id, true) == "")
        {
            if (PeGameMgr.IsMulti)
                MissionManager.Instance.RequestCompleteMission(id);
            else
            {
                MissionManager.Instance.CompleteMission(id);
                MissionCommonData mcd = MissionManager.GetMissionCommonData(id);
                if (mcd != null && mcd.m_Type != 0)
                    canShow = false;
            }
            return false;
        }

        return true;
    }

    void AddCSCreatorMission(NpcMissionData missionData)
    {
        //if (GameConfig.IsMultiMode)
        //{
        //    return;
        //}
        CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
        if (creator == null)
            return;

        if (creator.Assembly == null)
            return;

        if (m_CurSelNpc != null)
        {
            if (m_CurSelNpc.Id < 20000 && m_CurSelNpc.Id > 19990)   //冒险模式当中的声望任务id
                return;
        }

        int speMisID = 0;
        if (!m_CurSelNpc.IsRecruited())
        {
            if (!creator.CanAddNpc())
                speMisID = MissionManager.m_SpecialMissionID31;
            else
                speMisID = MissionManager.m_SpecialMissionID16;
        }

        if (missionData.m_CSRecruitMissionList.Count < 1)
        {
            if (speMisID != 0)
                m_MissionList.Add(speMisID);

            return;
        }


        if (missionData.m_CSRecruitMissionList[0] <= 0)
            return;

        for (int i = 0; i < missionData.m_CSRecruitMissionList.Count; i++)
        {
            if (!MissionManager.Instance.HadCompleteMission(missionData.m_CSRecruitMissionList[i]))
                return;
        }

        if (speMisID != 0)
            m_MissionList.Add(speMisID);
    }

    public bool canShow = true;
    public void UpdateMission()
    {
        canShow = true;
        if (m_Player == null)
            return;

        if (m_CurSelNpc == null)
            return;

        m_MissionList.Clear();
        m_MissionListReply.Clear();
        int id = 0;

        m_bShowShop = false;
        m_bShowStorage = false;

        NpcMissionData missionData = m_CurSelNpc.GetUserData() as NpcMissionData;
        if (null == missionData)
            return;

        int npcid = 0;
        if (PeGameMgr.IsStory)
            npcid = m_CurSelNpc.Id;
        else
        {
            if (missionData.m_bRandomNpc)
                npcid = missionData.m_Rnpc_ID;
            else
                npcid = m_CurSelNpc.Id;
        }

        if (!PeGameMgr.IsTutorial)
        {
            StoreData sd = StoreRepository.GetNpcStoreData(npcid);
            if (sd != null && sd.itemList.Count > 0)
                m_bShowShop = true;
        }

        for (int i = 0; i < missionData.m_MissionList.Count; i++)
        {
            id = missionData.m_MissionList[i];

            if (!CheckAddMissionListID(id, missionData))
                continue;

            id = StroyManager.Instance.ParseIDByColony(id);

            m_MissionList.Add(id);
        }
        
        if (!AdQuestChainLimit(AdRMRepository.m_AdRandomGroup[missionData.m_QCID]) && CheckAddMissionListID(missionData.m_RandomMission, missionData))
            m_MissionList.Add(missionData.m_RandomMission);

        for (int i = 0; i < missionData.m_MissionListReply.Count; i++)
        {
            if (!CheckAddMissionReplyID(missionData.m_MissionListReply[i]))
                continue;

            m_MissionListReply.Add(missionData.m_MissionListReply[i]);
        }

        //if (!m_MissionListReply.Contains(missionData.m_RandomMission) && CheckAddMissionReplyID(missionData.m_RandomMission))
        //    m_MissionListReply.Add(missionData.m_RandomMission);

        AddCSCreatorMission(missionData);

        ResetMissionList();
    }

    bool AdQuestChainLimit(AdRandomGroup group)
    {
        if (group.m_preLimit.Count == 0)
            return false;
        if (group.m_requstAll)
        {
            for (int i = 0; i < group.m_preLimit.Count; i++)
            {
                if (!MissionManager.Instance.m_PlayerMission.HadCompleteMissionAnyNum(group.m_preLimit[i]))
                    return true;
            }
            return false;
        }
        else
        {
            for (int i = 0; i < group.m_preLimit.Count; i++)
            {
                if (MissionManager.Instance.m_PlayerMission.HadCompleteMissionAnyNum(group.m_preLimit[i]))
                    return false;
            }
            return true;
        }
    }

    public void GetMutexID(int curMisID, ref List<int> idlist)
    {
        idlist.Clear();
        GameUI.Instance.mNPCTalk.m_selectMissionSource = curMisID;

        MissionCommonData data = MissionManager.GetMissionCommonData(curMisID);

        if (data == null)
            return;

        //判断条件
        if (data.m_GuanLianList.Count == 0)
            return;

        for (int m = 0; m < data.m_GuanLianList.Count; m++)
        {
            //!MissionManager.Instance.HadCompleteMission(data.m_GuanLianList[m]) && !MissionManager.Instance.HasMission(data.m_GuanLianList[m])
            if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(data.m_GuanLianList[m],false))
                idlist.Add(data.m_GuanLianList[m]);
        }

        return;
    }

    void AddMissionItem(int missionId)
    {
        MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
        AddItem.gameObject.name = "ItemName" + mSelIndex;
        mSelIndex++;
        AddItem.transform.parent = mUITable.transform;
        AddItem.transform.localPosition = Vector3.zero;
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;

        string strtitle = MissionRepository.GetMissionNpcListName(missionId, false);
        if (missionId == MissionManager.m_SpecialMissionID9)
        {
            if (EntityCreateMgr.Instance.IsRandomNpc(m_CurSelNpc))
            {
                NpcMissionData missionData = m_CurSelNpc.GetUserData() as NpcMissionData;

                if (null != missionData && (missionData.mCurComMisNum >= missionData.mCompletedMissionCount || UINPCTalk.m_QuickZM))
                {
                    strtitle = "[ffff00]" + strtitle + "[-]";
                }
            }
        }

        AddItem.SetMission(missionId, strtitle, this, m_Player);
        mMissionItemList.Add(AddItem);
    }

    void AddMissionReplyItem(int missionId)
    {
        MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
        AddItem.gameObject.name = "ItemName" + mSelIndex;
        mSelIndex++;
        AddItem.transform.parent = mUITable.transform;
        AddItem.transform.localPosition = Vector3.zero;
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;

        string strtitle = "";
        strtitle = MissionRepository.GetMissionNpcListName(missionId, true);
        AddItem.SetMission(missionId, strtitle, this, m_Player);
        mMissionItemList.Add(AddItem);

    }

    void ShowStorage()
    {
        MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
        AddItem.gameObject.name = "ItemName" + mSelIndex;
        mSelIndex++;
        AddItem.transform.parent = mUITable.transform;
        AddItem.transform.localPosition = Vector3.zero;
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;
        AddItem.SetMission(-3, PELocalization.GetString(8000139), "cangkuarr", this, m_Player);
        mMissionItemList.Add(AddItem);
    }

    void AddShopItem(int missionId)
    {
        MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
        AddItem.gameObject.name = "ItemName" + mSelIndex;
        mSelIndex++;
        AddItem.transform.parent = mUITable.transform;
        AddItem.transform.localPosition = Vector3.zero;
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;
        AddItem.SetMission(missionId, PELocalization.GetString(8000011), "ShopIntalk", this, m_Player);
        mMissionItemList.Add(AddItem);
    }

    void AddCloseItem(int missionId)
    {
        MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
        AddItem.gameObject.name = "ItemName" + mSelIndex;
        mSelIndex++;
        AddItem.transform.parent = mUITable.transform;
        AddItem.transform.localPosition = Vector3.zero;
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;
        AddItem.SetMission(missionId, PELocalization.GetString(8000010), "SubMission", this, m_Player);
        mMissionItemList.Add(AddItem);
    }

    void ResetMissionList()
    {
        mSelIndex = 0;
        for (int i = mMissionItemList.Count - 1; i >= 0; i--)
        {
            mMissionItemList[i].transform.parent = null;
            Destroy(mMissionItemList[i].gameObject);
        }
        mMissionItemList.Clear();

        for (int i = 0; i < m_MissionList.Count; i++)
        {
            if (m_MissionList[i] == MissionManager.m_SpecialMissionID9)
            {
                NpcMissionData missionData = m_CurSelNpc.GetUserData() as NpcMissionData;
                if (null == missionData)
                    return;
                if (missionData.mCurComMisNum < missionData.mCompletedMissionCount)
                    continue;
            }
            AddMissionItem(m_MissionList[i]);
        }
        for (int i = 0; i < m_MissionListReply.Count; i++)
            AddMissionReplyItem(m_MissionListReply[i]);

        if (m_bShowShop)
            AddShopItem(-1);

        if (m_bShowStorage)
            ShowStorage();

        AddCloseItem(-2);

        //		ReposTable();
        mUITable.Reposition();
    }

    void NpcOnColonyRoad() 
    {
        if (GameUI.Instance.mNPCTalk.isPlayingTalk)
            return;
        List<int> talkidList = new List<int>();
        talkidList.Add(4079);
        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(talkidList);
        GameUI.Instance.mNPCTalk.PreShow();
    }

    public override void Show()
    {
        if (!canShow)
            return;
        
        if (null == m_CurSelNpc)
            return;

		if (m_CurSelNpc.NpcCmpt != null && m_CurSelNpc.NpcCmpt.CsBacking)
		{
			NpcOnColonyRoad();
			return;
		}
//#if UNITY_5
//        if (GameUI.Instance.mNPCTalk.isActiveAndEnabled)
//#else
//        if (GameUI.Instance.mNPCTalk.active)
//#endif
//        {
//            m_CurSelNpc.CmdStopTalk();
//            //StroyManager.Instance.RemoveReq(m_CurSelNpc, EReqType.Dialogue);
//            return;
//        }

        //NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(m_CurSelNpc.Id);
        //m_CurSelNpc.SetUserData(nmd);
        NpcMissionData missionData = m_CurSelNpc.GetUserData() as NpcMissionData;
        if (missionData == null)
            return;

        if (missionData.mInFollowMission)
        {
            m_CurSelNpc.CmdStopTalk();
            StroyManager.Instance.RemoveReq(m_CurSelNpc, EReqType.Dialogue);
            return;
        }
        //if (m_CurSelNpc.m_bReturn)
        //    return;

        //		mUITable.Reposition();
        //		base.Show ();
        //        mUITable.Reposition();
        //		Show();

		PlayerStartAudio();
        base.Show();
        //		mUITable.Reposition();
        //		if (mTweener != null)
        //		{
        //			mTweener.Reset();
        //			mTweener.Play(true);
        //		}

        //		mUITable.Reposition();


        //m_CurSelNpc.SetStateType(NpcObject_N.StateType.StateType_Takling);
        //		Invoke("ReposTable", 0.2f);
        //mRepos = true;
    }

    void BaseShow()
    {
        base.Show();
    }

    protected override void OnClose()
    {
        if (m_CurSelNpc != null)
        {
            m_CurSelNpc.SayByeRandom();
        }

		PlayerEndAudio();

        Hide();
    }

    protected override void OnHide()
    {
        base.OnHide();
        if (m_CurSelNpc == null)
            return;
        m_CurSelNpc.CmdFaceToPoint(Vector3.zero);
        m_CurSelNpc.CmdStopTalk();
		
		mSayHalotarget = null;

        //StroyManager.Instance.RemoveReq(m_CurSelNpc, EReqType.Dialogue);
        if ((!GameUI.Instance.mNPCTalk.isPlayingTalk ||
            GameUI.Instance.mNPCTalk.CurTalkInfoIsRadio()) &&
            !GameUI.Instance.mShopWnd.isShopping)
            StroyManager.Instance.RemoveReq(m_CurSelNpc, EReqType.Dialogue);
    }

    public int BtnClickMission = 0;
    public void OnMutexBtnClick(int missionId)
    {
        ActiveWnd();

        if (missionId == -3)//npcstorage
        {
            if (null == m_CurSelNpc)
            {
                Debug.LogError("open storage, but npc is null");
            }

            NpcStorage storage = null;
            if (GameConfig.IsMultiMode)
            {
                storage = NpcStorageMgr.GetStorage(m_Player.Id);
            }
            else
            {
                storage = NpcStorageMgr.GetSinglePlayerStorage();
            }

            if (null == storage)
            {
                Debug.LogError(m_CurSelNpc + " is has no storage.");
            }

            //storage.Open(m_CurSelNpc);
            Hide();
        }

        if (missionId == -1) // shop
        {
            NpcMissionData userdata = m_CurSelNpc.GetUserData() as NpcMissionData;
            if (userdata == null)
                return;

            int npcid = 0;
            if (PeGameMgr.IsStory)
                npcid = m_CurSelNpc.Id;
            else
            {
                if (userdata.m_bRandomNpc)
                    npcid = userdata.m_Rnpc_ID;
                else
                    npcid = m_CurSelNpc.Id;

            }
            if (!GameConfig.IsMultiMode)
            {
                if (GameUI.Instance.mShopWnd.UpdataShop(StoreRepository.GetNpcStoreData(npcid)))
                    GameUI.Instance.mShopWnd.Show();
            }
            else
            {
                // to do-- send msg to server
                //m_CurSelNpc.mNpcId for npc, npcid for store
//                if (!userdata.m_bRandomNpc)
//                {
//                    Debug.Log("nr==null");
//                    return;
//                }
                //m_Player.GetShop(m_CurSelNpc.Id, userdata.m_Rnpc_ID);

                if (null != PlayerNetwork.mainPlayer)
                    PlayerNetwork.mainPlayer.RequestShopData(m_CurSelNpc.Id);
            }
        }
        else if (missionId == -2)
        {
			OnClose();
        }
        else
        {
            if (m_MissionList.Contains(missionId))
            {
                if (!MissionManager.Instance.CheckCSCreatorMis(missionId))
                    return;

                if (!MissionManager.Instance.CheckHeroMis())
                    return;

                if (MissionManager.Instance.IsTempLimit(missionId))
                {
                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4080 });
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4080 }, null, false);
                    Hide();
                    return;
                }

                BtnClickMission = missionId;
                if (MissionRepository.HaveTalkOP(missionId))
                {
                    Hide();
                    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionId, 1);
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    GameUI.Instance.mNPCTalk.PreShow();
                }
                else
                {
                    MissionManager.Instance.SetGetTakeMission(missionId, m_CurSelNpc, MissionManager.TakeMissionType.TakeMissionType_Get);
                    Hide();
                }
                BtnClickMission = 0;
            }
            else if (m_MissionListReply.Contains(missionId))
            {
                if (MissionManager.Instance.IsReplyMission(missionId))
                {
                    if (PeGameMgr.IsMulti)
                        MissionManager.Instance.RequestCompleteMission(missionId);
                    else
                    {
                        MissionManager.Instance.CompleteMission(missionId);
                        UpdateMission();
                    }

                }
                else
                {
                    if (MissionRepository.HaveTalkIN(missionId))
                    {
                        Hide();
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionId, 2);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                }


            }
        }

    }

    void ReposTable()
    {
        gameObject.SetActive(true);
        mUITable.Reposition();
        gameObject.SetActive(false);
    }

	void PlayerStartAudio()
	{
		if(!mSayHalo) return;
		mSayHalo = false;
		if(null == m_CurSelNpc)
			return;
		if(m_CurSelNpc.proto == EEntityProto.Npc)
		{
			NpcProtoDb.Item proto = NpcProtoDb.Get(m_CurSelNpc.ProtoID);
			if(null != proto && null != proto.chart1 && proto.chart1.Length > 0)
				AudioManager.instance.Create(m_CurSelNpc.position + 1.8f * Vector3.up, proto.chart1[Random.Range(0, proto.chart1.Length)], m_CurSelNpc.tr);
		}
	}

	
	void PlayerEndAudio()
	{
		if(null == m_CurSelNpc)
			return;
		if(m_CurSelNpc.proto == EEntityProto.Npc)
		{
			NpcProtoDb.Item proto = NpcProtoDb.Get(m_CurSelNpc.ProtoID);
			if(null != proto && null != proto.chart2 && proto.chart2.Length > 0)
				AudioManager.instance.Create(m_CurSelNpc.position + 1.8f * Vector3.up, proto.chart2[Random.Range(0, proto.chart2.Length)], m_CurSelNpc.tr);
		}
	}
}
