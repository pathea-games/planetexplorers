using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(ClampAttribute), true)]
	public class ClampDrawer : AttributePropertyDrawer<ClampAttribute>
	{
		protected override void SetFloat()
		{
			property.floatValue = Mathf.Clamp(floatValue, attribute.min, attribute.max);
		}


		protected override void SetInt()
		{
			property.intValue = Mathf.Clamp(intValue, (int)attribute.min, (int)attribute.max);
		}
	}
}
