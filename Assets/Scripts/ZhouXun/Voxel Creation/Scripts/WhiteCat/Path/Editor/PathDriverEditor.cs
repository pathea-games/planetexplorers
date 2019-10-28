using UnityEditor;
using UnityEngine;

namespace WhiteCat
{
	[CustomEditor(typeof(PathDriver))]
	class PathDriverEditor : Editor<PathDriver>
	{
		SerializedProperty path;
		SerializedProperty location;
		SerializedProperty rotationMode;
		SerializedProperty reverseForward;
		SerializedProperty constantUp;


		void OnEnable()
		{
			path = serializedObject.FindProperty("_path");
			location = serializedObject.FindProperty("_location");
			rotationMode = serializedObject.FindProperty("rotationMode");
			reverseForward = serializedObject.FindProperty("reverseForward");
			constantUp = serializedObject.FindProperty("constantUp");
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(path);
			EditorGUILayout.PropertyField(location);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(rotationMode);
			if(target.rotationMode != RotationMode.Ignore)
			{
				EditorGUILayout.PropertyField(reverseForward);
				if(target.rotationMode == RotationMode.ConstantUp)
				{
					EditorGUILayout.PropertyField(constantUp);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
