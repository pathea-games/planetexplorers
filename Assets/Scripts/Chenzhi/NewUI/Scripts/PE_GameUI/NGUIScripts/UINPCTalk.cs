using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using AiAsset;
using System;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;

public class UINPCTalk : UIBaseWidget
{
    public enum NormalOrSp
    {
        Normal,
        SP,
        halfSP
    }
    public UISlicedSprite mBg;

    public UILabel mContent;
    public UILabel mName;
    public UISprite mNpcBigHeadSp;
    public UITexture mNpcBigHeadTex;
    public UISprite mMicroPhoneSp;
    public UIPanel mTuition;

    public UITable mUITable;
    public GameObject mCloseBtn;

    public GameObject mTalkOnlyWnd;
    public MissionSelItem_N mPrefab;
    public NormalOrSp type;            //正常对话0，SP对话1
    public bool canClose = true;
    public bool isPlayingTalk = false;
    List<MissionSelItem_N> mSelList = new List<MissionSelItem_N>();

    bool mUpdateList = false;

    //int HeroMissionID;

    public AudioController currentAudio = null;     //当前对话的AudioController
    bool spTalkEndByAudioTime = false;              //当前对话的时间长度

    public UIScrollBar mScrollBar;

    public Collider mDragCollider;

    public static bool m_QuickZM = false;

    public struct NpcTalkInfo
    {
        public int talkid;
        public Texture2D npcicon;
        public int npcid;
        public List<int> otherNpc;
        public string desc;
        public int soundid;
        public string clip;
        public bool isRadio;
        public int needLangSkill;
        public object talkToNpcidOrVecter3;
        public object moveToNpcidOrVecter3;
        public int moveType;
        public List<int> endOtherNpc;

        public int missionTrigger;
        public MissionManager.TakeMissionType type;
    };

    public List<NpcTalkInfo> m_NpcTalkList = new List<NpcTalkInfo>();
    private int m_CurTalkIdx;
    private AudioSource m_Audio;
    private bool m_bMutex;
    private List<int> m_SelectMissionList = new List<int>();
    public int m_selectMissionSource;
    public PeEntity m_CurTalkNpc;

    //network
    //bool bupdate = false;

    public void UpdateNpcTalkInfo(List<int> tmpList, MissionCommonData data = null, bool IsClearTalkList = true)
    {
        //HeroMissionID = -1;
        if (IsClearTalkList)
            ClearNpcTalkInfos();
        MatchTalkInfo(tmpList, data, IsClearTalkList);
    }

    public void ClearNpcTalkInfos()
    {
        foreach (NpcTalkInfo npc in m_NpcTalkList)
        {
            if (npc.npcicon != null)
            {
                Texture2D.Destroy(npc.npcicon);
            }
        }

        m_NpcTalkList.Clear();
    }

    void SetNpcBigHeadSp(string spName)
    {
        mNpcBigHeadSp.spriteName = spName;
        mNpcBigHeadSp.MakePixelPerfect();
    }

    void ShowNpcMicroPhoneSp(bool _show)
    {
        mMicroPhoneSp.enabled = _show;
    }

    void GetTalkInfo2(MissionCommonData data, ref List<int> talklist)
    {
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (curType == TargetType.TargetType_Follow)
            {
                TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                if (folData == null)
                    continue;

                for (int m = 0; m < folData.m_TalkInfo.Count; m++)
                {
                    for (int k = 0; k < folData.m_TalkInfo[m].talkid.Count; k++)
                        talklist.Add(folData.m_TalkInfo[m].talkid[k]);
                }
            }
        }

