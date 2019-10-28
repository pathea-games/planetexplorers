using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class SystemSettingData 
{
	static SystemSettingData mInstance;
	public static SystemSettingData Instance
	{
		get
		{
			if(mInstance == null)
			{
				mInstance = new SystemSettingData();
//				mInstance.LoadSystemData();
			}
			return mInstance;
		}
	}
	public static void Save()
	{
		if(null != Instance && Instance.dataDirty)
			Instance.ApplySettings();
	}

	public bool dataDirty;
	
	public bool	mHasData = false;
	
	public string	mVersion = "0.795";

    public string mLanguage = "chinese";

	int _bLangChinese = -1;	//0: not chinese; >0: chinese; <0: not defined
	public bool IsChinese{
		get{
			if(_bLangChinese < 0){
				_bLangChinese = (mLanguage.Contains("chinese") || mLanguage.Contains("\u4e2d\u6587")) ? 1 : 0;
			}
			return _bLangChinese != 0;
		}
	}

    public byte TerrainLevel = 3;
    public byte RandomTerrainLevel = 0;
	public int mTreeLevel = 3;
	public int treeLevel
	{
		get
		{
			if((TerrainLevel <= 1 && Pathea.PeGameMgr.IsStory)
			   || (RandomTerrainLevel <= 1 && Pathea.PeGameMgr.randomMap))
				return 1;
			return mTreeLevel;
		}
	}

#if Win32Ver
	public int	mQualityLevel = 0;
#else
	public int	mQualityLevel = 6;
#endif
    public bool VoxelCacheEnabled = true;
	public int	mLightCount = 20;
	public int	mAnisotropicFiltering = 2;
	public int	mAntiAliasing = 1;
	public int	mShadowProjection = 1;
	public int	mShadowDistance = 2;
	public int	mShadowCascades = 2;

	int mWaterReflection_ = 2;
	public int	mWaterReflection{
		get{	return mWaterReflection_;	}
		set{	mWaterReflection_ = WaterReflection.ReflectionSetting = value;	}
	}
	
	bool mWaterRefraction = true;
	public bool WaterRefraction
	{
		get{return mWaterRefraction; }
		set{mWaterRefraction = value; 
			if(VFVoxelWater.self != null) VFVoxelWater.self.ApplyQuality(WaterRefraction, WaterDepth);
		}
	}
	
	bool mWaterDepth = true;
	public bool WaterDepth
	{
		get{return mWaterDepth; }
		set{mWaterDepth = value; 
			if(!mWaterDepth) WaterRefraction = false; 
			if(VFVoxelWater.self != null) VFVoxelWater.self.ApplyQuality(WaterRefraction, WaterDepth);
		}
	}
	
	public float GrassDensity = 0.6f;
	public RedGrass.ELodType mGrassLod = RedGrass.ELodType.LOD_3_TYPE_1;
	public RedGrass.ELodType GrassLod
	{
		get
		{
			if(Pathea.PeGameMgr.randomMap ? RandomTerrainLevel <= 1 : TerrainLevel <= 1)
				return RedGrass.ELodType.LOD_1_TYPE_1;
			return mGrassLod;
		}
		set	{ mGrassLod = value; }
	}
	
	public bool mDepthBlur = true;
	public bool mSSAO = false;
	
	public float SoundVolume = 1f;
	public float MusicVolume = 1f;
	public float DialogVolume = 1f;
	public float EffectVolume = 1f;

    public float cameraSensitivity = 1.5f;             //普通相机灵敏度
    public float holdGunCameraSensitivity = 1.5f;      //持枪相机灵敏度       lz-2018.01.18

    public bool holdGun = false;
    public float CameraSensitivity
    {
        get
        {
            return holdGun ? holdGunCameraSensitivity : cameraSensitivity;
        }
    }

	public float CameraFov = 60f;
	
	public bool	 CameraHorizontalInverse = false;
	public bool	 CameraVerticalInverse = false;
	
	public bool  mMMOControlType = true;

	public bool UseController = true;

	public bool FirstPersonCtrl = false;

	public bool AttackWhithMouseDir = false;

    public bool Tutorialed = false;
	
	public bool	 HideHeadgear = false;
	
	public bool	 HPNumbers = false;
	
	public bool  ClipCursor = false;
	
	public bool	 ApplyMonsterIK = false;
	
	public bool	 SyncCount = true; // default: true
	public void ResetVSync()
	{
		if (Pathea.PeFlowMgr.Instance.curScene == Pathea.PeFlowMgr.EPeScene.GameScene) {			
			QualitySettings.vSyncCount = SyncCount ? 1 : 0;
		} else {
			QualitySettings.vSyncCount = 1;
		}
	}

	public bool HDREffect = true;

	public float AbsEffectVolume	{		get {return SoundVolume * EffectVolume;}	}
	public float AbsMusicVolume		{		get {return SoundVolume * MusicVolume;}		}
	public float AbsDialogVolume	{		get {return SoundVolume * DialogVolume;}	}

	bool _fastLightingMode = true;
	public bool mFastLightingMode{
		get{ return _fastLightingMode;				}
		set{
			_fastLightingMode = value;
			LightMgr.Instance.SetLightMode (value);	}
	}

	public float CamInertia = 0f;

    //log:lz-2016.05.18 功能 #1924 在Options内增加载具的摄像机跟随灵敏度调整功能
    public float DriveCamInertia = 50f;
	
	public string GLSetting = "test2";
	
	bool LoadedData = false;
	
	string mFilepath = "";

    public bool FixBlurryFont = false;

    //lz-2016.08.24 增加Andy游戏引导开关
    public bool AndyGuidance = true;

    //lz-2016.08.26 增加鼠标状态提示
    public bool MouseStateTip = true;

    //lz-2016.09.26 增加隐藏玩家头顶的名字血条等（多人模式玩家自己用）
    public bool HidePlayerOverHeadInfo=false;

    public SystemSettingData()
	{
		dataDirty = false;
	}
	
	public void ApplyAudio()
	{
		XmlDocument xmlDoc = new XmlDocument();
		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}
		} catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			xmlDoc = new XmlDocument();
		}
		
		XmlElement findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Sound");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("Sound");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Volume",SoundVolume.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Music");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("Music");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Volume",MusicVolume.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Dialog");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("Dialog");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Volume",DialogVolume.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Effect");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("Effect");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Volume",EffectVolume.ToString());

		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				xmlDoc.Save (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
		}
	}
	
	public void ApplyVideo()
	{
		XmlDocument xmlDoc = new XmlDocument();
		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			xmlDoc = new XmlDocument();
		}
		
		XmlElement findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("QualityLevel");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("QualityLevel");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mQualityLevel.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GLSetting");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("GLSetting");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",GLSetting);
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("LightCount");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("LightCount");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mLightCount.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AnisotropicFiltering");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("AnisotropicFiltering");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mAnisotropicFiltering.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AntiAliasing");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("AntiAliasing");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mAntiAliasing.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowProjection");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("ShadowProjection");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mShadowProjection.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowDistance");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("ShadowDistance");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mShadowDistance.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowCascades");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("ShadowCascades");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mShadowCascades.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("mWaterReflection");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("mWaterReflection");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",mWaterReflection.ToString());
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("WaterRefraction");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("WaterRefraction");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",WaterRefraction.ToString());
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("WaterDepth");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("WaterDepth");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",WaterDepth.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GrassDensity");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("GrassDensity");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",GrassDensity.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GrassLod");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("GrassLod");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",((int)mGrassLod).ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Terrain");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("Terrain");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
        findNode.SetAttribute("Index", TerrainLevel.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("RandomTerrain");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("RandomTerrain");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index", RandomTerrainLevel.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("DepthBlur");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("DepthBlur");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",mDepthBlur.ToString());

		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("SSAO");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("SSAO");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",mSSAO.ToString());

		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("SyncCount");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("SyncCount");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",SyncCount.ToString());
		ResetVSync ();
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Tree");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("Tree");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index", mTreeLevel.ToString());
		
