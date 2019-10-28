using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AppearBlendShape;
using CustomCharactor;

public class UIPlayerBuildCtrl : MonoBehaviour
{
    //panel_0
    [SerializeField]
    UIInput mNameInput;
    [SerializeField]
    UIPageGridBox mHeadGridBox;
    [SerializeField]
    UIPageGridBox mFaceGridBox;
    [SerializeField]
    Palette_N mSkinColor;
    //panel_1
    [SerializeField]
    UIPageGridBox mHairGridBox;
    [SerializeField]
    Palette_N mHairColor;
    [SerializeField]
    UIScrollBar mFaceWidth;
    [SerializeField]
    UIScrollBar mFaceThickness;
    [SerializeField]
    UIScrollBar mChinWidth;
    [SerializeField]
    UIScrollBar mJawWidth;
    //panel_2
    [SerializeField]
    UIScrollBar mEyebrowsLocation;
    [SerializeField]
    UIScrollBar mEyebrowsDirection;
    [SerializeField]
    UIScrollBar mEyesDirection;
    [SerializeField]
    UIScrollBar mEyesSize;
    [SerializeField]
    Palette_N mEyeballsColor;
    [SerializeField]
    UIScrollBar mNoseLocation;
    [SerializeField]
    UIScrollBar mNoseHeight;
    [SerializeField]
    UIScrollBar mNoseSize;
    [SerializeField]
    UIScrollBar mMouthLocation;
    [SerializeField]
    UIScrollBar mMouthSize;
    [SerializeField]
    UIScrollBar mMouthShap;
    //panel_3	
    [SerializeField]
    UIScrollBar mShoulder;
    [SerializeField]
    UIScrollBar mBreast;
    [SerializeField]
    UIScrollBar mUpperArm;
    [SerializeField]
    UIScrollBar mLowerArm;
    [SerializeField]
    UIScrollBar mBelly;
    [SerializeField]
    UIScrollBar mWaist;
    [SerializeField]
    UIScrollBar mUpperLeg;
    [SerializeField]
    UIScrollBar mLowerLeg;
    [SerializeField]
    Palette_N mSkinColor2;

    //panel_4
    [SerializeField]
    UIPageGridBox mSaveGrodBox;
    [SerializeField]
    UIBtnTouZi mBtnRandom;
    [SerializeField]
    UILabelTishi mLbTishi;

    //ModePrefab
    [SerializeField]
    GameObject mMalePrefab;
    [SerializeField]
    GameObject mFemalePrefab;

    [SerializeField]
    UISprite mBtnMaleBg;
    [SerializeField]
    UISprite mBtnFemaleBg;
    [SerializeField]
    BoxCollider mBtnMaleCollider;
    [SerializeField]
    BoxCollider mBtnFemaleCollider;
    [SerializeField]
    Camera mUICamera;
    [SerializeField]
    GameObject mUIMapSelect;

    [HideInInspector]
    public bool haschanged = false;

    List<PlayerBuildGirdItem> mHeadList;
    List<PlayerBuildGirdItem> mFaceList;
    List<PlayerBuildGirdItem> mHairList;
    List<PlayerBuildGirdItem> mSaveList;

    public class ScrollItem
    {
        public UIScrollBar mScrollBar;
        public EMorphItem mType;

        public ScrollItem(UIScrollBar bar, EMorphItem type)
        {
            mScrollBar = bar;
            mType = type;
        }
    }
    List<ScrollItem> mScrollItemList;



    bool mInitEnd;
    Vector3 mBodyCamPos;
    int mCameraState = 0;
    UIPlayerBuildMoveCtrl mMoveCtrl;


    PlayerModel mMaleInfo;
    PlayerModel mFemaleInfo;
    PlayerModel mCurrent;


    public enum ESex
    {
        Def = 0,
        Female = 1,
        Male = 2
    }
    ESex Sex
    {
        get;
        set;
    }

    CustomMetaData mMetaData;

    Vector3 mTargetCamPos;
    float mRotSpeed = 90f;
    bool mRotLeft = false;
    bool mRotRight = false;

