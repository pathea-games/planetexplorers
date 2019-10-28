using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ItemAsset;

public class UIMissionMgr : MonoBehaviour
{
    static UIMissionMgr mInstance;
    public static UIMissionMgr Instance
	{
		get { return mInstance; }
	}

    //private MissionLabelMgr mLabelMissionMgr = null; // 地图图标事件注册

    void Awake()
    {
        mInstance = this;
        ClearMission();
        new MissionLabelMgr();
    }

    public class MissionView
    {
        public int mMissionID;
        public MissionType mMissionType; // 任务类型
        public string mMissionTitle;	//	任务标题                       
        public string mMissionDesc;  	//  任务描述  

        public bool mMissionTag;   // 任务追踪
        public bool mComplete;     // 完成任务

        public NpcInfo mMissionStartNpc; // 接任务Npc 
        public NpcInfo mMissionEndNpc;   // 交任务Npc
        public NpcInfo mMissionReplyNpc; // 显示头像npc

        public bool NeedArrow;
        public Vector3 mEndMisPos; // 交任务坐标
        public int mAttachOnId;

        public List<TargetShow> mTargetList;
        public List<ItemSample> mRewardsList;

        public MissionView()
        {
            mTargetList = new List<TargetShow>();
            mRewardsList = new List<ItemSample>();
            mMissionStartNpc = new NpcInfo();
            mMissionEndNpc = new NpcInfo();
            mMissionReplyNpc = new NpcInfo();
            mComplete = false;
            mMissionTag = true;
            mEndMisPos = Vector3.zero;
            NeedArrow = false;
        }

        //wangxiaoliang jia de 
        public static bool MatchID(TargetShow ite, int targetid)
        {
            return (ite.mID == targetid);
        }
        //------------------------
    };

    public class GetableMisView  // 可以接的任务
    {
        public int mMissionID;
        public string mMissionTitle;
        public Vector3 mPosition;  // 位置
        public NpcInfo TargetNpcInfo;
        public int mAttachOnId;

        public GetableMisView(int missionID, string misTitle, Vector3 positon, int attachOnID)
        {
            mMissionID = missionID;
            mMissionTitle = misTitle;
            mPosition = positon;
            TargetNpcInfo = new NpcInfo();
            mAttachOnId = attachOnID;
        }
    }

    public class NpcInfo
    {
        public string mName; //名字 
        public string mNpcIcoStr; //头像
    }

    public class TargetShow
    {
        public string mContent;
        public List<string> mIconName;
        public bool mComplete;

        public int mCount;
        public int mMaxCount;

        public int mID;
        public Vector3 mPosition;
        public float Radius;
        public int mAttachOnID;

        public TargetShow(int id = 0)
        {
            mID = id;
            mContent = "";
            mIconName = new List<string>();
            mCount = 0;
            mMaxCount = 0;
            mComplete = false;
            mPosition = Vector3.zero;
            Radius = -1f;
        }
    }

    public delegate void MissionViewEvent(MissionView misView);
    public event MissionViewEvent e_AddMission = null;
    public event MissionViewEvent e_DeleteMission = null;
    public event MissionViewEvent e_CheckTagMission = null;

    public delegate void BaseMissionEvent();
    public event BaseMissionEvent e_ReflahMissionWnd = null;


    public delegate void UnMisViewEvent(GetableMisView unMisView);
    public event UnMisViewEvent e_AddGetableMission = null;
    public event UnMisViewEvent e_DelGetableMission = null;

    public Dictionary<int, MissionView> m_MissonMap = new Dictionary<int, MissionView>(); // 接受的任务
    public Dictionary<int, GetableMisView> m_GetableMissonMap = new Dictionary<int, GetableMisView>(); // 可以接任务

    #region Mission event funcs

    public void ClearMission()
    {
        m_MissonMap.Clear();
        m_GetableMissonMap.Clear();
    }

    public MissionView GetMissionView(int missionID)
    {
        if (m_MissonMap.ContainsKey(missionID))
            return m_MissonMap[missionID];
        else
            return null;
    }

