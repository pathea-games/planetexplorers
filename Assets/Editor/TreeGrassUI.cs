//using UnityEngine;
//using UnityEditor;
//using System;
//using System.Linq;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//
//[CustomEditor(typeof(TreeGrass))]
//public class TreeGrassUI : Editor
//{
//    bool bFoldoutTreePrototypeList;
//    bool bFoldoutTreePrototypeBendfactor;
//    bool bFoleoutTreePrototypeSizes;
//    bool bFoldoutTreePrototypeNames;
//    Texture2D[] brushIcons;
//
//    int indent = 15;
//    #region DrawArray serials
//    public static void DrawArray(ref GameObject[] array, ref bool expand, string title)
//    {
//        //如果是植被列表，加载一个植被名字列表
//        if ("TreePrototype List" == title)
//        {
//            //UnityEngine.Object[] model = Resources.LoadAll("Model/scene/Grass-Ambient-Occlusion");
//
//            if (File.Exists(Application.dataPath + "/TreePrototypeList.txt"))
//            {
//                FileStream fs = new FileStream(Application.dataPath + "/TreePrototypeList.txt", FileMode.Open, FileAccess.Read);
//                StreamReader sr = new StreamReader(fs);
//
//                int length = int.Parse(sr.ReadLine());
//
//                if (length > 0)
//                {
//                    System.Array.Resize(ref array, length);
//                    for (int i = 0; i < length; i++)
//                    {
//                        bool isFind = false;
//                        string name = sr.ReadLine();
//                        string path = GameConfig.TreeColliderPrefabsPath + name + ".prefab";
//                        //先查找有碰撞
//                        if (File.Exists(path))
//                        {
//                            isFind = true;
//                            array[i] = (GameObject)Resources.LoadAssetAtPath(@"Assets/Prefabs/tree-Ambient-Occlusion/" + name + ".prefab",typeof(GameObject));
//                        }
//
//                        if (false == isFind && File.Exists(GameConfig.TreeFBXPath + name + ".FBX"))
//                        {
//                            isFind = true;
//                            array[i] = (GameObject)Resources.Load(GameConfig.TreeFBXResourcePath + name);
//                        }
//
//                        if (false == isFind && File.Exists(GameConfig.GrassFBXPath + name + ".FBX"))
//                        {
//                            isFind = true;
//                            array[i] = (GameObject)Resources.Load(GameConfig.GrassFBXResourcePath + name);
//                        }
//                    }
//                }
//
//                sr.Close();
//                fs.Close();
//            }
//        }
//
//        expand = EditorGUILayout.Foldout(expand, title);
//        if (expand)
//        {
//            int oldLen = array.Length;
//            int newLen = EditorGUILayout.IntField("   Size", oldLen);
//            if (newLen != oldLen)
//            {
//                System.Array.Resize(ref array, newLen);
//                if (oldLen > 0)
//                {
//                    for (int i = oldLen; i < newLen; i++) array[i] = array[oldLen - 1];
//                }
//            }
//
//            for (int x = 0; x < array.Length; x++)
//                array[x] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("   Element " + x), array[x], typeof(GameObject));
//        }
//    }
//    public static void DrawArray(ref float[] array, ref bool expand, string title)
//    {
//        //加载一个blent factor列表文件
//        if ("TreePrototype Bend Factor" == title)
//        {
//            if (File.Exists(Application.dataPath + "/TreePrototypeBendFactorList.txt"))
//            {
//                FileStream fs = new FileStream(Application.dataPath + "/TreePrototypeBendFactorList.txt", FileMode.Open, FileAccess.Read);
//                StreamReader sr = new StreamReader(fs);
//
//                int length = int.Parse(sr.ReadLine());
//
//                if (length > 0)
//                {
//                    System.Array.Resize(ref array, length);
//                    for (int i = 0; i < length; i++)
//                    {
//                        array[i] = float.Parse(sr.ReadLine());
//                    }
//                }
//
//                sr.Close();
//                fs.Close();
//            }
//        }
//
//        expand = EditorGUILayout.Foldout(expand, title);
//        if (expand)
//        {
//            int oldLen = array.Length;
//            int newLen = EditorGUILayout.IntField("   Size", oldLen);
//            if (newLen != oldLen)
//            {
//                System.Array.Resize(ref array, newLen);
//                if (oldLen > 0)
//                {
//                    for (int i = oldLen; i < newLen; i++) array[i] = array[oldLen - 1];
//                }
//            }
//
//            for (int x = 0; x < array.Length; x++)
//                array[x] = EditorGUILayout.FloatField(new GUIContent("   Element " + x), array[x]);
//        }
//    }
//    public static void DrawArray(ref int[] array, ref bool expand, string title)
//    {
//        expand = EditorGUILayout.Foldout(expand, title);
//        if (expand)
//        {
//            int oldLen = array.Length;
//            int newLen = EditorGUILayout.IntField("   Size", oldLen);
//            if (newLen != oldLen)
//            {
//                System.Array.Resize(ref array, newLen);
//                if (oldLen > 0)
//                {
//                    for (int i = oldLen; i < newLen; i++) array[i] = array[oldLen - 1];
//                }
//            }
//
//            for (int x = 0; x < array.Length; x++)
//                array[x] = EditorGUILayout.IntField(new GUIContent("   Element " + x), array[x]);
//        }
//    }
//    public static void DrawArray(ref string[] array, ref bool expand, string title)
//    {
//        expand = EditorGUILayout.Foldout(expand, title);
//        if (expand)
//        {
//            int oldLen = array.Length;
//            int newLen = EditorGUILayout.IntField("   Size", oldLen);
//            if (newLen != oldLen)
//            {
//                System.Array.Resize(ref array, newLen);
//                if (oldLen > 0)
//                {
//                    for (int i = oldLen; i < newLen; i++) array[i] = array[oldLen - 1];
//                }
//            }
//
//            for (int x = 0; x < array.Length; x++)
//                array[x] = EditorGUILayout.TextField(new GUIContent("   Element " + x), array[x]);
//        }
//    }
//    #endregion
//    public void DrawBrushGui(TreeGrass ve)
//    {
////         int i = 0;
////         int count = 0;
////         GUILayout.BeginHorizontal();
////         count = (int)TreeGrass.EBrushType.eBrushMax;
////         GUIContent[] brushToolbarContents = new GUIContent[count];
////         for (i = 0; i < count; i++)
////         {
////             brushToolbarContents[i] = new GUIContent(((TreeGrass.EBrushType)i).ToString(), brushIcons[i]);
////         }
////         ve.m_eBuildBrush = (TreeGrass.EBrushType)GUILayout.Toolbar((int)ve.m_eBuildBrush, brushToolbarContents, GUILayout.Width(120 * count), GUILayout.Height(32));
////         ve.m_drawGizmo = GUILayout.Toggle(ve.m_drawGizmo, new GUIContent("Draw\nBrush"));
////         GUILayout.EndHorizontal();
////         if (ve.m_eBuildBrush == TreeGrass.EBrushType.eTerStyleBrush && ve.terBrushTextures.Count > 0)
////         {
////             GUILayout.BeginHorizontal();
////             GUIContent[] terToolbarContents = new GUIContent[ve.terBrushTextures.Count];
////             for (i = 0; i < ve.terBrushTextures.Count; i++)
////             {
////                 terToolbarContents[i] = new GUIContent(ve.terBrushTextures[i]);
////             }
////             ve.terBrushType = GUILayout.Toolbar(ve.terBrushType, terToolbarContents, GUILayout.Width(64 * ve.terBrushTextures.Count), GUILayout.Height(64));
////             ve.terBrushProjector.material.SetTexture("_ShadowTex", ve.terBrushTextures[ve.terBrushType]);
////             GUILayout.EndHorizontal();
////             brushIcons[2] = ve.terBrushTextures[ve.terBrushType];
////         }
//    }
//    public void EditModeGui(TreeGrass ve)
//    {
////         int i = 0;
////         int count = 0;
//        GUILayout.BeginHorizontal();
////         count = 3;
////         GUIContent[] modeToolbarContents = new GUIContent[count];
////         for (i = 0; i < count; i++)
////         {
////             modeToolbarContents[i] = new GUIContent(((TreeGrass.EEditMode)i).ToString());
////         }
////         ve.m_eEditMode = (TreeGrass.EEditMode)GUILayout.Toolbar((int)ve.m_eEditMode, modeToolbarContents, GUILayout.Width(128 * count), GUILayout.Height(24));
//        GUILayout.EndHorizontal();
//    }
//    public void EditVoxelGui(TreeGrass ve)
//    {
////         int i = 0;
////         int count = 0;
////         GUILayout.BeginHorizontal();
////         count = 5;
////         GUIContent[] buildTypeToolbarContents = new GUIContent[count];
////         for (i = 0; i < count; i++)
////         {
////             buildTypeToolbarContents[i] = new GUIContent(((TreeGrass.EBuildType)i).ToString());
////         }
////         ve.m_eBuildType = (TreeGrass.EBuildType)GUILayout.Toolbar((int)ve.m_eBuildType, buildTypeToolbarContents, GUILayout.Width(77 * count), GUILayout.Height(24));
////         GUILayout.EndHorizontal();
//// 
////         if (ve.m_eBuildType == TreeGrass.EBuildType.eAutoPaintMat)
////         {
////             return;
////         }
//// 
////         ve.m_alterRadius = EditorGUILayout.Slider("Alter Radius", ve.m_alterRadius, 1, 500, GUILayout.Width(384));
////         ve.m_eVoxelTerrainSet = (TreeGrass.EVoxelTerrainSet)EditorGUILayout.EnumPopup("VoxelType Set", ve.m_eVoxelTerrainSet, GUILayout.Width(384));
////         ve.m_eVoxelTerrainSubSet = (TreeGrass.EVoxelTerrainSubSet)EditorGUILayout.EnumPopup("VoxelType Subset", ve.m_eVoxelTerrainSubSet, GUILayout.Width(384));
//// 
////         switch (ve.m_eBuildType)
////         {
////             case TreeGrass.EBuildType.eBuildVoxel: DrawBrushGui(ve); break;
////             case TreeGrass.EBuildType.ePaintMaterial: DrawBrushGui(ve); break;
////             case TreeGrass.EBuildType.eSmoothVoxel:
////                 ve.m_voxelFilterSize = EditorGUILayout.IntSlider("Voxel Filter Size", ve.m_voxelFilterSize, 1, 500, GUILayout.Width(384));
////                 DrawBrushGui(ve);
////                 break;
////             case TreeGrass.EBuildType.eGrowNoise:
////                 ve.m_NoiseStrength = EditorGUILayout.Slider("Noise Strength", ve.m_NoiseStrength, 0, 10, GUILayout.Width(384));
////                 ve.m_GrowNoiseCooldown = EditorGUILayout.Slider("Grow Noise Cooldown", ve.m_GrowNoiseCooldown, 0, 10, GUILayout.Width(384));
////                 DrawBrushGui(ve);
////                 break;
////             case TreeGrass.EBuildType.ePlantFilter:
////                 ve.FilterMap = (Texture2D)EditorGUILayout.ObjectField("Filter Map", ve.FilterMap, typeof(Texture2D), GUILayout.Width(384));
////                 ve.shootingOffset = EditorGUILayout.Slider("Shooting Offset", ve.shootingOffset, 0, 500, GUILayout.Width(384));
////                 ve.PlantFilterWeightDivisor = EditorGUILayout.IntSlider("Plant Filter Weight Divisor", ve.PlantFilterWeightDivisor, 0, 500, GUILayout.Width(384));
////                 ve.EdgeWeight = EditorGUILayout.IntSlider("Edgr Weight", ve.EdgeWeight, 0, 500, GUILayout.Width(384));
////                 break;
////         }
//    }
//    public void EditPlantGui(TreeGrass ve)
//    {
//        GUILayout.BeginHorizontal();
//        ve.m_buildStartIdx = EditorGUILayout.IntSlider("StartIdx", ve.m_buildStartIdx, 0, ve.m_treePrototypeList.Length, GUILayout.Width(192));
//        ve.m_buildRange = EditorGUILayout.IntSlider("Range", ve.m_buildRange, 0, ve.m_treePrototypeList.Length, GUILayout.Width(192));
//        GUILayout.EndHorizontal();
//        ve.m_buildDensity = EditorGUILayout.IntSlider("Density", ve.m_buildDensity, 0, 500, GUILayout.Width(192));
//        GUILayout.BeginHorizontal();
//        ve.m_baseWidthScale = EditorGUILayout.Slider("Base W Scale", ve.m_baseWidthScale, 0, 5, GUILayout.Width(192));
//        ve.m_baseHeightScale = EditorGUILayout.Slider("Base H Scale", ve.m_baseHeightScale, 0, 5, GUILayout.Width(192));
//        GUILayout.EndHorizontal();
//        ve.m_baseColor = EditorGUILayout.ColorField("Base Color", ve.m_baseColor, GUILayout.Width(192));
//        ve.m_maxBuildAngle = EditorGUILayout.Slider("Max Build Angle", ve.m_maxBuildAngle, 0, 90, GUILayout.Width(192));
//
//        DrawArray(ref ve.m_treePrototypeList, ref bFoldoutTreePrototypeList, "TreePrototype List");
//        DrawArray(ref ve.m_treePrototypeBendfactor, ref bFoldoutTreePrototypeBendfactor, "TreePrototype Bend Factor");
//        DrawArray(ref ve.m_treePrototypeSizes, ref bFoleoutTreePrototypeSizes, "TreePrototype Size");
//        string[] treePrototypeName = ve.m_treePrototypeName.ToArray();
//        DrawArray(ref treePrototypeName, ref bFoldoutTreePrototypeNames, "TreePrototype Name");
//        ve.m_treePrototypeName = treePrototypeName.ToList();
//    }
//
//    public void NotInEditGui(TreeGrass ve)
//    {
//        GUILayout.BeginHorizontal();
//        GUILayout.Space(indent);
//        GUILayout.EndHorizontal();
//    }
//
////     public void OnEnable()
////     {
////         TreeGrass ve = (TreeGrass)target as TreeGrass;
//// 
////         if (ve.m_boxBrushPrefab == null) ve.m_boxBrushPrefab = (GameObject)Resources.LoadAssetAtPath("Assets/Prefabs/GizmoCube.prefab", typeof(GameObject));
////         if (ve.m_sphereBrushPrefab == null) ve.m_sphereBrushPrefab = (GameObject)Resources.LoadAssetAtPath("Assets/Prefabs/GizmoSphere.prefab", typeof(GameObject));
////         if (brushIcons == null || brushIcons.Length != (int)TreeGrass.EBrushType.eBrushMax) brushIcons = new Texture2D[(int)TreeGrass.EBrushType.eBrushMax];
////         if (brushIcons[0] == null) brushIcons[0] = AssetPreview.GetAssetPreview(ve.m_boxBrushPrefab);
////         if (brushIcons[1] == null) brushIcons[1] = AssetPreview.GetAssetPreview(ve.m_sphereBrushPrefab);
////         if (ve.terBrushTextures.Count == 0)
////         {
////				string[] strBrushTextures = System.IO.Directory.GetFiles("/Resources/WaterTile","brush*.png");
////				for(int i = 0; i < strBrushTextures.Length; i++)
////				{	
////					string strBrushTex = Path.GetFileNameWithoutExtension(strBrushTextures[i]);
////					ve.terBrushTextures.Add((Texture2D)Resources.Load("WaterTile/"+strBrushTex));
////				}	
////         }
////         if (ve.terBrushProjector == null)
////         {
////             Transform prjTransform = ve.gameObject.transform.parent.FindChild("TmpPrj");
////             if (prjTransform == null)
////             {
////                 GameObject go = new GameObject("TmpPrj");
////                 prjTransform = go.transform;
////                 prjTransform.parent = ve.gameObject.transform.parent;
////             }
////             GameObject projectorGo = prjTransform.gameObject;
////             if ((ve.terBrushProjector = projectorGo.GetComponent<Projector>()) == null)
////             {
////                 ve.terBrushProjector = projectorGo.AddComponent<Projector>();
////                 ve.terBrushProjector.transform.localEulerAngles = new Vector3(90, 0, 0);
////                 ve.terBrushProjector.orthographic = true;
////                 ve.terBrushProjector.farClipPlane = 1000;
////                 ve.terBrushProjector.ignoreLayers = ~(1 << Pathea.Layer.VFVoxelTerrain);
////                 if (ve.terBrushProjector.material == null)
////                 {
////                     ve.terBrushProjector.material = (Material)Instantiate(Resources.Load("WaterTile/brushMat"));
////                 }
////                 //ve.terBrushProjector.material.SetTexture("_ShadowTex", waterPlane.brushTextures[waterPlane.brushType]);
////             }
////             ve.terBrushProjector.enabled = false;
////         }
////         if (brushIcons[2] == null) brushIcons[2] = ve.terBrushTextures[ve.terBrushType];
////     }
////     public void OnDisable()
////     {
////         TreeGrass ve = (TreeGrass)target as TreeGrass;
////         if (ve.terBrushProjector != null)
////         {
////             DestroyImmediate(ve.terBrushProjector.gameObject);
////         }
////     }
//    public override void OnInspectorGUI()
//    {
//        TreeGrass ve = (TreeGrass)target as TreeGrass;
//
//        GUILayout.BeginHorizontal();
//        GUILayout.Space(indent);
//        GUILayout.BeginVertical();
//        EditModeGui(ve);
////         switch (ve.m_eEditMode)
////         {
////             case TreeGrass.EEditMode.eEditNormSpec:
////                 break;
////             case TreeGrass.EEditMode.eEditVoxel:
////                 EditVoxelGui(ve);
////                 break;
////             case TreeGrass.EEditMode.eEditVegetables:
//        EditPlantGui(ve);
////                 break;
////         }
//        GUILayout.EndVertical();
//        GUILayout.EndHorizontal();
//    }
//    void OnSceneGUI()
//    {
//    }
//}
