using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using WhiteCat.ReflectionExtension;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenInterpolator))]
	class TweenInterpolatorEditor : Editor<TweenInterpolator>
	{
		const float step = 0.02f;
		const float showCurveHeight = 120;
		const float hideCurveHeight = 18;

		static Rect rect1, rect2, solidRect;
		static Vector3 point1, point2;
		static Matrix4x4 matrix, offset = Utility.identityMatrix;

		static Color rectBackColor = new Color(0.125f, 0.125f, 0.125f, 0.5f);
		static Color rectLineColor = new Color(1, 1, 1, 0.25f);
		static Color range01Color = new Color(1, 1, 1, 0.1f);
		static Color curveColor = new Color(0, 1, 0, 1);
		static Color timeColor = new Color(1, 0.25f, 0, 1);
		static Color factorColor = new Color(0, 0.75f, 1, 1);
		static Color ProgressColor = new Color(0.25f, 0.75f, 1, 0.5f);

		SerializedProperty method;
		SerializedProperty customCurve;
		SerializedProperty delay;
		SerializedProperty duration;
		SerializedProperty speed;
		SerializedProperty wrapMode;
		SerializedProperty timeLine;
		SerializedProperty onArriveAtBeginning;
		SerializedProperty onArriveAtEnding;

		FieldInfo isDraggingField;
		bool isDragging;
		bool lastRequiresConstantRepaint;

		bool showEvents;
		bool showCurve;

		float minValue, maxValue, scale;
		float topValue, bottomValue;


		void OnEnable()
		{
			isDraggingField = target.GetFieldInfo("_isDragging");

			method = serializedObject.FindProperty("method");
			customCurve = serializedObject.FindProperty("_customCurve");
			delay = serializedObject.FindProperty("_delay");
			duration = serializedObject.FindProperty("_duration");
			speed = serializedObject.FindProperty("_speed");
			wrapMode = serializedObject.FindProperty("wrapMode");
			timeLine = serializedObject.FindProperty("timeLine");
			onArriveAtBeginning = serializedObject.FindProperty("onArriveAtBeginning");
			onArriveAtEnding = serializedObject.FindProperty("onArriveAtEnding");

			UpdateMinMax();
		}


		void OnDisable()
		{
			if(!Application.isPlaying) target.isPlaying = false;
		}


		void UpdateMinMax()
		{
			float r;
			minValue = float.MaxValue;
			maxValue = float.MinValue;

			for (float i = 0; i <= 1; i += step)
			{
				r = target.Interpolate(i);
				if (r < minValue) minValue = r;
				if (r > maxValue) maxValue = r;
			}
		}


		void DrawCurve(Rect rect)
		{
			point1.x = rect.x;
			point2.x = rect.width;

			scale = maxValue - minValue;
			if (scale < 1)
			{
				scale = rect.height;
				point2.y = -scale;
				if (minValue < 0)
				{
					point1.y = rect.yMax - minValue * point2.y;
					bottomValue = minValue;
					topValue = minValue + 1;
				}
				else if (maxValue > 1)
				{
					point1.y = rect.yMin - maxValue * point2.y;
					topValue = maxValue;
					bottomValue = maxValue - 1;
				}
				else 
				{
					point1.y = rect.yMax;
					topValue = 1;
					bottomValue = 0;
				}
			}
			else
			{
				scale = rect.height / scale;
				point2.y = -scale;
				point1.y = rect.yMax - minValue * point2.y;
				topValue = maxValue;
				bottomValue = minValue;
			}

			if(topValue > 0 && bottomValue < 1)
			{
				float y = ((topValue < 1 ? topValue : 1) - bottomValue) * scale;
				solidRect.Set(rect.x, rect.yMax - y, rect.width, y - (bottomValue > 0 ? 0 : -bottomValue) * scale);
				EditorGUI.DrawRect(solidRect, range01Color);
			}

			point2.z = 1;
			offset.SetTRS(point1, Quaternion.identity, point2);
			point2.z = 0;

			matrix = Handles.matrix;
			Handles.matrix = matrix * offset;

			Handles.color = curveColor;
			point1.x = 0;
			point1.y = target.Interpolate(0);

			for (point2.x = step; point2.x <= 1.0f; point2.x += step)
			{
				point2.y = target.Interpolate(point2.x);
				Handles.DrawLine(point1, point2);

				point1.x = point2.x;
				point1.y = point2.y;
			}

			Handles.matrix = matrix;

			if (target.isPlaying || Application.isPlaying)
			{
				solidRect.Set(rect.x, rect.yMax - (target.Interpolate(target.normalizedTime) - bottomValue) * scale, rect.width, 1);
				EditorGUI.DrawRect(solidRect, factorColor);
				solidRect.Set(rect.x + target.normalizedTime * rect.width, rect.y, 1, rect.height);
				EditorGUI.DrawRect(solidRect, timeColor);
			}
		}


		public override bool RequiresConstantRepaint()
		{
			if (target.isPlaying || isDragging)
			{
				return lastRequiresConstantRepaint = true;
			}
			else
			{
				if (lastRequiresConstantRepaint) Repaint();
				return lastRequiresConstantRepaint = false;
			}
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(method);
			if(target.method == TweenMethod.CustomCurve)
			{
				EditorGUILayout.PropertyField(customCurve);
			}
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				UpdateMinMax();
				serializedObject.Update();
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(delay);
			EditorGUILayout.PropertyField(duration);
			EditorGUILayout.PropertyField(speed);
			EditorGUILayout.PropertyField(wrapMode);
			EditorGUILayout.PropertyField(timeLine);

			EditorGUILayout.Space();
			if (showEvents = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showEvents, "Events"))
			{
				EditorGUILayout.PropertyField(onArriveAtBeginning);
				EditorGUILayout.PropertyField(onArriveAtEnding);
			}
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();


			rect2 = rect1 = EditorGUILayout.GetControlRect(true, showCurve ? showCurveHeight : hideCurveHeight);
			rect2.xMin += EditorGUIUtility.labelWidth;
			rect1.height = hideCurveHeight;

			rect1.xMax = rect2.xMin - 12 - rect1.height;
			target.isPlaying = GUI.Toggle(rect1, target.isPlaying, "Play", "Button");

			rect1.Set(rect2.x - 4, rect2.y + 1, 1, hideCurveHeight);
			showCurve = EditorGUI.Foldout(rect1, showCurve, GUIContent.none);

			EditorGUI.DrawRect(rect2, rectBackColor);
			EditorKit.DrawWireRect(rect2, rectLineColor, 1);

			if (Event.current.type == EventType.mouseDown || isDragging)
			{
				Vector2 mouse = Event.current.mousePosition - rect2.min;
				if (!isDragging && mouse.x >= 0 && mouse.x <= rect2.width && mouse.y >= 0 && mouse.y <= rect2.height)
				{
					if(!Application.isPlaying) target.isPlaying = true;
					isDraggingField.SetValue(target, isDragging = true);
				}
				if (isDragging) target.normalizedTime = mouse.x / rect2.width;
			}
			if (Event.current.rawType == EventType.mouseUp)
			{
				isDraggingField.SetValue(target, isDragging = false);
			}

			if(showCurve)
			{
				DrawCurve(rect2);
			}
			else
			{
				if (target.isPlaying || Application.isPlaying)
				{
					rect1 = rect2;
					rect1.yMin = rect2.yMin + 1;
					rect1.yMax = rect2.yMax - 1;
					rect1.x = rect2.x + 1;
					rect1.width = (rect2.width - 2) * target.normalizedTime;
					EditorGUI.DrawRect(rect1, ProgressColor);
				}
			}
		}
	}
}
