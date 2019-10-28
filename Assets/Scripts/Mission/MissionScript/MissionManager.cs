using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtFollow;
using Pathea.PeEntityExtPlayerPackage;
using ItemAsset;

public class MissionManager : MonoBehaviour
{
    public struct PathInfo
    {
        public Vector3 pos;
        public bool isFinish;
    }

    public const int m_SpecialMissionID5 = 71;      //-
    public const int m_SpecialMissionID9 = 888;     //- ��ļ�ʹ�����
    public const int m_SpecialMissionID10 = 242;    //- �ŹǻҺ�
    public const int m_SpecialMissionID13 = 997;    //-  �������
    public const int m_SpecialMissionID14 = 998;    //-  �������
    public const int m_SpecialMissionID15 = 999;    //-  �������
    public const int m_SpecialMissionID16 = 191;    //-  �л��أ���ļ�ʹ����� ������
    public const int m_SpecialMissionID22 = 66;     //- �����Զ�����������
    public const int m_SpecialMissionID24 = 67;     //- �����Զ�����������
    public const int m_SpecialMissionID31 = 204;    //- �л��أ�û����ӣ�������ļ
    public const int m_SpecialMissionID42 = 212;    //-
    public const int m_SpecialMissionID43 = 158;    //- ��������,������һ������Alfonzo�����ܵ������
    public const int m_SpecialMissionID45 = 8;      //-
    public const int m_SpecialMissionID47 = 889;    //- ��ļ�ʹ�ʧ�ܵ���ʾ
    public const int m_SpecialMissionID51 = 254;
    public const int m_SpecialMissionID52 = 139;
    public const int m_SpecialMissionID53 = 61;
    public const int m_SpecialMissionID55 = 251;
    public const int m_SpecialMissionID58 = 444;
    public const int m_SpecialMissionID59 = 480;
    public const int m_SpecialMissionID60 = 481;
    public const int m_SpecialMissionID61 = 505;
    public const int m_SpecialMissionID62 = 506;
    public const int m_SpecialMissionID63 = 507;
    public const int m_SpecialMissionID64 = 497;
    public const int m_SpecialMissionID65 = 550;
    public const int m_SpecialMissionID66 = 553;    //Ѳ������;�в��ܴ���ؾ�
    public const int m_SpecialMissionID67 = 554;
    public const int m_SpecialMissionID68 = 500;
    public const int m_SpecialMissionID69 = 562;
    public const int m_SpecialMissionID80 = 629;    //- �ŹǻҺ�
    public const int m_SpecialMissionID81 = 628;    //�����ȥȡѪ��������
    public const int m_SpecialMissionID82 = 710;    //���npcѪ��������
    public const int m_SpecialMissionID83 = 678;    //GRV�ɴ�����
    public const int m_SpecialMissionID84 = 700;    //Ѳ������;�в��ܴ���ؾ�
    public const int m_SpecialMissionID85 = 703;    //Ѳ������;�в��ܴ���ؾ�
    public const int m_SpecialMissionID86 = 704;    //Ѳ������;�в��ܴ���ؾ�
    public const int m_SpecialMissionID87 = 697;    //��ˮ������;�в��ܾ�����
    public const int m_SpecialMissionID88 = 714;    //��ˮ������;�в��ܾ�����
    public const int m_SpecialMissionID89 = 822;   //�ϳ����ܷɻ�������
    public const int m_SpecialMissionID90 = 825;   //�ϳ��ɻ��������ؾߵ�����
    public const int m_SpecialMissionID91 = 826;
    public const int m_SpecialMissionID92 = 846;   //�����ɻ���в���̵���������
    public const int m_SpecialMissionID93 = 953;

    public List<int> m_OnCarMissionList;
    public static int m_CurSpecialMissionID = -1;
    public const int m_TalkInfoPlayer = -9999;
    string mNpcName = "";
    string mItemID = "";
    string mItemNum = "";
    string mMisID = "";
    string mMovePos = "";
    string mNPCFollower = "";
    string mAbilityID = "";
    string mSickNum = "";
    string mNpcNum = "";
	string mAItypeId ="";
    string mPlotID = "";
	string mStyleID ="";
	string mStrEntityId = "";
	string mStrEntityPos = "";
	string level = "";
    //public static string mTowerKillNum = "";
    //public static string mTowerMonCount = "";
    public static string mTowerCurWave = "";
    public static string mTowerTotalWave = "";
    public static bool mShowTools = false;
	public static int ToolsPage =1;

    public Dictionary<int, string> npcName = new Dictionary<int, string>();
    public PlayerMission m_PlayerMission = new PlayerMission();
    public bool m_bHadInitMission = false;
    List<int> iHadTalkedMap = new List<int>();

    void Awake()
    {
        mInstance = this;
        m_OnCarMissionList = new List<int>();
        m_OnCarMissionList.Add(553);
        m_OnCarMissionList.Add(699);
        m_OnCarMissionList.Add(700);
        m_OnCarMissionList.Add(701);
        m_OnCarMissionList.Add(702);
        m_OnCarMissionList.Add(703);
        m_OnCarMissionList.Add(704);
    }

    void Start()
    {
        StartCoroutine(WaitingPlayer());
    }

    IEnumerator WaitingPlayer()
    {
        while (PeCreature.Instance.mainPlayer == null || UIMissionMgr.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        RecordRemove();
        PeCreature.Instance.mainPlayer.ExtSetFaceIconBig("npc_big_UnKnown");
		EntityMgr.Instance.eventor.Subscribe(EntityCreateMgr.Instance.NpcMouseEventHandler);
		EntityMgr.Instance.npcTalkEventor.Subscribe(EntityCreateMgr.Instance.NpcTalkRequest);
        if (PeGameMgr.IsAdventure)
        {
            VArtifactTownManager.Instance.RegistTownDestryedEvent(OnTownDestroy);
            ReputationSystem.Instance.onReputationChange += OnReputationChange;
        }

        MonsterHandbookData.GetAllMonsterEvent += AllMonsterBook;
        if (CSMain.s_MgCreator != null)
            CSMain.s_MgCreator.RegisterListener(MoveVploas);

        StroyManager.Instance.InitPlayerEvent();
        if (!PeGameMgr.IsMulti)
            InitPlayerMission();

        if (PeGameMgr.IsSingleStory)
            StroyManager.Instance.InitMission();
        UIMissionMgr.Instance.e_DeleteMission += AbortMission;
        yield return 0;
    }

    void OnTownDestroy(int n)
    {
        AdventureWin();
    }

    void OnReputationChange(int playerId,int targetId)
    {
        AdventureWin();
    }

    void AdventureWin()
    {
        if (!m_PlayerMission.HadCompleteMission(9139) || !m_PlayerMission.HadCompleteMission(9140))
        {
            //lz-2017.05.10 Adventure最后的任务结局改为两个
            int surviveCount = 0;
            int peaceCount = 0;
            int playerId = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
            for (int i = 1; i < RandomMapConfig.allyCount; i++)
            {
                if (VATownGenerator.Instance.GetAllyTownExistCount(i) > 0)
                {
                    surviveCount++;
                    int targetId = VATownGenerator.Instance.GetPlayerId(i);
                    if (ReputationSystem.Instance.GetReputationLevel(playerId, targetId) > ReputationSystem.ReputationLevel.Neutral)
                    {
                        peaceCount++;
                    }
                }
            }

            int missionID = -1;

            if (surviveCount == 0) missionID = 9139;                //征服
            else if (surviveCount == peaceCount) missionID = 9140;  //外交

            if (missionID != -1 && !m_PlayerMission.HadCompleteMission(missionID))
            {
                if (MissionRepository.HaveTalkOP(missionID))
                {
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionID, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else
                        GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionID, 1);
                }
                else if (IsGetTakeMission(missionID))
                    SetGetTakeMission(missionID, null, MissionManager.TakeMissionType.TakeMissionType_Get);
            }
        }
    }

    void AllMonsterBook() 
    {
        if (PeGameMgr.IsMulti)
            MissionManager.Instance.RequestCompleteMission(10035);
        else
            CompleteMission(10035);
    }

    void MoveVploas(int event_type,CSEntity entity) 
    {
        if (event_type ==  CSConst.cetAddEntity && entity != null && entity is CSAssembly)
            ReflashCSUseItemMission();

        if (!StroyManager.Instance.moveVploas)
            return;
        if (event_type != CSConst.cetAddEntity)
            return;
        CSAssembly colony = entity as CSAssembly;
        if (colony == null)
            return;

        PeEntity npc = EntityMgr.Instance.Get(9056);
        if (CSMain.GetAssemblyLogic() != null)
            StroyManager.Instance.Translate(npc, CSMain.GetAssemblyLogic().m_NPCTrans[19].position);
    }

    void RecordRemove() 
    {
        SpecialHatred.ClearRecord();
        StroyManager.ClearRecord();
        foreach (var item in EntityMgr.Instance.All)
        {
            if (item.proto != EEntityProto.Npc)
                continue;
            NpcMissionData missionData = item.GetUserData() as NpcMissionData;
            if (missionData != null)
                missionData.mInFollowMission = false;
        }
    }

    public enum TakeMissionType
    {
        TakeMissionType_Unkown,
        TakeMissionType_Get,
        TakeMissionType_In,
        TakeMissionType_Complete,
    }

    static MissionManager mInstance;
    public static MissionManager Instance
    {
        get
        {
            return mInstance;
        }
    }