    public void RefalshMissionWnd()
    {
        if (e_ReflahMissionWnd != null)
            e_ReflahMissionWnd();
    }

    public bool AddMission(MissionView misView, bool isIgnoreHadCreate = false)
    {
        if (!isIgnoreHadCreate)
        {
            if (m_MissonMap.ContainsKey(misView.mMissionID))
                return false;
            m_MissonMap[misView.mMissionID] = misView;

        }

        if (e_AddMission != null)
            e_AddMission(misView);

        return true;
    }

    public bool DeleteMission(int missionID)
    {
        if (!m_MissonMap.ContainsKey(missionID))
            return false;

        if (e_DeleteMission != null)
            e_DeleteMission(m_MissonMap[missionID]);

        return m_MissonMap.Remove(missionID);
    }


    public bool DeleteMission(MissionView misView)
    {
        //lz-2017.01.03 crash bug 错误 #8204
        if (null == misView) return false;

        return DeleteMission(misView.mMissionID);

    }

    public void UpdateGetableMission() 
    {
        List<int> removeRecord = null;
        foreach (var item in m_GetableMissonMap.Keys)
        {
            if (MissionManager.Instance.IsGetTakeMission(item))
                continue;
            if (e_DelGetableMission != null)
                e_DelGetableMission(m_GetableMissonMap[item]);
            if (removeRecord == null)
                removeRecord = new List<int>();
            removeRecord.Add(item);
        }
        if (removeRecord != null) 
        {
            foreach (var item in removeRecord)
                m_GetableMissonMap.Remove(item);
        }
    }

    public void DeleteMission(Pathea.PeEntity npc) 
    {
        if (!Pathea.PeGameMgr.IsMulti)
            return;
        NpcMissionData missionData = Pathea.PeEntityExt.PeEntityExt.GetUserData(npc) as NpcMissionData;
        if (missionData == null)
            return;
        if (npc.proto == Pathea.EEntityProto.Npc)
        {
            foreach (var item in missionData.m_MissionList)
            {
                if (!MissionManager.Instance.IsGetTakeMission(item))
                    continue;

                if (!m_GetableMissonMap.ContainsKey(item))
                    continue;

                if (e_DelGetableMission != null)
                    e_DelGetableMission(m_GetableMissonMap[item]);
            }
            foreach (var item in missionData.m_MissionListReply)
            {
                if (MissionManager.Instance.HadCompleteMission(item))
                    continue;

                if (!m_MissonMap.ContainsKey(item))
                    continue;

                if (e_DeleteMission != null)
                    e_DeleteMission(m_MissonMap[item]);
            }
        }
        else if (npc.proto == Pathea.EEntityProto.RandomNpc && Pathea.PeGameMgr.IsStory)
        {
            if (missionData.m_RandomMission != 0)
            {
                if (m_GetableMissonMap.ContainsKey(missionData.m_RandomMission))
                {
                    if (e_DelGetableMission != null)
                        e_DelGetableMission(m_GetableMissonMap[missionData.m_RandomMission]);
                }
                if (m_MissonMap.ContainsKey(missionData.m_RandomMission))
                {
                    if (e_DeleteMission != null)
                        e_DeleteMission(m_MissonMap[missionData.m_RandomMission]);
                }
            }
        }
    }

