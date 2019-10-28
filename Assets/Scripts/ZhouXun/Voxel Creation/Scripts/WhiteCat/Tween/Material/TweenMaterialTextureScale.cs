using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值纹理缩放
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Material/Texture Scale")]
	public class TweenMaterialTextureScale : TweenMaterialProperty
	{
		public Vector2 from = Vector2.one;
		public Vector2 to = Vector2.one;

		public int mask = -1;
		Vector2 _temp;

		Material _material;
		Vector2 _original;


		public override void OnTween(float factor)
		{
			if (_material = material)
			{
				_temp = _material.GetTextureScale(propertyName);

				if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
				if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

				_material.SetTextureScale(propertyName, _temp);
			}
		}


		public override void OnRecord()
		{
			if (_material = material)
			{
				_original = _material.GetTextureScale(propertyName);
			}
		}


		public override void OnRestore()
		{
			if (_material = material)
			{
				_material.SetTextureScale(propertyName, _original);
			}
		}


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			if (_material = material)
			{
				from = _material.GetTextureScale(propertyName);
			}
		}

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			if (_material = material)
			{
				to = _material.GetTextureScale(propertyName);
			}
		}

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (_material = material)
			{
				_material.SetTextureScale(propertyName, from);
			}
		}

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (_material = material)
			{
				_material.SetTextureScale(propertyName, to);
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