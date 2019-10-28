using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(EulerAnglesAttribute), true)]
	public class EulerAnglesDrawer : AttributePropertyDrawer<EulerAnglesAttribute>
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Quaternion)
			{
				if (EditorGUIUtility.wideMode) return EditorGUIUtility.singleLineHeight;
				else return EditorGUIUtility.singleLineHeight * 2;
			}
			else return base.GetPropertyHeight(property, label);
		}


		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Quaternion)
			{
				if (!attribute.initialized)
				{
					attribute.initialized = true;
					attribute.eulerAngles = property.quaternionValue.eulerAngles;
				}
				EditorGUI.BeginChangeCheck();
				attribute.eulerAngles = EditorGUI.Vector3Field(rect, label, attribute.eulerAngles);
				if (EditorGUI.EndChangeCheck())
				{
					property.quaternionValue = Quaternion.Euler(attribute.eulerAngles);
				}
			}
			else base.OnGUI(rect, property, label);
		}
	}
}