    public void AddMission(Pathea.PeEntity npc) 
    {
        if (!Pathea.PeGameMgr.IsMulti)
            return;
        NpcMissionData missionData = Pathea.PeEntityExt.PeEntityExt.GetUserData(npc) as NpcMissionData;
        if (missionData == null)
            return;

        MissionCommonData mcd;
        if (npc.proto == Pathea.EEntityProto.Npc)
        {
            foreach (var item in missionData.m_MissionList)
            {
                if (!m_GetableMissonMap.ContainsKey(item))
                    continue; ;

                mcd = MissionManager.GetMissionCommonData(item);
                if (mcd == null)
                    continue;

                UIMissionMgr.GetableMisView gmv = new UIMissionMgr.GetableMisView(item, mcd.m_MissionName, npc.position, npc.Id);
                gmv.TargetNpcInfo.mName = npc.enityInfoCmpt.characterName.fullName;
                gmv.TargetNpcInfo.mNpcIcoStr = string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig) ? "npc_big_Unknown" : npc.enityInfoCmpt.faceIconBig;
                UIMissionMgr.Instance.AddGetableMission(gmv, true);
            }
            foreach (var item in missionData.m_MissionListReply)
            {
                if (!m_MissonMap.ContainsKey(item))
                    continue;

                mcd = MissionManager.GetMissionCommonData(item);
                if (mcd == null)
                    continue;

                Dictionary<string, string> missionFlagType = MissionManager.Instance.m_PlayerMission.GetMissionFlagType(item);
                if (missionFlagType == null)
                    continue;

                UIMissionMgr.MissionView mv = new UIMissionMgr.MissionView();
                mv.mMissionID = mcd.m_ID;
                mv.mMissionType = mcd.m_Type;
                mv.mMissionTitle = mcd.m_MissionName;
                npc = Pathea.EntityMgr.Instance.Get(mcd.m_iNpc);
                if (npc != null)
                {
					mv.mMissionStartNpc.mName = npc.enityInfoCmpt.characterName.fullName;
                    mv.mMissionStartNpc.mNpcIcoStr = string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig) ? "npc_big_Unknown" : npc.enityInfoCmpt.faceIconBig;
                }

                npc = Pathea.EntityMgr.Instance.Get(mcd.m_iReplyNpc);
                if (npc != null)
                {
					mv.mMissionEndNpc.mName = npc.enityInfoCmpt.characterName.fullName;
                    mv.mMissionEndNpc.mNpcIcoStr = string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig) ? "npc_big_Unknown" : npc.enityInfoCmpt.faceIconBig;
                    mv.mEndMisPos = npc.position;
                    mv.mAttachOnId = npc.Id;
                    mv.NeedArrow = true;
                }
                MissionManager.Instance.ParseMissionFlag(mcd, missionFlagType, mv);
                UIMissionMgr.Instance.AddMission(mv, true);
                UIMissionMgr.Instance.RefalshMissionWnd();
            }
        }
        else if (npc.proto == Pathea.EEntityProto.RandomNpc && Pathea.PeGameMgr.IsStory)
        {
            if (missionData.m_RandomMission != 0)
            {

                mcd = MissionManager.GetMissionCommonData(missionData.m_RandomMission);
                if (mcd != null)
                {
                    if (m_GetableMissonMap.ContainsKey(missionData.m_RandomMission))
                    {
                        UIMissionMgr.GetableMisView gmv = new UIMissionMgr.GetableMisView(missionData.m_RandomMission, mcd.m_MissionName, npc.position, npc.Id);
						gmv.TargetNpcInfo.mName = npc.enityInfoCmpt.characterName.fullName;
                        gmv.TargetNpcInfo.mNpcIcoStr = string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig) ? "npc_big_Unknown" : npc.enityInfoCmpt.faceIconBig;
                        UIMissionMgr.Instance.AddGetableMission(gmv, true);
                    }
                    if (m_MissonMap.ContainsKey(missionData.m_RandomMission))
                    {
                        Dictionary<string, string> missionFlagType = MissionManager.Instance.m_PlayerMission.GetMissionFlagType(missionData.m_RandomMission);
                        if (missionFlagType != null)
                        {
                            UIMissionMgr.MissionView mv = new UIMissionMgr.MissionView();
                            mv.mMissionID = mcd.m_ID;
                            mv.mMissionType = mcd.m_Type;
                            mv.mMissionTitle = mcd.m_MissionName;
                            npc = Pathea.EntityMgr.Instance.Get(mcd.m_iNpc);
                            if (npc != null)
                            {
								mv.mMissionStartNpc.mName = npc.enityInfoCmpt.characterName.fullName;
                                mv.mMissionStartNpc.mNpcIcoStr = string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig) ? "npc_big_Unknown" : npc.enityInfoCmpt.faceIconBig;
                            }

                            npc = Pathea.EntityMgr.Instance.Get(mcd.m_iReplyNpc);
                            if (npc != null)
                            {
                                mv.mMissionEndNpc.mName = npc.enityInfoCmpt.characterName.fullName;
                                mv.mMissionEndNpc.mNpcIcoStr = string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig) ? "npc_big_Unknown" : npc.enityInfoCmpt.faceIconBig;
                                mv.mEndMisPos = npc.position;
                                mv.mAttachOnId = npc.Id;
                                mv.NeedArrow = true;
                            }
                            MissionManager.Instance.ParseMissionFlag(mcd, missionFlagType, mv);
                            UIMissionMgr.Instance.AddMission(mv, true);
                            UIMissionMgr.Instance.RefalshMissionWnd();
                        }
                    }
                }
            }
        }
    }
    public void CheckMissionTag(MissionView misView)
    {
        if (e_CheckTagMission != null)
            e_CheckTagMission(misView);
    }

    #endregion



    #region GetabllMissionView Func

    public bool AddGetableMission(GetableMisView unMisView,bool isIgnoreHadCreate = false)
    {
        if (!isIgnoreHadCreate)
        {
            if (m_GetableMissonMap.ContainsKey(unMisView.mMissionID))
                return false;
            m_GetableMissonMap[unMisView.mMissionID] = unMisView;
        }

        if (e_AddGetableMission != null)
            e_AddGetableMission(unMisView);
        return true;
    }

    public bool DeleteGetableMission(GetableMisView unMisView)
    {
        return DeleteGetableMission(unMisView.mMissionID);
    }

    public bool DeleteGetableMission(int missionID)
    {
        if (!m_GetableMissonMap.ContainsKey(missionID))
            return false;

        if (e_DelGetableMission != null)
            e_DelGetableMission(m_GetableMissonMap[missionID]);

        return m_GetableMissonMap.Remove(missionID);
    }

    #endregion
}


