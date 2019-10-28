using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 乘客请求上车回调
	/// </summary>
	public interface IVCPassenger
	{
		// 上载具回调
		void GetOn(string sitAnimName, VCPBaseSeat seat);

		// 下载具回调
        void GetOff();

		// 同步乘客
		void Sync(Vector3 position, Quaternion rotation);

		// 如果是驾驶舱, 设置手的位置
		void SetHands(Transform left, Transform right);
	}
}