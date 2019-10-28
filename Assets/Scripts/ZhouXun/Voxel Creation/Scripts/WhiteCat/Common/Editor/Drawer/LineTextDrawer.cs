using UnityEditor;
using UnityEngine;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(LineTextAttribute), true)]
	public class LineTextDrawer : AttributeDecoratorDrawer<LineTextAttribute>
	{
		public override float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}


		public override void OnGUI(Rect rect)
		{
			EditorGUI.LabelField(rect, attribute.text);
		}
	}
}