        for (int i = 0; i < data.m_TalkIN.Count; i++)
        {
            //if (data.m_TalkIN[i] == 0)
            //    continue;

            talklist.Add(data.m_TalkIN[i]);
        }
    }

    void GetTalkInfo3(MissionCommonData data, int targetid, ref List<int> talklist)
    {
        TargetType curType;
        TypeSearchData seaData;
        TypeFollowData folData;

        if (data.m_Type == MissionType.MissionType_Mul)
        {
            int count = 0;
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                if (MissionManager.Instance.IsReplyTarget(data.m_ID, data.m_TargetIDList[i]))
                    count++;
            }

            count--;

            if (count < 0)
                return;

            if (data.m_TalkED.Count > count)
                talklist.Add(data.m_TalkED[count]);
        }
        else
        {
            curType = MissionRepository.GetTargetType(targetid);
            if (curType == TargetType.TargetType_Follow)
            {
                folData = MissionManager.GetTypeFollowData(targetid);
                if (folData != null)
                {
                    for (int m = 0; m < folData.m_ComTalkID.Count; m++)
                        talklist.Add(folData.m_ComTalkID[m]);
                }
            }
            else if (curType == TargetType.TargetType_Discovery)
            {
                seaData = MissionManager.GetTypeSearchData(targetid);
                if (seaData != null)
                {
                    for (int m = 0; m < seaData.m_TalkID.Count; m++)
                        talklist.Add(seaData.m_TalkID[m]);
                }
            }

            for (int i = 0; i < data.m_TalkED.Count; i++)
                talklist.Add(data.m_TalkED[i]);
        }

    }

    void GetTalkInfo4(MissionCommonData data, int targetid, bool bFailed, ref List<int> talklist)
    {
        TypeUseItemData useData = MissionManager.GetTypeUseItemData(targetid);
        if (useData == null)
            return;

        if (!bFailed)
        {
            talklist = new List<int>();
            for (int i = 0; i < useData.m_UsedPrompt.Count; i++)
                talklist.Add(useData.m_UsedPrompt[i]);

            for (int i = 0; i < useData.m_TalkID.Count; i++)
                talklist.Add(useData.m_TalkID[i]);
        }
        else
            talklist = useData.m_FailPrompt;
    }

    void GetTalkInfo5(MissionCommonData data, int targetid, ref List<int> talklist)
    {
        TargetType curType;
        TypeSearchData seaData;
        TypeFollowData folData;

        curType = MissionRepository.GetTargetType(targetid);
        if (curType == TargetType.TargetType_Follow)
        {
            folData = MissionManager.GetTypeFollowData(targetid);
            if (folData == null)
                return;

            for (int m = 0; m < folData.m_ComTalkID.Count; m++)
                talklist.Add(folData.m_ComTalkID[m]);
        }
        else if (curType == TargetType.TargetType_Discovery)
        {
            seaData = MissionManager.GetTypeSearchData(targetid);
            if (seaData == null)
                return;

            for (int m = 0; m < seaData.m_TalkID.Count; m++)
                talklist.Add(seaData.m_TalkID[m]);
        }
    }

    List<int> GetTalkInfo(int MissionID, byte type, MissionCommonData data, int targetid, bool bFailed)
    {
        List<int> talklist = null;
        if (MissionID == -1)
        {
            talklist = new List<int>();
            int initid = 1;

            talklist.Add(initid);
        }
        else
        {
            switch (type)
            {
                case 1:
                    talklist = data.m_TalkOP;
                    break;
                case 2:
                    talklist = new List<int>();
                    GetTalkInfo2(data, ref talklist);
                    break;
                case 3:
                    talklist = new List<int>();
                    GetTalkInfo3(data, targetid, ref talklist);
                    break;
                case 4:
                    talklist = new List<int>();
                    GetTalkInfo4(data, targetid, bFailed, ref talklist);
                    break;
                case 5:
                    talklist = new List<int>();
                    GetTalkInfo5(data, targetid, ref talklist);
                    break;
                case 6:
                    talklist = data.m_PromptOP;
                    break;
                case 7:
                    talklist = data.m_PromptIN;
                    break;
                case 8:
                    talklist = data.m_PromptED;
                    break;
                default:
                    talklist = new List<int>();
                    break;
            }
        }

        return talklist;
    }

    public void GetTalkInfo(int talkid, ref NpcTalkInfo talkinfo, MissionCommonData data)
    {
        TalkData talkdata;

        talkinfo.talkid = talkid;
        talkdata = TalkRespository.GetTalkData(talkid);
        if (talkdata == null)
            return;

        talkinfo.soundid = talkdata.m_SoundID;
        talkinfo.clip = talkdata.m_ClipName;
        talkinfo.needLangSkill = talkdata.needLangSkill;
        talkinfo.desc = ParseStrDefine(talkdata.m_Content, data, talkinfo.needLangSkill);
        talkinfo.isRadio = talkdata.isRadio;
        talkinfo.talkToNpcidOrVecter3 = talkdata.talkToNpcidOrVecter3;
        talkinfo.moveToNpcidOrVecter3 = talkdata.moveTonpcidOrvecter3;
        talkinfo.otherNpc = talkdata.m_otherNpc;
        talkinfo.endOtherNpc = talkdata.m_endOtherNpc;
        talkinfo.moveType = talkdata.m_moveType;

        talkinfo.npcid = talkdata.m_NpcID;

        //if (data != null && data.m_ID == MissionManager.m_SpecialMissionID47)
        //{
        //    if (GameUI.Instance.mNpcWnd.m_CurSelNpc == null)
        //        return;

        //    talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;

        //    int misID = 0;
        //    NpcMissionData missionData = GameUI.Instance.mNpcWnd.m_CurSelNpc.GetUserData() as NpcMissionData;
        //    if (null != missionData)
        //    {
        //        if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID13))
        //            misID = MissionManager.m_SpecialMissionID13;
        //        else if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID14))
        //            misID = MissionManager.m_SpecialMissionID14;
        //        else if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID15))
        //            misID = MissionManager.m_SpecialMissionID15;
        //    }
        //    HeroMissionID = misID;
        //}
        //else
        //{
        //    talkinfo.npcid = talkdata.m_NpcID;
        //}
    }

    public void ParseName(MissionCommonData data, ref NpcTalkInfo talkinfo)
    {
        if (talkinfo.npcid != MissionManager.m_TalkInfoPlayer)
        {
            if (talkinfo.npcid == 0)
            {
                if (data == null)
                    talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
                else if (MissionManager.HasRandomMission(data.m_ID) || data.m_ID == 9137 || data.m_ID == 9138)
                {
                    if (data.m_ID == GameUI.Instance.mNpcWnd.BtnClickMission)
                    {
                        if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
                            talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
                        else if (data.m_iNpc != 0)
                            talkinfo.npcid = data.m_iNpc;
                        else if (data == null && PeCreature.Instance.mainPlayer != null)
                            talkinfo.npcid = PeCreature.Instance.mainPlayer.Id;
                    }
                    else
                    {
                        if (data.m_iNpc != 0)
                            talkinfo.npcid = data.m_iNpc;
                        else if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
                            talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
                        else if (data == null && PeCreature.Instance.mainPlayer != null)
                            talkinfo.npcid = PeCreature.Instance.mainPlayer.Id;
                    }
                }
                else
                {
                    if (data.m_iReplyNpc != 0)
                        talkinfo.npcid = data.m_iReplyNpc;
                    else if (data.m_iNpc != 0)
                        talkinfo.npcid = data.m_iNpc;
                    else if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
                        talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
                    else if (data == null && PeCreature.Instance.mainPlayer != null)
                        talkinfo.npcid = PeCreature.Instance.mainPlayer.Id;
                }
            }
        }
        else if (PeCreature.Instance.mainPlayer != null)
            talkinfo.npcid = PeCreature.Instance.mainPlayer.Id;
    }

    public string ParseStrDefine(string content, MissionCommonData data, int needLangSkill = 0)
    {
        if (content == null)
            return "";

        string monstername = "\"monsterid%\"";
        string monsternum = "\"monsternum%\"";
        //        string killmonname = "\"killedmosternum%\"";

        string pos = "\"position%\"";
        string npc1 = "\"npcid1%\"";
        string npc2 = "\"npcid2%\"";
        string npc3 = "\"npcid3%\"";
        string npclist = "\"npclist%\"";
        string npcnum = "\"npcnum%\"";

        string itemname = "\"itemid%\"";
        string itemnum = "\"itemnum%\"";
        string targetitem = "\"targetitemid%\"";

        string givemisnpc = "\"givenpcid%\"";
        string receivenpc = "\"receivenpcid%\"";
        //        string giveitemname = "\"giveitemid%\"";
        //        string giveitemnum = "\"giveitemnum%\"";

        string ritemnum = "\"n-ri%\"";
        string ritemname = "\"ri%\"";
        string playername = "\"name%\"";
        string seedname = "\"seedname%\"";
        string npcDead = "\"npcdead%\"";
        string npcName = "\"npc_name%\"";
        string adNpcName = "\"AdvNPC%\"";
        string adTownName = "\"Town%\"";
        string adCampName = "\"AI%\"";
        string colonyNum = "\"colonistnum%\"";
        string enemyCamp = "\"EnemyCamp%\"";

        PeEntity npc;
        if (data != null)
        {
            for (int m = 0; m < data.m_TargetIDList.Count; m++)
            {
                TypeMonsterData monData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[m]);
                TypeCollectData colData = MissionManager.GetTypeCollectData(data.m_TargetIDList[m]);
                TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[m]);
                TypeSearchData seaData = MissionManager.GetTypeSearchData(data.m_TargetIDList[m]);
                TypeUseItemData useData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[m]);
                TypeMessengerData mesData = MissionManager.GetTypeMessengerData(data.m_TargetIDList[m]);
                //                TypeTowerDefendsData towData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[m]);

                if (monData != null)
                {
                    if (content.Contains(monstername))
                        content = content.Replace(monstername, AiDataBlock.GetAIDataName(monData.m_MonsterID));

                    if (content.Contains(monsternum))
                        content = content.Replace(monsternum, monData.m_MonsterNum.ToString());
                }
                else if (colData != null)
                {
                    if (content.Contains(itemname))
                        content = content.Replace(itemname, ItemAsset.ItemProto.GetName(colData.ItemID));

                    if (content.Contains(itemnum))
                        content = content.Replace(itemnum, colData.ItemNum.ToString());

                    if (content.Contains(targetitem))
                        content = content.Replace(targetitem, colData.m_TargetItemID.ToString());

                    string strName = "";
                    npc = EntityMgr.Instance.Get(data.m_iNpc);
                    if (npc != null)
                        strName = npc.ExtGetName();

                    if (content.Contains(npc1))
                        content = content.Replace(npc1, strName);
                }
                else if (folData != null)
                {
                    if (content.Contains(pos))
                        content = content.Replace(pos, folData.m_DistPos.ToString());


                    for (int i = 0; i < folData.m_iNpcList.Count; i++)
                    {
                        if (i == 0 && content.Contains(npc1))
                        {
                            npc = EntityMgr.Instance.Get(folData.m_iNpcList[i]);
                            if (npc != null)
                                content = content.Replace(npc1, npc.ExtGetName());
                        }
                        else if (i == 1 && content.Contains(npc2))
                        {
                            npc = EntityMgr.Instance.Get(folData.m_iNpcList[i]);
                            if (npc != null)
                                content = content.Replace(npc2, npc.ExtGetName());
                        }
                        else if (i == 2 && content.Contains(npc3))
                        {
                            npc = EntityMgr.Instance.Get(folData.m_iNpcList[i]);
                            if (npc != null)
                                content = content.Replace(npc3, npc.ExtGetName());
                        }
                    }
                }
                else if (seaData != null)
                {
                    if (content.Contains(pos))
                        content = content.Replace(pos, seaData.m_DistPos.ToString());

                    if (content.Contains(npc1))
                    {
                        npc = EntityMgr.Instance.Get(seaData.m_NpcID);
                        if (npc != null)
                            content = content.Replace(npc1, npc.ExtGetName());
                    }
                }
                else if (useData != null)
                {
                    if (content.Contains(pos))
                        content = content.Replace(pos, useData.m_Pos.ToString());

                    if (content.Contains(itemname))
                        content = content.Replace(itemname, ItemAsset.ItemProto.GetName(useData.m_ItemID));

                    if (content.Contains(itemnum))
                        content = content.Replace(itemnum, useData.m_UseNum.ToString());
                }
                else if (mesData != null)
                {
                    if (content.Contains(givemisnpc))
                    {
                        npc = EntityMgr.Instance.Get(mesData.m_iNpc);
                        if (npc != null)
                            content = content.Replace(givemisnpc, npc.ExtGetName());
                    }

                    if (content.Contains(receivenpc))
                    {
                        npc = EntityMgr.Instance.Get(mesData.m_iReplyNpc);
                        if (npc != null)
                            content = content.Replace(receivenpc, npc.ExtGetName());
                    }

                    if (content.Contains(itemname))
                        content = content.Replace(itemname, ItemAsset.ItemProto.GetName(mesData.m_ItemID));

                    if (content.Contains(itemnum))
                        content = content.Replace(itemnum, mesData.m_ItemNum.ToString());
                }
            }

            if (data.m_Com_RewardItem.Count > 0)
            {
                if (content.Contains(ritemnum))
                {
                    content = content.Replace(ritemnum, data.m_Com_RewardItem[0].num.ToString());
                }

                if (content.Contains(ritemname))
                {
                    content = content.Replace(ritemname, ItemAsset.ItemProto.GetName(data.m_Com_RewardItem[0].id));
                }
            }

            if (data.m_iColonyNpcList.Count > 1)
            {
                TalkData tdtmp = TalkRespository.GetTalkData(2174);
                if (tdtmp != null)
                {
                    string strtmp = " ";
                    for (int i = 1; i < data.m_iColonyNpcList.Count; i++)
                    {
                        npc = EntityMgr.Instance.Get(data.m_iColonyNpcList[i]);
                        if (npc == null)
                            continue;

                        if (npc.IsRecruited())
                            continue;

                        strtmp += npc.ExtGetName();

                        if (i < data.m_iColonyNpcList.Count - 1)
                        {
                            if (i == data.m_iColonyNpcList.Count - 2)
                                strtmp += "and ";
                            else
                                strtmp += ", ";
                        }
                    }

                    if (strtmp != " ")
                    {
                        content += tdtmp.m_Content;
                        content = content.Replace(npclist, strtmp);
                    }
                }
            }
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

        if (content.Contains(npcnum))
        {
            content = content.Replace(npcnum, StroyManager.Instance.GetMgCampNpcCount().ToString());
        }

        if (content.Contains(playername))
        {
            content = content.Replace(playername, Pathea.PeCreature.Instance.mainPlayer.ToString());
        }

        if (content.Contains(seedname))
        {
            content = content.Replace(seedname, RandomMapConfig.SeedString);
        }

        if (content.Contains(npcDead))
        {
            string tmp = "";
            StroyManager.deadNpcsName.ForEach(delegate (string s)
            {
                tmp += s + " ";
            });
            content = content.Replace(npcDead, tmp);
        }

        if (content.Contains(npcName))
        {
            string npcID;
            int n = content.IndexOf(npcName);
            if (content.Length >= n + 15)
            {
                npcID = content.Substring(n + 11, 4);
                if (PETools.PEMath.IsNumeral(npcID))
                {
                    npc = EntityMgr.Instance.Get(Convert.ToInt32(npcID));
                    if (npc != null)
                    {
                        content = content.Replace(npcName + npcID, npc.name.Substring(0, npc.name.Length - 1 - System.Convert.ToString(npc.Id).Length));
                    }
                    else if (MissionManager.Instance.m_PlayerMission.recordNpcName.ContainsKey(Convert.ToInt32(npcID)))
                    {
                        content = content.Replace(npcName + npcID, MissionManager.Instance.m_PlayerMission.recordNpcName[Convert.ToInt32(npcID)]);
                    }
                }
            }
        }

        if (content.Contains(colonyNum))
        {
            Vector3 v;
            if (CSMain.GetAssemblyPos(out v))
            {
                List<PeEntity> npcs = new List<PeEntity>(EntityMgr.Instance.All).FindAll(delegate (PeEntity e)
                {
                    if (e.proto == EEntityProto.Npc || e.proto == EEntityProto.RandomNpc || e.proto == EEntityProto.Player)
                    {
                        if (Vector3.Distance(v, e.position) < 250)
                            return true;
                    }
                    return false;
                });
                content = content.Replace(colonyNum, npcs.Count.ToString());
            }
            else
            {
                content = content.Replace(colonyNum, "a few");
            }
        }

        if (content.Contains(enemyCamp))
        {
            string enemyCampName = "";
            int playerId = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
            for (int i = 1; i < RandomMapConfig.allyCount; i++)
            {
                int targetId = VATownGenerator.Instance.GetPlayerId(i);
                if (ReputationSystem.Instance.GetReputationLevel(playerId, targetId) < ReputationSystem.ReputationLevel.Neutral)
                {
                    enemyCampName = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(i));
                    break;
                }
            }
            if (string.IsNullOrEmpty(enemyCamp))
            {
                for (int i = 0; i < RandomMapConfig.allyCount; i++)
                {
                    if (VATownGenerator.Instance.GetAllyType(i) == AllyType.Npc)
                    {
                        enemyCampName = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(i));
                        break;
                    }
                }
            }
            content = content.Replace(enemyCamp, enemyCampName);
        }

        if (needLangSkill > MissionManager.Instance.m_PlayerMission.LanguegeSkill)
        {
            string replace = "#";
            int percent = needLangSkill - MissionManager.Instance.m_PlayerMission.LanguegeSkill;
            int fuhaoNum = content.Split(',', '.', '!', '?', '，', '。', '！', '？', '\'').Length - 1;
            int num = (content.Length - fuhaoNum) * percent / needLangSkill;
            System.Random random = new System.Random();
            int n;
			if (!SystemSettingData.Instance.IsChinese)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[A-Za-z]");
                if (MissionManager.Instance.m_PlayerMission.LanguegeSkill == 0)
                {
                    for (int i = 0; i < content.Length; i++)
                    {
                        if (regex.IsMatch(Convert.ToString(content[i])))
                        {
                            content = content.Remove(i, 1);
                            content = content.Insert(i, replace);
                        }
                    }
                }
                else
                {
                    while (num > 0)
                    {
                        n = random.Next(content.Length);
                        if (regex.IsMatch(Convert.ToString(content[n])))
                        {
                            content = content.Remove(n, 1);
                            content = content.Insert(n, replace);
                            num--;
                        }
                    }
                }
            }
			else	// chinese
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[\u4e00-\u9fa5]");
                if (MissionManager.Instance.m_PlayerMission.LanguegeSkill == 0)
                {
                    for (int i = 0; i < content.Length; i++)
                    {
                        if (regex.IsMatch(Convert.ToString(content[i])))
                        {
                            content = content.Remove(i, 1);
                            content = content.Insert(i, replace);
                        }
                    }
                }
                else
                {
                    while (num > 0)
                    {
                        n = random.Next(content.Length);
                        if (regex.IsMatch(Convert.ToString(content[n])))
                        {
                            content = content.Remove(n, 1);
                            content = content.Insert(n, replace);
                            num--;
                        }
                    }
                }
            }
        }
        return content;
    }

    public void UpdateNpcTalkInfo(int MissionID, byte type, int targetid = 0, bool bFailed = false)
    {
        if (GameUI.Instance.mNpcWnd.isShow)
            GameUI.Instance.mNpcWnd.Hide();

        //HeroMissionID = -1;

        ClearNpcTalkInfos();

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
        {
            Debug.LogError("MissionData Error: Couldn't find DataBlock");
            return;
        }

        List<int> talklist = GetTalkInfo(MissionID, type, data, targetid, bFailed);

        if (talklist.Count == 0)
            return;


        if (MissionID == MissionManager.m_SpecialMissionID9 && PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>().GetServantNum() >= ServantLeaderCmpt.mMaxFollower)
        {
            talklist = new List<int>();
            talklist.Add(1573);
        }

        if (type > 5)
            type -= 5;

        MatchTalkInfo(talklist, data, true, MissionID, (MissionManager.TakeMissionType)type);
    }

    public void AddNpcTalkInfo(int MissionID, byte type, int targetid = 0, bool bFailed = false, bool isClearTalkList = false)
    {
        if (GameUI.Instance.mNpcWnd.isShow)
            GameUI.Instance.mNpcWnd.Hide();

        //HeroMissionID = -1;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
        {
            Debug.LogError("MissionData Error: Couldn't find DataBlock");
            return;
        }

        List<int> talklist = GetTalkInfo(MissionID, type, data, targetid, bFailed);

        if (talklist.Count == 0)
            return;

        int n = type;
        n = n == 8 ? 3 : n > 3 ? 0 : n;

        MatchTalkInfo(talklist, data, isClearTalkList, MissionID, (MissionManager.TakeMissionType)n);
    }

    public void MatchTalkInfo(List<int> talklist, MissionCommonData data, bool IsClearTalkList = true, int triggerMission = -1,
        MissionManager.TakeMissionType type = MissionManager.TakeMissionType.TakeMissionType_Unkown)
    {
        for (int i = 0; i < talklist.Count; i++)
        {
            NpcTalkInfo talkinfo = new NpcTalkInfo();

            GetTalkInfo(talklist[i], ref talkinfo, data);
            ParseName(data, ref talkinfo);

            if (i == talklist.Count - 1)
            {
                talkinfo.missionTrigger = triggerMission;
                talkinfo.type = type;
            }
            else
            {
                talkinfo.missionTrigger = -1;
                talkinfo.type = MissionManager.TakeMissionType.TakeMissionType_Unkown;
            }
            m_NpcTalkList.Add(talkinfo);
        }
        //isPlayingTalk = true;
        if (IsClearTalkList)
            m_CurTalkIdx = 0;
        m_bMutex = false;
        if (m_bMutex)
        {
            mTalkOnlyWnd.SetActive(false);
            return;
        }
        else
        {
            mTalkOnlyWnd.SetActive(true);
            if (m_NpcTalkList.Count > m_CurTalkIdx)
            {
                PeEntity npc = EntityMgr.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
                if (npc == null && data != null && MissionManager.HasRandomMission(data.m_ID))
                {
                    if (PeGameMgr.IsStory)
                        npc = EntityMgr.Instance.Get(data.m_iNpc);
                    else if (PeGameMgr.IsAdventure)
                    {
                        if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(m_NpcTalkList[m_CurTalkIdx].npcid))
                            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[m_NpcTalkList[m_CurTalkIdx].npcid]);
                        else
                            npc = EntityMgr.Instance.Get(data.m_iNpc);
                    }
                    if (npc != null)
                    {
                        for (int i = 0; i < m_NpcTalkList.Count; i++)
                        {
                            if (m_NpcTalkList[i].npcid != 0)
                                continue;
                            NpcTalkInfo info = m_NpcTalkList[i];
                            info.npcid = npc.Id;
                            m_NpcTalkList[i] = info;
                        }
                    }
                }
                bool effected = true;
                switch (m_NpcTalkList[m_CurTalkIdx].npcid)
                {
                    case -9998:
                        mName.text = "Puja Commander";
                        break;
                    case -9997:
                        mName.text = "Puja Soldier";
                        break;
                    case -9996:
                        mName.text = "Tony";
                        break;
                    case -9995:
                        mName.text = "Tips";
                        break;
                    case -9994:
                        mName.text = "Paja";
                        break;
                    case -9993:
                        mName.text = "Puja";
                        break;
                    case -9992:
                        if (CSMain.HasCSAssembly())
                        {
                            List<PeEntity> tmp = CSMain.GetCSNpcs();
                            if (tmp.Count > 0)
                            {
                                SetNpcBigHeadSp(tmp[0].ExtGetFaceIconBig());
                                mName.text = tmp[0].ExtGetName();
                            }
                            else
                            {
                                if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(1))
                                    npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[1]);
                                if (npc != null)
                                {
                                    mName.text = npc.ExtGetName();
                                    SetNpcBigHeadSp(npc.ExtGetFaceIconBig());
                                }
                            }
                        }
                        break;
                    case -9991:
                        mName.text = "???";
                        break;
                    default:
                        effected = false;
                        break;
                }

                if (effected)
                {
                    SetNpcBigHeadSp("npc_big_Unknown");
                }
                else
                {
                    if (npc != null)
                    {
                        mName.text = npc.ExtGetName();
                        SetNpcBigHeadSp(npc.ExtGetFaceIconBig());
                    }
                    else
                    {
                        mName.text = "???";
                        SetNpcBigHeadSp("npc_big_Unknown");
                    }
                }
                
				mContent.text = m_NpcTalkList[m_CurTalkIdx].desc;
				NPCTalkHistroy.Instance.AddHistroy(mName.text, mContent.text);
                //lz-2016.08.31 检测NPC对话触发引导
                InGameAidData.CheckNpcTalk(m_NpcTalkList[m_CurTalkIdx].talkid);

                if (null != GameUI.Instance)
                {
                    //lz-2016.11.07 UI上的Tutorial提示统一管理检测
                    GameUI.Instance.CheckTalkIDShowTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
                }
            }
            ClearMissionItem();
        }
    }

    public void UpdateMoveNpc()
    {
        if (npcid_targetid.Count == 0)
            return;
        int npcid = 0;
        foreach (var item in npcid_targetid.Keys)
        {
            PeEntity npc = EntityMgr.Instance.Get(item);
            PeEntity target = EntityMgr.Instance.Get(npcid_targetid[item]);
            if (npc == null || target == null)
                continue;
            if (Vector3.Distance(npc.position, target.position) <= 7)
            {
                npcid = item;
                NpcReachToTalk(npc);
                break;
            }
        }
        if (npcid != 0)
            npcid_targetid.Remove(npcid);
    }

    public void NpcReachToTalk(PeEntity npc)
    {
        StroyManager.Instance.RemoveReq(npc, EReqType.TalkMove);
        if (!needTalkNpc.ContainsKey(npc))
            return;
        StroyManager.Instance.SetTalking(npc, (string)needTalkNpc[npc][0], needTalkNpc[npc][1]);
        needTalkNpc.Remove(npc);
    }

    List<PeEntity> movingNpc = new List<PeEntity>();
    Dictionary<PeEntity, object[]> needTalkNpc = new Dictionary<PeEntity, object[]>();
    Dictionary<int, int> npcid_targetid = new Dictionary<int, int>();

    public void PreShow()
    {
        if (m_NpcTalkList.Count == 0)
            return;

        if ((m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is Vector3) ||
            ((m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is int)
            && (int)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 != -1))
        {
            List<int> needMoveNpc = new List<int>();
            foreach (var item in m_NpcTalkList[m_CurTalkIdx].otherNpc)
                needMoveNpc.Add(item);
            if (m_NpcTalkList[m_CurTalkIdx].npcid > 0 && m_NpcTalkList[m_CurTalkIdx].npcid != PeCreature.Instance.mainPlayer.Id)
                needMoveNpc.Add(m_NpcTalkList[m_CurTalkIdx].npcid);
            List<int> needMoveNpcInRange = new List<int>();

            bool bPass = true;
            Vector3 dest = Vector3.zero;
            PeEntity _targetEntity = null;
            PeEntity npc;

            if (needMoveNpc.Count != 0)
            {
                if (m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is Vector3)
                    dest = (Vector3)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3;
                else if (m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is int)
                {
                    if ((int)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 == 0 && PeCreature.Instance != null)
                        _targetEntity = PeCreature.Instance.mainPlayer;
                    else
                    {
                        npc = EntityMgr.Instance.Get((int)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3);
                        if (npc != null)
                            _targetEntity = npc;
                    }
                    if (_targetEntity != null)
                        dest = _targetEntity.position;
                }
                if (dest == Vector3.zero || dest.y < -5f && (PeGameMgr.IsStory && (dest.x <= 0 || dest.z <= 0)))
                    Debug.LogError("UINPCTalk: not found moveto entity!");
                else
                {
                    foreach (var item in needMoveNpc)
                    {
                        npc = EntityMgr.Instance.Get(item);
                        if (npc == null)
                            continue;
                        if (Vector3.Distance(npc.position, dest) > 13)
                            bPass = false;
                        else
                            needMoveNpcInRange.Add(item);
                    }
                }
            }
            if (!bPass)
            {
                foreach (var item in needMoveNpcInRange)
                {
                    if (needMoveNpc.Contains(item))
                        needMoveNpc.Remove(item);
                }
                List<Vector3> dists = StroyManager.Instance.GetMeetingPosition(dest, needMoveNpc.Count, 5);
                for (int i = 0; i < needMoveNpc.Count; i++)
                {
                    npc = EntityMgr.Instance.Get(needMoveNpc[i]);
                    if (npc == null)
                        continue;
                    this.movingNpc.Add(npc);
                    object[] objs = new object[2];
                    objs[0] = m_NpcTalkList[m_CurTalkIdx].clip == "0" ? "" : m_NpcTalkList[m_CurTalkIdx].clip;
                    objs[1] = m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3;
                    if (!needTalkNpc.ContainsKey(npc))
                        needTalkNpc.Add(npc, objs);
                    StroyManager.Instance.TalkMoveTo(npc, dists[i], 1, true,
                        (SpeedState)(m_NpcTalkList[m_CurTalkIdx].moveType < 2 ? 2 : m_NpcTalkList[m_CurTalkIdx].moveType));
                    if (_targetEntity != null)
                    {
                        if (!npcid_targetid.ContainsKey(npc.Id))
                            npcid_targetid.Add(npc.Id, _targetEntity.Id);
                        else
                            npcid_targetid[npc.Id] = _targetEntity.Id;
                    }
                    if (npc.NpcCmpt != null)
                        npc.NpcCmpt.FixedPointPos = dists[i];
                }
            }
        }
        Show();
    }


    public override void Show()
    {
        if (m_NpcTalkList.Count == 0)
            return;
        if (m_NpcTalkList[m_CurTalkIdx].missionTrigger == MissionManager.m_SpecialMissionID9)
        {
            if (!CheckRandomNpc())
            {
                m_CurTalkNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
                m_CurTalkNpc.CmdStartTalk();
                if (type == NormalOrSp.Normal)
                    StroyManager.Instance.SetTalking(m_CurTalkNpc);
                base.Show();
                return;
            }
        }
        PeEntity npc;
        ChangeNpcState(true);
        base.Show();
        //if (type == NormalOrSp.SP || type == NormalOrSp.halfSP)
        //{
        //    int descLength = m_NpcTalkList[m_CurTalkIdx].desc.Length;
        //    int spTalkTime = descLength < 40 ? 2 : descLength < 80 ? 4 : descLength < 120 ? 6 : 8;
        //    Invoke("SPTalkClose", spTalkTime);
        //    mCloseBtn.SetActive(false);
        //}
        //else
        //{
        //    mCloseBtn.SetActive(true);
        //}
        if (m_bMutex)
        {
            if (m_CurTalkNpc == null)
                m_CurTalkNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
            StroyManager.Instance.SetTalking(m_CurTalkNpc);
            mTalkOnlyWnd.SetActive(false);
            if (!mUpdateList)
            {
                mUpdateList = true;
                Invoke("UpdateList", 0.1f);
            }
            isPlayingTalk = true;
            return;
        }
        else
        {
            if (m_NpcTalkList.Count == 0)
            {
                Hide();
                return;
            }

            mTalkOnlyWnd.SetActive(true);
            mContent.text = m_NpcTalkList[m_CurTalkIdx].desc;

            //lz-2016.08.31 检测NPC对话触发引导
            InGameAidData.CheckNpcTalk(m_NpcTalkList[m_CurTalkIdx].talkid);

            if(null!=GameUI.Instance)
            {
                //lz-2016.11.07 UI上的Tutorial提示统一管理检测
                GameUI.Instance.CheckTalkIDShowTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
            }

            if (m_NpcTalkList[m_CurTalkIdx].talkid == 1348)
                npc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
            else
                npc = EntityMgr.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
            if (npc != null)
            {
                mName.text = npc.ExtGetName();
                SetNpcBigHeadSp(npc.ExtGetFaceIconBig());
            }
			NPCTalkHistroy.Instance.AddHistroy(mName.text, mContent.text);
        }

        npc = EntityMgr.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
        if (PeGameMgr.IsAdventure)
        {
            if (npc == null && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + m_NpcTalkList[m_CurTalkIdx].npcid))
                npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + m_NpcTalkList[m_CurTalkIdx].npcid]);
        }

        if (m_NpcTalkList[m_CurTalkIdx].npcid != 0)
            isPlayingTalk = true;
		if (npc == null) 
		{
			if (type == NormalOrSp.SP || type == NormalOrSp.halfSP) {
				int descLength = m_NpcTalkList [m_CurTalkIdx].desc.Length;
				float spTalkTime = descLength < 40 ? 2.5f : descLength < 80 ? 5f : descLength < 120 ? 7.5f : 10f;
				Invoke ("SPTalkClose", spTalkTime);
				mCloseBtn.SetActive (false);
			} 
			else 
			{
				mCloseBtn.SetActive(true);
			}
			return;
		}
        m_CurTalkNpc = npc;

        if (m_NpcTalkList[m_CurTalkIdx].isRadio)
            ShowNpcMicroPhoneSp(true);
        else
        {
            if (type == NormalOrSp.Normal || type == NormalOrSp.halfSP)
            {
                string tmp = m_NpcTalkList[m_CurTalkIdx].clip;
                if (m_NpcTalkList[m_CurTalkIdx].clip == "0")
                {
                    System.Random r = new System.Random();
                    int n = r.Next(2);
                    tmp = n == 0 ? "Talk0" : n == 1 ? "Talk1" : tmp;
                }
                if ((m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3 is int ||
                    m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3 is Vector3 &&
                    ((Vector3)m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3) != Vector3.zero) &&
                    !needTalkNpc.ContainsKey(m_CurTalkNpc))
                    StroyManager.Instance.SetTalking(m_CurTalkNpc, tmp, m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3);
                foreach (var item in m_NpcTalkList[m_CurTalkIdx].otherNpc)
                {
                    PeEntity talkNpc = EntityMgr.Instance.Get(item);
                    if (talkNpc == null)
                        continue;
                    if (needTalkNpc.ContainsKey(talkNpc))
                        continue;
                    StroyManager.Instance.SetTalking(talkNpc, "", m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3);
                }
            }
        }
        GameUI.Instance.PlayNpcTalkWndOpenAudioEffect();
        currentAudio = StroyManager.Instance.PlaySound(npc, m_NpcTalkList[m_CurTalkIdx].soundid);
        if (type == NormalOrSp.SP || type == NormalOrSp.halfSP)
        {
            if (currentAudio != null)
            {
                //update to set end
            }
            else
            {
                int descLength = m_NpcTalkList[m_CurTalkIdx].desc.Length;
                float spTalkTime = descLength < 40 ? 2.5f : descLength < 80 ? 5f : descLength < 120 ? 7.5f : 10f;
                Invoke("SPTalkClose", spTalkTime);
            }
            mCloseBtn.SetActive(false);
        }
        else
        {
            mCloseBtn.SetActive(true);
        }
    }

    public bool CurTalkInfoIsRadio()
    {
        if (m_CurTalkIdx >= m_NpcTalkList.Count)
            return false;
        return m_NpcTalkList[m_CurTalkIdx].isRadio;
    }

    public void SPTalkClose()
    {
        canClose = true;
        OnClose();
    }

    public void SpTalkSymbol(bool spOrHalf)
    {
        if (isPlayingTalk == false)
            m_NpcTalkList.Clear();
        NpcTalkInfo tmp = new NpcTalkInfo();
        tmp.otherNpc = new List<int>();
        tmp.endOtherNpc = new List<int>();
        if (spOrHalf)
            tmp.npcid = 9999;
        else
            tmp.npcid = 9998;
        tmp.missionTrigger = -1;
        m_NpcTalkList.Add(tmp);
        m_NpcTalkList.Add(tmp);
    }

    public bool IsCanSkip()
    {
        if (type == NormalOrSp.Normal)
            return true;
        else
            return false;
    }

    public void OpenmShowTuition()
    {
        mTuition.gameObject.SetActive(true);
    }

    public void OnMutexBtnClick(int missionId, string content)
    {
        ClearMissionItem();
        MissionManager.Instance.m_PlayerMission.SetMission(m_selectMissionSource);
        if (PeGameMgr.IsMulti)
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_SetMission, m_selectMissionSource);
        StroyManager.Instance.RemoveReq(m_CurTalkNpc, EReqType.Dialogue);

		if(null != MainPlayer.Instance.entity)
			NPCTalkHistroy.Instance.AddHistroy(MainPlayer.Instance.entity.ExtGetName(), content);

        //改为有对话，弹出NPC对话内容，没有，直接接取任务
        if (MissionRepository.HaveTalkOP(missionId))
        {
            UpdateNpcTalkInfo(missionId, 1);
            if (m_NpcTalkList[0].npcid != MissionManager.m_TalkInfoPlayer)
            {
                PeEntity npc = EntityMgr.Instance.Get(m_NpcTalkList[0].npcid);
                if (npc == null)
                {
                    Debug.LogError("npc is null");
                    return;
                }
                npc.CmdFaceToPoint(PeCreature.Instance.mainPlayer.ExtGetPos());
                //npc.ApplySound(m_NpcTalkList[m_CurTalkIdx].soundid);
                currentAudio = StroyManager.Instance.PlaySound(npc, m_NpcTalkList[m_CurTalkIdx].soundid);

            }
        }
        else
        {
            this.Hide();
            isPlayingTalk = false;
            MissionManager.Instance.SetGetTakeMission(missionId, m_CurTalkNpc, MissionManager.TakeMissionType.TakeMissionType_Get);
        }
    }

    protected override void OnClose()
    {
        mScrollBar.scrollValue = 0;
        if ((type == NormalOrSp.SP || type == NormalOrSp.halfSP) && canClose == false)
            return;

        if (m_NpcTalkList.Count <= m_CurTalkIdx)
        {
            OnHide();
            isPlayingTalk = false;
            return;
        }


        ShowNpcMicroPhoneSp(false);
        canClose = false;
        //当前对话音效关闭
        if (currentAudio != null)
        {
            currentAudio.Delete();
            //lz-2016.10.16 AudioController.Delete之后会回收，引用要置空，不然会操作的其他的AudioController
            currentAudio = null;
            spTalkEndByAudioTime = false;
        }

        if (m_CurTalkNpc != null)
        {
            m_CurTalkNpc.CmdStopTalk();
            if (!GameUI.Instance.mNpcWnd.isShow && !GameUI.Instance.mShopWnd.isShopping)
                StroyManager.Instance.RemoveReq(m_CurTalkNpc, EReqType.Dialogue);
            if (needTalkNpc.ContainsKey(m_CurTalkNpc))
                needTalkNpc.Remove(m_CurTalkNpc);
        }
        PeEntity npc;
        foreach (var item in m_NpcTalkList[m_CurTalkIdx].endOtherNpc)
        {
            npc = EntityMgr.Instance.Get(item);
            if (npc == null)
                continue;
            StroyManager.Instance.RemoveReq(npc, EReqType.Dialogue);
            if (needTalkNpc.ContainsKey(npc))
                needTalkNpc.Remove(npc);
        }

        if (m_bMutex)
            return;

        if (m_NpcTalkList.Count == 0)
        {
            isPlayingTalk = false;
            OnHide();
            return;
        }

        PlotLensAnimation.CheckIsStopCamera(m_NpcTalkList[m_CurTalkIdx].talkid);
        if (m_NpcTalkList.Count > m_CurTalkIdx && m_CurTalkIdx >= 0)
            CheckTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);

        m_CurTalkIdx++;
        int curnpcid = -1;
        if (m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger != -1)
        {
            foreach (var item in movingNpc)
                StroyManager.Instance.RemoveReq(item, EReqType.TalkMove);
            movingNpc.Clear();
            needTalkNpc.Clear();
            npcid_targetid.Clear();

            if (EntityMgr.Instance.Get(m_NpcTalkList[m_CurTalkIdx - 1].npcid) != null)
            {
                MissionManager.Instance.SetGetTakeMission(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger,
                                                            EntityMgr.Instance.Get(m_NpcTalkList[m_CurTalkIdx - 1].npcid),
                                                            m_NpcTalkList[m_CurTalkIdx - 1].type);
            }
            else
            {
                if (m_CurTalkIdx >= m_NpcTalkList.Count)
                {
                    if (m_CurTalkIdx - 1 < m_NpcTalkList.Count && m_CurTalkIdx >= 1)
                    {
                        curnpcid = m_NpcTalkList[m_CurTalkIdx - 1].npcid;
                    }
                }
                else
                {
                    curnpcid = m_NpcTalkList[m_CurTalkIdx].npcid;
                }
                if (curnpcid <= -9800 && curnpcid >= -9900)
                {
                    if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + curnpcid))
                    {
                        curnpcid = MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + curnpcid];
                    }
                }
                MissionManager.Instance.SetGetTakeMission(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger,
                                                            EntityMgr.Instance.Get(curnpcid),
                                                            m_NpcTalkList[m_CurTalkIdx - 1].type);
            }
        }

        int npcid = -1;

        if (m_CurTalkIdx >= m_NpcTalkList.Count)
        {
            movingNpc.Clear();
            needTalkNpc.Clear();
            npcid_targetid.Clear();
            isPlayingTalk = false;

            //m_CurTalkIdx = 0;
            type = NormalOrSp.Normal;
            ChangeNpcState(false);
            //this.Hide();
            OnHide();
            isPlayingTalk = false;
            if (m_CurTalkIdx - 1 < m_NpcTalkList.Count && m_CurTalkIdx >= 1)
            {
                npcid = m_NpcTalkList[m_CurTalkIdx - 1].npcid;
                m_CurTalkNpc = EntityMgr.Instance.Get(npcid);
            }
            int currentMisID = m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger;
            MissionManager.TakeMissionType currentType = m_NpcTalkList[m_CurTalkIdx - 1].type;

            //bupdate = true;

            //if (PeGameMgr.IsMulti && !PeGameMgr.IsStory)
            //    return;

            GameUI.Instance.mNpcWnd.GetMutexID(currentMisID, ref m_SelectMissionList);
            if (m_SelectMissionList.Count > 0)
            {
                m_bMutex = true;
                ResetMissionItem();
                m_CurTalkIdx = 0;
                GameUI.Instance.mNPCTalk.Show();
                if (PeGameMgr.IsAdventure)
                {
                    for (int i = 0; i < m_SelectMissionList.Count; i++)
                    {
                        int inpcid = curnpcid;
                        if (inpcid <= 0)
                            inpcid = npcid;
                        if (PeGameMgr.IsMulti)
                            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, m_SelectMissionList[i], inpcid);
                        else
                            AdRMRepository.CreateRandomMission(m_SelectMissionList[i]);
                    }
                }
                return;
            }

            //弹出对话库里面最后一句话的NPC的任务窗口

            if (m_CurTalkNpc == null)
            {
                m_CurTalkNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
                if (m_CurTalkNpc == null)
                    return;
            }

            if (needTalkNpc.ContainsKey(m_CurTalkNpc))
                needTalkNpc.Remove(m_CurTalkNpc);
            if (MissionManager.IsTalkMission(currentMisID)
                && currentMisID != MissionManager.m_SpecialMissionID61
                && currentMisID != MissionManager.m_SpecialMissionID62
                && currentMisID != MissionManager.m_SpecialMissionID9
                && PeGameMgr.IsSingle)
            {
                MissionCommonData mcd = MissionManager.GetMissionCommonData(currentMisID);
                if (!((mcd.m_StoryInfo.Count > 0)
                    || (currentType == MissionManager.TakeMissionType.TakeMissionType_Get && (mcd.m_OPID.Count > 0 || mcd.m_EDID.Count > 0))
                    || (currentType == MissionManager.TakeMissionType.TakeMissionType_Complete && mcd.m_EDID.Count > 0)))
                {
                    if (MissionManager.IsTalkMission(currentMisID) && currentMisID != 191)
                    {
                        GameUI.Instance.mNpcWnd.ChangeHeadTex(m_CurTalkNpc);
                        GameUI.Instance.mNpcWnd.SetCurSelNpc(m_CurTalkNpc);
                        GameUI.Instance.mNpcWnd.Show();
                    }
                }
            }

            //if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger))
            //{
            //    if (MissionRepository.HaveTalkOP(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger) && m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger != currentMisID)
            //    {
            //        GameUI.Instance.mNPCTalk.NormalOrSP(0);
            //        if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
            //        {
            //            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger, 1);
            //            GameUI.Instance.mNPCTalk.Show();
            //        }
            //        else
            //            GameUI.Instance.mNPCTalk.AddNpcTalkInfo(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger, 1);
            //        //triggerMutex = false;
            //    }
            //    else
            //    {
            //        MissionManager.Instance.SetGetTakeMission(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger, m_CurTalkNpc, (MissionManager.TakeMissionType)m_TalkType);
            //    }
            //}

            m_CurTalkIdx = 0;
        }
        else
        {
            while (m_NpcTalkList[m_CurTalkIdx].npcid == 9999 || m_NpcTalkList[m_CurTalkIdx].npcid == 9998)
            {
                canClose = false;
                if (m_NpcTalkList[m_CurTalkIdx].npcid == 9999)
                    type = NormalOrSp.SP;
                else
                    type = NormalOrSp.halfSP;
                m_CurTalkIdx++;
            }
            if (m_CurTalkIdx >= m_NpcTalkList.Count)
                return;
            npcid = m_NpcTalkList[m_CurTalkIdx].npcid;
            mContent.text = m_NpcTalkList[m_CurTalkIdx].desc;
            //lz-2016.08.31 检测NPC对话触发引导
            InGameAidData.CheckNpcTalk(m_NpcTalkList[m_CurTalkIdx].talkid);
            if (null != GameUI.Instance)
            {
                //lz-2016.11.07 UI上的Tutorial提示统一管理检测
                GameUI.Instance.CheckTalkIDShowTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
            }
            m_CurTalkNpc = EntityMgr.Instance.Get(npcid);
            if (m_CurTalkNpc == null)
            {
                SetNpcBigHeadSp("npc_big_Unknown");
                switch (npcid)
                {
                    case -9998:
                        mName.text = "Puja Commander";
                        break;
                    case -9997:
                        mName.text = "Puja Soldier";
                        break;
                    case -9996:
                        mName.text = "Tony";
                        break;
                    case -9995:
                        mName.text = "Tips";
                        break;
                    case -9994:
                        mName.text = "Paja";
                        break;
                    case -9993:
                        mName.text = "Puja";
                        break;
                    case -9992:
                        if (CSMain.HasCSAssembly())
                        {
                            List<PeEntity> tmp = CSMain.GetCSNpcs();
                            if (tmp.Count > 0)
                                mName.text = tmp[0].ExtGetName();
                            else
                            {
                                npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[1]);
                                if (npc != null)
                                    mName.text = npc.ExtGetName();
                            }
                        }
                        break;
                    case -9991:
                        mName.text = "???";
                        break;
                    default:
                        if (npcid > -9800 || npcid < -9900)
                            break;
                        if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + npcid))
                        {
                            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + npcid]);
                            if (npc != null)
                                mName.text = npc.ExtGetName();
                        }
                        break;
                }
            }
            else
            {
                mName.text = m_CurTalkNpc.ExtGetName();
                SetNpcBigHeadSp(m_CurTalkNpc.ExtGetFaceIconBig());
            }
			NPCTalkHistroy.Instance.AddHistroy(mName.text, mContent.text);
            GameUI.Instance.mNPCTalk.PreShow();
        }
    }


    void ResetMissionItem()
    {
        ClearMissionItem();
        for (int i = 0; i < m_SelectMissionList.Count; i++)
        {
            MissionSelItem_N AddItem = Instantiate(mPrefab) as MissionSelItem_N;
            AddItem.gameObject.name = "MissionItem" + i;
            AddItem.transform.parent = mUITable.transform;
            AddItem.transform.localPosition = Vector3.zero;
            AddItem.transform.localRotation = Quaternion.identity;
            AddItem.transform.localScale = Vector3.one;
            AddItem.SetMission(m_SelectMissionList[i], this);
            AddItem.ActiveMask();
            mSelList.Add(AddItem);
        }

		if(1 == m_SelectMissionList.Count)
		{
			string content = MissionRepository.GetMissionNpcListName (m_SelectMissionList[0], false);
			if(null != m_CurTalkNpc && string.IsNullOrEmpty(content))
				NPCTalkHistroy.Instance.AddHistroy(mName.text, content);
		}
        //		if(!mUpdateList)
        //		{
        //			mUpdateList = true;
        //			Invoke("UpdateList", 0.2f);
        //		}
        //		
        //		mUITable.Reposition();
        //		mBg.transform.localScale = new Vector3(512,mUITable.mVariableHeight + 14,1);
        //		mUITable.transform.localPosition = new Vector3(-250,mUITable.mVariableHeight + 10,-5000);
        mUITable.repositionNow = true;
    }

    void ClearMissionItem()
    {
        for (int i = mSelList.Count - 1; i >= 0; i--)
        {
            mSelList[i].transform.parent = null;
            Destroy(mSelList[i].gameObject);
        }
        mSelList.Clear();
        mUITable.Reposition();
    }

    public void NormalOrSP(int type)
    {
        this.type = (NormalOrSp)type;
    }

    void OnBgClick()
    {
        if (!m_bMutex)
            OnClose();
    }

    void Update()
    {
        UpdateMoveNpc();
        if (!m_bMutex && PeInput.Get(PeInput.LogicFunction.UI_SkipDialog1) && type == NormalOrSp.Normal)
            OnClose();

        if (currentAudio != null)
        {
            if (type == NormalOrSp.SP || type == NormalOrSp.halfSP)
            {
                if (!spTalkEndByAudioTime && currentAudio.length != 0)
                {
                    Invoke("SPTalkClose", currentAudio.length);
                    spTalkEndByAudioTime = true;
                }
            }
        }

        if (mNpcBigHeadSp.spriteName == "A" || mNpcBigHeadSp.spriteName.Length <= 1)
        {
            SetNpcBigHeadSp("npc_big_Unknown");
        }


        if (mNpcBigHeadSp.spriteName == "npc_big_Unknown")
        {

            if (m_CurTalkIdx > -1 && m_CurTalkIdx < m_NpcTalkList.Count)
            {
                if (m_NpcTalkList[m_CurTalkIdx].npcicon == null)
                {
                    PeEntity npc = EntityMgr.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
                    if (PeGameMgr.IsSingle)
                    {
                        if (npc == null && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + m_NpcTalkList[m_CurTalkIdx].npcid))
                            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + m_NpcTalkList[m_CurTalkIdx].npcid]);
                    }
                    else
                    {
                        if (npc == null && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + m_NpcTalkList[m_CurTalkIdx].npcid))
                            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + m_NpcTalkList[m_CurTalkIdx].npcid]);
                    }
                    if (npc == null && m_NpcTalkList[m_CurTalkIdx].npcid == -9992 && CSMain.HasCSAssembly())
                    {
                        List<PeEntity> tmp = CSMain.GetCSNpcs();
                        if (tmp.Count > 0)
                            npc = tmp[0];
                        else
                            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[1]);
                    }
                    if (npc != null && npc.proto != EEntityProto.Npc)
                    {
                        BiologyViewCmpt viewCmpt = npc.biologyViewCmpt;
                        NpcTalkInfo npcTalkInfo = m_NpcTalkList[m_CurTalkIdx];

                        npcTalkInfo.npcicon = PeViewStudio.TakePhoto(viewCmpt, 150, 150, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
                        mName.text = npc.ExtGetName();
                        m_NpcTalkList[m_CurTalkIdx] = npcTalkInfo;
                        //lz-2016.08.02 拍照失败显示问号头像
                        if (null == m_NpcTalkList[m_CurTalkIdx].npcicon)
                        {
                            mNpcBigHeadSp.gameObject.SetActive(true);
                            mNpcBigHeadTex.gameObject.SetActive(false);
                        }
                        else
                        {
                            mNpcBigHeadSp.gameObject.SetActive(false);
                            mNpcBigHeadTex.gameObject.SetActive(true);
                            mNpcBigHeadTex.mainTexture = m_NpcTalkList[m_CurTalkIdx].npcicon;
                        }


                        mNpcBigHeadTex.gameObject.SetActive(true);
                    }
                    else
                    {
                        mNpcBigHeadSp.gameObject.SetActive(true);
                        mNpcBigHeadTex.gameObject.SetActive(false);
                    }
                }
                else
                {
                    mNpcBigHeadSp.gameObject.SetActive(false);
                    mNpcBigHeadTex.gameObject.SetActive(true);
                }
            }


        }
        else
        {
            mNpcBigHeadSp.gameObject.SetActive(true);
            mNpcBigHeadTex.gameObject.SetActive(false);
        }

        mDragCollider.enabled = mScrollBar.foreground.gameObject.activeSelf;
    }

    void UpdateList()
    {
        mUpdateList = false;
        mUITable.Reposition();
        //		mBg.transform.localScale = new Vector3(512,mUITable.mVariableHeight + 14,1);
        //		mUITable.transform.localPosition = new Vector3(-250,mUITable.mVariableHeight + 10f,0);
    }

    bool CheckRandomNpc()
    {
        //if (NpcManager.Instance.GetHero(1) != null)
        //if (NpcManager.Instance.IsServantFull())
        //{
        //    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID46, 1);
        //    return false;
        //}

        bool bPass = true;

        NpcMissionData missionData = GameUI.Instance.mNpcWnd.m_CurSelNpc.GetUserData() as NpcMissionData;

        if (null != missionData)
        {
            //如果接了这个NPC的任务，不能转换成Hero
            if (MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID13))
            {
                if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID13))
                    bPass = false;
            }

            if (MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID14))
            {
                if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID14))
                    bPass = false;
            }

            if (MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID15))
            {
                if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID15))
                    bPass = false;
            }
        }

        if (!bPass)
        {
            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID47, 1);
            //HeroMissionID = 0;

            return false;
        }

        if (null != missionData)
        {
            if (missionData.mCurComMisNum < missionData.mCompletedMissionCount && !m_QuickZM)
            {
                UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID47, 1);
                return false;
            }
        }
        return true;
    }

    void ChangeNpcState(bool bStart)
    {
        //        PeEntity npc;
        int npcid;
        for (int i = 0; i < m_NpcTalkList.Count; i++)
        {
            npcid = m_NpcTalkList[i].npcid;
            m_CurTalkNpc = EntityMgr.Instance.Get(npcid);

            if (m_CurTalkNpc == null)
                continue;

            if (bStart)
                m_CurTalkNpc.CmdStartIdle();
            else
                m_CurTalkNpc.CmdStopIdle();
        }
    }

    bool CheckTutorial(int id)
    {
        if (id == 1261)
        {
            TutorialData.AddActiveTutorialID(3);
            TutorialData.AddActiveTutorialID(2);
            TutorialData.AddActiveTutorialID(TutorialData.BuildingId);
            TutorialData.AddActiveTutorialID(TutorialData.Building_1Id); //lz-2016.10.14 Building分成两张图了
            TutorialData.AddActiveTutorialID(12);
            TutorialData.AddActiveTutorialID(11);
            GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(3);
            GameUI.Instance.mPhoneWnd.Show();
            return true;
        }
        else if (id == 19)
        {
            TutorialData.AddActiveTutorialID(3);
            TutorialData.AddActiveTutorialID(2);
            TutorialData.AddActiveTutorialID(TutorialData.BuildingId);
            TutorialData.AddActiveTutorialID(TutorialData.Building_1Id); //lz-2016.10.14 Building分成两张图了
            TutorialData.AddActiveTutorialID(12);
            TutorialData.AddActiveTutorialID(11);
            GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(3);
            GameUI.Instance.mPhoneWnd.Hide();
            return true;
        }
        else if (id == 1490)
        {
            TutorialData.AddActiveTutorialID(4, false);
            TutorialData.AddActiveTutorialID(TutorialData.ColonyID0);
        }
        else if (id == 1491)
        {
            TutorialData.AddActiveTutorialID(5);
        }
        else if (id == 1713)
        {
            TutorialData.AddActiveTutorialID(6);
        }
        else if (id == 1492)
        {
            TutorialData.AddActiveTutorialID(7);
        }
        else if (id == 1997)
        {
            //lz-2016.10.18 后面又新加的一部分基地的帮助图片
            for (int i = 17; i < 20; i++)
            {
                TutorialData.AddActiveTutorialID(i, false);
            }
            TutorialData.AddActiveTutorialID(10);
        }
        else if (id == 2273)
        {
            TutorialData.AddActiveTutorialID(13);
            TutorialData.AddActiveTutorialID(14);
            GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(13);
            return true;
        }

        return false;
    }

    //public void NetWorkOnClose(int misID)
    //{
    //    if (!bupdate || npc_id == 0)
    //        return;
    //    bupdate = false;

    //    if (MissionRepository.HasTargetType(m_NpcTalkList[m_CurTalkIdx].missionTrigger, TargetType.TargetType_Follow)
    //        && m_TalkType != 3)
    //        return;

    //    GameUI.Instance.mNpcWnd.GetMutexID(m_NpcTalkList[m_CurTalkIdx].missionTrigger, ref m_SelectMissionList);

    //    if (m_SelectMissionList.Count > 0)
    //    {
    //        m_bMutex = true;
    //        ResetMissionItem();
    //        GameUI.Instance.mNPCTalk.Show();
    //        npc_id = 0;
    //        return;
    //    }

    //    //弹出对话库里面最后一句话的NPC的任务窗口

    //    m_CurTalkNpc = EntityMgr.Instance.Get(npc_id);
    //    npc_id = 0;
    //    if (m_CurTalkNpc == null)
    //        return;

    //    if (m_TalkType != 3 && misID != MissionManager.m_SpecialMissionID9)
    //    {
    //        GameUI.Instance.mNpcWnd.ChangeHeadTex(m_CurTalkNpc);
    //        //GameGui_N.Instance.mNPCTalk.UpdateMission();
    //        GameUI.Instance.mNpcWnd.SetCurSelNpc(m_CurTalkNpc);
    //        GameUI.Instance.mNpcWnd.Show();
    //    }
    //    else
    //    {
    //        m_CurTalkNpc.CmdStopTalk();
    //        StroyManager.Instance.RemoveReq(m_CurTalkNpc, EReqType.Dialogue);
    //    }
    //}
}