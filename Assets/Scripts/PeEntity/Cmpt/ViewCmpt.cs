using System;
using UnityEngine;

namespace Pathea
{
    public abstract class ViewCmpt : PeCmpt
	{
		/// <summary>
		/// 是否存在 View 层
		/// </summary>
		public abstract bool hasView { get; }


		/// <summary>
		/// 中心 Transform 对象. 子类实现时应当保证 View 层存在时返回有效的 transform 对象, 否则返回 null
		/// </summary>
		public abstract Transform centerTransform { get; }


		/// <summary>
		/// 中心位置. 由访问者保证仅在 View 层存在时访问此属性
		/// </summary>
		public Vector3 centerPosition { get { return (centerTransform != null) ? centerTransform.position : transform.position; } }
    }
}