#region MissionMapLabel

public class MissionLabelMgr
{
    public MissionLabelMgr()
    {
        UIMissionMgr.Instance.e_AddMission += OnAddMission;
        UIMissionMgr.Instance.e_DeleteMission += OnDeleteMission;
        UIMissionMgr.Instance.e_AddGetableMission += OnAddGetableMission;
        UIMissionMgr.Instance.e_DelGetableMission += OnDelGetableMission;
    }


    void OnAddMission(UIMissionMgr.MissionView misView)
    {
        if (misView == null)
            return;
        if (misView.mEndMisPos != Vector3.zero)
        {
            // Add MisEnd Label
            MissionLabel label = new MissionLabel(misView.mMissionID,
                                            MissionLabelType.misLb_end,
                                            misView.mEndMisPos,
                                            misView.mMissionTitle,
                                            -1f,
                                            misView.NeedArrow,
                                            misView.mAttachOnId);

            PeMap.LabelMgr.Instance.Add(label);
        }

        // Add MisTarget Label
        foreach (UIMissionMgr.TargetShow target in misView.mTargetList)
        {
            if(target.mComplete != true)
            {
                if (target.mPosition != Vector3.zero )
                {
                    MissionLabel label = new MissionLabel(misView.mMissionID,
                                                    MissionLabelType.misLb_target,
                                                    target.mPosition,
                                                    target.mContent,
                                                    target.Radius,
                                                    false,
                                                    target.mAttachOnID,
                                                    target);
                    PeMap.LabelMgr.Instance.Add(label);
                }
            }
        }
    }

    void OnDeleteMission(UIMissionMgr.MissionView misView)
    {
        if (misView == null)
            return;
        //Remove MissionLabel
        List<PeMap.ILabel> RemoveList = PeMap.LabelMgr.Instance.FindAll(itr => RemoveMissionLabel(misView.mMissionID, itr));
        foreach (PeMap.ILabel _ilabel in RemoveList)
        {
            PeMap.LabelMgr.Instance.Remove(_ilabel);
        }
        RemoveList.Clear();
    }

