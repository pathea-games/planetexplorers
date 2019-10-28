using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenLightIntensity))]
	class TweenLightIntensityEditor : TweenEditor<TweenLightIntensity>
	{
		SerializedProperty from, to;


		protected override void OnEnable()
		{
			base.OnEnable();
			from = serializedObject.FindProperty("from");
			to = serializedObject.FindProperty("to");
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
