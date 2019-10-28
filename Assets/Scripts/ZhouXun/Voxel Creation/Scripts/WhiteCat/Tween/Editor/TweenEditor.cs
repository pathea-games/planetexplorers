using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	class TweenEditor<Target> : Editor<Target> where Target : TweenBase
	{
		SerializedProperty interpolator;


		protected virtual void OnEnable()
		{
			interpolator = serializedObject.FindProperty("_interpolator");
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(interpolator);
			serializedObject.ApplyModifiedProperties();
		}


		// -----------------------------------------------------------------------------------


		const float toggleRatio = 0.1f;
		const float intervalRatio = 0.1f;
		const float fromLabelWidth = 35;
		const float toLabelWidth = 20;

		static Rect rect;
		static float lineWidth;
		static float inputWidth;
		static float labelWidth;


		public static void DrawFloatChannel(ref int mask, int bit, string label, ref float from, ref float to)
		{
			labelWidth = EditorGUIUtility.labelWidth;

			rect = EditorGUILayout.GetControlRect();
			lineWidth = rect.width;
			inputWidth = (lineWidth * (1 - intervalRatio * 2 - toggleRatio) - fromLabelWidth - toLabelWidth) * 0.5f;

			rect.width = lineWidth * toggleRatio;
			mask = mask.SetBit(bit, EditorGUI.ToggleLeft(rect, label, mask.GetBit(bit)));

			EditorGUIUtility.labelWidth = fromLabelWidth;
			rect.x += lineWidth * (toggleRatio + intervalRatio);
			rect.width = inputWidth + fromLabelWidth;
			from = EditorGUI.FloatField(rect, "From", from);

			EditorGUIUtility.labelWidth = toLabelWidth;
			rect.x = rect.xMax + lineWidth * intervalRatio;
			rect.width = inputWidth + toLabelWidth;
			to = EditorGUI.FloatField(rect, "To", to);

			EditorGUIUtility.labelWidth = labelWidth;
		}


		public static void DrawColorChannel(ref int mask, int bit, string label, ref float from, ref float to)
		{
			labelWidth = EditorGUIUtility.labelWidth;

			rect = EditorGUILayout.GetControlRect();
			lineWidth = rect.width;
			inputWidth = (lineWidth * (1 - intervalRatio * 2 - toggleRatio) - fromLabelWidth - toLabelWidth) * 0.5f;

			rect.width = lineWidth * toggleRatio;
			mask = mask.SetBit(bit, EditorGUI.ToggleLeft(rect, label, mask.GetBit(bit)));

			EditorGUIUtility.labelWidth = fromLabelWidth;
			rect.x += lineWidth * (toggleRatio + intervalRatio);
			rect.width = inputWidth + fromLabelWidth;
			from = Mathf.Clamp(EditorGUI.IntField(rect, "From", (int)(from * 255)), 0, 255) / 255.0f;

			EditorGUIUtility.labelWidth = toLabelWidth;
			rect.x = rect.xMax + lineWidth * intervalRatio;
			rect.width = inputWidth + toLabelWidth;
			to = Mathf.Clamp(EditorGUI.IntField(rect, "To", (int)(to * 255)), 0, 255) / 255.0f;

			EditorGUIUtility.labelWidth = labelWidth;
		}


		public static void DrawColorChannels(ref int mask, ref Color from, ref Color to)
		{
			DrawColorChannel(ref mask, 0, "R", ref from.r, ref to.r);
			DrawColorChannel(ref mask, 1, "G", ref from.g, ref to.g);
			DrawColorChannel(ref mask, 2, "B", ref from.b, ref to.b);
			DrawColorChannel(ref mask, 3, "A", ref from.a, ref to.a);

			labelWidth = EditorGUIUtility.labelWidth;

			rect = EditorGUILayout.GetControlRect();
			lineWidth = rect.width;
			inputWidth = (lineWidth * (1 - intervalRatio * 2 - toggleRatio) - fromLabelWidth - toLabelWidth) * 0.5f;

			EditorGUIUtility.labelWidth = fromLabelWidth;
			rect.x += lineWidth * (toggleRatio + intervalRatio);
			rect.width = inputWidth + fromLabelWidth;
			from = EditorGUI.ColorField(rect, "From", from);

			EditorGUIUtility.labelWidth = toLabelWidth;
			rect.x = rect.xMax + lineWidth * intervalRatio;
			rect.width = inputWidth + toLabelWidth;
			to = EditorGUI.ColorField(rect, "To", to);

			EditorGUIUtility.labelWidth = labelWidth;
		}


		public static void DrawVector2Channels(ref int mask, ref Vector2 from, ref Vector2 to)
		{
			DrawFloatChannel(ref mask, 0, "X", ref from.x, ref to.x);
			DrawFloatChannel(ref mask, 1, "Y", ref from.y, ref to.y);
		}


		public static void DrawVector3Channels(ref int mask, ref Vector3 from, ref Vector3 to)
		{
			DrawFloatChannel(ref mask, 0, "X", ref from.x, ref to.x);
			DrawFloatChannel(ref mask, 1, "Y", ref from.y, ref to.y);
			DrawFloatChannel(ref mask, 2, "Z", ref from.z, ref to.z);
		}


		public static void DrawVector4Channels(ref int mask, ref Vector4 from, ref Vector4 to)
		{
			DrawFloatChannel(ref mask, 0, "X", ref from.x, ref to.x);
			DrawFloatChannel(ref mask, 1, "Y", ref from.y, ref to.y);
			DrawFloatChannel(ref mask, 2, "Z", ref from.z, ref to.z);
			DrawFloatChannel(ref mask, 3, "W", ref from.w, ref to.w);
		}
	}
}
