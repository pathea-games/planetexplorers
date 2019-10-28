using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	
    public class MotionMgrCmpt : PeCmpt, IPeMsg
	{
		PEAction[] 	m_ActionDic;

		bool[] 	m_MaskArray;

		SkAliveEntity	m_SkEntity;

		BiologyViewCmpt m_View;
		
		public event Action<PEActionType> onActionStart;
		public event Action<PEActionType> onActionEnd;
		
		#if UNITY_EDITOR
		public bool ShowLog = false;
		#endif

		class RunningAction
		{
			MotionMgrCmpt m_MotionMgr;
			PEAction m_Action;
			ActionRelationData m_RelationData;
			PEActionParam m_Para;
			bool m_WaitRelation;
			bool m_DoAction;

			public RunningAction(MotionMgrCmpt mmc)
			{
				m_MotionMgr = mmc;
			}

			public void Do(PEAction action, ActionRelationData relationData, PEActionParam para, bool doImmediately = false)
			{
				m_Action = action;
				m_RelationData = relationData;
				m_Para = para;
				m_WaitRelation = !doImmediately;
				m_DoAction = true;
				
				action.PreDoAction();
				
				CheckRelation(doImmediately);
				
				TryDoAction();
			}

			public bool UpdateAction()
			{
				if(m_WaitRelation)
				{
					//Wait for execution mask
					if(!CheckExecution(m_RelationData))
						return false;
					//Do action
					if(m_Action.pauseAction)
						return false;
					m_WaitRelation = false;
				}

				if(m_DoAction)
				{
					m_Action.DoAction(m_Para);
					m_DoAction = false;
				}

				//Update action
				if(!m_Action.Update())
					return false;
				//Continue action pause by this action
				for(int i = 0; i < m_RelationData.m_PauseAction.Count; ++i)
					m_MotionMgr.ContinueAction(m_RelationData.m_PauseAction[i], m_Action.ActionType);

				m_MotionMgr.OnActionEnd(m_Action.ActionType);
				return true;
			}

			public void EndAction()
			{
				m_Action.EndImmediately();
				for(int i = 0; i < m_RelationData.m_PauseAction.Count; ++i)
					m_MotionMgr.ContinueAction(m_RelationData.m_PauseAction[i], m_Action.ActionType);
			}

			void CheckRelation(bool doImmediately)
			{
				//End other action
				for(int i = 0; i < m_RelationData.m_PauseAction.Count; ++i)
					m_MotionMgr.PauseAction(m_RelationData.m_PauseAction[i], m_Action.ActionType);

				for(int i = 0; i < m_RelationData.m_EndAction.Count; ++i)
				{
					if(doImmediately)
						m_MotionMgr.EndImmediately(m_RelationData.m_EndAction[i]);
					else
						m_MotionMgr.EndAction(m_RelationData.m_EndAction[i]);
				}
				for(int i = 0; i < m_RelationData.m_EndImmediately.Count; ++i)
					m_MotionMgr.EndImmediately(m_RelationData.m_EndImmediately[i]);
			}

			void TryDoAction()
			{
				if(m_WaitRelation)
				{
					//Wait for execution mask
					if(!CheckExecution(m_RelationData))
						return;
					//Do action
					if(m_Action.pauseAction)
						return;
					m_WaitRelation = false;
				}
				
				m_Action.DoAction(m_Para);
				m_DoAction = false;
			}

			bool CheckExecution(ActionRelationData data)
			{
				for(int i = 0; i < data.m_EndAction.Count; ++i)
					if(m_MotionMgr.IsActionRunning(data.m_EndAction[i]))
						return false;
				return true;
			}
		}

		RunningAction[] m_RunningAction;
		Stack<RunningAction> m_RunningActionPool;

		RunningAction GetRunningAction()
		{
			RunningAction ret = null;
			if(m_RunningActionPool.Count > 0)
				ret = m_RunningActionPool.Pop();
			if(null == ret)
				ret = new RunningAction(this);
			return ret;
		}

		bool m_HasActiveAction;

		bool m_FreezeCol;
		
		public bool FreezeCol
		{
			get { return m_FreezeCol; }
			set { m_FreezeCol = value;  if(null != m_View) m_View.ActivateCollider(!m_FreezeCol); }
		}

		List<Type> mFreezePhyRequestType;
        bool mFreezePhyStateForSystem;
		bool mFreezePhyByNet;

		public bool freezePhyState{ get; set; }

		public void FreezePhyByNet(bool v)
		{
			mFreezePhyByNet = v;
			UpdatePhy();
		}

        public void FreezePhyState(Type type, bool v)
        {
			if(v)
			{
				if(!mFreezePhyRequestType.Contains(type))
					mFreezePhyRequestType.Add(type);
			}
			else
				mFreezePhyRequestType.Remove(type);
            UpdatePhy();
        }

        public void FreezePhySteateForSystem(bool v)
        {
            mFreezePhyStateForSystem = v;
            UpdatePhy();
        }

        public bool freezePhyStateForSystem
        {
            get
            {
                return mFreezePhyStateForSystem;
            }
        }

        void UpdatePhy()
        {
			freezePhyState = mFreezePhyRequestType.Count > 0 || mFreezePhyStateForSystem || mFreezePhyByNet;
            if (null != m_View)
            {
                m_View.ActivatePhysics(!freezePhyState);
                m_View.ActivateRagdollPhysics(!mFreezePhyStateForSystem);
            }
        }

		public override void Awake ()
		{
			base.Awake ();
            mFreezePhyStateForSystem = true;
			FreezeCol = false;

			m_ActionDic = new PEAction[(int)PEActionType.Max];
			m_RunningAction = new RunningAction[(int)PEActionType.Max];
			m_RunningActionPool = new Stack<RunningAction>();
			mFreezePhyRequestType = new List<Type>();

			m_MaskArray = new bool[(int)PEActionMask.Max];
			for(int i = 0; i < m_MaskArray.Length; i++)
				m_MaskArray[i] = false;
		}

		public override void Start ()
		{
			base.Start ();
			m_SkEntity = Entity.aliveEntity;
			m_View = Entity.biologyViewCmpt;
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			m_HasActiveAction = false;
			for(int i = 0; i < m_RunningAction.Length; i++)
			{
				if(null != m_RunningAction[i])
				{
					m_HasActiveAction = true;
					if(m_RunningAction[i].UpdateAction())
					{
						m_RunningActionPool.Push(m_RunningAction[i]);
						m_RunningAction[i] = null;
					}
				}
			}
		}


		public void AddAction(PEAction action)
		{
			if(!HasAction(action.ActionType))
			{
				m_ActionDic[(int)action.ActionType] = action;
				action.motionMgr = this;
			}
		}

		public void RemoveAction(PEActionType type)
		{
			if(HasAction(type))
			{
				EndImmediately(type);
				m_ActionDic[(int)type] = null;
			}
		}

		public T GetAction<T>() where T : PEAction
		{
			for(int i = 0; i < m_ActionDic.Length; i++)
				if(null != m_ActionDic[i] && m_ActionDic[i].GetType() == typeof(T))
					return m_ActionDic[i] as T;
			return null;
		}

		public void SetMaskState(PEActionMask mask, bool state)
		{
			m_MaskArray[(int)mask] = state;
		}

		public bool GetMaskState(PEActionMask mask)
		{
			return m_MaskArray[(int)mask];
		}

		public bool isInAimState
		{
			get { return IsActionRunning(PEActionType.AimEquipHold) || IsActionRunning(PEActionType.GunHold) || IsActionRunning(PEActionType.BowHold);}
		}

		public bool HasAction(PEActionType type)
		{
			return null != m_ActionDic[(int)type];
		}

		public bool IsActionRunning()
		{
			return m_HasActiveAction;
		}

		public bool IsActionRunning(PEActionType type)
		{
			return null != m_RunningAction[(int)type];
		}

		public bool IsActionPause(PEActionType type)
		{
			return HasAction(type) && m_ActionDic[(int)type].pauseAction;
		}

		public bool CanDoAction(PEActionType type, PEActionParam para = null)
		{
			if(HasAction(type))
			{
				if(IsActionRunning(type))
				{
					return true;
				}
				else if(m_ActionDic[(int)type].CanDoAction(para))
				{
					ActionRelationData data = ActionRelationData.GetData(type);
					return CheckDepend(data);
				}
			}
			return false;
		}

		public bool DoAction(PEActionType type, PEActionParam para = null)
		{
			if(HasAction(type))
			{
				if(IsActionRunning(type))
				{
//					if(null != m_SkEntity._net)
//						((SkNetworkInterface)m_SkEntity._net).SendDoAction(type,para);
					m_ActionDic[(int)type].ResetAction(para);
					return true;
				}
				else if(m_ActionDic[(int)type].CanDoAction(para))
				{
					ActionRelationData data = ActionRelationData.GetData(type);
					if(null == data)
						return false;
					
					if(CheckDepend(data))
					{
						if(null != m_SkEntity._net)
							((SkNetworkInterface)m_SkEntity._net).SendDoAction(type,para);
						
						if(null != onActionStart)
							onActionStart(type);
						RunningAction runningAction = GetRunningAction();
						runningAction.Do(m_ActionDic[(int)type], data, para, false);
						m_RunningAction[(int)type] = runningAction;
						#if UNITY_EDITOR
						if(ShowLog)
							Debug.LogError("DoAction: " + type.ToString());
						#endif
						return true;
					}
				}
			}
			return false;
		}

		public void DoActionImmediately(PEActionType type, PEActionParam para = null)
		{
			if(HasAction(type))
			{
				if(IsActionRunning(type))
					m_ActionDic[(int)type].ResetAction(para);
				else
				{
					if(null != m_SkEntity._net)
						((SkNetworkInterface)m_SkEntity._net).SendDoAction(type,para);
					ActionRelationData data = ActionRelationData.GetData(type);					
					if(null != onActionStart)
						onActionStart(type);					
					RunningAction runningAction = GetRunningAction();
					runningAction.Do(m_ActionDic[(int)type], data, para, true);
					m_RunningAction[(int)type] = runningAction;
				}
			}
		}

		bool CheckDepend(ActionRelationData data)
		{
			if(null == data)
				return false;
			for(int i = 0; i < data.m_DependMask.Count; ++i)
			{
				if(data.m_DependMask[i].maskValue != m_MaskArray[(int)data.m_DependMask[i].maskType])
				{
#if UNITY_EDITOR
					if(ShowLog)
						Debug.LogError("ActionMask: " + data.m_DependMask[i].maskType.ToString() + "don't match");
#endif
					return false;
				}
			}
			return true;
		}
		
		public void PauseAction(PEActionType beType, PEActionType tryPauseType)
		{
			if(HasAction(beType))
			{
				if(IsActionRunning(beType) && !m_ActionDic[(int)beType].pauseAction)
					m_ActionDic[(int)beType].PauseAction();
				if(!m_ActionDic[(int)beType].m_PauseActions.Contains(tryPauseType))
					m_ActionDic[(int)beType].m_PauseActions.Add(tryPauseType);
			}
		}
		
		public void ContinueAction(PEActionType beType, PEActionType tryPauseType)
		{
			if(HasAction(beType))
			{
				m_ActionDic[(int)beType].m_PauseActions.Remove(tryPauseType);
				if(IsActionRunning(beType) && !m_ActionDic[(int)beType].pauseAction)
					m_ActionDic[(int)beType].ContinueAction();
			}
		}

		public bool EndAction(PEActionType type)
		{
			if(IsActionRunning(type))
			{
				m_ActionDic[(int)type].EndAction();
				//send stop action
				if(null != m_SkEntity._net && type != PEActionType.Wentfly && type != PEActionType.Knocked)
					((SkNetworkInterface)m_SkEntity._net).SendEndAction(type);
				return true;
			}
			return false;
		}

		public bool EndImmediately(PEActionType type)
		{
			if(IsActionRunning(type))
			{
				m_RunningActionPool.Push(m_RunningAction[(int)type]);
				m_RunningAction[(int)type].EndAction();
				m_RunningAction[(int)type] = null;
				//send stop action
				if(null != m_SkEntity._net && type != PEActionType.Wentfly && type != PEActionType.Knocked)
					((SkNetworkInterface)m_SkEntity._net).SendEndImmediately(type);
				OnActionEnd(type);
				return true;
			}
			return false;
		}

		void HideEquipFirstPerson(bool hide)
		{
			for(int i = 0; i < (int)PEActionType.Max; i++)
			{
				if(null != m_ActionDic[i])
				{
					iEquipHideAbleAction ehAction = m_ActionDic[i] as iEquipHideAbleAction;
					if(null != ehAction)
						ehAction.hideEquipInactive = hide;
				}
			}
		}

		#region IPeMsg implementation
		void IPeMsg.OnMsg (EMsg msg, params object[] args)
		{
			switch(msg)
			{
			case EMsg.View_Prefab_Build:
				//BiologyViewCmpt obj = args[0] as BiologyViewCmpt;
                UpdatePhy();
				FreezeCol = m_FreezeCol;
				Invoke("UpdateActionStateWhenBuildMode", PeGameMgr.IsMulti?1f:0.1f);
				break;
			case EMsg.View_Prefab_Destroy:
				UpdateActionStateWhenModeDestroy();
				break;
			case EMsg.View_FirstPerson:
				HideEquipFirstPerson((bool)args[0]);
				break;
			}
		}
		#endregion

		void UpdateActionStateWhenBuildMode()
		{
			for(int i = 0; i < (int)PEActionType.Max; i++)
				if(null != m_RunningAction[i] && null != m_ActionDic[i])
					m_ActionDic[i].OnModelBuild();
		}

		void UpdateActionStateWhenModeDestroy()
		{
			for(int i = 0; i < (int)PEActionType.Max; i++)
				if(null != m_RunningAction[i] && null != m_ActionDic[i])
					m_ActionDic[i].OnModelDestroy();
		}

		public void OnActionEnd(PEActionType type)
		{
			if(null != onActionEnd)
				onActionEnd(type);
		}
    }
}