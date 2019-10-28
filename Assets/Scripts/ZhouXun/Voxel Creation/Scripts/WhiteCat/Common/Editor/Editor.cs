using System;
using UnityEngine;
using UnityEditor;

namespace WhiteCat
{
	// Inspector 编辑器基类
	public class Editor<Target> : UnityEditor.Editor where Target : UnityEngine.Object
	{
		// 关联的编辑对象
		public new Target target { get { return base.target as Target; } }


		// undo 字符串
		public string undoString
		{
			get { return _undoString == null ? _undoString = target.ToString() : _undoString; }
			set { _undoString = value; }
		}
		string _undoString;


		// 调试输出
		public static void print(object message) { Debug.Log(message); }


		// 便捷编辑器绘制方法，如果检测到 draw 方法修改了值，将执行 apply 方法，并处理撤销和保存提示
		public void SmartDraw<T>(Func<T> draw, Action<T> apply)
		{
			EditorGUI.BeginChangeCheck();
			T value = draw();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, undoString);
				apply(value);
				EditorUtility.SetDirty(target);
			}
		}




		#region Inspector SmartDraw Functions


		public void DrawVector3Field(Rect rect, Vector3 value, Action<Vector3> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.Vector3Field(rect, GUIContent.none, value)
				: EditorGUI.Vector3Field(rect, label, value), apply);
		}

		public void DrawVector3FieldLayout(Vector3 value, Action<Vector3> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.Vector3Field(GUIContent.none, value, options)
				: EditorGUILayout.Vector3Field(label, value, options), apply);
		}

		public void DrawVector2Field(Rect rect, Vector2 value, Action<Vector2> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.Vector2Field(rect, GUIContent.none, value)
				: EditorGUI.Vector2Field(rect, label, value), apply);
		}

		public void DrawVector2FieldLayout(Vector2 value, Action<Vector2> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.Vector2Field(GUIContent.none, value, options)
				: EditorGUILayout.Vector2Field(label, value, options), apply);
		}

		public void DrawEnumPopup(Rect rect, Enum value, Action<Enum> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.EnumPopup(rect, value)
				: EditorGUI.EnumPopup(rect, label, value), apply);
		}

		public void DrawEnumPopupLayout(Enum value, Action<Enum> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.EnumPopup(value, options)
				: EditorGUILayout.EnumPopup(label, value, options), apply);
		}

		public void DrawIntField(Rect rect, int value, Action<int> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.IntField(rect, value)
				: EditorGUI.IntField(rect, label, value), apply);
		}

		public void DrawIntFieldLayout(int value, Action<int> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.IntField(value, options)
				: EditorGUILayout.IntField(label, value, options), apply);
		}

		public void DrawIntPopup(Rect rect, int value, string[] items, Action<int> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.Popup(rect, value, items)
				: EditorGUI.Popup(rect, label, value, items), apply);
		}

		public void DrawIntPopupLayout(int value, string[] items, Action<int> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.Popup(value, items, options)
				: EditorGUILayout.Popup(label, value, items, options), apply);
		}

		public void DrawFloatField(Rect rect, float value, Action<float> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.FloatField(rect, value)
				: EditorGUI.FloatField(rect, label, value), apply);
		}

		public void DrawFloatFieldLayout(float value, Action<float> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.FloatField(value, options)
				: EditorGUILayout.FloatField(label, value, options), apply);
		}

		public void DrawSlider(Rect rect, float value, float left, float right, Action<float> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.Slider(rect, value, left, right)
				: EditorGUI.Slider(rect, label, value, left, right), apply);
		}

		public void DrawSliderLayout(float value, float left, float right, Action<float> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.Slider(value, left, right, options)
				: EditorGUILayout.Slider(label, value, left, right, options), apply);
		}

		public void DrawToggle(Rect rect, bool value, Action<bool> apply, string label = null)
		{
			SmartDraw(() => label == null
				? EditorGUI.Toggle(rect, value)
				: EditorGUI.Toggle(rect, label, value), apply);
		}

		public void DrawToggleLayout(bool value, Action<bool> apply, string label = null, params GUILayoutOption[] options)
		{
			SmartDraw(() => label == null
				? EditorGUILayout.Toggle(value, options)
				: EditorGUILayout.Toggle(label, value, options), apply);
		}


		public void DrawObjectField<T>(Rect rect, T value, bool allowSceneObject, Action<T> apply, string label = null)
			where T : UnityEngine.Object
		{
			SmartDraw<T>(() => label == null
				? EditorGUI.ObjectField(rect, value, typeof(T), allowSceneObject) as T
				: EditorGUI.ObjectField(rect, label, value, typeof(T), allowSceneObject) as T, apply);
		}


		public void DrawObjectFieldLayout<T>(T value, bool allowSceneObject, Action<T> apply, string label = null, params GUILayoutOption[] options)
			where T : UnityEngine.Object
		{
			SmartDraw<T>(() => label == null
				? EditorGUILayout.ObjectField(value, typeof(T), allowSceneObject, options) as T
				: EditorGUILayout.ObjectField(label, value, typeof(T), allowSceneObject, options) as T, apply);
		}


		#endregion Inspector SmartDraw Functions
	}

}
