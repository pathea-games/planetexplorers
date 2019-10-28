using UnityEngine;
using System.Collections;

public static class PeEnv
{
	static int water_opt = 0;
	static float ControlRain = 1;
	static float BaseRain = 0;
	static float rainSwitch = 1;
	static float wetcoef_multiplier = 1f;
	static float wetcoef_offset = 0f;
	public static void SetControlRain(float maxValue){
		ControlRain = maxValue;
	}
	public static void SetBaseRain(float baseValue){
		BaseRain = baseValue;
	}
	public static void CanRain(bool canRain){
		if(canRain)
			rainSwitch = 1;
		else
			rainSwitch = 0;
	}

	private static float _nearSeaTarget = 1f;
	private static float _nearSeaCurrent = 1f;
	public static void AlterNearSea (bool nearSea)
	{
		_nearSeaTarget = nearSea ? 1f : 0f;
	}

	public static bool isRain{ get{ return (null != Nova)?(Nova.WetCoef > 0.55f):false; } }

	public static void Init ()
	{
		water_opt = 0;
		GameObject novasrc = Resources.Load("Nova Environment") as GameObject;
		if (novasrc != null)
		{
			GameObject novago = GameObject.Instantiate(novasrc) as GameObject;
			novago.name = novasrc.name;
			Nova = novago.GetComponent<NovaEnv.Executor>();
			NovaSettings = Nova.Settings;
			NovaOutput = Nova.Output;
			NovaSettings.MainCamera = Camera.main;
			Nova.OnExecutorCreate += OnExecutorCreate;
			Nova.OnExecutorDestroy += OnExecutorDestroy;

			if (Pathea.PeGameMgr.IsMulti)
			{
				Nova.SunSettings.Obliquity = 30f;
				Nova.SunSettings.Period = 1.0e10;
				Nova.SunSettings.Phi = 2.5e9;
				Nova.Settings.LocalLatitude = -37f;
			}
		}
		else
		{
			Debug.LogError("Cannot find NovaEnv prefab");
		}
	}

