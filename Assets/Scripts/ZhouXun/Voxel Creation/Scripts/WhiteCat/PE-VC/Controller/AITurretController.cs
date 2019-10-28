using UnityEngine;
using Pathea;

namespace WhiteCat
{
	public class AITurretController : AIBehaviourController
	{
        protected override AIMode defaultAIMode
		{
			get
			{
                return AIMode.Attack;
			}
		}


		// 初始化质量
		protected override float mass
		{
			get
			{
				return Mathf.Clamp(
					creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.aiTurretMassScale,
					PEVCConfig.instance.aiTurretMinMass,
					PEVCConfig.instance.aiTurretMaxMass);
			}
		}


		// 初始化质心位置
		protected override Vector3 centerOfMass
		{
			get { return creationController.creationData.m_Attribute.m_CenterOfMass; }
		}


		// 初始化惯性张量系数
		protected override Vector3 inertiaTensorScale
		{
			get { return Vector3.one; }
		}


		// 初始化阻力系数
		protected override void InitDrags(
			out float standardDrag, out float underwaterDrag,
			out float standardAngularDrag, out float underwaterAngularDrag)
		{
			standardDrag = PEVCConfig.instance.robotStandardDrag;
			underwaterDrag = PEVCConfig.instance.robotUnderwaterDrag;
			standardAngularDrag = PEVCConfig.instance.robotStandardAngularDrag;
			underwaterAngularDrag = PEVCConfig.instance.robotUnderwaterAngularDrag;
		}


        public int bulletProtoId
        {
            get { return GetWeapon().bulletProtoID; }
        }


		protected override void InitOtherThings()
		{
			base.InitOtherThings();

			// 物品操作
			gameObject.AddComponent<ItemScript>();
            gameObject.AddComponent<DragItemMousePickTowerCreation>().Init(this);

			rigidbody.constraints = RigidbodyConstraints.FreezeAll & (~RigidbodyConstraints.FreezePositionY);
		}


		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			if (isPlayerHost)
			{
				// 更新攻击目标信息
				UpdateAttactTarget();
			}
		}
	}
}