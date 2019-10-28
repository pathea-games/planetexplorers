using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mono.Data.SqliteClient;

public struct SenceFace
{
    public string name;
    public float angle;
}

public struct SenceAct
{
    public string name;
    public string act;
}

public struct NpcOpen
{
    public int npcid;
    public bool bopen;
}

public struct PosData
{
    public int type;
    public int npcID;
    public Vector3 pos;
}

public class NpcFace
{
    public int npcid;
    public int angle;
    public string npcother;
    public int otherid;
    public bool bmove;

    public NpcFace()
    {
        npcother = "";
        angle = -1;
        bmove = true;
    }
}

public struct NpcAct
{
    public int npcid;
    public string animation;
    public float bFloat;
    public bool btrue;
}

public struct MonAct 
{
    public List<int> mons;
    public string animation;
    public bool btrue;
    public float time;
}

public struct NpcCamp
{
    public int npcid;
    public int camp;
}

public class NpcRail
{
    public List<int> inpclist = new List<int>();
    public bool bplayer = false;
    public int othernpcid;
    public Vector3 pos;
}

public class NpcStyle
{
    public int bPlayer;
    public int npcid;
    public Vector3 pos;
    public int othernpcid;

    public NpcStyle()
    {
        bPlayer = 0;
        pos = Vector3.zero;
    }
}

public struct RandScenariosData
{
    public int id;
    public bool startOrClose;
    public List<int> scenarioIds;
    public float cd;
}

public class MoveNpcData 
{
    public List<int> npcsId = null;
    public Vector3 pos;
    public int targetNpc;
    public int missionOrPlot_id;
}

public struct MonsterSetInfo
{
    public int id;
    public int value;
}

public struct AbnormalInfo 
{
    public bool setOrRevive;
    public List<int> npcs;
    public int virusNum;
}

public struct ENpcBattleInfo 
{
    public List<int> npcId;
    public int type;
}

public struct ReputationInfo
{
    public int type;
    public int valve;
    public bool isEffect;
}

public struct SetDoodadEffect 
{
    public int id;
    public List<string> names;
    public bool openOrClose;
}

public struct KillMons
{
    public enum Type
	{
        protoTypeId,
        fixedId,
        max
	}
    public int id;
    public Type type;
    public Vector3 center;
    public float radius;
    public int monId;
    public int reviveTime;
}

public struct MoveMons 
{
    public int fixedId;
    public int stepOrRun;
    public Vector3 dist;
    public int missionOrPlot_id;
}

public class CheckMons
{
    public bool existOrNot;
    public int npcid;
    public Vector3 center;
    public int radius;
    public int protoTypeid;
    public bool missionOrPlot;
    public List<int> trigerId = new List<int>();
}

public struct MotionStyle 
{
    public int id;
    public Pathea.ENpcMotionStyle type;
}

public struct ChangePartrolmode 
{
    public List<int> monsId;
    public int type;
    public int radius;
}

public struct CameraInfo
{
    public int cameraId;
    public int talkId;
}

public struct CampState
{
    public int id;
    public bool isActive;
}

public class StoryData
{
    public int m_ID;
    public List<int> m_triggerPlot;
    public List<int> m_TalkList;
    public List<int> m_ServantTalkList;
    public List<MissionIDNum> m_CreateMonster;
    public List<int> m_DeleteMonster;
    public List<NpcStyle> m_TransNpc;   //瞬移
    public List<MoveNpcData> m_MoveNpc;
    public NpcRail m_NpcRail;
    public List<NpcFace> m_NpcFace;
    public List<NpcAct> m_NpcAct;
    public List<MonAct> m_MonAct;
    public List<ReputationInfo> m_NpcReq;
    public List<NpcAct> m_NpcAnimator;
    public List<string> m_PlayerAni;
    public List<MotionStyle> m_MotionStyle;
    public List<SenceFace> m_SenceFace;
    public List<SenceAct> m_SenceAct;
    public List<NpcCamp> m_NpcCamp;
    public string m_Special;
    public List<NpcOpen> m_NpcAI;
    public List<NpcOpen> m_NpcInvincible;
    public List<NpcOpen> m_FollowPlayerList;
    public float m_FollowDist;
    public List<MonsterSetInfo> m_MonsterCampList;
    public List<MonsterSetInfo> m_MonsterHarmList;
    public int m_MoveType;
    public int m_PausePlayer;
    public bool m_PauseNPC;
    public bool m_PauseMons;
    public float m_Delay; //延迟
    public List<Vector3> m_EffectPosList;
    public int m_EffectID;
    public List<PosData> m_SoundPosList;
    public int m_SoundID;
    public List<CameraInfo> m_CameraList;
    public List<int> m_iColonyNoOrderNpcList;
    public List<int> m_iColonyOrderNpcList;
    public List<int> m_killNpcList;          //3个数为一组处理
    public List<int> m_monsterHatredList;    //5个数为一组处理
    public List<int> m_npcHatredList;        //4个数为一组处理
    public List<int> m_harmList;                 //3个数为一组处理
    public List<int> m_doodadHarmList;       //3个数为一组处理
    public List<SetDoodadEffect> m_doodadEffectList;
    public List<ENpcBattleInfo> m_npcsBattle;           //最后一位为npc设置的状态，之前为NPCID号
    public List<string> m_plotMissionTrigger;
    public List<int> m_cantReviveNpc;
    public List<AbnormalInfo> m_abnormalInfo;
    public ReputationInfo[] m_reputationChange;
    public ReputationInfo m_nativeAttitude;
    public List<int> oldDoodad;
    public List<int> newDoodad;
    public List<KillMons> m_killMons;
    public List<int> m_stopKillMonsID;
    public List<ChangePartrolmode> m_monPatrolMode;       //最后一个为patrolMode，之前为定点怪ID
    public List<NpcType> m_npcType;
    public List<MoveMons> m_moveMons;
    public List<CheckMons> m_checkMons;
    public int m_increaseLangSkill;
    public int m_moveNpc_missionOrPlot_id;
    public int m_moveMons_missionOrPlot_id;
    public List<int> m_attractMons;
    public RandScenariosData m_randScenarios;
    public List<CampState> m_campAlert;
    public List<CampState> m_campActive;
    public List<int> m_comMission;
    public List<ENpcBattleInfo> m_whackedList;
    public List<int> m_getMission;
    public int m_pauseSiege;
    public int m_showTip;

