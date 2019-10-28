using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIHostCreateCtrl : MonoBehaviour
{
    public UIServerCtrl mServerCtrl;

    public UIMultiCustomCtrl mMultiCustomCtrl;

    public UIInput mServerName;
    public UIInput mPassword;
    public UILabel mMapName;
    public UICheckbox mPrivateServer;
    public UICheckbox mInfiniteResoureces;
    public UICheckbox mProxyServer;
    public UICheckbox mSkillTreeSystem;
    public UICheckbox mAllScriptsAvailable; //lz-2016.11.07 开启全脚本
    public UIPopupList mBiomePop;
    public UIPopupList mWeatherPop;
    public UIPopupList mGameModePop;
    public UIPopupList mGameTypePop;
    public UILabel mTeamNum;
    public UILabel mPlayerNum;
    public UILabel mDropRateNum;
    public UILabel mSeed;

    public UILabel mMonsterText;
    public UISlicedSprite mMonsterLeftBtnBg;
    public UISlicedSprite mMonsterRightBtnBg;

    public UISlicedSprite mTeamLeftBtnBg;
    public UISlicedSprite mTeamRightBtnBg;
	
    public UIScrollBar mSbTerrainHeight;
    public UIScrollBar mSbMapSzie;
    public UIScrollBar mSbHostilityAllyCount; //lz-2016.10.13 敌对阵营数量
    public UIScrollBar mSbRiverDensity;
    public UIScrollBar mSbRiverWidth;
	public UIScrollBar mSbPlainHeight;
	public UIScrollBar mSbFlatness;
	public UIScrollBar mSbBridgeMaxHeight;
	
    public UILabel mLbTerrainHeightInfo;
    public UILabel mLbMapSizeInfo;
    public UILabel mLbHostilityAllyCount;
    public UILabel mLbRiverDensityInfo;
    public UILabel mLbRiverWidthInfo;
	public UILabel mLbPlainHeight;
	public UILabel mLbFlatness;
	public UILabel mLbBridgeMaxHeight;


    private bool mMonsterYes = false;
    //private bool mMonsterCanChange = false;
    private string mGameTypeText = string.Empty;
    private string mGameModeText = string.Empty;
    private string mUID = string.Empty;

    //lz-2016.10.13 阵营数量等于敌对数量加一（玩家自己）
    private int mAllyCount { get { return (mHostilityAllyCount + 1); } }

    public List<string> mDropRateSelections;

    UIAlphaGroup[] mAlphaGroupsForCustom;
    public UIAlphaGroup[] mAlphaGroupsForStory;
    public BoxCollider[] mCollidersForCustom;
    public BoxCollider[] mCollidersForStory;
    public TweenPosition[] mTPs;

    List<MapConfig> mMapList = new List<MapConfig>();
    int mMapIndex;

    int teamNum;
    int playerNum;
    int mDropRateIndex;

    int mTerrainHeight = 128;
    int mMapSize = 0;
    int mHostilityAllyCount = 3;
    int mRiverDensity = 0;
    int mRiverWidth = 0;

	int plainHeight = 30;
	int flatness =50;
	int bridgeMaxHeight = 0;


    void Awake()
    {
        mAlphaGroupsForCustom = GetComponentsInChildren<UIAlphaGroup>();
    }
    
    void Start ()
    {
        mSbTerrainHeight.scrollValue = 1;
        mSbMapSzie.scrollValue = 0.5f;
        mSbHostilityAllyCount.scrollValue = 1;
        mSbRiverDensity.scrollValue = 0.25f;
        mSbRiverWidth.scrollValue = 0.1f;
        mSbPlainHeight.scrollValue = 0.15f;
        mSbFlatness.scrollValue = 0.25f;
        mSbBridgeMaxHeight.scrollValue = 0f;
        UpdateScrollValueChage();
    }

    public void InitMapInfo()
    {
        mMapList.Clear();
        mMapList.AddRange(MapsConfig.Self.PatheaMapConfig);

        mMapIndex = 0;
        mDropRateIndex = 0;

        teamNum = 1;
        playerNum = 4;


        mGameModePop.items.Clear();
        mGameTypePop.items.Clear();
        mBiomePop.items.Clear();
        mWeatherPop.items.Clear();

        if (mMapList.Count > 0)
        {
            mMapName.text = mMapList[mMapIndex].MapName;
            mGameModePop.items.AddRange(mMapList[mMapIndex].GameMode);
            mGameTypePop.items.AddRange(mMapList[mMapIndex].GameType);
            mBiomePop.items.AddRange(mMapList[mMapIndex].MapTerrainType);
            mWeatherPop.items.AddRange(mMapList[mMapIndex].MapWeatherType);

            mBiomePop.selection = mMapList[mMapIndex].MapTerrainType[0];
            mWeatherPop.selection = mMapList[mMapIndex].MapWeatherType[0];
            mGameModePop.selection = mMapList[mMapIndex].GameMode[0];
            mGameTypePop.selection = mMapList[mMapIndex].GameType[0];

            ResetMapList();

            mMonsterYes = true;
            mMonsterText.text = "YES";
        }
        else
        {
            mMapName.text = "";
            mGameModePop.items.Clear();
            mGameTypePop.items.Clear();
            mBiomePop.items.Clear();
            mWeatherPop.items.Clear();

            mTeamNum.text = "";
            mPlayerNum.text = "";
            mDropRateNum.text = "";
        }
    }

    void UpdateMonsterState()
    {
        float color = 0.3f;

        if (mGameTypeText == "VS" && mGameModeText == "Adventure")
        {
            //mMonsterCanChange = true;
            mMonsterLeftBtnBg.color = new Color(1, 1, 1, 1);
            mMonsterRightBtnBg.color = new Color(1, 1, 1, 1);
            BoxCollider bc = mMonsterLeftBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
            bc.enabled = true;
            BoxCollider bc2 = mMonsterRightBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
            bc2.enabled = true;
        }
        else
        {
            //mMonsterCanChange = false;

            mMonsterLeftBtnBg.color = new Color(color, color, color, 1);
            mMonsterRightBtnBg.color = new Color(color, color, color, 1);
            BoxCollider bc = mMonsterLeftBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
            bc.enabled = false;
            BoxCollider bc2 = mMonsterRightBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
            bc2.enabled = false;

            if (mGameTypeText == "Cooperation" && mGameModeText == "Adventure")
            {
                mMonsterYes = true;
                mMonsterText.text = "YES";
            }
            else
            {
                mMonsterYes = false;
                mMonsterText.text = "NO";
            }
        }
    }

    void ResetMapList()
    {
        switch (Pathea.PeGameMgr.gameType)
        {
            case Pathea.PeGameMgr.EGameType.VS:
                teamNum = 2;
                playerNum = 4;
                mDropRateIndex = 0;
                break;

            case Pathea.PeGameMgr.EGameType.Survive:
                teamNum = 0;
                playerNum = 4;
                mDropRateIndex = 0;
                break;

            case Pathea.PeGameMgr.EGameType.Cooperation:
                teamNum = 1;
                playerNum = 4;
                mDropRateIndex = 0;
                break;

            default:
                teamNum = 1;
                playerNum = 32;
                mDropRateIndex = 0;
                break;
        }

        mTeamNum.text = teamNum.ToString();
        mPlayerNum.text = playerNum.ToString();
        mDropRateNum.text = mDropRateSelections[mDropRateIndex] + "%";
    }

    void OnCancelHostBtn()
    {
        if (Input.GetMouseButtonUp(0))
        {
            mServerCtrl.gameObject.SetActive(true);
            mMultiCustomCtrl.Close();
            StartCoroutine(CloseThisObj(DurTime()));
        }
    }

    float DurTime()
    {
        float dur = 0f;
        foreach (TweenPosition tp in mTPs)
        {
            if (dur < tp.duration)
                dur = tp.duration;
        }
        return dur;
    }

    IEnumerator CloseThisObj(float _wait)
    {
        yield return new WaitForSeconds(_wait + 0.1f);
        gameObject.SetActive(false);
    }

    void OnCreateHostBtn()
    {
        if (string.IsNullOrEmpty(mServerName.text))
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000059));
            return;
        }

        if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Custom)
        {
            if (!mMultiCustomCtrl.HasSelectMap())//没选中地图
                return;

            if (LoadServer.Exist(mServerName.text))
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000485));
                return;
            }
            mMultiCustomCtrl.OnWndStartClick();//检测地图完整性
            StartCoroutine(WhetherMapCheckHasFinished());
            return;
        }

        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            string serverName = mServerName.text;
            //int serverport = uLink.NetworkUtility.FindAvailablePort(9900, 9915);
            string password = mPassword.text;
            string mapName = mMapName.text;
            bool publicServer = !mPrivateServer.isChecked;

            switch (mBiomePop.selection)
            {
                case "Grassland":
                    RandomMapConfig.RandomMapID = RandomMapType.GrassLand;
                    RandomMapConfig.vegetationId = RandomMapType.GrassLand;
                    break;
                case "Forest":
                    RandomMapConfig.RandomMapID = RandomMapType.Forest;
                    RandomMapConfig.vegetationId = RandomMapType.Forest;
                    break;
                case "Desert":
                    RandomMapConfig.RandomMapID = RandomMapType.Desert;
                    RandomMapConfig.vegetationId = RandomMapType.Desert;
                    break;
                case "Redstone":
                    RandomMapConfig.RandomMapID = RandomMapType.Redstone;
                    RandomMapConfig.vegetationId = RandomMapType.Redstone;
                    break;
                case "Rainforest":
                    RandomMapConfig.RandomMapID = RandomMapType.Rainforest;
                    RandomMapConfig.vegetationId = RandomMapType.Rainforest;
                    break;
                case "Mountain":
                    RandomMapConfig.RandomMapID = RandomMapType.Mountain;
                    RandomMapConfig.vegetationId = RandomMapType.Mountain;
                    break;
                case "Swamp":
                    RandomMapConfig.RandomMapID = RandomMapType.Swamp;
                    RandomMapConfig.vegetationId = RandomMapType.Swamp;
                    break;
                case "Crater":
                    RandomMapConfig.RandomMapID = RandomMapType.Crater;
                    RandomMapConfig.vegetationId = RandomMapType.Crater;
                    break;
                default:
                    RandomMapConfig.RandomMapID = RandomMapType.GrassLand;
                    RandomMapConfig.vegetationId = RandomMapType.GrassLand;
                    break;
            }

            switch (mWeatherPop.selection)
            {
                case "Dry":
                    RandomMapConfig.ScenceClimate = ClimateType.CT_Dry;
                    break;
                case "Temperate":
                    RandomMapConfig.ScenceClimate = ClimateType.CT_Temperate;
                    break;
                case "Wet":
                    RandomMapConfig.ScenceClimate = ClimateType.CT_Wet;
                    break;
                case "Random":
                    RandomMapConfig.ScenceClimate = ClimateType.CT_Random;
                    break;
            }

            Pathea.PeGameMgr.monsterYes = mMonsterYes;

            string seedStr = mSeed.text;

            serverName = serverName.Trim();
            if (string.IsNullOrEmpty(serverName))
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000060));
                return;
            }
            if (serverName.Length < 4 || serverName.Length > 19)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000486));
                return;
            }

            if (LoadServer.Exist(serverName))
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000485));
                return;
            }

            bool IsInfiniteResoureces = mInfiniteResoureces.isChecked;
            bool useProxy = mProxyServer.isChecked;
			bool useSkillTree = mSkillTreeSystem.isChecked;
			bool scriptsAvailable = mAllScriptsAvailable.isChecked;

			RandomMapConfig.useSkillTree = useSkillTree;

			MyServer ms = new MyServer();
            ms.gameName = serverName;
            ms.gamePassword = password;
            ms.mapName = mapName;
            ms.gameMode = (int)Pathea.PeGameMgr.sceneMode;
            ms.gameType = (int)Pathea.PeGameMgr.gameType;
            ms.seedStr = seedStr;
            ms.teamNum = teamNum;
            ms.numPerTeam = playerNum;
            ms.dropDeadPercent = Convert.ToInt32(mDropRateSelections[mDropRateIndex]);
            ms.terrainType = (int)RandomMapConfig.RandomMapID;
            ms.vegetationId = (int)RandomMapConfig.vegetationId;
            ms.sceneClimate = (int)RandomMapConfig.ScenceClimate;
            ms.monsterYes = Pathea.PeGameMgr.monsterYes;
            ms.unlimitedRes = IsInfiniteResoureces;
            ms.terrainHeight = mTerrainHeight;
            ms.mapSize = mMapSize;
            ms.riverDensity = mRiverDensity;
            ms.riverWidth = mRiverWidth;
			//a0.95
			ms.plainHeight = plainHeight;
			ms.flatness = flatness;
			ms.bridgeMaxHeight = bridgeMaxHeight;
			//b0.72
			ms.allyCount = mAllyCount;

			ms.scriptsAvailable = scriptsAvailable;
            ms.proxyServer = useProxy;
            ms.isPrivate = !publicServer;
            ms.masterRoleName = GameClientLobby.role.name;
            ms.useSkillTree = useSkillTree;
            ms.uid = "";
            //ms.dropDeadPercent = drop
            MyServerManager.CreateNewServer(ms);
        }
    }

    void OnGameModeChange(string item)
    {
        mGameModeText = item;
        //		UpdateMonsterState();

        if (item.Equals("Adventure"))
        {
            Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Adventure;
            mSkillTreeSystem.isChecked = false;
            mSkillTreeSystem.gameObject.SetActive(true);
            mInfiniteResoureces.isChecked = false;
            mInfiniteResoureces.gameObject.SetActive(true);
            OnExitStoryMode();
            mMultiCustomCtrl.Close();
            OnGameTypeChangs(mGameTypeText);
        }
        else if (item.Equals("Build"))
        {
            Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Build;
            mSkillTreeSystem.isChecked = false;
            mSkillTreeSystem.gameObject.SetActive(false);
            mInfiniteResoureces.isChecked = false;
            mInfiniteResoureces.gameObject.SetActive(true);
            OnExitStoryMode();
            mMultiCustomCtrl.Close();
            OnGameTypeChangs(mGameTypeText);
        }
        else if (item.Equals("Story"))
        {
            Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Story;
            mSkillTreeSystem.isChecked = false;
            mSkillTreeSystem.gameObject.SetActive(false);
            mInfiniteResoureces.isChecked = false;
            mInfiniteResoureces.gameObject.SetActive(false);
            mMonsterYes = true;
            mMonsterText.text = "YES";
            mMultiCustomCtrl.Close();

			string type = mMapList[mMapIndex].GameType[0];
			mGameTypePop.selection = type;
			OnGameTypeChangs(type);

			OnStoryMode();
        }
        else if (item.Equals("Custom"))
        {
            Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;
            mSkillTreeSystem.isChecked = false;
            mSkillTreeSystem.gameObject.SetActive(false);
            mInfiniteResoureces.isChecked = false;
            mInfiniteResoureces.gameObject.SetActive(false);

			string type = mMapList[mMapIndex].GameType[2];
			mGameTypePop.selection = type;
			OnGameTypeChangs(type);

			OnExitStoryMode();
            mMultiCustomCtrl.Open();
            return;
        }

        ResetMapList();
    }

    void OnGameTypeChangs(string item)
    {
        mGameTypeText = item;
		//		UpdateMonsterState();

		if (item == "VS")
        {
            Pathea.PeGameMgr.gameType = Pathea.PeGameMgr.EGameType.VS;
            mTeamLeftBtnBg.color = new Color(1, 1, 1, 1);
            mTeamRightBtnBg.color = new Color(1, 1, 1, 1);
			mAllScriptsAvailable.isChecked = true;
			mAllScriptsAvailable.gameObject.SetActive(true);
		}
        else if (item == "Survival")
        {
            Pathea.PeGameMgr.gameType = Pathea.PeGameMgr.EGameType.Survive;
            float color = 0.3f;
            mTeamLeftBtnBg.color = new Color(color, color, color, 1);
            mTeamRightBtnBg.color = new Color(color, color, color, 1);
			mAllScriptsAvailable.isChecked = true;
			mAllScriptsAvailable.gameObject.SetActive(true);
		}
        else
        {
            Pathea.PeGameMgr.gameType = Pathea.PeGameMgr.EGameType.Cooperation;
            float color = 0.3f;
            mTeamLeftBtnBg.color = new Color(color, color, color, 1);
            mTeamRightBtnBg.color = new Color(color, color, color, 1);
            if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Adventure)
            {
                if (!mAllScriptsAvailable.gameObject.activeInHierarchy)
                {
                    mAllScriptsAvailable.isChecked = true;
                    mAllScriptsAvailable.gameObject.SetActive(true);
                }
            }
            else
            {
                mAllScriptsAvailable.isChecked = false;
                mAllScriptsAvailable.gameObject.SetActive(false);
            }
		}

        ResetMapList();
    }

    void OnMapSelectLeft()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            mMapIndex -= 1;

            if (mMapIndex < 0)
                mMapIndex = mMapList.Count - 1;

            mGameModePop.items.Clear();
            mGameTypePop.items.Clear();

            mBiomePop.items.Clear();
            mWeatherPop.items.Clear();

            mMapName.text = mMapList[mMapIndex].MapName;
            mGameModePop.items.AddRange(mMapList[mMapIndex].GameMode);
            mGameTypePop.items.AddRange(mMapList[mMapIndex].GameType);
            mBiomePop.items.AddRange(mMapList[mMapIndex].MapTerrainType);
            mWeatherPop.items.AddRange(mMapList[mMapIndex].MapWeatherType);

            //			mGameTypePop.selection = mMapList[mMapIndex].GameType[0];
            //			OnGameTypeChangs(mMapList[mMapIndex].GameType[0]);
            //			
            //			mGameModePop.selection = mMapList[mMapIndex].GameMode[0];
            //			OnGameModeChange(mMapList[mMapIndex].GameMode[0]);

            mMonsterYes = false;
            mMonsterText.text = "YES";
        }
    }

    void OnMapSelectRight()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            mMapIndex += 1;

            if (mMapIndex >= mMapList.Count)
                mMapIndex = 0;

            mGameModePop.items.Clear();
            mGameTypePop.items.Clear();
            mBiomePop.items.Clear();
            mWeatherPop.items.Clear();

            mMapName.text = mMapList[mMapIndex].MapName;
            mGameModePop.items.AddRange(mMapList[mMapIndex].GameMode);
            mGameTypePop.items.AddRange(mMapList[mMapIndex].GameType);
            mBiomePop.items.AddRange(mMapList[mMapIndex].MapTerrainType);
            mWeatherPop.items.AddRange(mMapList[mMapIndex].MapWeatherType);


            mMonsterYes = false;
            mMonsterText.text = "YES";
        }
    }

    void OnMonsterSelectLeft()
    {
        //		if(Input.GetMouseButtonUp(0) && mMonsterCanChange)
        if (mGameModeText == "Story")
            return;
        if (Input.GetMouseButtonUp(0))
        {
            if (mMonsterYes)
            {
                mMonsterYes = false;
                mMonsterText.text = "NO";
            }
            else
            {
                mMonsterYes = true;
                mMonsterText.text = "YES";
            }
        }
    }

    void OnPlayerNumSelectLeft()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            if (!Pathea.PeGameMgr.IsSurvive)
            {
                if (teamNum * (playerNum / 2) >= 1)
                {
                    playerNum /= 2;
                    mPlayerNum.text = playerNum.ToString();
                }
            }
            else
            {
                playerNum = Mathf.Max(1, playerNum / 2);
                mPlayerNum.text = playerNum.ToString();
            }
        }
    }

    void OnPlayerNumSelectRight()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            if (!Pathea.PeGameMgr.IsSurvive)
            {
                if (teamNum * (playerNum * 2) <= 32)
                {
                    playerNum *= 2;
                    mPlayerNum.text = playerNum.ToString();
                }
            }
            else
            {
                playerNum = Mathf.Min(32, playerNum * 2);
                mPlayerNum.text = playerNum.ToString();
            }
        }
    }

    void OnTeamNumSelectLeft()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            if (Pathea.PeGameMgr.IsVS)
            {
                if (teamNum >= 3)
                {
                    teamNum--;
                    playerNum = 4;
                    mTeamNum.text = teamNum.ToString();
                    mPlayerNum.text = playerNum.ToString();
                }
            }
        }
    }

    void OnTeamNumSelectRight()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            if (Pathea.PeGameMgr.IsVS)
            {
                if (teamNum <= 3)
                {
                    teamNum++;
                    playerNum = 4;
                    mTeamNum.text = teamNum.ToString();
                    mPlayerNum.text = playerNum.ToString();
                }
            }
        }
    }

    void OnDropRateNumSelectLeft()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            if (--mDropRateIndex < 0)
                mDropRateIndex = mDropRateSelections.Count - 1;
            mDropRateNum.text = mDropRateSelections[mDropRateIndex] + "%";
        }
    }

    void OnDropRateNumSelectRight()
    {
        if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
        {
            if (++mDropRateIndex > mDropRateSelections.Count - 1)
                mDropRateIndex = 0;
            mDropRateNum.text = mDropRateSelections[mDropRateIndex] + "%"; ;
        }
    }

    void OnPrivateActivate()
    {
        if (mPrivateServer.isChecked && mProxyServer.isChecked)
            mProxyServer.isChecked = false;
    }

    void OnProxyActivate()
    {
        if (mProxyServer.isChecked && mPrivateServer.isChecked)
            mPrivateServer.isChecked = false;
    }

    void Update()
    {
        UpdateScrollValueChage();

        //test for UIAlphaGroup
        //if (Input.GetKeyDown(KeyCode.K))
        //    mAlphaGroup.State = 1;
        //else if(Input.GetKeyUp(KeyCode.K))
        //    mAlphaGroup.State = 0;

    }

    //float oldValue = 0;
    void UpdateScrollValueChage()
    {
        float value = mSbTerrainHeight.scrollValue;
        if (value >= 0 && value < 0.33)
        {
            mSbTerrainHeight.scrollValue = 0;
            mTerrainHeight = 128;
            mLbTerrainHeightInfo.text = "128m";
        }
        else if (value >= 0.33 && value < 0.66)
        {
            mSbTerrainHeight.scrollValue = 0.5f;
            mTerrainHeight = 256;
            mLbTerrainHeightInfo.text = "256m";
        }
        else if (value >= 0.66 && value <= 1)
        {
            mSbTerrainHeight.scrollValue = 1;
            mTerrainHeight = 512;
            mLbTerrainHeightInfo.text = "512m";
        }

        value = mSbMapSzie.scrollValue;
        if (value >= 0 && value < 0.125)
        {
            mSbMapSzie.scrollValue = 0;
            mMapSize = 4;
            mLbMapSizeInfo.text = "2km * 2km";
            UpdateHostilityAllyCountRangByMapSize(2);
        }
        else if (value >= 0.125 && value < 0.375)
        {
            mSbMapSzie.scrollValue = 0.25f;
            mMapSize = 3;
            mLbMapSizeInfo.text = "4km * 4km";
            UpdateHostilityAllyCountRangByMapSize(4);
        }
        else if (value >= 0.375 && value < 0.625)
        {
            mSbMapSzie.scrollValue = 0.5f;
            mMapSize = 2;
            mLbMapSizeInfo.text = "8km * 8km";
            UpdateHostilityAllyCountRangByMapSize(8);
        }
        else if (value >= 0.625 && value < 0.875)
        {
            mSbMapSzie.scrollValue = 0.75f;
            mMapSize = 1;
            mLbMapSizeInfo.text = "20km * 20km";
            UpdateHostilityAllyCountRangByMapSize(20);
        }
        else if (value >= 0.875 && value <= 1)
        {
            mSbMapSzie.scrollValue = 1;
            mMapSize = 0;
            mLbMapSizeInfo.text = "40km * 40km";
            UpdateHostilityAllyCountRangByMapSize(40);
        }


        int i_value = Convert.ToInt32(mSbRiverDensity.scrollValue * 100);
        if (i_value < 1)
            i_value = 1;
        mLbRiverDensityInfo.text = i_value.ToString();
        mRiverDensity = i_value;

        i_value = Convert.ToInt32(mSbRiverWidth.scrollValue * 100);
        if (i_value < 1)
            i_value = 1;
        mLbRiverWidthInfo.text = i_value.ToString();
        mRiverWidth = i_value;

		i_value = Convert.ToInt32(mSbPlainHeight.scrollValue * 100);
		if (i_value < 1)
			i_value = 1;
		mLbPlainHeight.text = i_value.ToString();
		plainHeight = i_value;
		
		i_value = Convert.ToInt32(mSbFlatness.scrollValue * 100);
		if (i_value < 1)
			i_value = 1;
		mLbFlatness.text = i_value.ToString();
		flatness = i_value;
		
		i_value = Convert.ToInt32(mSbBridgeMaxHeight.scrollValue * 100);
		mLbBridgeMaxHeight.text = i_value.ToString();
		bridgeMaxHeight = i_value;
    }

    public bool UseAnimation = true;
    public void OnCustomWndOpen()
    {
        foreach (UIAlphaGroup item in mAlphaGroupsForCustom)
        {
            item.State = 1;//暗
        }

        foreach (BoxCollider item in mCollidersForCustom)
        {
            item.enabled = false;
        }
        if (UseAnimation)
            HorizontalMove(true);
    }

    public void OnCustomWndClose()
    {
        foreach (UIAlphaGroup item in mAlphaGroupsForCustom)
        {
            item.State = 0;//亮
        }

        foreach (BoxCollider item in mCollidersForCustom)
        {
            item.enabled = true;
        }
        if (UseAnimation)
            HorizontalMove(false);
    }

    void OnStoryMode()
    {
        foreach (UIAlphaGroup item in mAlphaGroupsForStory)
        {
            item.State = 1;//暗
        }

        foreach (BoxCollider item in mCollidersForStory)
        {
            item.enabled = false;
        }
    }

    void OnExitStoryMode()
    {
        foreach (UIAlphaGroup item in mAlphaGroupsForStory)
        {
            item.State = 0;//亮
        }

        foreach (BoxCollider item in mCollidersForStory)
        {
            item.enabled = true;
        }
    }

    public string UID
    {
        get { return mUID; }
        set { mUID = value; }
    }

    IEnumerator WhetherMapCheckHasFinished()
    {
        while (true)
        {
            yield return 0;
            if (mMultiCustomCtrl.Integrity == true)
            {
                CreateCustomServer();
                break;
            }
            else if (mMultiCustomCtrl.Integrity == false)
            {
                break;
            }
        }
    }

    void HorizontalMove(bool _direction)
    {
        foreach (TweenPosition tp in mTPs)
        {
            tp.Play(_direction);
        }
    }

    void CreateCustomServer()
    {
        Pathea.CustomGameData.Mgr.Instance.curGameData = Pathea.CustomGameData.Mgr.Instance.GetCustomData(UID);
        if (null == Pathea.CustomGameData.Mgr.Instance.curGameData)
            return;

        string serverName = mServerName.text;
        if (LoadServer.Exist(serverName))
            return;

        Pathea.PeGameMgr.gameName = Pathea.CustomGameData.Mgr.Instance.curGameData.name;
        Pathea.PeGameMgr.monsterYes = mMonsterYes;
        Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;

        MyServer ms = new MyServer();
        ms.gameName = serverName;
        ms.gamePassword = mPassword.text;
        ms.gameMode = (int)Pathea.PeGameMgr.sceneMode;
        ms.masterRoleName = GameClientLobby.role.name;
        ms.mapName = Pathea.PeGameMgr.gameName;
        ms.seedStr = mSeed.text;
        ms.uid = UID;

        ms.unlimitedRes = mInfiniteResoureces.isChecked;
        ms.proxyServer = mProxyServer.isChecked;
        ms.isPrivate = mPrivateServer.isChecked;
        ms.useSkillTree = RandomMapConfig.useSkillTree = mSkillTreeSystem.isChecked;
		ms.scriptsAvailable = mAllScriptsAvailable.isChecked;

        MyServerManager.StartCustomServer(ms);
    }

    ///<summary>lz-2016.10.13 通过地图大小更新敌对阵营的数量范围</summary>
    void UpdateHostilityAllyCountRangByMapSize(int mapSize)
    {
        mSbHostilityAllyCount.enabled = true;
        float scrollValue = mSbHostilityAllyCount.scrollValue;
        switch (mapSize)
        {
            case 2:
                mHostilityAllyCount = 3;
                mSbHostilityAllyCount.scrollValue = 1;
                break;
            case 4:
                if (scrollValue <= 0.1f)
                {
                    mHostilityAllyCount = 3;
                    mSbHostilityAllyCount.scrollValue = 0f;
                }
                else if (scrollValue <= 0.51f)
                {
                    mHostilityAllyCount = 4;
                    mSbHostilityAllyCount.scrollValue = 0.5f;
                }
                else
                {
                    mHostilityAllyCount = 5;
                    mSbHostilityAllyCount.scrollValue = 1;
                }
                break;
            case 8:
                if (scrollValue <= 0.1f)
                {
                    mHostilityAllyCount = 3;
                    mSbHostilityAllyCount.scrollValue = 0f;
                }
                else if (scrollValue <= 0.34f)
                {
                    mHostilityAllyCount = 4;
                    mSbHostilityAllyCount.scrollValue = 0.33f;
                }
                else if (scrollValue <= 0.67f)
                {
                    mHostilityAllyCount = 5;
                    mSbHostilityAllyCount.scrollValue = 0.66f;
                }
                else
                {
                    mHostilityAllyCount = 6;
                    mSbHostilityAllyCount.scrollValue = 1f;
                }
                break;
            case 20:
            case 40:
                if (scrollValue <= 0.1f)
                {
                    mHostilityAllyCount = 3;
                    mSbHostilityAllyCount.scrollValue = 0f;
                }
                else if (scrollValue <= 0.26f)
                {
                    mHostilityAllyCount = 4;
                    mSbHostilityAllyCount.scrollValue = 0.25f;
                }
                else if (scrollValue <= 0.51f)
                {
                    mHostilityAllyCount = 5;
                    mSbHostilityAllyCount.scrollValue = 0.5f;
                }
                else if (scrollValue <= 0.76f)
                {
                    mHostilityAllyCount = 6;
                    mSbHostilityAllyCount.scrollValue = 0.75f;
                }
                else
                {
                    mHostilityAllyCount = 7;
                    mSbHostilityAllyCount.scrollValue = 1f;
                }
                break;
        }
        mLbHostilityAllyCount.text = mHostilityAllyCount.ToString();
    }
}
