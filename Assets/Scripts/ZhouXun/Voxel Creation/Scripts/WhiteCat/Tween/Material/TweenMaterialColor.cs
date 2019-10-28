using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值颜色
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Material/Color")]
	public class TweenMaterialColor : TweenMaterialProperty
	{
		public Color from = Color.white;
		public Color to = Color.white;

		public int mask = -1;
		Color _temp;

		Material _material;
		Color _original;


		public override void OnTween(float factor)
		{
			if (_material = material)
			{
				_temp = _material.GetColor(propertyID);

				if (mask.GetBit(0)) _temp.r = from.r + (to.r - from.r) * factor;
				if (mask.GetBit(1)) _temp.g = from.g + (to.g - from.g) * factor;
				if (mask.GetBit(2)) _temp.b = from.b + (to.b - from.b) * factor;
				if (mask.GetBit(3)) _temp.a = from.a + (to.a - from.a) * factor;

				_material.SetColor(propertyID, _temp);
			}
		}


		public override void OnRecord()
		{
			if (_material = material)
			{
				_original = _material.GetColor(propertyID);
			}
		}

		public override void OnRestore()
		{
			if (_material = material)
			{
				_material.SetColor(propertyID, _original);
			}
		}


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			if (_material = material)
			{
				from = _material.GetColor(propertyID);
			}
		}

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			if (_material = material)
			{
				to = _material.GetColor(propertyID);
			}
		}

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (_material = material)
			{
				_material.SetColor(propertyID, from);
			}
		}

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (_material = material)
			{
				_material.SetColor(propertyID, to);
			}
		}


#if UNITY_EDITOR
		public override UnityEditor.ShaderUtil.ShaderPropertyType propertyType
		{
			get { return UnityEditor.ShaderUtil.ShaderPropertyType.Color; }
		}
#endif
	}
}