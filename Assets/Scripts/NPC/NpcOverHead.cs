using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using System;

public enum NpcMissionState
{
    CanGet = 0,
    HasGet,
    CanSubmit,
    MainCanGet,
    MainHasGet,
    MainCanSubmit,
    Max
}

public class HeadInfoMgr
{
    public class NameInfo
    {
        public bool colored;
        public string text;
        public string coloredText;
        public string iconName;

        public string Text
        {
            get
            {
                if (colored)
                {
                    return coloredText;
                }
                else
                {
                    return text;
                }
            }
        }

        public string Icon
        {
            get
            {
                return iconName;
            }
        }
    }

    public class HeadInfo
    {
        public Transform targetTran;
        public NameInfo nameInfo;
    }

    static HeadInfoMgr instance;
    public static HeadInfoMgr Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new HeadInfoMgr();
            }

            return instance;
        }
    }

    List<HeadInfo> mList = new List<HeadInfo>(10);

    HeadInfo FindHeadInfo(Transform trans)
    {
        return mList.Find(delegate(HeadInfo info)
        {

            if (object.ReferenceEquals(info.targetTran, trans))
            {
                return true;
            }

            return false;
        });
    }

    HeadInfo GetHeadInfo(Transform trans)
    {
        HeadInfo headInfo = FindHeadInfo(trans);

        if (null == headInfo)
        {
            headInfo = new HeadInfo();
            headInfo.targetTran = trans;

            headInfo.nameInfo = new NameInfo();
        }

        return headInfo;
    }

    public IEnumerable<HeadInfo> Infos
    {
        get
        {
            return mList;
        }
    }

    public void Remove(Transform trans)
    {
        mList.RemoveAll(delegate(HeadInfo info)
        {
            if (object.ReferenceEquals(info.targetTran, trans))
            {
                return true;
            }

            return false;
        });
    }

    public void SetText(Transform trans, string text)
    {
        HeadInfo headInfo = GetHeadInfo(trans);
        headInfo.nameInfo.text = text;
        headInfo.nameInfo.coloredText = "[ffa7ff]" + text + "[-]";
    }

    public void SetColor(Transform trans, bool flag)
    {
        HeadInfo headInfo = GetHeadInfo(trans);
        headInfo.nameInfo.colored = flag;
    }

    public void SetIcon(Transform trans, string icon)
    {
        HeadInfo headInfo = GetHeadInfo(trans);
        headInfo.nameInfo.iconName = icon;
    }
}

