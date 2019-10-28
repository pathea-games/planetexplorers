using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5
using AntialiasingAsPostEffect = UnityStandardAssets.ImageEffects.Antialiasing;
#endif

public class UIOption : UIStaticWnd
{
    static UIOption mInstance;
    public static UIOption Instance { get { return mInstance; } }

    public UIStaticWnd mParentWnd = null;

    public GameObject mVideoWnd;
    public GameObject mAudioWnd;
    public GameObject mShortCutsWnd;
    public GameObject mMiscWnd;
    public GameObject mControlWnd;

    public GameObject mDefaultBtn;

    public UICheckbox mVideoBtn;
    public UICheckbox mAudioBtn;
    public UICheckbox mShortCutsBtn;

    public UIScrollBar mKeyScrollBar;

    public OptionItem_N mLightCountItem;
    public OptionItem_N mAnisotropicFilteringItem;
    public OptionItem_N mAntiAliasingItem;
    public OptionItem_N mShadowProjectionItem;
    public OptionItem_N mShadowDistanceItem;
    public OptionItem_N mShadowCascadesItem;
    public OptionItem_N mTerrainLevel;
    public OptionItem_N mRandomTerrainLevel;
    public OptionItem_N mTreeLevel;
    public OptionItem_N mGrassDensity;
    public OptionItem_N mGrassDistance;
    public OptionItem_N mWaterReflection;
    public OptionItem_N mWaterRefraction;
    public OptionItem_N mWaterDepth;
    public OptionItem_N mSSAO;
    public OptionItem_N mSyncCount;
    public OptionItem_N mDepthBlur;
    public OptionItem_N mHDR;
    public OptionItem_N mLightShadows;

    public ScrollItem mQuality;
    public ScrollItem mSoundVolume;
    public ScrollItem mMusicVolume;
    public ScrollItem mDialogVolume;
    public ScrollItem mEffectVolume;

    //	public UICheckbox	mCamCtrlMode;

    #region Control_Wnd
    public ScrollItem mHoldGunCameraSensitivity;
    public ScrollItem mCameraSensitivity;
    public UILabel mCamS;
    public UILabel mHoldGunCamS;
    public UILabel mCamSMin;
    public UILabel mCamSMax;

    float CamSMin = 0.3f;
    float CamSMax = 4f;

    public ScrollItem mCameraFOVScroll;
    public UILabel mCameraFOV;
    public UILabel mCameraFOVMin;
    public UILabel mCameraFOVMax;

    float CamFOVMin = 20f;
    float CamFOVMax = 90f;


    public ScrollItem mCameraInertia;
    public UILabel mCamInertiaValue;
    public UILabel mCamInertiaMin;
    public UILabel mCamInertiaMax;

    float CamInertiaMin = 0f;
    float CamInertiaMax = 10f;

    public ScrollItem DriveCameraInertiaScroll;
    public UILabel DriveCamInertiaValueLabel;
    public UILabel DriveCamInertiaMinLabel;
    public UILabel DriveCamInertiaMaxLabel;

    float m_DriveCamInertiaMin = 0f;
    float m_DriveCamInertiaMax = 50f;

    public UICheckbox mCamHorizontal;
    public UICheckbox mCamVertical;
    public UICheckbox mAttacMode;


    #endregion

    #region MISC_WND
    public UICheckbox mHideHeadgear;

    public UICheckbox mHPNumbers;

    public UICheckbox mLockCursor;

    public UICheckbox mMonsterIK;

    public UICheckbox mVoxelCache;

    public UICheckbox mFixFontBlurry;

    public UICheckbox mAndyGuidance;

    public UICheckbox mMouseStateTip;

    public UICheckbox mUseController;

    public UICheckbox mHidePlayerOverHeadInfo;
    #endregion

    //	public UICheckbox	mDepthBlur;
    //	
    //	public UICheckbox	mSSAO;
    //	
    //	public UICheckbox	mSyncCount;

    public static int[][] DefaultIndex = { new int[] { 0, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0 } };

    public float SoundVolume = 1f;
    public float MusicVolume = 1f;
    public float DialogVolume = 1f;
    public float EffectVolume = 1f;

    public Action VolumeChangeEvent; //lz-2016.12.16 音量改变事件


    int mTabIndex = 0;

    public UIGrid mKeysSetGrid;

    public KeySettingItem mPerfab;
    public KeyCategoryItem mCategoryPrefab;

    KeySettingItem[][] mKeySettingLists = new KeySettingItem[(int)EPeInputSettingsType.Max][];

    public enum KeyCategory
    {
        Common = 10179,
        Character = 10180,
        Construct = 10181,
        Carrier = 10182
    }

    int LastQualityLevel = 6;