    void OnGUI()
    {
#if UNITY_EDITOR		
		if (GUI.Button(new Rect(10, 130 + 80, 70, 20), "FreeCamera")){
			FreeCamera.SetFreeCameraMode();
		}
		if (GUI.Button(new Rect(100, 130 + 80, 70, 20), "ChkAiData")){
			if(MainPlayer.Instance.entity != null){
				Vector3 pos = MainPlayer.Instance.entity.position;
				int typeid = (int)AiUtil.GetPointType (pos);
				int mapid = PeMappingMgr.Instance.GetAiSpawnMapId (new Vector2 (pos.x, pos.z));
				Debug.Log("[ChkAiData]: Pos="+pos+" MapId="+mapid+" TypeId="+typeid);
			}
		}

        if (GUI.Button(new Rect(10, 130 + 100, 70, 20), "ShowTools"))
            mShowTools = !mShowTools;


        if (mShowTools)
        {
			if (GUI.Button(new Rect(100, 130 + 100, 70, 20), "Next"))
			{
				ToolsPage ++;
				if(ToolsPage >2)
					ToolsPage = 1;
			}

           if(ToolsPage ==1)
			{
				GUI.Label(new Rect(10, 130 + 150, 50, 20), "NpcName");
				mNpcName = GUI.TextArea(new Rect(85, 130 + 150, 100, 20), mNpcName);
				if (GUI.Button(new Rect(210, 130 + 150, 100, 20), "MoveEntity"))
				{
					int npcid = Convert.ToInt32(mNpcName);
					PeEntity npc = EntityMgr.Instance.Get(npcid);
					if (npc == null)
						return;
					
					Vector3 pos = PeCreature.Instance.mainPlayer.ExtGetPos();
					pos.z += 2;
					pos.y += 1;

					if(npc.NpcCmpt != null)
						npc.NpcCmpt.Req_Translate(pos);
					else
						npc.ExtSetPos(pos);
				}

                if (GUI.Button(new Rect(390, 130 + 150, 100, 20), "DestroyEntity"))
                {
                    int npcid = Convert.ToInt32(mNpcName);
                    PeEntity npc = EntityMgr.Instance.Get(npcid);
                    if (npc == null)
                        return;

                    PeLogicGlobal.Instance.DestroyEntity(npc.skEntity);
                    //GameObject.Destroy(npc);
                }

				GUI.Label(new Rect(10, 130 + 190, 80, 20), "ItemID");
				mItemID = GUI.TextArea(new Rect(85, 130 + 190, 100, 20), mItemID);
				GUI.Label(new Rect(190, 130 + 190, 80, 20), "ItemNum");
				mItemNum = GUI.TextArea(new Rect(290, 130 + 190, 100, 20), mItemNum);
				if (GUI.Button(new Rect(390, 130 + 190, 70, 20), "AddItem"))
				{
					int itemid = Convert.ToInt32(mItemID);
					int itemnum = Convert.ToInt32(mItemNum);
					if (NetworkInterface.IsClient)
					{
						PlayerNetwork.mainPlayer.RequestAddItem(itemid, itemnum);
						return;
					}
					
					PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
					if (pkg == null)
						return;
					
					pkg.Add(itemid, itemnum);
				}
				
				GUI.Label(new Rect(10, 130 + 230, 50, 20), "MisID");
				mMisID = GUI.TextArea(new Rect(85, 130 + 230, 100, 20), mMisID);
				if (GUI.Button(new Rect(210, 130 + 230, 70, 20), "GetMis"))
				{
					int id = Convert.ToInt32(mMisID);
					MissionCommonData data = MissionManager.GetMissionCommonData(id);
					if (data == null)
						return;
					
					PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
					SetGetTakeMission(id, npc, TakeMissionType.TakeMissionType_Get, false);
				}
				
				GUI.Label(new Rect(10, 130 + 270, 50, 20), "MisID");
				mMisID = GUI.TextArea(new Rect(85, 130 + 270, 100, 20), mMisID);
				if (GUI.Button(new Rect(210, 130 + 270, 70, 20), "ComMis"))
				{                   
                    int id = Convert.ToInt32(mMisID);
					if (PeGameMgr.IsMulti)
						MissionManager.Instance.RequestCompleteMission(id, -1, false);
					else
						MissionManager.Instance.CompleteMission(id, -1, false);
					//if (EntityCreateMgr.DbgUseLegacyCode)
					//{
					//	GameObject go = GameObject.Find("TowerMission");
					//	GameObject.Destroy(go);
					//}
					if (UITowerInfo.Instance != null)
						UITowerInfo.Instance.Hide();
				}

                if (GUI.Button(new Rect(280, 130 + 270, 90, 20), "AbortMis"))
                {                   
                    int id = Convert.ToInt32(mMisID);
                    if (PeGameMgr.IsMulti)
                         PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DeleteMission, id);
                }
                mMovePos = GUI.TextArea(new Rect(10, 130 + 310, 100, 20), mMovePos);
				if (GUI.Button(new Rect(120, 130 + 310, 100, 20), "MovePlayer"))
				{
					string[] tmplist = mMovePos.Split(',');
					if (tmplist.Length != 3)
						return;
					
					float x = Convert.ToSingle(tmplist[0]);
					float y = Convert.ToSingle(tmplist[1]);
					float z = Convert.ToSingle(tmplist[2]);
					
					Vector3 movepos = new Vector3(x, y, z);
					PeTrans view = PeCreature.Instance.mainPlayer.peTrans;
					if (view == null)
						return;
					
					view.position = movepos;
				}
				
				GUI.Label(new Rect(10, 130 + 350, 200, 20), "TDWave : " + mTowerCurWave + " \\ " + mTowerTotalWave);
				
				mNPCFollower = GUI.TextArea(new Rect(10, 130 + 390, 100, 20), mNPCFollower);
				if (GUI.Button(new Rect(120, 130 + 390, 100, 20), "mNPCFollower"))
				{
					int n = Convert.ToInt32(mNPCFollower);
					if (NpcMissionDataRepository.dicMissionData.ContainsKey(n))
					{
						ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                        //NpcMissionDataRepository.dicMissionData[n].m_MissionList.Contains(MissionManager.m_SpecialMissionID9) && 
						if (leader.GetServantNum() < ServantLeaderCmpt.mMaxFollower)
						{
							PeEntity npc = EntityMgr.Instance.Get(n);
							
							NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
							if (missionData == null)
								return;
							
							npc.SetBirthPos(npc.position);
							npc.CmdStopTalk();
							npc.Recruit();
							
							StroyManager.Instance.RemoveReq(npc, EReqType.Dialogue);
							NpcCmpt sc = npc.NpcCmpt;
							if (sc != null)
							{
								if (leader != null)
								{
									CSMain.SetNpcFollower(npc);
								}
							}
							npc.SetShopIcon(null);
						}
					}
				}
				
				GUI.Label(new Rect(10, 130 + 430, 50, 20), "AbilityID");
				mAbilityID = GUI.TextArea(new Rect(85, 130 + 430, 100, 20), mAbilityID);
				if (GUI.Button(new Rect(210, 130 + 430, 100, 20), "AddAbility"))
				{
					if (mAbilityID == "")
						return;
					int AblityId = Convert.ToInt32(mAbilityID);
					
					if (mNpcName == "")
						return;
					int NpcId = Convert.ToInt32(mNpcName);
					
					PeEntity npc = EntityMgr.Instance.Get(NpcId);
					if (npc == null)
						return;
					
					NpcCmpt npcCmpt = npc.GetCmpt<NpcCmpt>();
					if (npcCmpt == null)
						return;
					
					npcCmpt.AddAbility(AblityId);
					mNpcName = "";
					mAbilityID = "";
				}
				
				if (GUI.Button(new Rect(310, 130 + 430, 100, 20), "RemoveAbility"))
				{
					if (mAbilityID == "")
						return;
					int AblityId = Convert.ToInt32(mAbilityID);
					
					if (mNpcName == "")
						return;
					int NpcId = Convert.ToInt32(mNpcName);
					
					PeEntity npc = EntityMgr.Instance.Get(NpcId);
					if (npc == null)
						return;
					
					NpcCmpt npcCmpt = npc.GetCmpt<NpcCmpt>();
					if (npcCmpt == null)
						return;
					
					if (npcCmpt.RemoveAbliy(AblityId))
					{
						mAbilityID = "";
					}
					
					
				}

                if (GUI.Button(new Rect(10, 130 + 510, 70, 20), "DienShip0"))
                {
                    //PlayerNetwork.MainPlayer.RPCServer(EPacketType.PT_InGame_DeleteMission, 935);
                    transPoint = new Vector3(14798.09f, 20.98818f, 8246.396f);
                    yirdName = "DienShip0";
                    SceneTranslate();
                }
                if (GUI.Button(new Rect(10, 130 + 550, 70, 20), "L1Ship"))
				{
					transPoint = new Vector3(9649.354f, 90.488f, 12744.77f);
					yirdName = "L1Ship";
					SceneTranslate();
				}
				if (GUI.Button(new Rect(85, 130 + 510, 70, 20), "MainLand"))
				{
                    transPoint = new Vector3(14819.54f, 106.1666f, 8347.545f);                    
                    yirdName = "main";
					MessageBox_N.ShowYNBox(PELocalization.GetString(82209002), SceneTranslate, null);
				}
				if (GUI.Button(new Rect(85, 130 + 550, 70, 20), "MainLand"))
				{
					transPoint = new Vector3(9684.722f, 368.9954f, 12795.33f);
					yirdName = "main";
					MessageBox_N.ShowYNBox(PELocalization.GetString(82209002), SceneTranslate, null);
				}
				
				GUI.Label(new Rect(10, 130 + 590, 50, 20), "NpcNum");
				mNpcNum = GUI.TextArea(new Rect(85, 130 + 590, 70, 20), mNpcNum);
				if (GUI.Button(new Rect(160, 130 + 590, 80, 20), "TestMeeting"))
				{
					int n = Convert.ToInt32(mNpcNum);
					StroyManager.Instance.TestMeetingPos(n);
				}
				
				GUI.Label(new Rect(10, 130 + 470, 50, 20), "mSickNum");
				mSickNum = GUI.TextArea(new Rect(85, 130 + 470, 100, 20), mSickNum);
				if (GUI.Button(new Rect(210, 130 + 470, 100, 20), "AddSick"))
				{
					if (mSickNum == "")
						return;
					int SickNum = Convert.ToInt32(mSickNum);
					
					if (mNpcName == "")
						return;
					int NpcId = Convert.ToInt32(mNpcName);
					
					PeEntity npc = EntityMgr.Instance.Get(NpcId);
					if (npc == null)
						return;
					
					NpcCmpt npcCmpt = npc.GetCmpt<NpcCmpt>();
					if (npcCmpt == null)
						return;
					
					npcCmpt.AddSick((PEAbnormalType)SickNum);
					mNpcName = "";
					mSickNum = "";
				}
			}

			if(ToolsPage == 2)
			{
				GUI.Label(new Rect(10, 130 + 150, 50, 20), "AItypeId");
				mAItypeId = GUI.TextArea(new Rect(85, 130 + 150, 70, 20), mAItypeId);
				if (GUI.Button(new Rect(160, 130 + 150, 80, 20), "Addto"))
				{
					
					if(mAItypeId == "")
						return ;
					
					if (mNpcName == "")
						return;
					
					int TypeId =  Convert.ToInt32(mAItypeId);
					int NpcId = Convert.ToInt32(mNpcName);
					
					PeEntity npc = EntityMgr.Instance.Get(NpcId);
					if (npc == null)
						return;
					
					NpcCmpt npcCmpt = npc.GetCmpt<NpcCmpt>();
					if (npcCmpt == null)
						return;
					
					npcCmpt.NpcControlCmdId = TypeId;
					mNpcName = "";
					mAItypeId = "";
				}

                GUI.Label(new Rect(10, 130 + 190, 50, 20), "PlotID");
				mPlotID = GUI.TextArea(new Rect(85, 130 + 190, 100, 20), mPlotID);
                if (GUI.Button(new Rect(190, 130 + 190, 100, 20), "TriggerPlot"))
                    StroyManager.Instance.PushStoryList(new List<int>(Array.ConvertAll<string, int>(mPlotID.Split(','), ite => Convert.ToInt32(ite))));

				GUI.Label(new Rect(10, 130 + 230, 50, 20), "MotionStyle");
				mNpcName = GUI.TextArea(new Rect(85, 130 + 230, 50, 20), mNpcName);
				mStyleID = GUI.TextArea(new Rect(160, 130 + 230, 50, 20), mStyleID);
				if (GUI.Button(new Rect(250, 130 + 230, 120, 20), "ChangeStyle"))
				{
					int npcid = Convert.ToInt32(mNpcName);
					PeEntity npc = EntityMgr.Instance.Get(npcid);
					if (npc == null)
						return;

					NpcCmpt npcCmpt = npc.GetCmpt<NpcCmpt>();
					if (npcCmpt == null)
						return;

					int styleId =  Convert.ToInt32(mStyleID);

					if(styleId >0)
					  npcCmpt.MotionStyle = (ENpcMotionStyle)styleId;
				}

                if (GUI.Button(new Rect(10, 130 + 270, 70, 20), "PajaShip"))
                {
                    transPoint = new Vector3(1593.278f, 149.051f, 8021.335f);
                    yirdName = "PajaShip";
                    SceneTranslate();
                } 
                if (GUI.Button(new Rect(85, 130 + 270, 70, 20), "MainLand"))
                {
                    transPoint = new Vector3(1570, 118, 8024);
                    yirdName = "main";
                    MessageBox_N.ShowYNBox(PELocalization.GetString(82209002), SceneTranslate, null);
                }

				mStrEntityId = GUI.TextArea(new Rect(10, 130 + 310, 50, 20), mStrEntityId);
				mStrEntityPos = GUI.TextArea(new Rect(75, 130 + 310, 150, 20), mStrEntityPos);
				if (GUI.Button(new Rect(250, 130 + 310, 70, 20), "Move Entity"))
				{
					int id;
					PeEntity e;
					if(int.TryParse(mStrEntityId, out id) && (e = EntityMgr.Instance.Get(id)) != null){
//						Vector3 pos;
						string[] tmplist = mStrEntityPos.Split(',');
						if (tmplist.Length != 3)
							return;

						_entityToSet = e;
						_posToSet = new Vector3(Convert.ToSingle(tmplist[0]),
						                        Convert.ToSingle(tmplist[1]),
						                        Convert.ToSingle(tmplist[2]));
					}
				}
                if (GUI.Button(new Rect(10, 130 + 350, 70, 20), "LaunchCenter"))
                {
                    transPoint = new Vector3(1607, 158, 10411);
                    yirdName = "LaunchCenter";
                    SceneTranslate();
                }
                if (GUI.Button(new Rect(85, 130 + 350, 70, 20), "MainLand"))
                {
                    transPoint = new Vector3(1888, 267, 10392);
                    yirdName = "main";
                    MessageBox_N.ShowYNBox(PELocalization.GetString(82209002), SceneTranslate, null);
                }
				if (GUI.Button(new Rect(10, 130+390, 100, 20), "ShowAllTowns"))
				{
					VArtifactUtil.ShowAllTowns();
				}
				if (GUI.Button(new Rect(115, 130+390, 110, 20), "RemoveAllTowns"))
				{
					VArtifactUtil.RemoveAllTowns();
				}
				if (GUI.Button(new Rect(10, 130+430, 50, 20), "Rain!"))
				{
					PeEnv.SetBaseRain(0.55f);
				}
				if (GUI.Button(new Rect(65, 130+430, 100, 20), "RevertRain"))
				{
					PeEnv.SetBaseRain(0);
				}
				
				GUI.Label(new Rect(10, 130 + 470, 35, 20), "Level");
				level = GUI.TextArea(new Rect(45, 130+470, 20, 20), level);
				if (GUI.Button(new Rect(65, 130+470, 70, 20), "DunGen"))
				{
					int levelId;
					if(int.TryParse(level,out levelId))
					{
						if((levelId>=1&&levelId<=10)||levelId>=DungeonConstants.TASK_LEVEL_START)
							RandomDungenMgr.Instance.GenTestEntrance(levelId);
						else
							RandomDungenMgr.Instance.GenTestEntrance();
					}
//					Debug.Log(VATownGenerator.Instance.GetRandomExistEnemyPlayerId());
				}
			}

        }
#endif
    }
	PeEntity _entityToSet = null;
	Vector3 _posToSet;
	bool _bUseSetDirty = false;
	void LateUpdate()
	{
		if (Input.GetKey (KeyCode.F11))
			_bUseSetDirty = false;
		if (Input.GetKey (KeyCode.F12))
			_bUseSetDirty = true;

		if (_entityToSet != null) {
			_entityToSet.peTrans.position = _posToSet;
			if(_bUseSetDirty)	SceneMan.SetDirty(_entityToSet.lodCmpt);
			_entityToSet = null;
		}
	}

    public Vector3 transPoint;
    public string yirdName;

    private void ChangeDoodadShowVar(int n, bool tmp)
    {
        SceneDoodadLodCmpt doodadCmpt;
        PeEntity[] doodad = EntityMgr.Instance.GetDoodadEntities(n);
        if (doodad.Length > 0)
        {
            doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
            if (doodadCmpt != null)
                doodadCmpt.SetShowVar(tmp);
        }
    }

	public void TransPlayerAndMissionFollower(Vector3 pos){
		PeTrans view = PeCreature.Instance.mainPlayer.peTrans;
		if (view == null)
			return;
		if(!PeGameMgr.IsSingle)
            view.position = pos;
        else
		    view.fastTravelPos = pos;
		for (int i = 0; i < m_PlayerMission.followers.Count; i++)
		{
			if (m_PlayerMission.followers[i] == null)
				continue;
			m_PlayerMission.followers[i].GetComponent<PeEntity>().ExtSetPos(pos);
		}
	}
	public void TransMissionFollower(Vector3 pos){
		for (int i = 0; i < m_PlayerMission.followers.Count; i++)
		{
			if (m_PlayerMission.followers[i] == null)
				continue;
			m_PlayerMission.followers[i].GetComponent<PeEntity>().ExtSetPos(pos);
		}
	}
    public void SceneTranslate()
    {
		TransPlayerAndMissionFollower(transPoint);
		if (PeGameMgr.IsSingle&&yirdName!=AdventureScene.Dungen.ToString())
        {
            SinglePlayerTypeLoader singlePlayerTypeLoader = SinglePlayerTypeArchiveMgr.Instance.singleScenario;
            singlePlayerTypeLoader.SetYirdName(yirdName);
        }

        if (PeGameMgr.IsSingle)
        {
            if (yirdName == "main")
            {
                ChangeDoodadShowVar(242, true);
                ChangeDoodadShowVar(240, true);
                ChangeDoodadShowVar(324, false);
                ChangeDoodadShowVar(326, true);
                ChangeDoodadShowVar(327, true);

                for (int i = 461; i < 464; i++)
                    ChangeDoodadShowVar(i, false);

                for (int i = 456; i < 461; i++)
                    ChangeDoodadShowVar(i, true);

                //for (int i = 335; i < 342; i++)
                //    ChangeDoodadShowVar(i, true);
            }
            else if (yirdName == AdventureScene.MainAdventure.ToString())
            {

            }
            else if (yirdName == AdventureScene.Dungen.ToString())
            {

            }
            else
            {
                ChangeDoodadShowVar(242, false);
                ChangeDoodadShowVar(240, false);
                ChangeDoodadShowVar(324, true);
                ChangeDoodadShowVar(326, false);
                ChangeDoodadShowVar(327, false);

                for (int i = 461; i < 464; i++)
                    ChangeDoodadShowVar(i, true);

                for (int i = 456; i < 461; i++)
                    ChangeDoodadShowVar(i, false);

                //for (int i = 335; i < 342; i++)
                //    ChangeDoodadShowVar(i, false);
            }
        }

        if (PeGameMgr.IsSingle)
        {
			PeGameMgr.targetYird = yirdName;
            Pathea.PeGameMgr.loadArchive = ArchiveMgr.ESave.Auto1;
            PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
        }
        else
        {
            int scene = 0;            
            if (yirdName == "main")
                scene = 0;
            else if (yirdName == "L1Ship")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.L1Ship;
            }

            else if (yirdName == "DienShip0")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.DienShip0;
            }
            else if (yirdName == "DienShip1")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.DienShip1;
            }
            else if (yirdName == "DienShip2")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.DienShip2;
            }
            else if (yirdName == "DienShip3")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.DienShip3;
            }
            else if (yirdName == "DienShip4")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.DienShip4;
            }
            else if (yirdName == "DienShip5")
            {
                scene = (int)Pathea.SingleGameStory.StoryScene.DienShip5;
            }
            else if(yirdName == "PajaShip")
			{				
				scene = (int)Pathea.SingleGameStory.StoryScene.PajaShip;
			}
            else if (yirdName == "LaunchCenter")
            {                
                scene = (int)Pathea.SingleGameStory.StoryScene.LaunchCenter;
            }
            PlayerNetwork.mainPlayer.RequestChangeScene(scene);
        }
    }

    public static bool IsTalkMission(int MissionID)
    {
        MissionCommonData data = GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        if (data.IsTalkMission())
            return true;

        return false;
    }

    public static List<PeEntity> GetInteractiveEntity() 
    {
        List<PeEntity> result = new List<PeEntity>();
        foreach (var item in Instance.m_PlayerMission.followers)
            result.Add(item.Entity);
        foreach (var item in Instance.m_PlayerMission.pathFollowers) 
        {
            if (!result.Contains(item.Entity))
                result.Add(item.Entity);
        }

        foreach (var item in Instance.m_PlayerMission.m_MissionInfo)
        {
            if(GetMissionCommonData(item.Key).m_iReplyNpc != 0)
            {
                if (result.Find(e => e.Id == GetMissionCommonData(item.Key).m_iReplyNpc) == null)
                    result.Add(EntityMgr.Instance.Get(GetMissionCommonData(item.Key).m_iReplyNpc));
            }
        }
        return result;
    }

    public static MissionCommonData GetMissionCommonData(int MissionID)
    {
        int rid = MissionID / 1000;

        if (rid == 9)
        {
            if (PeGameMgr.IsStory)
                return RMRepository.GetRandomMission(MissionID);
            else
                return AdRMRepository.GetAdRandomMission(MissionID);
        }

        return MissionRepository.GetMissionCommonData(MissionID);
    }

    public static bool IsMainMission(int missionID) 
    {
        MissionCommonData data = GetMissionCommonData(missionID);
        if (data == null)
            return false;
        if (data.m_Type != MissionType.MissionType_Main)
            return false;
        return true;
    }

    public static bool CanDragAssembly(Vector3 pos,out int num)
    {
        num = -1;
        int maxDis = 1600;
        int midDis = 800;
        int minDis = 400;
#if UNITY_EDITOR
        maxDis = 50;
        midDis = 25;
        minDis = 10;
#endif
        if (PeGameMgr.IsStory) 
        {
            MapMaskData mmd = MapMaskData.s_tblMaskData.Find(ret => ret.mId == 29);
            if (mmd == null)
                return true;
            float dis = Vector3.Distance(pos, mmd.mPosition);
            if (dis < maxDis)
            {
                if (dis > midDis)
                    num = 0;
                else if (dis > minDis)
                    num = 1;
                else
                    num = 2;
                return false;
            }
            return true;
        }
        else
            return true;

    }

    public static bool HasRandomMission(int MissionID)
    {
        if (PeGameMgr.IsStory)
            return RMRepository.HasRandomMission(MissionID);
        else
        {
            //lz-2017.05.11 9139 和 9140 任务是特殊任务，需要特殊处理
            if (MissionID == 9135 || MissionID == 9136 || MissionID == 9137 || MissionID == 9138 || MissionID == 9139 || MissionID == 9140)
                return false;
            return AdRMRepository.HasAdRandomMission(MissionID);
        }
    }

    public static TypeMonsterData GetTypeMonsterData(int TargetID)
    {
        if (PeGameMgr.IsStory)
            return MissionRepository.GetTypeMonsterData(TargetID);
        else
            return AdRMRepository.GetAdTypeMonsterData(TargetID);
    }

    public static TypeCollectData GetTypeCollectData(int TargetID)
    {
        if (PeGameMgr.IsStory)
            return MissionRepository.GetTypeCollectData(TargetID);
        else
            return AdRMRepository.GetAdTypeCollectData(TargetID);
    }

    public static TypeFollowData GetTypeFollowData(int TargetID)
    {
        if (PeGameMgr.IsStory)
            return MissionRepository.GetTypeFollowData(TargetID);
        else
            return AdRMRepository.GetAdTypeFollowData(TargetID);
    }

    public static TypeSearchData GetTypeSearchData(int TargetID)
    {
        if (PeGameMgr.IsStory || PeGameMgr.IsTutorial)
            return MissionRepository.GetTypeSearchData(TargetID);
        else
            return AdRMRepository.GetAdTypeSearchData(TargetID);
    }

    public static TypeUseItemData GetTypeUseItemData(int TargetID)
    {
        if (PeGameMgr.IsStory)
            return MissionRepository.GetTypeUseItemData(TargetID);
        else
            return AdRMRepository.GetAdTypeUseItemData(TargetID);
    }

    public static TypeMessengerData GetTypeMessengerData(int TargetID)
    {
        if (PeGameMgr.IsStory)
            return MissionRepository.GetTypeMessengerData(TargetID);
        else
            return AdRMRepository.GetAdTypeMessengerData(TargetID);
    }

    public static TypeTowerDefendsData GetTypeTowerDefendsData(int TargetID)
    {
        if (PeGameMgr.IsStory)
            return MissionRepository.GetTypeTowerDefendsData(TargetID);
        else
            return AdRMRepository.GetAdTypeTowerDefendsData(TargetID);
    }

