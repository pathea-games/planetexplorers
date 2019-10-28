using System;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 基数样条路径节点
	/// </summary>
	[Serializable]
	public class CardinalNode : PathNode
	{
		public CardinalNode(Vector3 localPosition, Quaternion localRotation)
			: base(localPosition, localRotation)
		{
		}
	}
}