    void Awake()
    {
        mInstance = this;
        SoundVolume = SystemSettingData.Instance.SoundVolume;
        MusicVolume = SystemSettingData.Instance.MusicVolume;
        DialogVolume = SystemSettingData.Instance.DialogVolume;
        EffectVolume = SystemSettingData.Instance.EffectVolume;
        mCamSMin.text = CamSMin.ToString();
        mCamSMax.text = CamSMax.ToString();
        mCameraFOVMin.text = CamFOVMin.ToString();
        mCameraFOVMax.text = CamFOVMax.ToString();
        mCamInertiaMin.text = CamInertiaMin.ToString();
        mCamInertiaMax.text = CamInertiaMax.ToString();

        OnVideoBtn();
    }

    public void HideWindow()
    {
        Hide();
        SystemSettingData.Instance.SoundVolume = SoundVolume;
        SystemSettingData.Instance.MusicVolume = MusicVolume;
        SystemSettingData.Instance.DialogVolume = DialogVolume;
        SystemSettingData.Instance.EffectVolume = EffectVolume;
        if (mParentWnd != null)
        {
            mParentWnd.Show();
            mParentWnd = null;
        }
    }

    public void OnVideoBtn()
    {
        mTabIndex = 0;
        mDefaultBtn.SetActive(false);
        mVideoWnd.SetActive(true);
        mAudioWnd.SetActive(false);
        mShortCutsWnd.SetActive(false);
        mMiscWnd.SetActive(false);
        mControlWnd.SetActive(false);
        LastQualityLevel = SystemSettingData.Instance.mQualityLevel;
        ResetVideo(LastQualityLevel);
        mVideoBtn.isChecked = true;

        SystemSettingData.Instance.SoundVolume = SoundVolume;
        SystemSettingData.Instance.MusicVolume = MusicVolume;
        SystemSettingData.Instance.DialogVolume = DialogVolume;
        SystemSettingData.Instance.EffectVolume = EffectVolume;
    }

    void OnAudioBrn()
    {
        if (mTabIndex != 1)
        {
            mTabIndex = 1;
            mDefaultBtn.SetActive(false);
            mAudioWnd.SetActive(true);
            mVideoWnd.SetActive(false);
            mShortCutsWnd.SetActive(false);
            mMiscWnd.SetActive(false);
            mControlWnd.SetActive(false);
            ResetAudio();
        }
    }

    void OnShortCutsBtn()
    {
        if (mTabIndex != 2)
        {
            mTabIndex = 2;
            mDefaultBtn.SetActive(true);
            mShortCutsWnd.SetActive(true);
            mVideoWnd.SetActive(false);
            mAudioWnd.SetActive(false);
            mMiscWnd.SetActive(false);
            mControlWnd.SetActive(false);
            ResetkeySetting();
            SystemSettingData.Instance.SoundVolume = SoundVolume;
            SystemSettingData.Instance.MusicVolume = MusicVolume;
            SystemSettingData.Instance.DialogVolume = DialogVolume;
            SystemSettingData.Instance.EffectVolume = EffectVolume;
        }
    }

    void OnControlBtn()
    {
        if (mTabIndex != 3)
        {
            mTabIndex = 3;
            mDefaultBtn.SetActive(false);
            mControlWnd.SetActive(true);
            mShortCutsWnd.SetActive(false);
            mVideoWnd.SetActive(false);
            mAudioWnd.SetActive(false);
            mMiscWnd.SetActive(false);
            ResetControl();
            SystemSettingData.Instance.SoundVolume = SoundVolume;
            SystemSettingData.Instance.MusicVolume = MusicVolume;
            SystemSettingData.Instance.DialogVolume = DialogVolume;
            SystemSettingData.Instance.EffectVolume = EffectVolume;
        }
    }

    void OnMiscBtn()
    {
        if (mTabIndex != 4)
        {
            mTabIndex = 4;
            mDefaultBtn.SetActive(false);
            mMiscWnd.SetActive(true);
            mShortCutsWnd.SetActive(false);
            mVideoWnd.SetActive(false);
            mAudioWnd.SetActive(false);
            mControlWnd.SetActive(false);
            ResetMisc();
            SystemSettingData.Instance.SoundVolume = SoundVolume;
            SystemSettingData.Instance.MusicVolume = MusicVolume;
            SystemSettingData.Instance.DialogVolume = DialogVolume;
            SystemSettingData.Instance.EffectVolume = EffectVolume;
        }
    }

    void OnClearVoxelCacheBtnClick()
    {
        VFDataRTGenFileCache.ClearAllCache();
        MessageBox_N.ShowOkBox(PELocalization.GetString(8000913));
    }

    void OnCtrlType0()
    {
        //		mCamCtrlMode.isChecked = true;
    }

    void OnCtrlType1()
    {
        //		mCamCtrlMode.isChecked = false;
    }

