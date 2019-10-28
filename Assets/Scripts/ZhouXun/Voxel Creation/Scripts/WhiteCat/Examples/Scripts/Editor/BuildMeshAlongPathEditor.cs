using UnityEngine;
using UnityEditor;
using WhiteCat;

[CustomEditor(typeof(BuildMeshAlongPath))]
public class BuildMeshAlongPathEditor : Editor<BuildMeshAlongPath>
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUI.BeginDisabledGroup(!target.path);

		Rect rect = EditorGUILayout.GetControlRect(false, 18f);
		rect.xMin += EditorGUIUtility.labelWidth;

		if (GUI.Button(rect, "Build"))
		{
			target.Bulid();
			EditorKit.CreateAsset(target.GetComponent<MeshFilter>().sharedMesh, target.directory, target.fileName);
		}

		EditorGUI.EndDisabledGroup();
	}
}
