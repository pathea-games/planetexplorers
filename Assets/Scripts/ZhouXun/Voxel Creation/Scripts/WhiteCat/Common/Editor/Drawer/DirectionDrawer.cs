using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(DirectionAttribute), true)]
	public class DirectionDrawer : AttributePropertyDrawer<DirectionAttribute>
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Vector3)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			else return base.GetPropertyHeight(property, label);
		}


		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if(property.propertyType == SerializedPropertyType.Vector3)
			{
				if(!attribute.initialized)
				{
					attribute.eulerAngles = Quaternion.LookRotation(property.vector3Value).eulerAngles;
					attribute.initialized = true;
				}

				rect = EditorGUI.PrefixLabel(rect, label);
				rect.width *= 0.5f;

				EditorGUI.BeginChangeCheck();
				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 13;
				attribute.eulerAngles.x = EditorGUI.FloatField(rect, "X", attribute.eulerAngles.x);
				rect.x += rect.width + 2;
				attribute.eulerAngles.y = EditorGUI.FloatField(rect, "Y", attribute.eulerAngles.y);
				EditorGUIUtility.labelWidth = labelWidth;
				if(EditorGUI.EndChangeCheck())
				{
					property.vector3Value = Quaternion.Euler(attribute.eulerAngles) * Vector3.forward * attribute.length;
				}
			}
			else base.OnGUI(rect, property, label);
		}
	}
}
