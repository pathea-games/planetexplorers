using UnityEngine;
using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using PEIK;

namespace Pathea
{
	public class IKCmpt : PeCmpt , IPeMsg
	{
		PeTrans	m_Trans;

		[HideInInspector]
		public IKAnimEffectCtrl m_AnimEffectCtrl;
		[HideInInspector]
		public IKAimCtrl	m_IKAimCtrl;
		[HideInInspector]
		public IKFlashLight	m_IKFlashLight;
		FullBodyBipedIK		m_FBBIK;
		GrounderFBBIK		m_GroundFBBIK;
		IK[]				m_IKArray;

		private float		m_DefaultMappingValue;
		private float		m_DefaultSpineBend;

		float m_EnableFBBIKSqrDis = 32f * 32f;
		bool m_AutoCloseFBBIK;

		bool m_IKEnable = true;

		List<Type> m_SpineMask = new List<Type>();

		public bool aimActive
		{
			set
			{
				if(null != m_IKAimCtrl)
					m_IKAimCtrl.SetActive(value);
			}
			get
			{
				if(null != m_IKAimCtrl)
					return m_IKAimCtrl.active;
				return false;
			}
		}

		bool m_FlashLightActive = false;
		public bool flashLightActive
		{
			set
			{
				m_FlashLightActive = value;
				if(null != m_IKFlashLight)
					m_IKFlashLight.m_Active = m_FlashLightActive;
			}
			get
			{
				return m_FlashLightActive;
			}
		}

		public Vector3 aimTargetPos
		{
			get
			{
				if(null != m_IKAimCtrl)
					return m_IKAimCtrl.targetPos;
				return Vector3.zero;
			}
			
			set
			{
				if(null != m_IKFlashLight)
					m_IKFlashLight.targetPos = value;
				if(null != m_IKAimCtrl)
				{
					if(flashLightActive && null != m_IKFlashLight)
						m_IKAimCtrl.targetPos = m_IKFlashLight.targetPos;
					else
						m_IKAimCtrl.targetPos = value;
				}
			}
		}
		
		public Transform aimTargetTrans
		{
			get
			{
				if(null != m_IKAimCtrl)
					return m_IKAimCtrl.m_Target;
				return null;
			}
			
			set
			{
				if(null != m_IKAimCtrl)
					m_IKAimCtrl.m_Target = value;
			}
		}

		public Ray aimRay
		{
			get
			{
				if(null != m_IKAimCtrl)
					return m_IKAimCtrl.aimRay;
				return new Ray(Vector3.zero, Vector3.zero);
			}
		}

        public IKAimCtrl iKAimCtrl { get { return m_IKAimCtrl; } }

		public bool aimed
		{
			get
			{
				if(null != m_IKAimCtrl)
					return m_IKAimCtrl.aimed;
				return false;
			}
		}

		bool m_SmoothAim;

		public override void Start ()
		{
			base.Start ();
			m_Trans = Entity.peTrans;
			m_SmoothAim = Entity.GetCmpt<MainPlayerCmpt>() == null;
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();

			UpdateFlashLightIK();
			UpdateFBBIKState();
		}

		void UpdateFlashLightIK()
		{
			if(flashLightActive && null != m_IKFlashLight && null != m_IKAimCtrl)
				m_IKAimCtrl.targetPos = m_IKFlashLight.targetPos;
		}

		void UpdateFBBIKState()
		{
			if(null != m_FBBIK)
			{
				bool fBBIKEnable = false;
				if(null != m_AnimEffectCtrl){
					float sqrDisToCam = Vector3.SqrMagnitude(m_Trans.position - PETools.PEUtil.MainCamTransform.position);
					if(sqrDisToCam < m_EnableFBBIKSqrDis)
					{
						if(!m_AutoCloseFBBIK){
							fBBIKEnable = true;
						} else if(m_AnimEffectCtrl.m_MoveEffect.isRunning || m_AnimEffectCtrl.m_HitReaction.isRunning){
							fBBIKEnable = true;
						}
					}
					if(!fBBIKEnable && m_AnimEffectCtrl.m_HitReaction.isRunning)
						fBBIKEnable = true;
				}
				if(m_FBBIK.enabled != fBBIKEnable)
					m_FBBIK.enabled = fBBIKEnable;
			}
		}

