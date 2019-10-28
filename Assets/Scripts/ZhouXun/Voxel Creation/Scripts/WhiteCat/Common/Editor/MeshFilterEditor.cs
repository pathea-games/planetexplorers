#if UNITY_EDITOR

using WhiteCat;
using UnityEngine;
using UnityEditor;

namespace WhiteCatEditor
{
	/// <summary>
	/// MeshFilterEditor
	/// </summary>
	//[CustomEditor(typeof(MeshFilter), false)]
	public class MeshFilterEditor : Editor
	{
		static Color normalsColor = new Color(0f, 1f, 1f, 0.8f);
		static Color verticesColor = new Color(1f, 0.5f, 0f);
		static Color boundsColor = new Color(1f, 1f, 1f);
		const float normalsSize = 0.5f;
		const float verticesScreenSize = 4f;
		static Vector2 verticesScreenOffset = new Vector2(-verticesScreenSize * 0.5f, -verticesScreenSize * 0.5f - 2f);

		bool showVertices = false;
		bool showNormals = false;
		bool showBounds = false;

		Transform transform;
		Mesh mesh;


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			var rect = EditorGUILayout.GetControlRect(true);

			rect.width /= 3f;
			if (showVertices != GUI.Toggle(rect, showVertices, "Vertices", EditorStyles.miniButtonLeft))
			{
				showVertices = !showVertices;
				SceneView.RepaintAll();
			}

			rect.x = rect.xMax;
			if (showNormals != GUI.Toggle(rect, showNormals, "Normals", EditorStyles.miniButtonMid))
			{
				showNormals = !showNormals;
				SceneView.RepaintAll();
			}

			rect.x = rect.xMax;
			if (showBounds != GUI.Toggle(rect, showBounds, "Bounds", EditorStyles.miniButtonRight))
			{
				showBounds = !showBounds;
				SceneView.RepaintAll();
			}
		}


		void OnSceneGUI()
		{
			mesh = (target as MeshFilter).sharedMesh;
			transform = (target as MeshFilter).transform;

			if (mesh)
			{
				if (showNormals) DrawNormals();
				if (showVertices) DrawVertices();
				if (showBounds) DrawBounds();
			}

			transform = null;
			mesh = null;
		}


		void DrawNormals()
		{
			GUIKit.RecordAndSetHandlesMatrix(transform.localToWorldMatrix);

			var normals = mesh.normals;
			if (!Kit.IsNullOrEmpty(normals))
			{
				int count = normals.Length;
				var vertices = mesh.vertices;

				GUIKit.RecordAndSetHandlesColor(normalsColor);
				for (int i = 0; i < count; i++)
				{
					GUIKit.HandlesDrawAALine(vertices[i], vertices[i] + normals[i] * normalsSize);
				}
				GUIKit.RestoreHandlesColor();
			}

			GUIKit.RestoreHandlesMatrix();
		}


		void DrawVertices()
		{
			var vertices = mesh.vertices;
			var colors = mesh.colors32;
			int count = vertices.Length;
			bool hasColor = (colors != null && colors.Length == count);
			
			Rect rect = new Rect(0, 0, verticesScreenSize, verticesScreenSize);

			Handles.BeginGUI();
			GUIKit.RecordAndSetColor(verticesColor);

			for (int i = 0; i < count; i++)
			{
				if (hasColor) GUI.color = colors[i];

				rect.position = verticesScreenOffset + 
					HandleUtility.WorldToGUIPoint(transform.TransformPoint(vertices[i]));

				GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
			}

			GUIKit.RestoreColor();
			Handles.EndGUI();
		}


		void DrawBounds()
		{
			GUIKit.RecordAndSetHandlesMatrix(transform.localToWorldMatrix);

			GUIKit.RecordAndSetHandlesColor(boundsColor);
			GUIKit.HandlesDrawWireLocalBounds(mesh.bounds);
			GUIKit.RestoreHandlesColor();

			GUIKit.RestoreHandlesMatrix();
		}

	} // class MeshFilterEditor

} // namespace WhiteCatEditor

#endif // UNITY_EDITOR