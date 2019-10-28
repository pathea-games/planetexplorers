using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 插值 float 参数
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Material/Float")]
	public class TweenMaterialFloat : TweenMaterialProperty
	{
		public float from;
		public float to;

		Material _material;
		float _original;


		public override void OnTween(float factor)
		{
			if (_material = material)
			{
				_material.SetFloat(propertyID, from + (to - from) * factor);
			}
		}
		

		public override void OnRecord()
		{
			if (_material = material)
			{
				_original = _material.GetFloat(propertyID);
			}
		}


		public override void OnRestore()
		{
			if (_material = material)
			{
				_material.SetFloat(propertyID, _original);
			}
		}


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			if (_material = material)
			{
				from = _material.GetFloat(propertyID);
			}
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			if (_material = material)
			{
				to = _material.GetFloat(propertyID);
			}
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (_material = material)
			{
				_material.SetFloat(propertyID, from);
			}
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (_material = material)
			{
				_material.SetFloat(propertyID, to);
			}
		}


#if UNITY_EDITOR
		public override UnityEditor.ShaderUtil.ShaderPropertyType propertyType
		{
			get { return UnityEditor.ShaderUtil.ShaderPropertyType.Float; }
		}
#endif
	}
}