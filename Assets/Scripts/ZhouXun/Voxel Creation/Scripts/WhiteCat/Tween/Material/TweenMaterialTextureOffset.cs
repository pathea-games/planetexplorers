using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值纹理偏移
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Material/Texture Offset")]
	public class TweenMaterialTextureOffset : TweenMaterialProperty
	{
		public Vector2 from;
		public Vector2 to;

		public int mask = -1;
		Vector2 _temp;

		Material _material;
		Vector2 _original;


		public override void OnTween(float factor)
		{
			if (_material = material)
			{
				_temp = _material.GetTextureOffset(propertyName);

				if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
				if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

				_material.SetTextureOffset(propertyName, _temp);
			}
		}


		public override void OnRecord()
		{
			if (_material = material)
			{
				_original = _material.GetTextureOffset(propertyName);
			}
		}


		public override void OnRestore()
		{
			if (_material = material)
			{
				_material.SetTextureOffset(propertyName, _original);
			}
		}


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			if (_material = material)
			{
				from = _material.GetTextureOffset(propertyName);
			}
		}

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			if (_material = material)
			{
				to = _material.GetTextureOffset(propertyName);
			}
		}

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (_material = material)
			{
				_material.SetTextureOffset(propertyName, from);
			}
		}

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (_material = material)
			{
				_material.SetTextureOffset(propertyName, to);
			}
		}


#if UNITY_EDITOR
		public override UnityEditor.ShaderUtil.ShaderPropertyType propertyType
		{
			get { return UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv; }
		}
#endif
	}
}