#region PlayerMission
    public void RemoveFollowTowerMission() 
    {
        List<int> result = new List<int>();
        foreach (var item in m_PlayerMission.m_MissionInfo.Keys)
        {
            bool tmp = false;
            MissionCommonData data = GetMissionCommonData(item);
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                TargetType type = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (type == TargetType.TargetType_Follow || type == TargetType.TargetType_TowerDif)
                {
                    tmp = true;
                    break;
                }
            }
            if (tmp)
                result.Add(item);
        }
        foreach (var item in result)
        {
            m_PlayerMission.FailureMission(item);

            //lw:塔防时，玩家死亡取消复活传送会导致塔防任务失败（9112：建设基地任务完成可以重新激活该塔防），
            if(item == 9113)
            {
                m_PlayerMission.SetQuestVariable(9112, PlayerMission.MissionFlagStep, "0", true,true);
            }
        }
           
    }

    public void InitPlayerMission()
    {
        m_bHadInitMission = false;
        if(PeGameMgr.IsSingleAdventure)
            m_PlayerMission.adId_entityId[1] = 20008;
		GetSpecialItem.ClearLootSpecialItemRecord();
        
        foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_PlayerMission.m_RecordMisInfo)
        {
            MissionCommonData data;
            data = MissionManager.GetMissionCommonData(iter.Key);
            if (data == null)
                continue;

            foreach (KeyValuePair<string, string> ite in iter.Value)
            {
                if (ite.Key == PlayerMission.MissionFlagStep)
                {
                    SetQuestVariable(iter.Key, ite.Key, ite.Value, true, true);
                    Debug.Log("InitPlayerMission��ID��" + iter.Key);

                    for (int i = 0; i < data.m_TargetIDList.Count; i++)
                    {
                        TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                        if (curType == TargetType.TargetType_Unkown)
                            continue;
                        if (curType == TargetType.TargetType_KillMonster)
                        {
                            TypeMonsterData monData = GetTypeMonsterData(data.m_TargetIDList[i]);
                            if (monData == null)
                                continue;
                            int idx = i * 10;
                            if (monData.m_destroyTown)
                            {
                                VArtifactTownManager.Instance.RegistTownDestryedEvent(delegate (int n)
                                {
                                    if (n != monData.m_campID[0])
                                        return;
                                    int townNum = VATownGenerator.Instance.GetAllyTownDestroyedCount(monData.m_campID[0]);
                                    string townValue = string.Format("-1_{0}", townNum);
                                    ModifyQuestVariable(data.m_ID, PlayerMission.MissionFlagMonster + idx, townValue);
                                    if(PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null)
                                    {
                                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyMissionFlag, data.m_ID, PlayerMission.MissionFlagMonster + idx, townValue);
                                    }
                                    CompleteTarget(monData.m_TargetID, data.m_ID);
                                });
                            }
                        }
                    }
                }
                else
                {
                    if (PeGameMgr.IsMultiStory)
                    {
                        Dictionary<string, string> missionFlagType = m_PlayerMission.GetMissionFlagType(iter.Key);
                        if (missionFlagType == null)
                            SetQuestVariable(iter.Key, ite.Key, ite.Value, true, true);
                    }
                    ModifyQuestVariable(iter.Key, ite.Key, ite.Value);
                }                    
            }
            if(data.m_TargetIDList.Find(ite => MissionRepository.GetTargetType(ite) == TargetType.TargetType_Collect) != 0)
                UpdateMissionTrack(data.m_ID);
        }

        if (PeGameMgr.IsAdventure)
        {
            foreach (var item in m_PlayerMission.m_MissionInfo.Keys)
            {
                MissionCommonData data = MissionManager.GetMissionCommonData(item);
                if (!data.creDungeon.effect)
                    continue;
                if (!m_PlayerMission.adId_entityId.ContainsKey(data.creDungeon.npcID))
                    continue;
                PeEntity npc = EntityMgr.Instance.Get(m_PlayerMission.adId_entityId[data.creDungeon.npcID]);
                if (npc == null)
                    continue;
                Vector3 referToPos = npc.position;
                Vector2 rand = UnityEngine.Random.insideUnitCircle.normalized * data.creDungeon.radius;
                Vector2 onCircle = new Vector2(referToPos.x + rand.x, referToPos.z + rand.y);
                IntVector2 xzpos = new IntVector2((int)onCircle.x, (int)onCircle.y);
                xzpos = m_PlayerMission.AvoidTownPos(xzpos);

                if (RandomDungenMgr.Instance == null)
					RandomDungenMgrData.AddInitTaskEntrance(xzpos, data.creDungeon.dungeonLevel);//m_PlayerMission.AvoidTownPos(crePos), data.creDungeon.dungeonLevel);
                else
					RandomDungenMgr.Instance.GenTaskEntrance(xzpos, data.creDungeon.dungeonLevel);//m_PlayerMission.AvoidTownPos(new IntVector2(crePos.x,crePos.z)), data.creDungeon.dungeonLevel);
            }
        }

        //if (m_PlayerMission.m_MissionInfo.Count > 0)
        //    GameGui_N.Instance.mMissionTrackGui.AwakeWindow();

        m_bHadInitMission = true;

        CheckAllGetableMission();
        PlotLensAnimation.IsPlaying = false;
        //if (PeGameMgr.IsAdventure)
        //    CheckAdAllGetableMission();
        if (PeGameMgr.IsMultiStory)
            if (GameUI.Instance != null && GameUI.Instance.mNpcWnd != null)
                GameUI.Instance.mNpcWnd.UpdateMission();
    }

    public bool HasMission(int MissionID)
    {
        return m_PlayerMission.HasMission(MissionID);
    }

    public bool HadCompleteTarget(int TargetID)
    {
        return m_PlayerMission.HadCompleteTarget(TargetID);
    }

    public bool HadCompleteMission(int MissionID)
    {
        return m_PlayerMission.HadCompleteMission(MissionID);
    }

    //lz-2016.07.12
    public bool HadCompleteMissionAnyNum(int MissionID)
    {
        return m_PlayerMission.HadCompleteMissionAnyNum(MissionID);
    }

    public bool IsGetTakeMission(int MissionID)
    {
        return m_PlayerMission.IsGetTakeMission(MissionID);
    }

    public bool IsReplyTarget(int MissionID, int TargetID)
    {
        return m_PlayerMission.IsReplyTarget(MissionID, TargetID);
    }

    public bool IsReplyMission(int MissionID)
    {
        return m_PlayerMission.IsReplyMission(MissionID);
    }

    public string GetQuestVariable(int MissionID, string MissionFlag)
    {
        return m_PlayerMission.GetQuestVariable(MissionID, MissionFlag);
    }

    public int GetTowerDefineKillNum(int MissionID)
    {
        return m_PlayerMission.GetTowerDefineKillNum(MissionID);
    }

    public int SetQuestVariable(int MissionID, string MissionFlag, string MissionValue, bool pushStory = true, bool isRecord = false)
    {
        return m_PlayerMission.SetQuestVariable(MissionID, MissionFlag, MissionValue, pushStory, isRecord);
    }

    public bool ModifyQuestVariable(int MissionID, string MissionFlag, string MissionValue)
    {
        return m_PlayerMission.ModifyQuestVariable(MissionID, MissionFlag, MissionValue);
    }

    public void CompleteTarget(int TargetID, int MissionID, bool forceComplete = false, bool bFromNet = false,bool isOwner = true)
    {
        m_PlayerMission.CompleteTarget(TargetID, MissionID, forceComplete, bFromNet, isOwner);
    }

    public void CompleteMission(int MissionID, int TargetID = -1, bool bCheck = true,bool pushStory = true)
    {
        m_PlayerMission.CompleteMission(MissionID, TargetID, bCheck, pushStory);
    }

    public void AbortMission(UIMissionMgr.MissionView misView)
    {
        if (misView == null)
            return;

        MissionCommonData data = MissionManager.GetMissionCommonData(misView.mMissionID);

        if (data == null)
            return;

        if (!data.m_bGiveUp)
            return;

        m_PlayerMission.AbortMission(misView.mMissionID);
    }

    public void FailureMission(int MissionID)
    {
        // bDeleteMissioning = true;
        m_PlayerMission.FailureMission(MissionID);
        // bDeleteMissioning = false;
    }

	public int HasFollowMissionNet()
	{
		MissionCommonData data;
		foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_PlayerMission.m_MissionInfo)
		{
			int id = iter.Key;
			
			data = MissionManager.GetMissionCommonData(id);
			
			if (data == null)
				continue;
			
			for (int i = 0; i < data.m_TargetIDList.Count; i++)
			{
				TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
				if (curType != TargetType.TargetType_Follow)
					continue;
				
				TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
				if (folData == null)
					continue;
                if (folData.m_EMode == 1)
                    continue;
                if (PeGameMgr.IsMultiStory)
				{
					for (int m = 0; m < folData.m_iNpcList.Count; m++)
					{
						NetworkInterface net = NetworkObject.Get(folData.m_iNpcList[m]);
						if(net != null)
						{
							if((net as AiAdNpcNetwork) != null)
							{
								NpcCmpt cmpt = (net as AiAdNpcNetwork).npcCmpt;
								if(cmpt != null)
								{
									if(cmpt.GetFollowTargetId() == PlayerNetwork.mainPlayerId && cmpt.Net.hasOwnerAuth)
									{
										return id;
									}
								}
							}
						}
					}
				}
				else
					return id;
			}
		}
		
		return -1;
	}

    public int HasFollowMission()
    {
        MissionCommonData data;
        foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_PlayerMission.m_MissionInfo)
        {
            int id = iter.Key;

            data = MissionManager.GetMissionCommonData(id);

            if (data == null)
                continue;

            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType != TargetType.TargetType_Follow)
                    continue;

                TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                if (folData == null)
                    continue;

                if (folData.m_EMode == 1)
                    continue;

                return id;
            }
        }
        return -1;
    }

    public bool HasTowerDifMission()
    {
        MissionCommonData data;
        int id;
        foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_PlayerMission.m_MissionInfo)
        {
            id = iter.Key;

            data = MissionManager.GetMissionCommonData(id);

            if (data == null)
                continue;

            if (MissionRepository.HasTargetType(id, TargetType.TargetType_TowerDif))
            {
                if(PeGameMgr.IsSingle)
                    return true;
                else
                {
                    List<AiTowerDefense> towerdl = NetworkInterface.Get<AiTowerDefense>();
                    if (towerdl != null)
                    {
                        foreach(AiTowerDefense item in towerdl)
                        {
                            if (item != null && item.hasOwnerAuth)
                                return true;
                        }
                    }
                }
            }
                
        }

        return false;
    }

    public void RequestCompleteMission(int MissionID, int TargetID = -1, bool bChcek = true)
    {
        m_PlayerMission.RequestCompleteMission(MissionID, TargetID, bChcek);
    }

    public void RequestDeleteMission(int MissionID)
    {
        m_PlayerMission.ReplyDeleteMission(MissionID);
    }

    public void ProcessMonsterDead(int proid, int autoid)
    {
        m_PlayerMission.ProcessMonsterDead(proid, autoid);
    }

    public bool CheckCSCreatorMis(int MissionID)
    {
        return m_PlayerMission.CheckCSCreatorMis(MissionID);
    }

    public bool CheckHeroMis()
    {
        return m_PlayerMission.CheckHeroMis();
    }

    public void ProcessCollectMissionByID(int ItemID)
    {
        m_PlayerMission.ProcessCollectMissionByID(ItemID);
    }

    public void ProcessUseItemMissionByID(int ItemID, Vector3 pos, int addOrSubtract = 1,ItemObject itemobj = null)
    {
        m_PlayerMission.ProcessUseItemByID(ItemID, pos, addOrSubtract, itemobj);
    }
