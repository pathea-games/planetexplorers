using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值四维向量
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Material/Vector")]
	public class TweenMaterialVector : TweenMaterialProperty
	{
		public Vector4 from;
		public Vector4 to;

		public int mask = -1;
		Vector4 _temp;

		Material _material;
		Vector4 _original;


		public override void OnTween(float factor)
		{
			if (_material = material)
			{
				_temp = _material.GetVector(propertyID);

				if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
				if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
				if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;
				if (mask.GetBit(3)) _temp.w = from.w + (to.w - from.w) * factor;

				_material.SetVector(propertyID, _temp);
			}
		}


		public override void OnRecord()
		{
			if (_material = material)
			{
				_original = _material.GetVector(propertyID);
			}
		}

		public override void OnRestore()
		{
			if (_material = material)
			{
				_material.SetVector(propertyID, _original);
			}
		}


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			if (_material = material)
			{
				from = _material.GetVector(propertyID);
			}
		}

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			if (_material = material)
			{
				to = _material.GetVector(propertyID);
			}
		}

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (_material = material)
			{
				_material.SetVector(propertyID, from);
			}
		}

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (_material = material)
			{
				_material.SetVector(propertyID, to);
			}
		}


#if UNITY_EDITOR
		public override UnityEditor.ShaderUtil.ShaderPropertyType propertyType
		{
			get { return UnityEditor.ShaderUtil.ShaderPropertyType.Vector; }
		}
#endif
	}
}