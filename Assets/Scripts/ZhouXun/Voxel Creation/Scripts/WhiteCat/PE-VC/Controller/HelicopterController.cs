using System;
using UnityEngine;

namespace WhiteCat
{
	public class HelicopterController : CarrierController
	{
		VCPVtolRotor[] _vtolRotors;         // 螺旋桨
		VCPVtolThruster[] _vtolThrusters;   // 推进器

		float _liftForceFactor;             // 升力系数
        

		public float liftForceFactor { get { return _liftForceFactor; } }


		// 初始化质量
		protected override float mass
		{
			get
			{
				return Mathf.Clamp(
					creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.helicopterMassScale,
					PEVCConfig.instance.helicopterMinMass,
					PEVCConfig.instance.helicopterMaxMass);
			}
		}

		// 初始化质心位置
		protected override Vector3 centerOfMass
		{
			get
			{
				return new Vector3(0, creationController.creationData.m_Attribute.m_CenterOfMass.y, 0);
			}
		}

		// 初始化惯性张量系数
		protected override Vector3 inertiaTensorScale
		{
			get
			{
				return PEVCConfig.instance.helicopterInertiaTensorScale;
			}
		}

		// 初始化阻力系数
		protected override void InitDrags(
			out float standardDrag, out float underwaterDrag,
			out float standardAngularDrag, out float underwaterAngularDrag)
		{
			standardDrag = PEVCConfig.instance.helicopterStandardDrag;
			underwaterDrag = PEVCConfig.instance.helicopterUnderwaterDrag;
			standardAngularDrag = PEVCConfig.instance.helicopterStandardAngularDrag;
			underwaterAngularDrag = PEVCConfig.instance.helicopterUnderwaterAngularDrag;
		}


		protected override void InitOtherThings()
		{
			base.InitOtherThings();

			LoadParts(ref _vtolRotors);
			LoadParts(ref _vtolThrusters);

			// 避免计算误差，先重置 Transform
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;

			// 初始化螺旋桨
			if (_vtolRotors != null && _vtolRotors.Length > 0)
			{
                foreach (var rotor in _vtolRotors)
				{
					rotor.Init(this);
				}

                // 两种尺寸, 6个方向分别多少个?
                int[,] counts = new int[2, 6];
                foreach (var rotor in _vtolRotors)
                {
                    counts[rotor.sizeType, rotor.directionType]++;
                }

                foreach (var rotor in _vtolRotors)
                {
                    rotor.InitSoundScale(counts[rotor.sizeType, rotor.directionType]);
                }
            }

			// 初始化推进器
			if (_vtolThrusters != null && _vtolThrusters.Length > 0)
			{
				foreach (var thruster in _vtolThrusters)
				{
					thruster.Init(this);
				}
			}

			// 计算最大升力
			float maxLiftForce = 0;

			for (int i = 0; i < _vtolRotors.Length; i++)
			{
				maxLiftForce += _vtolRotors[i].maxLiftForce;
			}

			for (int i = 0; i < _vtolThrusters.Length; i++)
			{
				maxLiftForce += _vtolThrusters[i].maxLiftForce;
			}

			float factor = 1f;

			if (maxLiftForce > 1f)
			{
				// 最大上升加速度
				float maxLiftAcc = Mathf.Min(maxLiftForce / rigidbody.mass + Physics.gravity.y, PEVCConfig.instance.maxLiftAccelerate);
				factor = (maxLiftAcc - Physics.gravity.y) * rigidbody.mass / maxLiftForce;
			}

			// 设置螺旋桨最大转速
			float maxRotateSpeed = factor * PEVCConfig.instance.rotorMaxRotateSpeed;
			for (int i = 0; i < _vtolRotors.Length; i++)
			{
				_vtolRotors[i].InitMaxRotateSpeed(maxRotateSpeed);
			}

			// 设置推进器最大推力
			for (int i = 0; i < _vtolThrusters.Length; i++)
			{
				_vtolThrusters[i].InitMaxForceRatio(factor);
			}

			// 恢复 transform
			transform.position = position;
			transform.rotation = rotation;
		}


		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			// 平衡辅助
			Vector3 torque;
			if (Vector3.Dot(transform.up, Vector3.up) > 0)
			{
				// 小于 90 度
				torque = Vector3.Cross(transform.up, Vector3.up);
			}
			else
			{
				// 大于 90 度
				torque = Vector3.Cross(transform.up, Vector3.up).normalized;
			}
			rigidbody.AddTorque(rigidbody.mass * PEVCConfig.instance.helicopterBalanceHelp * torque);

			// 计算升力系数
			{
				Vector3 position = transform.position;
				float baseHeight = GetMaxTerrainHeight();
				if (baseHeight < 100) baseHeight = 100f;

				_liftForceFactor = PEVCConfig.instance.liftForceFactor.Evaluate((position.y - baseHeight) / PEVCConfig.instance.helicopterMaxHeight);
			}
		}


        public float GetMaxTerrainHeight()
        {
            Vector3 pos = transform.position;
            var mFlyBaseHeight = PeMappingMgr.Instance.GetTerrainHeight(pos);

            pos.x += 100;
            mFlyBaseHeight = Mathf.Max(PeMappingMgr.Instance.GetTerrainHeight(pos), mFlyBaseHeight);

            pos.x -= 200;
            mFlyBaseHeight = Mathf.Max(PeMappingMgr.Instance.GetTerrainHeight(pos), mFlyBaseHeight);

            pos.x += 100;
            pos.y += 100;
            mFlyBaseHeight = Mathf.Max(PeMappingMgr.Instance.GetTerrainHeight(pos), mFlyBaseHeight);

            pos.y -= 200;
            mFlyBaseHeight = Mathf.Max(PeMappingMgr.Instance.GetTerrainHeight(pos), mFlyBaseHeight);

            return mFlyBaseHeight;
        }
    }

}