    void OnApplyBtn()
    {
        switch (mTabIndex)
        {
            case 0:
                SystemSettingData.Instance.mQualityLevel = mQuality.mIndex;
                SystemSettingData.Instance.mLightCount = mLightCountItem.mIndex;
                SystemSettingData.Instance.mAnisotropicFiltering = mAnisotropicFilteringItem.mIndex;
                SystemSettingData.Instance.mAntiAliasing = mAntiAliasingItem.mIndex;
                SystemSettingData.Instance.mShadowProjection = mShadowProjectionItem.mIndex;
                SystemSettingData.Instance.mShadowDistance = mShadowDistanceItem.mIndex;
                SystemSettingData.Instance.mShadowCascades = mShadowCascadesItem.mIndex;
                SystemSettingData.Instance.mWaterReflection = mWaterReflection.mIndex;
                SystemSettingData.Instance.WaterRefraction = (mWaterRefraction.mIndex == 1);
                SystemSettingData.Instance.WaterDepth = (mWaterDepth.mIndex == 1);
                SystemSettingData.Instance.TerrainLevel = (byte)mTerrainLevel.mIndex;
                SystemSettingData.Instance.RandomTerrainLevel = (byte)mRandomTerrainLevel.mIndex;
				SystemSettingData.Instance.mTreeLevel = mTreeLevel.mIndex + 1;

                AntialiasingAsPostEffect aap = Camera.main.GetComponent<AntialiasingAsPostEffect>();
                if (null != aap)
                    aap.enabled = mAntiAliasingItem.mIndex > 0;


                SystemSettingData.Instance.GrassDensity = 1f * mGrassDensity.mIndex / (mGrassDensity.mSelections.Count - 1);
                //			SystemSettingData.Instance.GrassLod = GrassGlobalSettings.ELodType.LOD_1_TYPE_1
                //				+ (int)((GrassGlobalSettings.ELodType.Max - GrassGlobalSettings.ELodType.LOD_1_TYPE_1 - 2) * 1f * mGrassDistance.mIndex / (mGrassDistance.mSelections.Count - 1));
                //SystemSettingData.Instance.GrassLod = RedGrass.ELodType.LOD_1_TYPE_1
                //    + (int)((RedGrass.ELodType.Max - RedGrass.ELodType.LOD_1_TYPE_1 - 2) * 1f * mGrassDistance.mIndex / (mGrassDistance.mSelections.Count - 1));

                //			SystemSettingData.Instance.mDepthBlur = mDepthBlur.isChecked;
                //			SystemSettingData.Instance.mSSAO = mSSAO.isChecked;
                //			SystemSettingData.Instance.SyncCount = mSyncCount.isChecked;
                SystemSettingData.Instance.mGrassLod = ConvertIndexToGrassLod(mGrassDistance.mIndex);
                SystemSettingData.Instance.mDepthBlur = (mDepthBlur.mIndex == 1);
                SystemSettingData.Instance.mSSAO = (mSSAO.mIndex == 1);
                SystemSettingData.Instance.SyncCount = (mSyncCount.mIndex == 1);
                SystemSettingData.Instance.HDREffect = (mHDR.mIndex == 1);

                SystemSettingData.Instance.mFastLightingMode = !(mLightShadows.mIndex == 1);

                SystemSettingData.Instance.ApplyVideo();

                break;
            case 1:
                SoundVolume = SystemSettingData.Instance.SoundVolume;
                MusicVolume = SystemSettingData.Instance.MusicVolume;
                DialogVolume = SystemSettingData.Instance.DialogVolume;
                EffectVolume = SystemSettingData.Instance.EffectVolume;
                SystemSettingData.Instance.ApplyAudio();
                if (null!=VolumeChangeEvent)
                    VolumeChangeEvent();
                break;
            case 2:
                for (int i = 0; i < PeInput.SettingsAll.Length; i++)
                {
                    for (int j = 0; j < PeInput.SettingsAll[i].Length; j++)
                    {
                        PeInput.SettingsAll[i][j].Clone(mKeySettingLists[i][j]._keySetting);
                    }
                }
                SystemSettingData.Instance.ApplyKeySetting();

                if (UIMenuList.Instance != null)
                    UIMenuList.Instance.RefreshHotKeyName();
                break;
            case 3:
                SystemSettingData.Instance.cameraSensitivity = CamSMin + (CamSMax - CamSMin) * mCameraSensitivity.mScroll.scrollValue;
                SystemSettingData.Instance.holdGunCameraSensitivity = CamSMin + (CamSMax - CamSMin) * mHoldGunCameraSensitivity.mScroll.scrollValue;
                SystemSettingData.Instance.CameraFov = CamFOVMin + (CamFOVMax - CamFOVMin) * mCameraFOVScroll.mScroll.scrollValue;
                SystemSettingData.Instance.CamInertia = CamInertiaMin + (CamInertiaMax - CamInertiaMin) * mCameraInertia.mScroll.scrollValue;
                SystemSettingData.Instance.DriveCamInertia = this.m_DriveCamInertiaMin + (this.m_DriveCamInertiaMax - this.m_DriveCamInertiaMin) * this.DriveCameraInertiaScroll.mScroll.scrollValue;
                SystemSettingData.Instance.CameraHorizontalInverse = mCamHorizontal.isChecked;
                SystemSettingData.Instance.CameraVerticalInverse = mCamVertical.isChecked;
                SystemSettingData.Instance.AttackWhithMouseDir = mAttacMode.isChecked;
                SystemSettingData.Instance.UseController = mUseController.isChecked;
                SystemSettingData.Instance.ApplyControl();
                break;
            case 4:
                SystemSettingData.Instance.HideHeadgear = mHideHeadgear.isChecked;
                SystemSettingData.Instance.HPNumbers = mHPNumbers.isChecked;
                SystemSettingData.Instance.ClipCursor = mLockCursor.isChecked;
                SystemSettingData.Instance.ApplyMonsterIK = mMonsterIK.isChecked;
                SystemSettingData.Instance.VoxelCacheEnabled = mVoxelCache.isChecked;
                SystemSettingData.Instance.FixBlurryFont = mFixFontBlurry.isChecked;
                SystemSettingData.Instance.AndyGuidance = mAndyGuidance.isChecked;
                SystemSettingData.Instance.MouseStateTip = mMouseStateTip.isChecked;
                SystemSettingData.Instance.HidePlayerOverHeadInfo = mHidePlayerOverHeadInfo.isChecked;
                SystemSettingData.Instance.ApplyMisc();
                break;
        }
    }