    bool RemoveMissionLabel(int missionID, PeMap.ILabel label)
    {
        if (label.GetType() != PeMap.ELabelType.Mission)
            return false;
        if (!(label is MissionLabel))
            return false;

        MissionLabel msLabel = label as MissionLabel;

        if (msLabel.m_type == MissionLabelType.misLb_unActive)
            return false;

        return msLabel.m_missionID == missionID;
    }

    void OnAddGetableMission(UIMissionMgr.GetableMisView getableView)
    {
        if (getableView == null)
            return;
        if (getableView.mPosition != Vector3.zero)
        {
            // Add MisEnd Label
            MissionLabel label = new MissionLabel(getableView.mMissionID,
                                                  MissionLabelType.misLb_unActive,
                                                  getableView.mPosition,
                                                  getableView.mMissionTitle,
                                                  -1f,
                                                  false,
                                                  getableView.mAttachOnId);

            PeMap.LabelMgr.Instance.Add(label);
        }
    }

    void OnDelGetableMission(UIMissionMgr.GetableMisView getableView)
    {
        if (getableView == null)
            return;
        //Remove MissionLabel
        List<PeMap.ILabel> RemoveList = PeMap.LabelMgr.Instance.FindAll(itr => RemoveGetableLabel(getableView.mMissionID, itr));
        foreach (PeMap.ILabel _ilabel in RemoveList)
        {
            PeMap.LabelMgr.Instance.Remove(_ilabel);
        }
        RemoveList.Clear();
    }

    bool RemoveGetableLabel(int missionID, PeMap.ILabel label)
    {
        if (label.GetType() != PeMap.ELabelType.Mission)
            return false;
        if (!(label is MissionLabel))
            return false;

        MissionLabel msLabel = label as MissionLabel;

        if (msLabel.m_type != MissionLabelType.misLb_unActive)
            return false;

        return msLabel.m_missionID == missionID;
    }

}

public enum MissionLabelType
{
    misLb_none = 0,
    misLb_unActive, //未接的任务标记
    misLb_target, // 任务区域标记
    misLb_end, // 交任务的标记
    misLb_Max
}

public class ReviveLabel : PeMap.ILabel ,PeMap.ISerializable
{
    public Vector3 pos;

    public int GetIcon()
    {
        return 43;
    }

    public Vector3 GetPos()
    {
        return pos;
    }

    public string GetText()
    {
        return PELocalization.GetString(8000182);
    }

    public bool FastTravel()
    {
        return false;
    }

    public new PeMap.ELabelType GetType()
    {
        return PeMap.ELabelType.Revive;
    }

    public bool NeedArrow()
    {
        return true;
    }

    public float GetRadius()
    {
        return 20;
    }

    public PeMap.EShow GetShow()
    {
        return PeMap.EShow.All;
    }

    byte[] PeMap.ISerializable.Serialize()
    {
        return PETools.Serialize.Export((w) =>
        {
            PETools.Serialize.WriteVector3(w, pos);
        });
    }

    void PeMap.ISerializable.Deserialize(byte[] data)
    {
        PETools.Serialize.Import(data, (r) =>
        {
            pos = PETools.Serialize.ReadVector3(r);
        });
        PeMap.LabelMgr.Instance.Add(this);
    }

    public class Mgr : PeMap.ArchivableLabelMgr<Mgr, ReviveLabel>
    {
        protected override string GetArchiveKey()
        {
            return "ArchivableLabelMgr";
        }

        public override void Add(ReviveLabel item)
        {
            ReviveLabel only = Find(e => true);
            if (only != null)
                base.Remove(only);
            base.Add(item);
        }
    }

    #region lz-2016.06.02

    public bool CompareTo(PeMap.ILabel label)
    {
        if (label is ReviveLabel)
        {
            ReviveLabel reviveLabel = (ReviveLabel)label;
            if (this.pos == reviveLabel.pos)
                return true;
            return false;
        }
        return false;
    }

    #endregion
}


public class MissionLabel : PeMap.ILabel
{
    public int m_missionID = 0;
    public MissionLabelType m_type = MissionLabelType.misLb_none;
    public UIMissionMgr.TargetShow m_target = null;
    public bool IsComplete=false;
    public int m_attachOnID;
    public bool NeedOneRefreshPos=false;

