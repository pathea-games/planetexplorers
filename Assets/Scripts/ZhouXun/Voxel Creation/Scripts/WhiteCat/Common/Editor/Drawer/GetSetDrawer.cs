using UnityEngine;
using UnityEditor;
using WhiteCat.ReflectionExtension;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(GetSetAttribute), true)]
	public class GetSetDrawer : AttributePropertyDrawer<GetSetAttribute>
	{
		T GetValue<T>()
		{
			return (T)attribute.propertyInfo.GetValue(property.serializedObject.targetObject, null);
		}


		void SetValue<T>(T value)
		{
			Object target = property.serializedObject.targetObject;
			Undo.RecordObject(target, attribute.undoString);
			attribute.propertyInfo.SetValue(target, value, null);
			EditorUtility.SetDirty(target);
		}


		protected override void GetAnimationCurve() { animationCurveValue = GetValue<AnimationCurve>(); }
		protected override void GetBool() { boolValue = GetValue<bool>(); }
		protected override void GetBounds() { boundsValue = GetValue<Bounds>(); }
		protected override void GetColor() { colorValue = GetValue<Color>(); }
		protected override void GetEnum() { enumValue = GetValue<System.Enum>(); }
		protected override void GetInt() { intValue = GetValue<int>(); }
		protected override void GetLayerMask() { layerMaskValue = GetValue<LayerMask>(); }
		protected override void GetFloat() { floatValue = GetValue<float>(); }
		protected override void GetObjectReference() { objectReferenceValue = GetValue<UnityEngine.Object>(); }
		protected override void GetRect() { rectValue = GetValue<Rect>(); }
		protected override void GetString() { stringValue = GetValue<string>(); }
		protected override void GetVector2() { vector2Value = GetValue<Vector2>(); }
		protected override void GetVector3() { vector3Value = GetValue<Vector3>(); }
		protected override void GetVector4() { vector4Value = GetValue<Vector4>(); }



		protected override void SetAnimationCurve() { SetValue(animationCurveValue); }
		protected override void SetBool() { SetValue(boolValue); }
		protected override void SetBounds() { SetValue(boundsValue); }
		protected override void SetColor() { SetValue(colorValue); }
		protected override void SetEnum() { SetValue(enumValue); }
		protected override void SetInt() { SetValue(intValue); }
		protected override void SetLayerMask() { SetValue(layerMaskValue); }
		protected override void SetFloat() { SetValue(floatValue); }
		protected override void SetObjectReference() { SetValue(objectReferenceValue); }
		protected override void SetRect() { SetValue(rectValue); }
		protected override void SetString() { SetValue(stringValue); }
		protected override void SetVector2() { SetValue(vector2Value); }
		protected override void SetVector3() { SetValue(vector3Value); }
		protected override void SetVector4() { SetValue(vector4Value); }



		void Initialize(SerializedProperty property)
		{
			BasePropertyDrawer.property = property;
			if (attribute.propertyInfo == null)
			{
				Object target = property.serializedObject.targetObject;
				attribute.propertyInfo = target.GetPropertyInfo(attribute.propertyName);
				attribute.undoString = target.ToString();
				attribute.type = EditorKit.GetPropertyType(attribute.propertyInfo.PropertyType);
			}
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			return GetHeightOfPropertyType(attribute.type);
		}


		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			OnGUIOfPropertyType(attribute.type, rect, label);
		}
	}
}