    // Use this for initialization
    void Awake()
    {
        InitPlayer();
        mBodyCamPos = Camera.main.transform.position;
        CustomDataMgr.Instance.LoadAllData();
        // Init UI
        InitGirdBox(mHeadGridBox, out mHeadList, PlayerBuildGirdItem.Type.Type_Head);
        InitGirdBox(mFaceGridBox, out mFaceList, PlayerBuildGirdItem.Type.Type_Face);
        InitGirdBox(mHairGridBox, out mHairList, PlayerBuildGirdItem.Type.Type_Hair);
        InitGirdBox(mSaveGrodBox, out mSaveList, PlayerBuildGirdItem.Type.Type_Save);

        mHeadGridBox.e_RefalshGrid += ReflashHeadGridBox;
        mFaceGridBox.e_RefalshGrid += ReflashFaceGridBox;
        mHairGridBox.e_RefalshGrid += ReflashHairGridBox;
        mSaveGrodBox.e_RefalshGrid += ReflashSaveGridBox;

        if (mFaceList == null)
            Debug.Log("mFaceList is null");

        mSkinColor.e_ChangeColor += SetSkinColor;
        mSkinColor2.e_ChangeColor += SetSkinColor;
        mEyeballsColor.e_ChangeColor += SetEyeColor;
        mHairColor.e_ChangeColor += SetHairColor;


        mBtnRandom.e_EndRun += BtnRandomOnClick;
        mMoveCtrl = gameObject.GetComponent<UIPlayerBuildMoveCtrl>();

        InitScrolleBar();

        foreach (ScrollItem item in mScrollItemList)
        {
            // item.mScrollBar.onChange = OnScrollValueChange;
            UIEventListener listener = UIEventListener.Get(item.mScrollBar.background.gameObject);
            listener.onPress += OnScrollBarPress;
            listener = UIEventListener.Get(item.mScrollBar.foreground.gameObject);
            listener.onPress += OnScrollBarPress;
            listener.onDrag += OnScrollBarDrag;
        }
    }

    void OnScrollBarPress(GameObject go, bool isPressed)
    {
        if (CheckNeedSaveTip())
            haschanged = true;
    }

    void OnScrollBarDrag(GameObject go, Vector2 delta)
    {
        if (CheckNeedSaveTip())
            haschanged = true;
    }

