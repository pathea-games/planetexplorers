using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenPosition))]
	class TweenPositionEditor : TweenEditor<TweenPosition>
	{
		SerializedProperty relativeTo;

		int mask;
		Vector3 from, to;


		protected override void OnEnable()
		{
			base.OnEnable();
			relativeTo = serializedObject.FindProperty("relativeTo");
		} 


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			EditorGUILayout.PropertyField(relativeTo);
			serializedObject.ApplyModifiedProperties();

			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawVector3Channels(ref mask, ref from, ref to);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, undoString);
				target.mask = mask;
				target.from = from;
				target.to = to;
				EditorUtility.SetDirty(target);
			}
		}
	}
}