	public static void OnExecutorCreate ()
	{
		CameraForge.CameraController.AfterControllerPlay += AfterCamera;		
		NovaSettings.SeaHeight = VFVoxelWater.c_fWaterLvl;
		if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D9) {
			NovaOutput.SunlightIntensityBase = 0.001f;
		}
	}
	
	public static void OnExecutorDestroy ()
	{
		CameraForge.CameraController.AfterControllerPlay -= AfterCamera;
		water_opt = 0;
		Nova = null;
		NovaSettings = null;
		NovaOutput = null;
	}

	static SimplexNoiseEx s_envNoise = new SimplexNoiseEx (100000017);
	public static void Update ()
	{
		if (Nova != null)
		{
            if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.PajaShip)
                Nova.LocalTime = 0;
            else
			    Nova.LocalTime = GameTime.Timer.Second;
			if(Pathea.PeGameMgr.IsMulti)
			{
				if(PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.PajaShip)
				{
					Nova.LocalTime = 0;
				}
			}
			Nova.Settings.TimeElapseSpeed = GameTime.Timer.ElapseSpeed;
			Nova.Settings.SoundVolume = SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
			Nova.WetCoef = (float)System.Math.Pow(System.Math.Max((s_envNoise.Noise(Nova.LocalDay*1.2)
			                                                       + s_envNoise.Noise(Nova.LocalDay*2.4) * 0.5
			                                                       + s_envNoise.Noise(Nova.LocalDay*4.8) * 0.25
				                                                   + s_envNoise.Noise(Nova.LocalDay*9.6) * 0.125) * 0.38 + 0.45, 0), 3);
			// 小雨多一点..
			if (Nova.WetCoef > 0.55f)
			{
				Nova.WetCoef -= 0.55f;
				Nova.WetCoef *= 2.2f;
				Nova.WetCoef = (float)System.Math.Pow(Nova.WetCoef, 2f);
				Nova.WetCoef /= 2.2f;
				Nova.WetCoef += 0.55f;
			}
			Nova.WetCoef = (Mathf.Clamp01(Nova.WetCoef)*ControlRain+BaseRain)*rainSwitch;


			Debug.DrawLine(new Vector3((float)Nova.LocalDay, (float)Nova.WetCoef, 0), new Vector3((float)Nova.LocalDay + 0.01f, (float)Nova.WetCoef, 0), Color.white, 1000);

			bool water_opt1 = SystemSettingData.Instance.WaterDepth;
			bool water_opt2 = SystemSettingData.Instance.WaterRefraction;

			int water_opt_ = 0;

			if (water_opt1 && water_opt2)
				water_opt_ = 3;
			else if (water_opt1 || water_opt2)
				water_opt_ = 2;
			else
				water_opt_ = 1;

			if (water_opt != water_opt_)
			{
				Material water_mat = null;
				switch (water_opt_)
				{
				case 3: water_mat = Resources.Load<Material>("PEWater_High"); break;
				case 2: water_mat = Resources.Load<Material>("PEWater_Medium"); break;
				case 1: water_mat = Resources.Load<Material>("PEWater_Low"); break;
				default: break;
				}
				if (VFVoxelWater.self != null && PEWaveSystem.Self != null)
				{
					Material new_water_mat = Material.Instantiate(water_mat) as Material;
					VFVoxelWater.self.WaterMat = new_water_mat;
					NovaSettings.WaterMaterial = new_water_mat;
					water_opt = water_opt_;
				}
			}
			RenderSettings.fog = !VCEditor.s_Ready;

			if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Story)
			{
				if (PeMappingMgr.inited)
				{
					switch (PeMappingMgr.Instance.Biome)
					{
					case GraphMapping.EBiome.Sea: Nova.BiomoIndex = 0; break;
					case GraphMapping.EBiome.Marsh: Nova.BiomoIndex = 4; break;
					case GraphMapping.EBiome.Jungle: Nova.BiomoIndex = 2; break;
					case GraphMapping.EBiome.Forest: Nova.BiomoIndex = 1; break;
					case GraphMapping.EBiome.Desert: Nova.BiomoIndex = 0; break;
					case GraphMapping.EBiome.Canyon: Nova.BiomoIndex = 0; break;
					case GraphMapping.EBiome.Volcano: Nova.BiomoIndex = 6; break;
					case GraphMapping.EBiome.Grassland: Nova.BiomoIndex = 0; break;
					case GraphMapping.EBiome.Mountainous: Nova.BiomoIndex = 3; break;
					default: Nova.BiomoIndex = 0; break;
					}

					float mul_target = 1f;
					float ofs_target = 0f;
					switch (PeMappingMgr.Instance.Biome)
					{
					case GraphMapping.EBiome.Desert: mul_target = 0.3f; ofs_target = 0.0f; break;
					case GraphMapping.EBiome.Volcano: mul_target = 0.3f; ofs_target = 0.45f; break;
					default: mul_target = 1f; ofs_target = 0f; break;
					}

					wetcoef_multiplier = Mathf.Lerp(wetcoef_multiplier, mul_target, 0.01f);
					wetcoef_offset = Mathf.Lerp(wetcoef_offset, ofs_target, 0.01f);
					Nova.WetCoef *= wetcoef_multiplier;
					Nova.WetCoef += wetcoef_offset;
				}
				else
				{
					Nova.BiomoIndex = 0;
				}
			}
			else
			{
				Nova.BiomoIndex = 0;
			}

			if(RandomDungenMgrData.InDungeon){
				Nova.BiomoIndex = 5;
			}else{
				if(Pathea.PeGameMgr.IsAdventure||Pathea.PeGameMgr.IsBuild){
					if(Pathea.PeCreature.Instance.mainPlayer!=null){
						RandomMapType rmt = VFDataRTGen.GetXZMapType(Mathf.RoundToInt(Pathea.PeCreature.Instance.mainPlayer.position.x),
						                                             Mathf.RoundToInt(Pathea.PeCreature.Instance.mainPlayer.position.z));
						switch(rmt){
							case RandomMapType.Swamp: Nova.BiomoIndex = 4; break;
							case RandomMapType.Rainforest: Nova.BiomoIndex = 2; break;
							case RandomMapType.Forest: Nova.BiomoIndex = 1; break;
							case RandomMapType.Desert: Nova.BiomoIndex = 0; break;
							case RandomMapType.Redstone: Nova.BiomoIndex = 0; break;
							case RandomMapType.Crater: Nova.BiomoIndex = 6; break;
							case RandomMapType.GrassLand: Nova.BiomoIndex = 0; break;
							case RandomMapType.Mountain: Nova.BiomoIndex = 3; break;
							default: Nova.BiomoIndex = 0; break;
						}
					}
				}
			}

			if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Story ||
			    Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Custom)
			{
				switch (SystemSettingData.Instance.TerrainLevel)
				{
                //lz-2016.06.17 由(0=128M,1=256M，2=512M，3=1KM)改为(0=256M，1=512M，2=1KM),因此这里改为这样
				case 0: Nova.Settings.MaxFogEndDistance = 200; break;
				case 1: Nova.Settings.MaxFogEndDistance = 340; break;
				case 2: Nova.Settings.MaxFogEndDistance = 700; break;
				case 3: Nova.Settings.MaxFogEndDistance = 1300; break;
				}
			}
			else
			{
				Nova.Settings.MaxFogEndDistance = 550;
			}

			// 靠近海洋才打开水的反射效果，远离海洋则关闭，
			//if (Input.GetKeyDown(KeyCode.I)){		AlterNearSea(false);		}
			//if (Input.GetKeyDown(KeyCode.O)){		AlterNearSea(true);			}
			AlterNearSea(WaterReflection.ReqRefl());
			if (Mathf.Abs(_nearSeaTarget - _nearSeaCurrent) > 0.0001f)
				_nearSeaCurrent = Mathf.Lerp(_nearSeaCurrent, _nearSeaTarget, 0.04f); // About 3s

			if (_nearSeaCurrent < 0.001f)
			{
				WaterReflection.DisableRefl();
			}
			else
			{
				WaterReflection.EnableRefl();
			}
			Nova.WaterReflectionMasterBlend = _nearSeaCurrent;
		}// End Nova != null
	}



	static void AfterCamera (CameraForge.CameraController camc)
	{
		if (Nova != null)
		{
			#region UNDERWATER

			// 先用voxel粗略判断是否在水中
			Vector3 camPos = camc.pose.position;
			bool underwater = false;
			if ((camPos.x < 0 || camPos.z < 0) && Pathea.PeGameMgr.IsStory)
				underwater = false;
			else if (camPos.y < 0&&camPos.y>-100)
				underwater = true;
			else
				underwater = PETools.PEUtil.CheckPositionUnderWater(camPos);

			// 设置一个underwater的目标值， 等于0则不在水中，值越大则越深
			// 若在水中，初值设为50米。（若检测不到水面碰撞，就按50米处理了）
			float uw_target = underwater ? 50f : 0f;

			// 检测水面碰撞，计算摄像机在水中的深度
			RaycastHit rch_water;
			RaycastHit rch_water_up;
			if (Physics.Raycast(new Ray(camc.pose.position + Vector3.up * 200f, Vector3.down), out rch_water, 200f, 1 << Pathea.Layer.Water))
			{
				// 反向查询水的下边缘，不能查到，查到则不在水中
				if (!Physics.Raycast(new Ray(camc.pose.position, Vector3.up), out rch_water_up, 200f, 1 << Pathea.Layer.Water))
				{
					// 特殊处理 ?
					if( Pathea.PeGameMgr.IsMultiStory && PlayerNetwork.mainPlayer != null && (PlayerNetwork.mainPlayer._curSceneId != -1 && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand))
					{
						;
					}
					else
						uw_target = 200f - rch_water.distance;
				}
			}

			// 若粗测显示不在水中，而水深大于2米，则矛盾，置为0
			if (!underwater && uw_target > 2f)
			{
				uw_target = 0;
			}

			// 应用水下效果
			if ((Nova.Underwater > 0f) != underwater)
			{
				if (!underwater)
					Nova.Underwater = 0f;
				else
					Nova.Underwater = 0.01f;
				Nova.Output.Apply();
			}
			Nova.Underwater = Mathf.Lerp(Nova.Underwater, uw_target, 0.2f);

			#endregion
		}
	}

	public static NovaEnv.Executor Nova;
	public static NovaEnv.Settings NovaSettings;
	public static NovaEnv.Output NovaOutput;
}
