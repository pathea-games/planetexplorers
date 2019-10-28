using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 插值 range 参数
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Material/Range")]
	public class TweenMaterialRange : TweenMaterialFloat
	{
#if UNITY_EDITOR
		public override UnityEditor.ShaderUtil.ShaderPropertyType propertyType
		{
			get { return UnityEditor.ShaderUtil.ShaderPropertyType.Range; }
		}
#endif
	}
}