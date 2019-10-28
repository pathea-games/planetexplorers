#define DefineVoxelEditor
#if DefineVoxelEditor
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VoxelEditor))]
public class VoxelEditorUI : Editor {
	bool bFoldoutTreePrototypeList;
	bool bFoldoutTreePrototypeBendfactor;
	bool bFoleoutTreePrototypeSizes;
	bool bFoldoutTreePrototypeNames;
	Texture2D[] brushIcons;
	
	static Vector2[] normspecPowers;
	bool typeNormSpecFoldout = false;
	static Texture2D normspecTexture;	
	
	int indent = 15;
	#region DrawArray serials
	public static void DrawArray(ref GameObject[] array, ref bool expand, string title)
	{
		expand = EditorGUILayout.Foldout(expand, title);
		if(expand)
		{
			int oldLen = array.Length;
			int newLen = EditorGUILayout.IntField("   Size", oldLen);
			if(newLen != oldLen)
			{
				System.Array.Resize(ref array, newLen);
				if(oldLen > 0)
				{
					for(int i = oldLen; i < newLen; i++)	array[i] = array[oldLen-1];
				}
			}
			
			for(int x = 0; x < array.Length; x++)
				array[x] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("   Element "+x), array[x], typeof(GameObject), true);
		}	
	}
	public static void DrawArray(ref float[] array, ref bool expand, string title)
	{
		expand = EditorGUILayout.Foldout(expand, title);
		if(expand)
		{
			int oldLen = array.Length;
			int newLen = EditorGUILayout.IntField("   Size", oldLen);
			if(newLen != oldLen)
			{
				System.Array.Resize(ref array, newLen);
				if(oldLen > 0)
				{
					for(int i = oldLen; i < newLen; i++)	array[i] = array[oldLen-1];
				}
			}
			
			for(int x = 0; x < array.Length; x++)
				array[x] = EditorGUILayout.FloatField(new GUIContent("   Element "+x),array[x]);
		}	
	}
	public static void DrawArray(ref int[] array, ref bool expand, string title)
	{
		expand = EditorGUILayout.Foldout(expand, title);
		if(expand)
		{
			int oldLen = array.Length;
			int newLen = EditorGUILayout.IntField("   Size", oldLen);
			if(newLen != oldLen)
			{
				System.Array.Resize(ref array, newLen);
				if(oldLen > 0)
				{
					for(int i = oldLen; i < newLen; i++)	array[i] = array[oldLen-1];
				}
			}
			
			for(int x = 0; x < array.Length; x++)
				array[x] = EditorGUILayout.IntField(new GUIContent("   Element "+x),array[x]);	
		}	
	}
	public static void DrawArray(ref string[] array, ref bool expand, string title)
	{
		expand = EditorGUILayout.Foldout(expand, title);
		if(expand)
		{
			int oldLen = array.Length;
			int newLen = EditorGUILayout.IntField("   Size", oldLen);
			if(newLen != oldLen)
			{
				System.Array.Resize(ref array, newLen);
				if(oldLen > 0)
				{
					for(int i = oldLen; i < newLen; i++)	array[i] = array[oldLen-1];
				}
			}
			
			for(int x = 0; x < array.Length; x++)
				array[x] = EditorGUILayout.TextField(new GUIContent("   Element "+x),array[x]);	
		}	
	}
	#endregion
	public void DrawBrushGui(VoxelEditor ve)
	{
		int i = 0; 
		int count = 0;
		GUILayout.BeginHorizontal();
			count = (int)VoxelEditor.EBrushType.eBrushMax;
			GUIContent[] brushToolbarContents = new GUIContent[count];
			for(i = 0; i < count; i++){
				brushToolbarContents[i] = new GUIContent(((VoxelEditor.EBrushType)i).ToString(), brushIcons[i]);
			}
			ve.m_eBuildBrush = (VoxelEditor.EBrushType)GUILayout.Toolbar((int)ve.m_eBuildBrush, brushToolbarContents, GUILayout.Width(120*count), GUILayout.Height(32));
			ve.m_drawGizmo = GUILayout.Toggle(ve.m_drawGizmo, new GUIContent("Draw\nBrush"));
		GUILayout.EndHorizontal();
		if(ve.m_eBuildBrush == VoxelEditor.EBrushType.eTerStyleBrush && ve.terBrushTextures.Count > 0)
		{
			GUILayout.BeginHorizontal();
				GUIContent[] terToolbarContents = new GUIContent[ve.terBrushTextures.Count];
				for(i = 0; i < ve.terBrushTextures.Count; i++)
				{
					terToolbarContents[i] = new GUIContent(ve.terBrushTextures[i]);
				}
				ve.terBrushType = GUILayout.Toolbar(ve.terBrushType, terToolbarContents, GUILayout.Width(64*ve.terBrushTextures.Count), GUILayout.Height(64));
				ve.terBrushProjector.material.SetTexture("_ShadowTex", ve.terBrushTextures[ve.terBrushType]);
				GUILayout.Label("Note: Painting voxel materials automatically.");
				
			GUILayout.EndHorizontal();
			brushIcons[2] = ve.terBrushTextures[ve.terBrushType];
		}
	}
	public void EditModeGui(VoxelEditor ve)
	{
		int i = 0;
		int count = 0;
		GUILayout.BeginHorizontal();
			count = 3;
			GUIContent[] modeToolbarContents = new GUIContent[count];
			for(i = 0; i < count; i++){
				modeToolbarContents[i] = new GUIContent(((VoxelEditor.EEditMode)i).ToString());
			}
			ve.m_eEditMode = (VoxelEditor.EEditMode)GUILayout.Toolbar((int)ve.m_eEditMode, modeToolbarContents, GUILayout.Width(128*count), GUILayout.Height(24));
		GUILayout.EndHorizontal();
	}
	public void EditVoxelGui(VoxelEditor ve)
	{
		int i = 0; 
		int count = 0;
		GUILayout.BeginHorizontal();
			count = 5;
			GUIContent[] buildTypeToolbarContents = new GUIContent[count];
			for(i = 0; i < count; i++){
				buildTypeToolbarContents[i] = new GUIContent(((VoxelEditor.EBuildType)i).ToString());
			}
			ve.m_eBuildType = (VoxelEditor.EBuildType)GUILayout.Toolbar((int)ve.m_eBuildType, buildTypeToolbarContents, GUILayout.Width(77*count), GUILayout.Height(24));
		GUILayout.EndHorizontal();
		
		ve.m_alterRadius = EditorGUILayout.Slider("Alter Radius", ve.m_alterRadius,1,128, GUILayout.Width(384));
		GUI.enabled = ve.m_eBuildType!=VoxelEditor.EBuildType.eBuildVoxel || !ve.m_bPaintVoxelTypeMode;
		if(ve.m_eBuildType == VoxelEditor.EBuildType.eFlatten){
			ve.m_alterDstH = EditorGUILayout.Slider("Alter DstHeight", ve.m_alterDstH,1,999, GUILayout.Width(384));
		}else{
			ve.m_alterPower = EditorGUILayout.Slider("Alter Power", ve.m_alterPower,0.5f,2.0f, GUILayout.Width(384));
		}
		GUI.enabled = ve.m_eBuildBrush != VoxelEditor.EBrushType.eTerStyleBrush;
		ve.m_eVoxelTerrainSet = (VoxelEditor.EVoxelTerrainSet)EditorGUILayout.EnumPopup("VoxelType Set", ve.m_eVoxelTerrainSet, GUILayout.Width(384));
		ve.m_eVoxelTerrainSubSet = (VoxelEditor.EVoxelTerrainSubSet)EditorGUILayout.EnumPopup("VoxelType Subset", ve.m_eVoxelTerrainSubSet, GUILayout.Width(384));
		GUI.enabled = true;
		
		switch(ve.m_eBuildType){
		case VoxelEditor.EBuildType.eBuildVoxel:	
			ve.m_bPaintVoxelTypeMode = EditorGUILayout.Toggle("Paint Material Mode",ve.m_bPaintVoxelTypeMode, GUILayout.Width(384));
			DrawBrushGui(ve);
			break;
		case VoxelEditor.EBuildType.eFlatten:
			break;
		case VoxelEditor.EBuildType.eSmoothVoxel:
			ve.m_voxelFilterSize = EditorGUILayout.IntSlider("Voxel Filter Size", ve.m_voxelFilterSize,1,16, GUILayout.Width(384));
			break;
		case VoxelEditor.EBuildType.eGrowNoise:
			ve.m_NoiseStrength = EditorGUILayout.Slider("Noise Strength", ve.m_NoiseStrength,0,10, GUILayout.Width(384));
			ve.m_GrowNoiseCooldown = EditorGUILayout.Slider("Grow Noise Cooldown", ve.m_GrowNoiseCooldown,0,10, GUILayout.Width(384));
			break;
		case VoxelEditor.EBuildType.ePlantFilter:
			ve.FilterMap = (Texture2D)EditorGUILayout.ObjectField("Filter Map", ve.FilterMap, typeof(Texture2D), true, GUILayout.Width(384));
			ve.shootingOffset = EditorGUILayout.Slider("Shooting Offset", ve.shootingOffset,0,128, GUILayout.Width(384));
			ve.PlantFilterWeightDivisor = EditorGUILayout.IntSlider("Plant Filter Weight Divisor", ve.PlantFilterWeightDivisor,0,500, GUILayout.Width(384));
			ve.EdgeWeight = EditorGUILayout.IntSlider("Edge Weight", ve.EdgeWeight,0,128, GUILayout.Width(384));
			break;
		}
	}
	public void EditPlantGui(VoxelEditor ve)
	{
		GUILayout.BeginHorizontal();
			ve.m_buildStartIdx = EditorGUILayout.IntSlider("StartIdx", ve.m_buildStartIdx,0,ve.m_treePrototypeList.Length, GUILayout.Width(192));
			ve.m_buildRange = EditorGUILayout.IntSlider("Range", ve.m_buildRange,0,ve.m_treePrototypeList.Length, GUILayout.Width(192));
		GUILayout.EndHorizontal();
		ve.m_buildDensity = EditorGUILayout.IntSlider("Density", ve.m_buildDensity,0,500, GUILayout.Width(192));
		GUILayout.BeginHorizontal();
			ve.m_baseWidthScale = EditorGUILayout.Slider("Base W Scale", ve.m_baseWidthScale,0,5, GUILayout.Width(192));
			ve.m_baseHeightScale = EditorGUILayout.Slider("Base H Scale", ve.m_baseHeightScale,0,5, GUILayout.Width(192));
		GUILayout.EndHorizontal();
		ve.m_baseColor = EditorGUILayout.ColorField("Base Color", ve.m_baseColor, GUILayout.Width(192));
		ve.m_maxBuildAngle = EditorGUILayout.Slider("Max Build Angle", ve.m_maxBuildAngle,0,90, GUILayout.Width(192));
		
		DrawArray(ref ve.m_treePrototypeList, ref bFoldoutTreePrototypeList, "TreePrototype List");
		DrawArray(ref ve.m_treePrototypeBendfactor, ref bFoldoutTreePrototypeBendfactor, "TreePrototype Bend Factor");
		DrawArray(ref ve.m_treePrototypeSizes, ref bFoleoutTreePrototypeSizes, "TreePrototype Size");
		string[] treePrototypeName = ve.m_treePrototypeName.ToArray();
		DrawArray(ref treePrototypeName, ref bFoldoutTreePrototypeNames, "TreePrototype Name");
		ve.m_treePrototypeName = treePrototypeName.ToList();
	}
	public void NotInEditGui(VoxelEditor ve)
	{	
		GUILayout.BeginHorizontal();
			GUILayout.Space(indent);
		GUILayout.EndHorizontal();
	}

	#region EditNormSpec
	void EditNormSpecAwake()
	{
		normspecPowers = new Vector2[256];
		normspecTexture = Resources.Load("terrainNormSpec") as Texture2D;
		if(normspecTexture == null)
		{
			normspecTexture = new Texture2D(256,1);
			normspecTexture.filterMode = FilterMode.Point;
		}
		else
		{
			normspecTexture.filterMode = FilterMode.Point;
			for(int i = 0; i < 256; i++)
			{
				Color col = normspecTexture.GetPixel(i,0);
				normspecPowers[i].x = (col.r-0.5f)*16;	// [-8,+8]
				normspecPowers[i].y = (col.g)*4;	// [-0,+4]
			}
		}
	}
	public void EditNormSpecOnEnable (GameObject thisGo) {
		if(normspecTexture == null)
		{
			EditNormSpecAwake();
		}
	}
	public void EditNormSpecOnDisable()
	{
		/*
		if(normspecTexture != null)
		{
			byte[] pixles = normspecTexture.EncodeToPNG();
			System.IO.File.WriteAllBytes("Assets/Resources/terrainNormSpec.png", pixles);
		}
		*/
	}
	public void EditNormSpec (VoxelEditor ve) {
		typeNormSpecFoldout = EditorGUILayout.Foldout(typeNormSpecFoldout, "Voxel Type   /   Normal Power[-8,+8]   /   Spec Power[-8,+8]");
		if(typeNormSpecFoldout)
		{
			for(int i = 0; i < 256; i++)
			{
				EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(""+i+":", GUILayout.Width(32));
					normspecPowers[i].x = EditorGUILayout.Slider(normspecPowers[i].x,-8f,+8f, GUILayout.Width(180));
					normspecPowers[i].y = EditorGUILayout.Slider(normspecPowers[i].y, 0f,+4f, GUILayout.Width(160));
				EditorGUILayout.EndHorizontal();
			}
		}
		
		if (GUILayout.Button("Apply")) {
			for(int i = 0; i < 256; i++)
			{
				normspecTexture.SetPixel(i,0, new Color(0.5f+normspecPowers[i].x/16,normspecPowers[i].y/4,0.5f,1f));
			}
			if(ve.m_voxelTerrain != null && ve.m_voxelTerrain._defMat != null)
			{
				ve.m_voxelTerrain._defMat.SetTexture("_NormSpecPowerTex", normspecTexture);
				normspecTexture.Apply();
			}

			byte[] pixles = normspecTexture.EncodeToPNG();
			System.IO.File.WriteAllBytes("Assets/Resources/terrainNormSpec.png", pixles);
		}		
	}
	#endregion

	public void OnEnable () {
		VoxelEditor ve = (VoxelEditor) target as VoxelEditor;
		EditNormSpecOnEnable(ve.gameObject);
		
		if(ve.m_boxBrushPrefab == null)	ve.m_boxBrushPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/GizmoCube.prefab", typeof(GameObject));
		if(ve.m_sphereBrushPrefab == null)	ve.m_sphereBrushPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/GizmoSphere.prefab", typeof(GameObject));
		if(brushIcons == null || brushIcons.Length != (int)VoxelEditor.EBrushType.eBrushMax)	brushIcons = new Texture2D[(int)VoxelEditor.EBrushType.eBrushMax];
		if(brushIcons[0] == null)	brushIcons[0] = AssetPreview.GetAssetPreview(ve.m_boxBrushPrefab);
		if(brushIcons[1] == null)	brushIcons[1] = AssetPreview.GetAssetPreview(ve.m_sphereBrushPrefab);
		//if(ve.terBrushTextures.Count == 0)
		{
			ve.terBrushTextures.Clear();
			string[] strBrushTextures = System.IO.Directory.GetFiles("Assets/Resources/WaterTile","brush*.png");
			for(int i = 0; i < strBrushTextures.Length; i++)
			{	
				string strBrushTex = Path.GetFileNameWithoutExtension(strBrushTextures[i]);
				ve.terBrushTextures.Add((Texture2D)Resources.Load("WaterTile/"+strBrushTex));
			}
		}
		if(ve.terBrushProjector == null)
		{
			Transform prjTransform = ve.gameObject.transform.parent.FindChild("TmpPrj");
			if(prjTransform == null)
			{
				GameObject go = new GameObject("TmpPrj");
				prjTransform = go.transform;
				prjTransform.parent = ve.gameObject.transform.parent;
			}
			GameObject projectorGo = prjTransform.gameObject;
			if((ve.terBrushProjector=projectorGo.GetComponent<Projector>())==null)
			{
				ve.terBrushProjector = projectorGo.AddComponent<Projector>();
				ve.terBrushProjector.transform.localEulerAngles = new Vector3(90,0,0);
				ve.terBrushProjector.orthographic = true;
				ve.terBrushProjector.farClipPlane = 1500;
				ve.terBrushProjector.ignoreLayers = ~(1<<Pathea.Layer.VFVoxelTerrain);
				if(ve.terBrushProjector.material == null)
				{
					ve.terBrushProjector.material = (Material)Instantiate(Resources.Load("WaterTile/brushMat"));
				}
				//ve.terBrushProjector.material.SetTexture("_ShadowTex", waterPlane.brushTextures[waterPlane.brushType]);
			}
			ve.terBrushProjector.enabled = false;
		}
		if(brushIcons[2] == null)	brushIcons[2] = ve.terBrushTextures[ve.terBrushType];
	}
	public void OnDisable()
	{
		EditNormSpecOnDisable();
		
		VoxelEditor ve = (VoxelEditor) target as VoxelEditor;
		if(ve.terBrushProjector != null)
		{
			DestroyImmediate(ve.terBrushProjector.gameObject);
		}
	}	
	public override void OnInspectorGUI () {
		VoxelEditor ve = (VoxelEditor) target as VoxelEditor;
		GUILayout.BeginHorizontal();
			GUILayout.Space(indent);
			GUILayout.BeginVertical();
				EditModeGui(ve);
				switch(ve.m_eEditMode)
				{
				case VoxelEditor.EEditMode.eEditNormSpec:
					EditNormSpec(ve);
					break;
				case VoxelEditor.EEditMode.eEditVoxel:
					EditVoxelGui(ve);
					break;
				case VoxelEditor.EEditMode.eEditVegetables:
					EditPlantGui(ve);
					break;
				}
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
	void OnSceneGUI() {
	}
}

#endif
