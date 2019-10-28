using UnityEngine;
using System;
using SkillSystem;

namespace Pathea
{
	public class Motion_Live : PeCmpt, IPeMsg
	{
		Action_Sleep 	m_Sleep;
		Action_Gather	m_Gather;
		MotionMgrCmpt	m_MotionMgr;

		[SerializeField] Action_Hand m_Hand;
		[SerializeField] Action_Handed m_Handed;

		public Action_Gather gather{ get{ return m_Gather; } }

		public override void Start ()
		{
			base.Start ();
			if(null != Entity.aliveEntity)
				Entity.aliveEntity.deathEvent += OnDeath;

			m_MotionMgr = Entity.motionMgr;
			if(null != m_MotionMgr)
			{
				m_Sleep = new Action_Sleep();
				m_Gather = new Action_Gather();

				m_MotionMgr.AddAction(m_Sleep);
				m_MotionMgr.AddAction(new Action_Eat());
				m_MotionMgr.AddAction(m_Gather);
				m_MotionMgr.AddAction(new Action_PickUpItem());
				m_MotionMgr.AddAction(new Action_Sit());
				m_MotionMgr.AddAction(new Action_Stuned());
				m_MotionMgr.AddAction(new Action_Build());
				m_MotionMgr.AddAction(new Action_Operation());
				m_MotionMgr.AddAction(new Action_Lie());
				m_MotionMgr.AddAction(new Action_Cutscene());
				m_MotionMgr.AddAction(new Action_Cure());
				m_MotionMgr.AddAction(new Action_Leisure());
				m_MotionMgr.AddAction(new Action_Abnormal());
				m_MotionMgr.AddAction(m_Hand);
				m_MotionMgr.AddAction(m_Handed);
			}

		}

		#region IPeMsg implementation

		void IPeMsg.OnMsg (EMsg msg, params object[] args)
		{
			switch(msg)
			{
			case EMsg.View_Prefab_Build:
				BiologyViewCmpt obj = args[0] as BiologyViewCmpt;
				m_Sleep.m_PhyCtrl = obj.monoPhyCtrl;
				break;
			}
		}

		#endregion

		void OnDeath(SkEntity self, SkEntity caster)
		{
			if(Entity.Race == ERace.Paja || Entity.Race == ERace.Puja)
				m_MotionMgr.DoAction(PEActionType.AlienDeath);
			else
				m_MotionMgr.DoAction(PEActionType.Death);
		}
	}
}