		#region IPeMsg implementation
		void IPeMsg.OnMsg (EMsg msg, params object[] args)
		{
			switch (msg)
			{
                case EMsg.View_Prefab_Build:
				BiologyViewRoot viewRoot = args[1] as BiologyViewRoot;
				m_AnimEffectCtrl = viewRoot.ikAnimEffectCtrl;
				m_IKAimCtrl = viewRoot.ikAimCtrl;
				m_IKFlashLight = viewRoot.ikFlashLight;
				m_FBBIK = viewRoot.fbbik;
				m_GroundFBBIK = viewRoot.grounderFBBIK;
				m_IKArray = viewRoot.ikArray;

				if(null != m_IKAimCtrl)
					m_IKAimCtrl.SetSmoothMoveState(m_SmoothAim);
				m_AutoCloseFBBIK = (null == m_GroundFBBIK);
				if(null != m_FBBIK)
				{
					m_FBBIK.Disable();
					m_FBBIK.solver.iterations = 1;
					m_DefaultMappingValue = m_FBBIK.solver.leftArmMapping.weight;
				}
				if(null != m_GroundFBBIK)
					m_DefaultSpineBend = m_GroundFBBIK.spineBend;

				ikEnable = m_IKEnable;
				flashLightActive = m_FlashLightActive;
				enableArmMap = m_SpineMask.Count == 0;
				enableGrounderSpineEffect = m_SpineMask.Count == 0;
				break;
//			case EMsg.Battle_EnterShootMode:
//				enableGrounderSpineEffect = false;
//				enableArmMap = false;
//				break;
//			case EMsg.Battle_ExitShootMode:
//				enableGrounderSpineEffect = true;
//				enableArmMap = true;
//				break;
//			case EMsg.Battle_PauseShootMode:
//				if(null != m_IKAimCtrl)
//					m_IKAimCtrl.SetActive(false);
//				break;
//			case EMsg.Battle_ContinueShootMode:
//				if(null != m_IKAimCtrl)
//					m_IKAimCtrl.SetActive(true);
//				break;
			}
		}
		#endregion

		public bool ikEnable
		{
			set
			{
				m_IKEnable = value;
				if(null != m_FBBIK)
					m_FBBIK.solver.SetIKPositionWeight(m_IKEnable?1:0);
				if(null != m_GroundFBBIK)
					m_GroundFBBIK.weight = m_IKEnable?1:0;
				if(null != m_IKArray)
					foreach(IK ik in m_IKArray)
						if(null != ik)
							ik.enabled = m_IKEnable;
			}
		}

        //lz-2017.04.19 关闭或开启地面拉脚掌的IK
        public bool EnableGroundFBBIK
        {
            set
            {
                if (null != m_GroundFBBIK)
                    m_GroundFBBIK.weight = m_IKEnable ? 1 : 0;
            }
        }

		public void SetSpineEffectDeactiveState(Type type, bool deactive)
		{
			if(!deactive)
				m_SpineMask.Remove(type);
			else if(!m_SpineMask.Contains(type))
				m_SpineMask.Add(type);
			enableArmMap = m_SpineMask.Count == 0;
			enableGrounderSpineEffect = m_SpineMask.Count == 0;
		}

		bool enableArmMap
		{
			set
			{
				if(null != m_FBBIK)
				{
					m_FBBIK.solver.leftArmMapping.weight = value ? m_DefaultMappingValue : 0f;
					m_FBBIK.solver.rightArmMapping.weight = value ? m_DefaultMappingValue : 0f;
				}
			}
		}

		bool enableGrounderSpineEffect
		{
			set
			{
				if(null != m_GroundFBBIK)
				{
					m_GroundFBBIK.spineBend = value ? m_DefaultSpineBend : 0f;
				}
			}
		}
	}
}
