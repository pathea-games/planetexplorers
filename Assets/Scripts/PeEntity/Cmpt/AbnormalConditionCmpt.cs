using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Pathea
{
	public class AbnormalConditionCmpt : PeCmpt ,IPeMsg
	{
		public event Action<PEAbnormalType> evtStart;
		public event Action<PEAbnormalType> evtEnd;

		public event Action<int> evtBuffAdd;
		public event Action<int> evtBuffRemove;
		public event Action<int> evtItemAdd;
		public event Action<float> evtDamage;
		public event Action evtInWater;
		public event Action evtOutWater;

		public event Action<PEAbnormalAttack, Vector3> evtAbnormalAttack;

		PEAbnormal_N[] m_AbnormalList;

		EIdentity m_EIdentity = EIdentity.None;

		bool m_Inited = false;

		NetCmpt m_Net;

		public override void Start ()
		{
			base.Start ();

			if(!m_Inited)
			{
				m_Net = Entity.GetCmpt<NetCmpt>();
				if(null != Entity.commonCmpt)
					m_EIdentity = Entity.commonCmpt.Identity;
				InitData(m_EIdentity);
			}
		}

		public override void OnUpdate ()
		{
			if(null == m_AbnormalList) return;
			PEAbnormal_N abnormal;
			for(int i = 0; i < m_AbnormalList.Length; i++)
			{
				abnormal = m_AbnormalList[i];
				if(null != abnormal)
					abnormal.Update();
			}
		}

		public override void Deserialize (System.IO.BinaryReader r)
		{
			m_Inited = true;

			m_EIdentity = (EIdentity)r.ReadInt32();

			InitData(m_EIdentity);

			int count = r.ReadInt32();

			for(int i = 0; i < count; i++)
			{
				int typeIndex = r.ReadInt32();
				int size = r.ReadInt32();
				if(size > 0)
				{
					if(null != m_AbnormalList[typeIndex])
						m_AbnormalList[typeIndex].Deserialize(r.ReadBytes(size));
					else
						r.ReadBytes(size);
				}
			}
		}

		public override void Serialize (System.IO.BinaryWriter w)
		{
			if(!m_Inited)
			{
				m_Net = Entity.GetCmpt<NetCmpt>();
				if(null != Entity.commonCmpt)
					m_EIdentity = Entity.commonCmpt.Identity;
				InitData(m_EIdentity);
			}

			// EIdentity
			w.Write((int)m_EIdentity);

			List<PEAbnormalType> saveType = GetActiveAbnormalList();

			// Abnormal count
			w.Write(saveType.Count);
			
			// Abnormal
			for(int i = 0; i < saveType.Count; i++)
			{
				//WriteType
				w.Write((int)m_AbnormalList[(int)saveType[i]].type);

				byte[] data = m_AbnormalList[(int)saveType[i]].Serialize();
				if(null == data)
				{
					w.Write(0);
				}
				else
				{
					w.Write(data.Length);
					w.Write(data);
				}
			}
		}

		void IPeMsg.OnMsg (EMsg msg, params object[] args)
		{
			switch(msg)
			{
			case EMsg.State_Die:
				for(int i = 0; i < m_AbnormalList.Length; i++)
					if(null != m_AbnormalList[i])
						m_AbnormalList[i].OnDie();
				break;
			case EMsg.State_Revive:
				for(int i = 0; i < m_AbnormalList.Length; i++)
					if(null != m_AbnormalList[i])
						m_AbnormalList[i].OnRevive();
				break;
			case EMsg.state_Water:
				bool inwater = (bool)args[0];
				if(inwater)
				{
					if(null != evtInWater)
						evtInWater();
				}
				else
				{
					if(null != evtOutWater)
						evtOutWater();
				}
				break;
			case EMsg.Battle_BeAttacked:
				if(null != evtDamage)
					evtDamage((float)args[0]);
				break;
			}
		}

		void AddAbnormal(PEAbnormalType type, AbnormalData data)
		{
			PEAbnormal_N abnormalCondition = new PEAbnormal_N();
			abnormalCondition.Init(type, this, Entity, OnStartAbnormal, OnEndAbnormal);
			m_AbnormalList[(int)type] = abnormalCondition;
		}

		void InitData(EIdentity eIdentity)
		{
			m_AbnormalList = new PEAbnormal_N[(int)PEAbnormalType.Max];
			if(PeGameMgr.IsBuild || PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
				return;
			int mask = 0;
			switch(eIdentity)
			{
			case EIdentity.Player:
				mask = 1;
				break;
			case EIdentity.Npc:
				mask = 2;
				break;
			case EIdentity.Neutral:
				mask = 4;
				break;
			}

			for(int type = 1; type < (int)PEAbnormalType.Max; type++)
			{
				AbnormalData data = AbnormalData.GetData((PEAbnormalType)type);
				if(null != data && (data.target & mask) == mask)
					AddAbnormal((PEAbnormalType)type, data);
			}

			if(null != Entity.aliveEntity)
			{
				Entity.aliveEntity.evtOnBuffAdd += OnBuffAdd;
				Entity.aliveEntity.evtOnBuffRemove += OnBuffRemove;
			}

			PlayerPackageCmpt playercmpt = Entity.packageCmpt as PlayerPackageCmpt;
			if(null != playercmpt)
				playercmpt.getItemEventor.Subscribe(OnItemAdd);
		}

		void OnBuffAdd(int buffID)
		{
			if(null != evtBuffAdd)
				evtBuffAdd(buffID);
		}

		void OnBuffRemove(int buffID)
		{
			if(null != evtBuffRemove)
				evtBuffRemove(buffID);
		}

		void OnStartAbnormal(PEAbnormalType type)
		{
			if(null != evtStart)
				evtStart(type);

			//SendMsg
			NetSendStartMsg(type);
		}

		void OnEndAbnormal(PEAbnormalType type)
		{
			if(null != evtEnd)
				evtEnd(type);

			//SendMsg
			NetSendEndMsg(type);
		}

#region net interface
		void NetSendStartMsg(PEAbnormalType type)
		{
			if (!PeGameMgr.IsMulti || !m_Net.IsController)
				return;

			if (null == m_AbnormalList[(int)type])
				return;

			PlayerNetwork.SyncAbnormalConditionStart(Entity.Id, (int)type, m_AbnormalList[(int)type].Serialize());
		}

		void NetSendEndMsg(PEAbnormalType type)
		{
			if (!PeGameMgr.IsMulti || !m_Net.IsController)
				return;

			PlayerNetwork.SyncAbnormalConditionEnd(Entity.Id, (int)type);
		}
		
		public void NetApplyState(PEAbnormalType type, byte[] data)
		{
			if (null == m_AbnormalList)
			{
				Debug.LogError("AbnormalConditionCmpt has not been inited.");
				return;
			}
			if (null != m_AbnormalList[(int)type])
				m_AbnormalList[(int)type].Deserialize(data);
		}
		
		public void NetEndState(PEAbnormalType type)
		{
			if (null == m_AbnormalList)
			{
				Debug.LogError("AbnormalConditionCmpt has not been inited.");
				return;
			}
			if (null != m_AbnormalList[(int)type])
				m_AbnormalList[(int)type].EndCondition();
		}
#endregion

		public void ApplyAbnormalAttack(PEAbnormalAttack attack, Vector3 effectPos)
		{
			if (null != evtAbnormalAttack)
				evtAbnormalAttack (attack, effectPos);
			switch(attack.type)
			{
			case PEAbnormalAttackType.Dazzling:
				StartAbnormalCondition(PEAbnormalType.Dazzling);
				break;

			case PEAbnormalAttackType.Flashlight:
				StartAbnormalCondition(PEAbnormalType.Flashlight);
				break;
				
			case PEAbnormalAttackType.Tinnitus:
				StartAbnormalCondition(PEAbnormalType.Tinnitus);
				break;

			case PEAbnormalAttackType.Deafness:
				StartAbnormalCondition(PEAbnormalType.Deafness);
				break;
				
			case PEAbnormalAttackType.BlurredVision:
				StartAbnormalCondition(PEAbnormalType.BlurredVision);
				break;
			}
		}

		public bool CheckAbnormalCondition(PEAbnormalType type)
		{
			return null != m_AbnormalList[(int)type] && m_AbnormalList[(int)type].hasEffect;
		}
		public List<PEAbnormalType> GetActiveAbnormalList()
		{
			List<PEAbnormalType> retList = new List<PEAbnormalType>();
			if(null != m_AbnormalList)
			{
				for(int i = 0; i < m_AbnormalList.Length; i++)
					if(null != m_AbnormalList[i] && m_AbnormalList[i].hasEffect)
						retList.Add(m_AbnormalList[i].type);
			}
			return retList;
		}

		public void StartAbnormalCondition(PEAbnormalType type)
		{
			if(null != m_AbnormalList[(int)type])
				m_AbnormalList[(int)type].StartCondition();
		}

		public void EndAbnormalCondition(PEAbnormalType type)
		{
			if(null != m_AbnormalList[(int)type])
				m_AbnormalList[(int)type].EndCondition();
		}

		void OnItemAdd(object sender, PlayerPackageCmpt.GetItemEventArg evt)
		{
			if (null != evtItemAdd)
				evtItemAdd(evt.protoId);
		}
	}
}