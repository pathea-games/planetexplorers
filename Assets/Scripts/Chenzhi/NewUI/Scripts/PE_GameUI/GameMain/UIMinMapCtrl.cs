using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using System.IO;
using Pathea;
using PatheaScript;
using PeMap;
using WhiteCat;

public class UIMinMapCtrl : UIStaticWnd
{
    static UIMinMapCtrl mInstance;
    public static UIMinMapCtrl Instance { get { return mInstance; } }
    public UILabel mTimeLabel;
    public UISprite mWeatherSpr;
    public UIPanel mSubInfoPanel;
    public GameObject mMapLabelPrefab;
    public GameObject mArrowLablePrefab;

    public Camera mMiniMapCam;
    public UISprite mMapPlayerSpr;

    public UITexture mMiniMapTex;
    public Material mMinMapMaterial;
    public UILabel mPlayerPosText;
    public BoxCollider mMapCollider;

    public UISlicedSprite mSpCenterBg;
    public UISlicedSprite mSpLeftBg;
    public UISlicedSprite mSpRightBg;

    public UISlicedSprite mSpLeftBg_1;
    public UISlicedSprite mSpRightBg_1;

    public UITexture mTexCenterBg;
    public UITexture mTexLeftBg;
    public UITexture mTexRightBg;

    public GameObject mWnd;
    public GameObject mRTopBtns;
    public GameObject mRBottomBtns;
    public GameObject mChangeSize;
    public UISprite mTuodong;
    public Vector2 mMinSize;
    public Vector2 mMaxSize;

    [HideInInspector]
    public Vector2 mMapScale;

    public Vector2 mMapSize;
    public float mMapAlpha;
    public float mMapBright;
    //lz-2016.09.06 怪物攻城效果
    [SerializeField]
    protected MaplabelMonsterSiegeEffect m_SiegeEffectPrefab;
    [SerializeField]
    private TweenInterpolator m_MinMapTweenInterpolator;
    [SerializeField]
    private WhiteCat.TweenPosition m_MinMapPosTween;
    [SerializeField]
    private UIButton m_MinMapHideBtn;

    bool mShowBigMap;
    //float mUpdateSubInfoTime = 0.9f;
    //float mUpdateSubInfoElapseTime = 1;

    //bool mShowMissionTrack;
    Vector3 mMapCenterPos = Vector3.zero;
    double mMapReFlashTime = 0;
    bool mIsOnDrapSize = false;

    Pathea.PeTrans mView;
    List<UIMapLabel> m_CurrentMapLabelList = new List<UIMapLabel>();
    protected Queue<UIMapLabel> m_MapLabelPool = new Queue<UIMapLabel>(); //Log:lz-2016.04.18 这个UIPool用来优化MapLabel,避免反复实例和销毁
    List<UIMapArrow> mMapArrowList = new List<UIMapArrow>();
    private bool m_MinMapIsHide;


    #region mono methods

    void Awake()
    {
        mInstance = this;
        InitWindow();
    }

    void Start()
    {
        if (UIRecentDataMgr.Instance != null)
        {
            int x = UIRecentDataMgr.Instance.GetIntValue("MinMapSize_x", (int)mMapSize.x);
            int y = UIRecentDataMgr.Instance.GetIntValue("MinMapSize_y", (int)mMapSize.y);
            ReSetMapSize(new Vector2(x, y));
            mMapAlpha = UIRecentDataMgr.Instance.GetFloatValue("MinMapAlpha", mMapAlpha);
            mMapBright = UIRecentDataMgr.Instance.GetFloatValue("MinMapBright", mMapBright);
        }

        if (LabelMgr.Instance != null)
        {
            GetAllMapLabel();
            LabelMgr.Instance.eventor.Subscribe(AddOrRemoveLable);
        }
    }

    void Update()
    {
        if (mIsOnDrapSize)
            OnDrapSize();

        if (mMiniMapCam == null)
            return;

        if (!mMiniMapCam.gameObject.activeSelf)
            mMiniMapCam.gameObject.SetActive(true);

        UpdateCurAllLabelShow();
        UpdateMiniMap();
        UpdateTime();
        UpdateSubInfo();
    }

    void OnDestroy()
    {
        Material.Destroy(mMiniMapTex.material);
        UIEventListener.Get(m_MinMapHideBtn.gameObject).onClick -= HideMapBtnOnClick;
        m_MinMapTweenInterpolator.onArriveAtBeginning.RemoveListener(OnMinMapTweenFinish);
        m_MinMapTweenInterpolator.onArriveAtEnding.RemoveListener(OnMinMapTweenFinish);
    }

    #endregion

