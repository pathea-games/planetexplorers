using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem;

public class SceneEntityCreatorArchiver : ArchivableSingleton<SceneEntityCreatorArchiver>
{
	#region FixedSpawnPointFromDB
	public class AgentInfo : MonsterEntityCreator.AgentInfo
	{
		int _fixedId = -1;
		// interface
		public AgentInfo(int fixedId) : base()
		{
			_fixedId = fixedId;	
		}

		public override void OnSuceededToCreate (SceneEntityPosAgent agent)
		{
			base.OnSuceededToCreate (agent);
			if (_fixedId != -1)
			{
				//SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(fixedId, false);
				PeEntity mon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(_fixedId);
				if (mon != null && mon.aliveEntity != null)	// now no code for grp
					mon.aliveEntity.deathEvent += AliveEntityDeathEvent;
			}
		}		
		void AliveEntityDeathEvent(SkEntity arg1, SkEntity arg2)
		{
			FixedSpawnPointInfo info = null;
			if (SceneEntityCreatorArchiver.Instance._fixedSpawnPointInfos.TryGetValue(_fixedId, out info))
			{
				SceneMan.RemoveSceneObj(info._agent);

				if (info._needCD > 0.0f)
				{
					MissionManager.Instance.PeTimeToDo(delegate(){
						if(info._bActive){
							SceneMan.RemoveSceneObj(info._agent);
							SceneMan.AddSceneObj(info._agent);
						}
					}, info._needCD, _fixedId);
				} else {
					SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(_fixedId, false);
				}
				SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(_fixedId).aliveEntity.deathEvent -= AliveEntityDeathEvent;
			}
		}
	}
	public class FixedSpawnPointInfo
	{
		public bool _bActive;
		public SceneEntityPosAgent _agent;
		public float _needCD;
	}
	Dictionary<int, FixedSpawnPointInfo> _fixedSpawnPointInfos = new Dictionary<int, FixedSpawnPointInfo>();
	public void AddFixedSpawnPointToScene(List<int> pointIds)
	{
		List<SceneEntityPosAgent> agents = new List<SceneEntityPosAgent>();
		FixedSpawnPointInfo info = null;
		foreach (int id in pointIds)
		{
			if (_fixedSpawnPointInfos.TryGetValue(id, out info) && info._bActive)
			{
				if (PeGameMgr.IsMultiStory)
				{
					if (AISpawnPoint.s_spawnPointData[id].active == true)
						continue;
				}
				agents.Add(info._agent);
			}
		}
		SceneMan.AddSceneObjs(agents);
	}
	public void RemoveFixedSpawnPointFromScene(List<int> pointIds)
	{
		List<SceneEntityPosAgent> agents = new List<SceneEntityPosAgent>();
		FixedSpawnPointInfo info = null;
		foreach (int id in pointIds)
		{
			if (_fixedSpawnPointInfos.TryGetValue(id, out info) && info._bActive)
			{
				agents.Add(info._agent);
			}
		}
		SceneMan.RemoveSceneObjs(agents);
	}
	public void SetFixedSpawnPointActive(int pointID, bool active)
	{
		FixedSpawnPointInfo info = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out info))
		{
			if (PeGameMgr.IsMultiStory)
			{
				if (PlayerNetwork.mainPlayer != null)
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_AI_SetFixActive, pointID, active);
			}
			else
			{
				info._bActive = active;
				if (null != SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(pointID))
				{
					if (!active) SceneMan.RemoveSceneObj(info._agent);
					else ReqReborn(info._agent);
				}
				else
				{
                    if (active)
                    {
                        info._agent.canRide = false;
                        SceneMan.AddSceneObj(info._agent);
                    }
                }
			}
		}
	}
	public PeEntity GetEntityByFixedSpId(int pointID)
	{
		FixedSpawnPointInfo info = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out info))
		{
			return info._agent.entity;
		}
		return null;
	}
	
	public bool GetEntityReviveTimeSpId(int pointID,out float cdTime) 
	{
		FixedSpawnPointInfo info = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out info))
		{
			cdTime = info._needCD;
			return true;
		}
		cdTime = 0;
		return false;
	}
	
	public SceneEntityPosAgent GetAgentByFixedSpId(int pointID) 
	{
		FixedSpawnPointInfo info = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out info))
		{
			return info._agent;
		}
		return null;
	}
	
	public void SetEntityByFixedSpId(int pointID, PeEntity entity)
	{
		FixedSpawnPointInfo info = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out info))
		{
			info._agent.entity = entity;
		}
	}
	public void ReqReborn(int pointID)
	{
		FixedSpawnPointInfo info = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out info))
		{
			ReqReborn(info._agent);
		}
	}
	private void ReqReborn(SceneEntityPosAgent agent)
	{
		if (agent.IsIdle) {
            if (agent.entity == null || agent.entity.IsDead())
			{
				agent.entity = null;
                agent.canRide = false;
                SceneMan.RemoveSceneObj(agent);
				SceneMan.AddSceneObj(agent);
            } else if(agent.entity != null && agent.entity.monstermountCtrl != null 
                && agent.entity.monstermountCtrl.ctrlType == ECtrlType.Mount) //Lw2017.2.14:怪物已经被驯服，重新生成
            {
                agent.entity = null;
                agent.canRide = false;
                //SceneMan.RemoveSceneObj(agent);
                SceneMan.AddSceneObj(agent);
            }
			else
			{
                agent.canRide = false;
                MonsterCmpt mc = agent.Go.GetComponent<MonsterCmpt>();
                if (mc != null)
					mc.Req_MoveToPosition(agent.Pos, 1, true, SpeedState.Run);
            }
		}
	}
	private void Init4FixedSpawnPoint()
	{
		foreach (KeyValuePair<int, AISpawnPoint> pair in AISpawnPoint.s_spawnPointData)
		{
			AISpawnPoint pt = pair.Value;
			int protoId = pt.resId;
			if (pt.isGroup) protoId |= EntityProto.IdGrpMask;
            FixedSpawnPointInfo info = new FixedSpawnPointInfo();
			info._bActive = pt.active;
			info._needCD = pt.refreshtime;
			info._agent = MonsterEntityCreator.CreateAgent(pt.Position, protoId, Vector3.one, Quaternion.Euler(pt.euler));
			info._agent.spInfo = new AgentInfo(pair.Key);
			info._agent.FixPos = pt.fixPosition;
			_fixedSpawnPointInfos[pair.Key] = info;
		}
	}
	private void SetData4FixedSpawnPoint(BinaryReader br)
	{
		FixedSpawnPointInfo info = null;
		int cnt = br.ReadInt32();
		for (int i = 0; i < cnt; i++)
		{
			int id = br.ReadInt32();
			bool bActive = br.ReadBoolean();
			if (_fixedSpawnPointInfos.TryGetValue(id, out info))
			{
				info._bActive = bActive;
			}
		}
	}
	private void WriteData4FixedSpawnPoint(BinaryWriter bw)
	{
		bw.Write(_fixedSpawnPointInfos.Count);
		foreach (KeyValuePair<int, FixedSpawnPointInfo> pair in _fixedSpawnPointInfos)
		{
			bw.Write(pair.Key);
			bw.Write(pair.Value._bActive);
		}
	}
	#endregion
	#region PosRectNpcCnt
	Dictionary<IntVector2, int> _posRectNpcCnt = new Dictionary<IntVector2, int>();
	Dictionary<IntVector2, SceneEntityPosRect> _entityRects = new Dictionary<IntVector2, SceneEntityPosRect>();			// key: pos index
	private void Init4PosRectNpcCnt()
	{
		_posRectNpcCnt.Clear ();
	}
	private void SetData4PosRectNpcCnt(BinaryReader br)
	{
		int cnt = br.ReadInt32();
		for (int i = 0; i < cnt; i++)
		{
			int ix = br.ReadInt32();
			int iz = br.ReadInt32();
			int n = br.ReadInt32();
			_posRectNpcCnt[new IntVector2(ix,iz)] = n;
		}
	}
	private void WriteData4PosRectNpcCnt(BinaryWriter bw)
	{
		if (SceneEntityPosRect.EntityNpcNum == 0) {
			bw.Write((int)0);
			return;
		}
		bw.Write(_posRectNpcCnt.Count);	// place holder
		SceneEntityPosRect entityRect;
		foreach (KeyValuePair<IntVector2, int> pair in _posRectNpcCnt)
		{
			IntVector2 key = pair.Key;
			bw.Write(key.x);
			bw.Write(key.y);
			if (_entityRects.TryGetValue(pair.Key, out entityRect)){
				bw.Write(entityRect.CntNpcNotCreated);
			} else {
				bw.Write(pair.Value);
			}
		}
	}
	public void FillPosRect(int ix, int iz)
	{		
		SceneEntityPosRect entityRect;
		IntVector2 idx = IntVector2.Tmp;
		idx.x = ix;
		idx.y = iz;
		if (!_entityRects.TryGetValue(idx, out entityRect))
		{
			int cntNpc = SceneEntityPosRect.EntityNpcNum;
			int cntMonster = SceneEntityPosRect.EntityMonsterNum;
			IntVector2 key = new IntVector2(ix, iz);
			if(!_posRectNpcCnt.TryGetValue(key, out cntNpc)){
				cntNpc = SceneEntityPosRect.EntityNpcNum;
				_posRectNpcCnt[key] = cntNpc;
			}
			if (SceneEntityPosRect.EntityNpcNum != 0 && Mathf.Abs (ix) > 16 || Mathf.Abs (iz) > 16){	// Add limitation on npc's count
				cntNpc = 0;
			}

			entityRect = new SceneEntityPosRect(ix, iz);
			entityRect.Fill(cntNpc, cntMonster);
			_entityRects[key] = entityRect;
		}
	}
	#endregion
	
	// override singleton
	protected override void OnInit()
	{
		base.OnInit ();
		Init4FixedSpawnPoint ();
	}
	protected override void SetData(byte[] data)
	{
		if (data == null) return;
		
		using (MemoryStream ms = new MemoryStream(data)) {
			using(BinaryReader br = new BinaryReader (ms)){
				SetData4FixedSpawnPoint (br);
				if(ms.Position < ms.Length){
					SetData4PosRectNpcCnt (br);
				}
			}
		}
	}
	protected override void WriteData(BinaryWriter bw)
	{
		WriteData4FixedSpawnPoint (bw);
		WriteData4PosRectNpcCnt (bw);
	}
}

