#define DestroyLeftMonster
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem;

public class SceneEntityMissionPointTowerDefence : ISceneEntityMissionPoint
{
	static PlayerMission playerMission{ get { return MissionManager.Instance.m_PlayerMission; } }

	TypeTowerDefendsData _towerData;
	EntityMonsterBeacon _entityBcn; 
	int _entityBcnId = -1;
	int _idxTarId = -1;
	//int _leftCntToFin = 0;

	public int MissionId{ get; set; }
	public int TargetId{ get; set; }

	public SceneEntityMissionPointTowerDefence(int monsterBeaconId = -1)
	{
		_entityBcnId = monsterBeaconId;
	}

	public void Stop()
	{
		if (_entityBcn != null) {
			PeCreature.Instance.Destory (_entityBcn.Id);
		}
	}

    public bool Start()
    {
        //if (playerMission.GetMissionFlagType(MissionId) == null) return false;
        if (SceneEntityCreator.self.PlayerTrans == null) return false;

        //if (!MissionManager.Instance.m_bHadInitMission)
        //{
        //    Debug.LogError("[MissionManager]Mission not inited.");
        //    return false;
        //}
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionId);
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            if (data.m_TargetIDList[i] == TargetId)
            {
                _idxTarId = i;
                break;
            }
        }
        if (_idxTarId == -1) return false;
        _towerData = MissionManager.GetTypeTowerDefendsData(TargetId);
        if (_towerData == null) return false;

        if (PeGameMgr.IsSingle)
            playerMission.m_TowerUIData.MaxCount = _towerData.m_Count;
        else
            playerMission.m_TowerUIData.MaxCount = 0;
        playerMission.m_TowerUIData.MissionID = MissionId;
        playerMission.m_TowerUIData.CurCount = 0;

        if (_towerData.m_TdInfoId != 0)
        {
            _entityBcn = EntityMonsterBeacon.CreateMonsterBeaconByTDID(_towerData.m_TdInfoId, SceneEntityCreator.self.PlayerTrans,
                playerMission.m_TowerUIData, _entityBcnId, _towerData, data.m_iNpc);
        }
        else if (_towerData.m_SweepId.Count > 0)
        {
            _entityBcn = EntityMonsterBeacon.CreateMonsterBeaconBySweepID(_towerData.m_SweepId, SceneEntityCreator.self.PlayerTrans,
			                                                              playerMission.m_TowerUIData, _towerData.m_Time, _entityBcnId,_towerData, data.m_iNpc);
        }
        else
            return false;

        if (_towerData.m_tolTime != 0)
            MissionManager.Instance.SetTowerMissionStartTime(TargetId);

        if (_entityBcn == null) return false;
        _entityBcnId = _entityBcn.Id;
        _entityBcn.handlerNewWave += OnNewWave;
        _entityBcn.handlerOneDeath += OnOneDeath;

        PeEntity npc = null;
        for (int m = 0; m < _towerData.m_iNpcList.Count; m++)
        {
            npc = EntityMgr.Instance.Get(_towerData.m_iNpcList[m]);
            if (npc == null)
                continue;
            npc.SetInvincible(false);
        }

        //_leftCntToFin = _towerData.m_Count;
        //Register all waves into playerMission to avoid complete shortly
        string value = "0_0_0_0_0"; // just for pass checking in IsReplyTarget: x(spType), num, cnt, created, completeTarget
        playerMission.ModifyQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId, value);
        //MissionManager.mTowerKillNum = "0";
        //MissionManager.mTowerMonCount = towerData.m_Count.ToString();

        return true;
    }

	void OnNewWave(AISpawnTDWavesData.TDWaveSpData spData, int idxWave)
	{
		//string value = "0_0_0_1_0"; // just for pass checking in IsReplyTarget: x(spType), num, cnt, created, completeTarget
		//playerMission.ModifyQuestVariable(MissionId, "TDMONS" + _idxTarId, value);
		MissionManager.mTowerCurWave = (idxWave+1).ToString ();
		MissionManager.mTowerTotalWave = spData._waveDatas.Count.ToString ();
		MissionManager.Instance.UpdateMissionTrack (MissionId);
	}
    void OnOneDeath()
    {
        if (!Pathea.PeGameMgr.IsMulti)
        {
            string value = playerMission.GetQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId);
            string[] tmplist = value.Split('_');
            if (tmplist.Length < 2)
            {
                Debug.LogError("[TaskTowerDef]:Wrong Quest Var:" + value);
            }
            int num = tmplist.Length >= 2 ? Convert.ToInt32(tmplist[1]) : 0;
            num++;
            if (num < _towerData.m_Count)
            {
                value = "0_" + num + "_0_1_0"; // just for pass checking in IsReplyTarget: x(spType), num, cnt, created, completeTarget
                playerMission.ModifyQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId, value);
            }
            else if (num == _towerData.m_Count)
            {
                value = "0_" + num + "_0_1_1"; // just for pass checking in IsReplyTarget: x(spType), num, cnt, created, completeTarget
                playerMission.ModifyQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId, value);
                MissionManager.Instance.CompleteTarget(TargetId, MissionId, true);
            }
        }
    }
}
