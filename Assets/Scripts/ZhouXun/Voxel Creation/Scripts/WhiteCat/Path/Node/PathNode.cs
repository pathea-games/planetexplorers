using System;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 路径节点基类
	/// </summary>
	[Serializable]
	public class PathNode
	{
		public Vector3 localPosition;		// 本地位置
		public Quaternion localRotation;	// 本地旋转


		/// 构造方法
		public PathNode(Vector3 localPosition, Quaternion localRotation)
		{
			this.localPosition = localPosition;
			this.localRotation = localRotation;
		}
	}
}