    void Start()
    {
        BtnMaleOnClick();
        ChangeCameraPos();
        ResetSaveGridBox();
    }
    // Update is called once per frame
    float starDis = 0;
    void Update()
    {
        if (ChangeAppearData())
            RebuildModel();

        if (mMoveCtrl != null)
            if (mMoveCtrl.mCameraState != mCameraState)
                ChangeCameraPos();

        if (Input.GetMouseButton(1))
        {
            if (starDis != 0)
                mCurrent.mMode.transform.rotation *= Quaternion.AngleAxis((starDis - Input.mousePosition.x) * 20 * Time.deltaTime, mCurrent.mMode.transform.up);
            starDis = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(1))
            starDis = 0;

        if (isMouseOnClider(mBtnMaleCollider))
            mBtnMaleBg.color = Color.white;
        else
            mBtnMaleBg.color = (Sex == ESex.Male) ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1);
        if (isMouseOnClider(mBtnFemaleCollider))
            mBtnFemaleBg.color = Color.white;
        else
            mBtnFemaleBg.color = (Sex == ESex.Female) ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1);

        if (mRotLeft)
            mCurrent.mMode.transform.rotation *= Quaternion.Euler(0, mRotSpeed * Time.deltaTime, 0);
        if (mRotRight)
            mCurrent.mMode.transform.rotation *= Quaternion.Euler(0, -mRotSpeed * Time.deltaTime, 0);

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, mTargetCamPos, 6f * Time.deltaTime);
    }

    bool isMouseOnClider(BoxCollider collider)
    {
        Ray ray = mUICamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        //float mindist = 100f;
        bool resoult = collider.Raycast(ray, out rayHit, 100);
        return resoult;
    }

    Vector3 OffSetPos = new Vector3(0, 0.12f, 0.6f);
    void ChangeCameraPos()
    {
        mCameraState = mMoveCtrl.mCameraState;
        switch (mCameraState)
        {
            case 0:
                mTargetCamPos = mBodyCamPos;
                break;
            case 1:
                mTargetCamPos = mCurrent.mHeadTran.position + OffSetPos;
                break;
            default:
                break;
        }
    }


    // Init GridBox
    void InitGirdBox(UIPageGridBox gridBox, out List<PlayerBuildGirdItem> gridList, PlayerBuildGirdItem.Type _type)
    {
        gridBox.InitGrid();
        gridList = new List<PlayerBuildGirdItem>();
        for (int i = 0; i < gridBox.mItemsObject.Count; i++)
        {
            PlayerBuildGirdItem item = gridBox.mItemsObject[i].GetComponent<PlayerBuildGirdItem>();
            item.InitItem(i, _type);
            item.SetItemInfo("Null");
            item.e_ClickItem += GridBoxItemOnClick;
            item.canSelected = true;
            if (item != null)
                gridList.Add(item);
        }
    }

    void InitScrolleBar()
    {
        mScrollItemList = new List<ScrollItem>();

        mScrollItemList.Add(new ScrollItem(mFaceWidth, EMorphItem.FaceWidth));
        mScrollItemList.Add(new ScrollItem(mFaceThickness, EMorphItem.FaceThickness));
        mScrollItemList.Add(new ScrollItem(mChinWidth, EMorphItem.ChinWidth));
        mScrollItemList.Add(new ScrollItem(mJawWidth, EMorphItem.JawWidth));

        mScrollItemList.Add(new ScrollItem(mEyebrowsLocation, EMorphItem.EyebrowLocation));
        mScrollItemList.Add(new ScrollItem(mEyebrowsDirection, EMorphItem.EyebrowDirection));
        mScrollItemList.Add(new ScrollItem(mEyesDirection, EMorphItem.EyeDirection));
        mScrollItemList.Add(new ScrollItem(mEyesSize, EMorphItem.EyeSize));

        mScrollItemList.Add(new ScrollItem(mNoseLocation, EMorphItem.NoseLocation));
        mScrollItemList.Add(new ScrollItem(mNoseHeight, EMorphItem.NoseHeight));
        mScrollItemList.Add(new ScrollItem(mNoseSize, EMorphItem.NoseSize));

        mScrollItemList.Add(new ScrollItem(mMouthLocation, EMorphItem.MouthLocation));
        mScrollItemList.Add(new ScrollItem(mMouthSize, EMorphItem.MouthSize));
        mScrollItemList.Add(new ScrollItem(mMouthShap, EMorphItem.MouthShape));

        mScrollItemList.Add(new ScrollItem(mShoulder, EMorphItem.Shoulder));
        mScrollItemList.Add(new ScrollItem(mBreast, EMorphItem.Breast));
        mScrollItemList.Add(new ScrollItem(mUpperArm, EMorphItem.UpperArm));
        mScrollItemList.Add(new ScrollItem(mLowerArm, EMorphItem.LowerArm));

        mScrollItemList.Add(new ScrollItem(mBelly, EMorphItem.Belly));
        mScrollItemList.Add(new ScrollItem(mWaist, EMorphItem.Waist));
        mScrollItemList.Add(new ScrollItem(mUpperLeg, EMorphItem.UpperLeg));
        mScrollItemList.Add(new ScrollItem(mLowerLeg, EMorphItem.LowerLeg));
    }
    void InitPlayer()
    {
        // Init Male
        GameObject fgo = GameObject.Find("VirtualObjManager");
        GameObject obj = Instantiate(mMalePrefab) as GameObject;
        obj.transform.position = fgo.transform.position;
        obj.transform.rotation = Quaternion.Euler(0, -10, 0);

        mMaleInfo = obj.GetComponent<PlayerModel>();
        mMaleInfo.mAppearData = new AppearData();
        mMaleInfo.mClothed = new AvatarData();
        mMaleInfo.mNude = new AvatarData();

        obj.name = "Male";
        obj.tag = "Player";
        obj.SetActive(false);

        // Init Female
        obj = Instantiate(mFemalePrefab) as GameObject;
        obj.transform.position = fgo.transform.position;
        obj.transform.rotation = Quaternion.Euler(0, -10, 0);

        mFemaleInfo = obj.GetComponent<PlayerModel>();
        mFemaleInfo.mAppearData = new AppearData();
        mFemaleInfo.mClothed = new AvatarData();
        mFemaleInfo.mNude = new AvatarData();

        obj.name = "Female";
        obj.tag = "Player";
        obj.SetActive(false);
    }

    bool ChangeAppearData()
    {
        bool change = false;

        for (int i = 0; i < mScrollItemList.Count; i++)
        {
            float weight = mCurrent.mAppearData.GetWeight(mScrollItemList[i].mType);
            float scValue = mScrollItemList[i].mScrollBar.scrollValue * 2 - 1;
            if (!Mathf.Approximately(weight, scValue))
            {
                change = true;
                mCurrent.mAppearData.SetWeight(mScrollItemList[i].mType, scValue);
            }
        }
        return change;
    }

    void ResetBuildUIValue()
    {
        for (int i = 0; i < mScrollItemList.Count; i++)
        {
            float weight = mCurrent.mAppearData.GetWeight(mScrollItemList[i].mType);
            float scValue = mScrollItemList[i].mScrollBar.scrollValue * 2 - 1;
            if (!Mathf.Approximately(weight, scValue))
                mScrollItemList[i].mScrollBar.scrollValue = (weight + 1) / 2;
        }
        ResetHeadGridSelected();
        ResetHairGridSelected();
    }


    void ResetHeadGridSelected()
    {
        int index = mMetaData.GetHeadIndex(mCurrent.mNude[AvatarData.ESlot.Head]);
        int uiIndex = index - mHeadGridBox.mStartIndex;
        if (uiIndex < mHeadList.Count && uiIndex >= 0 && index != -1)
        {
            if (mHeadGridSelectedItem != mHeadList[uiIndex])
            {
                if (mHeadGridSelectedItem != null)
                    mHeadGridSelectedItem.isSelected = false;
                mHeadGridSelectedItem = mHeadList[uiIndex];
                mHeadGridSelectedItem.isSelected = true;
            }
        }
        else
        {
            if (mHeadGridSelectedItem != null)
                mHeadGridSelectedItem.isSelected = false;
            mHeadGridSelectedItem = null;
        }
    }

    void ResetHairGridSelected()
    {
        int index = mMetaData.GetHairIndex(mCurrent.mNude[AvatarData.ESlot.HairF]);
        int uiIndex = index - mHairGridBox.mStartIndex;
        if (uiIndex < mHairList.Count && uiIndex >= 0 && index != -1)
        {
            if (mHairGridSelectedItem != mHairList[uiIndex])
            {
                if (mHairGridSelectedItem != null)
                    mHairGridSelectedItem.isSelected = false;
                mHairGridSelectedItem = mHairList[uiIndex];
                mHairGridSelectedItem.isSelected = true;
            }
        }
        else
        {
            if (mHairGridSelectedItem != null)
                mHairGridSelectedItem.isSelected = false;
            mHairGridSelectedItem = null;
        }
    }



    void ResetBodyPartGridBox()
    {
        int tempCount = mMetaData.GetHeadCount();
        mHeadGridBox.ResetItemCount(tempCount);
        mHeadGridBox.ReflashGridCotent();

        tempCount = mMetaData.GetHairCount();
        mHairGridBox.ResetItemCount(tempCount);
        mHairGridBox.ReflashGridCotent();

        BtnResetOnClick();
    }

    void ResetSaveGridBox()
    {
        int count = CustomDataMgr.Instance.dataCount;
        mSaveGrodBox.ResetItemCount(count);
        mSaveGrodBox.ReflashGridCotent();
    }




    void RebuildModel()
    {
        mCurrent.BuildModel();
        // bounds center correction
        SkinnedMeshRenderer sm = mCurrent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (sm != null)
        {
            Bounds newBounds = sm.localBounds;
            newBounds.center = Vector3.zero;
            sm.localBounds = newBounds;
        }

    }

    void ApplyColor()
    {
        mCurrent.ApplyColor();
    }

    // call_back func 
    void ReflashHeadGridBox(int start)
    {
        for (int i = 0; i < mHeadList.Count; i++)
        {
            if (i + start < mHeadGridBox.mItemCount)
                mHeadList[i].SetItemInfo(mMetaData.GetHead(i + start).icon);
            else
                mHeadList[i].SetItemInfo("Null");
        }
        ResetHeadGridSelected();
    }
    // call_back func 
    void ReflashHairGridBox(int start)
    {
        for (int i = 0; i < mHairList.Count; i++)
        {
            if (i + start < mHairGridBox.mItemCount)
                mHairList[i].SetItemInfo(mMetaData.GetHair(i + start).icon);
            else
                mHairList[i].SetItemInfo("Null");
        }

        ResetHairGridSelected();
    }
    // call_back func 
    void ReflashFaceGridBox(int start)
    {

    }
    // call_back func 
    void ReflashSaveGridBox(int start)
    {
        for (int i = 0; i < mSaveList.Count; i++)
        {
            if (i + start < CustomDataMgr.Instance.dataCount)
            {
                mSaveList[i].SetItemInfo(CustomDataMgr.Instance.GetDataHeadIco(i + start));
            }
            else
                mSaveList[i].SetItemInfo("Null");
        }
        if (mSaveGridSelectedItem != null)
        {
            mSaveGridSelectedItem.isSelected = false;
            mSaveGridSelectedItem = null;
            mSaveGridSelectedIndex = -1;
        }
    }

    void SetHairColor(Color col)
    {
        mCurrent.mAppearData.mHairColor = col;
        ApplyColor();
        if (CheckNeedSaveTip())
            haschanged = true;
    }

    void SetSkinColor(Color col)
    {
        mCurrent.mAppearData.mSkinColor = col;
        ApplyColor();
        haschanged = true;
    }

    void SetEyeColor(Color col)
    {
        mCurrent.mAppearData.mEyeColor = col;
        ApplyColor();
        if (CheckNeedSaveTip())
            haschanged = true;
    }

    public bool actionOk = true;
    // login game

    public void FirstToTutorial()
    {
        //lw:多人出教程后，进入lobby
        TutorialExit.type = Pathea.PeGameMgr.playerType == Pathea.PeGameMgr.EPlayerType.Multiple ?
          TutorialExit.TutorialType.MultiLobby : TutorialExit.TutorialType.Story;

        SystemSettingData.Instance.Tutorialed = true;

        string nickname = mNameInput.text;

        if (string.IsNullOrEmpty(nickname))
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000051));
            return;
        }

        CustomDataMgr.Instance.Current = CreateCustomData();
        CustomDataMgr.Instance.Current.charactorName = nickname;

        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Tutorial;
		Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Tutorial;
        Pathea.PeGameMgr.tutorialMode = Pathea.PeGameMgr.ETutorialMode.DigBuild;

        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);
    }

    public static void MainmenuToTutorial()
    {
        TutorialExit.type = TutorialExit.TutorialType.Mainmenu;
        SystemSettingData.Instance.Tutorialed = true;
        Pathea.SingleGameStory.curType = Pathea.SingleGameStory.StoryScene.TrainingShip;

        Pathea.PeGameMgr.gameLevel = Pathea.PeGameMgr.EGameLevel.Easy;
        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Tutorial;
        Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Tutorial;
        Pathea.PeGameMgr.tutorialMode = Pathea.PeGameMgr.ETutorialMode.DigBuild;

        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);
    }

    void CreatePlayer()
    {
        string nickname = mNameInput.text;

        if (string.IsNullOrEmpty(nickname))
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000051));
            return;
        }

        int mSex = (int)Sex;

        if (Pathea.PeGameMgr.IsSingle)
        {
            CustomDataMgr.Instance.Current = CreateCustomData();
            CustomDataMgr.Instance.Current.charactorName = nickname;

            if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Custom)
            {
                //				mUIMapSelect.SetActive(true);
                Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;
                Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
                Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;

                Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);
            }

            else if (Pathea.PeGameMgr.randomMap)
            {
                SeedSetGui_N.Instance.Show();
            }
            else
            {
                IntroRunner.movieEnd = (() =>
                {
                    Debug.Log("<color=aqua>intro movie end.</color>");
                    PeSceneCtrl.Instance.GotoGameSence();
                });

                Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.Intro);
            }
        }
        else if (Pathea.PeGameMgr.IsMulti)
        {
            if (actionOk)
            {
                byte[] appearData = mCurrent.mAppearData.Serialize();
                byte[] nudeData = mCurrent.mNude.Serialize();
                LobbyInterface.LobbyRPC(ELobbyMsgType.RoleCreate, nickname, (byte)(mSex), appearData, nudeData);
                actionOk = false;
                Invoke("ResetActionOK", 2.0f);
            }
        }
        else
        {
            Debug.LogError("error player mode.");
        }
    }

    public void ResetActionOK()
    {
        actionOk = true;
    }

    bool SaveAppearData()
    {
        CustomCharactor.CustomData customData = CreateCustomData();
        return CustomDataMgr.Instance.SaveData(customData);
    }

    private CustomCharactor.CustomData CreateCustomData()
    {
        CustomCharactor.CustomData customData = new CustomCharactor.CustomData();
        customData.headIcon = PeViewStudio.TakePhoto(mCurrent.gameObject, 64, 64, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
        byte[] buf = mCurrent.mAppearData.Serialize();
        customData.appearData = new AppearData();
        customData.appearData.Deserialize(buf);
        customData.sex = (Sex == ESex.Male) ? CustomCharactor.ESex.Male : CustomCharactor.ESex.Female;
        customData.nudeAvatarData = new CustomCharactor.AvatarData(mCurrent.mNude);
        return customData;
    }

    void LoadAppearData(CustomCharactor.CustomData mCustomData)
    {
        if (mCustomData == null)
            return;

        Sex = (mCustomData.sex == CustomCharactor.ESex.Male) ? ESex.Male : ESex.Female;
        ChangeSex();

        mCurrent.mAppearData = mCustomData.appearData;
        mCurrent.mNude = mCustomData.nudeAvatarData;

        ResetBuildUIValue();
        RebuildModel();
    }

    void ChangeSex()
    {
        if (Sex == ESex.Male)
        {
            mMaleInfo.mMode.SetActive(true);
            mFemaleInfo.mMode.SetActive(false);
            mCurrent = mMaleInfo;
            mMetaData = CustomMetaData.InstanceMale;
        }
        else
        {
            mMaleInfo.mMode.SetActive(false);
            mFemaleInfo.mMode.SetActive(true);
            mCurrent = mFemaleInfo;
            mMetaData = CustomMetaData.InstanceFemale;
        }
        ChangeCameraPos();
        ResetBodyPartGridBox();
        SetDefaultColor();
    }

    void SetDefaultColor()
    {
        //lz-2016.08.21  拿到初始颜色
        mSkinColor.CurCol = mCurrent.mAppearData.mSkinColor;
        mSkinColor2.CurCol = mCurrent.mAppearData.mSkinColor;
        mEyeballsColor.CurCol = mCurrent.mAppearData.mEyeColor;
        mHairColor.CurCol = mCurrent.mAppearData.mHairColor;
    }

    #region OnClick Func

    void BtnEnterOnClick()
    {
        // Only do samething when left mouse click
        if (Input.GetMouseButtonUp(0))
        {
            bool isEditor = false;
#if UNITY_EDITOR
            isEditor = true;
#endif
            if (isEditor)
            {
                if(Pathea.PeGameMgr.playerType == Pathea.PeGameMgr.EPlayerType.Multiple)
                {
                    if (mNameInput.text == "tutorial")
                        SystemSettingData.Instance.Tutorialed = false;

                    CreatePlayer();
                }
                else
                {
                    if (mNameInput.text == "tutorial")
                        FirstToTutorial();
                    else
                        CreatePlayer();
                }
            }
            else
            {
                if (Pathea.PeGameMgr.playerType == Pathea.PeGameMgr.EPlayerType.Multiple)
                {
                    CreatePlayer();
                }
                else
                {
                    if (SystemSettingData.Instance.Tutorialed || Pathea.PeGameMgr.sceneMode != Pathea.PeGameMgr.ESceneMode.Story)
                        CreatePlayer();
                    else
                        FirstToTutorial();
                }
                   
            }
        }
    }

    void BtnBackOnClick()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000052), PeSceneCtrl.Instance.GotoMainMenuScene);
    }

    void BtnMaleOnClick()
    {
        Sex = ESex.Male;
        ChangeSex();
    }

    void BtnFemaleOnClick()
    {
        Sex = ESex.Female;
        ChangeSex();
    }

    void BtnRandomOnClick()
    {

        if (haschanged)
        {
            haschanged = !haschanged;
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000173), BtnSaveOnClick, BtnRandomOnClick);
            return;
        }

        int headCount = mMetaData.GetHeadCount();
        int headIndex = UnityEngine.Random.Range(0, headCount);
        mCurrent.mNude.SetPart(AvatarData.ESlot.Head, mMetaData.GetHead(headIndex).modelPath);

        int hairCount = mMetaData.GetHairCount();
        int hairIndex = UnityEngine.Random.Range(0, hairCount);
        string[] HairPath = mMetaData.GetHair(hairIndex).modelPath;
        mCurrent.mNude.SetPart(AvatarData.ESlot.HairF, HairPath[0]);
        mCurrent.mNude.SetPart(AvatarData.ESlot.HairT, HairPath[1]);
        mCurrent.mNude.SetPart(AvatarData.ESlot.HairB, HairPath[2]);

        mCurrent.mAppearData.mHairColor
            = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);

        mCurrent.mAppearData.mEyeColor
            = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);

        mCurrent.mAppearData.mSkinColor
            = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);

        mCurrent.mAppearData.RandomMorphWeight();

        ResetBuildUIValue();

        RebuildModel();

    }


    void BtnResetOnClick()
    {
        if (Sex == ESex.Male)
            mCurrent.mNude.SetMaleBody();
        else
            mCurrent.mNude.SetFemaleBody();

        mCurrent.mAppearData.Default();
        ResetBuildUIValue();
        RebuildModel();
        haschanged = false;
    }

    void BtnSaveOnClick()
    {
        SaveAppearData();
        ResetSaveGridBox();
        mLbTishi.ShowText(PELocalization.GetString(10064));
        haschanged = false;
    }

    void BtnLoadOnClick()
    {
        if (mSaveGridSelectedIndex < 0)
            return;

        if (mSaveGridSelectedIndex < CustomDataMgr.Instance.dataCount)
            LoadAppearData(CustomDataMgr.Instance.GetCustomData(mSaveGridSelectedIndex));
    }

    void BtnDeleteOnClick()
    {
        if (mSaveGridSelectedIndex < 0)
            return;

        if (mSaveGridSelectedIndex < CustomDataMgr.Instance.dataCount)
        {
            CustomDataMgr.Instance.DeleteData(mSaveGridSelectedIndex);
            ResetSaveGridBox();
        }
    }

    PlayerBuildGirdItem mHeadGridSelectedItem = null;
    PlayerBuildGirdItem mHairGridSelectedItem = null;
    PlayerBuildGirdItem mSaveGridSelectedItem = null;

    void GridBoxItemOnClick(int uiIndex, PlayerBuildGirdItem.Type _type)
    {
        if (_type == PlayerBuildGirdItem.Type.Type_Head)
        {
            int index = uiIndex + mHeadGridBox.mStartIndex;
            if (index >= mHeadGridBox.mItemCount)
                return;

            if (mHeadGridSelectedItem != null)
                mHeadGridSelectedItem.isSelected = false;

            mHeadGridSelectedItem = mHeadList[uiIndex];
            mHeadGridSelectedItem.isSelected = true;
            HeadGridBoxOnClick(index);
        }

        else if (_type == PlayerBuildGirdItem.Type.Type_Hair)
        {
            int index = uiIndex + mHairGridBox.mStartIndex;
            if (index >= mHairGridBox.mItemCount)
                return;

            if (mHairGridSelectedItem != null)
                mHairGridSelectedItem.isSelected = false;

            mHairGridSelectedItem = mHairList[uiIndex];
            mHairGridSelectedItem.isSelected = true;

            HairGridBoxOnClick(index);
        }

        else if (_type == PlayerBuildGirdItem.Type.Type_Face)
            FaceGridBoxOnClick(uiIndex + mFaceGridBox.mStartIndex);

        else if (_type == PlayerBuildGirdItem.Type.Type_Save)
        {
            int index = uiIndex + mSaveGrodBox.mStartIndex;
            if (index >= mSaveGrodBox.mItemCount)
                return;

            if (mSaveGridSelectedItem != null)
                mSaveGridSelectedItem.isSelected = false;

            mSaveGridSelectedItem = mSaveList[uiIndex];
            mSaveGridSelectedItem.isSelected = true;

            SaveGridBoxOnClick(index);
        }
    }


    void HeadGridBoxOnClick(int index)
    {
        mCurrent.mNude.SetPart(AvatarData.ESlot.Head, mMetaData.GetHead(index).modelPath);
        RebuildModel();
        haschanged = true;
    }
    void HairGridBoxOnClick(int index)
    {
        string[] HairPath = mMetaData.GetHair(index).modelPath;
        mCurrent.mNude.SetPart(AvatarData.ESlot.HairF, HairPath[0]);
        mCurrent.mNude.SetPart(AvatarData.ESlot.HairT, HairPath[1]);
        mCurrent.mNude.SetPart(AvatarData.ESlot.HairB, HairPath[2]);
        RebuildModel();
        haschanged = true;
    }
    void FaceGridBoxOnClick(int index)
    {

    }

    int mSaveGridSelectedIndex = -1;
    void SaveGridBoxOnClick(int index)
    {
        mSaveGridSelectedIndex = index;
    }



    void OnRotLeftBtnDown()
    {
        mRotLeft = true;
    }
    void OnRotLeftBtnUp()
    {
        mRotLeft = false;
    }

    void OnRotRightBtnDown()
    {
        mRotRight = true;
    }
    void OnRotRightBtnUp()
    {
        mRotRight = false;
    }
    #endregion


    bool CheckNeedSaveTip()
    {
        bool needTip = false;
        CustomCharactor.CustomData _data = CreateCustomData();

        List<CustomCharactor.CustomData> DList = CustomDataMgr.Instance.CustomDataList;
        if (DList.Count != 0)
        {
            foreach (CustomCharactor.CustomData item in DList)
            {
                if (CompareCustomData(_data, item))
                {
                    needTip = true;
                    break;
                }
            }
        }
        else
        {
            needTip = true;
        }
        return needTip;
    }

    bool CompareCustomData(CustomCharactor.CustomData _new, CustomCharactor.CustomData _old)
    {
        bool difference = false;

        //lz-2016.10.16 【错误 #4473】空对象
        if (null != _new && null != _new.appearData&&null != _old&&null != _old.appearData)
        {
            AppearData newapp = _new.appearData;
            AppearData oldapp = _old.appearData;
            //color
            if (newapp.mEyeColor != oldapp.mEyeColor)
                difference = true;
            else if (newapp.mLipColor != oldapp.mLipColor)
                difference = true;
            else if (newapp.mSkinColor != oldapp.mSkinColor)
                difference = true;
            else if (newapp.mHairColor != oldapp.mHairColor)
                difference = true;

            //lz-2016.10.16 【错误 #4473】空对象
            if (null != mScrollItemList && mScrollItemList.Count > 0)
            {
                //weight
                for (int i = 0; i < mScrollItemList.Count; i++)
                {
                    float newweight = newapp.GetWeight(mScrollItemList[i].mType);
                    float oldweight = oldapp.GetWeight(mScrollItemList[i].mType);
                    if (!Mathf.Approximately(newweight, oldweight))
                    {
                        difference = true;
                        break;
                    }
                }
            }

            //sex
            if (_new.sex != _old.sex)
                difference = true;
        }

        return difference;
    }
}