    #region pretected  methods
    protected override void InitWindow()
    {
        base.InitWindow();

        UIEventListener.Get(mMapCollider.gameObject).onClick += (go) => this.OnOpenWorldMap();

        if (mMiniMapCam == null)
        {
            GameObject mapCam = GameObject.Find("MinmapCamera");
            if (mapCam == null)
                return;
            mMiniMapCam = mapCam.GetComponent<Camera>();
            mMiniMapCam.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
        }
        RenderTexture mTex = new RenderTexture(Mathf.CeilToInt(256 * mMiniMapCam.aspect), 256, 1);
        mTex.isCubemap = false;
        mMiniMapCam.cullingMask |= 1 << Pathea.Layer.SceneStatic;
        mMiniMapCam.targetTexture = mTex;
        mMiniMapCam.orthographicSize = 128;
        mMiniMapCam.gameObject.SetActive(false);
        mMiniMapCam.aspect = 1;
        mMiniMapTex.material = Material.Instantiate(mMinMapMaterial) as Material;
        mMiniMapTex.material.SetTexture("_MainTex", mTex);
        //mShowMissionTrack = false;
        mMapScale = Vector2.one;

        UIEventListener.Get(m_MinMapHideBtn.gameObject).onClick += HideMapBtnOnClick;
        m_MinMapTweenInterpolator.onArriveAtBeginning.AddListener(OnMinMapTweenFinish);
        m_MinMapTweenInterpolator.onArriveAtEnding.AddListener(OnMinMapTweenFinish);
        m_MinMapIsHide = false;
    }
    #endregion

    #region private methods

    void HideMapBtnOnClick(GameObject go)
    {
        m_MinMapIsHide = !m_MinMapIsHide;
        m_MinMapHideBtn.isEnabled = false;
        m_MinMapTweenInterpolator.speed = m_MinMapIsHide ? 1 : -1;
        m_MinMapTweenInterpolator.isPlaying = true;
        
    }

    void OnMinMapTweenFinish()
    {
        m_MinMapHideBtn.transform.localRotation = Quaternion.Euler(m_MinMapIsHide ? new Vector3(0, 0, 180) : Vector3.zero);
        m_MinMapHideBtn.isEnabled = true;
    }

    void UpdateTweenInfo()
    {
        Vector3 curPos= m_MinMapHideBtn.transform.localPosition;
        curPos.y = -mSpRightBg.transform.localScale.y -15;
        m_MinMapHideBtn.transform.localPosition = curPos;
        m_MinMapPosTween.from = Vector3.one;
        float xOffset = GetMinMapWidth() + 5;
        m_MinMapPosTween.to = new Vector3(xOffset, 0,0);
    }

    Dictionary<int, List<MissionLabel>> npc_receiveSubmit = new Dictionary<int, List<MissionLabel>>();  //任务标示在小地图上的显示优先级
    enum ReceiveSubmit
    {
        nothing = 0,
        receive = 1,
        submit = 2,
        receiveSubmit = 3,
    }

    ReceiveSubmit CheckNpcMissionLabel(List<MissionLabel> tmp)
    {
        bool receive = false;
        bool submit = false;
        foreach (var item in tmp)
        {
            if (item.m_type == MissionLabelType.misLb_unActive)
                receive = true;
            else if (item.m_type == MissionLabelType.misLb_end)
                submit = true;
        }
        if (receive && submit)
            return ReceiveSubmit.receiveSubmit;
        else if (receive)
            return ReceiveSubmit.receive;
        else if (submit)
            return ReceiveSubmit.submit;
        else
            return ReceiveSubmit.nothing;
    }