    public StoryData()
    {
        m_Special = "";
        m_triggerPlot = new List<int>();
        m_TalkList = new List<int>();
        m_ServantTalkList = new List<int>();
        m_CreateMonster = new List<MissionIDNum>();
        m_DeleteMonster = new List<int>();
        m_TransNpc = new List<NpcStyle>();
        m_MoveNpc = new List<MoveNpcData>();
        m_NpcFace = new List<NpcFace>();
        m_NpcAct = new List<NpcAct>();
        m_MonAct = new List<MonAct>();
        m_NpcReq = new List<ReputationInfo>();
        m_SenceFace = new List<SenceFace>();
        m_SenceAct = new List<SenceAct>();
        m_NpcCamp = new List<NpcCamp>();
        m_NpcAI = new List<NpcOpen>();
        m_NpcInvincible = new List<NpcOpen>();
        m_FollowPlayerList = new List<NpcOpen>();
        m_MonsterCampList = new List<MonsterSetInfo>();
        m_MonsterHarmList = new List<MonsterSetInfo>();
        m_EffectPosList = new List<Vector3>();
        m_SoundPosList = new List<PosData>();
        m_CameraList = new List<CameraInfo>();
        m_NpcRail = new NpcRail();
        m_iColonyNoOrderNpcList = new List<int>();
        m_iColonyOrderNpcList = new List<int>();
        m_killNpcList = new List<int>();
        m_monsterHatredList = new List<int>();
        m_npcHatredList = new List<int>();
        m_harmList = new List<int>();
        m_doodadHarmList = new List<int>();
        m_plotMissionTrigger = new List<string>();
        m_cantReviveNpc = new List<int>();
        m_npcsBattle = new List<ENpcBattleInfo>();
        m_abnormalInfo = new List<AbnormalInfo>();
        m_reputationChange = new ReputationInfo[2];
        oldDoodad = new List<int>();
        newDoodad = new List<int>();
        m_doodadEffectList = new List<SetDoodadEffect>();
        m_killMons = new List<KillMons>();
        m_stopKillMonsID = new List<int>();
        m_monPatrolMode = new List<ChangePartrolmode>();
        m_npcType = new List<NpcType>();
        m_checkMons = new List<CheckMons>();
        m_moveMons = new List<MoveMons>();
        m_NpcAnimator = new List<NpcAct>();
        m_PlayerAni = new List<string>();
        m_MotionStyle = new List<MotionStyle>();
        m_attractMons = new List<int>();
        m_campAlert = new List<CampState>();
        m_campActive = new List<CampState>();
        m_comMission = new List<int>();
        m_whackedList = new List<ENpcBattleInfo>();
        m_getMission = new List<int>();
    }
}

public enum ReferToType
{
    None = 0,
    Player,
    Town,
    Npc,
    Transcript
}

public class AdCreNPC
{
    public ReferToType referToType;
    public int m_referToID;
    public float m_radius;
    public int m_NPCID;
}

public class AdEnterArea
{
    public int referToType;
    public int m_referToID;
    public float m_radius;
    public List<int> m_plotID;
    public AdEnterArea()
    {
        m_plotID = new List<int>();
    }
}

public class AdStoryData
{
    public int m_ID;
    public List<AdCreNPC> m_creNPC;
    public List<AdEnterArea> m_enterArea;
    public List<AdNPCMove> m_npcMove;
    public List<int> m_getMissionID;
    public List<int> m_comMissionID;
    public int m_showTip;
    public AdStoryData()
    {
        m_creNPC = new List<AdCreNPC>();
        m_enterArea = new List<AdEnterArea>();
        m_npcMove = new List<AdNPCMove>();
        m_getMissionID = new List<int>();
        m_comMissionID = new List<int>();
    }
}

public class AdNPCMove
{
    public int npcID;
    public ReferToType referToType;
    public int m_referToID;
    public float m_radius;
}