#endregion

    Vector3 GetSpawnPos(Vector3 center, int minAngle, int maxAngle, float dist, float radius)
    {
        System.Random r = new System.Random();
        float radian = r.Next(minAngle, maxAngle)/180 * Mathf.PI;
        Vector3 result = center + new Vector3(Mathf.Cos(radian) * dist, 0, Mathf.Sin(radian) * dist);
        Vector2 v2 = UnityEngine.Random.insideUnitCircle;
        result += new Vector3(v2.x * radius, 0f, v2.y * radius);
        return result;
    }

    Vector3 GetSpawnPos(Vector3 sor, Vector3 dst, float percent, float radius) 
    {
        Vector3 result = sor + ((dst - sor) * percent);
        Vector2 v2 = UnityEngine.Random.insideUnitCircle;
        result += new Vector3(v2.x * radius, 0f, v2.y * radius);
        return result;
    }

    public bool IsTempLimit(int missionID) 
    {
        MissionCommonData data = GetMissionCommonData(missionID);
        foreach (var item in data.m_tempLimit)
        {
            if (HasMission(item))
                return true;
        }
        return false;
    }

    public void SetGetTakeMission(int MissionID, PeEntity npc, TakeMissionType type, bool bCheck = true)
    {
        if (type == TakeMissionType.TakeMissionType_Unkown) return;
        //if (npc == null) return;

        if (!GameConfig.IsMultiMode || type == TakeMissionType.TakeMissionType_Complete)
        {
            ProcessSingleMode(MissionID, npc, type, bCheck);
            //lw:成就检测触发
            SteamAchievementsSystem.Instance.OnMissionChange(MissionID, 1);
        }
        else
        {
            //if (!PeGameMgr.IsMultiStory)
            {
                //if (!EntityCreateMgr.Instance.IsRandomNpc(npc))
                //    return;
            }           
            MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
            if (null == data)
                return;
			if (PeGameMgr.IsMulti && bCheck)
			{
				if (!IsGetTakeMission(MissionID) && !data.IsTalkMission())
				{
					return;
				}
			}
			
			m_PlayerMission.m_FollowPlayerName = PeCreature.Instance.mainPlayer.ExtGetName();
            int npcId = -1;
            if (npc != null)
            {
                if (npc.proto == EEntityProto.Npc || npc.proto == EEntityProto.RandomNpc)
                    npcId = npc.Id;
                else
                    npcId = -1;
            }

            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AccessMission, MissionID, npcId, (byte)type, bCheck);
        }
        SteamAchievementsSystem.Instance.OnMissionChange(MissionID, 1);

    }


    /// <summary>
    /// 用于放基地核心的时候更新一下基地目前已有的建筑的数据：目前仅更新训练所检测和贸易站检测
    /// </summary>
    public void ReflashCSUseItemMission()
    {
        int misId = 794;//基地检测训练所
        MissionCommonData data;
        bool Isdoing = m_PlayerMission.ConTainsMission(misId);
        if (Isdoing)
        {
            data = MissionManager.GetMissionCommonData(misId);
            if (data != null)
            {
                PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
                SetGetTakeMission(misId, npc, TakeMissionType.TakeMissionType_Get, false);
            }
        }

        misId = 847;//基地检测贸易站
        Isdoing = m_PlayerMission.ConTainsMission(misId);
        if (Isdoing)
        {
            data = MissionManager.GetMissionCommonData(misId);
            if (data != null)
            {
                PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
                SetGetTakeMission(misId, npc, TakeMissionType.TakeMissionType_Get, false);
            }
        }

        return;
    }

    public void ProcessSingleMode(int MissionID, PeEntity npc, TakeMissionType type, bool bCheck, AiAdNpcNetwork adNpc = null,bool pushStory = true)
    {
        if (MissionID < 0 || MissionID > (int)PlayerMission.MissionInfo.MAX_MISSION_COUNT)
            return;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return;

        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
        if (MissionID == MissionManager.m_SpecialMissionID16)
        {
            CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
            if (creator == null)
                return;

            creator.AddNpc(npc, true);
        }
        else if (MissionID == MissionManager.m_SpecialMissionID9)
        {
            if (!Pathea.PeGameMgr.IsMulti)
			{
				ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                if (leader.GetServantNum() < ServantLeaderCmpt.mMaxFollower)
                {
                    if (!EntityCreateMgr.Instance.IsRandomNpc(npc))
                        return;

                    if (missionData == null)
                        return;

                    npc.SetBirthPos(npc.position);

                    npc.CmdStopTalk();
                    StroyManager.Instance.RemoveReq(npc, EReqType.Dialogue);
                    npc.Recruit();
                    NpcCmpt sc = npc.NpcCmpt;
                    if (sc != null)
                    {
                        if (leader != null)
                        {
							CSMain.SetNpcFollower(npc);
                        }
                        sc.SetServantLeader(leader);
                    }
                    npc.SetShopIcon(null);
                }
			}
			else
			{
				PlayerNetwork.RequestNpcRecruit(npc.Id,true);
            }
        }
        
        bool forceCompleteMission = false;
        if (type == TakeMissionType.TakeMissionType_Get)
        {
            if (PeGameMgr.IsMultiStory)
			{
				if (bCheck && !IsGetTakeMission(MissionID) && !data.IsTalkMission())
					return;
			}
			else
			{
	            if (bCheck && !IsGetTakeMission(MissionID))
	                return;
			}

            string value;
            if (HasRandomMission(MissionID))
            {
                if (npc == null || npc == PeCreature.Instance.mainPlayer)
                    npc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
                if (!ProcessRandomMission(ref data, npc, adNpc))
                    return;

				if (!PeGameMgr.IsMulti)
				{
					if (PeGameMgr.IsAdventure)
						AdRMRepository.CreateRandomMission(MissionID);
					else
						RMRepository.CreateRandomMission(MissionID, missionData != null ? missionData.mCurComMisNum : -1);
				}
            }
			SetQuestVariable(MissionID, PlayerMission.MissionFlagStep, "0",pushStory);
            if (!data.IsTalkMission())
            {
                for (int i = 0; i < data.m_TargetIDList.Count; i++)
                {
                    TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                    if (curType == TargetType.TargetType_Unkown)
                        continue;

                    if (curType == TargetType.TargetType_KillMonster)
                    {
                        TypeMonsterData monData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[i]);
                        if (monData == null)
                            continue;

                        if (monData.m_destroyTown)
                        {
                            int idx = i * 10 + 0;
                            int num = VATownGenerator.Instance.GetAllyTownDestroyedCount(monData.m_campID[0]);
                            value = string.Format("-1_{0}", num);
                            ModifyQuestVariable(MissionID, PlayerMission.MissionFlagMonster + idx, value);

                            //m_PlayerMission.ModifyQuestVariable()
                            if (num >= monData.m_townNum[0])
                            {
                                if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null)
                                {
                                    int townNum = VATownGenerator.Instance.GetAllyTownDestroyedCount(monData.m_campID[0]);
                                    string townValue = string.Format("-1_{0}", townNum);
                                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyMissionFlag, data.m_ID, PlayerMission.MissionFlagMonster + idx, townValue);
                                }
                                CompleteTarget(monData.m_TargetID, data.m_ID);
                            }
                            else
                            {
                                VArtifactTownManager.Instance.RegistTownDestryedEvent(delegate (int n)
                                {
                                    bool found = false;
                                    foreach(var iter in monData.m_campID)
                                    {
                                        if (n == iter)
                                            found = true;
                                    }
                                    if (!found)
                                        return;
                                    int townNum = VATownGenerator.Instance.GetAllyTownDestroyedCount(n);
                                    string townValue = string.Format("-1_{0}", townNum);
                                    ModifyQuestVariable(MissionID, PlayerMission.MissionFlagMonster + idx, townValue);
                                    if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null)
                                    {
                                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyMissionFlag, data.m_ID, PlayerMission.MissionFlagMonster + idx, townValue);
                                    }
                                    CompleteTarget(monData.m_TargetID, data.m_ID);
                                });
                            }
                        }
                        else
                        {
                            for (int m = 0; m < monData.m_MonsterList.Count; m++)
                            {
                                int idx = i * 10 + m;
                                value = monData.m_MonsterList[m].npcs[UnityEngine.Random.Range(0, monData.m_MonsterList[m].npcs.Count)] + "_0";
                                ModifyQuestVariable(MissionID, PlayerMission.MissionFlagMonster + idx, value);
                            }
                        }
                    }
                    else if (curType == TargetType.TargetType_UseItem)
                    {
                        TypeUseItemData useData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[i]);
                        if (useData == null)
                            continue;

                        Vector3 vTarget;
                        Vector3 vAssembly;
                        if (useData.m_allowOld && CSMain.HasBuilding(useData.m_ItemID, CSMain.s_MgCreator, out vTarget)
                            && CSMain.GetAssemblyPos(out vAssembly))
                        {
							if (useData.m_Type == 0) 
							{
								value = useData.m_ItemID + "_1";
							}
							else 
							{
								if (Vector3.Distance(vTarget, vAssembly) < useData.m_Radius)
									value = useData.m_ItemID + "_1";
								else
									value = useData.m_ItemID + "_0";
							}
                            
                            ModifyQuestVariable(MissionID, PlayerMission.MissionFlagItem + i, value);
                            if (IsReplyTarget(data.m_ID, data.m_TargetIDList[i]))
                            {
                                if(useData.m_comMission)
                                    forceCompleteMission = true;
                                else
                                    CompleteTarget(data.m_TargetIDList[i], data.m_ID);
                            }
                            
                        }
                        else
                        {
                            value = useData.m_ItemID + "_0";
                            ModifyQuestVariable(MissionID, PlayerMission.MissionFlagItem + i, value);
                        }
                    }
                }
            }
        }

        switch (type)
        {
            case TakeMissionType.TakeMissionType_Get:
                {
                    if (data.m_OPID == null)
                        break;

                    for (int i = 0; i < data.m_OPID.Count; i++)
                    {
                        MissionCommonData receiveData = GetMissionCommonData(data.m_OPID[i]);
                        if (receiveData == null)
                            continue;
                        if (PeGameMgr.IsAdventure && HasRandomMission(data.m_OPID[i]))
                            ProcessRandomMission(ref receiveData, npc, adNpc);
                        
                        if (MissionRepository.HaveTalkOP(data.m_OPID[i]))
                        {
                            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_OPID[i], 1);
                            GameUI.Instance.mNPCTalk.PreShow();
                        }
                        else
                        {
                            if (IsGetTakeMission(data.m_OPID[i]))
                                SetGetTakeMission(data.m_OPID[i], npc, TakeMissionType.TakeMissionType_Get);
                        }
                    }

                }
                break;
            case TakeMissionType.TakeMissionType_In:
                {
                    if (data.m_INID == null)
                        break;

                    for (int i = 0; i < data.m_INID.Count; i++)
                    {
                        if (IsGetTakeMission(data.m_INID[i]))
                            SetGetTakeMission(data.m_INID[i], npc, TakeMissionType.TakeMissionType_Get);
                    }
                }
                break;
            case TakeMissionType.TakeMissionType_Complete:
                {
                    if (!HadCompleteMission(MissionID))
                        CompleteMission(MissionID);

                    if (data.m_EDID == null)
                        break;

                    for (int i = 0; i < data.m_EDID.Count; i++)
                    {
                        MissionCommonData receiveData = GetMissionCommonData(data.m_EDID[i]);
                        if (receiveData == null)
                            continue;
                        if (PeGameMgr.IsAdventure && HasRandomMission(data.m_EDID[i]))
                            ProcessRandomMission(ref receiveData, npc, adNpc);

                        if (MissionRepository.HaveTalkOP(data.m_EDID[i]))
                        {
                            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_EDID[i], 1);
                            GameUI.Instance.mNPCTalk.PreShow();
                        }
                        else
                        {
                            if (IsGetTakeMission(data.m_EDID[i]))
                                SetGetTakeMission(data.m_EDID[i], npc, TakeMissionType.TakeMissionType_Get);
                        }
                    }

                }
                break;
            default:
                break;
        }
        if (forceCompleteMission)
            CompleteMission(MissionID);
    }

    bool ProcessRandomMission(ref MissionCommonData data, PeEntity npc, AiAdNpcNetwork adNpc = null)
    {
        if (npc != null && npc.proto != EEntityProto.RandomNpc)
            return false;
        if (null == GameUI.Instance.mNPCTalk)
            return false;

        int npcid = 0;
//        string npcname = "0";
        Vector3 npcpos = Vector3.zero;
        NpcMissionData missionData = null;

        if (npc != null)
        {
            npcid = npc.Id;
            //npcname = npc.NpcName;
            missionData = npc.GetUserData() as NpcMissionData;
            npcpos = npc.ExtGetPos();
        }
        else if (adNpc != null)
        {
            //npcid = adNpc.ObjectID.ToString();
            //npcname = adNpc.mNpcName;
            missionData = adNpc.useData;
            npcpos = adNpc.transform.position;
        }

        data.m_iNpc = npcid;
        data.m_iReplyNpc = npcid;

        if (data.m_ID / 1000 != 9 && null == missionData)
            return false;
        
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (curType == TargetType.TargetType_Follow)
            {
                TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                if (folData == null)
                    continue;

                folData.m_iNpcList.Clear();
                folData.m_FailResetPos = npcpos;
                folData.m_iNpcList.Add(npcid);
            }
            else if (curType == TargetType.TargetType_TowerDif)
            {
                TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                if (towerData == null)
                    continue;

                towerData.m_iNpcList.Clear();

                towerData.m_iNpcList.Add(npcid);
                //towerData.m_Pos.type = TypeTowerDefendsData.PosType.pos;
                //towerData.m_Pos.pos = npcpos;
            }
            else if (curType != TargetType.TargetType_Discovery)
            {
                data.m_iReplyNpc = npcid;

                if (!data.isAutoReply && missionData != null && !missionData.m_MissionListReply.Contains(data.m_ID))
                    missionData.m_MissionListReply.Add(data.m_ID);
            }
        }

        return true;
    }

    public void UpdateMissionMainGUI(int MissionID, bool bComplete = true)
    {
        if (MissionID == m_SpecialMissionID5)
            return;

        if (MissionID > 0 && MissionID <= (int)PlayerMission.MissionInfo.MAX_MISSION_COUNT)
        {
            if (bComplete)
            {
                UIMissionMgr.Instance.DeleteMission(MissionID);
                return;
            }

            Dictionary<string, string> missionFlagType = m_PlayerMission.GetMissionFlagType(MissionID);
            if (missionFlagType == null)
                return;

            MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
            if (data == null)
                return;

            if (data.IsTalkMission())
                return;

            UIMissionMgr.MissionView mv = new UIMissionMgr.MissionView();
            mv.mMissionID = data.m_ID;
            mv.mMissionType = data.m_Type;
            mv.mMissionTitle = data.m_MissionName;
            PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
            if (npc != null)
            {
                mv.mMissionStartNpc.mName = npc.ExtGetName();
                mv.mMissionStartNpc.mNpcIcoStr = npc.ExtGetFaceIconBig();
            }

            npc = EntityMgr.Instance.Get(data.m_iReplyNpc);
            if (npc != null)
            {
                mv.mMissionEndNpc.mName = npc.ExtGetName();
                mv.mMissionEndNpc.mNpcIcoStr = npc.ExtGetFaceIconBig();
                mv.mEndMisPos = npc.ExtGetPos();
                mv.mAttachOnId = npc.Id;
                mv.NeedArrow = true;
            }

            npc = EntityMgr.Instance.Get(data.m_replyIconId);
            if (npc != null)
            {
                mv.mMissionReplyNpc.mName = npc.ExtGetName();
                mv.mMissionReplyNpc.mNpcIcoStr = npc.ExtGetFaceIconBig();
            }

            ParseMissionFlag(data, missionFlagType, mv);
            UIMissionMgr.Instance.AddMission(mv);
            UIMissionMgr.Instance.RefalshMissionWnd();
        }
    }

    void CheckViewComplete(UIMissionMgr.MissionView view)
    {
        bool result = true;
        foreach (var item in view.mTargetList)
        {
            if (item.mComplete == false)
            {
                result = false;
                break;
            }
        }
        view.mComplete = result;
    }

    public void UpdateUseMissionTrack(int MissionID, int targetID, int count = 0)
    {
        if (MissionID < 0 || MissionID > (int)PlayerMission.MissionInfo.MAX_MISSION_COUNT)
            return;

        UIMissionMgr.MissionView missview = UIMissionMgr.Instance.GetMissionView(MissionID);
        if (missview == null)
            return;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return;

        if (targetID >= data.m_TargetIDList.Count)
            return;

        UIMissionMgr.TargetShow tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, data.m_TargetIDList[targetID]));
        tarshow.mCount = count;

        if (tarshow.mMaxCount <= tarshow.mCount)
            tarshow.mComplete = true;
        else
            tarshow.mComplete = false;
        CheckViewComplete(missview);
    }

    public void UpdateMissionTrack(int MissionID, int count = 0, int TargetID = -1)
    {
        if (MissionID < 0 || MissionID > (int)PlayerMission.MissionInfo.MAX_MISSION_COUNT)
            return;

        UIMissionMgr.MissionView missview = UIMissionMgr.Instance.GetMissionView(MissionID);
        if (missview == null)
            return;

        if (TargetID != -1)
        {
            UIMissionMgr.TargetShow tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, TargetID));
            TargetType curType = MissionRepository.GetTargetType(TargetID);
            if (curType == TargetType.TargetType_Follow || curType == TargetType.TargetType_Discovery) 
            {
                if (curType == TargetType.TargetType_Follow)
                {
                    TypeFollowData folData = GetTypeFollowData(TargetID);
                    if (folData == null)
                        return;
                }
                else if (curType == TargetType.TargetType_Discovery)
                {
                    TypeSearchData seaData = GetTypeSearchData(TargetID);
                    if (seaData == null)
                        return;
                }
                if (tarshow == null)
                    return;

                tarshow.mPosition = Vector3.zero;
                tarshow.mComplete = true;
                return;
            }
            else if (curType == TargetType.TargetType_KillMonster) 
            {
                tarshow.mCount = count;

                if (tarshow.mMaxCount <= tarshow.mCount)
                    tarshow.mComplete = true;
                else
                    tarshow.mComplete = false;
                return;
            }
        }

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return;

        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            UIMissionMgr.TargetShow tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, data.m_TargetIDList[i]));
            if (tarshow == null)
                continue;

            if (PeCreature.Instance.mainPlayer == null)
                return;

            TargetType type = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            switch (type)
            {
                case TargetType.TargetType_KillMonster:
                    TypeMonsterData mkData = GetTypeMonsterData(data.m_TargetIDList[i]);
                    if (mkData != null)
                    {
                        tarshow.mCount = count;
                    }
                    break;
                case TargetType.TargetType_Collect:
                    TypeCollectData colData = GetTypeCollectData(data.m_TargetIDList[i]);
                    if (colData != null)
                    {
                        ECreation itemType = m_PlayerMission.IsSpecialID(colData.ItemID);
                        if (itemType != ECreation.Null)
                            tarshow.mCount = PeCreature.Instance.mainPlayer.GetCreationItemCount(itemType);
                        else
                            tarshow.mCount = PeCreature.Instance.mainPlayer.GetPkgItemCount(colData.ItemID);
                    }
                    break;
                case TargetType.TargetType_UseItem:
                    TypeUseItemData useData = GetTypeUseItemData(data.m_TargetIDList[i]);
                    if (useData != null)
                    {
                        Dictionary<string, string> missionFlagType = MissionManager.Instance.m_PlayerMission.GetMissionFlagType(MissionID);
                        if (missionFlagType.ContainsKey(useData.m_ItemID.ToString()))
                            tarshow.mCount = Convert.ToInt32(missionFlagType[useData.m_ItemID.ToString()]) + count;
                    }
                    break;
                case TargetType.TargetType_TowerDif:
                    TypeTowerDefendsData towerData = GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                    if (towerData != null)
                    {
                        count = GetTowerDefineKillNum(MissionID);
                        tarshow.mCount = count;
                    }
                    break;
                default:
                    continue;
            }
            
            if (tarshow.mMaxCount <= tarshow.mCount)
                tarshow.mComplete = true;
            else
                tarshow.mComplete = false;
        }
        CheckViewComplete(missview);
    }

    public void CheckAllGetableMission()
    {
        foreach (KeyValuePair<int, MissionCommonData> ite in MissionRepository.m_MissionCommonMap)
        {
            if (!MissionRepository.IsMainMission(ite.Key))
                continue;

            if (!IsGetTakeMission(ite.Key))
                continue;

            MissionCommonData data = ite.Value;
            PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
            if (npc == null)
                continue;
            if (m_PlayerMission.followers.Contains(npc.NpcCmpt))
                continue;
            if (ServantLeaderCmpt.Instance.mForcedFollowers.Contains(npc.NpcCmpt))
                continue;

            NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
            if (null == missionData)
                continue;

            if (!missionData.m_MissionList.Contains(ite.Key))
                continue;

            UIMissionMgr.GetableMisView gmv = new UIMissionMgr.GetableMisView(ite.Key, data.m_MissionName, npc.ExtGetPos(), npc.Id);
            gmv.TargetNpcInfo.mName = npc.ExtGetName();
            gmv.TargetNpcInfo.mNpcIcoStr = npc.ExtGetFaceIconBig();
            UIMissionMgr.Instance.AddGetableMission(gmv);
        }
        if (PeGameMgr.IsStory || PeGameMgr.IsAdventure)
        {
            foreach (var item in NpcMissionDataRepository.dicMissionData)
            {
                if (item.Value.m_RandomMission == 0)
                    continue;
                if (!IsGetTakeMission(item.Value.m_RandomMission))
                    continue;

                PeEntity npc = EntityMgr.Instance.Get(item.Key);
                if (npc == null)
                    continue;
                if (m_PlayerMission.followers.Contains(npc.NpcCmpt))
                    continue;
                if (Array.Find<NpcCmpt>(ServantLeaderCmpt.Instance.mFollowers,delegate(NpcCmpt nc)
                {
                    if (nc == npc.NpcCmpt)
                        return true;
                    return false;
                }))
                    continue;


                NpcMissionData missionData = item.Value;
                if (null == missionData)
                    continue;

                UIMissionMgr.GetableMisView gmv = new UIMissionMgr.GetableMisView(item.Value.m_RandomMission, "RandomMission", npc.ExtGetPos(), npc.Id);
                gmv.TargetNpcInfo.mName = npc.ExtGetName();
                gmv.TargetNpcInfo.mNpcIcoStr = npc.ExtGetFaceIconBig();
                UIMissionMgr.Instance.AddGetableMission(gmv);
            }
        }
    }

    void CheckAdAllGetableMission() 
    {
        foreach (KeyValuePair<int, MissionCommonData> ite in AdRMRepository.m_AdRandMisMap)
        {
            if (!IsGetTakeMission(ite.Key))
                continue;

            MissionCommonData data = ite.Value;
            //if (data.IsTalkMission())
            //    continue;

            NpcMissionData tmp;
            int npcId = 0;
            foreach (var item in EntityMgr.Instance.mDicEntity)
            {
                tmp = item.Value.GetUserData() as NpcMissionData;
                if (tmp == null)
                    continue;
                foreach (var item1 in AdRMRepository.m_AdRandomGroup[tmp.m_QCID].m_GroupList)
                {
                    //item1.Value.Find(delegate(GroupInfo o)
                    //{
                    //    if (o.id == data.m_ID)
                    //    {
                    //        npcId = item.Key;
                    //        return true;
                    //    }
                    //    return false;
                    //});
                    for (int i = 0; i < item1.Value.Count; i++)
                    {
                        if (item1.Value[i].id == data.m_ID)
                        {
                            npcId = item.Key;
                            break;
                        }
                    }
                }
            }
            PeEntity npc = EntityMgr.Instance.Get(npcId);
            if (npc == null)
                continue;

            AdRMRepository.m_AdRandMisMap[ite.Key].m_iNpc = npcId;
            //NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
            //if (null == missionData)
            //    continue;

            //if (!missionData.m_MissionList.Contains(ite.Key))
            //    continue;

            UIMissionMgr.GetableMisView gmv = new UIMissionMgr.GetableMisView(ite.Key, data.m_MissionName, npc.ExtGetPos(), npc.Id);
            gmv.TargetNpcInfo.mName = npc.ExtGetName();
            gmv.TargetNpcInfo.mNpcIcoStr = npc.ExtGetFaceIconBig();
            UIMissionMgr.Instance.AddGetableMission(gmv);
        }
    }

    public void ParseMissionFlag(MissionCommonData data, Dictionary<string, string> MissionFlagType, UIMissionMgr.MissionView stMV)
    {
        string content = data.m_Description;
        if (content == null)
            return;

        data.m_MissionName = GameUI.Instance.mNPCTalk.ParseStrDefine(data.m_MissionName, data);
        content = GameUI.Instance.mNPCTalk.ParseStrDefine(content, data);

        if (data.m_Type == MissionType.MissionType_Mul)
        {
            UIMissionMgr.TargetShow addTarget = new UIMissionMgr.TargetShow();
            addTarget.mContent = data.m_MulDesc;

            stMV.mTargetList.Add(addTarget);

            stMV.mMissionDesc = content;
            return;
        }

        string monstername = "\"monsterid%\"";
        string monsternum = "\"monsternum%\"";
//        string killmonname = "\"killedmosternum%\"";

        string pos = "\"position%\"";
        string npc1 = "\"npcid1%\"";
        string npc2 = "\"npcid2%\"";
        string npc3 = "\"npcid3%\"";
//        string npclist = "\"npclist%\"";
//        string npcnum = "\"npcnum%\"";

        string itemname = "\"itemid%\"";
        string itemnum = "\"itemnum%\"";
        string targetitem = "\"targetitemid%\"";

        string givemisnpc = "\"givenpcid%\"";
        string receivenpc = "\"receivenpcid%\"";
        string adNpcName = "\"AdvNPC%\"";
        string adTownName = "\"Town%\"";
        string adCampName = "\"AI%\"";

        PeEntity npc;

        int num;
        for (int m = 0; m < data.m_TargetIDList.Count; m++)
        {
            TypeMonsterData monData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[m]);
            TypeCollectData colData = MissionManager.GetTypeCollectData(data.m_TargetIDList[m]);
            TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[m]);
            TypeSearchData seaData = MissionManager.GetTypeSearchData(data.m_TargetIDList[m]);
            TypeUseItemData useData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[m]);
            TypeMessengerData mesData = MissionManager.GetTypeMessengerData(data.m_TargetIDList[m]);
            TypeTowerDefendsData towData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[m]);
            UIMissionMgr.TargetShow addTarget = new UIMissionMgr.TargetShow(data.m_TargetIDList[m]);

            if (monData != null)
            {
                addTarget.mContent = monData.m_Desc != "0" ? monData.m_Desc : "";
//                string oldcontent = "";

                for (int i = 0; i < monData.m_MonsterList.Count; i++)
                {
                    MonsterProtoDb.Item protoItem;

                    List<MonsterProtoDb.Item> protoItems = new List<MonsterProtoDb.Item>();
                    for (int j = 0; j < monData.m_MonsterList[i].npcs.Count; j++)
                    {
                        protoItem = MonsterProtoDb.Get(monData.m_MonsterList[i].npcs[j]);
                        if (protoItem == null)
                            continue;
                        protoItems.Add(protoItem);
                    }
                    foreach (var item in protoItems)
                        addTarget.mIconName.Add(item.icon);
                    addTarget.mMaxCount = monData.m_MonsterList[i].type;

                    num = m_PlayerMission.GetQuestVariable(data.m_ID, monData.m_MonsterList[i].npcs[UnityEngine.Random.Range(0, monData.m_MonsterList[i].npcs.Count)]);
                    addTarget.mCount = num;
                    if (num >= monData.m_MonsterList[i].type)
                        addTarget.mComplete = true;

                    string names = "";
                    for (int j = 0; j < protoItems.Count; j++)
                    {
                        names += protoItems[j].name;
                        if (j == protoItems.Count - 1)
                            continue;
                        names += "s or ";
                    }

                    if (addTarget.mContent.Contains(monstername))
                        addTarget.mContent = addTarget.mContent.Replace(monstername, names);

                    if (addTarget.mContent.Contains(monsternum))
                        addTarget.mContent = addTarget.mContent.Replace(monsternum, monData.m_MonsterList[i].type.ToString());

                    //addTarget.mContent += "[-]";
                    //oldcontent += addTarget.mContent;
                }
            }
            else if (colData != null)
            {
                addTarget.mContent = colData.m_Desc != "0" ? colData.m_Desc : "";

                string[] tmplist = ItemProto.GetIconName(colData.ItemID);
                if (tmplist == null) 
                {
                    ECreation creationType = m_PlayerMission.IsSpecialID(colData.ItemID);
                    switch (creationType)
                    {
                        // �� ------------------------
                        case ECreation.Sword:
                            tmplist = new string[3] { "task_created_001", "0", "0" };
                            break;
                        // ��ǹ/����ǹ ------------------------
                        case ECreation.HandGun:
                            tmplist = new string[3] { "task_created_002", "0", "0" };
                            break;
                        // ��ǹ/˫��ǹ ------------------------
                        case ECreation.Rifle:
                            tmplist = new string[3] { "task_created_003", "0", "0" };
                            break;
                        // �ؾ� ------------------------
                        case ECreation.Vehicle:
                            tmplist = new string[3] { "task_created_007", "0", "0" };
                            break;
                        case ECreation.Aircraft:
                            tmplist = new string[3] { "task_created_009", "0", "0" };
                            break;
                        default:
                            break;
                    }
                }
                if (tmplist == null || tmplist.Length != 3)
                    continue;

                if (colData.m_TargetPos != Vector3.zero)
                {
                    addTarget.mPosition = colData.m_TargetPos;
                    addTarget.Radius = 10;
                    stMV.NeedArrow = true;
                }

                addTarget.mIconName.Add(tmplist[0]);

                addTarget.mMaxCount = colData.ItemNum;

                ECreation type = m_PlayerMission.IsSpecialID(colData.ItemID);
                if (type != ECreation.Null)
                    num = PeCreature.Instance.mainPlayer.GetCreationItemCount(type);
                else
                    num = PeCreature.Instance.mainPlayer.GetPkgItemCount(colData.ItemID);

                addTarget.mCount = num;
                if (num >= colData.ItemNum)
                    addTarget.mComplete = true;

                if (addTarget.mContent.Contains(itemname))
                    addTarget.mContent = addTarget.mContent.Replace(itemname, ItemAsset.ItemProto.GetName(colData.ItemID));

                if (addTarget.mContent.Contains(itemnum))
                    addTarget.mContent = addTarget.mContent.Replace(itemnum, colData.ItemNum.ToString());

                if (addTarget.mContent.Contains(targetitem))
                    addTarget.mContent = addTarget.mContent.Replace(targetitem, colData.m_TargetItemID.ToString());

                if (addTarget.mContent.Contains(npc1))
                {
                    npc = EntityMgr.Instance.Get(data.m_iNpc);
                    if (npc != null)
                        addTarget.mContent = addTarget.mContent.Replace(npc1, npc.ExtGetName());
                }

                //addTarget.mContent += "[-]";
            }
            else if (folData != null)
            {
                addTarget.mContent = folData.m_Desc != "0" ? folData.m_Desc : "";
                for (int i = 0; i < folData.m_iNpcList.Count; i++)
                {
                    npc = EntityMgr.Instance.Get(folData.m_iNpcList[i]);

                    if (npc != null)
                    {
                        if (npc.ExtGetName() == "AllenCarryingGerdy")
                            addTarget.mIconName.Add("npc_AllenCarter");
                        else
                            addTarget.mIconName.Add(npc.ExtGetFaceIconBig());

                        if (i == 0 && addTarget.mContent.Contains(npc1))
                            addTarget.mContent = addTarget.mContent.Replace(npc1, npc.ExtGetName());
                        else if (i == 1 && addTarget.mContent.Contains(npc2))
                            addTarget.mContent = addTarget.mContent.Replace(npc2, npc.ExtGetName());
                        else if (i == 2 && addTarget.mContent.Contains(npc3))
                            addTarget.mContent = addTarget.mContent.Replace(npc3, npc.ExtGetName());
                    }
                }

                if (folData.m_BuildID != 0)
                {
					PlayerMission.GetBuildingPos(data.m_ID == 9032 ? 0 : data.m_ID, out addTarget.mPosition);
                    addTarget.Radius = folData.m_TrackRadius;
                    stMV.NeedArrow = true;
                }
                else if (folData.m_DistPos != Vector3.zero)
                {
                    addTarget.mPosition = folData.m_DistPos;
                    addTarget.Radius = folData.m_TrackRadius;
                    stMV.NeedArrow = true;
                }
                else if (folData.m_LookNameID != 0)
                {
                    npc = EntityMgr.Instance.Get(folData.m_LookNameID);
                    if (npc != null)
                    {
                        addTarget.mPosition = new Vector3(1, 1, 1);
                        addTarget.Radius = folData.m_TrackRadius;
                        stMV.NeedArrow = true;
                        addTarget.mAttachOnID = folData.m_LookNameID;
                    }
                }
                if((PeGameMgr.IsMulti && !PeGameMgr.IsMultiStory) && folData.m_DistPos == Vector3.zero)
                {
                    folData.m_DistPos = new Vector3(1, 1, 1);
                    addTarget.mPosition = folData.m_DistPos;
                    addTarget.Radius = folData.m_TrackRadius;
                    stMV.NeedArrow = true;
                }
                    
                if (folData.m_SceneType != 0) 
                {
					if (PeGameMgr.IsMultiStory)
					{
						if(PlayerNetwork.mainPlayer != null)
						{
							if (folData.m_SceneType == 1)
							{
								if (folData.m_SceneType != PlayerNetwork.mainPlayer._curSceneId)
									addTarget.mPosition = new Vector3(9687, 370, 12799);
							}
							else if (folData.m_SceneType == 2)
							{
								if (folData.m_SceneType != PlayerNetwork.mainPlayer._curSceneId)
									addTarget.mPosition = new Vector3(14820, 107, 8353);
							}
						}
						
					}
					else
					{
						if (folData.m_SceneType == 1)
						{
							if (folData.m_SceneType != (int)SingleGameStory.curType)
								addTarget.mPosition = new Vector3(9687, 370, 12799);
						}
						else if (folData.m_SceneType == 2)
						{
							if (folData.m_SceneType != (int)SingleGameStory.curType)
								addTarget.mPosition = new Vector3(14820, 107, 8353);
						}
					}
                   
                }

                if (addTarget.mContent.Contains(pos))
                    addTarget.mContent = addTarget.mContent.Replace(pos, folData.m_DistPos.ToString());

                if (HadCompleteTarget(folData.m_TargetID))
                {
                    addTarget.mComplete = true;
                    addTarget.mPosition = Vector3.zero;
                }
            }
            else if (seaData != null)
            {
                addTarget.mContent = seaData.m_Desc != "0" ? seaData.m_Desc : "";

                if (seaData.m_TrackRadius != 0)
                {
                    if (seaData.m_DistPos != Vector3.zero)
                    {
                        addTarget.mPosition = seaData.m_DistPos;
                        addTarget.Radius = seaData.m_TrackRadius;
                        stMV.NeedArrow = true;
                    }
                    if((!PeGameMgr.IsMultiStory && PeGameMgr.IsMulti) && seaData.m_DistPos == Vector3.zero)
                    {
                        seaData.m_DistPos = new Vector3(1, 1, 1);
                        addTarget.mPosition = seaData.m_DistPos;
                        addTarget.Radius = seaData.m_TrackRadius;
                        stMV.NeedArrow = true;
                    }
                    if (seaData.m_NpcID != 0)
                    {
                        npc = EntityMgr.Instance.Get(seaData.m_NpcID);
                        if (npc != null)
                        {
                            addTarget.mPosition = new Vector3(1, 1, 1);
                            addTarget.Radius = seaData.m_TrackRadius;
                            addTarget.mAttachOnID = seaData.m_NpcID;
                            stMV.NeedArrow = true;
                        }
                    }
                }
                
                if (seaData.m_SceneType != 0)
                {
                    if (seaData.m_SceneType == 1)
                    {
                        if (seaData.m_SceneType != (int)SingleGameStory.curType)
                            addTarget.mPosition = new Vector3(9687, 370, 12799);
                    }
                    else if (seaData.m_SceneType == 2)
                    {
                        if (seaData.m_SceneType != (int)SingleGameStory.curType)
                            addTarget.mPosition = new Vector3(14820, 107, 8353);
                    }
                }

                if (addTarget.mContent.Contains(pos))
                    addTarget.mContent = addTarget.mContent.Replace(pos, seaData.m_DistPos.ToString());

                if (addTarget.mContent.Contains(npc1))
                {
                    npc = EntityMgr.Instance.Get(seaData.m_NpcID);
                    if (npc != null)
                        addTarget.mContent = addTarget.mContent.Replace(npc1, npc.ExtGetName());
                }

                if (HadCompleteTarget(seaData.m_TargetID))
                {
                    addTarget.mComplete = true;
                    addTarget.mPosition = Vector3.zero;
                }
            }
            else if (useData != null)
            {
                addTarget.mContent = useData.m_Desc != "0" ? useData.m_Desc : "";

                string[] tmplist = ItemProto.GetIconName(useData.m_ItemID);
                if (tmplist == null)
                {
                    ECreation creationType = m_PlayerMission.IsSpecialID(useData.m_ItemID);
                    switch (creationType)
                    {
                        // �� ------------------------
                        case ECreation.Sword:
                            tmplist = new string[3] { "task_created_001", "0", "0" };
                            break;
                        // ��ǹ/����ǹ ------------------------
                        case ECreation.HandGun:
                            tmplist = new string[3] { "task_created_002", "0", "0" };
                            break;
                        // ��ǹ/˫��ǹ ------------------------
                        case ECreation.Rifle:
                            tmplist = new string[3] { "task_created_003", "0", "0" };
                            break;
                        // �ؾ� ------------------------
                        case ECreation.Vehicle:
                            tmplist = new string[3] { "task_created_007", "0", "0" };
                            break;
                        case ECreation.Aircraft:
                            tmplist = new string[3] { "task_created_009", "0", "0" };
                            break;
                        default:
                            break;
                    }
                }
                if (tmplist == null || tmplist.Length != 3)
                    continue;

                addTarget.mIconName.Add(tmplist[0]);
                addTarget.mMaxCount = useData.m_UseNum;
                addTarget.mCount = m_PlayerMission.GetQuestVariable(data.m_ID, useData.m_ItemID);

                if (useData.m_Pos != Vector3.zero)
                {
                    if (useData.m_Pos == new Vector3(-255, -255, -255))
                    {
                        Vector3 csMain;
                        if (CSMain.GetAssemblyPos(out csMain))
                            addTarget.mPosition = csMain;
                    }
                    else
                        addTarget.mPosition = useData.m_Pos;

                    addTarget.Radius = useData.m_Radius;
                    stMV.NeedArrow = true;
                }
                if((PeGameMgr.IsMulti && !PeGameMgr.IsMultiStory) && useData.m_Pos == Vector3.zero)
                {
                    useData.m_Pos = new Vector3(1,1,1);
                    if (useData.m_Pos == new Vector3(-255, -255, -255))
                    {
                        Vector3 csMain;
                        if (CSMain.GetAssemblyPos(out csMain))
                            addTarget.mPosition = csMain;
                    }
                    else
                        addTarget.mPosition = useData.m_Pos;

                    addTarget.Radius = useData.m_Radius;
                    stMV.NeedArrow = true;
                }

                if (addTarget.mContent.Contains(pos))
                    addTarget.mContent = addTarget.mContent.Replace(pos, useData.m_Pos.ToString());

                if (addTarget.mContent.Contains(itemname))
                    addTarget.mContent = addTarget.mContent.Replace(itemname, ItemAsset.ItemProto.GetName(useData.m_ItemID));

                if (addTarget.mContent.Contains(itemnum))
                    addTarget.mContent = addTarget.mContent.Replace(itemnum, useData.m_UseNum.ToString());

                //addTarget.mContent += "[-]";

                if (addTarget.mCount >= addTarget.mMaxCount)
                {
                    addTarget.mPosition = Vector3.zero;
                    addTarget.mComplete = true;
                }

            }
            else if (mesData != null)
            {
                addTarget.mContent = mesData.m_Desc != "0" ? mesData.m_Desc : "";
                npc = EntityMgr.Instance.Get(mesData.m_iReplyNpc);
                if (npc != null)
                {
                    addTarget.mIconName.Add(npc.ExtGetFaceIconBig());
                    if (addTarget.mContent.Contains(receivenpc))
                        addTarget.mContent = addTarget.mContent.Replace(receivenpc, npc.ExtGetName());
                }

                npc = EntityMgr.Instance.Get(mesData.m_iNpc);
                if (npc != null)
                {
                    if (addTarget.mContent.Contains(givemisnpc))
                        addTarget.mContent = addTarget.mContent.Replace(givemisnpc, npc.ExtGetName());
                }

                if (addTarget.mContent.Contains(itemname))
                    addTarget.mContent = addTarget.mContent.Replace(itemname, ItemAsset.ItemProto.GetName(mesData.m_ItemID));

                if (addTarget.mContent.Contains(itemnum))
                    addTarget.mContent = addTarget.mContent.Replace(itemnum, mesData.m_ItemNum.ToString());

                addTarget.mMaxCount = mesData.m_ItemNum;
            }
            else if (towData != null)
            {
                addTarget.mContent = towData.m_Desc != "0" ? towData.m_Desc : "";
                addTarget.mMaxCount = towData.m_Count;
            }

            if (PeGameMgr.IsAdventure)
            {
                string id;
                if (addTarget.mContent.Contains(adNpcName))
                {
                    int index = addTarget.mContent.IndexOf(adNpcName);
                    if (addTarget.mContent.Length >= index + 9 + 3)
                    {
                        id = addTarget.mContent.Substring(index + 9, 3);
                        if (PETools.PEMath.IsNumeral(id))
                        {
                            int n = Convert.ToInt32(id);
                            if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(n))
                            {
                                npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[n]);
                                if (npc != null)
                                {
                                    string tmp1 = addTarget.mContent.Substring(index, 9 + 3);
                                    string tmp2 = npc.name.Substring(0, npc.name.Length - 1 - System.Convert.ToString(npc.Id).Length);
                                    addTarget.mContent = addTarget.mContent.Replace(tmp1, tmp2);
                                }
                            }
                        }
                    }
                }

                if (addTarget.mContent.Contains(adTownName))
                {
                    int index = addTarget.mContent.IndexOf(adTownName);
                    if (addTarget.mContent.Length >= index + 7 + 3)
                    {
                        id = addTarget.mContent.Substring(index + 7, 3);
                        if (PETools.PEMath.IsNumeral(id))
                        {
                            int n = Convert.ToInt32(id);
                            string tmp;
                            VArtifactUtil.GetTownName(n, out tmp);
                            addTarget.mContent = addTarget.mContent.Replace(addTarget.mContent.Substring(index, 7 + 3), tmp);
                        }
                    }
                }

                if (addTarget.mContent.Contains(adCampName))
                {
                    int index = addTarget.mContent.IndexOf(adCampName);
                    if (addTarget.mContent.Length >= index + 5 + 3)
                    {
                        addTarget.mContent = addTarget.mContent.Replace(addTarget.mContent.Substring(index, 5 + 3), "Puja");
                    }
                }
            }

            stMV.mTargetList.Add(addTarget);
        }

        if (PeGameMgr.IsAdventure)
        {
            string id;
            if (content.Contains(adNpcName))
            {
                int index = content.IndexOf(adNpcName);
                if (content.Length >= index + 9 + 3)
                {
                    id = content.Substring(index + 9, 3);
                    if (PETools.PEMath.IsNumeral(id))
                    {
                        int n = Convert.ToInt32(id);
                        if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(n))
                        {
                            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[n]);
                            if (npc != null)
                            {
                                string tmp1 = content.Substring(index, 9 + 3);
                                string tmp2 = npc.name.Substring(0, npc.name.Length - 1 - System.Convert.ToString(npc.Id).Length);
                                content = content.Replace(tmp1, tmp2);
                            }
                        }
                    }
                }
            }

            if (content.Contains(adTownName))
            {
                int index = content.IndexOf(adTownName);
                if (content.Length >= index + 7 + 3)
                {
                    id = content.Substring(index + 7, 3);
                    if (PETools.PEMath.IsNumeral(id))
                    {
                        int n = Convert.ToInt32(id);
                        string tmp;
                        VArtifactUtil.GetTownName(n, out tmp);
                        content = content.Replace(content.Substring(index, 7 + 3), tmp);
                    }
                }
            }

            if (content.Contains(adCampName))
            {
                int index = content.IndexOf(adCampName);
                if (content.Length >= index + 5 + 3)
                {
                    content = content.Replace(content.Substring(index, 5 + 3), "Puja");
                }
            }
        }

        stMV.mMissionDesc = content;

        //REWARD
        for (int i = 0; i < data.m_Com_RewardItem.Count; i++)
        {
            ItemSample itemGrid = new ItemSample(data.m_Com_RewardItem[i].id, data.m_Com_RewardItem[i].num);
            if (null == itemGrid.protoData)
            {
                Debug.LogError("xxxxxxxxxxxxxxxxxx========================================" + data.m_Com_RewardItem[i].id);
                continue;
            }
            if (null == itemGrid || itemGrid.protoData.equipSex != PeSex.Undefined/* && itemGrid.prototypeData.mEquiSex != m_Player.GetPlayerSex()*/)
                continue;

            if (data.m_Com_RewardItem[i].id > 0)
            {
                stMV.mRewardsList.Add(itemGrid);
            }
        }

        CheckViewComplete(stMV);
        //SELREWARD
        //for (int i = 0; i < data.m_Com_SelRewardItem.Count; i++)
        //{
        //    ItemSample itemGrid = new ItemSample(data.m_Com_SelRewardItem[i].id, data.m_Com_SelRewardItem[i].num);

        //    if (itemGrid.prototypeData.mEquiSex != 0/* && itemGrid.prototypeData.mEquiSex != m_Player.GetPlayerSex()*/)
        //        continue;

        //    if (data.m_Com_SelRewardItem[i].id > 0)
        //    {
        //        stMV.mSelRewardsList.Add(itemGrid);
        //    }
        //}
    }

    float gameTime = 0f;
    void Update()
    {
        if (PeCreature.Instance.mainPlayer != null)
        {
            UpdateMission();
            UpdateMissionNeedTime();
            UpdateTimer();
            MoveAdventureLeader();
        }
    }

    Vector3 recordTownPos;
    void MoveAdventureLeader()
    {
        if (PeGameMgr.IsAdventure)
        {
            if (Time.time - gameTime > 3f)
            {
                gameTime = Time.time;
                int campID;
                Vector3 townPos;
                float distance = VATownGenerator.Instance.GetNearestAllyDistance(PeCreature.Instance.mainPlayer.position, out campID, out townPos);
                if (campID != 0 && distance < 150f && Vector3.Distance(townPos,recordTownPos) > 1)
                {
                    recordTownPos = townPos;
                    PeEntity npc = EntityMgr.Instance.Get(20000 - campID);
                    if (npc != null)
                    {
                        Vector3 v = townPos + (PeCreature.Instance.mainPlayer.position - townPos).normalized * 105f;
                        IntVector2 posXZ = new IntVector2((int)v.x,(int)v.z);
                        StroyManager.Instance.MoveTo(npc, new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y));
                    }
                }
            }
        }
    }