    void MinMapMissionLabelRealation(MissionLabel tmp,bool add)             //小地图显示任务标示的优先级关系
    {
        if (add)
        {
            int npcId = tmp.m_attachOnID;
            if (tmp.m_type == MissionLabelType.misLb_target)
            {
                AddMapLabel(tmp);
                AddArrowLabel(tmp);
                return;
            }
            if (tmp.m_type == MissionLabelType.misLb_unActive)
            {
                if (!npc_receiveSubmit.ContainsKey(npcId))
                {
                    npc_receiveSubmit.Add(npcId, new List<MissionLabel>());
                    npc_receiveSubmit[npcId].Add(tmp);
                    AddMapLabel(tmp);
                    AddArrowLabel(tmp);
                }
                else
                {
                    npc_receiveSubmit[npcId].Add(tmp);
                    if (CheckNpcMissionLabel(npc_receiveSubmit[npcId]) == ReceiveSubmit.receive)
                    {
                        AddMapLabel(tmp);
                        AddArrowLabel(tmp);
                    }
                }
            }
            else if (tmp.m_type == MissionLabelType.misLb_end)
            {
                if (!npc_receiveSubmit.ContainsKey(npcId))
                {
                    npc_receiveSubmit.Add(npcId, new List<MissionLabel>());
                    npc_receiveSubmit[npcId].Add(tmp);
                    AddMapLabel(tmp);
                    AddArrowLabel(tmp);
                }
                else
                {
                    if (CheckNpcMissionLabel(npc_receiveSubmit[npcId]) == ReceiveSubmit.receive)
                    {
                        foreach (var item in npc_receiveSubmit[npcId])
                        {
                            if (item.m_type == MissionLabelType.misLb_unActive) 
                            {
                                RemvoeMapLabel(item);
                                RemvoArrowLabel(item);
                            }
                        }
                    }
                    npc_receiveSubmit[npcId].Add(tmp);
                    AddMapLabel(tmp);
                    AddArrowLabel(tmp);
                }
            } 
        }
        else
        {
            int npcId = tmp.m_attachOnID;
            if (tmp.m_type == MissionLabelType.misLb_target)
            {
                RemvoeMapLabel(tmp);
                RemvoArrowLabel(tmp);
                return;
            }
            if (tmp.m_type == MissionLabelType.misLb_unActive)
            {
                if (npc_receiveSubmit.ContainsKey(npcId))
                {
                    if (CheckNpcMissionLabel(npc_receiveSubmit[npcId]) == ReceiveSubmit.receive)
                    {
                        RemvoeMapLabel(tmp);
                        RemvoArrowLabel(tmp);
                    }
                    npc_receiveSubmit[npcId].Remove(tmp);
                }
                else
                {
                    RemvoeMapLabel(tmp);
                    RemvoArrowLabel(tmp);
                }
            }
            else if (tmp.m_type == MissionLabelType.misLb_end)
            {
                RemvoeMapLabel(tmp);
                RemvoArrowLabel(tmp);
                if (npc_receiveSubmit.ContainsKey(npcId))
                {
                    npc_receiveSubmit[npcId].Remove(tmp);
                    if (CheckNpcMissionLabel(npc_receiveSubmit[npcId]) == ReceiveSubmit.receive)
                    {
                        foreach (var item in npc_receiveSubmit[npcId])
                        {
                            AddMapLabel(item);
                            AddArrowLabel(item);
                        }
                    }
                }
            }
        }
    }

