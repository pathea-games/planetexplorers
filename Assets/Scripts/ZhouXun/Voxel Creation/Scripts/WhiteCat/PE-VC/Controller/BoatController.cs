using UnityEngine;

namespace WhiteCat
{
	public class BoatController : CarrierController
	{
		VCPShipPropeller[] _propellers;		// 螺旋桨
		VCPShipRudder[] _rudders;			// 方向舵
		VCPSubmarineBallastTank[] _tanks;   // 水箱


		// 初始化质量
		protected override float mass
		{
			get
			{
				return Mathf.Clamp(
					creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.boatMassScale,
					PEVCConfig.instance.boatMinMass,
					PEVCConfig.instance.boatMaxMass);
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
				return PEVCConfig.instance.boatInertiaTensorScale;
			}
		}

		// 初始化阻力系数
		protected override void InitDrags(
			out float standardDrag, out float underwaterDrag,
			out float standardAngularDrag, out float underwaterAngularDrag)
		{
			standardDrag = PEVCConfig.instance.boatStandardDrag;
			underwaterDrag = PEVCConfig.instance.boatUnderwaterDrag;
			standardAngularDrag = PEVCConfig.instance.boatStandardAngularDrag;
			underwaterAngularDrag = PEVCConfig.instance.boatUnderwaterAngularDrag;
		}


		protected override void InitOtherThings()
		{
			base.InitOtherThings();

            LoadParts(ref _propellers);
            LoadParts(ref _rudders);
            LoadParts(ref _tanks);

			gameObject.AddComponent<CreationWaterMask>();

			// 初始化螺旋桨
			if (_propellers != null && _propellers.Length > 0)
			{
				foreach (var propeller in _propellers)
				{
					propeller.Init(this, _tanks != null && _tanks.Length > 0);
				}
			}

			// 初始化方向舵
			if (_rudders != null && _rudders.Length > 0)
			{
				foreach (var rudder in _rudders)
				{
					rudder.Init(this);
				}
			}

			// 初始化水箱
			if (_tanks != null && _tanks.Length > 0)
			{
				foreach (var tank in _tanks)
				{
					tank.Init(this);
				}
			}

			InitWave();
		}


		void InitWave()
		{
			Transform wavetrans = new GameObject ("Wave").transform;
			wavetrans.SetParent (creationController.centerObject, false);
			wavetrans.localPosition = new Vector3 (creationController.bounds.size.x * 0.25f, -creationController.bounds.size.y * 0.5f, 0f);
			
			var wave = wavetrans.gameObject.AddComponent<PEWaterLineWaveTracer>();
			wave.TracerTrans = wavetrans;
			wave.Height = creationController.bounds.size.y;
			wave.Width = creationController.bounds.size.x;
			wave.Length = creationController.bounds.size.z;

			wave.WaveSpeed = 0.15f;
			wave.AutoGenWave = true;
			wave.WaveDuration = 15f;
			wave.Frequency = 25;
			wave.Strength = 40;
			wave.Frequency = 30;
			wave.TimeOffsetFactor = 8;
			wave.IntervalTime = 0.4f;
			wave.DeltaTime = 0.3f;
			wave.ScaleRate = 0.0015f;
			wave.DefualtScale = 4;
			wave.DefualtScaleFactor = 10;
			wave.Distance = 512;
			wave.Desc = "Boat wave";
			wave.IsValid = true;

			wavetrans = Instantiate (wavetrans.gameObject).transform;
			wavetrans.SetParent (creationController.centerObject, false);
			wavetrans.localPosition = new Vector3 (-creationController.bounds.size.x * 0.25f, -creationController.bounds.size.y * 0.5f, 0f);
		}


        public float FluidDisplacement(Vector3 wpos)
		{
			if (VFVoxelWater.self != null && VFVoxelWater.self.Voxels != null)
			{
				int x = Mathf.RoundToInt(wpos.x);
				int z = Mathf.RoundToInt(wpos.z);

				float base_y = Mathf.Floor(wpos.y - 0.5f) + 0.5f;
				float bias_y = wpos.y - base_y;
				int y0 = Mathf.FloorToInt(base_y) + 1;
				int y1 = y0 + 1;
				VFVoxel voxel0 = VFVoxelWater.self.Voxels.SafeRead(x, y0, z);
				VFVoxel voxel1 = VFVoxelWater.self.Voxels.SafeRead(x, y1, z);
				return Mathf.Clamp01(Mathf.Lerp((float)voxel0.Volume, (float)voxel1.Volume, bias_y) / 255.0f);
			}
			return 0;
		}


		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			//// 平衡辅助
			//Vector3 torque;
			//if (Vector3.Dot(transform.up, Vector3.up) > 0)
			//{
			//	// 小于 90 度
			//	torque = Vector3.Cross(transform.up, Vector3.up);
			//}
			//else
			//{
			//	// 大于 90 度
			//	torque = Vector3.Cross(transform.up, Vector3.up).normalized;
			//}
			//rigidbody.AddTorque(rigidbody.mass * PEVCConfig.instance.boatBalanceHelp * underWaterFactor * torque);

			// 应用浮力

			float buoyancyFactor = (1f - Mathf.Clamp(rigidbody.velocity.y, 0, 10f) * 0.1f) * PEVCConfig.instance.buoyancyFactor;

			float amplitude = 0.16f * underWaterFactor * (1f - underWaterFactor);
            buoyancyFactor *= Mathf.Sin(Time.timeSinceLevelLoad * 3f) * amplitude + (1f - amplitude);

			Vector3 buoyancy = Vector3.zero;

			var list = creationController.creationData.m_Attribute.m_FluidDisplacement;
			for (int i=0; i< list.Count; i++)
			{
				var vp = list[i];
				Vector3 wpos = transform.TransformPoint(vp.localPosition);
				buoyancy.y = vp.pos_volume * FluidDisplacement(wpos) * buoyancyFactor;
				rigidbody.AddForceAtPosition(buoyancy, wpos);
			}
		}

	}

}