#region TimerTodo
    List<TimerTodo> timers = new List<TimerTodo>();
    List<TimerTodo> recordRemove = new List<TimerTodo>();
    private class TimerTodo 
    {
        public PETimer timer;
        public Action act;
        public int fixedID;
    }
    void UpdateTimer() 
    {
        if (timers.Count == 0)
            return;
        foreach (var item in timers)
        {
            item.timer.ElapseSpeed = -GameTime.Timer.ElapseSpeed;
            item.timer.Update(Time.deltaTime);
            if (item.timer.Second <= 0)
            {
                item.act();
                recordRemove.Add(item);
            }
        }
        foreach (var item in recordRemove)
            timers.Remove(item);
    }
    public void PeTimeToDo(Action _func, double _second, int fixedID)
    {
        PETimer timer = new PETimer();
        timer.Second = _second;
        TimerTodo todo = new TimerTodo();
        todo.timer = timer;
        todo.act = _func;
        todo.fixedID = fixedID;

        timers.Add(todo);
    }
    public void RemoveTimerByID(int fixedID) 
    {
        TimerTodo tmp = timers.Find(e => (e.fixedID == fixedID) ? true : false);
        if (tmp != null)
            timers.Remove(tmp);
    }
#endregion

    void UpdateMissionNeedTime() 
    {
        if (m_PlayerMission.m_MissionTime.Count == 0)
            return;
        MissionCommonData record = null;
        foreach (var item in m_PlayerMission.m_MissionTime)
        {
            MissionCommonData mcd = MissionManager.GetMissionCommonData(item.Key);
            if (mcd == null)
                continue;
            if (GameTime.Timer.Second - item.Value > mcd.m_NeedTime)
            {
                record = mcd;
                break;
            }
        }
        if(record == null)
            return;
        m_PlayerMission.UpdateNpcMissionTex(EntityMgr.Instance.Get(record.m_iReplyNpc));
        m_PlayerMission.m_MissionTime.Remove(record.m_ID);
        if(record.m_timeOverToPlot != 0)
            StroyManager.Instance.PushStoryList(new List<int> { record.m_timeOverToPlot });
    }

    public void UpdateMission()
    {
        MissionCommonData data;
        int id;
        Dictionary<int, Dictionary<string, string>> MissionInfo = m_PlayerMission.m_MissionInfo;
        List<int> missionID = new List<int>();
        foreach (var item in MissionInfo)
        {
            missionID.Add(item.Key);
        }

        foreach (int iter in missionID)
        {
            id = iter;
            if (id == MissionManager.m_SpecialMissionID45 || id == MissionManager.m_SpecialMissionID83)
                continue;
            else if (id == MissionManager.m_SpecialMissionID64)
            {
                CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
                if (creator == null)
                    continue;

                if (creator.Assembly == null)
                    continue;

                Vector3 vc1 = creator.Assembly.Position;
                MapMaskData mmd = MapMaskData.s_tblMaskData.Find(ret => ret.mId == 29);
                if (mmd == null)
                    continue;

                Vector3 vc2 = mmd.mPosition;

                if (PERailwayCtrl.HasRoute(vc1, vc2))
                {
                    if (PeGameMgr.IsMulti)
                        MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID64);
                    else
                        CompleteMission(MissionManager.m_SpecialMissionID64);
                    return;
                }
            }

            data = MissionManager.GetMissionCommonData(id);

            if (data == null)
                continue;


            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {

                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_TowerDif)
                {
                    if (UpdateTowerMission(id, data.m_TargetIDList[i]))
                        return;

                }
                else if (curType == TargetType.TargetType_Follow)
                {
                    if (UpdateFollowMission(id, data.m_TargetIDList[i]))
                        return;
                }

                if (curType != TargetType.TargetType_Discovery
                    && data.m_ID != MissionManager.m_SpecialMissionID42)
                    continue;

                if (UpdateSearchMission(id, data.m_TargetIDList[i]))
                    return;
            }
        }
    }

    Dictionary<int, float> targetId_time = new Dictionary<int, float>();
    public void SetTowerMissionStartTime(int targetID) 
    {
        targetId_time.Add(targetID, Time.time);
    }
    public bool UpdateTowerMission(int MissionID, int TargetID)
    {
        TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(TargetID);
        if (towerData == null)
            return false;

        foreach (var item in targetId_time)
        {
            if (TargetID != item.Key)
                continue;
            if ((Time.time - item.Value) > towerData.m_tolTime)
            {
                targetId_time.Remove(TargetID);
                CompleteTarget(TargetID, MissionID, true);
            }
        }

        if (PeGameMgr.IsMulti )
        {
            //if(AiTowerDefense.IsAuth())
            //{
            //    float dis = Vector3.Distance(towerData.finallyPos, AiTowerDefense.GetAuthPos());
            //    if (towerData.m_range != 0)
            //    {
            //        if (dis > towerData.m_range)
            //        {
            //            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);

            //            if (targetId_time.ContainsKey(TargetID))
            //                targetId_time.Remove(TargetID);

            //            FailureMission(MissionID);
            //            return true;
            //        }
            //    }
            //}
        }        
        else
        {
            float dis = Vector3.Distance(towerData.finallyPos, StroyManager.Instance.GetPlayerPos());
            if (towerData.m_range != 0)
            {
                if (dis > towerData.m_range)
                {
                    if (targetId_time.ContainsKey(TargetID))
                        targetId_time.Remove(TargetID);

                    FailureMission(MissionID);
                    return true;
                }
            }
        }

        PeEntity npc;

        for (int m = 0; m < towerData.m_iNpcList.Count; m++)
        {
            npc = EntityMgr.Instance.Get(towerData.m_iNpcList[m]);

            if (npc == null)
                continue;

            if (npc.IsDead())
            {               
                if (targetId_time.ContainsKey(TargetID))
                    targetId_time.Remove(TargetID);
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
				}
				else
					FailureMission(MissionID);
				return true;
            }
        }

        foreach (var item in towerData.m_ObjectList)
        {
            PeEntity[] doodads = EntityMgr.Instance.GetDoodadEntities(item);
            if (doodads == null || doodads.Length == 0)
                continue;
            npc = EntityMgr.Instance.GetDoodadEntities(item)[0];

            if (npc == null)
                continue;

            if (npc.IsDead())
            {                
                if (targetId_time.ContainsKey(TargetID))
                    targetId_time.Remove(TargetID);
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
				}
				else
					FailureMission(MissionID);
				return true;
            }
        }

        if (SPAutomatic.IsSpawning())
            return false;

        //��������ɱ����������Ҫ���������
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            UIMissionMgr.MissionView missview = UIMissionMgr.Instance.GetMissionView(MissionID);
            UIMissionMgr.TargetShow tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, data.m_TargetIDList[i]));
            if (tarshow != null && tarshow.mMaxCount <= tarshow.mCount)
            {
                if (PeGameMgr.IsMulti)
                    MissionManager.Instance.RequestCompleteMission(MissionID, -1, false);
                else
                    MissionManager.Instance.CompleteMission(MissionID, -1, false);
                //if (EntityCreateMgr.DbgUseLegacyCode)
                //{
                //    GameObject go = GameObject.Find("TowerMission");
                //    GameObject.Destroy(go);
                //}
                if (UITowerInfo.Instance != null)
                    UITowerInfo.Instance.Hide();
            }
        }

        SPTowerDefence spd = SPAutomatic.GetTowerDefence(MissionID);
        if (spd == null)
            return false;


        if (spd.KilledCount < towerData.m_Count)
        {
            return false;
        }
        CompleteTarget(towerData.m_TargetID, MissionID);

        return true;
    }

    Dictionary<int, int[]> followTarget_num = new Dictionary<int, int[]>();
    Dictionary<int, List<object>> npcWaitTime = new Dictionary<int, List<object>>();    //��һ��intΪNPCid��list<object>��һ��boolΪ��ǰ�������(false��ҳ�ǰ��true������)���ڶ���floatΪֹͣʱ��
    float timer = 0;
    public bool UpdateFollowMission(int MissionID, int TargetID)
    {
        TypeFollowData folData = MissionManager.GetTypeFollowData(TargetID);
        if (folData == null)
            return false;

        if (MissionID == MissionManager.m_SpecialMissionID87
                || MissionID == MissionManager.m_SpecialMissionID88)
        {
            if (MainPlayer.Instance.entity.GetCmpt<Motion_Equip>())
            {
                if (MainPlayer.Instance.entity.GetCmpt<Motion_Equip>().IsWeaponActive() 
                    && Vector3.Distance(MainPlayer.Instance.entity.position, new Vector3(10973f, 226.9163f, 9259f)) <= 100)
                {
                    if (PeGameMgr.IsMulti)
                    {
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
                    }
                    else
						FailureMission(MissionID);
                    return true;
                }
            }
        }

        if (!followTarget_num.ContainsKey(TargetID))
            followTarget_num.Add(TargetID, new int[folData.m_iNpcList.Count]);
        UpdateNpcTalk(folData.m_TalkInfo);

        Vector3 disPos = folData.m_DistPos;
        if (folData.m_LookNameID != 0)
            disPos = StroyManager.Instance.GetNpcPos(folData.m_LookNameID);
        else if (folData.m_BuildID > 0)
        {
			PlayerMission.GetBuildingPos(MissionID == 9032 ? 0 : MissionID, out disPos);
            folData.m_DistRadius = 1;
        }

        PeEntity npc;
        for (int i = 0; i < folData.m_iFailNpc.Count; i++)
        {
            npc = EntityMgr.Instance.Get(folData.m_iFailNpc[i]);
            if (npc == null)
                continue;
            if (npc.IsDeath())
            {
                if (PeGameMgr.IsMulti)
                {
                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
                }
                else
                    FailureMission(MissionID);
                return true;
            }
        }

        if (timer < PETools.PEMath.Epsilon)
            timer = Time.time;
        for (int m = 0; m < folData.m_iNpcList.Count; m++)
        {
            npc = EntityMgr.Instance.Get(folData.m_iNpcList[m]);

            if (npc == null)
                continue;

			if (folData.m_EMode != 1 && folData.m_LookNameID != 0)
            {
                NpcCmpt nc = npc.NpcCmpt;
                if (nc != null && !nc.Req_Contains(EReqType.FollowPath))
                {
                    if (Time.time - timer > 3f && Vector3.Distance(npc.position, disPos) > 2)
                    {
                        if (TargetID != 3081 || npc.Id != 9033)
                        {
                            Vector3 v = AiUtil.GetRandomPosition(disPos, 0.5f, 1);
                            StroyManager.Instance.MoveTo(npc, v, folData.m_DistRadius, true, SpeedState.Run);
                            nc.FixedPointPos = v;
                            if (m == folData.m_iNpcList.Count - 1)
                                timer = Time.time;
                        }
                    }
                }

            }

            //if (MissionID == 10011 && !PeCreature.Instance.mainPlayer.IsOnCarrier())
            //    return false;

            if (MissionID == MissionManager.m_SpecialMissionID66
                || MissionID == MissionManager.m_SpecialMissionID84
                || MissionID == MissionManager.m_SpecialMissionID85
                || MissionID == MissionManager.m_SpecialMissionID86)
            {
                if (npc.IsOnCarrier())
                {
                    if (PeGameMgr.IsMultiStory)
                    {
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
                    }
					else
                    	FailureMission(MissionID);
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    List<int> talkList = new List<int>();
                    talkList.Add(3011);
                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(talkList, null, true);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(talkList, null, false);
                    return false;
                }
            }

            if (npc.IsDead())
                npc.SetInvincible(true);

            float npcDistance = Vector3.Distance(npc.ExtGetPos(), disPos);
            if (folData.m_EMode != 0 && folData.m_EMode != 1)
            {
                float npcPlayerDis = Vector3.Distance(npc.ExtGetPos(), StroyManager.Instance.GetPlayerPos());

                float playerDistance = Vector3.Distance(StroyManager.Instance.GetPlayerPos(), disPos);
                if (folData.m_EMode == 2)
                {
                    if (npcPlayerDis > folData.m_WaitDist[0])
                    {
                        if (!npcWaitTime.ContainsKey(npc.Id))
                        {
                            List<object> tmp = new List<object>();
                            if (playerDistance < npcDistance)
                                tmp.Add(false);
                            else
                                tmp.Add(true);
                            tmp.Add(Time.time);
                            npcWaitTime.Add(npc.Id, tmp);
                        }
                        npc.NpcCmpt.MisstionAskStop = true;
                    }
                }
                else if (folData.m_EMode == 3 && playerDistance < npcDistance && npcPlayerDis > folData.m_WaitDist[0])
                {
                    if (!npcWaitTime.ContainsKey(npc.Id))
                    {
                        List<object> tmp = new List<object>();
                        tmp.Add(false);
                        tmp.Add(Time.time);
                        npcWaitTime.Add(npc.Id, tmp);
                    }
                    npc.NpcCmpt.MisstionAskStop = true;
                }
                else if (folData.m_EMode == 4 && playerDistance > npcDistance && npcPlayerDis > folData.m_WaitDist[0])
                {
                    if (!npcWaitTime.ContainsKey(npc.Id))
                    {
                        List<object> tmp = new List<object>();
                        tmp.Add(true);
                        tmp.Add(Time.time);
                        npcWaitTime.Add(npc.Id, tmp);
                    }
                    npc.NpcCmpt.MisstionAskStop = true;
                }

                if (npcPlayerDis < folData.m_WaitDist[1])
                {
                    if (npcWaitTime.ContainsKey(npc.Id))
                        npcWaitTime.Remove(npc.Id);
                    npc.NpcCmpt.MisstionAskStop = false;
                }
            }

            if (npcWaitTime.ContainsKey(npc.Id) && (Time.time - (float)npcWaitTime[npc.Id][1]) > 7)
            {
                npcWaitTime[npc.Id][1] = Time.time;
                if (!folData.npcid_behindTalk_forwardTalk.ContainsKey(npc.Id))
                    continue;
                if (!(bool)npcWaitTime[npc.Id][0])
                {
                    GameUI.Instance.mNPCTalk.SpTalkSymbol(false);
                    UINPCTalk.NpcTalkInfo talkinfo = new UINPCTalk.NpcTalkInfo();
                    GameUI.Instance.mNPCTalk.GetTalkInfo(folData.npcid_behindTalk_forwardTalk[npc.Id][0], ref talkinfo, null);
                    GameUI.Instance.mNPCTalk.ParseName(null, ref talkinfo);
                    GameUI.Instance.mNPCTalk.m_NpcTalkList.Add(talkinfo);

                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                        GameUI.Instance.mNPCTalk.SPTalkClose();
                }
                else
                {
                    GameUI.Instance.mNPCTalk.SpTalkSymbol(false);
                    UINPCTalk.NpcTalkInfo talkinfo = new UINPCTalk.NpcTalkInfo();
                    GameUI.Instance.mNPCTalk.GetTalkInfo(folData.npcid_behindTalk_forwardTalk[npc.Id][1], ref talkinfo, null);
                    GameUI.Instance.mNPCTalk.ParseName(null, ref talkinfo);
                    GameUI.Instance.mNPCTalk.m_NpcTalkList.Add(talkinfo);

                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                        GameUI.Instance.mNPCTalk.SPTalkClose();
                }
            }

            if (HasRandomMission(MissionID))
            {
                Vector2 v1 = new Vector2(npc.ExtGetPos().x, npc.ExtGetPos().z);
                Vector2 v2 = new Vector2(disPos.x, disPos.z);

                npcDistance = Vector3.Distance(v1, v2);
            }

            if (npcDistance <= folData.m_DistRadius)
            {
				if(PeGameMgr.IsMultiStory)
				{
					if(PlayerNetwork.mainPlayer._curSceneId == folData.m_SceneType)
					{
						if (m < followTarget_num[TargetID].Length && m >= 0)
                        {
                            npc.NpcCmpt.MisstionAskStop = false;
                            followTarget_num[TargetID][m] = 1;
                        }							
					}
				}
				else
				{
					if (folData.m_SceneType == (int)SingleGameStory.curType)
					{
                        if (m < followTarget_num[TargetID].Length && m >= 0)
                        {
                            npc.NpcCmpt.MisstionAskStop = false;
                            followTarget_num[TargetID][m] = 1;
                        }
					}
				}
                if (npcWaitTime.ContainsKey(npc.Id))
                    npcWaitTime.Remove(npc.Id);
            }
        }

        if (!Array.Exists(followTarget_num[TargetID], ite => ite != 1))
        {
			if (folData.m_isNeedPlayer && Vector3.Distance(StroyManager.Instance.GetPlayerPos(), disPos) > folData.m_DistRadius)
                return false;
            CompleteTarget(TargetID, MissionID);
            if (followTarget_num.ContainsKey(TargetID))
                followTarget_num.Remove(TargetID);

            return true;
        }

        return false;
    }

    public bool UpdateSearchMission(int MissionID, int TargetID)
    {
//        Vector3 pos = Vector3.zero;
        TypeSearchData searchData = MissionManager.GetTypeSearchData(TargetID);
        if (searchData == null)
            return false;

        if (PeGameMgr.IsAdventure)
        {
            if (searchData.m_notForDungeon && PeCreature.Instance.mainPlayer.position.y < 0)
                return false;
        }

        if (MissionID == m_SpecialMissionID64) 
        {
            if (RailWayMissionIsCompleted(searchData) && !HadCompleteTarget(TargetID)) 
            {
                if(PeGameMgr.IsMultiStory)
				{
					if (searchData.m_SceneType == PlayerNetwork.mainPlayer._curSceneId)
					{
						CompleteTarget(TargetID, MissionID);
						m_PlayerMission.UpdateAllNpcMisTex();
						
						return true;
					}
				}
				else
				{
					if (searchData.m_SceneType == (int)SingleGameStory.curType)
	                {
	                    CompleteTarget(TargetID, MissionID);
	                    m_PlayerMission.UpdateAllNpcMisTex();

	                    return true;
	                }
				}
            }
            return false;
        }

        PeEntity player = PeCreature.Instance.mainPlayer;
        if (MissionID == m_SpecialMissionID90 || MissionID == m_SpecialMissionID91)
        {
            if (player.IsOnCarrier())
            {
                FailureMission(MissionID);
                return false;
            }
        } 
        float dis = 0;
        if (searchData.m_NpcID != 0)
        {
            PeEntity npc = EntityMgr.Instance.Get(searchData.m_NpcID);
            if (npc == null)
                return false;

            dis = Vector3.Distance(npc.position, StroyManager.Instance.GetPlayerPos());
            if (HasRandomMission(MissionID))
            {
                Vector2 v1 = new Vector2(npc.ExtGetPos().x, npc.ExtGetPos().z);
                Vector2 v2 = new Vector2(StroyManager.Instance.GetPlayerPos().x, StroyManager.Instance.GetPlayerPos().z);
                dis = Vector2.Distance(v1, v2);
            }

        }
        else
        {
            dis = Vector3.Distance(StroyManager.Instance.GetPlayerPos(), searchData.m_DistPos);
            if (HasRandomMission(MissionID))
            {
                Vector2 v1 = new Vector2(searchData.m_DistPos.x, searchData.m_DistPos.z);
                Vector2 v2 = new Vector2(StroyManager.Instance.GetPlayerPos().x, StroyManager.Instance.GetPlayerPos().z);
                dis = Vector2.Distance(v1, v2);
            }
        }

        if (dis < searchData.m_DistRadius && !HadCompleteTarget(TargetID))
        {
			if(PeGameMgr.IsMultiStory)
			{
				if (searchData.m_SceneType == PlayerNetwork.mainPlayer._curSceneId)
				{
					CompleteTarget(TargetID, MissionID);
					m_PlayerMission.UpdateAllNpcMisTex();
					
					return true;
				}
			}
			else
			{
				if (searchData.m_SceneType == (int)SingleGameStory.curType)
	            {
	                CompleteTarget(TargetID, MissionID);
	                m_PlayerMission.UpdateAllNpcMisTex();

	                return true;
	            }
			}
        }

        return false;
    }

    bool RailWayMissionIsCompleted(TypeSearchData searchData) 
    {
        if(!CSMain.HasCSAssembly())
            return false;
        bool conoly = false;
        bool destination = false;
        foreach (var item in Railway.Manager.Instance.GetRoutes())
        {
            for (int i = 0; i < item.pointCount; i++)
			{
                Vector3 v;
                Railway.Point point = item.GetPointByIndex(i);
                if (point.pointType == Railway.Point.EType.Joint)
                    continue;
                CSMain.GetAssemblyPos(out v);
                if (Vector3.Distance(point.position, v) < searchData.m_DistRadius)
                    conoly = true;
                if (Vector3.Distance(point.position, searchData.m_DistPos) < searchData.m_DistRadius)
                    destination = true;
			}
            if (conoly == true && destination == true && item.train != null)
                return true;
        }
        return false;
    }

    void UpdateNpcTalk(List<TalkInfo> talkInfos)
    {
        foreach (var item in talkInfos)
        {
            if (iHadTalkedMap.Contains(item.talkid[0]))
                continue;
            if (Vector3.Distance(PeCreature.Instance.mainPlayer.position, item.pos) <= item.radius)
            {
                GameUI.Instance.mNPCTalk.SpTalkSymbol(true);
                GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(item.talkid, null, false);

                if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    GameUI.Instance.mNPCTalk.SPTalkClose();

                if (!iHadTalkedMap.Contains(item.talkid[0]))
                    iHadTalkedMap.Add(item.talkid[0]);
            }
        }
    }

    public static void RPC_S2C_SetMonsterLeft(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int nMissionID;
//        int num;
        bool flag = false;
        nMissionID = stream.Read<int>();
        /*num = */stream.Read<int>();
        //  flag = stream.Read<bool>();
        MissionCommonData data = GetMissionCommonData(nMissionID);
        if (null == data)
            return;
        //GameGui_N.Instance.mMissionTrackGui.SetMonsterLeft(nMissionID, num);
        if (flag)
        {

        }
    }
}

