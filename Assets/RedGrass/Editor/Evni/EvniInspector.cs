using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RedGrass
{
	[CustomEditor(typeof(EvniAsset))]
	public class EvniInspector : Editor 
	{
		EvniAsset script  { get { return target as EvniAsset; }}

		SerializedProperty mChunkSizeProp = null;
		SerializedProperty mLodTypeProp = null;

		void OnEnable ()
		{
			mChunkSizeProp = serializedObject.FindProperty("ChunkSize");
			mLodTypeProp = serializedObject.FindProperty("LODType");
			script.CalcExtraTileVars();
			script.CalcExtraFileVars();
			script.CalcLODExtraVars();
			script.CalcExtraChunkSizeVars();
		}

		bool mDirty = false;

		public override void OnInspectorGUI ()
		{

			if (mDirty)
			{

				mDirty = false;
			}
//			 base.OnInspectorGUI();
//			return;
			#region FILE_SETTING
			EditorTools.Separator();
			EditorTools.Header("<< File Settings");
			script.Tile = EditorGUILayout.IntField("Tile", script.Tile);; 
			script.XStart = EditorGUILayout.IntField("X32Start", script.XStart);
			script.ZStart = EditorGUILayout.IntField("Y32Start", script.ZStart);

			script.XTileCount = EditorGUILayout.IntField("XTileCount", script.XTileCount);
			script.ZTileCount = EditorGUILayout.IntField("YTileCount", script.ZTileCount);

			// Extra Vars
//			if ( _tile !=  script.Tile || script.XStart != _xStart || script.ZStart != _zStart
//			    || script.XTileCount != _xTileCount || script.ZTileCount != _zTileCount)
//			{
//				script.CalcExtraTileVars();
//
//				_tile = script.Tile;
//				_xStart = script.XStart;
//				_zStart = script.ZStart;
//				_xTileCount = script.XTileCount;
//				_zTileCount = script.ZTileCount;
//			}

			string s = "";
			s += " XZTileCount  =  " + script.XZTileCount.ToString() + "\r\n";
			s += " XEnd  =  " + script.XEnd.ToString() + "     ";
			s += " ZEnd  =  " + script.ZEnd.ToString() + "     " ;
			EditorGUILayout.HelpBox(s, MessageType.Info);

			script.FileXCount = EditorGUILayout.IntField("FileXCount", script.FileXCount);
			script.FlieZCount = EditorGUILayout.IntField("Y32Start", script.FlieZCount);

			// Extra Vars
//			if (_fileXCount != script.FileXCount || _fileZCount != script.FlieZCount)
//			{
//				script.CalcExtraFileVars();
//
//				_fileXCount = script.FileXCount;
//				_fileZCount = script.FlieZCount;
//			}

			s = "FileXZcount  =  " + script.FileXZcount.ToString();
			EditorGUILayout.HelpBox(s, MessageType.Info);
			#endregion

			#region CHUNK_SETTING
			EditorTools.Separator();
			EditorTools.Header("<< Chunk Settings");

//			mChunkSizeProp.enumValueIndex = EditorGUILayout.Popup("Chunk Size", mChunkSizeProp.enumValueIndex, mChunkSizeProp.enumNames);
			script.ChunkSize = (EChunkSize)EditorGUILayout.Popup("Chunk Size", (int)script.ChunkSize, mChunkSizeProp.enumNames);

//			if (select_index != mChunkSizeProp.enumValueIndex )
//			{
//				script.CalcExtraChunkSizeVars();
//				
//			}
//			script.CalcExtraChunkSizeVars();

			s = "Chunk Size :" + script.CHUNKSIZE.ToString();
			EditorGUILayout.HelpBox(s, MessageType.Info);

			script.Density = EditorGUILayout.FloatField ("Main Density", script.Density);


//			mLodTypeProp.enumValueIndex = EditorGUILayout.Popup("LOD Type", mLodTypeProp.enumValueIndex, mLodTypeProp.enumNames);
			script.LODType = (ELodType)EditorGUILayout.Popup("LOD Type", (int)script.LODType, mLodTypeProp.enumNames);
//
//			if (select_index != mLodTypeProp.enumValueIndex)
//			{
//				script.CalcLODExtraVars();
//
//			}

			s = script.GetLodTypeDesc(script.LODType);
			EditorGUILayout.HelpBox(s, MessageType.Info);

			for (int i = 0; i <= script.MaxLOD; i++)
			{
				script.LODDensities[i] = EditorGUILayout.FloatField("LOD " + i.ToString() + " Density ", script.LODDensities[i] );
			}


			script.MeshQuadMaxCount = EditorGUILayout.IntField("MeshQuadMaxCount", script.MeshQuadMaxCount);

//			if (GUI)
//			EditorUtility.SetDirty(script);
//			AssetDatabase.SaveAssets();

			if (GUI.changed)
			{

				script.CalcExtraChunkSizeVars();
				script.CalcLODExtraVars();
				script.CalcExtraFileVars();
				script.CalcExtraTileVars();

				mDirty = true;

				AssetDatabase.SaveAssets(); 
				EditorUtility.SetDirty(script);
			}
			
			#endregion

		}
	}
}
