using System;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 贝塞尔路径节点
	/// </summary>
	[Serializable]
	public class BezierNode : PathNode
	{
		[SerializeField] float _localForwardTangent;		// 前向切线长度
		[SerializeField] float _localBackTangent;			// 后向切线长度


		// 构造方法
		public BezierNode(Vector3 localPosition, Quaternion localRotation, float localForwardTangent, float localBackTangent)
			: base(localPosition, localRotation)
		{
			this.localForwardTangent = localForwardTangent;
			this.localBackTangent = localBackTangent;
		}


		// 前向切线长度
		public float localForwardTangent
		{
			get { return _localForwardTangent; }
			set { _localForwardTangent = Mathf.Clamp(value, 0.001f, 1000); }
		}


		// 后向切线长度
		public float localBackTangent
		{
			get { return _localBackTangent; }
			set { _localBackTangent = Mathf.Clamp(value, 0.001f, 1000); }
		}


		// 本地前向切线端点
		public Vector3 localForwardPoint
		{
			get { return localPosition + localRotation * new Vector3(0, 0, _localForwardTangent); }
			set
			{
				value -= localPosition;
				localForwardTangent = value.magnitude;
				localRotation = Quaternion.LookRotation(value, localRotation * Vector3.up);
			}
		}


		// 本地后向切线端点
		public Vector3 localBackPoint
		{
			get { return localPosition + localRotation * new Vector3(0, 0, -_localBackTangent); }
			set
			{
				value = localPosition - value;
				localBackTangent = value.magnitude;
				localRotation = Quaternion.LookRotation(value, localRotation * Vector3.up);
			}
		}

	}
}