public class StoryRepository
{
    public static Dictionary<int, StoryData> m_StoryRespository = new Dictionary<int, StoryData>();
    public static StoryData GetStroyData(int id)
    {
        if (!m_StoryRespository.ContainsKey(id))
            return null;

        return m_StoryRespository[id];
    }
    public static void LoadData_story()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Plot");
        reader.Read();
        while (reader.Read())
        {
            StoryData data = new StoryData();
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            string tmp;
            string[] tmplist;
            string[] tmplist1;
            string[] tmplist2;
            float x, y, z;

            tmp = reader.GetString(reader.GetOrdinal("plot"));
            if (!tmp.Equals("0"))
            {
                tmplist = tmp.Split(',');
                foreach (var item in tmplist)
                {
                    data.m_triggerPlot.Add(Convert.ToInt32(item));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("ScenarioID"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                int id = Convert.ToInt32(tmplist[i]);
                if (id == 0)
                    continue;

                data.m_TalkList.Add(id);
            }

            tmp = reader.GetString(reader.GetOrdinal("ScenarioSP"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                int id = Convert.ToInt32(tmplist[i]);
                if (id == 0)
                    continue;

                data.m_ServantTalkList.Add(id);
            }

            tmp = reader.GetString(reader.GetOrdinal("RandScenario"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 4)
            {
                RandScenariosData rsd;
                rsd.scenarioIds = new List<int>();
                rsd.id = Convert.ToInt32(tmplist[0]);
                rsd.startOrClose = tmplist[1].Equals("1") ? true : false;
                rsd.cd = Convert.ToSingle(tmplist[3]);

                tmplist1 = tmplist[2].Split(',');
                foreach (var item in tmplist1)
                    rsd.scenarioIds.Add(Convert.ToInt32(item));

                data.m_randScenarios = rsd;
            }

            tmp = reader.GetString(reader.GetOrdinal("CreateMons"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                MissionIDNum mon;
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length > 1)
                    mon.num = Convert.ToInt32(tmplist1[1]);
                else
                    mon.num = 1;

                mon.id = Convert.ToInt32(tmplist1[0]);
                if (mon.id == 0)
                    continue;

                data.m_CreateMonster.Add(mon);
            }

            tmp = reader.GetString(reader.GetOrdinal("DelMons"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                int id = Convert.ToInt32(tmplist[i]);
                if (id == 0)
                    continue;

                data.m_DeleteMonster.Add(id);
            }

            tmp = reader.GetString(reader.GetOrdinal("TransNPC"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcStyle ns = new NpcStyle();
                ns.npcid = Convert.ToInt32(tmplist1[0]);
                tmplist2 = tmplist1[1].Split(',');
                if (tmplist2.Length != 3)
                    continue;

                x = Convert.ToSingle(tmplist2[0]);
                y = Convert.ToSingle(tmplist2[1]);
                z = Convert.ToSingle(tmplist2[2]);
                ns.pos = new Vector3(x, y, z);

                data.m_TransNpc.Add(ns);
            }

            tmp = reader.GetString(reader.GetOrdinal("MoveNPC"));
            tmplist = tmp.Split(':')[0].Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2 && tmplist1.Length != 4)
                    continue;

                MoveNpcData mnd = new MoveNpcData();
                tmplist2 = tmplist1[0].Split(',');
                mnd.npcsId = new List<int>(Array.ConvertAll<string, int>(tmplist2, s => int.Parse(s)));

                tmplist2 = tmplist1[1].Split(',');
                if (tmplist2.Length == 3)
                {
                    x = Convert.ToSingle(tmplist2[0]);
                    y = Convert.ToSingle(tmplist2[1]);
                    z = Convert.ToSingle(tmplist2[2]);
                    mnd.pos = new Vector3(x, y, z);
                }
                else if (PETools.PEMath.IsNumeral(tmplist2[0]))
                    mnd.targetNpc = int.Parse(tmplist2[0]);
                else
                {
                    if (tmplist2[0] == "Colony")
                        mnd.targetNpc = -99;
                    else if (tmplist2[0] == "NColony")
                        mnd.targetNpc = -98;
                }

                if (tmplist1.Length == 4) 
                    mnd.missionOrPlot_id = Convert.ToInt32(tmplist1[2]) * 10000 + Convert.ToInt32(tmplist1[3]);

                data.m_MoveNpc.Add(mnd);
            }

            tmplist = tmp.Split(':');
            if (tmplist.Length == 2) 
            {
                tmplist1 = tmplist[1].Split('_');
                if (tmplist1.Length == 2)
                    data.m_moveNpc_missionOrPlot_id = Convert.ToInt32(tmplist1[0]) * 10000 + Convert.ToInt32(tmplist1[1]);
            }

            //railnpc,railnpc_pos(npcpos)(player)
            tmp = reader.GetString(reader.GetOrdinal("RailNPC"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 2)
            {
                tmplist1 = tmplist[0].Split(',');
                for (int i = 0; i < tmplist1.Length; i++)
                    data.m_NpcRail.inpclist.Add(Convert.ToInt32(tmplist1[i]));

                if (tmplist[1] == "20000")
                    data.m_NpcRail.bplayer = true;
                else
                {
                    tmplist2 = tmplist[1].Split(',');
                    if (tmplist2.Length == 3)
                    {
                        x = Convert.ToSingle(tmplist2[0]);
                        y = Convert.ToSingle(tmplist2[1]);
                        z = Convert.ToSingle(tmplist2[2]);
                        data.m_NpcRail.pos = new Vector3(x, y, z);
                    }
                    else
                        data.m_NpcRail.othernpcid = Convert.ToInt32(tmplist[1]);
                }
            }
            

            //npcname_npcother(angle)_bmove
            tmp = reader.GetString(reader.GetOrdinal("NPCface"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length < 2)
                    continue;

                NpcFace nf = new NpcFace();
                nf.npcid = Convert.ToInt32(tmplist1[0]);
                int outinfo;
                if (int.TryParse(tmplist1[1], out outinfo))
                    nf.angle = Convert.ToInt32(tmplist1[1]);
                else
                    nf.npcother = tmplist1[1];

                if (tmplist1.Length == 3)
                    nf.bmove = (Convert.ToInt32(tmplist1[2]) == 1) ? true : false;

                data.m_NpcFace.Add(nf);
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCact"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 4)
                    continue;

                NpcAct nf = new NpcAct();
                nf.npcid = Convert.ToInt32(tmplist1[0]);
                nf.animation = tmplist1[1];
                nf.btrue = (Convert.ToInt32(tmplist1[2]) == 1) ? true : false;
                nf.bFloat = Convert.ToSingle(tmplist1[3]);

                data.m_NpcAct.Add(nf);
            }

            tmp = reader.GetString(reader.GetOrdinal("MonsAni"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 4)
                    continue;

                MonAct ma = new MonAct();
                ma.mons = new List<int>();
                tmplist2 = tmplist1[0].Split(',');
                for (int j = 0; j < tmplist2.Length; j++)
                    ma.mons.Add(Convert.ToInt32(tmplist2[j]));
                ma.animation = tmplist1[1];
                ma.btrue = Convert.ToInt32(tmplist1[2]) == 1 ? true : false;
                ma.time = Convert.ToSingle(tmplist1[3]);

                data.m_MonAct.Add(ma);
            }

            tmp = reader.GetString(reader.GetOrdinal("SetActive"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 3)
                    continue;

                ReputationInfo nq = new ReputationInfo();
                nq.isEffect = Convert.ToInt32(tmplist1[0]) == 1 ? true : false;
                nq.type = Convert.ToInt32(tmplist1[1]);
                nq.valve = Convert.ToInt32(tmplist1[2]);

                data.m_NpcReq.Add(nq);
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCAni"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 3)
                    continue;
                tmplist2 = tmplist1[0].Split(',');
                for (int i = 0; i < tmplist2.Length; i++)
                {
                    NpcAct na = new NpcAct();
                    na.npcid = Convert.ToInt32(tmplist2[i]);
                    na.animation = tmplist1[1];
                    na.btrue = tmplist1[2].Equals("1") ? true : false;
                    data.m_NpcAnimator.Add(na);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("Playeract"));
            if (!tmp.Equals("0"))
            {
                tmplist = tmp.Split('_');
                if (tmplist.Length == 2)
                {
                    data.m_PlayerAni.Add(tmplist[0]);
                    data.m_PlayerAni.Add(tmplist[1]);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCStatus"));
            if (!tmp.Equals("0"))
            {
                tmplist = tmp.Split(';');
                foreach (var item in tmplist)
                {
                    tmplist1 = item.Split('_');
                    if (tmplist1.Length != 2)
                        continue;
                    MotionStyle ms;
                    ms.id = Convert.ToInt32(tmplist1[0]);
                    ms.type = (Pathea.ENpcMotionStyle)(Convert.ToInt32(tmplist1[1]) + 1);
                    data.m_MotionStyle.Add(ms);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("Sceneface"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('-');
                if (tmplist1.Length != 2)
                    continue;

                SenceFace sf;
                sf.name = tmplist1[0];
                sf.angle = Convert.ToSingle(tmplist1[1]);

                data.m_SenceFace.Add(sf);
            }


            tmp = reader.GetString(reader.GetOrdinal("Sceneact"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('-');
                if (tmplist1.Length != 2)
                    continue;

                SenceAct sa;
                sa.name = tmplist1[0];
                sa.act = tmplist1[1];

                data.m_SenceAct.Add(sa);
            }



            tmp = reader.GetString(reader.GetOrdinal("NPCcamp"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcCamp nc = new NpcCamp();
                nc.npcid = Convert.ToInt32(tmplist1[0]);
                nc.camp = Convert.ToInt32(tmplist1[1]);

                data.m_NpcCamp.Add(nc);
            }

            data.m_Special = reader.GetString(reader.GetOrdinal("Special"));

            tmp = reader.GetString(reader.GetOrdinal("Ai"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcOpen no = new NpcOpen();
                no.npcid = Convert.ToInt32(tmplist1[0]);
                no.bopen = (Convert.ToInt32(tmplist1[1]) == 1) ? true : false;

                data.m_NpcAI.Add(no);
            }

            tmp = reader.GetString(reader.GetOrdinal("Invincible"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcOpen no = new NpcOpen();
                no.npcid = Convert.ToInt32(tmplist1[0]);
                no.bopen = (Convert.ToInt32(tmplist1[1]) == 1) ? true : false;

                data.m_NpcInvincible.Add(no);
            }

            tmp = reader.GetString(reader.GetOrdinal("FolPlayer"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcOpen no = new NpcOpen();
                no.npcid = Convert.ToInt32(tmplist1[0]);
                no.bopen = (Convert.ToInt32(tmplist1[1]) == 1) ? true : false;

                data.m_FollowPlayerList.Add(no);
            }

            data.m_FollowDist = Convert.ToSingle(reader.GetString(reader.GetOrdinal("FollowDist")));

            tmp = reader.GetString(reader.GetOrdinal("Monstercamp"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                MonsterSetInfo mon;
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                mon.id = Convert.ToInt32(tmplist1[0]);
                mon.value = Convert.ToInt32(tmplist1[1]);
                if (mon.id == 0)
                    continue;

                data.m_MonsterCampList.Add(mon);
            }

            tmp = reader.GetString(reader.GetOrdinal("Monsterharm"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                MonsterSetInfo mon;
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                mon.id = Convert.ToInt32(tmplist1[0]);
                mon.value = Convert.ToInt32(tmplist1[1]);
                if (mon.id == 0)
                    continue;

                data.m_MonsterHarmList.Add(mon);
            }

            tmp = reader.GetString(reader.GetOrdinal("MoveMons"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 3 && tmplist1.Length != 5)
                    continue;
                MoveMons mm;
                mm.fixedId = Convert.ToInt32(tmplist1[0]);
                mm.stepOrRun = Convert.ToInt32(tmplist1[1]);
                tmplist2 = tmplist1[2].Split(',');
                x = Convert.ToSingle(tmplist2[0]);
                y = Convert.ToSingle(tmplist2[1]);
                z = Convert.ToSingle(tmplist2[2]);
                mm.dist = new Vector3(x, y, z);

                if (tmplist1.Length == 5)
                    mm.missionOrPlot_id = Convert.ToInt32(tmplist1[3]) * 10000 + Convert.ToInt32(tmplist1[4]);
                else
                    mm.missionOrPlot_id = 0;

                data.m_moveMons.Add(mm);
            }

            tmplist = tmp.Split(':');
            if (tmplist.Length == 2)
            {
                tmplist1 = tmplist[1].Split('_');
                if (tmplist1.Length == 2)
                    data.m_moveMons_missionOrPlot_id = Convert.ToInt32(tmplist1[0]) * 10000 + Convert.ToInt32(tmplist1[1]);
            }



            data.m_MoveType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Movemethod")));

            data.m_PausePlayer = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PausePlayer")));

            data.m_PauseNPC = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PauseNPC"))) > 0 ? false : true;

            data.m_PauseMons = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PauseMons"))) > 0 ? true : false;

            data.m_Delay = Convert.ToSingle(reader.GetString(reader.GetOrdinal("Delay")));

            tmp = reader.GetString(reader.GetOrdinal("EffectsPos"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split(',');
                if (tmplist1.Length != 3)
                    continue;

                x = Convert.ToSingle(tmplist1[0]);
                y = Convert.ToSingle(tmplist1[1]);
                z = Convert.ToSingle(tmplist1[2]);

                data.m_EffectPosList.Add(new Vector3(x, y, z));
            }

            data.m_EffectID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("EffectsID")));

            tmp = reader.GetString(reader.GetOrdinal("SoundsPos"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split(',');
                if (tmplist1.Length != 3 && tmplist1.Length != 1)
                    continue;
                PosData ptmp = new PosData();
                if (tmplist1.Length == 3)
                {
                    ptmp.type = 3;
                    x = Convert.ToSingle(tmplist1[0]);
                    y = Convert.ToSingle(tmplist1[1]);
                    z = Convert.ToSingle(tmplist1[2]);
                    ptmp.pos = new Vector3(x, y, z);
                }
                else if(tmplist1.Length == 1)
                {
                    ptmp.type = 1;
                    ptmp.npcID = Convert.ToInt32(tmplist1[0]);
                }

                data.m_SoundPosList.Add(ptmp);
            }

            data.m_SoundID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("SoundsID")));

            tmp = reader.GetString(reader.GetOrdinal("CameraID"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;
                tmplist1 = tmplist[i].Split('_');
                CameraInfo ci;
                ci.cameraId = Convert.ToInt32(tmplist1[0]);
                if (tmplist1.Length == 2)
                    ci.talkId = Convert.ToInt32(tmplist1[1]);
                else
                    ci.talkId = 0;

                data.m_CameraList.Add(ci);
            }

            tmp = reader.GetString(reader.GetOrdinal("ColonyNoOrder"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                data.m_iColonyNoOrderNpcList.Add(Convert.ToInt32(tmplist[i]));
            }

            tmp = reader.GetString(reader.GetOrdinal("ColonyOrder"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                data.m_iColonyOrderNpcList.Add(Convert.ToInt32(tmplist[i]));
            }

            tmp = reader.GetString(reader.GetOrdinal("Siege"));
            data.m_pauseSiege = Convert.ToInt32(tmp);

            //之后添加的剧情处理
            tmp = reader.GetString(reader.GetOrdinal("AttractMons"));
            if (!tmp.Equals("0"))
            {
                tmplist = tmp.Split(':');
                tmplist1 = tmplist[0].Split(',');
                foreach (var item in tmplist1)
                    data.m_attractMons.Add(Convert.ToInt32(item));
                data.m_attractMons.Add(-9999);
                tmplist2 = tmplist[1].Split('_',',');
                foreach (var item in tmplist2)
                    data.m_attractMons.Add(Convert.ToInt32(item));
            }

            tmp = reader.GetString(reader.GetOrdinal("KillNPC"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_killNpcList.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("MonsterHatred"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_monsterHatredList.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCHatred"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_npcHatredList.Add(Convert.ToInt32(tmplist[i]));
                }
            }


            tmp = reader.GetString(reader.GetOrdinal("Harm"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_harmList.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("DoodadHarm"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_',';');
                if (tmplist.Length == 3) 
                {
                    foreach (var item in tmplist)
                    {
                        data.m_doodadHarmList.Add(Convert.ToInt32(item));
                    }
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("DoodadEffect"));
            if (tmp != "0")
            {
                tmplist = tmp.Split(';');
                SetDoodadEffect deInfo;
                foreach (var item in tmplist)
                {
                    tmplist1 = item.Split(':');
                    if (tmplist1.Length != 3)
                        continue;
                    deInfo.id = Convert.ToInt32(tmplist1[0]);

                    deInfo.names = new List<string>();
                    tmplist2 = tmplist1[1].Split(',');
                    foreach (var name in tmplist2)
                        deInfo.names.Add(name);

                    deInfo.openOrClose = Convert.ToInt32(tmplist1[2]) == 1 ? true : false;
                    data.m_doodadEffectList.Add(deInfo);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("ENpcBattle"));
            if (tmp != "0")
            {
                tmplist = tmp.Split(';');
                ENpcBattleInfo nbInfo;
                foreach (var item in tmplist)
                {
                    tmplist1 = item.Split(',', '_');
                    if (tmplist1.Length < 2)
                        continue;
                    nbInfo.npcId = new List<int>();
                    for (int i = 0; i < tmplist1.Length - 1; i++)
                    {
                        nbInfo.npcId.Add(Convert.ToInt32(tmplist1[i]));
                    }
                    nbInfo.type = Convert.ToInt32(tmplist1[tmplist1.Length - 1]);
                    data.m_npcsBattle.Add(nbInfo);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("Area"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_');
                if (tmplist.Length == 6)
                {
                    for (int i = 0; i < tmplist.Length; i++)
                    {
                        data.m_plotMissionTrigger.Add(tmplist[i]);
                    }
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("CantRevive"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
			{
                if (Convert.ToInt32(tmplist[i]) == 0)
                    continue;
                data.m_cantReviveNpc.Add(Convert.ToInt32(tmplist[i]));
			}

            tmp = reader.GetString(reader.GetOrdinal("Abnormal"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 3)
                    continue;
                AbnormalInfo abInfo;
                abInfo.npcs = new List<int>();
                if (Convert.ToInt32(tmplist1[0]) == 1)
                    abInfo.setOrRevive = true;
                else
                    abInfo.setOrRevive = false;
                tmplist2 = tmplist1[1].Split(',');
                for (int j = 0; j < tmplist2.Length; j++)
                {
                    abInfo.npcs.Add(Convert.ToInt32(tmplist2[j]));
                }
                abInfo.virusNum = Convert.ToInt32(tmplist1[2]);
                data.m_abnormalInfo.Add(abInfo);
            }

            tmp = reader.GetString(reader.GetOrdinal("ReputationPuja"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 2) 
            {
                ReputationInfo rInfo;
                rInfo.type = Convert.ToInt32(tmplist[0]);
                rInfo.valve = Convert.ToInt32(tmplist[1]);
                rInfo.isEffect = true;
                data.m_reputationChange[0] = rInfo;
            }

            tmp = reader.GetString(reader.GetOrdinal("ReputationPaja"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 2)
            {
                ReputationInfo rInfo;
                rInfo.type = Convert.ToInt32(tmplist[0]);
                rInfo.valve = Convert.ToInt32(tmplist[1]);
                rInfo.isEffect = true;
                data.m_reputationChange[1] = rInfo;
            }

            tmp = reader.GetString(reader.GetOrdinal("NativeAttitude"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 2)
            {
                ReputationInfo rInfo;
                rInfo.type = Convert.ToInt32(tmplist[0]);
                rInfo.valve = Convert.ToInt32(tmplist[1]);
                rInfo.isEffect = true;
                data.m_nativeAttitude = rInfo;
            }

            tmp = reader.GetString(reader.GetOrdinal("ChangeDoodad"));
            tmplist = tmp.Split(';');
            if (tmplist.Length == 2) 
            {
                tmplist1 = tmplist[0].Split(',');
                tmplist2 = tmplist[1].Split(',');
				int n;
                foreach (var item in tmplist1)
				{
					n = Convert.ToInt32(item);
					if(n == 0)
						continue;
					data.oldDoodad.Add(n);
				}
                foreach (var item in tmplist2)
				{
					n = Convert.ToInt32(item);
					if(n == 0)
						continue;
					data.newDoodad.Add(n);
				}
            }

            tmp = reader.GetString(reader.GetOrdinal("KillMons"));
            tmplist = tmp.Split(';');
            KillMons kmInfo;
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 5)
                    continue;
                kmInfo.id = Convert.ToInt32(tmplist1[0]);
                tmplist2 = tmplist1[1].Split(',');
                if (tmplist2.Length == 3)
                {
                    x = Convert.ToSingle(tmplist2[0]);
                    y = Convert.ToSingle(tmplist2[1]);
                    z = Convert.ToSingle(tmplist2[2]);
                    kmInfo.center = new Vector3(x, y, z);
                    kmInfo.radius = Convert.ToSingle(tmplist1[2]);
                    kmInfo.type = KillMons.Type.protoTypeId;
                }
                else
                {
                    kmInfo.center = Vector3.zero;
                    kmInfo.radius = 0;
                    kmInfo.type = KillMons.Type.fixedId;
                }

                kmInfo.monId = Convert.ToInt32(tmplist1[3]);
                kmInfo.reviveTime = Convert.ToInt32(tmplist1[4]);

                data.m_killMons.Add(kmInfo);
            }

            tmp = reader.GetString(reader.GetOrdinal("StopKillMons"));
            tmplist = tmp.Split(',');
            foreach (var item in tmplist)
            {
                if (item.Equals("0"))
                    continue;
                data.m_stopKillMonsID.Add(Convert.ToInt32(item));
            }

            tmp = reader.GetString(reader.GetOrdinal("MonsPatrolRange"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 3)
                    continue;
                ChangePartrolmode cpm;
                cpm.monsId = new List<int>();
                tmplist2 = tmplist1[0].Split(',');
                foreach (var item1 in tmplist2)
                    cpm.monsId.Add(Convert.ToInt32(item1));

                cpm.type = Convert.ToInt32(tmplist1[1]);
                cpm.radius = Convert.ToInt32(tmplist1[2]);

                data.m_monPatrolMode.Add(cpm);
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCType"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                if (item.Equals("0"))
                    continue;
                NpcType nt;
                nt.npcs = new List<int>();
                nt.type = -1;
                string[] conPre = item.Split('_');
                if (conPre.Length != 2)
                    continue;
                foreach (var item1 in conPre[0].Split(','))
                {
                    nt.npcs.Add(Convert.ToInt32(item1));
                }
                nt.type = Convert.ToInt32(conPre[1]);
                data.m_npcType.Add(nt);
            }

            tmp = reader.GetString(reader.GetOrdinal("MonsCheck"));
            tmplist = tmp.Split(':');
            if (tmplist.Length == 2)
            {
                tmplist1 = tmplist[0].Split(';');
                foreach (var item in tmplist1)
                {
                    tmplist2 = item.Split('_');
                    if (tmplist2.Length != 4)
                        continue;
                    CheckMons cm = new CheckMons();
                    cm.existOrNot = tmplist2[0].Equals("1") ? true : false;
                    if (tmplist2[1].Split(',').Length == 3)
                    {
                        x = Convert.ToSingle(tmplist2[1].Split(',')[0]);
                        y = Convert.ToSingle(tmplist2[1].Split(',')[1]);
                        z = Convert.ToSingle(tmplist2[1].Split(',')[2]);
                        cm.center = new Vector3(x, y, z);
                    }
                    else
                        cm.npcid = Convert.ToInt32(tmplist2[1]);
                    cm.radius = Convert.ToInt32(tmplist2[2]);
                    cm.protoTypeid = Convert.ToInt32(tmplist2[3]);
                    data.m_checkMons.Add(cm);
                }
                if (data.m_checkMons.Count > 0)
                {
                    CheckMons cm = new CheckMons();
                    cm.missionOrPlot = tmplist[1].Split('_')[0].Equals("1") ? true : false;
                    string triggers = tmplist[1].Split('_')[1];
                    foreach (var item in triggers.Split(','))
                        cm.trigerId.Add(Convert.ToInt32(item));
                    data.m_checkMons.Add(cm);
                }
            }

            data.m_increaseLangSkill = Convert.ToInt32(reader.GetString(reader.GetOrdinal("IncreaseLangSkill")));

            tmp = reader.GetString(reader.GetOrdinal("SetAlert"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 2)
                    continue;
                CampState ca;
                ca.id = Convert.ToInt32(tmplist1[0]);
                ca.isActive = Convert.ToInt32(tmplist1[1]) == 1 ? true : false;
                data.m_campAlert.Add(ca);
            }

            tmp = reader.GetString(reader.GetOrdinal("SetActiveCamp"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 2)
                    continue;
                CampState cs;
                cs.id = Convert.ToInt32(tmplist1[0]);
                cs.isActive = Convert.ToInt32(tmplist1[1]) == 1 ? true : false;
                data.m_campActive.Add(cs);
            }

            tmp = reader.GetString(reader.GetOrdinal("ComMission"));
            tmplist = tmp.Split(',');
            foreach (var item in tmplist)
            {
                int n = Convert.ToInt32(item);
                if (n == 0)
                    continue;
                data.m_comMission.Add(n);
            }

            tmp = reader.GetString(reader.GetOrdinal("CantWhacked"));
            if (tmp != "0")
            {
                ENpcBattleInfo whached;
                tmplist = tmp.Split(';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    whached = new ENpcBattleInfo();
                    whached.npcId = new List<int>();
                    tmplist1 = tmplist[i].Split('_');
                    whached.type = Convert.ToInt32(tmplist1[1]);

                    tmplist2 = tmplist1[0].Split(',');
                    for (int j = 0; j < tmplist2.Length; j++)
                        whached.npcId.Add(Convert.ToInt32(tmplist2[j]));

                    data.m_whackedList.Add(whached);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("GetMission"));
            if (tmp != "0")
            {
                tmplist = tmp.Split(',');
                for (int i = 0; i < tmplist.Length; i++)
                    data.m_getMission.Add(Convert.ToInt32(tmplist[i]));
            }

            tmp = reader.GetString(reader.GetOrdinal("ShowTip"));
            data.m_showTip = Convert.ToInt32(tmp);

            m_StoryRespository.Add(data.m_ID, data);
        }
    }

    public static Dictionary<int, AdStoryData> m_AdStoryRespository = new Dictionary<int, AdStoryData>();
    public static AdStoryData GetAdStroyData(int id)
    {
        if (!m_AdStoryRespository.ContainsKey(id))
            return null;

        return m_AdStoryRespository[id];
    }
    public static void LoadData_Adventure()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdvPlot");
        while (reader.Read())
        {
            AdStoryData data = new AdStoryData();
            string tmp;
            string[] tmpList1,tmpList2,tmpList3;
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            tmp = reader.GetString(reader.GetOrdinal("CreNPC"));
            tmpList1 = tmp.Split(';');
            for (int i = 0; i < tmpList1.Length; i++)
            {
                tmpList2 = tmpList1[i].Split('_');
                if (tmpList2.Length != 4)
                    continue;
                AdCreNPC creNPC = new AdCreNPC();
                creNPC.referToType = (ReferToType)Convert.ToInt32(tmpList2[0]);
                creNPC.m_referToID = Convert.ToInt32(tmpList2[1]);
                creNPC.m_radius = Convert.ToSingle(tmpList2[2]);
                creNPC.m_NPCID = Convert.ToInt32(tmpList2[3]);

                data.m_creNPC.Add(creNPC);
            }

            tmp = reader.GetString(reader.GetOrdinal("NearPos"));
            tmpList1 = tmp.Split(';');
            for (int i = 0; i < tmpList1.Length; i++)
            {
                tmpList2 = tmpList1[i].Split('_');
                if (tmpList2.Length != 4)
                    continue;
                AdEnterArea enterArea = new AdEnterArea();
                enterArea.referToType = Convert.ToInt32(tmpList2[0]);
                enterArea.m_referToID = Convert.ToInt32(tmpList2[1]);
                enterArea.m_radius = Convert.ToSingle(tmpList2[2]);
                tmpList3 = tmpList2[3].Split(',');
                for (int j = 0; j < tmpList3.Length; j++)
                    enterArea.m_plotID.Add(Convert.ToInt32(tmpList3[i]));

                data.m_enterArea.Add(enterArea);
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCMove"));
            tmpList1 = tmp.Split(';');
            for (int i = 0; i < tmpList1.Length; i++)
            {
                tmpList2 = tmpList1[i].Split('_');
                if (tmpList2.Length != 4)
                    continue;
                AdNPCMove move = new AdNPCMove();
                move.npcID = Convert.ToInt32(tmpList2[0]);
                move.referToType = (ReferToType)Convert.ToInt32(tmpList2[1]);
                move.m_referToID = Convert.ToInt32(tmpList2[2]);
                move.m_radius = Convert.ToSingle(tmpList2[3]);

                data.m_npcMove.Add(move);
            }

            tmp = reader.GetString(reader.GetOrdinal("GetMission"));
            tmpList1 = tmp.Split(',');
            for (int i = 0; i < tmpList1.Length; i++)
            {
                int n = Convert.ToInt32(tmpList1[i]);
                if (n == 0)
                    continue;
                data.m_getMissionID.Add(n);
            }

            tmp = reader.GetString(reader.GetOrdinal("ComMission"));
            tmpList1 = tmp.Split(',');
            for (int i = 0; i < tmpList1.Length; i++)
            {
                int n = Convert.ToInt32(tmpList1[i]);
                if (n == 0)
                    continue;
                data.m_comMissionID.Add(n);
            }

            tmp = reader.GetString(reader.GetOrdinal("ShowTip"));
            data.m_showTip = Convert.ToInt32(tmp);

            m_AdStoryRespository.Add(data.m_ID, data);
        }
    }

    public static void LoadData()
    {
        LoadData_story();
        LoadData_Adventure();
    }
}

public class MissionInit
{
    public int m_ID;
    public List<int> m_ComMisID;
    public int m_NComMisID;
    public List<NpcAct> m_NpcAct;
    public List<NpcFace> m_NpcFace;
    public List<NpcCamp> m_NpcCamp;
    public List<int> m_DeleteMonster;
    public List<int> m_iDeleteNpc;
    public List<NpcStyle> m_TransNpc;
    public List<NpcOpen> m_FollowPlayerList;
    public List<MonsterSetInfo> m_MonsterCampList;
    public List<MonsterSetInfo> m_MonsterHarmList;
    public string m_Special;
    public List<int> m_iColonyNoOrderNpcList;
    public List<int> m_monsterHatredList;    //5个数为一组处理
    public List<int> m_npcHatredList;        //4个数为一组处理
    public List<int> m_harmList;                 //3个数为一组处理
    public List<int> m_doodadHarmList;
    public List<ENpcBattleInfo> m_npcsBattle;           //最后一位为npc状态设置值，之前位数为npcID
    public List<string> m_plotMissionTrigger;
    public List<int> cantReviveNpc;
    public List<KillMons> m_killMons;
    public List<SetDoodadEffect> m_doodadEffectList;
    public List<NpcType> m_npcType;
    public List<int> m_triggerPlot;
    public List<int> m_killNpcList;

    public MissionInit()
    {
        m_ComMisID = new List<int>();
        m_NpcAct = new List<NpcAct>();
        m_NpcFace = new List<NpcFace>();
        m_NpcCamp = new List<NpcCamp>();
        m_DeleteMonster = new List<int>();
        m_iDeleteNpc = new List<int>();
        m_TransNpc = new List<NpcStyle>();
        m_FollowPlayerList = new List<NpcOpen>();
        m_MonsterCampList = new List<MonsterSetInfo>();
        m_MonsterHarmList = new List<MonsterSetInfo>();
        m_iColonyNoOrderNpcList = new List<int>();
        m_plotMissionTrigger = new List<string>();
        m_monsterHatredList = new List<int>();
        m_npcHatredList = new List<int>();
        m_harmList = new List<int>();
        m_doodadHarmList = new List<int>();
        cantReviveNpc = new List<int>();
        m_npcsBattle = new List<ENpcBattleInfo>();
        m_killMons = new List<KillMons>();
        m_doodadEffectList = new List<SetDoodadEffect>();
        m_npcType = new List<NpcType>();
        m_triggerPlot = new List<int>();
        m_killNpcList = new List<int>();
    }
}

public class MisInitRepository
{
    public static Dictionary<int, MissionInit> m_MisInitMap = new Dictionary<int, MissionInit>();

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Initialization");
        reader.Read();
        while (reader.Read())
        {
            MissionInit data = new MissionInit();
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            string tmp = reader.GetString(reader.GetOrdinal("Finished"));
            string[] tmplist = tmp.Split(';');
            string[] tmplist1;
            string[] tmplist2;
            float x, y, z;

            if (tmp != "0")
            {
                for (int i = 0; i < tmplist.Length; i++)
                {
                    tmplist1 = tmplist[i].Split('_');
                    for (int m = 0; m < tmplist1.Length; m++)
                    {
                        data.m_ComMisID.Add(Convert.ToInt32(tmplist1[m]));
                    }
                }
            }

            data.m_NComMisID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("UnFinished")));

            tmp = reader.GetString(reader.GetOrdinal("PlotID"));
            if (!tmp.Equals("0"))
            {
                tmplist = tmp.Split(',');
                foreach (var item in tmplist)
                {
                    data.m_triggerPlot.Add(Convert.ToInt32(item));
                }
            }

            //npc animation
            tmp = reader.GetString(reader.GetOrdinal("NPCact"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 4)
                    continue;

                NpcAct nf = new NpcAct();
                nf.npcid = Convert.ToInt32(tmplist1[0]);
                nf.animation = tmplist1[1];
                nf.btrue = (Convert.ToInt32(tmplist1[2]) == 1) ? true : false;
                nf.bFloat = Convert.ToSingle(tmplist1[3]);

                data.m_NpcAct.Add(nf);
            }

            //npcname_npcother(angle)_bmove
            tmp = reader.GetString(reader.GetOrdinal("NPCface"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length < 2)
                    continue;

                NpcFace nf = new NpcFace();
                nf.npcid = Convert.ToInt32(tmplist1[0]);
                int outinfo;
                if (int.TryParse(tmplist1[1], out outinfo))
                    nf.angle = Convert.ToInt32(tmplist1[1]);
                else
                    nf.npcother = tmplist1[1];

                if (tmplist1.Length == 3)
                    nf.bmove = (Convert.ToInt32(tmplist1[2]) == 1) ? true : false;

                data.m_NpcFace.Add(nf);
            }


            tmp = reader.GetString(reader.GetOrdinal("NPCcamp"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcCamp nc = new NpcCamp();
                nc.npcid = Convert.ToInt32(tmplist1[0]);
                nc.camp = Convert.ToInt32(tmplist1[1]);

                data.m_NpcCamp.Add(nc);
            }


            tmp = reader.GetString(reader.GetOrdinal("DelMonster"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                int id = Convert.ToInt32(tmplist[i]);
                if (id == 0)
                    continue;

                data.m_DeleteMonster.Add(id);
            }

            tmp = reader.GetString(reader.GetOrdinal("DelNPC"));
            if (tmp != "0")
            {
                tmplist = tmp.Split(',');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_iDeleteNpc.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("TransNPC"));
            tmplist = tmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcStyle ns = new NpcStyle();
                ns.npcid = Convert.ToInt32(tmplist1[0]);
                tmplist2 = tmplist1[1].Split(',');
                if (tmplist2.Length == 3)
                {
                    x = Convert.ToSingle(tmplist2[0]);
                    y = Convert.ToSingle(tmplist2[1]);
                    z = Convert.ToSingle(tmplist2[2]);
                    ns.pos = new Vector3(x, y, z);
                }

                data.m_TransNpc.Add(ns);
            }

            tmp = reader.GetString(reader.GetOrdinal("FolPlayer"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                NpcOpen no = new NpcOpen();
                no.npcid = Convert.ToInt32(tmplist1[0]);
                no.bopen = (Convert.ToInt32(tmplist1[1]) == 1) ? true : false;

                data.m_FollowPlayerList.Add(no);
            }

            tmp = reader.GetString(reader.GetOrdinal("Monstercamp"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                MonsterSetInfo mon;
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                mon.id = Convert.ToInt32(tmplist1[0]);
                mon.value = Convert.ToInt32(tmplist1[1]);
                if (mon.id == 0)
                    continue;

                data.m_MonsterCampList.Add(mon);
            }

            tmp = reader.GetString(reader.GetOrdinal("Monsterharm"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                MonsterSetInfo mon;
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                mon.id = Convert.ToInt32(tmplist1[0]);
                mon.value = Convert.ToInt32(tmplist1[1]);
                if (mon.id == 0)
                    continue;

                data.m_MonsterHarmList.Add(mon);
            }

            data.m_Special = reader.GetString(reader.GetOrdinal("Special"));

            tmp = reader.GetString(reader.GetOrdinal("ColonyNoOrder"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                data.m_iColonyNoOrderNpcList.Add(Convert.ToInt32(tmplist[i]));
            }

            tmp = reader.GetString(reader.GetOrdinal("MonsterHatred"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_monsterHatredList.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCHatred"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_npcHatredList.Add(Convert.ToInt32(tmplist[i]));
                }
            }


            tmp = reader.GetString(reader.GetOrdinal("Harm"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_harmList.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("DoodadHarm"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                if (tmplist.Length == 3)
                {
                    foreach (var item in tmplist)
                    {
                        data.m_doodadHarmList.Add(Convert.ToInt32(item));
                    }
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("DoodadEffect"));
            if (tmp != "0")
            {
                tmplist = tmp.Split(';');
                SetDoodadEffect deInfo;
                foreach (var item in tmplist)
                {
                    tmplist1 = item.Split(':');
                    if (tmplist1.Length != 3)
                        continue;
                    deInfo.id = Convert.ToInt32(tmplist1[0]);

                    deInfo.names = new List<string>();
                    tmplist2 = tmplist1[1].Split(',');
                    foreach (var name in tmplist2)
                        deInfo.names.Add(name);

                    deInfo.openOrClose = Convert.ToInt32(tmplist1[2]) == 1 ? true : false;
                    data.m_doodadEffectList.Add(deInfo);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("ENpcBattle"));
            if (tmp != "0")
            {
                tmplist = tmp.Split(';');
                ENpcBattleInfo nbInfo;
                foreach (var item in tmplist)
                {
                    tmplist1 = item.Split(',', '_');
                    if (tmplist1.Length < 2)
                        continue;
                    nbInfo.npcId = new List<int>();
                    for (int i = 0; i < tmplist1.Length - 1; i++)
                    {
                        nbInfo.npcId.Add(Convert.ToInt32(tmplist1[i]));
                    }
                    nbInfo.type = Convert.ToInt32(tmplist1[tmplist1.Length - 1]);
                    data.m_npcsBattle.Add(nbInfo);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("Area"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_plotMissionTrigger.Add(tmplist[i]);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("CantRevive"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
			{
                if(Convert.ToInt32(tmplist[i]) == 0)
                    continue;
                data.cantReviveNpc.Add(Convert.ToInt32(tmplist[i]));
			}

            tmp = reader.GetString(reader.GetOrdinal("KillMons"));
            tmplist = tmp.Split(';');
            KillMons kmInfo;
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 5)
                    continue;
                kmInfo.id = Convert.ToInt32(tmplist1[0]);
                tmplist2 = tmplist1[1].Split(',');
                if (tmplist2.Length == 3)
                {
                    x = Convert.ToSingle(tmplist2[0]);
                    y = Convert.ToSingle(tmplist2[1]);
                    z = Convert.ToSingle(tmplist2[2]);
                    kmInfo.center = new Vector3(x, y, z);
                    kmInfo.radius = Convert.ToSingle(tmplist1[2]);
                    kmInfo.type = KillMons.Type.protoTypeId;
                }
                else
                {
                    kmInfo.center = Vector3.zero;
                    kmInfo.radius = 0;
                    kmInfo.type = KillMons.Type.fixedId;
                }

                kmInfo.monId = Convert.ToInt32(tmplist1[3]);
                kmInfo.reviveTime = Convert.ToInt32(tmplist1[4]);

                data.m_killMons.Add(kmInfo);
            }

            tmp = reader.GetString(reader.GetOrdinal("NPCType"));
            tmplist = tmp.Split(';');
            foreach (var item in tmplist)
            {
                if (item.Equals("0"))
                    continue;
                NpcType nt;
                nt.npcs = new List<int>();
                nt.type = -1;
                string[] conPre = item.Split('_');
                for (int i = 0; i < conPre.Length; i++)
                {
                    if (i < (conPre.Length - 1))
                        nt.npcs.Add(Convert.ToInt32(conPre[i]));
                    else
                        nt.type = Convert.ToInt32(conPre[i]);
                }
                data.m_npcType.Add(nt);
            }

            tmp = reader.GetString(reader.GetOrdinal("KillNPC"));
            if (tmp != "0")
            {
                tmplist = tmp.Split('_', ';');
                for (int i = 0; i < tmplist.Length; i++)
                {
                    data.m_killNpcList.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            m_MisInitMap.Add(data.m_ID, data);
        }
    }
}


public class CameraMove
{
    public int type;    //1推移， 2瞬移
    public Vector3 pos;
    public int npcid;
    public int dirType; //1身前， 2身后
}

public class CameraRot
{
    public int type;    //1旋转， 2瞬转
    public int type1;   //1水平， 2垂直, 3水平垂直同时
    public float angleX;
    public float angleY;
    public int dirType; //1顺时针， 2逆时针
}

public class CameraTrack
{
    public int npcid;
    public int type;    //1角度固定， 2坐标固定,  3都不固定
}

public class CameraPlot
{
    public int m_ID;
    public int m_ControlType;       //0立即执行， 1与前一条同时执行， 2等待之前命令完成再执行
    public CameraMove m_CamMove;
    public CameraRot m_CamRot;
    public CameraTrack m_CamTrack;
    public bool m_CamToPlayer;      //1交回玩家角度， //0交给任务摄像机控制
    public int m_Delay;

    public CameraPlot()
    {
        m_CamMove = new CameraMove();
        m_CamRot = new CameraRot();
        m_CamTrack = new CameraTrack();
    }
}

public class CameraRepository
{
    public static Dictionary<int, CameraPlot> m_CameraMap = new Dictionary<int, CameraPlot>();

    public static CameraPlot GetCameraPlotData(int id)
    {
        if (!m_CameraMap.ContainsKey(id))
            return null;

        return m_CameraMap[id];
    }

    public static void LoadCameraPlot()
    {
        string[] tmplist1;
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Camera");
        reader.Read();
        while (reader.Read())
        {
            CameraPlot data = new CameraPlot();
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            data.m_ControlType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));

            string tmp = reader.GetString(reader.GetOrdinal("CamPos"));
            string[] tmplist = tmp.Split('_');
            if (tmplist.Length == 3)
            {
                data.m_CamMove.type = Convert.ToInt32(tmplist[0]);
                tmplist1 = tmplist[1].Split(',');
                if (tmplist1.Length == 3)
                {
                    data.m_CamMove.pos.x = Convert.ToSingle(tmplist1[0]);
                    data.m_CamMove.pos.y = Convert.ToSingle(tmplist1[1]);
                    data.m_CamMove.pos.z = Convert.ToSingle(tmplist1[2]);
                }
                else
                    data.m_CamMove.npcid = Convert.ToInt32(tmplist[1]);

                data.m_CamMove.dirType = Convert.ToInt32(tmplist[2]);
            }

            tmp = reader.GetString(reader.GetOrdinal("CamRotation"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 4)
            {
                data.m_CamRot.type = Convert.ToInt32(tmplist[0]);
                data.m_CamRot.type1 = Convert.ToInt32(tmplist[1]);
                data.m_CamRot.angleY = Convert.ToSingle(tmplist[2]);
                data.m_CamRot.dirType = Convert.ToInt32(tmplist[3]);
            }
            else if (tmplist.Length == 5)
            {
                data.m_CamRot.type = Convert.ToInt32(tmplist[0]);
                data.m_CamRot.type1 = Convert.ToInt32(tmplist[1]);
                data.m_CamRot.angleY = Convert.ToSingle(tmplist[2]);
                data.m_CamRot.angleX = Convert.ToSingle(tmplist[3]);
                data.m_CamRot.dirType = Convert.ToInt32(tmplist[4]);
            }

            tmp = reader.GetString(reader.GetOrdinal("CamFollow"));
            tmplist = tmp.Split('_');
            if (tmplist.Length == 2)
            {
                data.m_CamTrack.npcid = Convert.ToInt32(tmplist[0]);
                data.m_CamTrack.type = Convert.ToInt32(tmplist[1]);
            }

            data.m_CamToPlayer = Convert.ToInt32(reader.GetString(reader.GetOrdinal("CamtoPlayer"))) > 0 ? true : false;

            data.m_Delay = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Delay")));

            m_CameraMap.Add(data.m_ID, data);
        }
    }
}