    //lz-2016.07.28 因为吴怡秋把草的距离等级判定改为三个了，所以这里加入草的距离等级和选项下标互相转换的方法
    int ConvertGrassLodToIndex(RedGrass.ELodType loadType)
    {
        switch (loadType)
        {
            case RedGrass.ELodType.LOD_1_TYPE_1:
                return 0;
            case RedGrass.ELodType.LOD_2_TYPE_2:
                return 1;
            case RedGrass.ELodType.LOD_3_TYPE_1:
                return 2;
            default:
                return 2;
        }
    }

    RedGrass.ELodType ConvertIndexToGrassLod(int index)
    {
        switch (index)
        {
            case 0:
                return RedGrass.ELodType.LOD_1_TYPE_1;
            case 1:
                return RedGrass.ELodType.LOD_2_TYPE_2;
            case 2:
                return RedGrass.ELodType.LOD_3_TYPE_1;
            default:
                return RedGrass.ELodType.LOD_3_TYPE_1;
        }
    }

    void OnOkbtn()
    {
        OnApplyBtn();
        OnCanncelBtn();
    }

    void OnCanncelBtn()
    {
        OnVideoBtn();
        HideWindow();
    }

    void OnDefaultBtn()
    {
        int _count = mKeysSetGrid.transform.childCount;
        for (int i = 0; i < _count; i++)
        {
            Destroy(mKeysSetGrid.transform.GetChild(i).gameObject);
        }

        this.StartCoroutine(this.ResetInterator());
    }

    IEnumerator ResetInterator()
    {
        //lz-2016.06.07:Invoke方法会受TimeScale影响，而在游戏中，打开系统菜单的时候TimeScale会被调整为0，暂停游戏，所有这里改为协程，不受TimeScale影响
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < 0.2f)
        {
            yield return null;
        }
        PeInput.ResetSetting();
        mKeySettingLists[0] = null;
        mKeySettingLists[1] = null;
        mKeySettingLists[2] = null;
        mKeySettingLists[3] = null;
        ResetkeySetting();

