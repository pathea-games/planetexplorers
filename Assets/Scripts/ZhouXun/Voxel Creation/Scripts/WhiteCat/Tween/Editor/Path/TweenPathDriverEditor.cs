using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenPathDriver))]
	class TweenPathDriverEditor : TweenEditor<TweenPathDriver>
	{
		SerializedProperty from, to;


		protected override void OnEnable()
		{
			base.OnEnable();
			from = serializedObject.FindProperty("_from");
			to = serializedObject.FindProperty("_to");
		}


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.PropertyField(from);
			EditorGUILayout.PropertyField(to);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
