using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenMaterialFloat))]
	class TweenMaterialFloatEditor : TweenMaterialPropertyEditor<TweenMaterialFloat>
	{
		protected SerializedProperty from;
		protected SerializedProperty to;


		protected override void OnEnable()
		{
			base.OnEnable();

			from = serializedObject.FindProperty("from");
			to = serializedObject.FindProperty("to");
		}


		protected override void OnDrawSubclass()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(from);
			EditorGUILayout.PropertyField(to);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