        if (UIMenuList.Instance != null)
            UIMenuList.Instance.RefreshHotKeyName();
    }

    void ResetVideo(int qualityLevel, bool fromUpdate = false)
    {
        mQuality.SetIndex(qualityLevel);
        switch (qualityLevel)
        {
            case 0:
                mLightCountItem.SetIndex(0);
                mAnisotropicFilteringItem.SetIndex(0);
                mAntiAliasingItem.SetIndex(0);
                mShadowProjectionItem.SetIndex(0);
                mShadowDistanceItem.SetIndex(0);
                mShadowCascadesItem.SetIndex(0);
                mWaterReflection.SetIndex(0);
                mWaterRefraction.SetIndex(0);
                mWaterDepth.SetIndex(0);
                mTerrainLevel.SetIndex(1);//256 instead of 128
                mRandomTerrainLevel.SetIndex(0);
                mTreeLevel.SetIndex(0);
                mGrassDensity.SetIndex(0);
                mGrassDistance.SetIndex(0);
                mSSAO.SetIndex(0);
                mSyncCount.SetIndex(0);
                mDepthBlur.SetIndex(0);
                mHDR.SetIndex(0);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
            case 1:
                mLightCountItem.SetIndex(5);
                mAnisotropicFilteringItem.SetIndex(0);
                mAntiAliasingItem.SetIndex(1);
                mShadowProjectionItem.SetIndex(0);
                mShadowDistanceItem.SetIndex(1);
                mShadowCascadesItem.SetIndex(0);
                mWaterReflection.SetIndex(0);
                mWaterRefraction.SetIndex(0);
                mWaterDepth.SetIndex(0);
				mTerrainLevel.SetIndex(1);//256 instead of 128
                mRandomTerrainLevel.SetIndex(0);
                mTreeLevel.SetIndex(1);
                mGrassDensity.SetIndex(1);
                mGrassDistance.SetIndex(0);
                mSSAO.SetIndex(0);
                mSyncCount.SetIndex(0);
                mDepthBlur.SetIndex(0);
                mHDR.SetIndex(0);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
            case 2:
                mLightCountItem.SetIndex(10);
                mAnisotropicFilteringItem.SetIndex(0);
                mAntiAliasingItem.SetIndex(1);
                mShadowProjectionItem.SetIndex(0);
                mShadowDistanceItem.SetIndex(2);
                mShadowCascadesItem.SetIndex(1);
                mWaterReflection.SetIndex(1);
                mWaterRefraction.SetIndex(1);
                mWaterDepth.SetIndex(1);
				mTerrainLevel.SetIndex(1);//256 instead of 128
                mRandomTerrainLevel.SetIndex(1);
                mTreeLevel.SetIndex(1);
                mGrassDensity.SetIndex(2);
                mGrassDistance.SetIndex(1);
                mSSAO.SetIndex(0);
                mSyncCount.SetIndex(0);
                mDepthBlur.SetIndex(1);
                mHDR.SetIndex(0);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
            case 3:
                mLightCountItem.SetIndex(20);
                mAnisotropicFilteringItem.SetIndex(1);
                mAntiAliasingItem.SetIndex(2);
                mShadowProjectionItem.SetIndex(1);
                mShadowDistanceItem.SetIndex(2);
                mShadowCascadesItem.SetIndex(1);
                mWaterReflection.SetIndex(1);
                mWaterRefraction.SetIndex(1);
                mWaterDepth.SetIndex(1);
				mTerrainLevel.SetIndex(1);//256 instead of 128
                mRandomTerrainLevel.SetIndex(1);
                mTreeLevel.SetIndex(2);
                mGrassDensity.SetIndex(3);
                mGrassDistance.SetIndex(1);
                mSSAO.SetIndex(0);
                mSyncCount.SetIndex(0);
                mDepthBlur.SetIndex(1);
                mHDR.SetIndex(0);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
            case 4:
                mLightCountItem.SetIndex(50);
                mAnisotropicFilteringItem.SetIndex(2);
                mAntiAliasingItem.SetIndex(3);
                mShadowProjectionItem.SetIndex(1);
                mShadowDistanceItem.SetIndex(3);
                mShadowCascadesItem.SetIndex(2);
                mWaterReflection.SetIndex(2);
                mWaterRefraction.SetIndex(1);
                mWaterDepth.SetIndex(1);
				mTerrainLevel.SetIndex(2);
                mRandomTerrainLevel.SetIndex(2);
                mTreeLevel.SetIndex(3);
                mGrassDensity.SetIndex(4);
                mGrassDistance.SetIndex(2);
                mSSAO.SetIndex(0);
                mSyncCount.SetIndex(0);
                mDepthBlur.SetIndex(1);
                mHDR.SetIndex(0);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
            case 5:
                mLightCountItem.SetIndex(50);
                mAnisotropicFilteringItem.SetIndex(2);
                mAntiAliasingItem.SetIndex(3);
                mShadowProjectionItem.SetIndex(1);
                mShadowDistanceItem.SetIndex(4);
                mShadowCascadesItem.SetIndex(2);
                mWaterReflection.SetIndex(2);
                mWaterRefraction.SetIndex(1);
                mWaterDepth.SetIndex(1);
                mTerrainLevel.SetIndex(3);
                mRandomTerrainLevel.SetIndex(2);
                mTreeLevel.SetIndex(4);
                mGrassDensity.SetIndex(4);
                mGrassDistance.SetIndex(2);
                mSSAO.SetIndex(0);
                mSyncCount.SetIndex(1);
                mDepthBlur.SetIndex(1);
                mHDR.SetIndex(1);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
            case 6:
                mLightCountItem.SetIndex(SystemSettingData.Instance.mLightCount);
                mAnisotropicFilteringItem.SetIndex(SystemSettingData.Instance.mAnisotropicFiltering);
                mAntiAliasingItem.SetIndex(SystemSettingData.Instance.mAntiAliasing);
                mShadowProjectionItem.SetIndex(SystemSettingData.Instance.mShadowProjection);
                mShadowDistanceItem.SetIndex(SystemSettingData.Instance.mShadowDistance);
                mShadowCascadesItem.SetIndex(SystemSettingData.Instance.mShadowCascades);
                mWaterReflection.SetIndex(SystemSettingData.Instance.mWaterReflection);
                mWaterRefraction.SetIndex(SystemSettingData.Instance.WaterRefraction ? 1 : 0);
                mWaterDepth.SetIndex(SystemSettingData.Instance.WaterDepth ? 1 : 0);
                mTerrainLevel.SetIndex(SystemSettingData.Instance.TerrainLevel);
                mRandomTerrainLevel.SetIndex(SystemSettingData.Instance.RandomTerrainLevel);
				mTreeLevel.SetIndex(SystemSettingData.Instance.mTreeLevel - 1);
                mGrassDensity.SetIndex((int)(SystemSettingData.Instance.GrassDensity * (mGrassDensity.mSelections.Count - 1)));
                mGrassDistance.SetIndex(ConvertGrassLodToIndex(SystemSettingData.Instance.mGrassLod));
                mSSAO.SetIndex(SystemSettingData.Instance.mSSAO ? 1 : 0);
                mSyncCount.SetIndex(SystemSettingData.Instance.SyncCount ? 1 : 0);
                mDepthBlur.SetIndex(SystemSettingData.Instance.mDepthBlur ? 1 : 0);
                mHDR.SetIndex(SystemSettingData.Instance.HDREffect ? 1 : 0);
                mLightShadows.SetIndex(SystemSettingData.Instance.mFastLightingMode ? 0 : 1);
                break;
        }
    }

    void ResetAudio()
    {
        mSoundVolume.SetValue(SystemSettingData.Instance.SoundVolume);
        mMusicVolume.SetValue(SystemSettingData.Instance.MusicVolume);
        mDialogVolume.SetValue(SystemSettingData.Instance.DialogVolume);
        mEffectVolume.SetValue(SystemSettingData.Instance.EffectVolume);

        SoundVolume = SystemSettingData.Instance.SoundVolume;
        MusicVolume = SystemSettingData.Instance.MusicVolume;
        DialogVolume = SystemSettingData.Instance.DialogVolume;
        EffectVolume = SystemSettingData.Instance.EffectVolume;
    }

    void ResetkeySetting()
    {
        CreatKeyList(ref mKeySettingLists[0], PeInput.SettingsAll[0].Length, KeyCategory.Common);
        CreatKeyList(ref mKeySettingLists[1], PeInput.SettingsAll[1].Length, KeyCategory.Character);
        CreatKeyList(ref mKeySettingLists[2], PeInput.SettingsAll[2].Length, KeyCategory.Construct);
        CreatKeyList(ref mKeySettingLists[3], PeInput.SettingsAll[3].Length, KeyCategory.Carrier);

        mKeysSetGrid.Reposition();
        //if (mKeySettingList_Common == null)
        //{
        //    KeyCategoryItem cateitem = Instantiate(mCategoryPrefab) as KeyCategoryItem;
        //    cateitem.transform.parent = mKeysSetGrid.transform;
        //    cateitem.transform.localScale = Vector3.one;
        //    cateitem.transform.localPosition = Vector3.zero;
        //    cateitem.transform.localRotation = Quaternion.identity;
        //    cateitem.mStringID =(int) KeyCategory.Common;

        //    mKeySettingList_Common = new KeySettingItem[Common];
        //    for (int i = 0; i < Common; i++)
        //    {
        //        KeySettingItem addItem = Instantiate(mPerfab) as KeySettingItem;
        //        addItem.transform.parent = mKeysSetGrid.transform;
        //        addItem.transform.localScale = Vector3.one;
        //        addItem.transform.localPosition = Vector3.zero;
        //        addItem.transform.localRotation = Quaternion.identity;
        //        addItem.gameObject.name = "KeySetting" + i;
        //        addItem._keySettingName = PeInput.IndexToString(i);
        //        addItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.Settings[i]);
        //        mKeySettingList_Common[i] = addItem;
        //    }       
        //}
        //else
        //{
        //    for (int i = 0; i < Common; i++)
        //    {
        //        mKeySettingList_Common[i]._keySetting = new PeInput.KeyJoySettingPair(PeInput.Settings[i]);
        //    }
        //}
    }


    void CreatKeyList(ref KeySettingItem[] _array, int _count, KeyCategory _keycate)
    {
        if (_array == null)
        {
            KeyCategoryItem cateitem = Instantiate(mCategoryPrefab) as KeyCategoryItem;
            cateitem.transform.parent = mKeysSetGrid.transform;
            cateitem.transform.localScale = Vector3.one;
            cateitem.transform.localPosition = Vector3.zero;
            cateitem.transform.localRotation = Quaternion.identity;
            cateitem.mStringID = (int)_keycate;

            _array = new KeySettingItem[_count];

            for (int i = 0; i < _count; i++)
            {
                KeySettingItem addItem = Instantiate(mPerfab) as KeySettingItem;
                addItem.transform.parent = mKeysSetGrid.transform;
                addItem.transform.localScale = Vector3.one;
                addItem.transform.localPosition = Vector3.zero;
                addItem.transform.localRotation = Quaternion.identity;
                addItem.gameObject.name = "KeySetting" + i;
                switch (_keycate)
                {
                    case KeyCategory.Common:
                        addItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][i]);
                        addItem._keySettingName = PeInput.StrIdOfGeneral(i);
                        break;
                    case KeyCategory.Character:
                        addItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[1][i]);
                        addItem._keySettingName = PeInput.StrIdOfChrCtrl(i);
                        break;
                    case KeyCategory.Construct:
                        addItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[2][i]);
                        addItem._keySettingName = PeInput.StrIdOfBuildMd(i);
                        break;
                    case KeyCategory.Carrier:
                        addItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[3][i]);
                        addItem._keySettingName = PeInput.StrIdOfVehicle(i);
                        break;
                }
                _array[i] = addItem;
            }
        }
        else
        {
            for (int i = 0; i < _count; i++)
            {
                switch (_keycate)
                {
                    case KeyCategory.Common:
                        _array[i]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][i]);
                        _array[i]._keySettingName = PeInput.StrIdOfGeneral(i);
                        break;
                    case KeyCategory.Character:
                        _array[i]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[1][i]);
                        _array[i]._keySettingName = PeInput.StrIdOfChrCtrl(i);
                        break;
                    case KeyCategory.Construct:
                        _array[i]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[2][i]);
                        _array[i]._keySettingName = PeInput.StrIdOfBuildMd(i);
                        break;
                    case KeyCategory.Carrier:
                        _array[i]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[3][i]);
                        _array[i]._keySettingName = PeInput.StrIdOfVehicle(i);
                        break;
                }
            }
        }
    }


    void ResetControl()
    {
        mCameraSensitivity.SetValue((SystemSettingData.Instance.cameraSensitivity - CamSMin) / (CamSMax - CamSMin));
        mHoldGunCameraSensitivity.SetValue((SystemSettingData.Instance.holdGunCameraSensitivity - CamSMin) / (CamSMax - CamSMin));
        mCameraFOVScroll.SetValue((SystemSettingData.Instance.CameraFov - CamFOVMin) / (CamFOVMax - CamFOVMin));
        this.DriveCameraInertiaScroll.SetValue((SystemSettingData.Instance.DriveCamInertia - m_DriveCamInertiaMin) / (m_DriveCamInertiaMax - m_DriveCamInertiaMin));
        mCameraInertia.SetValue((SystemSettingData.Instance.CamInertia - CamInertiaMin) / (CamInertiaMax - CamInertiaMin));
        mCamHorizontal.isChecked = SystemSettingData.Instance.CameraHorizontalInverse;
        mCamVertical.isChecked = SystemSettingData.Instance.CameraVerticalInverse;
        mAttacMode.isChecked = SystemSettingData.Instance.AttackWhithMouseDir;
        mUseController.isChecked = SystemSettingData.Instance.UseController;
    }

    void ResetMisc()
    {
        //		mCameraSensitivity.SetValue((SystemSettingData.Instance.CameraSensitivity - CamSMin)/(CamSMax - CamSMin));
        //		mCameraFOVScroll.SetValue((SystemSettingData.Instance.CameraFov - CamFOVMin)/(CamFOVMax - CamFOVMin));
        //		mCamHorizontal.isChecked = SystemSettingData.Instance.CameraHorizontalInverse;
        //		mCamVertical.isChecked = SystemSettingData.Instance.CameraVerticalInverse;
        mHideHeadgear.isChecked = SystemSettingData.Instance.HideHeadgear;
        mHPNumbers.isChecked = SystemSettingData.Instance.HPNumbers;
        mLockCursor.isChecked = SystemSettingData.Instance.ClipCursor;
        mMonsterIK.isChecked = SystemSettingData.Instance.ApplyMonsterIK;
        //		mSyncCount.isChecked = SystemSettingData.Instance.SyncCount;
        mVoxelCache.isChecked = SystemSettingData.Instance.VoxelCacheEnabled;
        mFixFontBlurry.isChecked = SystemSettingData.Instance.FixBlurryFont;
        mAndyGuidance.isChecked =SystemSettingData.Instance.AndyGuidance;
        mMouseStateTip.isChecked= SystemSettingData.Instance.MouseStateTip;
        mHidePlayerOverHeadInfo.isChecked=SystemSettingData.Instance.HidePlayerOverHeadInfo;

        //		mDepthBlur.isChecked = SystemSettingData.Instance.mDepthBlur;
        //		mSSAO.isChecked = SystemSettingData.Instance.mSSAO;
    }

    protected override void OnClose()
    {
        OnCanncelBtn();
    }

    void LateUpdate()
    {
        if (LastQualityLevel != mQuality.mIndex)
        {
            LastQualityLevel = mQuality.mIndex;
            ResetVideo(LastQualityLevel, true);
        }

        switch (mTabIndex)
        {
            case 1:
                SystemSettingData.Instance.SoundVolume = mSoundVolume.mScroll.scrollValue;
                SystemSettingData.Instance.MusicVolume = mMusicVolume.mScroll.scrollValue;
                SystemSettingData.Instance.DialogVolume = mDialogVolume.mScroll.scrollValue;
                SystemSettingData.Instance.EffectVolume = mEffectVolume.mScroll.scrollValue;
                break;
        }
        //		if(mWndCenter.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        //			OnCanncelBtn();

        if (mWaterDepth.mIndex == 0)
            mWaterRefraction.SetIndex(0);
    }

    public void OnChange()
    {
        if (mTabIndex == 0)
        {
            mQuality.SetIndex(6);
            LastQualityLevel = mQuality.mIndex;
        }
    }

    // return: succeed or not;
    int[] tmpIdx = new int[4];
    bool TrySetKeyInCommon(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor, int conflictMask = -1)
    {
        tmpIdx[0] = accessor.FindInArray(mKeySettingLists[0], newKey);
        if (tmpIdx[0] < 0) return false;	// Can not set because locked

        KeyCode oldKey = accessor.Get(itemToSet);
        if (tmpIdx[0] > 0)
        {
            accessor.Set(mKeySettingLists[0][tmpIdx[0] - 1], oldKey);
        }
        else
        {
            for (int i = 1; i < 4; i++)
            {
                if (((1 << i) & conflictMask) == 0) { tmpIdx[i] = 0; continue; }
                tmpIdx[i] = accessor.FindInArray(mKeySettingLists[i], newKey);
                if (tmpIdx[i] < 0) return false;	// Can not set because locked
            }
            for (int i = 1; i < 4; i++)
            {
                if (tmpIdx[i] > 0)
                {
                    accessor.Set(mKeySettingLists[i][tmpIdx[i] - 1], oldKey);
                }
            }
        }
        accessor.Set(itemToSet, newKey);
        return true;
    }
    bool TrySetKeyInOther(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor, int cur)
    {
        tmpIdx[cur] = accessor.FindInArray(mKeySettingLists[cur], newKey);
        if (tmpIdx[cur] < 0) return false;	// Can not set because locked

        KeyCode oldKey = accessor.Get(itemToSet);
        if (tmpIdx[cur] > 0)
        {
            accessor.Set(mKeySettingLists[cur][tmpIdx[cur] - 1], oldKey);
        }
        else
        {
            tmpIdx[0] = accessor.FindInArray(mKeySettingLists[0], newKey);
            if (tmpIdx[0] < 0) return false;	// Can not set because locked
            if (tmpIdx[0] > 0)
            {
                if (!TrySetKeyInCommon(mKeySettingLists[0][tmpIdx[0] - 1], oldKey, accessor, (-1 & ~(1 << cur))))
                    return false;
            }
        }
        accessor.Set(itemToSet, newKey);
        return true;
    }
	public bool TrySetKey(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor)
	{
		// Special for common settings
		if (0 <= System.Array.IndexOf(mKeySettingLists[0], itemToSet))
		{
			return TrySetKeyInCommon(itemToSet, newKey, accessor, -1);
		}
		for (int i = 1; i < 4; i++)
		{
			if (0 <= System.Array.IndexOf(mKeySettingLists[i], itemToSet))
			{
				return TrySetKeyInOther(itemToSet, newKey, accessor, i);
			}
		}
		return false;
	}

	public KeySettingItem TestConflict(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor)
    {
		int[] masks = new int[]{0xff, 0x03, 0x05, 0x09, 0x11, 0x21};
		int n = mKeySettingLists.Length;
		int idx = 0;
		for (int i = 0; i < n; i++) {
			if (0 <= System.Array.IndexOf(mKeySettingLists[i], itemToSet)){
				for(int j = 0; j < n; j++){
					if((masks[i]&(1<<j))!= 0 && 0 != (idx = accessor.FindInArray(mKeySettingLists[j], newKey))){
						if(idx < 0)	idx = -idx;
						return mKeySettingLists[j][idx-1];
					}
				}
			}
		}
		return null;
    }

    void Update()
    {
        mCamS.text = ((int)((CamSMin + (CamSMax - CamSMin) * mCameraSensitivity.mScroll.scrollValue) * 10) / 10f).ToString();
        mHoldGunCamS.text = ((int)((CamSMin + (CamSMax - CamSMin) * mHoldGunCameraSensitivity.mScroll.scrollValue) * 10) / 10f).ToString();
        mCameraFOV.text = ((int)(CamFOVMin + (CamFOVMax - CamFOVMin) * mCameraFOVScroll.mScroll.scrollValue)).ToString();
        mCamInertiaValue.text = ((int)((CamInertiaMin + (CamInertiaMax - CamInertiaMin) * mCameraInertia.mScroll.scrollValue) * 100) / 100f).ToString();
        this.DriveCamInertiaValueLabel.text = ((int)((m_DriveCamInertiaMin + (m_DriveCamInertiaMax - m_DriveCamInertiaMin) * DriveCameraInertiaScroll.mScroll.scrollValue) * 100) / 100f).ToString();
    }
}
