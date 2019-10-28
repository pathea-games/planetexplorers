using Mono.Data.SqliteClient;
using System;
using System.IO;
using UnityEngine;

public class LocalDatabase : MonoBehaviour {
	private static string s_tmpDbFileName = null;
	private static SqliteAccessCS s_localDatabase = null;
	
	public static SqliteAccessCS Instance{
		get{
#if UNITY_EDITOR			
			if(Application.isEditor){
				if(s_localDatabase == null)
				{
					s_localDatabase = LoadDb();
				}
			}
#endif			
			return s_localDatabase;
		}
    }
	public static SqliteAccessCS PureInstance{ get { return s_localDatabase; } }
	private static SqliteAccessCS LoadDb()
	{
		try{ // Load ex international
			PELocalization.LoadData(GameConfig.DataBaseI18NPath);
			Debug.Log("[I18N]Succeed to load data:"+GameConfig.DataBaseI18NPath);
		} catch {
			Debug.LogError("[I18N]Failed to load data:"+GameConfig.DataBaseI18NPath);
		}

		TextAsset dbAsset = Resources.Load(GameConfig.DataBaseFile, typeof(TextAsset)) as TextAsset;
		try{
			s_tmpDbFileName = System.IO.Path.GetTempFileName();
			using(FileStream fsDbW = new FileStream(s_tmpDbFileName, FileMode.Open, FileAccess.Write, FileShare.None))
			{
				fsDbW.Write(dbAsset.bytes, 0, dbAsset.bytes.Length);
			}
		} catch {
			string tempDir = System.IO.Path.GetTempPath();	
			if(!Directory.Exists(tempDir))	Directory.CreateDirectory(tempDir);

			do{
				s_tmpDbFileName = System.IO.Path.Combine(tempDir, System.IO.Path.GetRandomFileName());
			}while (File.Exists(s_tmpDbFileName));
			Debug.LogWarning("Failed to create temp file!  Retry with "+s_tmpDbFileName);
			try{
				using(FileStream fsDbW = new FileStream(s_tmpDbFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				{
					fsDbW.Write(dbAsset.bytes, 0, dbAsset.bytes.Length);
				}
			}catch(Exception e){
				Debug.LogError("Failed to create temp file!  Try running the game as administrator. \r\n\r\n"+e.ToString());
			}
		}
		return new SqliteAccessCS(s_tmpDbFileName);
	}
	public static void LoadAllData()
	{
		if (s_localDatabase != null)
			return;

#if UNITY_EDITOR
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start ();
#endif
		s_localDatabase = LoadDb();
		SkillSystem.SkData.LoadData();
        Pathea.Effect.EffectData.LoadData();
        Pathea.Projectile.ProjectileData.LoadData();
        Pathea.RequestRelation.LoadData();
        Pathea.CampData.LoadData();
        Pathea.ThreatData.LoadData();
        Pathea.DamageData.LoadData();
        HumanSoundData.LoadData();
		ItemDropData.LoadData();

		PELocalization.LoadData();

		NaturalResAsset.NaturalRes.LoadData();
		//SkillAsset.EffCastData.LoadData();
		//SkillAsset.EffSkill.LoadData();
		//SkillAsset.MergeSkill.LoadData();
		//AnimData.LoadData();
		//AnimSoundData.LoadData();

		AiAsset.AiData.LoadData();

		SoundAsset.SESoundBuff.LoadData();
		SoundAsset.SESoundStory.LoadData();
        //CharacterData.LoadCharacterData();
		StoryDoodadMap.LoadData ();
		StoreRepository.LoadData();
		NpcMissionDataRepository.LoadData();
		//PlayerAttribute.LoadData();
		MissionRepository.LoadData();
		TalkRespository.LoadData();
		//NpcRandomRepository.LoadData();
		ShopRespository.LoadData();
		WareHouseManager.LoadData();
		//HeroTalkRepository.LoadData();
		MutiPlayRandRespository.LoadData();
		PromptRepository.LoadData();
		
        //MapIconData.LoadDate();
        //MapMaskData.LoadDate();
		CampPatrolData.LoadDate();
		Camp.LoadData ();
		RepProcessor.LoadData ();
		
		CloudManager.LoadData();
		//BattleUnitData.LoadData();
		TutorialData.LoadData();
		//RepairMachineManager.LoadData();
        MapMaskData.LoadDate();
        MessageData.LoadData();   //lz-2016.07.13 Add it
        MonsterHandbookData.LoadData(); //lz-2016.07.20 Add it
		StoryRepository.LoadData();
        RMRepository.LoadRandMission();
		MisInitRepository.LoadData();
		CameraRepository.LoadCameraPlot();
		AdRMRepository.LoadData();
		VCConfig.InitConfig();
		Cutscene.LoadData();
		
//		BuildBrushData.LoadBrush();
		BSPattern.LoadBrush();
		BSVoxelMatMap.Load();
		BSBlockMatMap.Load();
		BlockBuilding.LoadBuilding();
		LifeFormRule.LoadData();
		PlantInfo.LoadData();
		MetalScanData.LoadData();
		BattleConstData.LoadData();		
		CustomCharactor.CustomMetaData.LoadData();
		SkillTreeInfo.LoadData ();
        VArtifactUtil.LoadData();
		Pathea.ActionRelationData.LoadActionRelation();

        //colony
        CSInfoMgr.LoadData();
        ProcessingObjInfo.LoadData();
        CSTradeInfoData.LoadData();
        CampTradeIdData.LoadData();
        AbnormalTypeTreatData.LoadData();
		CSMedicineSupport.LoadData();
        //RandomItemMgr
        RandomItemDataMgr.LoadData();
        FecesData.LoadData();
		//randomdungeon
		RandomDungeonDataBase.LoadData();
		AbnormalData.LoadData();
		PEAbnormalNoticeData.LoadData();

		RelationInfo.LoadData();
		EquipSetData.LoadData();
		SuitSetData.LoadData();

		CheatData.LoadData();

        Pathea.NpcProtoDb.Load();
        Pathea.MonsterProtoDb.Load();
        Pathea.MonsterRandomDb.Load();
        Pathea.MonsterGroupProtoDb.Load ();
        Pathea.RandomNpcDb.Load();
        Pathea.PlayerProtoDb.Load();
        Pathea.TowerProtoDb.Load();
        Pathea.DoodadProtoDb.Load();
		Pathea.AttPlusNPCData.Load();
		Pathea.AttPlusBuffDb.Load();
		Pathea.NpcTypeDb.Load();
		Pathea.NpcRandomTalkDb.Load();
		Pathea.NpcThinkDb.LoadData();
		Pathea.NpcEatDb.LoadData();
		Pathea.NpcRobotDb.Load();
		Pathea.NPCScheduleData.Load();
        Pathea.NpcVoiceDb.LoadData();
        InGameAidData.LoadData(); //lz-2016.08.21 add it
        MountsSkillDb.LoadData(); 

#if UNITY_EDITOR
        sw.Stop ();
		Debug.Log("Database Loaded : "+sw.ElapsedMilliseconds);
		sw.Reset();
#else
		Debug.Log("Database Loaded");
#endif
	}
	public static void FreeAllData()
	{
#if UNITY_EDITOR
		Debug.Log("Mem size before ocl CleanUp :"+GC.GetTotalMemory(true));

        Pathea.NpcProtoDb.Release();
        Pathea.MonsterProtoDb.Release();
        Pathea.RandomNpcDb.Release();
        Pathea.PlayerProtoDb.Release();
        Pathea.TowerProtoDb.Release();
        Pathea.DoodadProtoDb.Release();
		Pathea.AttPlusNPCData.Release();
		Pathea.AttPlusBuffDb.Release();
		Pathea.NpcTypeDb.Release();
		Pathea.NpcRandomTalkDb.Release();
		Pathea.NpcThinkDb.Release();
		Pathea.NpcEatDb.Release();
		Pathea.NPCScheduleData.Release();
        Pathea.NpcVoiceDb.Release();
		CampPathDb.Release();
        MountsSkillDb.Relese();

        //		ItemAsset.ModeInfo.s_tblModeInfo = null;
        //ItemAsset.ItemType.s_tblItemType = null;
        //ItemAsset.ItemData.s_tblItemData = null;
        //		ItemAsset.VeinData.s_tblVeinData = null;
        NaturalResAsset.NaturalRes.s_tblNaturalRes = null;
		SkillAsset.EffSkill.s_tblEffSkills = null;
		//SkillAsset.MergeSkill.s_tblMergeSkills = null;
		//AnimData.s_tblAnimData = null;
		SoundAsset.SESoundBuff.s_tblSeSoundBuffs = null;
		
		DestroyImmediate(VFVoxelTerrain.self);
		Debug.Log("Mem size after VFVoxelTerrain CleanUp :"+GC.GetTotalMemory(true));
        //DestroyImmediate(PlayerFactory.self);
        //DestroyImmediate(PlayerFactory.mMianPlayerObj);
		Resources.UnloadUnusedAssets();
		Debug.Log("Mem size after all CleanUp :"+GC.GetTotalMemory(true));
		//VFVoxelTerrain.DestroyImmediate(VFVoxelTerrain.self);
#endif
		s_localDatabase.CloseDB();
		File.Delete(s_tmpDbFileName);
	}
 
	public static string findGrid(string listName,int id,string lineName)
	{
		SqliteDataReader reader = Instance.ReadFullTable(listName);
		string ret= null;
		int i =0;
        while (reader.Read())
        {
			if(i>1)
			{
	            int fid = Convert.ToInt32(reader.GetString(0));
				if(fid == id)
				{
					ret = reader.GetString(reader.GetOrdinal(lineName));
					return ret;
				}
			}
			i++;
		}
		return ret;
	}
}
