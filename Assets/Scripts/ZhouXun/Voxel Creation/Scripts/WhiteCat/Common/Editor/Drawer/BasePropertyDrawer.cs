using System;
using UnityEditor;
using UnityEngine;

namespace WhiteCat
{
	//[CustomPropertyDrawer(typeof(LayerMask), true)]
	public class BasePropertyDrawer : PropertyDrawer
	{
		#region Params of Get&Set Methods

		protected static SerializedProperty property;
		protected static AnimationCurve animationCurveValue;
		protected static bool boolValue;
		protected static Bounds boundsValue;
		protected static Color colorValue;
		protected static Enum enumValue;
		protected static int intValue;
		protected static LayerMask layerMaskValue;
		protected static float floatValue;
		protected static UnityEngine.Object objectReferenceValue;
		protected static Rect rectValue;
		protected static string stringValue;
		protected static Vector2 vector2Value;
		protected static Vector3 vector3Value;
		protected static Vector4 vector4Value;

		#endregion



		#region Get Methods

		protected virtual void GetAnimationCurve() { animationCurveValue = property.animationCurveValue; }
		protected virtual void GetBool() { boolValue = property.boolValue; }
		protected virtual void GetBounds() { boundsValue = property.boundsValue; }
		protected virtual void GetColor() { colorValue = property.colorValue; }
		protected virtual void GetEnum() { enumValue = Enum.ToObject(fieldInfo.FieldType, property.intValue) as Enum; }
		protected virtual void GetInt() { intValue = property.intValue; }
		protected virtual void GetLayerMask() { layerMaskValue = property.intValue; }
		protected virtual void GetFloat() { floatValue = property.floatValue; }
		protected virtual void GetObjectReference() { objectReferenceValue = property.objectReferenceValue; }
		protected virtual void GetRect() { rectValue = property.rectValue; }
		protected virtual void GetString() { stringValue = property.stringValue; }
		protected virtual void GetVector2() { vector2Value = property.vector2Value; }
		protected virtual void GetVector3() { vector3Value = property.vector3Value; }
		protected virtual void GetVector4() { vector4Value = property.vector4Value; }

		#endregion



		#region Set Methods

		protected virtual void SetAnimationCurve() { property.animationCurveValue = animationCurveValue; }
		protected virtual void SetBool() { property.boolValue = boolValue; }
		protected virtual void SetBounds() { property.boundsValue = boundsValue; }
		protected virtual void SetColor() { property.colorValue = colorValue; }
		protected virtual void SetEnum() { property.intValue = Convert.ToInt32(enumValue); }
		protected virtual void SetInt() { property.intValue = intValue; }
		protected virtual void SetLayerMask() { property.intValue = layerMaskValue; }
		protected virtual void SetFloat() { property.floatValue = floatValue; }
		protected virtual void SetObjectReference() { property.objectReferenceValue = objectReferenceValue; }
		protected virtual void SetRect() { property.rectValue = rectValue; }
		protected virtual void SetString() { property.stringValue = stringValue; }
		protected virtual void SetVector2() { property.vector2Value = vector2Value; }
		protected virtual void SetVector3() { property.vector3Value = vector3Value; }
		protected virtual void SetVector4() { property.vector4Value = vector4Value; }

		#endregion



		protected float GetHeightOfPropertyType(SerializedPropertyType type)
		{
			switch (type)
			{
				case SerializedPropertyType.Bounds:
					return EditorGUIUtility.singleLineHeight * 3;

				case SerializedPropertyType.Rect:
					if (EditorGUIUtility.wideMode) return EditorGUIUtility.singleLineHeight * 2;
					return EditorGUIUtility.singleLineHeight * 3;

				case SerializedPropertyType.Vector2:
				case SerializedPropertyType.Vector3:
					if (EditorGUIUtility.wideMode) return EditorGUIUtility.singleLineHeight;
					return EditorGUIUtility.singleLineHeight * 2;

				case SerializedPropertyType.Vector4:
					return EditorGUIUtility.singleLineHeight * 2;

				default:
					return EditorGUIUtility.singleLineHeight;
			}
		}



