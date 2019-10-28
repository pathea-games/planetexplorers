#define Have_ENTITY
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;

public enum EntityType
{
    EntityType_Npc,
    EntityType_Monster,
    EntityType_Doodad,
    EntityType_MonsterTD, // MonsterTD is legacy enumerator number
}

public interface ISceneEntityMissionPoint	// life cycle depend on a certain mission
{
    int MissionId { get; set; }
    int TargetId { get; set; }
    bool Start();
    void Stop();
    //void OnMissionPause();
    //void OnMissionResume();
}

public class SceneEntityCreator : MonoBehaviour
{
    public static SceneEntityCreator self = null;
	private bool _bReadyToRefresh = false;
	private bool isNeedRefresh = false;
	Dictionary<IntVector2, ISceneEntityMissionPoint> _missionEntityPoints = new Dictionary<IntVector2, ISceneEntityMissionPoint>();	// key: missionId, targetId
    PeTrans _playerTrans = null;
    public Transform PlayerTrans
    {
        get
        {
            if (_playerTrans == null)
                _playerTrans = PeCreature.Instance.mainPlayer.peTrans;
            return _playerTrans.trans;
        }
    }

    // Mono methods
    void Awake()
    {
        self = this;
    }
    void Update()
    {
#if Have_ENTITY
        //if (EntityCreateMgr.DbgUseLegacyCode)
        //    return;

		// single dungeon has pos of current, all entities but player and followers are inactive
		// multi mode's dungeon 's Y < -500, so agent with y==0 will not get a y to gen because raycast is from MaxHitTest to zero
		if ((PeGameMgr.IsSingleBuild) ||
			(PeGameMgr.IsSingleAdventure && PeGameMgr.yirdName == AdventureScene.Dungen.ToString()))
			return;

		if (SceneMan.self == null || SceneMan.self.Observer == null)
			return;
		isNeedRefresh |= SceneMan.self.CenterMoved;

		if(!_bReadyToRefresh || !isNeedRefresh)
			return;

		isNeedRefresh = false;
		SceneEntityPosRect.EntityPosToRectIdx(SceneMan.self.Observer.position, IntVector2.Tmp);
		int cx = IntVector2.Tmp.x;
		int cz = IntVector2.Tmp.y;
		for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
				SceneEntityCreatorArchiver.Instance.FillPosRect(cx + x, cz + z);
            }
        }
#endif
        //TODO : Delete unused 
    }
    // New/Restore
    public void New()
    {
#if Have_ENTITY
        //if (EntityCreateMgr.DbgUseLegacyCode)
        //    return;

        NpcEntityCreator.Init();
        MonsterEntityCreator.Init();
        DoodadEntityCreator.Init();
        SceneEntityCreatorArchiver.Instance.New();
        if (PeGameMgr.IsStory || PeGameMgr.IsTutorial)
        {
            if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
            {
                StartCoroutine(InitTutorialNpc());
            }
            else
            {
                DoodadEntityCreator.CreateStoryDoodads(true);
                StartCoroutine(InitStoryNpc());
            }
        }
		_bReadyToRefresh = true;
#endif
    }
    public void Restore()
    {
		NpcEntityCreator.Init ();
		MonsterEntityCreator.Init ();
		DoodadEntityCreator.Init ();
		SceneEntityCreatorArchiver.Instance.Restore();
        if (PeGameMgr.IsStory)
        {
			if(PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial)
			{
	            DoodadEntityCreator.CreateStoryDoodads(false);
			}
        }
		_bReadyToRefresh = true;
    }
    IEnumerator InitStoryNpc()
    {
        while (PeCreature.Instance.mainPlayer == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        if (PeGameMgr.IsStory)
        {
            NpcEntityCreator.CreateStoryLineNpc();
            NpcEntityCreator.CreateStoryRandNpc();
        }
        yield return 2;
    }

    IEnumerator InitTutorialNpc()
    {
        while (PeCreature.Instance.mainPlayer == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        NpcEntityCreator.CreateTutorialLineNpc();
        // NpcEntityCreator.CreateStoryRandNpc();

        yield return 2;
    }

    // Utils
    public void AddMissionPoint(int missionId, int targetId, int entityId = -1)
    {
        TargetType curType = MissionRepository.GetTargetType(targetId);
        if (curType == TargetType.TargetType_TowerDif)
        {
            IntVector2 mpKey = new IntVector2(missionId, targetId);
            ISceneEntityMissionPoint mp = new SceneEntityMissionPointTowerDefence(entityId);
            mp.MissionId = missionId;
            mp.TargetId = targetId;
            if (mp.Start())
            {
                _missionEntityPoints[mpKey] = mp;
            }
        }
        else if (curType == TargetType.TargetType_KillMonster)
        {
            IntVector2 mpKey = new IntVector2(missionId, targetId);
            ISceneEntityMissionPoint mp = new SceneEntityMissionPointMonsterKill();
            mp.MissionId = missionId;
            mp.TargetId = targetId;
            if (mp.Start())
            {
                _missionEntityPoints[mpKey] = mp;
            }
        }
    }
    public void RemoveMissionPoint(int missionId, int targetId)
    {
        if (targetId < 0)
        {
            List<IntVector2> lstPos = new List<IntVector2>();
            foreach (KeyValuePair<IntVector2, ISceneEntityMissionPoint> pair in _missionEntityPoints)
            {
                if (pair.Key.x == missionId)
                {
                    pair.Value.Stop();
                    lstPos.Add(pair.Key);
                }
            }
            foreach (IntVector2 pos in lstPos)
            {
                _missionEntityPoints.Remove(pos);
            }
        }
        else
        {
            IntVector2 mpKey = new IntVector2(missionId, targetId);
            ISceneEntityMissionPoint mp;
            if (_missionEntityPoints.TryGetValue(mpKey, out mp))
            {
                mp.Stop();
                _missionEntityPoints.Remove(mpKey);
            }
        }
    }
    public ISceneEntityMissionPoint GetMissionPoint(int missionId, int targetId)
    {
        IntVector2 mpKey = new IntVector2(missionId, targetId);
        return _missionEntityPoints.ContainsKey(mpKey) ? _missionEntityPoints[mpKey] : null;
    }
    public void SetSpawnPointActive(int id, bool active)
    {
		SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(id, active);
    }
}

