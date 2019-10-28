#if UNITY_EDITOR

using UnityEngine;
using WhiteCat;
using UnityEditor;

namespace WhiteCatEditor
{
	/// <summary>
	/// RigidbodyEditor
	/// </summary>
	//[CustomEditor(typeof(Rigidbody), false)]
	public class RigidbodyEditor : Editor
	{
		static Color bigDiscColor = new Color(1f, 0f, 0f, 0.1f);
		static Color smallDiscColor = new Color(1f, 0f, 0.5f, 1f);
		static float bigDiscSize = 0.24f;
		static float smallDiscSize = 0.04f;


		public override void OnInspectorGUI()
		{
			var rigidbody = target as Rigidbody;
			DrawDefaultInspector();

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);

			EditorGUI.BeginChangeCheck();
			var value = EditorGUILayout.Vector3Field("Center Of Mass", rigidbody.centerOfMass);
			if (EditorGUI.EndChangeCheck())
			{
				rigidbody.centerOfMass = value;
			}

			EditorGUI.BeginChangeCheck();
			value = EditorGUILayout.Vector3Field("Inertia Tensor", rigidbody.inertiaTensor);
			if (EditorGUI.EndChangeCheck())
			{
				value.x = Mathf.Clamp(value.x, Kit.OneMillionth, Kit.Million);
				value.y = Mathf.Clamp(value.y, Kit.OneMillionth, Kit.Million);
				value.z = Mathf.Clamp(value.z, Kit.OneMillionth, Kit.Million);
				rigidbody.inertiaTensor = value;
			}

			EditorGUI.EndDisabledGroup();
		}


		void OnSceneGUI()
		{
			var rigidbody = target as Rigidbody;

			Vector3 center = rigidbody.worldCenterOfMass;
			Vector3 normal = SceneView.currentDrawingSceneView.rotation * Vector3.back;
			float handleSize = HandleUtility.GetHandleSize(center);

			GUIKit.RecordAndSetHandlesColor(bigDiscColor);
			Handles.DrawSolidDisc(center, normal, bigDiscSize * handleSize);
			GUIKit.RestoreHandlesColor();

			GUIKit.RecordAndSetHandlesColor(smallDiscColor);
			Handles.DrawSolidDisc(center, normal, smallDiscSize * handleSize);
			GUIKit.RestoreHandlesColor();
		}

	} // class RigidbodyEditor

} // namespace WhiteCatEditor

#endif // UNITY_EDITOR