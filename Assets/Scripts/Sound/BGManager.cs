using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class BGManager : MonoBehaviour 
{
	public static BGManager Instance;
    static float MinBattleTime = 5.0f;
    const int BattleID = 701;
    const int WaterID = 916;
    const int WaterSurfaceSeaID = 918;
    const int WaterSurfaceRiverID = 917;
    const int CreationMusicID = 1162;
    const int PlayerBaseMusicID = 1162;
    const int NativeBaseMusicID = 1161;
    const int DungeonIronMusicID = 1161;
    const int DungeonCaveMusicID = 836;

    int mBgMusicID;
    float mBattleBgTime;
    bool mBattle;
    bool mInWater;
    bool mInWaterSurface;
    bool mInitPlayer;

    PeEntity mPlayer;
    PeEntity mVehicle;

    List<PeEntity> mPlayerEnemies;
    List<PeEntity> mVehicleEnemies;

	AudioController mBgBattleAudio;
	AudioController mBgWaterAudio;
	AudioController mBgWaterSurfaceSeaAudio;
	AudioController mBgWaterSurfaceRiverAudio;

    Dictionary<int, AudioController> mBgMusicDic = new Dictionary<int, AudioController>();

    public int bgMusic
    {
        set 
        {
            if (mBgMusicID != value)
            {
                if (mBgMusicID > 0)
                    OnBgMusicExit(mBgMusicID);

                mBgMusicID = value;

                if (mBgMusicID > 0)
                    OnBgMusicEnter(mBgMusicID);
            }
        }
    }

	public bool battle
	{
		set
		{
			if(mBattle != value)
			{
				mBattle = value;

				if(mBattle)
					OnBattleEnter();
				else
					OnBattleExit();
			}
		}
		get
		{
			return mBattle;
		}
	}

    public bool inWater
	{
		set
		{
			if(mInWater != value)
			{
				mInWater = value;

				if(mInWater)
					OnPlayerWaterEnter();
				else
					OnPlayerWaterExit();
			}
		}
		get
		{
			return mInWater;
		}
	}

    public bool inWaterSurface
	{
		set
		{
			if(mInWaterSurface != value)
			{
				mInWaterSurface = value;

				if(mInWaterSurface)
					OnPlayerWaterSurfaceEnter();
				else
					OnPlayerWaterSurfaceExit();
			}
		}
		get
		{
			return mInWaterSurface;
		}
	}

	public AudioController bgAudio
	{
		get
		{
            if (mBgMusicDic.ContainsKey(mBgMusicID))
                return mBgMusicDic[mBgMusicID];

            return null;
		}
	}

	public AudioController bgBattleAudio
	{
		get
		{
			return mBgBattleAudio;
		}
	}

    int GetEnvironmentBgMusic()
    {
        if (VCEditor.s_Active)
            return CreationMusicID;
        else
        {
            PeEntity player = PeCreature.Instance.mainPlayer;
            if(player != null)
            {
                if (CSMain.Instance != null && CSMain.Instance.IsInAssemblyArea(player.position))
                    return PlayerBaseMusicID;
                else
                {
                    if (PeGameMgr.IsAdventure)
                    {
                        if (RandomDunGenUtil.IsInDungeon(player))
                        {
                            if (RandomDunGenUtil.GetDungeonType() == DungeonType.Iron)
                                return DungeonIronMusicID;
                            else
                                return DungeonCaveMusicID;
                        }
                        else if (VArtifactUtil.IsInNativeCampArea(player.position) >= 0)
                            return NativeBaseMusicID;
                    }

                    if(PeGameMgr.IsStory)
                    {
                        int soundID = PeMap.StaticPoint.Mgr.Instance.GetMapSoundID(player.position);
                        if (soundID > 0)
                            return soundID;
                    }
                }
            }
        }

        return 0;
    }

    protected virtual int GetCurrentBgMusicID()
    {
        return 0;
    }

    IEnumerator UpdateTerrain()
    {
        while (true)
        {
            int bgMusicID = GetEnvironmentBgMusic();

            if (bgMusicID > 0)
                bgMusic = bgMusicID;
            else
                bgMusic = GetCurrentBgMusicID();

            yield return new WaitForSeconds(10.0f);
        }
    }

    IEnumerator PauseBgAudio()
    {
        while (true)
        {
            if (this is BGMainMenu)
                yield break;

            if (bgAudio != null && bgAudio.length > Mathf.Epsilon)
            {
				float len = bgAudio.length;
                if (!bgAudio.isPlaying){
					if(!mBattle){
	                    bgAudio.PlayAudio(10.0f);
						yield return new WaitForSeconds(Random.Range(2, 4) * len);
					}
				} else { // use else to avoid bgAudio become null after yield wait
					if (bgAudio.time >= len - 10f)
	                {
	                    bgAudio.StopAudio(10.0f);
						yield return new WaitForSeconds(Random.Range(1, 3) * len);
	                }
				}
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    bool CalculateBattleState()
    {
        if (mPlayer != null)
        {
            for (int i = 0; i < mPlayerEnemies.Count; i++)
            {
                if (mPlayerEnemies[i] != null 
                    && PETools.PEUtil.SqrMagnitude(mPlayer.position, mPlayerEnemies[i].position, false) < 10f*10f)
                {
                    mBattleBgTime = Time.time;
                }
            }
        }

        if (mVehicle != null)
        {
            for (int i = 0; i < mVehicleEnemies.Count; i++)
            {
                if (mVehicleEnemies[i] != null 
                    && PETools.PEUtil.SqrMagnitude(mVehicle.position, mVehicleEnemies[i].position, false) < 10f*10f)
                {
                    mBattleBgTime = Time.time;
                }
            }
        }

        return Time.time - mBattleBgTime < MinBattleTime;
    }

    bool IsInWaterSurfaceSea(Vector3 position)
    {
        if(inWaterSurface)
        {
            if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Adventure)
                return VFDataRTGen.IsSea((int)position.x, (int)position.z);
            else if(PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story)
            {
                Vector2 pos = new Vector2(position.x, position.z);
                int mapid = PeMappingMgr.Instance.GetAiSpawnMapId(pos);
                return mapid == 25 || mapid == 26 || mapid == 22;
            }
        }

        return false;
    }

    void OnPlayerWaterEnter()
    {
        if (mBgWaterAudio != null)
            mBgWaterAudio.PlayAudio(5.0f);
    }

    void OnPlayerWaterExit()
    {
        if (mBgWaterAudio != null)
            mBgWaterAudio.StopAudio(5.0f);
    }

    void OnPlayerWaterSurfaceEnter()
    {
        if(mPlayer == null || IsInWaterSurfaceSea(mPlayer.position))
        {
            if (mBgWaterSurfaceSeaAudio)
                mBgWaterSurfaceSeaAudio.PlayAudio(5.0f);
        }
        else
        {
            if (mBgWaterSurfaceRiverAudio)
                mBgWaterSurfaceRiverAudio.PlayAudio(5.0f);
        }
    }

    void OnPlayerWaterSurfaceExit()
    {
        if (mBgWaterSurfaceSeaAudio)
            mBgWaterSurfaceSeaAudio.StopAudio(5.0f);

        if (mBgWaterSurfaceRiverAudio)
            mBgWaterSurfaceRiverAudio.StopAudio(5.0f);
    }

	void OnBattleEnter()
	{
        if (bgAudio != null)
        {
            bgAudio.StopAudio(5.0f);
        }

        if (mBgBattleAudio != null)
        {
            mBgBattleAudio.PlayAudio(10.0f);
        }
	}

	void OnBattleExit()
	{
        if (bgAudio != null)
        {
            bgAudio.PlayAudio(10.0f);
        }

        if (mBgBattleAudio != null)
        {
            mBgBattleAudio.PauseAudio(5.0f);
        }
	}

    void OnBgMusicEnter(int id)
    {
        if (id > 0)
        {
            if (mBgMusicDic.ContainsKey(id) && mBgMusicDic[id] != null)
                mBgMusicDic[id].PlayAudio();
            else
            {
				AudioController audioCtrl = AudioManager.instance.Create(PETools.PEUtil.MainCamTransform.position,
                                                                         id,
                                                                         transform,
                                                                         false,
                                                                         false);
				if(audioCtrl != null){
	                audioCtrl.PlayAudio(10.0f);
	                mBgMusicDic.Add(id, audioCtrl);
				}
            }
        }
    }

    void OnBgMusicExit(int id)
    {
        if (id > 0)
        {
            if (mBgMusicDic.ContainsKey(id))
            {
                if (mBgMusicDic[id] != null)
                {
                    mBgMusicDic[id].StopAudio(5.0f);
                    mBgMusicDic[id].Delete(5.0f);
                }
                mBgMusicDic.Remove(id);
            }
        }
    }

    void OnPlayerGetOnCarrier(WhiteCat.CarrierController carrier)
    {
        mVehicle = carrier.GetComponent<PeEntity>();

        if (mVehicle != null && mVehicle.peSkEntity != null)
        {
            mVehicle.peSkEntity.attackEvent += OnPlayerVehicleAttack;
            mVehicle.peSkEntity.onHpReduce += OnPlayerVehicleBeAttack;

            mVehicle.peSkEntity.OnBeEnemyEnter += OnVehicleEnemyEnter;
            mVehicle.peSkEntity.OnBeEnemyExit  += OnVehicleEnemyExit;
        }

        mPlayerEnemies.Clear();
    }

    void OnPlayerGetOffCarrier(WhiteCat.CarrierController carrier)
    {
        if (mVehicle != null && mVehicle.peSkEntity != null)
        {
            mVehicle.peSkEntity.attackEvent -= OnPlayerVehicleAttack;
            mVehicle.peSkEntity.onHpReduce  -= OnPlayerVehicleBeAttack;

            mVehicle.peSkEntity.OnBeEnemyEnter -= OnVehicleEnemyEnter;
            mVehicle.peSkEntity.OnBeEnemyExit  -= OnVehicleEnemyExit;
        }

        mVehicle = null;

        mVehicleEnemies.Clear();
    }

    void OnPlayerEnemyEnter(PeEntity enemy)
    {
        if (!mPlayerEnemies.Contains(enemy))
            mPlayerEnemies.Add(enemy);
    }

    void OnPlayerEnemyExit(PeEntity enemy)
    {
        if (mPlayerEnemies.Contains(enemy))
            mPlayerEnemies.Remove(enemy);
    }

    void OnVehicleEnemyEnter(PeEntity enemy)
    {
        if (!mVehicleEnemies.Contains(enemy))
            mVehicleEnemies.Add(enemy);
    }

    void OnVehicleEnemyExit(PeEntity enemy)
    {
        if (mVehicleEnemies.Contains(enemy))
            mVehicleEnemies.Remove(enemy);
    }

    void OnPlayerAttack(SkillSystem.SkEntity hurt, float damage)
    {
        mBattleBgTime = Time.time;
    }

    void OnPlayerBeAttack(SkillSystem.SkEntity hurt, float damage)
    {
        if (hurt == null || (mPlayer != null && mPlayer.skEntity == hurt))
            return;

        mBattleBgTime = Time.time;
    }

    void OnPlayerVehicleAttack(SkillSystem.SkEntity hurt, float damage)
    {
        mBattleBgTime = Time.time;
    }

    void OnPlayerVehicleBeAttack(SkillSystem.SkEntity hurt, float damage)
    {
        if (hurt == null || (mVehicle != null && mVehicle.skEntity == hurt))
            return;

        mBattleBgTime = Time.time;
    }

	void Awake()
	{
		Instance = this;

        mPlayerEnemies = new List<PeEntity>();
        mVehicleEnemies = new List<PeEntity>();

        Vector3 position = PETools.PEUtil.MainCamTransform.position;
        mBgBattleAudio              = AudioManager.instance.Create(position, BattleID, transform, false, false);
        mBgWaterAudio               = AudioManager.instance.Create(position, WaterID, transform, false, false);
        mBgWaterSurfaceSeaAudio     = AudioManager.instance.Create(position, WaterSurfaceSeaID, transform, false, false);
        mBgWaterSurfaceRiverAudio   = AudioManager.instance.Create(position, WaterSurfaceRiverID, transform, false, false);

        StartCoroutine(UpdateTerrain());
        StartCoroutine(PauseBgAudio());
	}
	
	void Update()
	{
        //if (battleEnemies.Count > 0 || Time.time - mBattleBgTime < MinBattleTime)
        //    battle = true;
        //else
        //    battle = false;

        if(mPlayer == null)
            mPlayer = PeCreature.Instance.mainPlayer;

        if(mPlayer != null && !mInitPlayer)
        {
            mInitPlayer = true;

            if (mPlayer.passengerCmpt != null)
            {
                mPlayer.passengerCmpt.onGetOnCarrier += OnPlayerGetOnCarrier;
                mPlayer.passengerCmpt.onGetOffCarrier += OnPlayerGetOffCarrier;
            }

            mPlayer.peSkEntity.attackEvent += OnPlayerAttack;
            mPlayer.peSkEntity.onHpReduce += OnPlayerBeAttack;

            mPlayer.peSkEntity.OnBeEnemyEnter += OnPlayerEnemyEnter;
            mPlayer.peSkEntity.OnBeEnemyExit  += OnPlayerEnemyExit;
        }

        if(mPlayer != null 
            && mPlayer.biologyViewCmpt != null 
            && mPlayer.biologyViewCmpt.monoPhyCtrl != null)
        {
            bool spineInWater = mPlayer.biologyViewCmpt.monoPhyCtrl.spineInWater;
            bool headInWater = mPlayer.biologyViewCmpt.monoPhyCtrl.headInWater;
            inWater = headInWater;
            inWaterSurface = spineInWater && !headInWater;
        }

        battle = CalculateBattleState();
	}
}