public class MisRepositoryArchiveMgr : ArchivableSingleton<MisRepositoryArchiveMgr>
{
    const string ArchiveKey = "ArchiveKeyMisRepository";

    protected override bool GetYird()
    {
        return false;
    }

    protected override string GetArchiveKey()
    {
        return ArchiveKey;
    }

    public override void New()
    {
        base.New();

        if (PeGameMgr.IsStory)
        {
            if (PeCreature.Instance == null || PeCreature.Instance.mainPlayer == null)
            {
                //Debug.LogError("storymode error,cant load missionmgr");
                GlobalBehaviour.RegisterEvent(WaitForAssets);
                return;
            }
            MotionMgrCmpt mmc = PeCreature.Instance.mainPlayer.motionMgr;
            if (mmc == null)
                return;

            mmc.DoAction(PEActionType.Lie);
            if (PeGameMgr.IsMultiStory)
                MissionManager.Instance.Invoke("MultiGetUp",5f);
            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(1, 1);
            GameUI.Instance.mNPCTalk.PreShow();
        }
        else if (PeGameMgr.IsSingleAdventure)
        {
			MissionManager.Instance.StartCoroutine(AdventureInit());
        }
    }

	IEnumerator AdventureInit()
	{
		while (VArtifactTownManager.Instance==null) 
		{
			yield return 0;
		}
		while (VArtifactTownManager.Instance.missionStartNpcEntityId<0) 
		{
			yield return 0;
		}
		while (!EntityMgr.Instance.Get(VArtifactTownManager.Instance.missionStartNpcEntityId)) 
		{
			yield return 0;
		}
		PeEntity npc = EntityMgr.Instance.Get(VArtifactTownManager.Instance.missionStartNpcEntityId);
		GameUI.Instance.mNpcWnd.m_CurSelNpc = npc;
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(9027, 1);
		GameUI.Instance.mNPCTalk.PreShow(); 
		MissionManager.Instance.m_PlayerMission.UpdateAllNpcMisTex();
        AdventureCampNpc();
    }