		protected void OnGUIOfPropertyType(SerializedPropertyType type, Rect rect, GUIContent label)
		{
			switch (type)
			{
				case SerializedPropertyType.AnimationCurve:
					EditorGUI.BeginChangeCheck(); GetAnimationCurve();
					animationCurveValue = EditorGUI.CurveField(rect, label, animationCurveValue);
					if (EditorGUI.EndChangeCheck()) SetAnimationCurve();
					return;

				case SerializedPropertyType.Boolean:
					EditorGUI.BeginChangeCheck(); GetBool();
					boolValue = EditorGUI.Toggle(rect, label, boolValue);
					if (EditorGUI.EndChangeCheck()) SetBool();
					return;

				case SerializedPropertyType.Bounds:
					EditorGUI.BeginChangeCheck(); GetBounds();
					boundsValue = EditorGUI.BoundsField(rect, label, boundsValue);
					if (EditorGUI.EndChangeCheck()) SetBounds();
					return;

				case SerializedPropertyType.Color:
					EditorGUI.BeginChangeCheck(); GetColor();
					colorValue = EditorGUI.ColorField(rect, label, colorValue);
					if (EditorGUI.EndChangeCheck()) SetColor();
					return;

				case SerializedPropertyType.Enum:
					EditorGUI.BeginChangeCheck(); GetEnum();
					enumValue = EditorGUI.EnumPopup(rect, label, enumValue);
					if (EditorGUI.EndChangeCheck()) SetEnum();
					return;

				case SerializedPropertyType.Float:
					EditorGUI.BeginChangeCheck(); GetFloat();
					floatValue = EditorGUI.FloatField(rect, label, floatValue);
					if (EditorGUI.EndChangeCheck()) SetFloat();
					return;

				case SerializedPropertyType.Integer:
					EditorGUI.BeginChangeCheck(); GetInt();
					intValue = EditorGUI.IntField(rect, label, intValue);
					if (EditorGUI.EndChangeCheck()) SetInt();
					return;

				case SerializedPropertyType.LayerMask:
					SerializedProperty property = BasePropertyDrawer.property;
					GetLayerMask();
					EditorKit.LayerMaskField(rect, label, layerMaskValue,
						mask =>
						{
							property.serializedObject.Update();
							BasePropertyDrawer.property = property;
							layerMaskValue = mask;
							SetLayerMask();
							property.serializedObject.ApplyModifiedProperties();
						});
					return;

				case SerializedPropertyType.ObjectReference:
					EditorGUI.BeginChangeCheck(); GetObjectReference();
					objectReferenceValue = EditorGUI.ObjectField(rect, label, objectReferenceValue,
						fieldInfo.FieldType, !EditorUtility.IsPersistent(BasePropertyDrawer.property.serializedObject.targetObject));
					if (EditorGUI.EndChangeCheck()) SetObjectReference();
					return;

				case SerializedPropertyType.Rect:
					EditorGUI.BeginChangeCheck(); GetRect();
					rectValue = EditorGUI.RectField(rect, label, rectValue);
					if (EditorGUI.EndChangeCheck()) SetRect();
					return;

				case SerializedPropertyType.String:
					EditorGUI.BeginChangeCheck(); GetString();
					stringValue = EditorGUI.TextField(rect, label, stringValue);
					if (EditorGUI.EndChangeCheck()) SetString();
					return;

				case SerializedPropertyType.Vector2:
					EditorGUI.BeginChangeCheck(); GetVector2();
					vector2Value = EditorGUI.Vector2Field(rect, label, vector2Value);
					if (EditorGUI.EndChangeCheck()) SetVector2();
					return;

				case SerializedPropertyType.Vector3:
					EditorGUI.BeginChangeCheck(); GetVector3();
					vector3Value = EditorGUI.Vector3Field(rect, label, vector3Value);
					if (EditorGUI.EndChangeCheck()) SetVector3();
					return;

				case SerializedPropertyType.Vector4:
					EditorGUI.BeginChangeCheck(); GetVector4();
					vector4Value = EditorGUI.Vector4Field(rect, label.text, vector4Value);
					if (EditorGUI.EndChangeCheck()) SetVector4();
					return;

				default:
					EditorGUI.LabelField(rect, label.text, "Not supported");
					return;
			}
		}



		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			BasePropertyDrawer.property = property;
			return GetHeightOfPropertyType(property.propertyType);
		}



		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			BasePropertyDrawer.property = property;
			OnGUIOfPropertyType(property.propertyType, rect, label);
		}

	}
}
