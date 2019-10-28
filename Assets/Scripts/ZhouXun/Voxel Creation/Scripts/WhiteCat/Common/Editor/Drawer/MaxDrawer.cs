using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(MaxAttribute), true)]
	public class MaxDrawer : AttributePropertyDrawer<MaxAttribute>
	{
		protected override void SetFloat()
		{
			property.floatValue = Mathf.Min(floatValue, attribute.max);
		}


		protected override void SetInt()
		{
			property.intValue = Mathf.Min(intValue, (int)attribute.max);
		}
	}
}