//		GrassMgr.RefreshSettings(GrassDensity, (int)GrassLod);

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HDREffect");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("HDREffect");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("Index",HDREffect.ToString());

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FastLightingMode");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("FastLightingMode");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		//mFastLightingMode = true;
		findNode.SetAttribute("Index",mFastLightingMode.ToString());	

		switch(mQualityLevel)
		{
		case 0:
			QualitySettings.SetQualityLevel(0);
#if Win32Ver
			TerrainLevel = 0;
#else
			TerrainLevel = 1; //increase from 128M to 256M for bug 502857
#endif
			RandomTerrainLevel = 0;
			mTreeLevel = 1;
			HDREffect = false;
			break;
		case 1:
			QualitySettings.SetQualityLevel(1);
			TerrainLevel = 1; //increase from 128M to 256M for bug 502857
			RandomTerrainLevel = 0;
			mTreeLevel = 1;
			HDREffect = false;
			break;
		case 2:
			QualitySettings.SetQualityLevel(2);
			TerrainLevel = 1;
			RandomTerrainLevel = 1;
			mTreeLevel = 1;
			HDREffect = false;
			break;
		case 3:
			QualitySettings.SetQualityLevel(3);
			TerrainLevel = 2;
			RandomTerrainLevel = 1;
			mTreeLevel = 3;
			HDREffect = true;
			break;
		case 4:
			QualitySettings.SetQualityLevel(4);
			TerrainLevel = 3;
			RandomTerrainLevel = 2;
			mTreeLevel = 4;
			HDREffect = true;
			break;
		case 5:
			QualitySettings.SetQualityLevel(5);
			TerrainLevel = 3;
			RandomTerrainLevel = 2;
			mTreeLevel = 5;
			HDREffect = true;
			break;
		case 6:
			QualitySettings.SetQualityLevel(3);	// for Ubuntu with AMD card, larger than 3 will cause game select GL_FBCONFIG 47/55 which can not render defered-render cam
			QualitySettings.pixelLightCount = mLightCount;
			switch(mAnisotropicFiltering)
			{
			case 0:
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				break;
			case 1:
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
				break;
			case 2:
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				break;
			}
			
			QualitySettings.antiAliasing = (mAntiAliasing > 0)? 4 : 0;
			
			QualitySettings.shadowProjection = (mShadowProjection==1)? ShadowProjection.StableFit :ShadowProjection.CloseFit;
			
			switch(mShadowDistance)
			{
			case 0:
				QualitySettings.shadowDistance = 1;
				break;
			case 1:
				QualitySettings.shadowDistance = 50;
				break;
			case 2:
				QualitySettings.shadowDistance = 100;
				break;
			case 3:
				QualitySettings.shadowDistance = 200;
				break;
			case 4:
				QualitySettings.shadowDistance = 400;
				break;
			}
			
			switch(mShadowCascades)
			{
			case 0:
				QualitySettings.shadowCascades = 0;
				break;
			case 1:
				QualitySettings.shadowCascades = 2;
				break;
			case 2:
				QualitySettings.shadowCascades = 4;
				break;
			}
			break;
		}
		
		PeGrassSystem.Refresh(GrassDensity, (int)GrassLod);
		PECameraMan.ApplySysSetting();

		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				xmlDoc.Save (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
		}
	}
	
	public void ApplyKeySetting()
	{
		PeInput.SaveInputConfig(mFilepath);
	}

	public void ApplyControl()
	{
		XmlDocument xmlDoc = new XmlDocument();
		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			xmlDoc = new XmlDocument();
		}
		
		XmlElement findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("mMMOControlType");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("mMMOControlType");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",mMMOControlType.ToString());

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("UseController");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("UseController");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value", UseController.ToString());


		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FirstPersonCtrl");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("FirstPersonCtrl");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value", FirstPersonCtrl.ToString());

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraSensitivity");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("CameraSensitivity");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",cameraSensitivity.ToString());
		
        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HoldGunCameraSensitivity");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("HoldGunCameraSensitivity");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", holdGunCameraSensitivity.ToString());

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraFov");
        if (null == findNode)
		{
			findNode = xmlDoc.CreateElement("CameraFov");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",CameraFov.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraHorizontalInverse");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("CameraHorizontalInverse");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",CameraHorizontalInverse.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraVerticalInverse");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("CameraVerticalInverse");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",CameraVerticalInverse.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CamInertia");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("CamInertia");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value", CamInertia.ToString());

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("DriveCamInertia");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("DriveCamInertia");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", DriveCamInertia.ToString());

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AttackWhithMouseDir");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("AttackWhithMouseDir");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value", AttackWhithMouseDir.ToString());

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Tutorialed");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("Tutorialed");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", Tutorialed.ToString());

		PECameraMan.ApplySysSetting();

		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				xmlDoc.Save (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
		}
	}
	
	public void ApplyMisc()
	{
		XmlDocument xmlDoc = new XmlDocument();
		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			xmlDoc = new XmlDocument();
		}
		
		XmlElement findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HideHeadgear");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("HideHeadgear");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",HideHeadgear.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HPNumbers");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("HPNumbers");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",HPNumbers.ToString());
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ClipCursor");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("ClipCursor");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",ClipCursor.ToString());

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FixBlurryFont");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("FixBlurryFont");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", FixBlurryFont.ToString());
        if (GameUI.Instance != null)
        {
            UILabel[] labels = GameUI.Instance.gameObject.GetComponentsInChildren<UILabel>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].MakePixelPerfect();
            }
        }

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AndyGuidance");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("AndyGuidance");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", AndyGuidance.ToString());


        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("MouseStateTip");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("MouseStateTip");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", MouseStateTip.ToString());
        //#if UNITY_STANDALONE_WIN
        //        if(ClipCursor)
        //            CursorCliping.Lock();
        //        else
        //            CursorCliping.Unlock();
        //#endif

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HidePlayerOverHeadInfo");
        if (null == findNode)
        {
            findNode = xmlDoc.CreateElement("HidePlayerOverHeadInfo");
            xmlDoc.DocumentElement.AppendChild(findNode);
        }
        findNode.SetAttribute("value", HidePlayerOverHeadInfo.ToString());

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ApplyMonsterIK");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("ApplyMonsterIK");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value",ApplyMonsterIK.ToString());

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("VoxelCache");
		if(null == findNode)
		{
			findNode = xmlDoc.CreateElement("VoxelCache");
			xmlDoc.DocumentElement.AppendChild(findNode);
		}
		findNode.SetAttribute("value", VoxelCacheEnabled.ToString());		
			
		PECameraMan.ApplySysSetting();

		try{
			using (FileStream fs = new FileStream (mFilepath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				xmlDoc.Save (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
		}		
	}
	
	public void ApplySettings()
	{
		ApplyAudio();
		ApplyVideo();
		ApplyKeySetting();
		ApplyControl();
		ApplyMisc();
		dataDirty = false;
	}
	
	void SetSystemData(XmlDocument xmlDoc)
	{
		//Video
//		XmlNodeList nodelist = xmlDoc.DocumentElement.ChildNodes;
		XmlElement findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("QualityLevel");
		if(null != findNode)
			mQualityLevel = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("LightCount");
		if(null != findNode)
			mLightCount = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AnisotropicFiltering");
		if(null != findNode)
			mAnisotropicFiltering = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AntiAliasing");
		if(null != findNode)
			mAntiAliasing = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowProjection");
		if(null != findNode)
			mShadowProjection = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowDistance");
		if(null != findNode)
			mShadowDistance = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowCascades");
		if(null != findNode)
			mShadowCascades = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("mWaterReflection");
		if(null != findNode)
			mWaterReflection = Convert.ToInt32(findNode.GetAttribute("Index"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("WaterRefraction");
		if(null != findNode)
			WaterRefraction = Convert.ToBoolean(findNode.GetAttribute("Index"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("WaterDepth");
		if(null != findNode)
			WaterDepth = Convert.ToBoolean(findNode.GetAttribute("Index"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GrassDensity");
		if(null != findNode)
			GrassDensity = Convert.ToSingle(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GrassLod");
		if(null != findNode)
			mGrassLod = (RedGrass.ELodType)Convert.ToInt32(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Terrain");
		if(null != findNode)
			TerrainLevel = Convert.ToByte(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("RandomTerrain");
		if(null != findNode)
			RandomTerrainLevel = Convert.ToByte(findNode.GetAttribute("Index"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Tree");
		if(null != findNode)
			mTreeLevel = Convert.ToInt32(findNode.GetAttribute("Index"));

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HDREffect");
		if(null != findNode)
			HDREffect = Convert.ToBoolean(findNode.GetAttribute("Index"));

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FastLightingMode");
		if(null != findNode)
			mFastLightingMode = Convert.ToBoolean(findNode.GetAttribute("Index"));

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Sound");
		if(null != findNode)
				SoundVolume = Convert.ToSingle(findNode.GetAttribute("Volume"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Music");
		if(null != findNode)
				MusicVolume = Convert.ToSingle(findNode.GetAttribute("Volume"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Dialog");
		if(null != findNode)
				DialogVolume = Convert.ToSingle(findNode.GetAttribute("Volume"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Effect");
		if(null != findNode)
				EffectVolume = Convert.ToSingle(findNode.GetAttribute("Volume"));
		
		if(xmlDoc.DocumentElement.HasAttribute("Version")
				 && xmlDoc.DocumentElement.GetAttribute("Version") == mVersion)
		{
			findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("mMMOControlType");
			if(null != findNode)
				mMMOControlType = Convert.ToBoolean(findNode.GetAttribute("value"));
		}
		else
		{
			mMMOControlType = false;
		}

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("UseController");
		if(null != findNode)
			UseController = Convert.ToBoolean(findNode.GetAttribute("value"));

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FirstPersonCtrl");
		if(null != findNode)
			FirstPersonCtrl = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraSensitivity");
		if(null != findNode)
			cameraSensitivity = Convert.ToSingle(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HoldGunCameraSensitivity");
        if (null != findNode)
            holdGunCameraSensitivity = Convert.ToSingle(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraFov");
		if(null != findNode)
			CameraFov = Convert.ToSingle(findNode.GetAttribute("value"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraHorizontalInverse");
		if(null != findNode)
			CameraHorizontalInverse = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraVerticalInverse");
		if(null != findNode)
			CameraVerticalInverse = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HideHeadgear");
		if(null != findNode)
			HideHeadgear = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HPNumbers");
		if(null != findNode)
			HPNumbers = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ClipCursor");
		if(null != findNode)
			ClipCursor = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ApplyMonsterIK");
		if(null != findNode)
			ApplyMonsterIK = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("SyncCount");
		if(null != findNode)
			SyncCount = Convert.ToBoolean(findNode.GetAttribute("value"));
			
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("VoxelCache");
		if(null != findNode)
			VoxelCacheEnabled = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("DepthBlur");
		if(null != findNode)
			mDepthBlur = Convert.ToBoolean(findNode.GetAttribute("value"));

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CamInertia");
		if(null != findNode)
			CamInertia = Convert.ToSingle(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("DriveCamInertia");
        if (null != findNode)
            DriveCamInertia = Convert.ToSingle(findNode.GetAttribute("value"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("SSAO");
		if(null != findNode)
			mSSAO = Convert.ToBoolean(findNode.GetAttribute("value"));
		
		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AttackWhithMouseDir");
		if(null != findNode)
			AttackWhithMouseDir = Convert.ToBoolean(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Tutorialed");
        if (null != findNode)
            Tutorialed = Convert.ToBoolean(findNode.GetAttribute("value"));

		findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GLSetting");
		if(null != findNode)
			GLSetting = findNode.GetAttribute("value");

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FixBlurryFont");
        if (null != findNode)
            FixBlurryFont = Convert.ToBoolean(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AndyGuidance");
        if (null != findNode)
            AndyGuidance = Convert.ToBoolean(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("MouseStateTip");
        if (null != findNode)
            MouseStateTip = Convert.ToBoolean(findNode.GetAttribute("value"));

        findNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HidePlayerOverHeadInfo");
        if (null != findNode)
            HidePlayerOverHeadInfo = Convert.ToBoolean(findNode.GetAttribute("value"));

        PeInput.LoadInputConfig(mFilepath);
		
		ApplySettings();
	}
	
	public void LoadSystemData()
	{
		if(LoadedData)
			return;

		if(null == InControl.InControlManager.Instance)
			Debug.LogError("InControlManager isn't init.");
		
		mFilepath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir;
        if (!Directory.Exists(mFilepath))
            Directory.CreateDirectory(mFilepath);
		
		mFilepath += "/SystemSaveData.xml";
		
		try{
			XmlDocument xmlDoc = new XmlDocument();
			using (FileStream fs = new FileStream (mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}
			SetSystemData(xmlDoc);
			mHasData = true;
		} catch {
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<" + "SystemData" + "/>");
			xmlDoc.DocumentElement.SetAttribute("Version",mVersion);
			try{
				using (FileStream fs = new FileStream (mFilepath, FileMode.Create, FileAccess.Write, FileShare.None)) {
					xmlDoc.Save (fs);
				}
			}catch(Exception e){
				GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			}
			ApplySettings();
			mHasData = false;
		}
		LoadedData = true;
	}
}