    string m_text = string.Empty;
    Vector3 m_postion;
    float m_raidus;
    bool m_needArrow;

    public MissionLabel(int missionID, MissionLabelType type, Vector3 pos, string text, float raidus, bool needArrow, int attachOnId, UIMissionMgr.TargetShow target = null)
    {
        m_type = type;
        m_postion = pos;
        m_text = text;
        m_raidus = raidus;
        m_missionID = missionID;
        m_needArrow = needArrow;
        m_attachOnID = attachOnId;
        m_target = target;
        UIMissionMgr.MissionView view = UIMissionMgr.Instance.GetMissionView(m_missionID);
        if (null != view)
        {
            IsComplete = view.mComplete;
        }
    }

    public void SetLabelPos(Vector3 v, bool needOneRefreshPos=false)
    {
        m_postion = v;
        //lz-2016.10.27 多人那边有时候misLb_target需要先添加，后刷新位置
        NeedOneRefreshPos = needOneRefreshPos;
    }

    int PeMap.ILabel.GetIcon()
    {
        switch (m_type)
        {
            case MissionLabelType.misLb_unActive:
                return PeMap.MapIcon.TaskGetable;
            case MissionLabelType.misLb_target:
                return PeMap.MapIcon.TaskTarget;
            case MissionLabelType.misLb_end:
                return PeMap.MapIcon.TaskCmplt;
            default:
                break;
        }
        return PeMap.MapIcon.None;
    }

    public Color32 GetMissionColor()
    {
        Color32 col = new Color32(255, 255, 255, 255);
        switch (m_type)
        {
            case MissionLabelType.misLb_target:
                col= MissionMapLabelColor.MissionTargetCol;
                break;
            case MissionLabelType.misLb_unActive:
            case MissionLabelType.misLb_end:
                if (m_type == MissionLabelType.misLb_end&& !IsComplete)
                    col = MissionMapLabelColor.UnFinishedCol; //lz-2016.10.13 未完成的任务不区分主线和支线，使用一个颜色
                else
                    col=MissionManager.IsMainMission(m_missionID) ? MissionMapLabelColor.MainLineCol : MissionMapLabelColor.SideLineCol;
                break;
        }
        return col;
    }

    Vector3 PeMap.ILabel.GetPos()
    {
        return m_postion;
    }

    string PeMap.ILabel.GetText()
    {
		return m_text=="0"?"":m_text;
    }

    bool PeMap.ILabel.FastTravel()
    {
        return false;
    }

    PeMap.ELabelType PeMap.ILabel.GetType()
    {
        return PeMap.ELabelType.Mission;
    }

    bool PeMap.ILabel.NeedArrow()
    {
        return m_needArrow;
    }

    float PeMap.ILabel.GetRadius()
    {
        return m_raidus;
    }



    PeMap.EShow PeMap.ILabel.GetShow()
    {
        switch (m_type)
        {
            case MissionLabelType.misLb_unActive:
                //lz-2016.08.05 如果是主线任务在大小地图上都显示
                if(MissionManager.IsMainMission(m_missionID))
                    return PeMap.EShow.All;
                return PeMap.EShow.MinMap;
            case MissionLabelType.misLb_target:
                return PeMap.EShow.All;
            case MissionLabelType.misLb_end:
                return PeMap.EShow.All;
            default:
                break;
        }
        return PeMap.EShow.Max;
    }

    #region lz-2016.06.02 

    public bool CompareTo(PeMap.ILabel label)
    {
        if (label is MissionLabel)
        {
            MissionLabel missionLabel = (MissionLabel)label;
            //lz-2016.06.24 会有多个label的missionid，type，postion都一样，只是半径不一样比如421任务，所以这里再加入半径判断
            if (this.m_missionID == missionLabel.m_missionID 
                && this.m_type == missionLabel.m_type 
                && missionLabel.m_postion == m_postion 
                && missionLabel.m_raidus==m_raidus)
                return true;
            else
                return false;
        }
        else
            return false;
    }
    #endregion
}

#endregion
