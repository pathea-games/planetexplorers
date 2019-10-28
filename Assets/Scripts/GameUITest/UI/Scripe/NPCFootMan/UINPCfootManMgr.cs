using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;

public class UINPCfootManMgr : MonoBehaviour
{

    static UINPCfootManMgr mInstance;
    public static UINPCfootManMgr Instance { get { return mInstance; } }

    [SerializeField]
    Transform Centent = null;

    [SerializeField]
    GameObject UIfootManItemPrefab = null;

    [HideInInspector]
    public UIfootManItem mUIfootManItem = null;

    [SerializeField]
    UIGrid mGird;




    public class FootmanInfo
    {

        public int Num;
        public int Followerid;
        public Texture mTexture;
        public float Hppercent;
        public NpcCmpt mNpCmpt;

    }
    List<FootmanInfo> mInfoList = new List<FootmanInfo>();

    public List<UIfootManItem> mItemList = new List<UIfootManItem>();


    Pathea.PeEntity mEntity;
    public Pathea.PeEntity Entity
    {
        get
        {
            if (mEntity == null)
            {
                mEntity = GetComponent<Pathea.PeEntity>();
            }
            return mEntity;
        }
    }


    void Awake()
    {
        mInstance = this;
        AddFootManItem(ServantLeaderCmpt.mMaxFollower);
        StartCoroutine(NewGetFollowerAlive());
    }

    IEnumerator NewGetFollowerAlive()
    {
        yield return new WaitForSeconds(5f);
        GetFollowerAlive();
    }

    void IntServant()
    {

    }

    // Use this for initialization
    void Start()
    {
        if (ServantLeaderCmpt.Instance == null)
            return;


    }
    public Vector3 newPos;
    void Update()
    {
        Centent.localPosition = new Vector3(-UIMinMapCtrl.Instance.GetMinMapWidth() - 20, -5, -22);

        //UpdataDeadTime();
        GameUI.Instance.mRevive.mUpdate();
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    Debug.Log(ServantDeadLabel.ServantDeadLabelList.Count);

        //    foreach (ServantDeadLabel item in ServantDeadLabel.ServantDeadLabelList.Values)
        //    {
        //        Debug.Log(item.servantId);
        //    }
          
        //}
    }

    List<SkAliveEntity> mAlive = new List<SkAliveEntity>();
    //int countFollower = 0;
    //bool ShowNpc = false;

    public void GetFollowerAlive()
    {
        if (ServantLeaderCmpt.Instance == null)
            return;
        mInfoList.Clear();
        mAlive.Clear();
        foreach (NpcCmpt cmpt in ServantLeaderCmpt.Instance.mFollowers)
        {
            if (cmpt != null && cmpt.Alive != null)
            {
                //cmpt.Alive.onHpChange -= OnHpChange;
                //cmpt.Alive.deathEvent -= OnServantDead;
                //cmpt.Alive.reviveEvent -= OnServantRevive;
                //cmpt.Alive.onHpChange += OnHpChange;
                //cmpt.Alive.deathEvent += OnServantDead;
                //cmpt.Alive.reviveEvent += OnServantRevive;
                mAlive.Add(cmpt.Alive);
                FootmanInfo Info = new FootmanInfo();
                //Info.Followerid = cmpt.Alive.Entity.Id;
                //Info.Hppercent = cmpt.Alive.HPPercent;
                Info.mNpCmpt = cmpt;
                Info.mTexture = Pathea.PeEntityExt.PeEntityExt.ExtGetFaceTex(cmpt.Follwerentity);
                mInfoList.Add(Info);

            }
        }

        for (int i = 0; i < mItemList.Count; i++)
        {
            //lz-2016.09.22 更新信息的时候再显示出来
            mItemList[i].gameObject.SetActive(true);
            if (i < mInfoList.Count)
            {
                mItemList[i].FootmanInfo = mInfoList[i];
            }
            else
            {
                mItemList[i].FootmanInfo = null;
            }
        }

        for (int i = 0; i < mItemList.Count; i++)
        {
            if (i < mAlive.Count)
            {
                mItemList[i].SkEntity = mAlive[i];
            }
            else
            {
                mItemList[i].SkEntity = null;
            }
        }

        mGird.Reposition();

    }


    //void OnServantDead(SkEntity skSelf, SkEntity skCaster)
    //{
    //    ServantDeadLabel sd = new ServantDeadLabel(PeMap.MapIcon.ServantDeadPlace,);
    //}

    //void OnServantRevive(SkEntity skSelf)
    //{

    //}

    //NpcCmpt GetNpccmpt(SkEntity skentity)
    //{
    //    for (int i = 0; i < mItemList.Count; i++)
    //    {
    //        if (mItemList[i].npcCmpt == null)
    //            continue;
    //       if((SkEntity)mItemList[i].npcCmpt.Alive==skentity) 
    //    }
    //}

    float m_deathTime;
    //float m_DelayTime = 100000.0f;
    public bool m_dead;
    SkAliveEntity m_deadNpc;
    //void OnNpcDead(SkEntity skSelf, SkEntity skCaster)
    //{
    //    m_DelayTime = 100000.0f;
    //    m_dead = true;
    //    m_deadNpc = skSelf as SkAliveEntity;
    //    if (m_deadNpc != null)
    //    {
    //        foreach (UIfootManItem item in mItemList)
    //        {
    //            if (item.FollowerId == m_deadNpc.Entity.Id)
    //            {
    //                //item.ShowNpcDead(true);
    //                return;
    //            }
    //        }
    //    }

