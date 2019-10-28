using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 脚本组件基类
	/// </summary>
	public class BaseBehaviour : MonoBehaviour
	{
		public RectTransform rectTransform
		{
			get { return transform as RectTransform; }
		}
	}
}
