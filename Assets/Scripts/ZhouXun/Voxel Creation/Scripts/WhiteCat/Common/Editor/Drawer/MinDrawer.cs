using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(MinAttribute), true)]
	public class MinDrawer : AttributePropertyDrawer<MinAttribute>
	{
		protected override void SetFloat()
		{
			property.floatValue = Mathf.Max(floatValue, attribute.min);
		}


		protected override void SetInt()
		{
			property.intValue = Mathf.Max(intValue, (int)attribute.min);
		}
	}
}