public class NpcOverHead : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer missionMark = null;
    [SerializeField]
    private MeshRenderer revivalMark = null;
    [SerializeField]
    UILabel mLable;
    [SerializeField]
    UISprite mIcon;
    [SerializeField]
    Transform NameLb;
    [SerializeField]
    Transform PlayerTrans;
    [SerializeField]
    Transform NpcTrans;
    [SerializeField]
    Transform MonsterTrans;
    [SerializeField]
    Transform SpeakTrans;
    [SerializeField]
    NpcSpeakSentenseItem mSpeakSentensePrefab;
    [SerializeField]
    UISlider mBloodPlayer;
    [SerializeField]
    Bloodcmpt mPlayerBloodcmpt;
    [SerializeField]
    UISlider mBloodMonster;
    [SerializeField]
    Bloodcmpt mMonsterBloodcmpt;
    [SerializeField]
    UISlider mBloodNpc;
    [SerializeField]
    Bloodcmpt mNpcBloodcmpt;

    public float minDis = 0.4f;
    public float maxDis = 12f;
    public float heightAdjust = 0.4f;
    public float scaleAdjust = 8f;
    public Pathea.PeEntity EntityOver { get { return m_Entity; } set { m_Entity = value; } }
    //lz-2016.10.13 环绕血条,处理特殊怪物(不会和模型穿插)
    [SerializeField]
    private bool m_CircleHpBar =false;
    //lz-2016.10.13 低血条,处理特殊怪物（低有可能会）
    [SerializeField]
    private bool m_LowHpBar = false;
    [SerializeField]
    private float m_LowHpBarFactor = 0.2f;


    //lz-2016.10.13 需要环绕血条的怪物ID
    private List<int> m_NeedCircleHpBarMonsterIDs = new List<int> { 95 };
    //lz-2016.10.13 需要低血条的怪物ID
    private List<int> m_NeedLowHpBarMonsterIDs = new List<int> { 96 };


    NpcMissionState missionState = NpcMissionState.Max;
    float mTimer = 0f;
    //Pathea.EEntityProto m_EntityProto;
    Bounds m_localBounds;
    bool InitBloodPos = false;
    Pathea.PeEntity m_Entity;
    #region Blood
    SkAliveEntity m_SkAlive;
    Transform mCurBloodTrans;
    UISlider mCurBloodSlider;
    EEntityProto mCurEEntityProto = EEntityProto.Max;
    PeEntity m_entity;
    float HPchangeTime;
    float BLOOD_MAX_TIME = 20.0f;
    bool mInitBlood = false;
    bool mDamge = false;
    bool mInitNameLb = false;
    bool mHideMainPlayerInfo = false;

    #endregion

    #region mono methods
    void Awake()
    {
        //lz-2016.06.07 太浪费了，反正是预制物体，为什么不在预制物体上把要用到的组件关联好，而要实例一个NpcOverHead都通过GetComponent和FindChild给对象赋值？
        //InitOverHead();
    }

    void Start()
    {
        //InitOverHead();
        //StartCoroutine(Hpcmpt());
        SetRevivalMark(false, 0f);
        StartCoroutine(UpdateBlood());
        CheckMainPlayerInfo();
    }

    void Update()
    {
        //lz-2016.06.14 npc是任务跟随着或者仆从就不显示任务图标
        if (null != m_Entity && null != m_Entity.NpcCmpt)
        {
            if (m_entity.NpcCmpt.IsFollower)
            {
                missionMark.gameObject.SetActive(false);
            }
            else
            {
                SetState(missionState);
            }
        }

        //lz-2016.09.26 检查多人模式是否隐藏玩家个人的信息
        CheckMainPlayerInfo();
    }

    void LateUpdate()
    {
        // Sethealth();
        mTimer += Time.deltaTime;
        if (mTimer >= 0.25f)
        {
            mTimer = 0f;
            UpdateSpeakPos();
        }
        UpdateTransform();
    }

    #endregion


    #region public methods

    public void UpdateTransform()
    {
        Transform camTrans = PETools.PEUtil.MainCamTransform;
        if (camTrans != null && null != m_Entity && m_Entity.peTrans != null)
        {
            // pos
            Vector3 pos = Vector3.zero;
            float camDis = 0f;
            if (m_CircleHpBar )
            {
                pos = m_Entity.peTrans.center;
                camDis = Vector3.Distance(camTrans.position, pos);
                camDis = Mathf.Clamp(camDis, minDis, maxDis);
                float maxLength = Mathf.Max(m_Entity.peTrans.boundExtend.extents.x,m_Entity.peTrans.boundExtend.extents.z);
                Vector3 duration = Vector3.Normalize(camTrans.position - pos);
                pos.z +=(duration.z* maxLength);
                pos.x +=(duration.x * maxLength);
                float offsetY = camDis * heightAdjust;
                pos.y = pos.y +offsetY;
            }
            else
            {
                pos = m_Entity.peTrans.uiHeadTop;
                camDis = Vector3.Distance(camTrans.position, pos);
                camDis = Mathf.Clamp(camDis, minDis, maxDis);
                float offsetY = camDis * heightAdjust;
                pos.y = pos.y + offsetY;
            }
            transform.position = pos;

            // scale
            float scale = camDis / scaleAdjust;
            transform.localScale = new Vector3(scale, scale, 0);

            // rotation
            transform.rotation = camTrans.rotation;

            // bounds
            SetBouds(m_Entity.peTrans.bound);
        }
    }

    public void SetNameShow(bool showOrnot)
    {
        NameLbShow(showOrnot);

    }

    public bool SetProto(Pathea.EEntityProto Proto)
    {
        //        if (Proto == null)
        //            return false;	// always false
        //m_EntityProto = Proto;
        return true;

    }

    public void SetTheEntity(Pathea.PeEntity entity)
    {
        m_Entity = entity;
        //lz-2016.10.13 设置是否需要环绕血条
        m_CircleHpBar= m_NeedCircleHpBarMonsterIDs.Contains(m_Entity.ProtoID);
        //lz-2016.10.13 设置是否需要低血条
        m_LowHpBar = m_NeedLowHpBarMonsterIDs.Contains(m_Entity.ProtoID);
    }

    public void SetBouds(Bounds localBounds)
    {
        m_localBounds = localBounds;
        UpdateLocalPostion();
    }

    public void ShowMissionMark(bool show)
    {
        missionMark.gameObject.SetActive(show);
    }

    #region delete it

    public void WillBeDestroyed() { }

    public bool Visiable
    {
        get
        {
            return gameObject.activeInHierarchy;
        }
        set
        {
            gameObject.SetActive(value);
        }
    }

    #endregion

    public void SetState(NpcMissionState state)
    {
        missionState = state;
        SetStateMark(state);
    }

    public void SetRevivalMark(Pathea.EEntityProto Proto, bool show, float percent)
    {
        if (null == revivalMark)
        {
            Debug.LogWarning(this + "revivalMark is null");
            return;
        }

        if (Proto != Pathea.EEntityProto.Npc)
            return;

        if (!show)
        {
            revivalMark.gameObject.SetActive(false);
            SetStateMark(missionState);
            return;
        }

        revivalMark.gameObject.SetActive(true);
        revivalMark.material.SetFloat("_Percent", percent);
        SetStateMark(NpcMissionState.Max);
    }

    public void SetRevivalMark(bool show, float percent)
    {
        if (null == revivalMark)
        {
            Debug.LogWarning(this + "revivalMark is null");
            return;
        }

        if (!show)
        {
            revivalMark.gameObject.SetActive(false);
            SetStateMark(missionState);
            return;
        }

        revivalMark.gameObject.SetActive(true);
        revivalMark.material.SetFloat("_Percent", percent);
        SetStateMark(NpcMissionState.Max);
    }

    public void SetNpcShowName(string npcName, bool colored = false)
    {
        HeadInfoMgr.Instance.SetText(transform, npcName);

        //	NpcName.gameObject.transform.position =new Vector3(0,0.2f,0);
        //NpcName.text=npcName;
        if (mLable == null)
            return;

        mLable.text = npcName;

        return;
    }

    public void SetNameColord(bool colored)
    {
        HeadInfoMgr.Instance.SetColor(transform, colored);
    }

    public void SetNpcShopIcon(string shopIcon)
    {
        HeadInfoMgr.Instance.SetIcon(transform, shopIcon);
    }

    public void SetShowIcon(string Icon)
    {
        switch (Icon)
        {
            case "shop_wuqi":
            case "shop_fangju":
            case "shop_buji":
            case "shop_jiaju":
            case "shop_zahuo":
                //lz-2016.09.05 让图标紧挨着名字左边，避免离得太远
                mIcon.spriteName = Icon;
                mIcon.MakePixelPerfect();
                Vector3 labelTrans = mLable.transform.localPosition;
                labelTrans.x = labelTrans.x - (mLable.relativeSize.x * mLable.font.size + mIcon.transform.localScale.x) * 0.5f - 5f;
                mIcon.transform.localPosition = labelTrans;
                break;
            default:
                mIcon.spriteName = "null";
                break;
        }
    }

    public void Reset()
    {
        SetState(NpcMissionState.Max);
        SetRevivalMark(false, 0f);
        HeadInfoMgr.Instance.Remove(transform);
    }

    public void NameLbShow(bool show)
    {
        mInitNameLb = true;
        NameLb.gameObject.SetActive(show);
    }

    public void BloodShow(bool show)
    {
        if (mCurBloodTrans == null)
            return;

        mInitBlood = true;
        mCurBloodTrans.gameObject.SetActive((show&&!mHideMainPlayerInfo));
    }

    public void HpChange(SkEntity caster, float hpChange)
    {
        if (PeGameMgr.IsSingle && CurEEntityProto == EEntityProto.Player)
            return;


        SetBloodPercent(HpPercent, hpChange);
        mDamge = true;
        HPchangeTime = Time.time;
    }

    public bool SetBloodPercent(float percent,float hpchange)
    {
        if (mCurBloodSlider == null)
            return false;

        if (null != mCurBloodTrans)
        {
            if (percent > 0)
            {
                if (hpchange <0)
                    BloodShow(true);
            }
            else
            {
                if(m_entity.entityProto.proto != EEntityProto.Player)
                    BloodShow(false);
            }
        }
       
        mCurBloodSlider.sliderValue = percent;
        return true;
    }

    public void InitTheentity(PeEntity entity)
    {
        m_entity = entity;
    }
    public EEntityProto CurEEntityProto
    {
        set
        {
            mCurEEntityProto = value;
            SwichBloodByProto(mCurEEntityProto);
        }
        get { return mCurEEntityProto; }
    }

    public SkAliveEntity SkAlive
    {
        get
        {
            if (m_SkAlive == null)
            {
                if (m_entity != null)
                    m_SkAlive = m_entity.GetCmpt<SkAliveEntity>();

                return m_SkAlive;
            }
            return m_SkAlive;
        }
    }

    public float HpPercent
    {
        get { return SkAlive != null ? SkAlive.HPPercent : 1.0f; }
    }

    #endregion

    #region private methods
    private void CheckMainPlayerInfo()
    {
        if (null != m_Entity && PeGameMgr.IsMulti)
        {
            if (m_Entity.IsMainPlayer)
            {
                //lz-2016.10.08 第一人称的时候隐藏玩家头顶的信息【错误 #3739】
                if (SystemSettingData.Instance.FirstPersonCtrl)
                {
                    mHideMainPlayerInfo = true;
                    UpdateMainPlayerInfoState();
                }
                else if (SystemSettingData.Instance.HidePlayerOverHeadInfo != mHideMainPlayerInfo)
                {
                    mHideMainPlayerInfo = SystemSettingData.Instance.HidePlayerOverHeadInfo;
                    UpdateMainPlayerInfoState();
                }
            }
        }
    }
    private void UpdateMainPlayerInfoState()
    {
        SetNameShow(!mHideMainPlayerInfo);
        mCurBloodTrans.gameObject.SetActive(!mHideMainPlayerInfo);
    }
    void DestroyUI()
    {
		if(PlayerTrans!= null)	PlayerTrans.gameObject.SetActive(false);
		if(NpcTrans!= null)	NpcTrans.gameObject.SetActive(false);
		if(MonsterTrans!= null)	MonsterTrans.gameObject.SetActive(false);
    }

    void Destroy()
    {
        HeadInfoMgr.Instance.Remove(transform);
    }

    void OnDestroy()
    {
        DestroyUI();
    }

    void InitOverHead()
    {
        Transform trans = transform.FindChild("MissionMark");
        if (null == trans)
        {
            Debug.LogWarning("no text mesh to show mission mark");
            return;
        }

        missionMark = trans.GetComponent<MeshRenderer>();


        //***********************************************
        trans = transform.FindChild("Revival");
        if (null == trans)
        {
            Debug.LogWarning("no Revival found");
            return;
        }

        revivalMark = trans.GetComponent<MeshRenderer>();
        SetRevivalMark(false, 0f);
         

        trans = transform.FindChild("Namlb");
        if (null == trans)
        {
            Debug.LogWarning("no text show Namlb");
            return;
        }
        NameLb = trans;

        trans = trans.transform.FindChild("Label");
        if (null == trans)
        {
            Debug.LogWarning("no text show Namlb");
            return;
        }
        mLable = trans.GetComponent<UILabel>() as UILabel;

        trans = NameLb.transform.FindChild("Icon");
        if (null == trans)
        {
            Debug.LogWarning("no text show Icon");
            return;
        }
        mIcon = trans.GetComponent<UISprite>() as UISprite;

        trans = transform.FindChild("Player");
        PlayerTrans = trans;
        if (PlayerTrans != null)
            trans = PlayerTrans.FindChild("BloodItemPlayer");
        if (trans != null)
        {
            mBloodPlayer = trans.GetComponent<UISlider>() as UISlider;
            mPlayerBloodcmpt = trans.GetComponent<Bloodcmpt>();
        }

        trans = transform.FindChild("NPc");
        NpcTrans = trans;
        if (NpcTrans != null)
            trans = NpcTrans.FindChild("BloodItemNpc");
        if (trans != null)
        {
            mBloodNpc = trans.GetComponent<UISlider>() as UISlider;
            mNpcBloodcmpt = trans.GetComponent<Bloodcmpt>();
        }

        trans = transform.FindChild("Monster");
        MonsterTrans = trans;
        if (MonsterTrans != null)
            trans = MonsterTrans.FindChild("BloodItemMon");
        if (trans != null)
        {
            mBloodMonster = trans.GetComponent<UISlider>() as UISlider;
            mMonsterBloodcmpt = trans.GetComponent<Bloodcmpt>();
        }


        trans = transform.FindChild("Speak");
        SpeakTrans = trans;
        if (SpeakTrans != null)
            mSpeakSentensePrefab = SpeakTrans.FindChild("SentencePrefab").GetComponent<NpcSpeakSentenseItem>();

        return;
    }

    void setLocalScale(Vector2 Stepsize)
    {
        mBloodPlayer.fullSize = Stepsize;
        mPlayerBloodcmpt.setBackScale(Stepsize);

        mBloodNpc.fullSize = Stepsize;
        mNpcBloodcmpt.setBackScale(Stepsize);

        mBloodMonster.fullSize = Stepsize;
        mMonsterBloodcmpt.setBackScale(Stepsize);
    }

    void setPostion()
    {
        Vector3 pos = new Vector3(-mBloodPlayer.fullSize.x / 200.0f, -0.2f, 0);

        float up = 1.0f;
        if (mBloodPlayer.fullSize.x >= 1900.0f)
            up = 6.2f;

        mBloodPlayer.transform.localPosition = pos;
        float offsetY = 0f;
        if (m_LowHpBar)
        {
            //lz-2016.10.12 低血条
            offsetY = pos.y + (up* m_LowHpBarFactor);
        }
        else if (m_CircleHpBar)
        {
            //lz-2016.10.12 环绕血条
            offsetY = pos.y;
        }
        else
        {
            offsetY = pos.y + up;
        }
        mBloodMonster.transform.localPosition = new Vector3(pos.x, offsetY, pos.z); 
        mBloodNpc.transform.localPosition = pos;
    }

    Vector2 calculateSteps(Vector3 size)
    {
        //if(size.x >= )
        float x = size.x;
        //if (size.x >= 5.0f)
        //    x = 5.0f;

        //Vector2 step = new Vector2(Mathf.Clamp(x * 80.0f, 80, 300), Mathf.Clamp((x * 80.0f) / 20.0f, 4, 15));

        //Vector2 step = new Vector2(x * 80.0f, x * 80.0f) / 20.0f);
        Vector2 step = Vector3.zero;
        step.x = Mathf.Round(Mathf.Clamp(x * 20f, 80, 300));
        step.y = Mathf.Round(step.x / 20);
        return step;
    }

    void UpdateLocalPostion()
    {
        if (/*m_localBounds != null && */m_localBounds.size != Vector3.zero&& !InitBloodPos)
        {
            setLocalScale(calculateSteps(m_localBounds.size));
            setPostion();
            InitBloodPos = true;
        }
        //m_localBounds.size.y;
    }

    #region  Speaking

    List<NpcSpeakSentenseItem> mSpeakSentenseList = new List<NpcSpeakSentenseItem>();

    public void SayOneWord(string _content, float _interval)
    {
        if (mSpeakSentenseList.Count > 0)//提前结束上一句话
        {
            mSpeakSentenseList[0].AheadDisappear();
        }

        NpcSpeakSentenseItem item = CreatOneSentense(mSpeakSentensePrefab, SpeakTrans);
        mSpeakSentenseList.Add(item);
        item.OnDestroySelfEvent += OnDestroySentenseSelf;
        item.SayOneWord(_content, _interval);

        UpdateSentensePos();
    }

    NpcSpeakSentenseItem CreatOneSentense(NpcSpeakSentenseItem _prefab, Transform _parent)
    {
        NpcSpeakSentenseItem item = Instantiate(_prefab) as NpcSpeakSentenseItem;
        item.gameObject.SetActive(true);
        item.transform.parent = _parent;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        return item;
    }

    void OnDestroySentenseSelf(NpcSpeakSentenseItem _item)
    {
        mSpeakSentenseList.Remove(_item);
    }

    void UpdateSentensePos()
    {
        if (mSpeakSentenseList.Count <= 1)
            return;
        for (int i = mSpeakSentenseList.Count - 2; i >= 0; i--)
        {
            Vector3 oldPos;
            oldPos = mSpeakSentenseList[i].transform.localPosition;
            Vector3 newPos = new Vector3(oldPos.x, oldPos.y + 35f, oldPos.z);
            mSpeakSentenseList[i].transform.localPosition = newPos;
        }
    }

    void UpdateSpeakPos()
    {
        if (missionMark.gameObject.activeInHierarchy)
            SpeakTrans.localPosition = new Vector3(SpeakTrans.localPosition.x, 1.25f, SpeakTrans.localPosition.z);
        else if (revivalMark.gameObject.activeInHierarchy)
            SpeakTrans.localPosition = new Vector3(SpeakTrans.localPosition.x, 0.8f, SpeakTrans.localPosition.z);
        else
            SpeakTrans.localPosition = new Vector3(SpeakTrans.localPosition.x, 0.35f, SpeakTrans.localPosition.z);
    }

    #endregion

    Texture GetMissionMark(NpcMissionState mark)
    {
        switch (mark)
        {
            case NpcMissionState.CanGet:
                return Resources.Load("Texture2d/BillBoard/map_an_3") as Texture;
            case NpcMissionState.HasGet:
                return Resources.Load("Texture2d/BillBoard/map_an_1") as Texture;
            case NpcMissionState.CanSubmit:
                return Resources.Load("Texture2d/BillBoard/map_an_2") as Texture;
            case NpcMissionState.MainCanGet:
                return Resources.Load("Texture2d/BillBoard/map_an_3_1") as Texture;
            case NpcMissionState.MainHasGet:
                return Resources.Load("Texture2d/BillBoard/map_an_1_1") as Texture;
            case NpcMissionState.MainCanSubmit:
                return Resources.Load("Texture2d/BillBoard/map_an_2_1") as Texture;
            default:
                return null;
        }
    }

    void SetStateMark(NpcMissionState state)
    {
        if (null == missionMark)
        {
            Debug.LogWarning(this + "missionMark is null");
            return;
        }

        if (NpcMissionState.Max == state)
        {
            missionMark.gameObject.SetActive(false);
            return;
        }
        revivalMark.gameObject.SetActive(false);
        missionMark.gameObject.SetActive(true);
        missionMark.material.mainTexture = GetMissionMark(state);
    }

    void SwichBloodByProto(EEntityProto proto)
    {
        if (mCurBloodTrans != null)
            return;

        switch (proto)
        {
            case EEntityProto.Player:
                {
                    if (PeGameMgr.IsMulti&& null!=m_Entity)
                    {
                        if (null == m_Entity.netCmpt|| (null!=PeCreature.Instance&&m_Entity == PeCreature.Instance.mainPlayer))
                        {
                            //lz-2016.11.07 玩家自己显示红色血条
                            mCurBloodTrans = PlayerTrans;
                            mCurBloodSlider = mBloodPlayer;
                        }
                        else
                        {
                            //lz-2016.11.07 队友显示蓝色血条
                            if (null!=m_Entity.netCmpt.network
                                && null!=PeCreature.Instance&& null!=PeCreature.Instance.mainPlayer&& null!=PeCreature.Instance.mainPlayer.netCmpt&& null!=PeCreature.Instance.mainPlayer.netCmpt.network
                                && m_Entity.netCmpt.network.TeamId == PeCreature.Instance.mainPlayer.netCmpt.network.TeamId)
                            {
                                mCurBloodTrans = NpcTrans;
                                mCurBloodSlider = mBloodNpc;
                            }
                            else
                            {
                                //lz-2016.11.07 敌人显示红色血条
                                mCurBloodTrans = PlayerTrans;
                                mCurBloodSlider = mBloodPlayer;
                            }
                        }
                    }
                    else
                    {
                        //lz-2016.11.07 单人红色血条
                        mCurBloodTrans = PlayerTrans;
                        mCurBloodSlider = mBloodPlayer;
                    }
                    
                }
                break;

            //	case EEntityProto.RandomNpc:
            case EEntityProto.Npc:
                {
                    mCurBloodTrans = NpcTrans;
                    mCurBloodSlider = mBloodNpc;
                }
                break;

            case EEntityProto.Monster:
            case EEntityProto.Doodad:
                {
                    mCurBloodTrans = MonsterTrans;
                    mCurBloodSlider = mBloodMonster;
                }
                break;
            default:
                {
                    mCurBloodTrans = null;
                    mCurBloodSlider = null;
                }
                break;
        }
    }

    void InitPlyerBlood()
    {
        if (mCurBloodTrans == null || mInitBlood)
            return;

        if (PeGameMgr.IsSingle)
        {
            BloodShow(false);
        }
        else
        {
            BloodShow(true);
        }


    }

    void InitPlayerNameLb()
    {
        if (mCurBloodTrans == null || mInitNameLb)
            return;

        if (PeGameMgr.IsSingle)
            NameLbShow(false);
        else
            NameLbShow(true);
    }

    void InitOtherBlood()
    {
        if (mCurBloodTrans == null || mInitBlood)
            return;

        //		if(Time.time - HPchangeTime >= 30.0f && CanShow)
        //		{
        //			CanShow = false;
        //			HPchangeTime = Time.time;
        //		}
        BloodShow(false);
    }

    void InitBlood(EEntityProto proto)
    {
        if (mInitBlood)
            return;

        switch (proto)
        {
            case EEntityProto.Player:
                InitPlyerBlood();
                break;

            case EEntityProto.Npc:
            case EEntityProto.Monster:
            case EEntityProto.Doodad:
                InitOtherBlood();
                break;

            default:
                BloodShow(false);
                break;

        }
    }

    void TimerCout(EEntityProto proto, float startTime)
    {
        if (proto == EEntityProto.Player || mCurBloodTrans == null || !mDamge)
            return;

        if (Time.time - startTime >= BLOOD_MAX_TIME && mDamge)
        {
            BloodShow(false);
            mDamge = false;
        }
        return;
    }

    void InitNameLb(EEntityProto proto)
    {
        if (mInitNameLb)
            return;

        switch (proto)
        {
            case EEntityProto.Player:
                InitPlayerNameLb();
                if (PeGameMgr.IsMulti)
                    NameLbShow(true);
                break;

            case EEntityProto.Npc:
            case EEntityProto.RandomNpc:
                NameLbShow(true);
                break;
            case EEntityProto.Monster:
            case EEntityProto.Doodad:
                NameLbShow(false);
                break;

            default:
                NameLbShow(false);
                break;

        }
    }

    IEnumerator UpdateBlood()
    {
        while (true)
        {
            //Sethealth();
            //	if(mCurBloodTrans != null && (Time.time - HPchangeTime) >= 10.0f)
            //mCurBloodTrans.gameObject.SetActive(false);
            SwichBloodByProto(mCurEEntityProto);
            InitBlood(mCurEEntityProto);
            InitNameLb(mCurEEntityProto);
            TimerCout(mCurEEntityProto, HPchangeTime);
            yield return new WaitForSeconds(1.0f);
        }

    }
    #endregion
}
