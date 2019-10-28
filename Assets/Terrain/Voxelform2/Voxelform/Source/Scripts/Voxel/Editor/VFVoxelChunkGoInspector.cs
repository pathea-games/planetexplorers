using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (VFVoxelChunkGo))]
public class VFVoxelChunkGoInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		VFVoxelChunkGo script = (VFVoxelChunkGo)target;
		bool bApplyRead = EditorGUILayout.Toggle ("ReadVoxel", script.bApplyRead);
		if (bApplyRead != script.bApplyRead) {
			script.bApplyRead = bApplyRead;
		}
		bool bApplyWrite = EditorGUILayout.Toggle ("WriteVoxel", script.bApplyWrite);
		if (bApplyWrite != script.bApplyWrite) {
			script.bApplyWrite = bApplyWrite;
		}
		bool bGenTrans = EditorGUILayout.Toggle ("GenTrans", script.bGenTrans);
		if (bGenTrans != script.bGenTrans) {
			script.bGenTrans = bGenTrans;
		}
	}
}
