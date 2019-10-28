using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

using Pathea.PeEntityExt;
using System.Collections.Generic;
using System;
using DbField = PETools.DbReader.DbFieldAttribute;


namespace Pathea
{
	public enum EThinkingType
	{
		Leisure = 1,
		Interaction,
		Stroll,
		Patrol,
		Dining,
		Sleep,
		Work,
		Cure,
		Combat,
		Pursuit,
		Assist,
		Recourse,
		Mission,
		WaitCure,
		Max
	}

	public enum EThinkingMask
	{
		None        = 0,
		Block       = 1, //当前的被暂停
		Blocked     = -1, //新加入的被暂停
		Delete      = 2,  //终止当前
		Deleted     = -2  //不接受新加入
	}

	public class NpcThinking
	{
		public int ID;
		public string Name;
		public EThinkingType Type;
		public Dictionary<EThinkingType,EThinkingMask> mThinkInfo;

		public NpcThinking()
		{
			mThinkInfo = new Dictionary<EThinkingType, EThinkingMask>();
		}

		public EThinkingMask GetMask(EThinkingType type)
		{
			return mThinkInfo[type];
		}
	}

	public class NpcThinkDb 
	{
		static Dictionary<EThinkingType,NpcThinking> _sThinking;
		public static void LoadData()
		{
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPCThinking");
			
//			int fieldCount = reader.FieldCount - 2;
			_sThinking = new Dictionary<EThinkingType, NpcThinking>();
			while (reader.Read())
			{
				NpcThinking think = new NpcThinking();
				think.ID = reader.GetInt32(0);
				think.Type = (EThinkingType)think.ID;
				think.Name = reader.GetString(1);

				for (int i = 2; i <reader.FieldCount; i++)
				{
					think.mThinkInfo.Add((EThinkingType)(i-1),(EThinkingMask)reader.GetInt32(i));
				}
				
				_sThinking.Add(think.Type,think);
			}
		}
		
		public static void Release()
		{
			_sThinking = null;
		}
		
		public static NpcThinking Get(EThinkingType type)
		{
			return _sThinking[type];
		} 
		
		public static NpcThinking Get(int id)
		{
			return Get((EThinkingType)id);
		}


		//未开始的行为使用
		public static bool CanDo(PeEntity entity,EThinkingType type)
		{
			NpcCmpt npc = entity.NpcCmpt;
			if(npc == null)
				return true;

			return npc.ThinkAgent.CanDo(type);
		}

		//正在进行的行为使用
		//返回值 Ture 当前type可以执行
		public static bool CanDoing(PeEntity entity,EThinkingType _newtThink)
		{
			NpcCmpt npc = entity.NpcCmpt;
			if(npc == null)
				return true;

			if(!npc.ThinkAgent.CanDo(_newtThink))
			{
				EThinkingType _curThinkType = npc.ThinkAgent.GetNowDo();
				//NpcThinking curthink = Get(_curThinkType);
				NpcThinking newthink = Get(_newtThink);

				//1:阻塞当前行为，执行新加入行为
				if(newthink.GetMask(_curThinkType) == EThinkingMask.Block)
				{
					npc.ThinkAgent.RemoveThink(_curThinkType);
					npc.ThinkAgent.RemoveThink(_newtThink);

					npc.ThinkAgent.AddThink(_curThinkType);
					npc.ThinkAgent.AddThink(_newtThink);
				    return true;
				}

				//-1:阻塞新加入行为，执行当前行为
				if(newthink.GetMask(_curThinkType) == EThinkingMask.Blocked)
				{
					npc.ThinkAgent.RemoveThink(_newtThink);
					npc.ThinkAgent.RemoveThink(_curThinkType);

					npc.ThinkAgent.AddThink(_newtThink);
					npc.ThinkAgent.AddThink(_curThinkType);
					return false;
				}

				//接受新的行为，删除当前行为
				if(newthink.GetMask(_curThinkType) == EThinkingMask.Delete)
				{
					npc.ThinkAgent.RemoveThink(_curThinkType);
					npc.ThinkAgent.AddThink(_newtThink);
					return true;
				}
				//不接收新的
				if(newthink.GetMask(_curThinkType) == EThinkingMask.Deleted)
				{
	             //no noth
				 return false;
				}

			}
			return true;
		}

	}

	public class NpcThinkAgent
	{
		List<EThinkingType> m_NeedThink;

		public NpcThinkAgent()
		{
			m_NeedThink = new List<EThinkingType>();
		}

		public bool ContainsType(EThinkingType type)
		{
			for(int i=0;i<m_NeedThink.Count;i++)
			{
				if(m_NeedThink[i] == type)
					return true;
			}
			return false;
		}

		public void AddThink(EThinkingType type)
		{
			if(!ContainsType(type))
              m_NeedThink.Add(type);
		}

		public bool RemoveThink(EThinkingType type)
		{
			if(ContainsType(type))
			  return  m_NeedThink.Remove(type);

			return false;
		}

		public void RunThinking()
		{

		}

		public bool CanDo(EThinkingType curtype)
		{
			if(m_NeedThink.Count == 0)
				return true;

			return m_NeedThink[m_NeedThink.Count-1] == curtype;
		
		}

		public bool hasthinkType(EThinkingType curtype)
		{
			for(int i=0;i<m_NeedThink.Count;i++)
			{
				if(m_NeedThink[i] == curtype)
					return true;
			}
			return false;
		}

//		public EThinkingType CalculateThink(EThinkingType newtype)
//		{
//			EThinkingType tempType;
//			if(m_NeedThink == null)
//				return EThinkingType.Max;
//
//			if(m_NeedThink.Count == 1)
//				return m_NeedThink[0];
//
//			NpcThinking newTh = NpcThinkDb.Get(newtype);
//
//			tempType = m_NeedThink[0];
//			for(int i=1;i<m_NeedThink.Count;i++)
//			{
//				newTh.GetMask(m_NeedThink[i]);
//			}
//		}

//		void swichType(EThinkingType newtype,EThinkingType oldtype)
//		{
//			NpcThinking th0 = NpcThinkDb.Get(oldtype);
//			NpcThinking th1 = NpcThinkDb.Get(newtype);
//		}

		public EThinkingType GetNowDo()
		{
			return m_NeedThink[m_NeedThink.Count-1];
		}


	}

}