    void AddOrRemoveLable(object sender, LabelMgr.Args arg)
    {
        if (arg == null || arg.label == null)
            return;
        if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial &&
            arg.label.GetType() == ELabelType.Npc)
        {
            if(((MapCmpt)(arg.label)).Common.entityProto.proto == EEntityProto.Monster)
                return;
        }
        if (arg.add)
        {
            if (arg.label.GetType() == ELabelType.Mission)
            {
                MissionLabel tmp = (arg.label as MissionLabel);
                MinMapMissionLabelRealation(tmp, arg.add);
            }
            else
            {
                AddMapLabel(arg.label);
                AddArrowLabel(arg.label);
            }
        }
        else
        {
            if (arg.label.GetType() == ELabelType.Mission)
            {
                MissionLabel tmp = (arg.label as MissionLabel);
                MinMapMissionLabelRealation(tmp, arg.add);
            }
            else
            {
                RemvoeMapLabel(arg.label);
                RemvoArrowLabel(arg.label);
            }
        }
    }

    void GetAllMapLabel()
    {
        foreach (UIMapLabel uiLabel in m_CurrentMapLabelList)
        {
            if (uiLabel != null)
            {
                TryRemoveMonsterSiegeEffect(uiLabel);
                uiLabel.gameObject.SetActive(false);
                this.m_MapLabelPool.Enqueue(uiLabel);
            }
        }
        this.m_CurrentMapLabelList.Clear();
        LabelMgr.Instance.ForEach(delegate(ILabel label)
        {
                AddMapLabel(label);
        });
    }

    void AddMapLabel(ILabel label)
    {
        if (label.GetShow() == EShow.All || label.GetShow() == EShow.MinMap)
        {
            UIMapLabel uiLabel = null;
            if (this.m_MapLabelPool.Count > 0)
            {
                uiLabel = this.m_MapLabelPool.Dequeue();
                uiLabel.gameObject.SetActive(true);
            }
            else
            {
                GameObject obj = GameObject.Instantiate(mMapLabelPrefab) as GameObject;
                obj.transform.parent = mSubInfoPanel.transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.transform.localRotation = Quaternion.identity;
                uiLabel = obj.GetComponent<UIMapLabel>();
            }
            if (uiLabel != null)
            {
                uiLabel.transform.localScale = Vector3.one;
                uiLabel.SetLabel(label, true);
                uiLabel.gameObject.name = "MinMapLabel: " + uiLabel._ILabel.GetText();
                this.m_CurrentMapLabelList.Add(uiLabel);
            }

            TryAddMonsterSiegeEffect(uiLabel);

            if (label.GetType() == ELabelType.Mission)
            {
                MissionLabel ml = label as MissionLabel;
                if(ml.m_attachOnID != 0)
                    uiLabel.SetLabelPosByNPC(ml.m_attachOnID);
                //missionID = ml.m_missionID;
                //if ((label as MissionLabel).m_target != null)
                //    targetID = ml.m_target.mID;

                //if (Pathea.PeGameMgr.IsStory)
                //{
                //    if (MissionRepository.m_MissionCommonMap.ContainsKey(missionID))
                //    {
                //        if (ml.m_type == MissionLabelType.misLb_target)
                //        {
                //            if (MissionRepository.m_TypeFollow.ContainsKey(targetID))
                //            {
                //                TypeFollowData fol = MissionManager.GetTypeFollowData(targetID);
                //                if (fol.m_LookNameID != 0)
                //                    uiLabel.SetLabelPosByNPC(fol.m_LookNameID);
                //            }
                //            else if (MissionRepository.m_TypeSearch.ContainsKey(targetID))
                //            {
                //                TypeSearchData ser = MissionManager.GetTypeSearchData(targetID);
                //                if (ser.m_NpcID != 0)
                //                    uiLabel.SetLabelPosByNPC(ser.m_NpcID);
                //            }
                //        }
                //        else if (ml.m_type == MissionLabelType.misLb_unActive)
                //        {
                //            uiLabel.SetLabelPosByNPC(MissionRepository.m_MissionCommonMap[missionID].m_iNpc);
                //        }
                //        else if (ml.m_type == MissionLabelType.misLb_end)
                //        {
                //            uiLabel.SetLabelPosByNPC(MissionRepository.m_MissionCommonMap[missionID].m_iReplyNpc);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var item in NpcMissionDataRepository.dicMissionData)
                //        {
                //            if (item.Value.m_RandomMission != missionID)
                //                continue;
                //            if (ml.m_type == MissionLabelType.misLb_unActive)
                //            {
                //                uiLabel.SetLabelPosByNPC(item.Key);
                //            }
                //            else if (ml.m_type == MissionLabelType.misLb_end)
                //            {
                //                uiLabel.SetLabelPosByNPC(item.Key);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (AdRMRepository.m_AdRandMisMap.ContainsKey(missionID))
                //    {
                //        if (ml.m_type == MissionLabelType.misLb_target)
                //        {
                //            if (AdRMRepository.m_AdTypeFollow.ContainsKey(targetID))
                //            {
                //                TypeFollowData fol = MissionManager.GetTypeFollowData(targetID);
                //                if (fol.m_LookNameID != 0)
                //                    uiLabel.SetLabelPosByNPC(fol.m_LookNameID);
                //            }
                //            else if (AdRMRepository.m_AdTypeSearch.ContainsKey(targetID))
                //            {
                //                TypeSearchData ser = MissionManager.GetTypeSearchData(targetID);
                //                if (ser.m_NpcID != 0)
                //                    uiLabel.SetLabelPosByNPC(ser.m_NpcID);
                //            }
                //        }
                //        else if (ml.m_type == MissionLabelType.misLb_unActive)
                //        {
                //            uiLabel.SetLabelPosByNPC(AdRMRepository.m_AdRandMisMap[missionID].m_iNpc);
                //        }
                //        else if (ml.m_type == MissionLabelType.misLb_end)
                //        {
                //            uiLabel.SetLabelPosByNPC(AdRMRepository.m_AdRandMisMap[missionID].m_iReplyNpc);
                //        }
                //    }
                //}
            }
        }
    }

    /// <summary> lz-2016.10.09 尝试添加怪物攻城特效</summary>
    void TryAddMonsterSiegeEffect(UIMapLabel uiLabel)
    {
        ILabel label = uiLabel._ILabel;
        //lz-2016.09.06 如果是怪物攻城，就添加一个攻城效果
        if (null != label && label is MonsterBeaconMark)
        {
            MonsterBeaconMark mark = label as MonsterBeaconMark;
            if (mark.IsMonsterSiege)
            {
                TryRemoveMonsterSiegeEffect(uiLabel);
                GameObject go = (GameObject)Instantiate(m_SiegeEffectPrefab.gameObject);
                go.transform.parent = uiLabel.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.GetComponent<MaplabelMonsterSiegeEffect>().Run = true;
            }
        }
    }

    /// <summary> lz-2016.10.09 尝试移除怪物攻城特效</summary>
    void TryRemoveMonsterSiegeEffect(UIMapLabel uiLabel)
    {
        ILabel label = uiLabel._ILabel;
        //lz-2016.09.06 如果是怪物攻城，就移除掉效果
        if (null != label && label is MonsterBeaconMark)
        {
            MonsterBeaconMark mark = label as MonsterBeaconMark;
            if (mark.IsMonsterSiege)
            {
                MaplabelMonsterSiegeEffect[] effects = uiLabel.GetComponentsInChildren<MaplabelMonsterSiegeEffect>(true);
                if (null != effects && effects.Length > 0)
                {
                    for (int i = 0; i < effects.Length; i++)
                    {
                        effects[i].Run = false;
                        Destroy(effects[i].gameObject);
                    }
                }
            }
        }
    }

    void RemvoeMapLabel(ILabel label)
    {
        UIMapLabel uiLabel = m_CurrentMapLabelList.Find(itr => (itr._ILabel.CompareTo(label)));
        if (uiLabel != null)
        {
            TryRemoveMonsterSiegeEffect(uiLabel);
            uiLabel.gameObject.SetActive(false);
            this.m_CurrentMapLabelList.Remove(uiLabel);
            this.m_MapLabelPool.Enqueue(uiLabel);
        }
    }

    void AddArrowLabel(ILabel label)
    {
        if (label.GetShow() == EShow.All || label.GetShow() == EShow.MinMap)
        {
            MissionLabel ml = label as MissionLabel;
            if (ml == null)
                return;
            if (ml.m_target == null)
                return;

            GameObject obj = GameObject.Instantiate(mArrowLablePrefab) as GameObject;
            obj.transform.parent = mSubInfoPanel.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;

            UIMapArrow uiArrow = obj.GetComponent<UIMapArrow>();
            if (uiArrow != null)
            {
                uiArrow.SetLabel(label, UIMapArrow.EArrowType.Main);
                mMapArrowList.Add(uiArrow);

                uiArrow.visualWidth = mMapSize.x - 10;
                uiArrow.visualHeight = mMapSize.y - 10;
            }
        }
    }

    void RemvoArrowLabel(ILabel label)
    {
        UIMapArrow uiLabel = mMapArrowList.Find(itr => (itr.trackLabel.CompareTo(label)));
        if (uiLabel != null)
        {
            GameObject.Destroy(uiLabel.gameObject);
            uiLabel.gameObject.transform.parent = null;
            mMapArrowList.Remove(uiLabel);
        }
    }


    void UpdateMapPos()
    {
        mWnd.transform.localPosition = new Vector3(-Convert.ToInt32(mMapSize.x / 2 + 1), -Convert.ToInt32(mMapSize.y / 2 + 1), 0);
        mSpCenterBg.transform.localScale = new Vector3(mMapSize.x - 30, mMapSize.y, 1);
        mMiniMapTex.transform.localScale = new Vector3(mMapSize.x, mMapSize.y, 1);
        mSpLeftBg.transform.localScale = new Vector3(20, mMapSize.y, 1);
        mSpRightBg.transform.localScale = new Vector3(33, mMapSize.y, 1);

        mSpLeftBg.transform.parent.localPosition = new Vector3(-Convert.ToInt32(mMapSize.x / 2 + 10), 0, 0);
        mSpRightBg.transform.parent.localPosition = new Vector3(Convert.ToInt32(mMapSize.x / 2 - 15), 0, 0);
        mPlayerPosText.transform.localPosition = new Vector3(15, -Convert.ToInt32(mMapSize.y / 2 - 10), 0);
        mPlayerPosText.MakePixelPerfect();
        mTimeLabel.transform.localPosition = new Vector3(Convert.ToInt32(mMapSize.x/2+5), Convert.ToInt32(mMapSize.y / 2 - 12), 0);
        mTimeLabel.MakePixelPerfect();

        mRTopBtns.transform.localPosition = new Vector3(0, Convert.ToInt32(mMapSize.y / 2), 0);
        mRBottomBtns.transform.localPosition = new Vector3(0, -Convert.ToInt32(mMapSize.y / 2), 0);
        mWeatherSpr.transform.localPosition = new Vector3(0, Convert.ToInt32(mMapSize.y / 2 - 12), 0);
        mChangeSize.transform.localPosition = new Vector3(0, -Convert.ToInt32(mMapSize.y / 2), 0);

        mSubInfoPanel.clipRange = new Vector4(-15, 0, mMapSize.x - 25, mMapSize.y);

        mSpLeftBg_1.transform.localPosition = mSpLeftBg.transform.localPosition;
        mSpLeftBg_1.transform.localScale = mSpLeftBg.transform.localScale;
        mTexLeftBg.transform.localPosition = new Vector3(mSpLeftBg.transform.localPosition.x, mSpLeftBg.transform.localPosition.y, -2);
        mTexLeftBg.transform.localScale = mSpLeftBg.transform.localScale;

        mSpRightBg_1.transform.localPosition = mSpRightBg.transform.localPosition;
        mSpRightBg_1.transform.localScale = mSpRightBg.transform.localScale;
        mTexRightBg.transform.localPosition = new Vector3(mSpRightBg.transform.localPosition.x, mSpRightBg.transform.localPosition.y, -2);
        mTexRightBg.transform.localScale = mSpRightBg.transform.localScale;

        mTexCenterBg.transform.localPosition = new Vector3(mSpCenterBg.transform.localPosition.x, mSpCenterBg.transform.localPosition.y, -2);
        mTexCenterBg.transform.localScale = mSpCenterBg.transform.localScale;

        // map arraw
        for (int i = 0; i < mMapArrowList.Count; i++)
        {
            mMapArrowList[i].visualWidth = mMapSize.x - 20;
            mMapArrowList[i].visualHeight = mMapSize.y - 10;
        }
    }


    void ReSetMapSize(Vector2 _MapSize)
    {
        if (_MapSize.x < mMinSize.x)
            _MapSize.x = mMinSize.x;
        if (_MapSize.y < mMinSize.y)
            _MapSize.y = mMinSize.y;
        if (_MapSize.x > mMaxSize.x)
            _MapSize.x = mMaxSize.x;
        if (_MapSize.y > mMaxSize.y)
            _MapSize.y = mMaxSize.y;

        mMapSize = new Vector2(Convert.ToInt32(_MapSize.x), Convert.ToInt32(_MapSize.y));
        mMapScale = new Vector2(mMapSize.x / 120, mMapSize.y / 120);
        UIRecentDataMgr.Instance.SetIntValue("MinMapSize_x", (int)mMapSize.x);
        UIRecentDataMgr.Instance.SetIntValue("MinMapSize_y", (int)mMapSize.y);
        UpdateMapPos();
        UpdateTweenInfo();
    }

    void OnDrapSize()
    {
        if (Input.GetMouseButton(0))
        {
            int x = Screen.width - Convert.ToInt32(Input.mousePosition.x) - 16;
            int y = Screen.height - Convert.ToInt32(Input.mousePosition.y) + 2;
            ReSetMapSize(new Vector2(x, y));

        }
    }

    void OnDrapSizeMouseMoveIn()
    {
        if (mIsOnDrapSize == false)
        {
            Cursor.visible = false;
            mTuodong.enabled = true;
            mIsOnDrapSize = true;
        }
    }

    void OnDrapSizeMouseMoveOut()
    {
        if (mIsOnDrapSize == true)
        {
            Cursor.visible = true;
            mTuodong.enabled = false;
            mIsOnDrapSize = false;
        }
    }

    void UpdateMiniMap()
    {
        if (null == mView)
        {
            Pathea.PeEntity e = Pathea.PeCreature.Instance.mainPlayer;
            if (null != e)
            {
                mView = e.GetCmpt<Pathea.PeTrans>();
            }
        }

		if (null != mView && GameUI.Instance.bVoxelComplete)
        {
            if (!GameConfig.IsInVCE)
            {
                if (!mMiniMapCam.targetTexture.IsCreated())
                {
                    mMiniMapCam.targetTexture.Create();
                    ReFlashMap();
                }

                Vector3 pos = mView.position + 1000 * Vector3.up;
                pos.y = 0;

                //if (true)
				if(Vector3.Distance(pos, mMapCenterPos) > 62f || (GameTime.Timer.Second - mMapReFlashTime > 600.0)) // disable camera would cause unreasonable memory allocation
                {
                    mMiniMapCam.enabled = true;
                    mMapCenterPos = pos;
                    mMiniMapCam.transform.position = mView.position + 300 * Vector3.up;
                    mMapReFlashTime = GameTime.Timer.Second;
                }
                else
                {
                    if (mMiniMapCam.enabled)
                        mMiniMapCam.enabled = false;
                }

                mMiniMapTex.uvRect = new Rect((pos.x - mMapCenterPos.x) / 248f / mMiniMapCam.aspect + 0.25f, (pos.z - mMapCenterPos.z) / 248f + 0.25f, 0.5f / mMiniMapCam.aspect, 0.5f);
                float mCenter_x = Convert.ToSingle(0.5 + (pos.x - mMapCenterPos.x) / 248f);
                float mCenter_y = Convert.ToSingle(0.5 + (pos.z - mMapCenterPos.z) / 248f);
                mMiniMapTex.material.SetFloat("_Center_x", mCenter_x);
                mMiniMapTex.material.SetFloat("_Center_y", mCenter_y);
                mMiniMapTex.material.SetFloat("_Alpha", mMapAlpha);
                mMiniMapTex.material.SetFloat("_Bright", mMapBright);

                //float xfactor = (mMapSize.x - mMinSize.x)/(mMaxSize.x - mMinSize.x);
                //float yfactor = (mMapSize.y - mMinSize.y) / (mMaxSize.y - mMinSize.y);

                //mMiniMapTex.uvRect = new Rect((pos.x - mMapCenterPos.x) / 248f + ((1 - xfactor) * 0.5f), (pos.z - mMapCenterPos.z) / 248f + ((1 - xfactor) * 0.5f), xfactor, yfactor);
                //float mCenter_x = Convert.ToSingle(xfactor + (pos.x - mMapCenterPos.x) / 248f);
                //float mCenter_y = Convert.ToSingle(yfactor + (pos.z - mMapCenterPos.z) / 248f);
                //mMiniMapTex.material.SetFloat("_Center_x", mCenter_x);
                //mMiniMapTex.material.SetFloat("_Center_y", mCenter_y);
                //mMiniMapTex.material.SetFloat("_Alpha", mMapAlpha);
                //mMiniMapTex.material.SetFloat("_Bright", mMapBright);

            }
        }
    }

	void UpdateTime()
    {
		mTimeLabel.text = GameTime.Timer.GetStrHhMm ();
	}

    void UpdateSubInfo()
    {
        if (mView != null)
        {
			mPlayerPosText.text = mView.GetStrPXZ();
            mMapPlayerSpr.transform.rotation = Quaternion.Euler(0, 0, -mView.rotation.eulerAngles.y);
        }
    }

    void UpdateCurAllLabelShow()
    {
        if (null==this.m_CurrentMapLabelList  || this.m_CurrentMapLabelList.Count <= 0) return;
        for (int i = 0; i < this.m_CurrentMapLabelList.Count; i++)
        {
            UIMapLabel mapLabel=this.m_CurrentMapLabelList[i];
            if (null != mapLabel &&null!=mapLabel._ILabel)
            {
                if (this.CheckInViewYRange(mapLabel._ILabel.GetPos().y))
                {
                    if (!mapLabel.gameObject.activeSelf)
                        mapLabel.gameObject.SetActive(true);
                }
                else
                {
                    if (mapLabel.gameObject.activeSelf)
                        mapLabel.gameObject.SetActive(false);
                }

                //lz-2016.06.15 npc是任务跟随着或者仆从就不显示小地图任务相关图标
                if(mapLabel.type== ELabelType.Mission)
                {
                    //lz-2016.06.16 任务区域标类型的任务图标不隐藏，npc对象改为UIMapLabel所在的npc对象，而不是任务的归属npc对象
                    MissionLabel missionLabel = (MissionLabel)mapLabel._ILabel;
                    //lz-2016.10.12 这里能return，return会结束循环，导致后面的Label不能正常遍历检测
                    if (missionLabel.m_type != MissionLabelType.misLb_target && mapLabel.NpcID != -1)
                    {
                        PeEntity entity = EntityMgr.Instance.Get(mapLabel.NpcID);
                        if (null != entity && null != entity.NpcCmpt && entity.NpcCmpt.IsFollower)
                        {
                            if (mapLabel.gameObject.activeSelf)
                                mapLabel.gameObject.SetActive(false);
                        }
                        else
                        {
                            if (!mapLabel.gameObject.activeSelf)
                                mapLabel.gameObject.SetActive(true);
                        }
                    }
                }

                //lz-2018.01.03 队友更新朝向角度
                if (PeGameMgr.IsMulti && mapLabel._ILabel.GetIcon() == MapIcon.AllyPlayer)
                {
                    MapCmpt mapCmpt = (mapLabel._ILabel as MapCmpt);
                    if (mapCmpt && mapCmpt.Entity && mapCmpt.Entity.peTrans)
                    {
                        mapLabel.transform.rotation = Quaternion.Euler(0, 0, -mapCmpt.Entity.peTrans.rotation.eulerAngles.y);
                    }
                }
            }
        }
    }
    void BtnAlphaOnClick()
    {
        mMapAlpha += 0.15f;
        if (mMapAlpha > 0.9)
            mMapAlpha = 0.2f;
        UIRecentDataMgr.Instance.SetFloatValue("MinMapAlpha", mMapAlpha);
    }

    void BtnBrightOnClick()
    {
        mMapBright += 0.5f;
        if (mMapBright > 3)
            mMapBright = 1;
        UIRecentDataMgr.Instance.SetFloatValue("MinMapBright", mMapBright);
    }

    void BtnMissionOnClick()
    {
        if (Pathea.PeGameMgr.IsCustom)
        {
            if (GameUI.Instance.mCustomMissionTrack.isShow)
                GameUI.Instance.mCustomMissionTrack.Hide();
            else
            {
                GameUI.Instance.mCustomMissionTrack.Show();
                GameUI.Instance.mCustomMissionTrack.Show();
            }
        }
        else
        {
            if (GameUI.Instance.mMissionTrackWnd.isShow)
                GameUI.Instance.mMissionTrackWnd.Hide();
            else
                GameUI.Instance.mMissionTrackWnd.Show();
        }
    }

    void OnOpenWorldMap()
    {
        //lz-2016.06.24 吴哥说小地图按下鼠标左键打开世界地图，右键不操作
        if (Input.GetMouseButtonUp(0)&&GameUI.Instance != null && GameUI.Instance.mUIWorldMap != null && PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial)
            GameUI.Instance.mUIWorldMap.Show();
    }

    void OnNpcTalkHistoryClick()
    {
        if (null != GameUI.Instance)
        {
            GameUI.Instance.mNpcTalkHistoryWnd.ChangeWindowShowState();
        }
    }

    #endregion

    #region public methods

    public void ReFlashMap()
    {
        mMapCenterPos = -100000f * Vector3.one;
    }

    public float CameraNear{
		get{
			return mMiniMapCam.nearClipPlane;
		}
		set{
			
			mMiniMapCam.nearClipPlane= value;
		}
	}
	public float CameraFar{
		get{
			return mMiniMapCam.farClipPlane;
		}
		set{
			
			mMiniMapCam.farClipPlane= value;
		}
	}
	public float CameraPosY{
		get{return mMiniMapCam.transform.position.y;}
	}

	public void UpdateCameraPos(){
		mMiniMapCam.transform.position = mView.position + 300 * Vector3.up;
	}
	
	public bool CheckInViewYRange(float curY)
    {
        if (null != this.mMiniMapCam)
        {
            float nearY = this.mMiniMapCam.transform.position.y - this.CameraNear;
            float farY = this.mMiniMapCam.transform.position.y - this.CameraFar;
            return curY<=nearY&&curY>=farY;
        }
        return false;
    }

    /// <summary>获取小地图组件的整体宽度 </summary>
    public float GetMinMapWidth()
    {
        return mSpLeftBg.transform.localScale.x + mSpCenterBg.transform.localScale.x + mSpRightBg.transform.localScale.x;
    }
    #endregion

    #region Tutorial 
    [SerializeField]
    private UIWndTutorialTip_N m_MapTutorialPrefab;
    [SerializeField]
    private Transform m_MapTutorialParent;
    [SerializeField]
    private UIWndTutorialTip_N m_MissionLogTutorialPrefab;
    [SerializeField]
    private Transform m_MissionLogTutorialParent;
    [SerializeField]
    private UIWndTutorialTip_N m_ConversationsTutorialPrefab;
    [SerializeField]
    private Transform m_ConversationsTutorialParent;

    /// <summary>lz-2016.11.03 显示这个小地图的Tutorial</summary>
    public void ShowMapTutorial()
    {
        if (PeGameMgr.IsTutorial)
        {
            //lz-2016.11.07 弹提示的时候如果小地图隐藏着，就弹出来
            if (m_MinMapIsHide)
                HideMapBtnOnClick(m_MinMapHideBtn.gameObject);
            GameObject go = Instantiate(m_MapTutorialPrefab.gameObject);
            go.transform.parent = m_MapTutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>lz-2016.11.03 显示任务追踪按钮的Tutorial</summary>
    public void ShowMissionTrackTutorial()
    {
        if (PeGameMgr.IsTutorial)
        {
            //lz-2016.11.07 弹提示的时候如果小地图隐藏着，就弹出来
            if (m_MinMapIsHide)
                HideMapBtnOnClick(m_MinMapHideBtn.gameObject);
            GameObject go = Instantiate(m_MissionLogTutorialPrefab.gameObject);
            go.transform.parent = m_MissionLogTutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            //lz-2016.11.07 任务追踪Tutorial显示结束的时候显示对话回溯按钮的Tutorial
            go.GetComponent<UIWndTutorialTip_N>().DeleteEvent = ShowConversationsBtnTutorial;
        }
    }

    /// <summary>lz-2016.11.03 显示对话回溯按钮的Tutorial</summary>
    private void ShowConversationsBtnTutorial()
    {
        if (PeGameMgr.IsTutorial)
        {
            //lz-2016.11.07 弹提示的时候如果小地图隐藏着，就弹出来
            if (m_MinMapIsHide)
                HideMapBtnOnClick(m_MinMapHideBtn.gameObject);
            GameObject go = Instantiate(m_ConversationsTutorialPrefab.gameObject);
            go.transform.parent = m_ConversationsTutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
    }
    #endregion
}