    //}

    //void UpdataDeadTime()
    //{
    //    if (m_dead)
    //    {
    //        m_DelayTime--;
    //        GameUI.Instance.mRevive.SetReviveWaitTime(m_DelayTime / 80);

    //        if (m_DelayTime / 80 == 0)
    //        {
    //            if ((ServantLeaderCmpt.Instance != null) && (m_deadNpc != null))
    //            {
    //                m_dead = !ServantLeaderCmpt.Instance.RemoveServant(m_deadNpc.Entity.GetCmpt<NpcCmpt>());

    //                if (!m_dead)
    //                    GameUI.Instance.mRevive.HideServantRevive();
    //            }

    //        }
    //    }

    //}
    //void OnRevive(SkEntity entity)
    //{
    //    SkAliveEntity Alive = entity as SkAliveEntity;
    //    if (Alive != null)
    //    {
    //        m_dead = false;
    //        foreach (UIfootManItem item in mItemList)
    //        {
    //            if (item.FollowerId == Alive.Entity.Id)
    //            {
    //                //item.ShowNpcDead(false);
    //                return;
    //            }
    //        }
    //    }
    //}

    //void OnHpChange(SkEntity caster, float hpChange)
    //{
    //    ReflashItem();
    //    //UpdateHp();
    //}

    //void UpdateHp()
    //{
    //    foreach (SkAliveEntity alive in mAlive)
    //    {
    //        foreach (UIfootManItem manitem in mItemList)
    //        {
    //            if (manitem.FollowerId == alive.Entity.Id)
    //            {
    //                manitem.SetNPCHpPercent(alive.HPPercent);
    //            }
    //        }
    //    }
    //}

    public void AddFootManInfo(UINPCfootManMgr.FootmanInfo Info)
    {
        mInfoList.Add(Info);
        return;
    }

    //void AddFootManItem(FootmanInfo info)
    //{
    //    GameObject obj = GameObject.Instantiate(UIfootManItemPrefab) as GameObject;
    //    obj.transform.parent = mGird.transform;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.SetActive(true);

    //    UIfootManItem item = obj.GetComponent<UIfootManItem>();

    //    item.FollowerId = info.Followerid;
    //    item.SetNPCHpPercent(info.Hppercent);
    //    item.npcCmpt = info.mNpCmpt;
    //    item.SetNpcHeadTextre(info.mTexture);

    //    mItemList.Add(item);
    //}

    void AddFootManItem(int _count)
    {
        for (int i = 0; i < _count; i++)
        {
            GameObject obj = GameObject.Instantiate(UIfootManItemPrefab) as GameObject;
            obj.transform.parent = mGird.transform;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            UIfootManItem item = obj.GetComponent<UIfootManItem>();
            item.mIndex = i;
            //lz-2016.09.22 加载的时候先隐藏，等有数据在开启
            obj.SetActive(false);
            mItemList.Add(item);
        }

        mGird.repositionNow = true;
    }

    //void CloseChoseShow(UIfootManItem item)
    //{
    //    foreach (UIfootManItem manitem in mItemList)
    //    {
    //        if (manitem != item)
    //            manitem.CloseChose();
    //    }
    //}
    //void ButtonOnClick(object sender)
    //{
    //    UIfootManItem Item = sender as UIfootManItem;
    //    if (null != Item)
    //    {
    //        CloseChoseShow(Item);
    //        Item.ShowChose();
    //    }
    //}

    void ChoseOnClick(object sender, ENpcBattle type)
    {
        UIfootManItem Item = sender as UIfootManItem;
        if (null != Item)
        {
            //Item.ChangeBattle(type);
        }
    }


    void Clear()
    {
        foreach (UIfootManItem item in mItemList)
        {
            if (item != null)
            {
                GameObject.Destroy(item.gameObject);
                item.gameObject.transform.parent = null;
            }
        }
        mItemList.Clear();
    }

    //void ReflashItem()
    //{
    //    // Clear();
    //    foreach (FootmanInfo info in mInfoList)
    //    {
    //        //AddFootManItem(info);

    //    }
    //    mGird.repositionNow = true;
    //}

}

public class ServantDeadLabel : PeMap.ILabel
{
    int icon;
    Vector3 pos;
    string text;
    public int servantId;

    public ServantDeadLabel(int _icon, Vector3 _pos, string _text, int _id)
    {
        this.icon = _icon;
        this.pos = _pos;
        this.text = _text;
        this.servantId = _id;
    }

    int PeMap.ILabel.GetIcon()
    {
        return icon;
    }

    Vector3 PeMap.ILabel.GetPos()
    {
        return pos;
    }

    string PeMap.ILabel.GetText()
    {
        return text;
    }

    bool PeMap.ILabel.FastTravel()
    {
        return false;
    }

    PeMap.ELabelType PeMap.ILabel.GetType()
    {
        return PeMap.ELabelType.Npc;
    }

    bool PeMap.ILabel.NeedArrow()
    {
        return false;
    }

    float PeMap.ILabel.GetRadius()
    {
        return -1f;
    }

    PeMap.EShow PeMap.ILabel.GetShow()
    {
        return PeMap.EShow.All;
    }

    #region lz-2016.06.02 

    public bool CompareTo(PeMap.ILabel label)
    {
        if (label is ServantDeadLabel)
        {
            ServantDeadLabel servantDeadlabel = (ServantDeadLabel)label;
            if (this.servantId == servantDeadlabel.servantId)
                return true;
            return false;
        }
        return false;
    }
    #endregion
}
