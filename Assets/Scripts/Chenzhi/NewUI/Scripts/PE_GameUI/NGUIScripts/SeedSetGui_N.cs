using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SeedSetGui_N : UIStaticWnd
{
    public class LocalKeyValue
    {
        public int Key { get; private set; }
        public string Value { get; private set; }
        public LocalKeyValue(int key)
        {
            Key = key;
            Value = PELocalization.GetString(key);
        }
    }

    static SeedSetGui_N mInstance;
    public static SeedSetGui_N Instance { get { return mInstance; } }
    public UIInput mSeedNumIP;
    public UIPopupList mBiomePL;
    public UIPopupList mWeatherPL;

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

    public UICheckbox mCbUseSkillTree;
    public GameObject mUseSkillRoot;

    public UICheckbox mOpenAllScripts;
    public GameObject mOpenAllScriptsRoot;

    //lz-2016.08.02 地形和天气的下拉列表框的本地化键和值
    private List<LocalKeyValue> m_BiomeItemList = new List<LocalKeyValue>();
    private List<LocalKeyValue> m_WeatherItemList = new List<LocalKeyValue>();
    //lz-2016.10.13 阵营数量等于敌对数量加一（玩家自己）
    private int mAllyCount { get { return (mHostilityAllyCount + 1); } }

    int mTerrainHeight = 512;
    int mMapSize = 2;
    int mHostilityAllyCount = 3;
    int mRiverDensity = 5;
    int mRiverWidth = 10;

    int plainHeight = 5;//1-100
    int flatness = 50;//1-100
    int bridgeMaxHeight = 20;//0-100

    void Awake()
    {
        mInstance = this;

        //lz-2016.08.02  地形和天气改为翻译映射
        m_BiomeItemList.Add(new LocalKeyValue(8000609));
        m_BiomeItemList.Add(new LocalKeyValue(8000610));
        m_BiomeItemList.Add(new LocalKeyValue(8000611));
        m_BiomeItemList.Add(new LocalKeyValue(8000612));
        m_BiomeItemList.Add(new LocalKeyValue(8000613));
        m_BiomeItemList.Add(new LocalKeyValue(8000614));
        m_BiomeItemList.Add(new LocalKeyValue(8000615));
        m_BiomeItemList.Add(new LocalKeyValue(8000616));

        mBiomePL.items.Clear();
        for (int i = 0; i < m_BiomeItemList.Count; i++)
        {
            mBiomePL.items.Add(m_BiomeItemList[i].Value);
        }

        mSeedNumIP.text = "Planet Maria";

        m_WeatherItemList.Add(new LocalKeyValue(8000617));
        m_WeatherItemList.Add(new LocalKeyValue(8000618));
        m_WeatherItemList.Add(new LocalKeyValue(8000619));
        m_WeatherItemList.Add(new LocalKeyValue(8000620));

        mWeatherPL.items.Clear();
        for (int i = 0; i < m_WeatherItemList.Count; i++)
        {
            mWeatherPL.items.Add(m_WeatherItemList[i].Value);
        }

        if (Pathea.PeGameMgr.IsAdventure)
        {
            mUseSkillRoot.SetActive(true);
            mOpenAllScriptsRoot.SetActive(true);
        }
        else
        {
            mUseSkillRoot.SetActive(false);
            mOpenAllScriptsRoot.SetActive(false);
        }
    }

    void Start()
    {
        mSbTerrainHeight.scrollValue = 1;
        mSbMapSzie.scrollValue = 0.5f;
        mSbHostilityAllyCount.scrollValue = 1f;
        mSbRiverDensity.scrollValue = 0.25f;
        mSbRiverWidth.scrollValue = 0.1f;
        mSbPlainHeight.scrollValue = 0.15f;
        mSbFlatness.scrollValue = 0.25f;
        mSbBridgeMaxHeight.scrollValue = 0;
        mWeatherPL.textLabel.text = m_WeatherItemList[1].Value;
        mBiomePL.textLabel.text = m_BiomeItemList[0].Value;
        UpdateScrollValueChage();
    }

    void SeedSetEnd()
    {
        LocalKeyValue biomeLocalkv = m_BiomeItemList.Find(kv => kv.Value == mBiomePL.textLabel.text);
        if (null == biomeLocalkv) return;
        switch (biomeLocalkv.Key)
        {
            case 8000609:
                RandomMapConfig.RandomMapID = RandomMapType.GrassLand;
                RandomMapConfig.vegetationId = RandomMapType.GrassLand;
                break;
            case 8000610:
                RandomMapConfig.RandomMapID = RandomMapType.Forest;
                RandomMapConfig.vegetationId = RandomMapType.Forest;
                break;
            case 8000611:
                RandomMapConfig.RandomMapID = RandomMapType.Desert;
                RandomMapConfig.vegetationId = RandomMapType.Desert;
                break;
            case 8000612:
                RandomMapConfig.RandomMapID = RandomMapType.Redstone;
                RandomMapConfig.vegetationId = RandomMapType.Redstone;
                break;
            case 8000613:
                RandomMapConfig.RandomMapID = RandomMapType.Rainforest;
                RandomMapConfig.vegetationId = RandomMapType.Rainforest;
                break;
            //lz-2016.08.02 添加三种新地形选项
            case 8000614:
                RandomMapConfig.RandomMapID = RandomMapType.Mountain;
                RandomMapConfig.vegetationId = RandomMapType.Mountain;
                break;
            case 8000615:
                RandomMapConfig.RandomMapID = RandomMapType.Swamp;
                RandomMapConfig.vegetationId = RandomMapType.Swamp;
                break;
            case 8000616:
                RandomMapConfig.RandomMapID = RandomMapType.Crater;
                RandomMapConfig.vegetationId = RandomMapType.Crater;
                break;
        }

        LocalKeyValue weatherLocalkv = m_WeatherItemList.Find(kv => kv.Value == mWeatherPL.textLabel.text);
        if (null == weatherLocalkv) return;
        switch (weatherLocalkv.Key)
        {
            case 8000617:
                RandomMapConfig.ScenceClimate = ClimateType.CT_Dry;
                break;
            case 8000618:
                RandomMapConfig.ScenceClimate = ClimateType.CT_Temperate;
                break;
            case 8000619:
                RandomMapConfig.ScenceClimate = ClimateType.CT_Wet;
                break;
            case 8000620:
                RandomMapConfig.ScenceClimate = ClimateType.CT_Random;
                break;
        }

        //WeatherConfig.SetClimateType(RandomMapConfig.ScenceClimate,RandomMapConfig.RandomMapID);
        RandomMapConfig.SeedString = mSeedNumIP.text;
        int seed = 0;
        char[] charG = mSeedNumIP.text.ToCharArray();
        for (int i = 0; i < charG.Length; i++)
            seed += (i + 1) * (int)charG[i];

        RandomMapConfig.RandSeed = Convert.ToInt32(seed);
        RandomMapConfig.TerrainHeight = mTerrainHeight;
        RandomMapConfig.mapSize = mMapSize;
        RandomMapConfig.allyCount = mAllyCount; 
        RandomMapConfig.riverDensity = mRiverDensity;
        RandomMapConfig.riverWidth = mRiverWidth;
        RandomMapConfig.plainHeight = plainHeight;
        RandomMapConfig.flatness = flatness;
        RandomMapConfig.bridgeMaxHeight = bridgeMaxHeight;

        if (mUseSkillRoot.activeSelf)
            RandomMapConfig.useSkillTree = mCbUseSkillTree.isChecked;
        else
            RandomMapConfig.useSkillTree = false;

        if (mOpenAllScriptsRoot.activeSelf)
            RandomMapConfig.openAllScripts = mOpenAllScripts.isChecked;
        else
            RandomMapConfig.openAllScripts = false;
        RandomMapConfig.InitTownAreaPara();
        PeSceneCtrl.Instance.GotoGameSence();
    }

    void OnOkBtn()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SeedSetEnd();
        }
    }

    void OnCancelBtn()
    {
        if (Input.GetMouseButtonUp(0))
            Hide();
    }

    void OnRandomBtn()
    {
        int rand = UnityEngine.Random.Range(0, m_BiomeItemList.Count - 1);
        mBiomePL.selection = m_BiomeItemList[rand].Value;

        rand = UnityEngine.Random.Range(0, m_WeatherItemList.Count - 1);
        mWeatherPL.selection = m_WeatherItemList[rand].Value;

        //		rand = UnityEngine.Random.Range(1,2);
        //		mSizePL.textLabel.text = (rand*256).ToString() + "X" + (rand*256).ToString();
        mSeedNumIP.text = UnityEngine.Random.Range(0, 1000000).ToString();

        mSbTerrainHeight.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbMapSzie.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbHostilityAllyCount.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbRiverDensity.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbRiverWidth.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbPlainHeight.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbFlatness.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100;
        mSbBridgeMaxHeight.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(0, 100)) / 100;
        UpdateScrollValueChage();
    }

    void OnSeedSubmit(string text)
    {
        SeedSetEnd();
    }




    void Update()
    {
        UpdateScrollValueChage();
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