    static readonly int[] equipment = new int[] { 97, 115, 133, 151, 75, 85, 172, 181, 194, 212, 55 };
    void AdventureCampNpc()
    {
        for (int i = 1; i < RandomMapConfig.allyCount; i++)
        {
            AllyType type = VATownGenerator.Instance.GetAllyType(i);
            PeEntity npc = null;
            NpcMissionData useData = new NpcMissionData();
            switch (type)
            {
                case AllyType.Puja:
                    npc = PeCreature.Instance.CreateNpc(20000 - i, 68, Vector3.zero, Quaternion.identity, Vector3.one);
                    break;
                case AllyType.Paja:
                    npc = PeCreature.Instance.CreateNpc(20000 - i, 69, Vector3.zero, Quaternion.identity, Vector3.one);
                    break;
                case AllyType.Npc:
                    npc = PeCreature.Instance.CreateNpc(20000 - i, 70, Vector3.zero, Quaternion.identity, Vector3.one);
                    PeEntityCreator.InitEquipment(npc, equipment);
                    break;
                default:
                    continue;
            }
            int campId = VATownGenerator.Instance.GetPlayerId(i);
            npc.SetAttribute(AttribType.DefaultPlayerID, campId);
            string name = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(i));
            npc.ExtSetName(new CharacterName(name));
            useData.m_MissionList.Add(9135);
            useData.m_MissionList.Add(9136);
            useData.m_MissionList.Add(9137);
            useData.m_MissionList.Add(9138);
            useData.m_MissionListReply.Add(9137);
            useData.m_MissionListReply.Add(9138);
            npc.SetUserData(useData);
            NpcMissionDataRepository.AddMissionData(npc.Id, useData);
        }
    }

    void MultiGetUp() 
    {
        PeCreature.Instance.mainPlayer.motionMgr.EndAction(PEActionType.Lie);
    }

    //public void DienNew()
    //{
    //    MissionCommonData data = MissionManager.GetMissionCommonData(641);
    //    if (data == null)
    //        return;

    //    PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
    //    MissionManager.Instance.SetGetTakeMission(641, npc, MissionManager.TakeMissionType.TakeMissionType_Get, false);
    //}

    public static bool WaitForAssets()
    {
        if (PeCreature.Instance == null || PeCreature.Instance.mainPlayer == null)
        {
            return false;
        }

        MotionMgrCmpt mmc = PeCreature.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
        if (mmc == null)
            return false;
        if (PeGameMgr.IsMultiStory && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._initOk)
        {
            if (PlayerNetwork.mainPlayer._gameStarted)
                return true;
            else
            {
                //mmc.DoAction(PEActionType.Lie);
                GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(1, 1);
                GameUI.Instance.mNPCTalk.PreShow(); 
                if (PlayerNetwork.mainPlayer != null)
                {
                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GameStarted);
                }
                return true;
            }
        }
        return false;
    }

    protected override void SetData(byte[] data)
    {
        if (data == null)
            return;

        MissionManager.Instance.m_PlayerMission.Import(data);
    }

    protected override void WriteData(BinaryWriter bw)
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;

        MissionManager.Instance.m_PlayerMission.Export(bw);
    }
}



