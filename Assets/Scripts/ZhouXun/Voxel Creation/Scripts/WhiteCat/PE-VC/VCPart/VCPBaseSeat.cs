using UnityEngine;
using System;
using Pathea;

namespace WhiteCat
{
	/// <summary>
	/// 座位基类
	/// </summary>
	public abstract class VCPBaseSeat : VCPart
	{
		[SerializeField] Transform _pivotPoint;
		[SerializeField] string _sitAnimName;
		[SerializeField] GameObject _humanModel;
		public event Action getOffCallback;

		IVCPassenger _passenger;
		public IVCPassenger passenger { get { return _passenger; } }


		public CarrierController drivingController { get { return GetComponentInParent<CarrierController>(); } }


		/// <summary>
		/// 乘客上载具时调用此方法, 此后乘客会一直同步位置和旋转
		/// </summary>
		public virtual void GetOn(IVCPassenger passenger)
		{
			enabled = true;
			_passenger = passenger;
			passenger.GetOn(_sitAnimName, this);
		}


		/// <summary>
		/// 乘客下载具调用此方法, 组件同时关闭
		/// </summary>
		public void GetOff()
		{
			if (getOffCallback != null) getOffCallback();
			getOffCallback = null;

			_passenger.GetOff ();
			_passenger = null;
			enabled = false;
		}


		// 查找下车位置, 参数为当前人位置, 输出下车位置
		public bool FindGetOffPosition(out Vector3 position)
		{
			position = Vector3.zero;

			var controller = drivingController;
            
			// 速度限制
			//if (controller.rigidbody.velocity.magnitude > 10) return false;

            var carrier = controller.transform;
			var bounds = controller.creationController.bounds;
			var min = bounds.min;
			var max = bounds.max;

			//if (!CheckGetOffHeight(carrier, bounds)) return false;

			Vector3 loc;
			for (loc.y = min.y - 2.5f; loc.y < max.y + 2.5f; loc.y += 0.5f)
			{
				for (float offsetZ = 0.25f; offsetZ < (max.z - min.z) * 0.5f + 2.5f; offsetZ += 0.5f)
				{
					for (float offsetX = 0.25f; offsetX < (max.x - min.x) * 0.5f + 2.5f; offsetX += 0.5f)
					{
						loc.z = (max.z + min.z) * 0.5f + offsetZ;

						loc.x = (max.x + min.x) * 0.5f - offsetX;
						if (CheckGetOffPosition(carrier, position = carrier.TransformPoint(loc))) return true;

						loc.x = (max.x + min.x) * 0.5f + offsetX;
						if (CheckGetOffPosition(carrier, position = carrier.TransformPoint(loc))) return true;

						loc.z = (max.z + min.z) * 0.5f - offsetZ;
						if (CheckGetOffPosition(carrier, position = carrier.TransformPoint(loc))) return true;

						loc.x = (max.x + min.x) * 0.5f - offsetX;
						if (CheckGetOffPosition(carrier, position = carrier.TransformPoint(loc))) return true;
					}
				}
			}

			return false;
		}


		bool CheckGetOffPosition(Transform carrier, Vector3 pos)
		{
			Vector3 end = pos;
			end.y += 1.5f;
			if (!Physics.CheckCapsule(pos, end, 0.55f, PEVCConfig.instance.getOffLayerMask, QueryTriggerInteraction.Ignore))
			{
				// 是否有墙壁
				var direction = pos - transform.position;

				var hits = Physics.RaycastAll(
                    transform.position, direction, direction.magnitude, 
                    PEVCConfig.instance.getOffLayerMask, QueryTriggerInteraction.Ignore);

				bool ok = true;

				for (int i = 0; i < hits.Length; i++)
				{
					if (!hits[i].transform.IsChildOf(carrier))
					{
						ok = false;
						break;
					}
				}

				// 落脚点检测
				if (ok && Physics.Raycast(pos, Vector3.down, 5f, PEVCConfig.instance.getOffLayerMask, QueryTriggerInteraction.Ignore))
				{
					//Debug.DrawLine(pos + Vector3.up, pos + Vector3.down, Color.yellow, 10);
					//Debug.DrawLine(pos + Vector3.left, pos + Vector3.right, Color.yellow, 10);
					//Debug.DrawLine(pos + Vector3.forward, pos + Vector3.back, Color.yellow, 10);
					return true;
				}
			}
			return false;
		}


		public void SyncPassenger()
		{
			if (_passenger != null)
			{
				_passenger.Sync(_pivotPoint.position, _pivotPoint.rotation);
//				Pathea.PassengerCmpt entity = _passenger as Pathea.PassengerCmpt;
//				if(entity != null )
//				{
//					entity.UpdateHeadInfo();
//				}
			}
		}


		/// <summary>
		/// 同步乘客
		/// </summary>
//		protected virtual void LateUpdate()
//		{
//			_passenger.Sync(_pivotPoint.position, _pivotPoint.rotation);
//		}


		void Update()
		{
			SyncPassenger();
		}


		public void DestroyHumanModel()
		{
			Destroy(_humanModel);
		}
    }
}